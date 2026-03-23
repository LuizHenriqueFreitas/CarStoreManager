namespace CarStoreManager.Domain.ValueObjects;

public class ValorHora
{
    public decimal Valor { get; }

    protected ValorHora() { } // EF

    public ValorHora(decimal valor)
    {
        if (valor <= 0)
            throw new ArgumentException("Valor por hora deve ser maior que zero");

        Valor = decimal.Round(valor, 2);
    }

    // =========================
    // COMPORTAMENTO
    // =========================

    public Dinheiro Calcular(int horas)
    {
        if (horas <= 0)
            throw new ArgumentException("Horas inválidas");

        return new Dinheiro(Valor * horas);
    }

    public ValorHora Atualizar(decimal novoValor)
    {
        return new ValorHora(novoValor);
    }

    // =========================
    // OPERADORES (opcional, mas top)
    // =========================

    public static implicit operator decimal(ValorHora v)
        => v.Valor;

    public override string ToString()
        => $"R$ {Valor:F2}";
}