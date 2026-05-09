using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Domain.ValueObjects;

/*
    Este arquivo implementa os testes automaticos da classe Telefone.cs
    um VO do sistema, que inclui regra internas de funcionamento.

    Testes automaticos implementados para:
        Criar telefones validos,
        Bloquear telefones invalidos,
        Permitir Atualização de numero valido,
        Bloquear Atualização de numero invalido,
        Verifivar a formatação de saida com o metodo ToString()
*/


public class TelefoneTest
{
    /*
        O teste abaixo envia 3 modelos de fomatação de entrada
        que devem ser aceitos. 
        Com ddd, parenteses e - , ou sem eles.
    */
    [Theory]
    [InlineData("(11) 99746-2324")]
    [InlineData("11 99746 2324")]
    [InlineData("11 99746-2324")]
    [InlineData("11997462324")]

    public void Deve_Criar_Telefone_Valido_Normalizado(string numero)
    {
        // cenario
        var telefone = new Telefone(numero);
        
        // validação
        Assert.Equal("11997462324", telefone.GetTelefone());
    }

    /*
        O teste abaixo envia 3 modelos de fomatação de entrada
        que devem ser bloqueados. 
        Com numeros a mais ou a menos que o limite do padrao brasileiro,
        ou entradas vazias.
    */
    [Theory]
    [InlineData("+55 (11) 99746-2324")]
    [InlineData("11 99746 23246")]
    [InlineData("11 9974-324")]
    [InlineData("")]
    public void Deve_Bloquear_Telefone_Inalido(string numero)
    {
        // validação
        Assert.Throws<ArgumentException>(() => new Telefone(numero));
    }

    /*
        metodo de atualizar um telefone ja existente,
        deve permitir os valores propostos, mesma regra da criação
        de um novo numero
    */
    [Theory]
    [InlineData("(11) 99746-2324")]
    [InlineData("11 99746 2324")]
    [InlineData("11 99746-2324")]
    [InlineData("11997462324")]
    public void Deve_Atualizar_Telefone_Normalizado(string numero)
    {
        // cenario
        var telefone = new Telefone("34282657799");

        // aplicação
        telefone.AtualizarTelefone(numero);

        // validação
        Assert.Equal("11997462324", telefone.GetTelefone());
    }

    /*
        metodo de atualizar um telefone ja existente,
        deve bloquear os valores propostos, mesma regra da criação
        de um novo numero
    */
    [Theory]
    [InlineData("+55 (11) 99746-2324")]
    [InlineData("11 99746 23246")]
    [InlineData("11 9974-324")]
    [InlineData("")]
    public void Deve_Bloquear_Atualizar_Telefone_Inalido(string numero)
    {
        // cenario
        var telefone = new Telefone("34282657799");

        // validação
        Assert.Throws<ArgumentException>(() => telefone.AtualizarTelefone(numero));
    }

    //verificando a saida formatada com o metodo ToString()
    [Fact]
    public void Verifcar_Formatação_Telefone()
    {
        // cenario
        var telefone = new Telefone("11997462324");
        // validação
        Assert.Equal("(11) 99746-2324", telefone.ToString());
    }
}