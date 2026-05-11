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
        proposta.Entrada.GetValorDinheiro().Should().Be(0m);
        proposta.Status.Should().Be(StatusPropostaVenda.Criada);
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
    public void Aprovar_AVista_ComModoPagamentoDefinido_MudaParaAprovada()
    {
        var proposta = CriarPropostaBase();
        proposta.DefinirModoPagamento(ModoPagamento.Dinheiro);
        proposta.Aprovar();
        proposta.Status.Should().Be(StatusPropostaVenda.Aprovada);
        proposta.DataAprovacao.Should().NotBeNull();
    }

    [Fact]
    public void Aprovar_SemModoPagamento_LancaInvalidOperationException()
    {
        var proposta = CriarPropostaBase();
        Action act = () => proposta.Aprovar();
        act.Should().Throw<InvalidOperationException>().WithMessage("*modo de pagamento*");
    }

    [Fact]
    public void Aprovar_Financiamento_SemRespostaFinanciadora_LancaInvalidOperationException()
    {
        var proposta = CriarPropostaBase();
        proposta.DefinirModoPagamento(ModoPagamento.Financiamento);
        Action act = () => proposta.Aprovar();
        act.Should().Throw<InvalidOperationException>().WithMessage("*financiamento*");
    }

    [Theory]
    [InlineData(StatusPropostaVenda.Rejeitada)]
    [InlineData(StatusPropostaVenda.Cancelada)]
    [InlineData(StatusPropostaVenda.Concluida)]
    [InlineData(StatusPropostaVenda.Expirada)]
    public void Aprovar_EstadoTerminal_LancaInvalidOperationException(StatusPropostaVenda statusInicial)
    {
        var proposta = CriarPropostaBase();
        typeof(PropostaVenda).GetProperty("Status")?.SetValue(proposta, statusInicial);
        Action act = () => proposta.Aprovar();
        act.Should().Throw<InvalidOperationException>().WithMessage("*terminal*");
    }

    [Fact]
    public void Rejeitar_ComMotivo_MudaParaRejeitada()
    {
        var proposta = CriarPropostaBase();
        proposta.Rejeitar("cliente desistiu");
        proposta.Status.Should().Be(StatusPropostaVenda.Rejeitada);
        proposta.MotivoRejeicao.Should().Be("cliente desistiu");
    }

    [Fact]
    public void Rejeitar_SemMotivo_LancaArgumentException()
    {
        var proposta = CriarPropostaBase();
        Action act = () => proposta.Rejeitar(" ");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(StatusPropostaVenda.Rejeitada)]
    [InlineData(StatusPropostaVenda.Cancelada)]
    [InlineData(StatusPropostaVenda.Concluida)]
    [InlineData(StatusPropostaVenda.Expirada)]
    public void Rejeitar_EstadoTerminal_LancaInvalidOperationException(StatusPropostaVenda statusInicial)
    {
        var proposta = CriarPropostaBase();
        typeof(PropostaVenda).GetProperty("Status")?.SetValue(proposta, statusInicial);
        Action act = () => proposta.Rejeitar("motivo qualquer");
        act.Should().Throw<InvalidOperationException>().WithMessage("*terminal*");
    }

    [Fact]
    public void Cancelar_ComMotivo_MudaParaCancelada()
    {
        var proposta = CriarPropostaBase();
        proposta.Cancelar("desistência");
        proposta.Status.Should().Be(StatusPropostaVenda.Cancelada);
        proposta.MotivoCancelamento.Should().Be("desistência");
    }

    [Fact]
    public void TentarExpirar_PropostaCriadaHa8Dias_MarcaComoExpirada()
    {
        var proposta = CriarPropostaBase();
        // Forçar DataCriacao 8 dias atrás
        typeof(PropostaVenda).BaseType?.GetProperty("DataCriacao")?.SetValue(proposta, DateTime.UtcNow.AddDays(-8));
        // Mas o DataCriacao da PropostaVenda é shadowed — usar a property direta da derived class
        typeof(PropostaVenda).GetProperty("DataCriacao")?.SetValue(proposta, DateTime.UtcNow.AddDays(-8));

        var expirou = proposta.TentarExpirar();

        expirou.Should().BeTrue();
        proposta.Status.Should().Be(StatusPropostaVenda.Expirada);
    }

    [Fact]
    public void TentarExpirar_PropostaRecente_NaoExpira()
    {
        var proposta = CriarPropostaBase();
        var expirou = proposta.TentarExpirar();
        expirou.Should().BeFalse();
        proposta.Status.Should().Be(StatusPropostaVenda.Criada);
    }

    [Fact]
    public void TentarExpirar_PropostaJaAprovada_NaoExpira()
    {
        var proposta = CriarPropostaBase();
        proposta.DefinirModoPagamento(ModoPagamento.Dinheiro);
        proposta.Aprovar();
        typeof(PropostaVenda).GetProperty("DataCriacao")?.SetValue(proposta, DateTime.UtcNow.AddDays(-30));

        var expirou = proposta.TentarExpirar();
        expirou.Should().BeFalse();
        proposta.Status.Should().Be(StatusPropostaVenda.Aprovada);
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
