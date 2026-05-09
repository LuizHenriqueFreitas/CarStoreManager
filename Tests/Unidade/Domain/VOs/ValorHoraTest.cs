using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Domain.ValueObjects;

/*
    Este arquivo implementa os testes automaticos da classe ValorHora.cs
    um VO do sistema, que inclui regra internas de funcionamento.

    Testes automaticos implementados para:
        Criar ValorHora valido,
        Bloquear ValorHora invalido,
        Atualizar ValorHora valido,
        Bloquear atualização com valor invalido,
        Veficar o calculo de horas trabalhadas * ValorHora,
        Verifivar a formatação de saida com o metodo ToString()
*/


public class ValorHoraTest
{
    //cria valorHora valido
    [Fact]
    public void Deve_Criar_ValorHora_Valido()
    {
        // cenario
        var vHora = new ValorHora(15);
        // validação
        Assert.True(vHora.GetValorHora() == new Dinheiro(15));
    }

    //bloqueia valorHora invalido
    [Fact]
    public void Deve_Bloquear_ValorHora_Inalido()
    {    
        // validação
        Assert.Throws<ArgumentException>(() => new ValorHora(0));
    }

    //permite atualizar valorHora valido
    [Fact]
    public void Deve_Atualizar_ValorHora_Valido()
    {
        // cenario
        var vHora = new ValorHora(15);
        // aplicação
        vHora.Atualizar(20);
        // validação
        Assert.True(vHora.GetValorHora() == new Dinheiro(20));
    }

    //bloqueia atualizar valorHora invalido
    [Fact]
    public void Deve_Bloquear_Atualizar_ValorHora_Invalido()
    {
        // cenario
        var vHora = new ValorHora(15);
        // validação
        Assert.Throws<ArgumentException>(() => vHora.Atualizar(0));
    }

    /*
        verifica se o calculo de mao de obra esta correto
        valor hora * horas do servico
    */
    [Fact]
    public void Deve_Calcular_Servico_ValorHora()
    {
        // cenario
        var vHora = new ValorHora(15);
        // aplicação
        var maoDeObra = vHora.CalcularServicoValorHora(2);
        // validação
        Assert.True(maoDeObra == new Dinheiro(30));
    }

    //verifica a saida formatada pelo metodo ToString()
    [Fact]
    public void Valida_Formatacao_ValorHora()
    {
        // cenario
        var vHora = new ValorHora(15);
        // verificação
        Assert.Equal("R$ 15,00", vHora.ToString());
    }
}