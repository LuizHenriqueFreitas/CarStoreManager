using FluentAssertions;
using CarStoreManager.Domain.Entities.Sistema;
using Xunit;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Sistema;

public class ConfiguracaoSistemaTest
{
    [Fact]
    public void Construtor_PadraoDeFabrica_AmbosModulosAtivos()
    {
        var cfg = new ConfiguracaoSistema(true);

        cfg.ModuloConcessionariaAtivo.Should().BeTrue();
        cfg.ModuloOficinaAtivo.Should().BeTrue();
    }

    [Fact]
    public void ConfigurarModulos_DesativaUm_Aplica()
    {
        var cfg = new ConfiguracaoSistema(true);

        cfg.ConfigurarModulos(concessionariaAtivo: false, oficinaAtivo: true);

        cfg.ModuloConcessionariaAtivo.Should().BeFalse();
        cfg.ModuloOficinaAtivo.Should().BeTrue();
        cfg.DataUltimaAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void ConfigurarModulos_OsDoisDesativados_Lanca()
    {
        var cfg = new ConfiguracaoSistema(true);

        var act = () => cfg.ConfigurarModulos(concessionariaAtivo: false, oficinaAtivo: false);

        act.Should().Throw<ArgumentException>();
    }
}
