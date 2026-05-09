// Este arquivo nao foi revisado nem documentado
using FluentAssertions;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Concessionaria;
public class PropostaVendaTest
{
    // ==================== CONSTRUTOR ====================

    [Fact]
    public void Construtor_CamposValidos_CriaPropostaComStatusRascunho()
    {
        var vendedorId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var proposta = new PropostaVenda(
            vendedorId, 
            veiculoId, 
            clienteId, 
            100000m, 
            50
        );

        proposta.VendedorId.Should().Be(vendedorId);
        proposta.VeiculoVendaId.Should().Be(veiculoId);
        proposta.ClienteId.Should().Be(clienteId);
        proposta.ValorBase.GetValorDinheiro().Should().Be(100000m);
        proposta.Desconto.GetDescontoValor().Should().Be(50);
        proposta.ValorFinal.GetValorDinheiro().Should().Be(50000m);
        Assert.Null(proposta.Entrada);
        proposta.Status.Should().Be(StatusPropostaVenda.Rascunho);
        proposta.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Construtor_ValorBaseNegativo_LancaArgumentException()
    {
        Action act = () => new PropostaVenda(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), -1, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Construtor_DescontoNegativo_LancaArgumentException()
    {
        Action act = () => new PropostaVenda(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100000, -1);
        act.Should().Throw<ArgumentException>();
    }

    // ==================== DESCONTO ====================

    [Fact]
    public void AplicarDesconto_ValorValido_RecalculaValorFinal()
    {
        var proposta = CriarPropostaBase();
        proposta.AplicarDesconto(10);
        proposta.GetDesconto().Should().Be(10);
        proposta.GetValorFinal().Should().Be(90000m);
    }

    [Fact]
    public void AplicarDesconto_Zero_ValorFinalIgualBase()
    {
        var proposta = CriarPropostaBase();
        proposta.AplicarDesconto(0);
        proposta.GetValorFinal().Should().Be(100000m);
    }

    [Fact]
    public void AplicarDesconto_MaiorQueValorBase_LancaArgumentException()
    {
        var proposta = CriarPropostaBase();
        // Desconto maior que o valor base resulta em valor final negativo
        // O ValueObject Dinheiro deve lançar exceção
        Action act = () => proposta.AplicarDesconto(150000m);
        act.Should().Throw<ArgumentException>();
    }

    // ==================== ENTRADA ====================

    [Fact]
    public void AtualizarEntrada_ValorMenorQueValorFinal_AtualizaEntrada()
    {
        var proposta = CriarPropostaBase();
        proposta.AtualizarEntrada(30000m);
        proposta.GetEntrada().Should().Be(30000m);
    }

    [Fact]
    public void AtualizarEntrada_ValorIgualAoValorFinal_DeveAceitar()
    {
        var proposta = CriarPropostaBase();
        proposta.AplicarDesconto(0); // valor final = 100k
        proposta.AtualizarEntrada(100000m);
        proposta.GetEntrada().Should().Be(100000m);
    }

    [Fact]
    public void AtualizarEntrada_MaiorQueValorFinal_LancaArgumentException()
    {
        var proposta = CriarPropostaBase();
        Action act = () => proposta.AtualizarEntrada(120000m);
        act.Should().Throw<ArgumentException>().WithMessage("*Entrada*");
    }

    [Fact]
    public void AtualizarEntrada_ValorNegativo_LancaArgumentException()
    {
        var proposta = CriarPropostaBase();
        Action act = () => proposta.AtualizarEntrada(-1);
        act.Should().Throw<ArgumentException>();
    }

    // ==================== STATUS ====================

    [Fact]
    public void Aprovar_StatusRascunho_MudaParaAprovada()
    {
        var proposta = CriarPropostaBase();
        proposta.Aprovar();
        proposta.Status.Should().Be(StatusPropostaVenda.Aprovada);
    }

    [Theory]
    [InlineData(StatusPropostaVenda.Aprovada)]
    [InlineData(StatusPropostaVenda.Rejeitada)]
    [InlineData(StatusPropostaVenda.Cancelada)]
    public void Aprovar_StatusDiferenteDeRascunho_LancaInvalidOperationException(StatusPropostaVenda statusInicial)
    {
        var proposta = CriarPropostaBase();
        typeof(PropostaVenda)
            .GetProperty("Status")
            ?.SetValue(proposta, statusInicial);

        Action act = () => proposta.Aprovar();
        act.Should().Throw<InvalidOperationException>().WithMessage("*Estado*");
    }

    [Fact]
    public void Rejeitar_StatusRascunho_MudaParaRejeitada()
    {
        var proposta = CriarPropostaBase();
        proposta.Rejeitar();
        proposta.Status.Should().Be(StatusPropostaVenda.Rejeitada);
    }

    [Theory]
    [InlineData(StatusPropostaVenda.Aprovada)]
    [InlineData(StatusPropostaVenda.Rejeitada)]
    [InlineData(StatusPropostaVenda.Cancelada)]
    public void Rejeitar_StatusDiferenteDeRascunho_LancaInvalidOperationException(StatusPropostaVenda statusInicial)
    {
        var proposta = CriarPropostaBase();
        typeof(PropostaVenda)
            .GetProperty("Status")
            ?.SetValue(proposta, statusInicial);

        Action act = () => proposta.Rejeitar();
        act.Should().Throw<InvalidOperationException>().WithMessage("*Estado*");
    }

    [Fact]
    public void Cancelar_QualquerStatus_MudaParaCancelada()
    {
        var proposta = CriarPropostaBase();
        proposta.Aprovar(); // vai para aprovada
        proposta.Cancelar();

        Assert.Equal(StatusPropostaVenda.Cancelada, proposta.Status);
    }

    // ==================== MÉTODO AUXILIAR ====================

    private static PropostaVenda CriarPropostaBase()
    {
        return new PropostaVenda(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100000m,
            5);
    }
}
