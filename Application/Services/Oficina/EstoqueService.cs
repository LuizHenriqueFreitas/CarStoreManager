using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.Componente;
using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

public class EstoqueService : IEstoqueService
{
    private readonly IEstoqueRepository _estoqueRepo;
    private readonly IComponenteRepository _componenteRepo;
    private readonly IOrdemServicoRepository _ordemRepo;
    private readonly IBalancoMensalDespesaService _balancoDespesaService;

    public EstoqueService(
        IEstoqueRepository estoqueRepo,
        IComponenteRepository componenteRepo,
        IOrdemServicoRepository ordemRepo,
        IBalancoMensalDespesaService balancoDespesaService)
    {
        _estoqueRepo = estoqueRepo;
        _componenteRepo = componenteRepo;
        _ordemRepo = ordemRepo;
        _balancoDespesaService = balancoDespesaService;
    }

    public async Task<Result<IEnumerable<EstoqueComponenteDTO>>> ListarAsync()
    {
        var estoques = await _estoqueRepo.GetAllAsync();
        return Result<IEnumerable<EstoqueComponenteDTO>>.Ok(estoques.Select(MapToDto));
    }

    public async Task<Result<IEnumerable<EstoqueComponenteDTO>>> ListarComEstoqueBaixoAsync()
    {
        var estoques = await _estoqueRepo.ObterComEstoqueBaixoAsync();
        return Result<IEnumerable<EstoqueComponenteDTO>>.Ok(estoques.Select(MapToDto));
    }

    public async Task<Result<EstoqueComponenteDTO>> ObterPorComponenteAsync(Guid componenteId)
    {
        var estoque = await _estoqueRepo.ObterPorComponenteAsync(componenteId);
        return estoque is null
            ? Result<EstoqueComponenteDTO>.Fail("Estoque não encontrado para o componente.")
            : Result<EstoqueComponenteDTO>.Ok(MapToDto(estoque));
    }

    public async Task<Result> CriarOuAtualizarMinimoAsync(Guid componenteId, int quantidadeMinima)
    {
        if (quantidadeMinima < 0)
            return Result.Fail("Quantidade mínima não pode ser negativa.");

        var componente = await _componenteRepo.GetByIdAsync(componenteId);
        if (componente is null)
            return Result.Fail("Componente não encontrado.");

        try
        {
            var estoque = await _estoqueRepo.ObterPorComponenteAsync(componenteId);
            if (estoque is null)
            {
                estoque = new EstoqueComponente(componenteId, quantidadeMinima);
                await _estoqueRepo.AddAsync(estoque);
            }
            else
            {
                // Mantém Id, QuantidadeAtual etc — só atualiza o mínimo.
                estoque.DefinirMinimo(quantidadeMinima);
                _estoqueRepo.Update(estoque);
            }

            await _estoqueRepo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> EntradaAsync(Guid componenteId, int quantidade)
    {
        if (quantidade <= 0)
            return Result.Fail("Quantidade deve ser positiva.");

        try
        {
            var estoque = await _estoqueRepo.ObterPorComponenteAsync(componenteId);
            var novo = estoque is null;
            estoque ??= new EstoqueComponente(componenteId, 0);

            estoque.Adicionar(quantidade);

            if (novo)
                await _estoqueRepo.AddAsync(estoque);
            else
                _estoqueRepo.Update(estoque);

            await _estoqueRepo.SaveChangesAsync();

            // Concilia automaticamente itens de OS que estavam aguardando
            // a chegada deste componente.
            await ConciliarItensAguardandoAsync(componenteId);

            var componente = await _componenteRepo.GetByIdAsync(componenteId);
            if (componente is not null)
                await RegistrarDespesaCompraComponenteAsync(componente, quantidade, DateTime.Today);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /// <summary>
    /// Lança o custo da reposição de estoque como item de despesa no
    /// balanço mensal da competência informada — categoria "Compra de
    /// componentes", setor Oficina. Mesmo padrão de
    /// <see cref="Concessionaria.VeiculoVendaService"/> pra compra de
    /// veículo: garante as despesas recorrentes do modelo antes de somar
    /// o item ("já existe" é esperado e ignorado quando outra entrada de
    /// estoque já tocou essa competência).
    /// </summary>
    private async Task RegistrarDespesaCompraComponenteAsync(
        Domain.Entities.Oficina.Componente componente, int quantidade, DateTime dataCompra)
    {
        await _balancoDespesaService.GerarDoModeloAsync(dataCompra.Year, dataCompra.Month);

        await _balancoDespesaService.SalvarItemAsync(new SalvarItemBalancoDTO
        {
            Ano = dataCompra.Year,
            Mes = dataCompra.Month,
            Nome = $"Compra de componente: {componente.Nome} — {componente.SKUInterno} (x{quantidade})",
            Setor = "Oficina",
            Categoria = "Compra de componentes",
            Valor = componente.CustoUnitario * quantidade
        });
    }

    public async Task<Result> SaidaAsync(Guid componenteId, int quantidade)
    {
        if (quantidade <= 0)
            return Result.Fail("Quantidade deve ser positiva.");

        var estoque = await _estoqueRepo.ObterPorComponenteAsync(componenteId);
        if (estoque is null)
            return Result.Fail("Estoque não encontrado para o componente.");

        try
        {
            estoque.Remover(quantidade);
            _estoqueRepo.Update(estoque);
            await _estoqueRepo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /// <summary>
    /// Marca como "Recebido" todos os itens de OS que estavam aguardando
    /// chegada deste componente. Salva mudanças via OrdemServicoRepository.
    /// Falhas são silenciosas: a entrada de estoque já tem sucesso por si.
    /// </summary>
    private async Task ConciliarItensAguardandoAsync(Guid componenteId)
    {
        try
        {
            var ordens = (await _ordemRepo.ObterComItensAguardandoAsync(componenteId)).ToList();
            if (ordens.Count == 0) return;

            foreach (var ordem in ordens)
            {
                var itens = ordem.Itens.Where(i =>
                    i.ComponenteId == componenteId
                    && i.Origem == Domain.Enums.OrigemItemOrdemServico.Encomenda
                    && i.StatusItem == Domain.Enums.StatusItemOrdemServico.AguardandoChegada);

                foreach (var item in itens)
                    item.MarcarComoRecebido();

                _ordemRepo.Update(ordem);
            }

            await _ordemRepo.SaveChangesAsync();
        }
        catch
        {
            // não falha a entrada de estoque por causa da conciliação
        }
    }

    private static EstoqueComponenteDTO MapToDto(EstoqueComponente e) => new()
    {
        Id = e.Id,
        ComponenteId = e.PecaId,
        ComponenteNome = e.Componente?.Nome ?? string.Empty,
        ComponentePartNumber = e.Componente?.PartNumber ?? string.Empty,
        Sistema = string.Empty,
        QuantidadeAtual = e.QuantidadeAtual,
        QuantidadeMinima = e.QuantidadeMinima
    };
}
