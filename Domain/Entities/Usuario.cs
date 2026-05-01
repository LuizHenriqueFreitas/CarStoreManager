// classe base de usuarios, usada por vendedor, mecanico e admin

using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities;

public class Usuario : Entity
{
    public string Nome { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public Telefone Telefone { get; private set; } = null!;
    public string SenhaHash { get; private set; } = null!;
    public RoleUsuario Role { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCriacao { get; private set; }

    protected Usuario() { }

    public Usuario(
        string nome,
        Email email,
        Telefone telefone,
        string senhaHash,
        RoleUsuario role)
    {
        Nome = nome;
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Telefone = telefone ?? throw new ArgumentNullException(nameof(telefone));

        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new ArgumentException("Senha inválida");

        SenhaHash = senhaHash;
        Role = role;
        Ativo = true;
        DataCriacao = DateTime.UtcNow;
    }

    // =========================
    // GETTERS
    // =========================

    public string GetNome() => Nome;
    public string GetEmail() => Email.Endereco.ToString();
    public string GetTelefone() => Telefone.Numero.ToString();
    public string GetRole() => Role.ToString();

    // =========================
    // REGRAS DE NEGOCIOS - SETERS
    // =========================

    public void AlterarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome inválido");
        Nome = nome.Trim();
    }

    public void AlterarEmail(Email email)
        => Email = email ?? throw new ArgumentNullException(nameof(email));

    public void AlterarTelefone(Telefone telefone)
        => Telefone = telefone ?? throw new ArgumentNullException(nameof(telefone));

    public void AlterarSenha(string novaSenhaHash)
    {
        if (string.IsNullOrWhiteSpace(novaSenhaHash))
            throw new ArgumentException("Senha inválida");
        SenhaHash = novaSenhaHash;
    }

    public void AtualizarDadosPessoais(string nome, Email email, Telefone telefone)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome inválido");
        Nome = nome.Trim();
        Email = email;
        Telefone = telefone;
    }

    public void Desativar()
    {
        if (!Ativo)
            throw new InvalidOperationException("Usuário já está inativo");
        Ativo = false;
    }

    public void Reativar()
    {
        if (Ativo)
            throw new InvalidOperationException("Usuário já está ativo");
        Ativo = true;
    }
}