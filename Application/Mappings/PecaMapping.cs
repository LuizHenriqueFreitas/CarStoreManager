using CarStoreManager.Application.DTOs;
using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Application.Mappings;

public static class PecaMapping
{
    public static PecaDTO ToDTO(Peca peca)
    {
        return new PecaDTO(
            peca.Id,
            peca.Nome,
            peca.Modelo,
            peca.Valor,
            peca.QuantidadeEstoque
        );
    }

    public static Peca ToEntity(PecaDTO dto)
    {
        return new Peca(
            dto.Nome,
            dto.Modelo,
            dto.Valor,
            dto.QuantidadeEstoque
        );
    }
}