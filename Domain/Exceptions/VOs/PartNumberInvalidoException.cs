namespace CarStoreManager.Domain.Exceptions;

public class PartNumberInvalidoException : DomainException
{
    public PartNumberInvalidoException(string partNumber)
        : base($"Part Number '{partNumber}' inválido. Deve ter 1 a 50 caracteres alfanuméricos (com -, /, ., _).")
    {
    }
}
