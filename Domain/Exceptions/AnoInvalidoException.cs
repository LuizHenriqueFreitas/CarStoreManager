namespace CarStoreManager.Domain.Exceptions;

public class AnoInvalidoException : DomainException
{
    public AnoInvalidoException(int ano)
        : base($"O ano: {ano} NÃO PODE SER REGISTRADO.")
    {
    }
}