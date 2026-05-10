using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.ValueObjects;

namespace Oficina.Domain.Entities;

public class Fornecedor : Entity
{
    public string RazaoSocial { get; private set; } = null!;

    public string NomeFantasia { get; private set; } = null!;

    public string Cnpj { get; private set; } = null!; // trocar por VO

    public Email Email { get; private set; } = null!;

    public Telefone Telefone { get; private set; } = null!;

    public bool Ativo { get; private set; } = true;

    public DateTime Datacriação => DateTime.Today;

    protected Fornecedor() {}

    public Fornecedor(
        string razaoSocial,
        string nomeFantasia,
        string cnpj,
        string email,
        string telefone)
    {
        RazaoSocial = razaoSocial;
        NomeFantasia = nomeFantasia;
        Cnpj = cnpj;
        Email = new Email(email);
        Telefone = new Telefone(telefone);
    }
}