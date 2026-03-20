namespace CarStoreManager.Domain.Exceptions;

public class EstoqueInsuficienteException : DomainException
{
    public EstoqueInsuficienteException(string nomePeca)
        : base($"Estoque insuficiente para a peça: {nomePeca}.")
    {
    }
}