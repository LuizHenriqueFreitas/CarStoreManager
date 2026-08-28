using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.ValueObjects;

/// <summary>
/// VO que encapsula o arranjo de comissão de uma consignação: ou um valor
/// fixo pago ao proprietário, ou uma porcentagem (0-100) do valor de venda.
/// </summary>
public class ComissaoConsignacao
{
    public TipoComissaoConsignacao Tipo { get; private set; }
    public Dinheiro ValorVendaEsperado { get; private set; } = null!;
    public Dinheiro? ValorFixoProprietario { get; private set; }
    public Percentual? PorcentagemProprietario { get; private set; }

    protected ComissaoConsignacao() { }

    private ComissaoConsignacao(
        TipoComissaoConsignacao tipo,
        decimal valorVendaEsperado,
        decimal? valorFixoProprietario,
        decimal? porcentagemProprietario)
    {
        if (valorVendaEsperado <= 0)
            throw new ArgumentException("Valor de venda esperado deve ser maior que 0.", nameof(valorVendaEsperado));

        Tipo = tipo;
        ValorVendaEsperado = new Dinheiro(valorVendaEsperado);
        ValorFixoProprietario = valorFixoProprietario is decimal vf ? new Dinheiro(vf) : null;
        PorcentagemProprietario = porcentagemProprietario is decimal pp ? new Percentual(pp) : null;
    }

    public static ComissaoConsignacao CriarFixo(decimal valorVendaEsperado, decimal valorFixoProprietario)
    {
        if (valorFixoProprietario <= 0)
            throw new ArgumentException("Valor fixo do proprietário deve ser maior que 0.", nameof(valorFixoProprietario));

        return new ComissaoConsignacao(TipoComissaoConsignacao.Fixo, valorVendaEsperado, valorFixoProprietario, null);
    }

    public static ComissaoConsignacao CriarPorcentagem(decimal valorVendaEsperado, decimal porcentagemProprietario)
    {
        if (porcentagemProprietario <= 0 || porcentagemProprietario > 100)
            throw new ArgumentException("Porcentagem do proprietário deve estar entre 0 e 100.", nameof(porcentagemProprietario));

        return new ComissaoConsignacao(TipoComissaoConsignacao.Porcentagem, valorVendaEsperado, null, porcentagemProprietario);
    }

    /// <summary>Calcula quanto o proprietário recebe dado o valor efetivo de venda.</summary>
    public Dinheiro CalcularPagamentoPropietario(Dinheiro valorVenda) => Tipo switch
    {
        TipoComissaoConsignacao.Fixo => ValorFixoProprietario!,
        TipoComissaoConsignacao.Porcentagem => PorcentagemProprietario!.CalcularDescontoValor(valorVenda),
        _ => throw new InvalidOperationException("Tipo de comissão inválido.")
    };
}
