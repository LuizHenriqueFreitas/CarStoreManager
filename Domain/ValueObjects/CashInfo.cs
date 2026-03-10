namespace CarStoreManager.Domain.ValueObjects;

public class CashInfo
{
    public decimal Valor { get; private set; }

    public CashInfo(decimal valor)
    {
        if (valor < 0)
            throw new ArgumentException("Valor monetário não pode ser negativo.");

        this.Valor = valor;
    }

    public static CashInfo operator +(CashInfo a, CashInfo b)
        => new CashInfo(a.Valor + b.Valor);

    public static CashInfo operator -(CashInfo a, CashInfo b)
        => new CashInfo(a.Valor - b.Valor);

    public override string ToString()
    {
        return Valor.ToString("C");
    }
}

