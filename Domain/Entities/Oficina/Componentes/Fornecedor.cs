using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Oficina;

public class Fornecedor : Entity
{
    public string RazaoSocial { get; private set; } = null!;

    public string NomeFantasia { get; private set; } = null!;

    public Cnpj Cnpj { get; private set; } = null!;

    public Email Email { get; private set; } = null!;

    public Telefone Telefone { get; private set; } = null!;

    public bool Ativo { get; private set; } = true;

    protected Fornecedor() { }

    public Fornecedor(
        string razaoSocial,
        string nomeFantasia,
        string cnpj,
        string email,
        string telefone)
    {
        SetRazaoSocial(razaoSocial);
        SetNomeFantasia(nomeFantasia);
        Cnpj = new Cnpj(cnpj);
        Email = new Email(email);
        Telefone = new Telefone(telefone);
    }

    public void SetRazaoSocial(string razaoSocial)
    {
        if (string.IsNullOrWhiteSpace(razaoSocial))
            throw new ArgumentException("Razão social não pode ser vazia.", nameof(razaoSocial));
        if (razaoSocial.Length > 200)
            throw new ArgumentException("Razão social não pode ter mais de 200 caracteres.", nameof(razaoSocial));
        RazaoSocial = razaoSocial.Trim();
    }

    public void SetNomeFantasia(string nomeFantasia)
    {
        if (string.IsNullOrWhiteSpace(nomeFantasia))
            throw new ArgumentException("Nome fantasia não pode ser vazio.", nameof(nomeFantasia));
        if (nomeFantasia.Length > 200)
            throw new ArgumentException("Nome fantasia não pode ter mais de 200 caracteres.", nameof(nomeFantasia));
        NomeFantasia = nomeFantasia.Trim();
    }

    public void Ativar() => Ativo = true;
    public void Desativar() => Ativo = false;
}
