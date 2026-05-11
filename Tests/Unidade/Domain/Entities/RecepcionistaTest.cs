using FluentAssertions;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades;

public class RecepcionistaTest
{
    [Fact]
    public void Construtor_DadosValidos_CriaInstanciaComRoleRecepcionista()
    {
        var data = DateTime.Now.AddDays(1);
        var r = new Recepcionista(
            "Ana Recepcionista", "ana@x.com", "11988887777", "Senha@1",
            3500m, NivelFuncionario.Pleno, data);

        r.GetNome().Should().Be("Ana Recepcionista");
        r.Role.Should().Be(RoleUsuario.Recepcionista);
        r.GetNivel().Should().Be("Pleno");
        r.GetDataContratacao().Should().Be(data);
        r.Ativo.Should().BeTrue();
    }

    [Fact]
    public void AtualizarNivel_NovoNivel_AtualizaCampo()
    {
        var r = CriarValido();
        r.AtualizarNivel(NivelFuncionario.Senior);
        r.GetNivel().Should().Be("Senior");
    }

    [Fact]
    public void Desativar_TornaInativo()
    {
        var r = CriarValido();
        r.Desativar();
        r.Ativo.Should().BeFalse();
    }

    private static Recepcionista CriarValido() => new(
        "Ana", "ana@x.com", "11988887777", "Senha@1",
        3500m, NivelFuncionario.Pleno, DateTime.Now.AddDays(1));
}
