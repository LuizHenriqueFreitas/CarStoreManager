using CarStoreManager.Domain.ValueObjects;
using FluentAssertions;

namespace CarStoreManager.Tests.Unidade.Domain.ValueObjects;

/*
    Este arquivo implementa os testes automaticos da classe Email.cs
    um VO do sistema, que inclui regra internas de funcionamento.

    Testes automaticos implementados para:
        Criar um email valido,
        Verificar a normalização do email,
        Bloquear email vazio,
        Bloquear email com regex errado,
        Verifica que emails sejam independentede to case sensitive,
        Verifica que emails sejam comparaveis com operadores == e !=
        Emails iguais devem ter o mesmo hashcode,
        Verificação do retorno com GetEmail
*/

public class EmailTest
{
    //cria um email valido
    [Fact]
    public void Deve_Criar_Email_Valido()
    {
        var email = new Email("teste@email.com");

        Assert.True(email.GetEmail() == "teste@email.com");
    }

    //verifica que o email seja normalizado
    [Fact]
    public void Deve_Normalizar_Email()
    {
        var email = new Email("  TESTE@Email.COM ");

        Assert.True(email.GetEmail() == "teste@email.com");
    }

    //verifica que emails vazios são aceitos
    [Fact]
    public void Bloquear_Email_Vazio()
    {
        Assert.Throws<ArgumentException>(() => new Email(" "));
    }

    /*
        O teste abaixo executa 5 tentativas, todas devem ser bloqueadas
        cada uma é correspondente a um possivel modo errado de
        digitar um email. 

        O email correto é composto por "algo@outra.coisa" ,
        qualquer coisa fora disso deve ser bloqueada
    */
    [Theory]
    [InlineData("teste")]
    [InlineData("teste@")]
    [InlineData("@email.com")]
    [InlineData("teste@email")]
    [InlineData("teste@.com")]
    public void Deve_Bloquear_Email_Invalido(string valor)
    {
        Action act = () => new Email(valor);

        act.Should().Throw<ArgumentException>()
        .WithMessage("Email inválido");
    }

    /*
        Verifica que independente do case sensitive ao criar,
        todos os emails sao normalizados. se 2 emails tiverem
        as mesmas letras nos mesmos lugares, eles serao iguais
    */
    [Fact]
    public void Deve_Ser_Igual_Independentemente_De_Case()
    {
        var a = new Email("TESTE@email.com");
        var b = new Email("teste@email.com");

        Assert.True(a == b);
    }

    //verifica que seja possivel usar os operadores == e != entre emails
    [Fact]
    public void Deve_Comparar_Com_Operadores()
    {
        var a = new Email("teste@email.com");
        var b = new Email("teste@email.com");

        Assert.True(a == b);
        Assert.False(a != b);
    }

    //verifica que 2 emails iguais tenham o mesmo hashcode
    [Fact]
    public void Deve_Ter_Mesmo_HashCode_Para_Emails_Iguais()
    {
        var a = new Email("teste@email.com");
        var b = new Email("TESTE@email.com");

        Assert.True(a.GetHashCode() == b.GetHashCode());
    }

    //verifica o retorno de GetEmail()
    [Fact]
    public void Deve_Retornar_Endereco()
    {
        var email = new Email("teste@email.com");

        Assert.True(email.GetEmail() == "teste@email.com");
    }
}