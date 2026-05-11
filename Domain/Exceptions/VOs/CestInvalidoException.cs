namespace CarStoreManager.Domain.Exceptions;

public class CestInvalidoException : DomainException
{
    public CestInvalidoException(string cest)
        : base($"CEST '{cest}' inválido. Deve conter exatamente 7 dígitos numéricos.")
    {
    }
}
