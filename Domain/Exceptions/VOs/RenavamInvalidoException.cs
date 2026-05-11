namespace CarStoreManager.Domain.Exceptions;

public class RenavamInvalidoException : DomainException
{
    public RenavamInvalidoException(string renavam)
        : base($"RENAVAM '{renavam}' inválido — deve conter 9 a 11 dígitos numéricos com dígito verificador correto.")
    {
    }
}
