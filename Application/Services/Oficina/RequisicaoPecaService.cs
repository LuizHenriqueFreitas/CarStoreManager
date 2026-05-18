using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services.Oficina;

public class RequisicaoPecaService : IRequisicaoPecaService
{
    private readonly IRequisicaoPecaRepository _requisicoesRepo;
    private readonly IOrdemServicoRepository _ordensRepo;
    private readonly IVeiculoClienteRepository _veiculosRepo;
    private readonly IComponenteRepository _componentesRepo;

    public RequisicaoPecaService(
        IRequisicaoPecaRepository requisicoesRepo,
        IOrdemServicoRepository ordensRepo,
        IVeiculoClienteRepository veiculosRepo,
        IComponenteRepository componentesRepo)
    {
        _requisicoesRepo = requisicoesRepo;
        _ordensRepo = ordensRepo;
        _veiculosRepo = veiculosRepo;
        _componentesRepo = componentesRepo;
    }

    public async Task<Result<RequisicaoPecaDTO>> AbrirAsync(
        Guid ordemId, Guid mecanicoId, CriarRequisicaoPecaDTO dto)
    {
        var ordem = await _ordensRepo.GetByIdAsync(ordemId);
        if (ordem is null) return Result<RequisicaoPecaDTO>.Fail("OS não encontrada.");

        try
        {
            var req = new RequisicaoPecaOS(ordemId, mecanicoId,
                dto.DescricaoPeca, dto.Justificativa, dto.Quantidade);

            // Pausa o orçamento (se ainda estiver em fase de montagem)
            ordem.MarcarComoBuscandoPecas();

            await _requisicoesRepo.AddAsync(req);
            _ordensRepo.Update(ordem);
            await _requisicoesRepo.SaveChangesAsync();

            return Result<RequisicaoPecaDTO>.Ok(await MapAsync(req));
        }
        catch (Exception ex) { return Result<RequisicaoPecaDTO>.Fail(ex.Message); }
    }

    public async Task<Result<IEnumerable<RequisicaoPecaDTO>>> ListarPendentesAsync()
    {
        var pendentes = await _requisicoesRepo.ObterPorStatusAsync(StatusRequisicaoPeca.Pendente);
        var lista = new List<RequisicaoPecaDTO>();
        foreach (var r in pendentes) lista.Add(await MapAsync(r));
        return Result<IEnumerable<RequisicaoPecaDTO>>.Ok(lista);
    }

    public async Task<Result<IEnumerable<RequisicaoPecaDTO>>> ListarPorOrdemAsync(Guid ordemId)
    {
        var reqs = await _requisicoesRepo.ObterPorOrdemAsync(ordemId);
        var lista = new List<RequisicaoPecaDTO>();
        foreach (var r in reqs) lista.Add(await MapAsync(r));
        return Result<IEnumerable<RequisicaoPecaDTO>>.Ok(lista);
    }

    public async Task<Result> AtenderAsync(Guid requisicaoId, Guid resolvidaPor, AtenderRequisicaoDTO dto)
    {
        var req = await _requisicoesRepo.GetByIdAsync(requisicaoId);
        if (req is null) return Result.Fail("Requisição não encontrada.");

        var componente = await _componentesRepo.GetByIdAsync(dto.ComponenteId);
        if (componente is null) return Result.Fail("Componente vinculado não existe.");

        var ordem = await _ordensRepo.GetByIdAsync(req.OrdemServicoId);
        if (ordem is null) return Result.Fail("OS associada à requisição não encontrada.");

        try
        {
            req.Atender(resolvidaPor, dto.ComponenteId, dto.Observacao);

            // Cria automaticamente o item na OS com Origem=Encomenda. O mecânico
            // não precisa mais adicionar manualmente — atender a requisição já
            // amarra a peça à OS, marcada como AguardandoChegada.
            var quantidade = dto.Quantidade ?? req.Quantidade;
            var valorUnit = dto.ValorUnitario ?? componente.ValorVenda;

            if (quantidade <= 0) quantidade = 1;
            if (valorUnit < 0) valorUnit = 0;

            var item = new ItemOrdemServico(
                componente.Id,
                ordem.Id,
                quantidade,
                valorUnit,
                OrigemItemOrdemServico.Encomenda);

            ordem.AdicionarItem(item);

            _requisicoesRepo.Update(req);
            _ordensRepo.Update(ordem);
            await _ordensRepo.AdicionarItemAsync(item);
            await _requisicoesRepo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> RejeitarAsync(Guid requisicaoId, Guid resolvidaPor, RejeitarRequisicaoDTO dto)
    {
        var req = await _requisicoesRepo.GetByIdAsync(requisicaoId);
        if (req is null) return Result.Fail("Requisição não encontrada.");

        try
        {
            req.Rejeitar(resolvidaPor, dto.Motivo);
            _requisicoesRepo.Update(req);
            await _requisicoesRepo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> LiberarOrdemAsync(Guid ordemId)
    {
        var ordem = await _ordensRepo.GetByIdAsync(ordemId);
        if (ordem is null) return Result.Fail("OS não encontrada.");

        var pendentes = await _requisicoesRepo.ContarPendentesPorOrdemAsync(ordemId);
        if (pendentes > 0)
            return Result.Fail($"Ainda há {pendentes} requisição(ões) pendente(s) — atenda ou rejeite todas antes de liberar.");

        try
        {
            ordem.RetomarAposBuscaDePecas();
            _ordensRepo.Update(ordem);
            await _ordensRepo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    private async Task<RequisicaoPecaDTO> MapAsync(RequisicaoPecaOS r)
    {
        var dto = new RequisicaoPecaDTO
        {
            Id = r.Id,
            OrdemServicoId = r.OrdemServicoId,
            MecanicoId = r.MecanicoId,
            DescricaoPeca = r.DescricaoPeca,
            Justificativa = r.Justificativa,
            Quantidade = r.Quantidade,
            Status = r.Status.ToString(),
            DataCriacao = r.DataCriacao,
            DataResolucao = r.DataResolucao,
            ResolvidaPor = r.ResolvidaPor,
            ObservacaoAdmin = r.ObservacaoAdmin,
            ComponenteAtendidoId = r.ComponenteAtendidoId
        };

        // Snapshot do veículo (best-effort — falha silenciosa não quebra a listagem)
        try
        {
            var ordem = await _ordensRepo.GetByIdAsync(r.OrdemServicoId);
            if (ordem is not null)
            {
                var veic = await _veiculosRepo.GetByIdAsync(ordem.VeiculoClienteId);
                if (veic is not null)
                {
                    dto.VeiculoMarcaModelo = $"{veic.GetMarca()} {veic.GetModelo()}";
                    dto.VeiculoPlaca = veic.Placa.ToString();
                    dto.VeiculoAno = veic.GetAno();
                }
            }
        }
        catch { /* best-effort */ }

        return dto;
    }
}
