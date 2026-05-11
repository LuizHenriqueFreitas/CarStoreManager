namespace CarStoreManager.Domain.Exceptions;

public class NcmInvalidoException : DomainException
{
    public NcmInvalidoException(string ncm)
        : base($"NCM '{ncm}' inválido. Deve conter exatamente 8 dígitos numéricos.")
    {
    }
}
