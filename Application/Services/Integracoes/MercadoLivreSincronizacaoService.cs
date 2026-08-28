using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Entities.Integracoes;
using CarStoreManager.Domain.Interfaces.Repositories.Integracoes;
using CarStoreManager.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace CarStoreManager.Application.Services.Integracoes;

/// <summary>
/// Processa a conexão OAuth e as notificações de venda do Mercado Livre.
/// ProcessarNotificacaoWebhookAsync/ProcessarItemVendidoAsync é o coração da
/// correção crítica desta feature: idempotência por (IdPedido, IdItem) —
/// entregas duplicadas de webhook (comportamento normal, não exceção, em
/// provedores de webhook reais) não podem decrementar estoque duas vezes.
/// </summary>
public class MercadoLivreSincronizacaoService : IMercadoLivreSincronizacaoService
{
    private readonly IAnuncioMercadoLivreRepository _anuncioRepo;
    private readonly IVendaMercadoLivreRepository _vendaRepo;
    private readonly IEstoqueRepository _estoqueRepo;
    private readonly IVeiculoVendaRepository _veiculoVendaRepo;
    private readonly IVeiculoConsignacaoRepository _veiculoConsignacaoRepo;
    private readonly IConfiguracaoMercadoLivreRepository _configRepo;
    private readonly IMercadoLivreApiClient _apiClient;
    private readonly MercadoLivreTokenHelper _tokenHelper;
    private readonly ITokenCriptografiaService _cripto;
    private readonly MercadoLivreConfig _config;

    public MercadoLivreSincronizacaoService(
        IAnuncioMercadoLivreRepository anuncioRepo,
        IVendaMercadoLivreRepository vendaRepo,
        IEstoqueRepository estoqueRepo,
        IVeiculoVendaRepository veiculoVendaRepo,
        IVeiculoConsignacaoRepository veiculoConsignacaoRepo,
        IConfiguracaoMercadoLivreRepository configRepo,
        IMercadoLivreApiClient apiClient,
        MercadoLivreTokenHelper tokenHelper,
        ITokenCriptografiaService cripto,
        IOptions<MercadoLivreConfig> config)
    {
        _anuncioRepo = anuncioRepo;
        _vendaRepo = vendaRepo;
        _estoqueRepo = estoqueRepo;
        _veiculoVendaRepo = veiculoVendaRepo;
        _veiculoConsignacaoRepo = veiculoConsignacaoRepo;
        _configRepo = configRepo;
        _apiClient = apiClient;
        _tokenHelper = tokenHelper;
        _cripto = cripto;
        _config = config.Value;
    }

    public Task<Result<string>> ObterUrlConexaoAsync(string state)
    {
        if (string.IsNullOrWhiteSpace(_config.ClientId) || string.IsNullOrWhiteSpace(_config.RedirectUri))
            return Task.FromResult(Result<string>.Fail(
                "Mercado Livre não configurado — preencha ClientId/RedirectUri em appsettings.json."));

        var url = "https://auth.mercadolivre.com.br/authorization" +
                  $"?response_type=code&client_id={Uri.EscapeDataString(_config.ClientId)}" +
                  $"&redirect_uri={Uri.EscapeDataString(_config.RedirectUri)}" +
                  $"&state={Uri.EscapeDataString(state)}";

        return Task.FromResult(Result<string>.Ok(url));
    }

    public async Task<Result> ProcessarCallbackOAuthAsync(string code)
    {
        try
        {
            var token = await _apiClient.TrocarCodigoPorTokenAsync(code);
            var usuario = await _apiClient.ObterUsuarioAsync(token.AccessToken);

            var cfg = await _configRepo.ObterAsync();
            cfg.RegistrarConexao(
                usuario.Id.ToString(),
                usuario.Email,
                _cripto.Proteger(token.AccessToken),
                _cripto.Proteger(token.RefreshToken),
                DateTime.UtcNow.AddSeconds(token.ExpiresIn));
            await _configRepo.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Erro ao conectar com o Mercado Livre: {ex.Message}");
        }
    }

