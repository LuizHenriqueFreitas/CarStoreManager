using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Application.Mappings.Oficina;

public static class ItemOrdemServicoMapping
{
    // =========================
    // ENTITY → DTO
    // =========================

    public static ItemOrdemServicoDTO ToDto(ItemOrdemServico entity)
    {
        return new ItemOrdemServicoDTO
        {
            Id = entity.GetId(),
            ComponenteId = entity.GetComponentId(),
            Quantidade = entity.GetQuantidade(),
            ValorUnitario = entity.GetValorUnitario(),
            ValorTotal = entity.GetValorTotal()
        };
    }


    // =========================
    // LISTA
    // =========================

    public static IEnumerable<ItemOrdemServicoDTO> ToDtoList(IEnumerable<ItemOrdemServico> entities)
    {
        return entities.Select(ToDto);
    }
}