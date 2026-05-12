//Arquivo que faz as converções entre DTOS e Entidades de Veiculos de Venda

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
            Marca = entity.GetMarca(),
            Modelo = entity.GetModelo(),
            Cor = entity.GetCor(),
            Motorizacao = entity.GetMotorizacao(),
            Ano = entity.GetAno(),
            Quilometragem = entity.GetQuilometragem(),
            Placa = entity.GetPlacaCarro(),
            Renavam = entity.GetRenavam(),
            Cambio = entity.Cambio.ToString(),
            Combustivel = entity.Combustivel.ToString(),
            Disponibilidade = entity.Disponibilidade.ToString(),
            Valor = entity.GetValor(),
            AnoUltimoIpvaPago = entity.AnoUltimoIpvaPago,
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
            Marca = entity.GetMarca(),
            Modelo = entity.GetModelo(),
            Ano = entity.GetAno(),
            Quilometragem = entity.Quilometragem.GetQuilometragem(),
            Motorizacao = entity.GetMotorizacao(),
            Combustivel = entity.Combustivel.ToString(),
            Disponibilidade = entity.Disponibilidade.ToString(),
            Placa = entity.Placa.ToString(),
            AnoUltimoIpvaPago = entity.AnoUltimoIpvaPago,
            // Acessorios é [Flags] enum — extrai cada flag setada como string
            Acessorios = Enum.GetValues<CarStoreManager.Domain.Enums.AcessoriosVeiculo>()
                .Where(a => a != CarStoreManager.Domain.Enums.AcessoriosVeiculo.Nenhum
                            && entity.Acessorios.HasFlag(a))
                .Select(a => a.ToString())
                .ToList(),
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
            dto.Ano,
            dto.Quilometragem,
            dto.Placa,
            dto.Renavam,
            ConverterEnum<TipoCambio>(dto.Cambio, "Câmbio"),
            ConverterEnum<TipoCombustivel>(dto.Combustivel, "Combustível"),
            dto.Valor,
            ConverterAcessorios(dto.Acessorios),
            dto.AnoUltimoIpvaPago
        );
    }

    public static void UpdateEntity(VeiculoVenda entity, AtualizarVeiculoVendaDTO dto)
    {
        entity.AtualizarVeiculoVendaDados(
            new Dinheiro(dto.Valor),
            ConverterEnum<DisponibilidadeVeiculo>(dto.Disponibilidade, "Disponibilidade")
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