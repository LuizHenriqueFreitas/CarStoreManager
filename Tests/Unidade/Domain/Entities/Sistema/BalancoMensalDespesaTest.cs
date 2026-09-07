using System;
using FluentAssertions;
using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Enums;
using Xunit;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Sistema;

public class BalancoMensalDespesaTest
{
    [Fact]
    public void Construtor_NormalizaCompetenciaParaPrimeiroDia()
    {
        var b = new BalancoMensalDespesa(new DateOnly(2026, 9, 17));
        b.Competencia.Should().Be(new DateOnly(2026, 9, 1));
        b.Fechado.Should().BeFalse();
    }

    [Fact]
    public void AdicionarItem_SomaNoTotalEPorSetor()
    {
        var b = new BalancoMensalDespesa(new DateOnly(2026, 9, 1));
        b.AdicionarItem("Energia", SetorDespesa.Geral, "energia", 500m);
        b.AdicionarItem("Salário mecânico", SetorDespesa.Oficina, "pessoal", 3000m);

        b.Total().Should().Be(3500m);
        b.TotalPorSetor(SetorDespesa.Oficina).Should().Be(3000m);
        b.TotalPorSetor(SetorDespesa.Concessionaria).Should().Be(0m);
    }

    [Fact]
    public void Fechar_BloqueiaEdicao()
    {
        var b = new BalancoMensalDespesa(new DateOnly(2026, 9, 1));
        b.AdicionarItem("Energia", SetorDespesa.Geral, null, 100m);
        b.Fechar();
        b.Fechado.Should().BeTrue();

        Action add = () => b.AdicionarItem("Água", SetorDespesa.Geral, null, 50m);
        add.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reabrir_PermiteEditarDeNovo()
    {
        var b = new BalancoMensalDespesa(new DateOnly(2026, 9, 1));
        b.Fechar();
        b.Reabrir();
        b.Fechado.Should().BeFalse();

        Action add = () => b.AdicionarItem("Água", SetorDespesa.Geral, null, 50m);
        add.Should().NotThrow();
    }

    [Fact]
    public void ItemBalanco_NomeVazio_Lanca()
    {
        var b = new BalancoMensalDespesa(new DateOnly(2026, 9, 1));
        Action act = () => b.AdicionarItem("  ", SetorDespesa.Geral, null, 10m);
        act.Should().Throw<ArgumentException>();
    }
}
