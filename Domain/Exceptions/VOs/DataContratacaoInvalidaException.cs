namespace CarStoreManager.Domain.Exceptions;

public class DataContratacaoInvalidoException : DomainException
{
    public DataContratacaoInvalidoException(DateTime data)
        : base($"A data: {data} NÃO PODE SER REGISTRADO - INVALIDO.\n Você pode rgistrar um funcionarios em até 30 dias apartir de hoje.")
    {
    }
}