namespace Domain.ValueObjects;

public class PlacaVeiculo
{
    public string Numero { get; private set; }

    public PlacaVeiculo(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("Placa não pode ser vazia.");

        Numero = numero.ToUpper();
    }

    public override string ToString()
    {
        return Numero;
    }
}