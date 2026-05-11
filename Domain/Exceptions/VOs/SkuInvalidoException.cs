namespace CarStoreManager.Domain.Exceptions;

public class SkuInvalidoException : DomainException
{
    public SkuInvalidoException(string sku)
        : base($"SKU '{sku}' inválido. Deve ter de 1 a 50 caracteres alfanuméricos.")
    {
    }
}
