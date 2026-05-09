// Este arquivo nao foi revisado nem documentado
using FluentAssertions;
using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Oficina;

public class ItemOrdemServicoTests
{
    // ==================== CONSTRUTOR ====================

    [Fact]
    public void Construtor_CamposValidos_CriaItemEAtribuiCorretamente()
    {
        var componenteId = Guid.NewGuid();
        var ordemServicoId = Guid.NewGuid();
        var quantidade = 3;
        var valorUnitario = 150.75m;

        var item = new ItemOrdemServico(componenteId, ordemServicoId, quantidade, valorUnitario);

        item.ComponenteId.Should().Be(componenteId);
        item.OrdemServicoId.Should().Be(ordemServicoId);
        item.Quantidade.Should().Be(quantidade);
        item.ValorUnitario.GetValorDinheiro().Should().Be(valorUnitario);
        item.ValorTotal.GetValorDinheiro().Should().Be(quantidade * valorUnitario); // 452.25m
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Construtor_QuantidadeInvalida_LancaArgumentException(int quantidade)
    {
        Action act = () => new ItemOrdemServico(Guid.NewGuid(), Guid.NewGuid(), quantidade, 50m);
        act.Should().Throw<ArgumentException>().WithMessage("*Quantidade*");
    }

    [Fact]
    public void Construtor_ValorUnitarioNegativo_LancaArgumentException()
    {
        Action act = () => new ItemOrdemServico(Guid.NewGuid(), Guid.NewGuid(), 1, -0.01m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Construtor_ValorUnitarioZero_DeveAceitar()
    {
        var item = new ItemOrdemServico(Guid.NewGuid(), Guid.NewGuid(), 1, 0m);
        item.ValorUnitario.GetValorDinheiro().Should().Be(0m);
        item.ValorTotal.GetValorDinheiro().Should().Be(0m);
    }

    // ==================== GETTERS ====================

    [Fact]
    public void GetComponentId_RetornaComponenteId()
    {
        var componenteId = Guid.NewGuid();
        var item = new ItemOrdemServico(componenteId, Guid.NewGuid(), 1, 10m);
        item.GetComponentId().Should().Be(componenteId);
    }

    [Fact]
    public void GetOrdemServicoId_RetornaOrdemServicoId()
    {
        var ordemId = Guid.NewGuid();
        var item = new ItemOrdemServico(Guid.NewGuid(), ordemId, 1, 10m);
        item.GetOrdemServicoId().Should().Be(ordemId);
    }

    [Fact]
    public void GetQuantidade_RetornaQuantidade()
    {
        var item = new ItemOrdemServico(Guid.NewGuid(), Guid.NewGuid(), 5, 20m);
        item.GetQuantidade().Should().Be(5);
    }

    [Fact]
    public void GetValorUnitario_RetornaDecimal()
    {
        var item = new ItemOrdemServico(Guid.NewGuid(), Guid.NewGuid(), 1, 123.45m);
        item.GetValorUnitario().Should().Be(123.45m);
    }

    [Fact]
    public void GetValorTotal_RetornaDecimal()
    {
        var item = new ItemOrdemServico(Guid.NewGuid(), Guid.NewGuid(), 4, 25m);
        item.GetValorTotal().Should().Be(100m);
    }

    // ==================== ALTERAR QUANTIDADE ====================

    [Fact]
    public void AlterarQuantidade_ValorValido_AtualizaQuantidadeERecalculaTotal()
    {
        var item = new ItemOrdemServico(Guid.NewGuid(), Guid.NewGuid(), 2, 50m); // total = 100
        item.AlterarQuantidade(5);
        item.Quantidade.Should().Be(5);
        item.ValorTotal.GetValorDinheiro().Should().Be(250m); // 5 * 50
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void AlterarQuantidade_Invalida_LancaArgumentException(int novaQuantidade)
    {
        var item = new ItemOrdemServico(Guid.NewGuid(), Guid.NewGuid(), 1, 50m);
        Action act = () => item.AlterarQuantidade(novaQuantidade);
        act.Should().Throw<ArgumentException>().WithMessage("*Quantidade*");
    }

    // ==================== ATUALIZAR VALOR UNITÁRIO ====================

    [Fact]
    public void AtualizarValorUnitario_ValorValido_AtualizaERecalculaTotal()
    {
        var item = new ItemOrdemServico(Guid.NewGuid(), Guid.NewGuid(), 3, 20m); // total = 60
        item.AtualizarValorUnitario(25m);
        item.ValorUnitario.GetValorDinheiro().Should().Be(25m);
        item.ValorTotal.GetValorDinheiro().Should().Be(75m); // 3 * 25
    }

    [Fact]
    public void AtualizarValorUnitario_ValorNegativo_LancaArgumentException()
    {
        var item = new ItemOrdemServico(Guid.NewGuid(), Guid.NewGuid(), 1, 10m);
        Action act = () => item.AtualizarValorUnitario(-1m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AtualizarValorUnitario_Zero_DeveAceitar()
    {
        var item = new ItemOrdemServico(Guid.NewGuid(), Guid.NewGuid(), 5, 30m);
        item.AtualizarValorUnitario(0m);
        item.ValorUnitario.GetValorDinheiro().Should().Be(0m);
        item.ValorTotal.GetValorDinheiro().Should().Be(0m);
    }

    // ==================== CÁLCULO DO TOTAL ====================

    [Fact]
    public void CalcularTotal_AcionadoPeloConstrutor_RecalculaCorretamente()
    {
        var item = new ItemOrdemServico(Guid.NewGuid(), Guid.NewGuid(), 10, 3.5m);
        item.ValorTotal.GetValorDinheiro().Should().Be(35m);
    }

    [Fact]
    public void CalcularTotal_AposAlterarQuantidade_RecalculaCorretamente()
    {
        var item = new ItemOrdemServico(Guid.NewGuid(), Guid.NewGuid(), 2, 10m);
        item.AlterarQuantidade(7);
        item.ValorTotal.GetValorDinheiro().Should().Be(70m);
    }

    [Fact]
    public void CalcularTotal_AposAtualizarValorUnitario_RecalculaCorretamente()
    {
        var item = new ItemOrdemServico(Guid.NewGuid(), Guid.NewGuid(), 5, 100m);
        item.AtualizarValorUnitario(80m);
        item.ValorTotal.GetValorDinheiro().Should().Be(400m);
    }
}