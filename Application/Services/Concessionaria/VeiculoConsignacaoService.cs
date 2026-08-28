using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs;
using CarStoreManager.Application.DTOs.Concessionaria.VeiculoConsignacao;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Concessionaria;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

/*
    Fotos não vivem como navegação na entidade VeiculoConsignacao — são
    geridas via IFotoService com EntidadeTipo="VeiculoConsignacao", igual ao
    caminho genérico já usado por outras entidades. Por isso este service
    injeta IFotoService diretamente, em vez de expor AdicionarFoto/RemoverFoto
    aqui (a página de detalhe chama IFotoService direto).
*/
public class VeiculoConsignacaoService : IVeiculoConsignacaoService
{
    private readonly IVeiculoConsignacaoRepository _repository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IVendedorRepository _vendedorRepository;
    private readonly IFotoService _fotoService;

    public VeiculoConsignacaoService(
        IVeiculoConsignacaoRepository repository,
        IClienteRepository clienteRepository,
        IVendedorRepository vendedorRepository,
        IFotoService fotoService)
    {
        _repository = repository;
        _clienteRepository = clienteRepository;
        _vendedorRepository = vendedorRepository;
        _fotoService = fotoService;
    }

    public async Task<Result<VeiculoConsignacaoDTO>> GetByIdAsync(Guid id)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        if (veiculo is null)
            return Result<VeiculoConsignacaoDTO>.Fail("Veículo consignado não encontrado");

        if (veiculo.TentarExpirar())
        {
            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();
        }

        var (clienteNome, vendedorNome) = await ObterNomesAsync(veiculo.ClienteProprietarioId, veiculo.VendedorResponsavelId);
        var rFotos = await _fotoService.GetFotosByEntidadeAsync("VeiculoConsignacao", id);
        var fotos = rFotos.IsSuccess ? rFotos.Value! : new List<FotoDto>();

        return Result<VeiculoConsignacaoDTO>.Ok(VeiculoConsignacaoMapping.ToDto(veiculo, clienteNome, vendedorNome, fotos));
    }

    public async Task<Result<IEnumerable<VeiculoConsignacaoListaDTO>>> GetAllAsync()
    {
        var veiculos = await _repository.GetAllAsync();
        return Result<IEnumerable<VeiculoConsignacaoListaDTO>>.Ok(await MapListaAsync(veiculos));
    }

    public async Task<Result<IEnumerable<VeiculoConsignacaoListaDTO>>> ObterAtivasAsync()
    {
        var ativas = await _repository.ObterPorStatusAsync(Domain.Enums.StatusConsignacao.Ativa);
        return Result<IEnumerable<VeiculoConsignacaoListaDTO>>.Ok(await MapListaAsync(ativas));
    }

    public async Task<Result<Guid>> AddAsync(CriarVeiculoConsignacaoDTO dto)
    {
        try
        {
            var cliente = await _clienteRepository.GetByIdAsync(dto.ClienteProprietarioId);
            if (cliente is null)
                return Result<Guid>.Fail("Cliente proprietário não encontrado");

            var vendedor = await _vendedorRepository.GetByIdAsync(dto.VendedorResponsavelId);
            if (vendedor is null)
                return Result<Guid>.Fail("Vendedor responsável não encontrado");

            var veiculo = VeiculoConsignacaoMapping.ToEntity(dto);
            await _repository.AddAsync(veiculo);
            await _repository.SaveChangesAsync();
            return Result<Guid>.Ok(veiculo.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar consignação: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(AtualizarVeiculoConsignacaoDTO dto)
    {
        var veiculo = await _repository.GetByIdAsync(dto.Id);
        if (veiculo is null)
            return Result.Fail("Veículo consignado não encontrado");

        try
        {
            VeiculoConsignacaoMapping.UpdateEntity(veiculo, dto);
            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> RenovarAsync(Guid id, int diasAdicionais = 90)
        => await ExecutarTransicaoAsync(id, v => v.RenovarConsignacao(diasAdicionais));

    public async Task<Result> MarcarComoVendidaAsync(Guid id)
        => await ExecutarTransicaoAsync(id, v => v.MarcarComoVendida());

    public async Task<Result> ConcluirVendaAsync(Guid id)
        => await ExecutarTransicaoAsync(id, v => v.ConcluirVenda());

    public async Task<Result> DevolverAsync(Guid id)
        => await ExecutarTransicaoAsync(id, v => v.Devolver());

    public async Task<Result> CancelarAsync(Guid id, string motivo)
        => await ExecutarTransicaoAsync(id, v => v.Cancelar(motivo));

    public async Task<Result> RemoveAsync(Guid id)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        if (veiculo is null)
            return Result.Fail("Veículo consignado não encontrado");

        _repository.Remove(veiculo);
        await _repository.SaveChangesAsync();
        return Result.Ok();
    }

    private async Task<Result> ExecutarTransicaoAsync(Guid id, Action<Domain.Entities.Concessionaria.VeiculoConsignacao> transicao)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        if (veiculo is null)
            return Result.Fail("Veículo consignado não encontrado");

        try
        {
            // NÃO chama _repository.Update(veiculo) aqui: a entidade já está
            // rastreada pelo DbContext (veio de GetByIdAsync com
            // .Include(Historico) no mesmo escopo). Um Update() explícito
            // percorre o grafo inteiro e marca até o HistoricoConsignacao
            // recém-adicionado por RegistrarEvento() como Modified em vez de
            // Added (o Id dele já é um Guid não-vazio, gerado no construtor,
            // então a heurística do EF não reconhece que é novo) — o UPDATE
            // gerado pra essa linha inexistente afeta 0 linhas e o SaveChanges
            // lança DbUpdateConcurrencyException. O change tracker já vê as
            // mutações feitas por transicao(); só falta persistir.
            transicao(veiculo);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    private async Task<List<VeiculoConsignacaoListaDTO>> MapListaAsync(IEnumerable<Domain.Entities.Concessionaria.VeiculoConsignacao> veiculos)
    {
        var lista = new List<VeiculoConsignacaoListaDTO>();
        foreach (var v in veiculos)
        {
            var (clienteNome, vendedorNome) = await ObterNomesAsync(v.ClienteProprietarioId, v.VendedorResponsavelId);
            var rFotos = await _fotoService.GetFotosByEntidadeAsync("VeiculoConsignacao", v.Id);
            var fotoPrincipal = rFotos.IsSuccess
                ? rFotos.Value!.OrderBy(f => f.Ordem).FirstOrDefault()?.Url
                : null;
            lista.Add(VeiculoConsignacaoMapping.ToListaDto(v, clienteNome, vendedorNome, fotoPrincipal));
        }
        return lista;
    }

    private async Task<(string clienteNome, string vendedorNome)> ObterNomesAsync(Guid clienteId, Guid vendedorId)
    {
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        var vendedor = await _vendedorRepository.GetByIdAsync(vendedorId);
        return (cliente?.GetNome() ?? "Desconhecido", vendedor?.GetNome() ?? "Desconhecido");
    }
}
