using CarStoreManager.Domain.Exceptions;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Domain.ValueObjects;

/*
    Este arquivo implementa os testes automaticos da classe Cpf.cs
    um VO do sistema, que inclui regra internas de funcionamento.

    testes implementados fazem:
        Verificar um CFP valido,
        Bloquear um CPF vazio,
        Bloquear um CPF com menos de 11 caracteres,
        Bloquear um CPF com mais de 11 caracteres,
        Bloquear um CPF com todos os numeros iguais,
        Bloquear um CPF com digito verificador 1 invalido,
        Bloquear um CPF com digito verificador 2 invalido,
        Verificar a remoção de formatação - importante para o processo de verificar validade do cpf,
        Verificar a re-formatação - importante para exibição.
*/

public class CpfTest
{
    [Fact]
    public void Deve_Criar_Cpf_Valido()
    {
        //cenario
        var cpf = new Cpf("529.982.247-25");

        //aplicação
        bool cpfValido = cpf.Validar(cpf.GetNumeroCpf());

        //validação
        Assert.True(cpfValido);
        Assert.True(cpf.GetNumeroCpf() == "52998224725");
    }

    /*
        No próprio metodo construtor do cpf, ele chama 
        a função de validação, sendo assim, nos testes abaixo
        a criação do cenario, de criar um ano já é mandada como
        parametro para o assert que deve receber verifica a 
        mensagem de erro presente na exception "CpfInvalidoException"
    */

    [Fact]
    public void Deve_Bloquear_Cpf_Invalido_Vazio()
    {
        Assert.Throws<CpfInvalidoException>(() => new Cpf(""));
    }

    [Fact]
    public void Deve_Bloquear_Cpf_Invalido_Menos_de_11_Digitos()
    {
        Assert.Throws<CpfInvalidoException>(() => new Cpf("5299822472"));
    }

    [Fact]
    public void Deve_Bloquear_Cpf_Invalido_Mais_de_11_Digitos()
    {
        Assert.Throws<CpfInvalidoException>(() => new Cpf("529982247258"));
    }

    [Fact]
    public void Deve_Bloquear_Cpf_Invalido_Numeros_Iguais()
    {
        Assert.Throws<CpfInvalidoException>(() => new Cpf("111.111.111-11"));
    }
    
    [Fact]
    public void Deve_Bloquear_Cpf_Invalido_Digito_Verificador_1_Invalido()
    {
        Assert.Throws<CpfInvalidoException>(() => new Cpf("52998224745"));
    }

    [Fact]
    public void Deve_Bloquear_Cpf_Invalido_Digito_Verificador_2_Invalido()
    {
        Assert.Throws<CpfInvalidoException>(() => new Cpf("52998224721"));
    }

    //verifica se a formatação é removida corretamente
    [Fact]
    public void Deve_Remover_Formatacao_Cpf()
    {
        var cpf = new Cpf("529.982.247-25");

        Assert.True(cpf.GetNumeroCpf() == "52998224725");
    }

    //verifica se a formatação é colocada corretamente
    [Fact]
    public void Deve_Formatar_Cpf_Corretamente()
    {
        var cpf = new Cpf("52998224725");

        Assert.True(cpf.ToString() == "529.982.247-25");
    }
}
