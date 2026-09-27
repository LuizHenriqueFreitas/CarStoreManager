using System.Security.Claims;
using Moq;
using Xunit;
using FluentAssertions;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Web.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace CarStoreManager.Tests.Unidade.Web.Authorization;

public class RecursoAcessoHandlerTest
{
    private const string Chave = "pagina:/oficina/fornecedores";

    private static ClaimsPrincipal CriarUsuario(Guid usuarioId, RoleUsuario role)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new Claim(ClaimTypes.Role, role.ToString())
        }, "TesteAuth");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task HandleRequirementAsync_ServicoPermite_Succeed()
    {
        var usuarioId = Guid.NewGuid();
        var permissaoMock = new Mock<IPermissaoAcessoService>();
        permissaoMock.Setup(s => s.PodeAcessarAsync(usuarioId, RoleUsuario.Vendedor, Chave)).ReturnsAsync(true);

        var requirement = new RecursoAcessoRequirement(Chave);
        var context = new AuthorizationHandlerContext(new[] { requirement }, CriarUsuario(usuarioId, RoleUsuario.Vendedor), null);

        await new RecursoAcessoHandler(permissaoMock.Object).HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ServicoNega_NaoSucceed()
    {
        var usuarioId = Guid.NewGuid();
        var permissaoMock = new Mock<IPermissaoAcessoService>();
        permissaoMock.Setup(s => s.PodeAcessarAsync(usuarioId, RoleUsuario.Vendedor, Chave)).ReturnsAsync(false);

        var requirement = new RecursoAcessoRequirement(Chave);
        var context = new AuthorizationHandlerContext(new[] { requirement }, CriarUsuario(usuarioId, RoleUsuario.Vendedor), null);

        await new RecursoAcessoHandler(permissaoMock.Object).HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_UsuarioNaoAutenticado_NaoSucceed()
    {
        var permissaoMock = new Mock<IPermissaoAcessoService>();
        var requirement = new RecursoAcessoRequirement(Chave);
        var contexto = new AuthorizationHandlerContext(new[] { requirement }, new ClaimsPrincipal(new ClaimsIdentity()), null);

        await new RecursoAcessoHandler(permissaoMock.Object).HandleAsync(contexto);

        contexto.HasSucceeded.Should().BeFalse();
        permissaoMock.Verify(s => s.PodeAcessarAsync(It.IsAny<Guid>(), It.IsAny<RoleUsuario>(), It.IsAny<string>()), Times.Never);
    }
}
