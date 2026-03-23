namespace CarStoreManager.Domain.ValueObjects;

public class Parcelas
{
    public int Quantidade { get; }

    public Parcelas(int quantidade)
    {
        if (quantidade <= 0 || quantidade > 72)
            throw new ArgumentException("Parcelas inválidas");

        Quantidade = quantidade;
    }
}