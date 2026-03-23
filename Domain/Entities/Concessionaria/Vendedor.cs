//acrescentar metodos para calculo de comissao, verificar se esta habilitado a vender e mesmo total de vendas

using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Concessionaria;

public class Vendedor : Entity
{
    public string Nome { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public Telefone Telefone { get; private set; } = null!;

    public bool Ativo { get; private set; }

    protected Vendedor() { } // EF

    public Vendedor(string nome, Email email, Telefone telefone)
    {
        DefinirNome(nome);
        Email = email;
        Telefone = telefone;
        Ativo = true;
    }

    // =========================
    // MÉTODOS DE NEGÓCIO
    // =========================

    public void DefinirNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome inválido");

        Nome = nome.Trim();
    }

    public void AtualizarEmail(Email email)
    {
        Email = email;
    }

    public void AtualizarTelefone(Telefone telefone)
    {
        Telefone = telefone;
    }

    public void Desativar()
    {
        Ativo = false;
    }

    public void Ativar()
    {
        Ativo = true;
    }

    public void AtualizarDados(string nome, Email email, Telefone telefone)
    {
        DefinirNome(nome);
        AtualizarEmail(email);
        AtualizarTelefone(telefone);
    }
}