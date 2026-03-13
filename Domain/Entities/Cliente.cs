namespace CarStoreManager.Domain.Entities;

public class Cliente
{
    public Guid Id { get; private set; }

    public string Nome { get; private set; } = null!;

    public string Documento { get; private set; } = null!;
    public string Telefone { get; private set; } = null!;

    public string Email { get; private set; }= null!;

    public Cliente(string nome, string documento, string telefone, string email)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Documento = documento;
        Telefone = telefone;
        Email = email;
    }

    private Cliente() { }

    public void AtualizarDados(string nome, string telefone, string email)
    {
        this.Nome = nome;
        this.Telefone = telefone;
        this.Email = email;
    }
}