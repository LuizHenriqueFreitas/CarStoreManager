using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Domain.ValueObjects;

/*
    Este arquivo implementa os testes automaticos da classe Dinheiro.cs
    um VO do sistema, que inclui regra internas de funcionamento.

    Testes automaticos implementados para:
        Validar criação de dinheiro correto,
        Arredondar valor para 2 casas decimais,
        Bloquear criação de dinhiero negativo,
        Validar operação de soma pelo metodo Somar(),
        Validar operação de soma pelo operador "+",
        Validar operação de subtração pelo metodo Subtrair(),
        Validar operação de subtração pelo operador "-",
        Bloquear resultado negativo no meotodo Subtrair(),
        Validar operação de multiplicação pelo metodo Multiplicar(),
        Validar operação de multiplicação pelo operador "*",
        Bloquear resultado negativo no meotodo Multiplicar(),
        Validar operação de dividisão pelo metodo Dividir(),
        Validar operação de dividisão pelo operador "/",
        Bloquear divirsor zero,
        Bloquear divirsor negativo,
        Validar operação com sinal de "<",
        Validar operação com sinal de ">",
        Validar operação de igualdade / desigualdade,
        Verificar a formatação de saida.


    Não estao implemenados - mas acredito que faça pouca diferença:
        metodos para bloquear resultados negativos em soma e divisao, tanto por metodo quanto por sinal
*/

public class DinheiroTest
{
    //verifica a criação de um dinheiro valido
    [Fact]
    public void Deve_Criar_Dinheiro_Valido()
    {
        var dinheiro = new Dinheiro(10.50m);

        Assert.True(dinheiro.GetValorDinheiro() == 10.50m);
    }
    
    //verifica o arredondamento em 2 casas decimais ao criar um dinheiro
    [Fact]
    public void Deve_Arredondar_Para_Duas_Casas()
    {
        var dinheiro = new Dinheiro(10.555m);

        Assert.True(dinheiro.GetValorDinheiro() == 10.56m);
    }

    //dinheiros negativos nao podem ser criados
    [Fact]
    public void Deve_Bloquear_Valor_Negativo()
    {
        Assert.Throws<ArgumentException>(() => new Dinheiro(-1));
    }

    //testa o metodo de soma do dinheiro
    [Fact]
    public void Deve_Somar()
    {
        var resultado = new Dinheiro(10).Somar(new Dinheiro(5));

        Assert.True(resultado.GetValorDinheiro() == 15);
    }

    //testa a soma de dinheiros com sinal +
    [Fact]
    public void Deve_Somar_Usando_Operador()
    {
        var resultado = new Dinheiro(10) + new Dinheiro(5);

        Assert.True(resultado.GetValorDinheiro() == 15);
    }

    //testa o metodo de subtracao do dinheiro
    [Fact]
    public void Deve_Subtrair()
    {
        var resultado = new Dinheiro(10).Subtrair(new Dinheiro(5));

        Assert.True(resultado.GetValorDinheiro() == 5);
    }

    //testa a subtracao de dinheiros com sinal -
    [Fact]
    public void Deve_Subtrair_Usando_Operador()
    {
        var resultado = new Dinheiro(10) - new Dinheiro(5);

        Assert.True(resultado.GetValorDinheiro() == 5);
    }

    //subtrações não podem ter resultados negativos
    [Fact]
    public void Deve_Bloquear_Resultado_Negativo_Subtracao()
    {
        Assert.Throws<ArgumentException>(() => new Dinheiro(5).Subtrair(new Dinheiro(10)));
    }

    //testa o metodo de multiplicação do dinheiro por um valor inteiro
    [Fact]
    public void Deve_Multiplicar()
    {
        var resultado = new Dinheiro(10).Multiplicar(3);

        Assert.True(resultado.GetValorDinheiro() == 30);
    }

    //testa a multiplicação de dinheiro por um valor interio com sinal *
    [Fact]
    public void Deve_Multiplicar_Usando_Operador()
    {
        var resultado = new Dinheiro(10) * 5;

        Assert.True(resultado.GetValorDinheiro() == 50);
    }

    //multiplicações não podem ter resultados negativos
    [Fact]
    public void Deve_Bloquear_Resultado_Negativo_Multiplicacao()
    {
        Assert.Throws<ArgumentException>(() => new Dinheiro(10).Multiplicar(-1));
    }

    //testa o metodo de divisão do dinheiro por um valor inteiro
    [Fact]
    public void Deve_Dividir()
    {
        var resultado = new Dinheiro(10).Dividir(2);

        Assert.True(resultado.GetValorDinheiro() == 5);
    }

    //testa a multiplicação de dinheiro por um valor interio com sinal /
    [Fact]
    public void Deve_Dividir_Usando_Operador()
    {
        var resultado = new Dinheiro(10) / 5;

        Assert.True(resultado.GetValorDinheiro() == 2);
    }

    //Não é permitido divisões por 0
    [Fact]
    public void Deve_Bloquear_Divisor_Zero()
    {
        Assert.Throws<ArgumentException>(() => new Dinheiro(10).Dividir(0));
    }
    
    //Não é permitido divisão por numeros negativos
    [Fact]
    public void Deve_Bloquear_Divisor_Negativo()
    {
        Assert.Throws<ArgumentException>(() => new Dinheiro(10).Dividir(-1));
    }

    /*
        abaixo seguem testes para os metodos de operadores logicos
        entre 2 objetos do tipo dinheiro. Em ordem:
        - metodo de maior que,
        - metodo de menor que,
        - verifica detectar igualdade,
        - verifica detectar diferença
    */

    [Fact]
    public void Deve_Ser_Maior_Que()
    {
        var a = new Dinheiro(10);
        var b = new Dinheiro(5);

        Assert.True(a.MaiorQue(b));
    }

    [Fact]
    public void Deve_Ser_Menor_Que()
    {
        var a = new Dinheiro(5);
        var b = new Dinheiro(10);

        Assert.True(a.MenorQue(b));
    }

    [Fact]
    public void Deve_Ser_Igual()
    {
        var a = new Dinheiro(10);
        var b = new Dinheiro(10);

        Assert.True(a.Igual(b));
    }

    [Fact]
    public void Deve_Diferenciar_Valores()
    {
        var a = new Dinheiro(10);
        var b = new Dinheiro(5);

        Assert.False(a.Igual(b));
    }

    //verifica formatação de saida
    [Fact]
    public void Deve_Formatar_Corretamente()
    {
        var dinheiro = new Dinheiro(10);

        Assert.True(dinheiro.ToString() == "R$ 10,00");
    }
}