namespace CarStoreManager.Domain.Exceptions;

public class CnpjInvalidoException : DomainException
{
    public CnpjInvalidoException(string cnpj)
        : base($"CNPJ '{cnpj}' inválido — verifique formato (14 dígitos) e dígitos verificadores.")
    {
    }
}
