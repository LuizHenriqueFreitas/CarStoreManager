using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de Cliente.cs

    Esta classe tem testes automaticos implementados para:
        Criar um cliente valido,
        Bloquear criação de cliente com nome invalido,
        Bloquear criação de cliente com email invalido,
        Bloquear criação de cliente com telefone invalido,
        Bloquear criação de cliente com cpf invalido,
        Verificar o metodo de atualização dos dados do cliente.
*/

public class Cliente : Entity
{
    public string Nome { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public Telefone Telefone { get; private set; } = null!;
    public Cpf Cpf { get; private set; } = null!;

    protected Cliente() { }

    public Cliente(string nome, string email, string telefone, string cpf)
    {
        AtualizarClienteNome(nome);
        Email = new Email(email);
        Telefone = new Telefone(telefone);
        Cpf = new Cpf(cpf);
    }

    //metodos getters de cada atributo
    public string GetNome() => Nome.ToString();
    public string GetEmail() => Email.GetEmail();
    public string GetTelefone() => Telefone.ToString();
    public string GetCpf() => Cpf.ToString();

    /*
        Abaixo metodos setters para atualizar
        os valores dos campos
    */
    public void AtualizarClienteNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome inválido");

        Nome = nome;
    }

    public void AtualizarClienteEmail(string email)
    {
        Email.SetEmail(email);
    }

    public void AtualizarClienteTelefone(string telefone)
    {
        Telefone.AtualizarTelefone(telefone);
    }

    /*
        não há um metodo para atualização do cpf
        pois não é comum alguem trocar de cpf. 
        A regra de negócio adotada é que o cpf 
        permanece o mesmo para sempre
    */
    public void AtualizarClienteDados(string nome, string email, string telefone)
    {
        AtualizarClienteNome(nome);
        AtualizarClienteTelefone(telefone);
        AtualizarClienteEmail(email);
    }
}