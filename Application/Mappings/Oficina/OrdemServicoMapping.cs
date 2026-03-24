using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Mappings.Oficina;

public static class OrdemServicoMapping
{
    // =========================
    // ENTITY → DETALHE
    // =========================

    public static OrdemServicoDTO ToDto(OrdemServico entity)
    {
        return new OrdemServicoDTO
        {
            Id = entity.Id,
            ClienteId = entity.ClienteId,
            VeiculoId = entity.VeiculoId,
            MecanicoId = entity.MecanicoId,

            Descricao = entity.Descricao,
            DataCriacao = entity.DataCriacao,
            PrazoEstimado = entity.PrazoEstimado,

            CustoServico = entity.GetCustoServico(),
            ValorTotal = entity.GetValorTotal(),

            Status = entity.Status.ToString(),

            Itens = entity.Itens
                .Select(ItemOrdemServicoMapping.ToDto)
                .ToList() ?? new List<ItemOrdemServicoDTO>()
        };
    }


    // =========================
    // ENTITY → LISTA (TABELA)
    // =========================

    public static OrdemServicoListaDTO ToListaDto(OrdemServico entity)
    {
        return new OrdemServicoListaDTO
        {
            Id = entity.Id,
            NomeCliente = entity.Status.ToString(),
            Descricao = entity.Descricao,
            Status = entity.Status.ToString(),
            PrazoEstimado = entity.PrazoEstimado,
            ValorTotal = entity.GetValorTotal()
        };
    }


    // =========================
    // DTO → ENTITY (CRIAÇÃO)
    // =========================

    public static OrdemServico ToEntity(CriarOrdemServicoDTO dto)
    {
        return new OrdemServico(
            dto.VeiculoId,
            dto.MecanicoId,
            dto.ClienteId,
            ConverterTipo(dto.Tipo),
            dto.Descricao,
            dto.PrazoEstimado,
            new Dinheiro(dto.CustoServico)
        );
    }

    private static TipoServico ConverterTipo(string tipo)
    {
        if (!Enum.TryParse<TipoServico>(tipo, true, out var resultado))
            throw new ArgumentException($"Tipo inválido: {tipo}");

        return resultado;
    }

    // =========================
    // LISTA
    // =========================

    public static IEnumerable<OrdemServicoListaDTO> ToListaDtoList(IEnumerable<OrdemServico> entities)
    {
        return entities.Select(ToListaDto);
    }
}