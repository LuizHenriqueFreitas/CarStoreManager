namespace CarStoreManager.Domain.ValueObjects;

public class Quilometragem
{
    public int Valor { get; }

    protected Quilometragem() { } // EF

    public Quilometragem(int valor)
    {
        if (valor < 0)
            throw new ArgumentException("Quilometragem não pode ser negativa");

        Valor = valor;
    }

    // =========================
    // REGRAS DE NEGÓCIO
    // =========================

    public Quilometragem Atualizar(int novoValor)
    {
        if (novoValor < Valor)
            throw new InvalidOperationException("Quilometragem não pode diminuir");

        return new Quilometragem(novoValor);
    }

    public Quilometragem Adicionar(int km)
    {
        if (km <= 0)
            throw new ArgumentException("Valor inválido para incremento");

        return new Quilometragem(Valor + km);
    }

    // =========================
    // Equals
    // =========================

    public override bool Equals(object? obj)
    {
        if (obj is not Quilometragem other)
            return false;

        return Valor == other.Valor;
    }

    public override int GetHashCode()
        => Valor.GetHashCode();

    public static bool operator ==(Quilometragem a, Quilometragem b)
        => a?.Valor == b?.Valor;

    public static bool operator !=(Quilometragem a, Quilometragem b)
        => !(a == b);

    public override string ToString()
        => $"{Valor:N0} km";
}