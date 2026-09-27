using System;
using FluentAssertions;
using CarStoreManager.Domain.Entities.Sistema;
using Xunit;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Sistema;

public class PermissaoIndividualTest
{
    [Fact]
    public void Construtor_ComDadosValidos_CriaCorretamente()
    {
        var usuarioId = Guid.NewGuid();
        var p = new PermissaoIndividual(usuarioId, "pagina:/oficina/fornecedores", true);

        p.UsuarioId.Should().Be(usuarioId);
        p.RecursoChave.Should().Be("pagina:/oficina/fornecedores");
        p.Permitido.Should().BeTrue();
        p.DataAtualizacao.Should().BeNull();
    }

    [Fact]
    public void Construtor_UsuarioIdVazio_Lanca()
    {
        var act = () => new PermissaoIndividual(Guid.Empty, "pagina:/oficina/fornecedores", true);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Construtor_ChaveVazia_Lanca()
    {
        var act = () => new PermissaoIndividual(Guid.NewGuid(), "", true);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Atualizar_MudaValorESetaData()
    {
        var p = new PermissaoIndividual(Guid.NewGuid(), "acao:oficina.novo-fornecedor", false);
        p.Atualizar(true);

        p.Permitido.Should().BeTrue();
        p.DataAtualizacao.Should().NotBeNull();
    }
}
