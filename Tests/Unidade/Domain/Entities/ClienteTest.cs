using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Exceptions;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades;

/*
    Este arquivo implementa os testes automaticos da classe Usuario.cs
    um VO do sistema, que inclui regra internas de funcionamento.

    Esta classe tem testes automaticos implementados para:
        Criar um cliente valido,
        Bloquear criação de cliente com nome invalido,
        Bloquear criação de cliente com email invalido,
        Bloquear criação de cliente com telefone invalido,
        Bloquear criação de cliente com cpf invalido,
        Verificar o metodo de atualização dos dados do cliente.
*/

public class ClienteTest
{
    //teste verifica criação de cliente valido
    [Fact]
    public void Deve_Criar_Cliente_Valido()
    {
        // cenario
        var user = new Cliente(
            "Pedro Silveira",
            "pedro@email.com",
            "11 98556-7124",
            "529.982.247-25"
        );
        // validação
        Assert.Equal("Pedro Silveira", user.GetNome());
        Assert.Equal("pedro@email.com", user.GetEmail());
        Assert.Equal("(11) 98556-7124", user.GetTelefone());
        Assert.Equal("529.982.247-25", user.GetCpf());
    }

    //teste verifica que deve bloquear nomes invalidos
    [Fact]
    public void Deve_Bloquear_Cliente_Nome_Invalido()
    {
        // validação
        Assert.Throws<ArgumentException>(() => 
            new Cliente(
            "   ",
            "pedro@email.com",
            "11 98556-7124",
            "529.982.247-25"
        ));
    }

    //teste verifica que deve bloquear emails invalidos
    [Theory]
    [InlineData("teste")]
    [InlineData("teste@")]
    [InlineData("@email.com")]
    [InlineData("teste@email")]
    [InlineData("teste@.com")]
    public void Deve_Bloquear_Cliente_Email_Invalido(string email)
    {
        // validação
        Assert.Throws<ArgumentException>(() => 
            new Cliente(
            "Pedro Silveira",
            email,
            "11 98556-7124",
            "529.982.247-25"
        ));
    }

    //teste verifica que deve bloquear telefones invalidos
    [Theory]
    [InlineData("+55 (11) 99746-2324")]
    [InlineData("11 99746 23246")]
    [InlineData("11 9974-324")]
    [InlineData("")]
    public void Deve_Bloquear_Cliente_Telefone_Invalido(string numero)
    {
        // validação
        Assert.Throws<ArgumentException>(() => 
            new Cliente(
            "Pedro Silveira",
            "pedro@email.com",
            numero,
            "529.982.247-25"
        ));
    }

    //teste verifica que deve bloquear cpfs invalidos
    [Theory]
    [InlineData("5299822472")]
    [InlineData("529982247258")]
    [InlineData("111.111.111-11")]
    [InlineData("52998224745")]
    [InlineData("52998224721")]
    [InlineData("")]
    public void Deve_Bloquear_Cliente_Cpf_Invalido(string cpf)
    {
        // validação
        Assert.Throws<CpfInvalidoException>(() => 
            new Cliente(
            "Pedro Silveira",
            "pedro@email.com",
            "11 98556-7124",
            cpf
        ));
    }

    /*
        teste que verifica o funcionamento do metodo
        de atualização dos dados do cliente.

        Cpf não é atualizavel, essa é uma regra de negocio.
    */
    [Fact]
    public void Deve_Atualizar_Dados_Cliente()
    {
        // cenario
        var user = new Cliente(
            "Pedro Silveira",
            "pedro@email.com",
            "11 98556-7124",
            "529.982.247-25"
        );
        // aplicação
        user.AtualizarClienteDados("Pedro Silva", "silva@email.com", "34282657799");
        // validação
        Assert.Equal("Pedro Silva", user.GetNome());
        Assert.Equal("silva@email.com", user.GetEmail());
        Assert.Equal("(34) 28265-7799", user.GetTelefone());
        Assert.Equal("529.982.247-25", user.GetCpf());
    }
}