using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Mappings.Oficina;

public static class OrdemServicoMapping
{
    public static OrdemServicoDTO ToDto(OrdemServico entity)
    {
        return new OrdemServicoDTO
        {
            Id = entity.GetId(),
            ClienteId = entity.GetClienteId(),
            VeiculoClienteId = entity.GetVeiculoClienteId(),
            MecanicoId = entity.GetMecacnicoId(),
            Tipo = entity.GetTipoServico(),
            Descricao = entity.GetDescricao(),
            NumeroPublico = entity.GetNumeroPublico(),
            DataCriacao = entity.GetDataCriacao(),
            PrazoEstimado = entity.GetPrazoEstimado(),
            CustoServico = entity.GetCustoServico(),
            ValorTotal = entity.GetValorTotal(),
            Status = entity.GetStatus(),
            Itens = entity.Itens
                .Select(ItemOrdemServicoMapping.ToDto)
                .ToList(),
            Checklist = entity.Checklist
                .OrderBy(c => c.OrdemExibicao)
                .Select(c => new ChecklistItemDTO
                {
                    Id = c.Id,
                    Descricao = c.Descricao,
                    Status = c.Status.ToString(),
                    Origem = c.Origem.ToString(),
                    OrdemExibicao = c.OrdemExibicao
                })
                .ToList()
        };
    }

    public static OrdemServicoListaDTO ToListaDto(OrdemServico entity)
    {
        return new OrdemServicoListaDTO
        {
            Id = entity.GetId(),
            ClienteId = entity.GetClienteId(),
            VeiculoClienteId = entity.GetVeiculoClienteId(),
            Tipo = entity.GetTipoServico(),
            NumeroPublico = entity.GetNumeroPublico(),
            Status = entity.GetStatus(),
            PrazoEstimado = entity.GetPrazoEstimado(),
            ValorTotal = entity.GetValorTotal()
        };
    }

    public static OrdemServicoPublicaDTO ToPublicaDto(OrdemServico entity)
    {
        return new OrdemServicoPublicaDTO
        {
            NumeroPublico = entity.NumeroPublico,
            Tipo = entity.Tipo.ToString(),
            Descricao = entity.Descricao,
            Status = entity.Status.ToString(),
            DataCriacao = entity.DataCriacao,
            PrazoEstimado = entity.PrazoEstimado,
            Checklist = entity.Checklist
                .OrderBy(c => c.OrdemExibicao)
                .Select(c => new ChecklistItemPublicoDTO
                {
                    Descricao = c.Descricao,
                    Status = c.Status.ToString()
                })
                .ToList()
        };
    }

    public static OrdemServico ToEntity(CriarOrdemServicoDTO dto)
    {
        return new OrdemServico(
            dto.VeiculoClienteId,
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
}