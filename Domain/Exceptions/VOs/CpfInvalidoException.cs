namespace CarStoreManager.Domain.Exceptions;

public class CpfInvalidoException : DomainException
{
    public CpfInvalidoException(string cpf)
        : base($"O CPF: {cpf} NÃO PODE SER REGISTRADO - INVALIDO.")
    {
    }
}