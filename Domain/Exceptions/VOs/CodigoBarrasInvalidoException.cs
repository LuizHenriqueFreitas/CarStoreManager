namespace CarStoreManager.Domain.Exceptions;

public class CodigoBarrasInvalidoException : DomainException
{
    public CodigoBarrasInvalidoException(string codigo)
        : base($"Código de barras '{codigo}' inválido. Deve conter entre 8 e 14 dígitos numéricos (GTIN/EAN).")
    {
    }
}
