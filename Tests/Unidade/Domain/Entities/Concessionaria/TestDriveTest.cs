using System;
using FluentAssertions;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using Xunit;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Concessionaria;

public class TestDriveTest
{
    private static TestDrive Novo() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Today.AddDays(1), "obs");

    [Fact]
    public void Construtor_Valido_CriaAgendado()
    {
        var td = Novo();
        td.Status.Should().Be(StatusTestDrive.Agendado);
        td.Observacao.Should().Be("obs");
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public void Construtor_IdVazio_LancaArgumentException(bool veic, bool cli, bool vend)
    {
        Action act = () => new TestDrive(
            veic ? Guid.Empty : Guid.NewGuid(),
            cli ? Guid.Empty : Guid.NewGuid(),
            vend ? Guid.Empty : Guid.NewGuid(),
            DateTime.Today.AddDays(1));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Construtor_ObservacaoVazia_ViraNull()
    {
        var td = new TestDrive(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, "   ");
        td.Observacao.Should().BeNull();
    }

    [Fact]
    public void MarcarRealizado_MudaStatus()
    {
        var td = Novo();
        td.MarcarRealizado();
        td.Status.Should().Be(StatusTestDrive.Realizado);
    }

    [Fact]
    public void Reagendar_AposRealizado_Lanca()
    {
        var td = Novo();
        td.MarcarRealizado();
        Action act = () => td.Reagendar(DateTime.Now.AddDays(2));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reagendar_Agendado_AtualizaDataEVoltaParaAgendado()
    {
        var td = Novo();
        td.MarcarNaoCompareceu();
        var nova = DateTime.Today.AddDays(3);
        td.Reagendar(nova);
        td.DataHora.Should().Be(nova);
        td.Status.Should().Be(StatusTestDrive.Agendado);
    }

    [Fact]
    public void TrocarVendedor_IdValido_Atualiza()
    {
        var td = Novo();
        var novo = Guid.NewGuid();
        td.TrocarVendedor(novo);
        td.VendedorId.Should().Be(novo);
    }

    [Fact]
    public void TrocarVendedor_IdVazio_Lanca()
    {
        var td = Novo();
        Action act = () => td.TrocarVendedor(Guid.Empty);
        act.Should().Throw<ArgumentException>();
    }
}
