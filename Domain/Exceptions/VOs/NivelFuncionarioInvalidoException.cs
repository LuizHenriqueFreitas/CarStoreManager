namespace CarStoreManager.Domain.Exceptions;

public class NivelFuncionarioInvalidoException : DomainException
{
    public NivelFuncionarioInvalidoException(string nivel)
        : base($"O Nivel de experiência: {nivel} NÃO PODE SER REGISTRADO - INVALIDO.")
    {
    }
}