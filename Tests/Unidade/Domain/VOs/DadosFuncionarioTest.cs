using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Exceptions;
using CarStoreManager.Domain.ValueObjects;
using FluentAssertions;

namespace CarStoreManager.Tests.Unidade.Domain.ValueObjects;

/*
    Este arquivo implementa os testes automaticos da classe DadosFuncionario.cs
    um VO do sistema, que inclui regra internas de funcionamento.

    Testes automaticos implementados para:
        Validar um funcionario valido contratado hoje,
        Validar um funcionario valido contradado para até 30 dias no futuro,
        Bloquear funcionario cadastrado antes de hoje,
        Bloquear funcionario cadastrado mais de 30 dias no futuro,
        Validar criar um funcioanrio de nivel Junior,
        Validar criar um funcioanrio de nivel Pleno,
        Validar criar um funcioanrio de nivel Senior,
        Calculo de anos trabalhando na empresa.
*/

public class DadosFuncionarioTest
{
    [Fact]
    public void Valida_Criar_DadosFuncionario_Valido_Hoje()
    {
        //cenario
        var data = DateTime.UtcNow;

        var dados = new DadosFuncionario(NivelFuncionario.Junior, data);

        //verificação
        dados.GetNivel().Should().Be(NivelFuncionario.Junior);
        dados.GetDataContratacao().Should().Be(data);
    }

    [Fact]
    public void Valida_Criar_DadosFuncionario_Valido_Daqui_30_Dias()
    {
        //cenario
        var data = DateTime.UtcNow.AddDays(30);

        var dados = new DadosFuncionario(NivelFuncionario.Junior, data);

        //verificação
        dados.GetNivel().Should().Be(NivelFuncionario.Junior);
        dados.GetDataContratacao().Should().Be(data);
    }

    /*
        No próprio metodo construtor do DadosFuncionario, ele chama 
        a função de validação, sendo assim, nos testes abaixo
        a criação do cenario, de criar um ano já é mandada como
        parametro para o assert que deve receber verifica a 
        mensagem de erro presente na exception "DataContratacaoInvalidoException"
    */

    [Fact]
    public void Deve_Rejeitar_Data_Antes_De_Hoje()
    {
        Assert.Throws<DataContratacaoInvalidoException>(() => new DadosFuncionario(NivelFuncionario.Junior, DateTime.UtcNow.AddDays(-1)));
    }

    [Fact]
    public void Deve_Rejeitar_Data_Futura_Maior_Que_30_Dias()
    {
        Assert.Throws<DataContratacaoInvalidoException>(() => new DadosFuncionario(NivelFuncionario.Junior, DateTime.UtcNow.AddDays(31)));
    }

    /*
        Abaixo seguem alguns testes que parecem desnecessarios
        Servem apenas para validar que os itens do Enum de NivelFuncionario
        (Junior, Pleno e Senior) são niveis validos para os funcionarios.
    */

    [Fact]
    public void Valida_Criar_DadosFuncionario_Valido_Junior()
    {
        //cenario
        var data = DateTime.UtcNow;

        var dados = new DadosFuncionario(NivelFuncionario.Junior, data);

        //aplicação
        bool nivelValido = dados.ValidarNivelFuncionario(dados.GetNivel());

        //verificação
        Assert.True(nivelValido);
    }

    [Fact]
    public void Valida_Criar_DadosFuncionario_Valido_Pleno()
    {
        //cenario
        var data = DateTime.UtcNow;

        var dados = new DadosFuncionario(NivelFuncionario.Pleno, data);

        //aplicação
        bool nivelValido = dados.ValidarNivelFuncionario(dados.GetNivel());

        //verificação
        Assert.True(nivelValido);
    }

    [Fact]
    public void Valida_Criar_DadosFuncionario_Valido_Senior()
    {
        //cenario
        var data = DateTime.UtcNow;

        var dados = new DadosFuncionario(NivelFuncionario.Senior, data);

        //aplicação
        bool nivelValido = dados.ValidarNivelFuncionario(dados.GetNivel());

        //verificação
        Assert.True(nivelValido);
    }

    /*
        A função abaixo calcula os anos de empresa de um funcionario.
        Nossas regras de cadastro de funcionario empedem o sistema de 
        cadastrar qualquer pessoa antes do dia de hoje, sendo assim só 
        podemos testar que, quem foi cadastrado hoje deve ter 0 anos de empresa
    */

    [Fact]
    public void Deve_Calcular_Anos_De_Empresa_De_Alguem_Contratado_Hoje()
    {
        var data = DateTime.UtcNow;

        var dados = new DadosFuncionario(NivelFuncionario.Senior, data);

        var anos = dados.CalcularAnosEmpresa();

        Assert.True(anos == 0);
    }
}