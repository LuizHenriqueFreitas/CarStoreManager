// Este arquivo nao foi revisado nem documentado
/*
using FluentAssertions;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Oficina;

public class ComponenteTest
{
    // ==================== CONSTRUTOR ====================

    [Fact]
    public void Construtor_CamposValidos_AtribuiCorretamente()
    {
        var componente = new Componente("Pastilha de freio", "P-2023", SistemaComponente.Freios, 120.50m, 10, 3);

        componente.Nome.Should().Be("Pastilha de freio");
        componente.Modelo.Should().Be("P-2023");
        componente.Sistema.Should().Be(SistemaComponente.Freios);
        componente.Valor.GetValorDinheiro().Should().Be(120.50m);
        componente.QuantidadeEstoque.Should().Be(10);
        componente.EstoqueMinimo.Should().Be(3);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_NomeInvalido_LancaArgumentException(string nome)
    {
        Action act = () => new Componente(nome, "Modelo", SistemaComponente.Motor, 100, 0, 0);
        act.Should().Throw<ArgumentException>().WithMessage("*Nome*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ModeloInvalido_LancaArgumentException(string modelo)
    {
        Action act = () => new Componente("Nome", modelo, SistemaComponente.Motor, 100, 0, 0);
        act.Should().Throw<ArgumentException>().WithMessage("*Modelo*");
    }

    [Fact]
    public void Construtor_QuantidadeInicialNegativa_LancaArgumentException()
    {
        Action act = () => new Componente("Nome", "Modelo", SistemaComponente.Motor, 100, -1, 0);
        act.Should().Throw<ArgumentException>().WithMessage("*Estoque inicial*");
    }

    [Fact]
    public void Construtor_EstoqueMinimoNegativo_LancaArgumentException()
    {
        Action act = () => new Componente("Nome", "Modelo", SistemaComponente.Motor, 100, 0, -1);
        act.Should().Throw<ArgumentException>().WithMessage("*Estoque mínimo*");
    }

    [Fact]
    public void Construtor_ValorNegativo_LancaArgumentException()
    {
        Action act = () => new Componente("Nome", "Modelo", SistemaComponente.Motor, -1, 0, 0);
        act.Should().Throw<ArgumentException>().WithMessage("*Valor*"); // Dinheiro valida no construtor
    }

    [Fact]
    public void Construtor_QuantidadeInicialZero_DeveAceitar()
    {
        var componente = new Componente("Filtro", "F-01", SistemaComponente.Motor, 50, 0, 0);
        componente.QuantidadeEstoque.Should().Be(0);
    }

    // ==================== GETTERS ====================

    [Fact]
    public void GetNome_RetornaNome()
    {
        var componente = new Componente("Correia", "C-10", SistemaComponente.Motor, 80, 5, 1);
        componente.GetNome().Should().Be("Correia");
    }

    [Fact]
    public void GetModelo_RetornaModelo()
    {
        var componente = new Componente("Correia", "C-10", SistemaComponente.Motor, 80, 5, 1);
        componente.GetModelo().Should().Be("C-10");
    }

    [Fact]
    public void GetSistema_RetornaStringDoEnum()
    {
        var componente = new Componente("Amortecedor", "A-05", SistemaComponente.Suspensao, 200, 4, 2);
        componente.GetSistema().Should().Be("Suspensao"); // ou o valor exato do ToString do enum
    }

    [Fact]
    public void GetValor_RetornaDecimal()
    {
        var componente = new Componente("Peça", "XPTO", SistemaComponente.Eletrica, 75.9m, 3, 1);
        componente.GetValor().Should().Be(75.9m);
    }

    [Fact]
    public void GetQuantidade_RetornaEstoque()
    {
        var componente = new Componente("Peça", "XPTO", SistemaComponente.Eletrica, 75.9m, 3, 1);
        componente.GetQuantidade().Should().Be(3);
    }

    [Fact]
    public void GetEstoqueMinimo_RetornaMinimo()
    {
        var componente = new Componente("Peça", "XPTO", SistemaComponente.Eletrica, 75.9m, 3, 1);
        componente.GetEstoqueMinimo().Should().Be(1);
    }

    // ==================== SETTERS ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DefinirNome_Invalido_LancaArgumentException(string nome)
    {
        var componente = new Componente("Original", "M-1", SistemaComponente.Motor, 10, 0, 0);
        Action act = () => componente.DefinirNome(nome);
        act.Should().Throw<ArgumentException>().WithMessage("*Nome*");
    }

    [Fact]
    public void DefinirNome_Valido_AlteraNome()
    {
        var componente = new Componente("Original", "M-1", SistemaComponente.Motor, 10, 0, 0);
        componente.DefinirNome("Novo Nome");
        componente.Nome.Should().Be("Novo Nome");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DefinirModelo_Invalido_LancaArgumentException(string modelo)
    {
        var componente = new Componente("Nome", "M-1", SistemaComponente.Motor, 10, 0, 0);
        Action act = () => componente.DefinirModelo(modelo);
        act.Should().Throw<ArgumentException>().WithMessage("*Modelo*");
    }

    [Fact]
    public void DefinirModelo_Valido_AlteraModelo()
    {
        var componente = new Componente("Nome", "M-1", SistemaComponente.Motor, 10, 0, 0);
        componente.DefinirModelo("M-2");
        componente.Modelo.Should().Be("M-2");
    }

    [Fact]
    public void AtualizarValor_AlteraValor()
    {
        var componente = new Componente("Peça", "XPTO", SistemaComponente.Motor, 50, 1, 1);
        componente.AtualizarValor(new Dinheiro(45.5m));
        componente.Valor.GetValorDinheiro().Should().Be(45.5m);
    }

    [Fact]
    public void DefinirEstoqueMinimo_Negativo_LancaArgumentException()
    {
        var componente = new Componente("Peça", "XPTO", SistemaComponente.Motor, 50, 1, 1);
        Action act = () => componente.DefinirEstoqueMinimo(-1);
        act.Should().Throw<ArgumentException>().WithMessage("*Estoque mínimo*");
    }

    [Fact]
    public void DefinirEstoqueMinimo_Valido_AlteraMinimo()
    {
        var componente = new Componente("Peça", "XPTO", SistemaComponente.Motor, 50, 1, 1);
        componente.DefinirEstoqueMinimo(5);
        componente.EstoqueMinimo.Should().Be(5);
    }

    [Fact]
    public void AtualizarDadosComponente_AtualizaValores()
    {
        var componente = new Componente("Peça", "XPTO", SistemaComponente.Motor, 50, 1, 1);
        componente.AtualizarDadosComponente(30.0m, 10, 3);
        componente.GetValor().Should().Be(30.0m);
        componente.GetQuantidade().Should().Be(10);
        componente.GetEstoqueMinimo().Should().Be(3);
    }

    // ==================== GERENCIAMENTO DE ESTOQUE ====================

    [Fact]
    public void AdicionarEstoque_QuantidadePositiva_AumentaEstoque()
    {
        var componente = new Componente("Filtro", "F-1", SistemaComponente.Motor, 20, 5, 2);
        componente.AdicionarEstoque(3);
        componente.QuantidadeEstoque.Should().Be(8);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void AdicionarEstoque_QuantidadeInvalida_LancaArgumentException(int quantidade)
    {
        var componente = new Componente("Filtro", "F-1", SistemaComponente.Motor, 20, 5, 2);
        Action act = () => componente.AdicionarEstoque(quantidade);
        act.Should().Throw<ArgumentException>().WithMessage("*Quantidade*");
    }

    [Fact]
    public void RemoverEstoque_QuantidadeValida_ReduzEstoque()
    {
        var componente = new Componente("Óleo", "O-10", SistemaComponente.Motor, 40, 10, 2);
        componente.RemoverEstoque(4);
        componente.QuantidadeEstoque.Should().Be(6);
    }

    [Fact]
    public void RemoverEstoque_QuantidadeMaiorQueEstoque_LancaInvalidOperationException()
    {
        var componente = new Componente("Óleo", "O-10", SistemaComponente.Motor, 40, 3, 1);
        Action act = () => componente.RemoverEstoque(5);
        act.Should().Throw<InvalidOperationException>().WithMessage("*Estoque insuficiente*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void RemoverEstoque_QuantidadeInvalida_LancaArgumentException(int quantidade)
    {
        var componente = new Componente("Óleo", "O-10", SistemaComponente.Motor, 40, 5, 1);
        Action act = () => componente.RemoverEstoque(quantidade);
        act.Should().Throw<ArgumentException>().WithMessage("*Quantidade*");
    }

    [Fact]
    public void EstoqueBaixo_QuandoQuantidadeMenorQueMinimo_RetornaTrue()
    {
        var componente = new Componente("Pastilha", "P-1", SistemaComponente.Freios, 70, 2, 3);
        componente.EstoqueBaixo().Should().BeTrue();
    }

    [Fact]
    public void EstoqueBaixo_QuandoQuantidadeIgualAoMinimo_RetornaTrue()
    {
        var componente = new Componente("Pastilha", "P-1", SistemaComponente.Freios, 70, 2, 2);
        componente.EstoqueBaixo().Should().BeTrue();
    }

    [Fact]
    public void EstoqueBaixo_QuandoQuantidadeMaiorQueMinimo_RetornaFalse()
    {
        var componente = new Componente("Pastilha", "P-1", SistemaComponente.Freios, 70, 5, 2);
        componente.EstoqueBaixo().Should().BeFalse();
    }

    [Fact]
    public void TemEstoque_QuantidadeMenorOuIgual_RetornaTrue()
    {
        var componente = new Componente("Disco", "D-1", SistemaComponente.Freios, 150, 4, 1);
        componente.TemEstoque(4).Should().BeTrue();
        componente.TemEstoque(2).Should().BeTrue();
    }

    [Fact]
    public void TemEstoque_QuantidadeMaior_RetornaFalse()
    {
        var componente = new Componente("Disco", "D-1", SistemaComponente.Freios, 150, 4, 1);
        componente.TemEstoque(5).Should().BeFalse();
    }
}*/