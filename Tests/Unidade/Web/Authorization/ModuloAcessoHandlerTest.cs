using System.Security.Claims;
using Moq;
using Xunit;
using FluentAssertions;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Web.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace CarStoreManager.Tests.Unidade.Web.Authorization;

public class ModuloAcessoHandlerTest
{
    private static AuthorizationHandlerContext CriarContexto(ModuloAcessoRequirement requirement)
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }, "TesteAuth");
        return new AuthorizationHandlerContext(new[] { requirement }, new ClaimsPrincipal(identity), null);
    }

    [Fact]
    public async Task HandleRequirementAsync_ModuloAtivo_Succeed()
    {
        var configMock = new Mock<IConfiguracaoSistemaService>();
        configMock.Setup(s => s.ObterModulosAtivosAsync())
            .ReturnsAsync(Result<(bool, bool)>.Ok((true, true)));

        var context = CriarContexto(new ModuloAcessoRequirement("concessionaria"));
        await new ModuloAcessoHandler(configMock.Object).HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ModuloDesativado_NaoSucceed()
    {
        var configMock = new Mock<IConfiguracaoSistemaService>();
        configMock.Setup(s => s.ObterModulosAtivosAsync())
            .ReturnsAsync(Result<(bool, bool)>.Ok((false, true)));

        var context = CriarContexto(new ModuloAcessoRequirement("concessionaria"));
        await new ModuloAcessoHandler(configMock.Object).HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_OutroModuloAtivo_NaoAfeta()
    {
        var configMock = new Mock<IConfiguracaoSistemaService>();
        configMock.Setup(s => s.ObterModulosAtivosAsync())
            .ReturnsAsync(Result<(bool, bool)>.Ok((true, false)));

        var context = CriarContexto(new ModuloAcessoRequirement("oficina"));
        await new ModuloAcessoHandler(configMock.Object).HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }
}
