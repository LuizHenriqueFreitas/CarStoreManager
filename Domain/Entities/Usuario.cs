/* 
    classe base de usuarios, 
    usada por vendedor, mecanico e admin
*/

using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de Usuario.cs, que serve de base para
    os 3 tipos de usuarios ativos do sistema.

    Esta classe tem testes automaticos implementados para:
        Criar um usuario valido com todos os roles,
        Bloquear criação de um usuario com nome invalido,
        Bloquear criação de um usuario com email invalido,
        Bloquear criação de um usuario com telefone invalido,
        Verifica a atualização de dados pessoais,
        Verifica a atualização de senha,
        Verifica a função de Desativar Usuario,
        Verifica a função de Reativar Usuario
*/

public class Usuario : Entity
{
    public string Nome { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public Telefone Telefone { get; private set; } = null!;
    public Senha Senha { get; private set; } = null!;
    public Dinheiro Salario { get; private set; } = null!;
    public RoleUsuario Role { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCriacao { get; private set; }

    protected Usuario() { }

    /*
        Em seguida pode-se ver o construtor do usuario que recebe
        como parametro os dados em tipos primitivos em sua maioria
        e são convertidos para dados mais complexos e validados,
        como Email, Telefone etc.
    */
    public Usuario(
        string nome,
        string email,
        string telefone,
        string senha,
        decimal salario,
        RoleUsuario role)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome inválido");

        Nome = nome;
        Email = new Email(email);
        Telefone = new Telefone(telefone);
        Senha = new Senha(senha);
        Salario = new Dinheiro(salario);

        Role = role;
        Ativo = true;
        DataCriacao = DateTime.UtcNow;
    }

    /*
        Abaixo estão em sequencia todos os getters
        eles estao utilizando a sintaxe mais compacta.
    */
    public string GetNome() => Nome;
    public string GetEmail() => Email.GetEmail();
    public string GetTelefone() => Telefone.ToString();
    public string GetSenhaHash() => Senha.GetSenhaHash();
    public Senha GetSenha() => Senha;
    public decimal GetSalario() => Salario.GetValorDinheiro();
    public string GetRole() => Role.ToString();
    public bool GetAtivo() => Ativo;
    public DateTime GetDataCriacao() => DataCriacao;

    /*
        Abaixo estão também os Setters 
        com aplicação das regras de negócios
        para cada atributo da classe
    */
    public void AtualizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome inválido");
        
        Nome = nome.Trim();
    }

    public void AtualizarEmail(string email)
        => Email.SetEmail(email);

    public void AtualizarTelefone(string telefone)
        => Telefone.AtualizarTelefone(telefone);

    public void AtualizarSenha(string novaSenha)
        => Senha.AtualizarSenha(novaSenha);

    //metodo para atualizar o salario do funcionario
    public void AtualizarSalario(decimal novoSalario)
        => Salario.SetValorDinheiro(new Dinheiro(novoSalario));

    //talvez esse codigo nao seja acessado, mas ja foi implementado caso necessite
    public void AtualizarRole(string novaSenha)
        => Senha.AtualizarSenha(novaSenha);

    public void AtualizarDadosPessoais(string nome, string email, string telefone)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome inválido");
        
        AtualizarNome(nome);
        AtualizarEmail(email);
        AtualizarTelefone(telefone);
    }

    //Abaixo funções para desativar e reativar o usuário
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