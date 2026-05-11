using CarStoreManager.Domain.Exceptions;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Domain.ValueObjects;

/*
    Este arquivo implementa os testes automaticos da classe PlacaVeiculo.cs
    um VO do sistema, que inclui regra internas de funcionamento.

    Testes automaticos implementados para:
        Criar placas validas nos padroes permitidos,
        Bloquear placas fora dos padroes permitidos,
        Verificar o metodo de normalização
*/


public class PlacaVeiculoTest
{
    /*
        O proximo teste verifica a criação de placas que devem ser aceitas
        - padrao brasileiro antigo com " - "
        - padrao brasileiro antigo sem " - "
        - padrao brasileiro mercosul
    */
    [Theory]
    [InlineData("ABC-1234")]
    [InlineData("ABC1234")]
    [InlineData("ABC1D23")]
    [InlineData("ABC-1D23")]    // Mercosul COM hífen — regressão (antes só aceitava sem)
    [InlineData("abc1d23")]     // case-insensitive — normalização
    public void Deve_Criar_Placa_Valida(string numero)
    {
        // cenario
        var placa = new PlacaVeiculo(numero);

        //aplica
        bool validaPlaca = placa.ValidaPlaca(placa.GetPlaca());

        // valida
        Assert.True(validaPlaca);
    }

    /*
        O proximo teste verifica a criação de placas que devem ser bloqueadas
        Temos algumas sintaxes erradas, fora dos padrões de regex permitidos
        e tambem placas vazias - todos devem ser bloqueados
    */
    [Theory]
    [InlineData("1255AADB")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("ABC-12-3D4")]
    [InlineData("ABC1DBB")]
    public void Deve_Bloquear_Placa_Invalida(string numero)
    {
        Assert.Throws<PlacaVeiculoInvalidaException>(() => new PlacaVeiculo(numero));
    }

    /*
        A normalização só faz diferenca nessa placa do padrao antigo
        pois caso o usuario insira a placa de forma que nao faz sentido,
        como "A B C - 1234" ou outras variações, ela vai ser barrada pela
        validação com regex que tem dentro do construtor, entao a placa
        nem chegará a ser criada. O mesmo vale pro regex do mercosul.
    */
    [Fact]
    public void Deve_Nomalizar_Placa()
    {
        // cenario
        var placa = new PlacaVeiculo("ABC-1234");

        //aplica
        string valorPlaca = placa.GetPlaca();

        // valida
        Assert.True(valorPlaca == "ABC1234");
    }

}