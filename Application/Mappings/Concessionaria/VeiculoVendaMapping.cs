using CarStoreManager.Application.DTOs.Concessionaria.VeiculoVenda;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Mappings.Concessionaria;

public static class VeiculoVendaMapping
{
    public static VeiculoVendaDTO ToDto(VeiculoVenda entity)
    {
        return new VeiculoVendaDTO
        {
            Id = entity.Id,
            Marca = entity.Marca,
            Modelo = entity.Modelo,
            Cor = entity.Cor,
            Motorizacao = entity.Motorizacao,
            Ano = entity.GetAno(),
            Quilometragem = entity.GetQuilometragem(),
            Placa = entity.GetPlaca(),
            Cambio = entity.Cambio.ToString(),
            Combustivel = entity.Combustivel.ToString(),
            Estado = entity.Estado.ToString(),
            Disponibilidade = entity.Disponibilidade.ToString(),
            Valor = entity.GetValor(),
            Acessorios = entity.GetAcessoriosLista(),
            Fotos = entity.Fotos
                .OrderBy(f => f.Ordem)
                .Select(f => f.Url)
                .ToList()
        };
    }

    public static VeiculoVendaListaDTO ToListaDto(VeiculoVenda entity)
    {
        return new VeiculoVendaListaDTO
        {
            Id = entity.Id,
            Marca = entity.Marca,
            Modelo = entity.Modelo,
            Ano = entity.GetAno(),
            Combustivel = entity.Combustivel.ToString(),
            Disponibilidade = entity.Disponibilidade.ToString(),
            Valor = entity.GetValor(),
            FotoPrincipal = entity.Fotos
                .OrderBy(f => f.Ordem)
                .FirstOrDefault()?.Url
        };
    }

    public static VeiculoVenda ToEntity(CriarVeiculoVendaDTO dto)
    {
        return new VeiculoVenda(
            dto.Marca,
            dto.Modelo,
            dto.Cor,
            dto.Motorizacao,
            new Ano(dto.Ano),
            new Quilometragem(dto.Quilometragem),
            new PlacaVeiculo(dto.Placa),
            ConverterEnum<TipoCambio>(dto.Cambio, "Câmbio"),
            ConverterEnum<TipoCombustivel>(dto.Combustivel, "Combustível"),
            ConverterEnum<EstadoConservacao>(dto.Estado, "Estado"),
            new Dinheiro(dto.Valor),
            ConverterAcessorios(dto.Acessorios)
        );
    }

    public static void UpdateEntity(VeiculoVenda entity, AtualizarVeiculoVendaDTO dto)
    {
        entity.AtualizarDados(
            dto.Marca,
            dto.Modelo,
            dto.Cor,
            dto.Motorizacao,
            new Dinheiro(dto.Valor),
            ConverterEnum<DisponibilidadeVeiculo>(dto.Disponibilidade, "Disponibilidade"),
            dto.Quilometragem,
            ConverterEnum<EstadoConservacao>(dto.Estado, "Estado"),
            ConverterEnum<TipoCambio>(dto.Cambio, "Câmbio"),
            ConverterEnum<TipoCombustivel>(dto.Combustivel, "Combustível"),
            ConverterAcessorios(dto.Acessorios)
        );
    }

    // =========================
    // HELPERS PRIVADOS
    // =========================

    private static T ConverterEnum<T>(string valor, string campo) where T : struct, Enum
    {
        if (!Enum.TryParse<T>(valor, true, out var resultado))
            throw new ArgumentException($"{campo} inválido: {valor}");
        return resultado;
    }

    private static AcessoriosVeiculo ConverterAcessorios(List<string> acessorios)
    {
        var resultado = AcessoriosVeiculo.Nenhum;
        foreach (var a in acessorios)
            if (Enum.TryParse<AcessoriosVeiculo>(a, true, out var acc))
                resultado |= acc;
        return resultado;
    }
}