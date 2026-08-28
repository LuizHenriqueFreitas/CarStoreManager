using CarStoreManager.Application.DTOs;
using CarStoreManager.Application.DTOs.Concessionaria.VeiculoConsignacao;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Mappings.Concessionaria;

public static class VeiculoConsignacaoMapping
{
    public static VeiculoConsignacaoDTO ToDto(
        VeiculoConsignacao entity,
        string clienteNome,
        string vendedorNome,
        List<FotoDto> fotos)
    {
        return new VeiculoConsignacaoDTO
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
            Cambio = entity.GetCambio(),
            Combustivel = entity.GetCombustivel(),
            Acessorios = entity.GetAcessoriosLista(),
            ClienteProprietarioId = entity.ClienteProprietarioId,
            ClienteProprietarioNome = clienteNome,
            VendedorResponsavelId = entity.VendedorResponsavelId,
            VendedorResponsavelNome = vendedorNome,
            TipoComissao = entity.Comissao.Tipo.ToString(),
            ValorVendaEsperado = entity.Comissao.ValorVendaEsperado.GetValorDinheiro(),
            ValorFixoProprietario = entity.Comissao.ValorFixoProprietario?.GetValorDinheiro(),
            PorcentagemProprietario = entity.Comissao.PorcentagemProprietario?.GetDescontoValor(),
            TextoContrato = entity.TextoContrato,
            UrlContratoPdf = entity.UrlContratoPdf,
            DataInicio = entity.DataInicio,
            DataVencimento = entity.DataVencimento,
            DiasRestantes = entity.DiasRestantes,
            EstaVencido = entity.EstaVencido,
            Status = entity.Status.ToString(),
            Historico = entity.Historico
                .OrderByDescending(h => h.DataCriacao)
                .Select(h => new HistoricoConsignacaoDTO
                {
                    Id = h.Id,
                    TipoEvento = h.TipoEvento.ToString(),
                    Descricao = h.Descricao,
                    Data = h.DataCriacao
                })
                .ToList(),
            Fotos = fotos
        };
    }

    public static VeiculoConsignacaoListaDTO ToListaDto(
        VeiculoConsignacao entity,
        string clienteNome,
        string vendedorNome,
        string? fotoPrincipal = null)
    {
        return new VeiculoConsignacaoListaDTO
        {
            Id = entity.Id,
            Marca = entity.GetMarca(),
            Modelo = entity.GetModelo(),
            Ano = entity.GetAno(),
            Quilometragem = entity.GetQuilometragem(),
            Motorizacao = entity.GetMotorizacao(),
            Combustivel = entity.GetCombustivel(),
            Placa = entity.GetPlacaCarro(),
            Status = entity.Status.ToString(),
            DiasRestantes = entity.DiasRestantes,
            EstaVencido = entity.EstaVencido,
            ValorVendaEsperado = entity.Comissao.ValorVendaEsperado.GetValorDinheiro(),
            ClienteProprietarioNome = clienteNome,
            VendedorResponsavelNome = vendedorNome,
            FotoPrincipal = fotoPrincipal,
            DataCriacao = entity.DataCriacao
        };
    }

    public static VeiculoConsignacao ToEntity(CriarVeiculoConsignacaoDTO dto)
    {
        var comissao = ConverterEnum<TipoComissaoConsignacao>(dto.TipoComissao, "Tipo de comissão") switch
        {
            TipoComissaoConsignacao.Fixo => ComissaoConsignacao.CriarFixo(
                dto.ValorVendaEsperado,
                dto.ValorFixoProprietario ?? throw new ArgumentException("Valor fixo do proprietário é obrigatório para comissão do tipo Fixo.")),
            TipoComissaoConsignacao.Porcentagem => ComissaoConsignacao.CriarPorcentagem(
                dto.ValorVendaEsperado,
                dto.PorcentagemProprietario ?? throw new ArgumentException("Porcentagem do proprietário é obrigatória para comissão do tipo Porcentagem.")),
            _ => throw new ArgumentException($"Tipo de comissão inválido: {dto.TipoComissao}")
        };

        return new VeiculoConsignacao(
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
            dto.ClienteProprietarioId,
            dto.VendedorResponsavelId,
            comissao,
            dto.TextoContrato,
            dto.UrlContratoPdf,
            dto.PrazoDias,
            ConverterAcessorios(dto.Acessorios)
        );
    }

    public static void UpdateEntity(VeiculoConsignacao entity, AtualizarVeiculoConsignacaoDTO dto)
    {
        if (dto.TextoContrato is not null || dto.UrlContratoPdf is not null)
            entity.AtualizarContrato(dto.TextoContrato ?? entity.TextoContrato, dto.UrlContratoPdf ?? entity.UrlContratoPdf);

        if (dto.VendedorResponsavelId is Guid novoVendedorId)
            entity.AlterarVendedorResponsavel(novoVendedorId);
    }

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
