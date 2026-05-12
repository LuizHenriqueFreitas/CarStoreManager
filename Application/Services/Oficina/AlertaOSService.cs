using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services.Oficina;

public class AlertaOSService : IAlertaOSService
{
    private readonly IAlertaOSRepository _alertasRepo;
    private readonly IOrdemServicoRepository _ordensRepo;

    public AlertaOSService(IAlertaOSRepository alertasRepo, IOrdemServicoRepository ordensRepo)
    {
        _alertasRepo = alertasRepo;
        _ordensRepo = ordensRepo;
    }

    public async Task<Result<AlertaOSDTO>> EmitirAsync(Guid ordemId, Guid mecanicoId, CriarAlertaOSDTO dto)
    {
        var ordem = await _ordensRepo.GetByIdAsync(ordemId);
        if (ordem is null) return Result<AlertaOSDTO>.Fail("OS não encontrada.");

        try
        {
            // Pausa a OS — só permitido em EmAndamento (regra na própria entidade)
            ordem.Pausar();
            var alerta = new AlertaOS(ordemId, mecanicoId, dto.Descricao);

            await _alertasRepo.AddAsync(alerta);
            _ordensRepo.Update(ordem);
            await _alertasRepo.SaveChangesAsync();

            return Result<AlertaOSDTO>.Ok(MapToDto(alerta));
        }
        catch (Exception ex) { return Result<AlertaOSDTO>.Fail(ex.Message); }
    }

    public async Task<Result<IEnumerable<AlertaOSDTO>>> ListarPendentesAsync()
    {
        var alertas = await _alertasRepo.ObterPorStatusAsync(StatusAlertaOS.Pendente);
        return Result<IEnumerable<AlertaOSDTO>>.Ok(alertas.Select(MapToDto));
    }

    public async Task<Result<IEnumerable<AlertaOSDTO>>> ListarPorOrdemAsync(Guid ordemId)
    {
        var alertas = await _alertasRepo.ObterPorOrdemAsync(ordemId);
        return Result<IEnumerable<AlertaOSDTO>>.Ok(alertas.Select(MapToDto));
    }

    public async Task<Result> ResolverAsync(Guid alertaId, Guid resolvidoPor, ResolverAlertaDTO dto)
    {
        var alerta = await _alertasRepo.GetByIdAsync(alertaId);
        if (alerta is null) return Result.Fail("Alerta não encontrado.");

        var ordem = await _ordensRepo.GetByIdAsync(alerta.OrdemServicoId);
        if (ordem is null) return Result.Fail("OS vinculada não encontrada.");

        try
        {
            alerta.RegistrarDecisaoCliente(dto.Aprovou, resolvidoPor, dto.ObservacaoCliente);
            // Em ambos os casos a OS volta para EmAndamento — recepcionista
            // já ajustou itens/custos antes (se aprovou) via endpoints existentes.
            ordem.Retomar();
            _alertasRepo.Update(alerta);
            _ordensRepo.Update(ordem);
            await _alertasRepo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    private static AlertaOSDTO MapToDto(AlertaOS a) => new()
    {
        Id = a.Id,
        OrdemServicoId = a.OrdemServicoId,
        MecanicoId = a.MecanicoId,
        Descricao = a.Descricao,
        Status = a.Status.ToString(),
        DataCriacao = a.DataCriacao,
        DataResolucao = a.DataResolucao,
        ResolvidoPor = a.ResolvidoPor,
        ObservacaoCliente = a.ObservacaoCliente
    };
}
