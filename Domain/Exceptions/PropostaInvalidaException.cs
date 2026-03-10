namespace Domain.Exceptions;

public class PropostaInvalidaException : DomainException
{
    public PropostaInvalidaException(string mensagem)
        : base(mensagem)
    {
    }
}