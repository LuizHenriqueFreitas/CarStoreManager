using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities;

public class Cliente : Entity
{
    public string Nome { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public Telefone Telefone { get; private set; } = null!;
    public Cpf CPF { get; private set; } = null!;

    protected Cliente() { } // EF

    public Cliente(string nome, Cpf cpf, Telefone telefone, Email email)
    {
        DefinirNome(nome);
        CPF = cpf; // CPF normalmente não muda
        Telefone = telefone;
        Email = email;
    }

    // =========================
    // GETERS
    // =========================

    public string GetEmail()
    {
        return Email.ToString();
    }

    public string GetTelefone()
    {
        return Telefone.ToString();
    }

    public string GetCpf()
    {
        return CPF.ToString();
    }

    // =========================
    // MÉTODOS DE NEGÓCIO
    // =========================

    public void DefinirNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome inválido");

        Nome = nome;
    }

    public void AtualizarEmail(Email email)
    {
        Email = email;
    }

    public void AtualizarTelefone(Telefone telefone)
    {
        Telefone = telefone;
    }

    public void AtualizarDados(string nome, Telefone telefone, Email email)
    {
        DefinirNome(nome);
        AtualizarTelefone(telefone);
        AtualizarEmail(email);
    }
}