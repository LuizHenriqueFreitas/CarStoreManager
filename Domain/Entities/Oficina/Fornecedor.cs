using System.Text.RegularExpressions;
using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Oficina;

/*
    Fornecedor de peças/componentes para a oficina. Nome e CNPJ são
    obrigatórios; endereço, email e telefone são opcionais — muitos
    fornecedores só têm um contato comercial (telefone ou email), sem
    endereço cadastrado.
*/
public class Fornecedor : Entity
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public string Nome { get; private set; } = null!;
    public Cnpj Cnpj { get; private set; } = null!;
    public Guid? EnderecoId { get; private set; }
    public Endereco? Endereco { get; private set; }
    public string? Email { get; private set; }
    public string? Telefone { get; private set; }
    public bool Ativo { get; private set; } = true;

    protected Fornecedor() { }

    public Fornecedor(string nome, string cnpj, Endereco? endereco = null, string? email = null, string? telefone = null)
    {
        SetNome(nome);
        Cnpj = new Cnpj(cnpj);
        AtualizarEndereco(endereco);
        AtualizarEmail(email);
        AtualizarTelefone(telefone);
        // Ativo já é true por padrão
    }

    public void SetNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio ou nulo.", nameof(nome));
        if (nome.Length > 200)
            throw new ArgumentException("Nome não pode ter mais de 200 caracteres.", nameof(nome));
        Nome = nome.Trim();
    }

    public void AtualizarEndereco(Endereco? endereco)
    {
        Endereco = endereco;
        EnderecoId = endereco?.Id;
    }

    /// <summary>
    /// Aplica os campos de um endereço novo/atualizado — mutando o endereço
    /// já existente em vez de trocar a referência, pra não deixar uma linha
    /// órfã na tabela Enderecos (mesmo raciocínio de Cliente.AtualizarClienteEndereco).
    /// Sem logradouro informado, mantém o endereço atual como está (a tela de
    /// edição não tem como "limpar" um endereço já cadastrado).
    /// </summary>
    public void AtualizarDadosEndereco(
        string? logradouro,
        string? numero,
        string? complemento,
        string? bairro,
        string? cidade,
        string? uf,
        string? cep)
    {
        if (string.IsNullOrWhiteSpace(logradouro))
            return;

        if (Endereco is null)
        {
            Endereco = new Endereco(logradouro, numero!, complemento, bairro!, cidade!, uf!, cep!);
            EnderecoId = Endereco.Id;
        }
        else
        {
            Endereco.AtualizarDados(logradouro, numero!, complemento, bairro!, cidade!, uf!, cep!);
        }
    }

    public void AtualizarEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) { Email = null; return; }

        var normalizado = email.Trim().ToLowerInvariant();
        if (!EmailRegex.IsMatch(normalizado))
            throw new ArgumentException("Email inválido", nameof(email));

        Email = normalizado;
    }

    public void AtualizarTelefone(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone)) { Telefone = null; return; }

        var digitos = new string(telefone.Where(char.IsDigit).ToArray());
        if (digitos.Length is not (10 or 11))
            throw new ArgumentException("Telefone inválido", nameof(telefone));

        Telefone = digitos;
    }

    /// <summary>Atualiza email, telefone e endereço de uma vez — usado pela tela de gerenciamento de fornecedores.</summary>
    public void AtualizarContato(
        string? email,
        string? telefone,
        string? logradouro,
        string? numero,
        string? complemento,
        string? bairro,
        string? cidade,
        string? uf,
        string? cep)
    {
        AtualizarEmail(email);
        AtualizarTelefone(telefone);
        AtualizarDadosEndereco(logradouro, numero, complemento, bairro, cidade, uf, cep);
    }

    public void Ativar() => Ativo = true;
    public void Desativar() => Ativo = false;
}
