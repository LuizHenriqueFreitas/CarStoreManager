using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Domain.ValueObjects;

/*
    Este arquivo implementa os testes automaticos da classe Parcelas.cs
    um VO do sistema, que inclui regra internas de funcionamento.

    Testes automaticos implementados para:
        Criar quantidade de parcelas valida,
        Bloquear quantidade de parcelas invalida
*/

public class ParcelasTest
{

    /*
        Abaixo temos em ordem:
        -Teste de limites permitidos 1 até 72
        -Teste de limites bloqueados <= 0 ou >= 73
    */
    [Theory]
    [InlineData(1)]
    [InlineData(72)]
    public void Criar_Quantidade_Parcela_Valida(int quantidade)
    {
        // cenario
        var parcela = new Parcelas(quantidade);
        // validação
        Assert.True(parcela.GetParcelasQuantidade() == quantidade);
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(73)]
    public void Bloquear_Quantidade_Parcela_Invalida(int quantidade)
    {
        Assert.Throws<ArgumentException>(() =>  new Parcelas(quantidade));
    }
}