    public async Task<Result> DesconectarAsync()
    {
        var cfg = await _configRepo.ObterAsync();
        cfg.Desconectar();
        await _configRepo.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result<ConfiguracaoMercadoLivreDTO>> ObterConfiguracaoAsync()
    {
        var cfg = await _configRepo.ObterAsync();
        return Result<ConfiguracaoMercadoLivreDTO>.Ok(new ConfiguracaoMercadoLivreDTO
        {
            Conectado = cfg.Conectado,
            EmailContaConectada = cfg.EmailContaConectada,
            DataConexao = cfg.DataConexao,
            DataExpiracaoToken = cfg.DataExpiracaoToken,
            Ambiente = _config.Ambiente
        });
    }

    public async Task<Result> ProcessarNotificacaoWebhookAsync(MercadoLivreWebhookNotificacaoDTO notificacao)
    {
        var tokenResult = await _tokenHelper.ObterAccessTokenValidoAsync();
        if (!tokenResult.IsSuccess)
            return Result.Fail(tokenResult.Error!); // sem token não há como buscar o pedido — vale a pena o ML tentar de novo

        try
        {
            // Nunca confia no corpo do webhook — sempre busca o dado real na API do ML.
            var pedido = await _apiClient.ObterPedidoAsync(notificacao.Resource, tokenResult.Value!);
            if (pedido.OrderItems is null || pedido.OrderItems.Count == 0)
                return Result.Ok();

            foreach (var item in pedido.OrderItems)
                await ProcessarItemVendidoAsync(item.Item.Id, pedido.Id.ToString(), item.Quantity, item.UnitPrice);

            return Result.Ok();
        }
        catch (Exception)
        {
            // Falha ao buscar o pedido (rede, ML fora do ar) — nada foi registrado ainda,
            // então é seguro e desejável deixar o ML tentar de novo mais tarde.
            return Result.Fail("Erro ao consultar pedido no Mercado Livre.");
        }
    }

    /// <summary>
    /// Núcleo idempotente do processamento de uma venda, chamado pelo webhook real.
    /// </summary>
    private async Task ProcessarItemVendidoAsync(string idItemPlataforma, string idPedidoPlataforma, int quantidade, decimal preco)
    {
        // [IDEMPOTÊNCIA] Já processamos esse (pedido, item) antes? Entrega duplicada
        // de webhook é comportamento normal — sem essa checagem, decrementaríamos
        // estoque duas vezes para a mesma venda.
        var existente = await _vendaRepo.ObterPorPedidoEItemAsync(idPedidoPlataforma, idItemPlataforma);
        if (existente is not null)
            return;

        var anuncio = await _anuncioRepo.ObterPorItemIdMLAsync(idItemPlataforma);
        if (anuncio is null)
            return; // item não rastreado localmente — nada a reconciliar

        var venda = new Domain.Entities.Integracoes.VendaMercadoLivre(anuncio.Id, idPedidoPlataforma, idItemPlataforma, quantidade, preco);
        await _vendaRepo.AddAsync(venda);
        await _vendaRepo.SaveChangesAsync(); // COMMIT 1 — a venda sobrevive independente do que acontecer a seguir

        try
        {
            switch (anuncio.EntidadeTipo)
            {
                case "Componente":
                    var estoque = await _estoqueRepo.ObterPorComponenteAsync(anuncio.EntidadeId)
                        ?? throw new InvalidOperationException("Estoque não encontrado para o componente.");
                    estoque.Remover(quantidade); // pode lançar InvalidOperationException("Estoque insuficiente")
                    _estoqueRepo.Update(estoque);
                    await _estoqueRepo.SaveChangesAsync();
                    if (estoque.QuantidadeAtual <= 0)
                        anuncio.MarcarComoVendido();
                    break;

                case "VeiculoVenda":
                    var veiculoVenda = await _veiculoVendaRepo.GetByIdAsync(anuncio.EntidadeId)
                        ?? throw new InvalidOperationException("Veículo não encontrado.");
                    veiculoVenda.MarcarComoVendido();
                    _veiculoVendaRepo.Update(veiculoVenda);
                    await _veiculoVendaRepo.SaveChangesAsync();
                    anuncio.MarcarComoVendido();
                    break;

                case "VeiculoConsignacao":
                    var consignacao = await _veiculoConsignacaoRepo.GetByIdAsync(anuncio.EntidadeId)
                        ?? throw new InvalidOperationException("Consignação não encontrada.");
                    // MarcarComoVendida — não ConcluirVenda, que é o passo separado
                    // de "proprietário recebeu o pagamento", fora do escopo da venda ML.
                    consignacao.MarcarComoVendida();
                    _veiculoConsignacaoRepo.Update(consignacao);
                    await _veiculoConsignacaoRepo.SaveChangesAsync();
                    anuncio.MarcarComoVendido();
                    break;

                default:
                    throw new InvalidOperationException($"Tipo de entidade não suportado: {anuncio.EntidadeTipo}");
            }

            _anuncioRepo.Update(anuncio);
            await _anuncioRepo.SaveChangesAsync();

            venda.MarcarComoProcessada();
        }
        catch (Exception ex)
        {
            // A venda no ML já aconteceu de verdade — não há como "rejeitar".
            // Registra o erro para reconciliação manual do admin em vez de deixar a
            // exceção propagar (isso derrubaria o handler do webhook).
            venda.MarcarComoErro(ex.Message);
            anuncio.RegistrarErro(ex.Message);
            _anuncioRepo.Update(anuncio);
            await _anuncioRepo.SaveChangesAsync();
        }

        await _vendaRepo.SaveChangesAsync(); // COMMIT 2 — sempre roda, sucesso ou erro
    }
}
