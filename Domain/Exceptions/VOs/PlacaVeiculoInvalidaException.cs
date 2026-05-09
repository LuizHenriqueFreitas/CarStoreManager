namespace CarStoreManager.Domain.Exceptions;

public class PlacaVeiculoInvalidaException : DomainException
{
    public PlacaVeiculoInvalidaException(string placa)
        : base($"A Placa: {placa} Não segue os padroes brasileiros - INVALIDO.")
    {
    }
}