namespace CarStoreManager.Domain.Exceptions;

public class CodigoOEMInvalidoException : DomainException
{
    public CodigoOEMInvalidoException(string codigo)
        : base($"Código OEM '{codigo}' inválido. Deve ter até 50 caracteres alfanuméricos.")
    {
    }
}
