using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.Vendedor;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Tests.Integratrion.Helpers;
using CarStoreManager.Web;

namespace CarStoreManager.Tests.Integratrion.Controllers;

/// <summary>
/// Testes de integração do VendedorController.
/// O controller inteiro é [Authorize(Roles="Admin")] — qualquer outra role deve receber 403.
/// </summary>
public class VendedorControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IVendedorService> _serviceMock;

    public VendedorControllerTests(WebApplicationFactory<Program> factory)
    {
        _serviceMock = new Mock<IVendedorService>();
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // "Testing" faz o Program.cs pular migrate/seed do banco real —
            // esses testes mockam o service, nunca tocam o AppDbContext de verdade.
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IVendedorService>();
                services.AddScoped(_ => _serviceMock.Object);

                services.AddTestAuth();
            });
        });
    }

    private HttpClient ClienteAdmin()
    {
        var c = _factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "Admin");
        return c;
    }

    // ==================== AUTORIZAÇÃO ====================

    [Fact]
    public async Task QualquerEndpoint_SemAutenticacao_RetornaUnauthorized()
    {
        var response = await _factory.CreateClient().GetAsync("/api/vendedor");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("Vendedor")]
    [InlineData("GerenteVendas")]
    [InlineData("Recepcionista")]
    public async Task GetTodos_RoleNaoAdmin_RetornaForbidden(string role)
    {
        var c = _factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", role);

        var response = await c.GetAsync("/api/vendedor");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ==================== GET ====================

    [Fact]
    public async Task GetTodos_ComoAdmin_RetornaOk()
    {
        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result<IEnumerable<VendedorListaDTO>>.Ok(new List<VendedorListaDTO>()));

        var response = await ClienteAdmin().GetAsync("/api/vendedor");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPorId_Existente_RetornaOk()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByIdAsync(id))
            .ReturnsAsync(Result<VendedorDTO>.Ok(new VendedorDTO { Id = id, Nome = "Vendedor X" }));

        var response = await ClienteAdmin().GetAsync($"/api/vendedor/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<VendedorDTO>())!.Nome.Should().Be("Vendedor X");
    }

    [Fact]
    public async Task GetPorId_Inexistente_RetornaNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Result<VendedorDTO>.Fail("Vendedor não encontrado"));

        var response = await ClienteAdmin().GetAsync($"/api/vendedor/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ==================== POST ====================

    [Fact]
    public async Task Criar_DTOValido_RetornaCreated()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.AddAsync(It.IsAny<CriarVendedorDTO>()))
            .ReturnsAsync(Result<Guid>.Ok(id));

        var response = await ClienteAdmin().PostAsJsonAsync("/api/vendedor", new CriarVendedorDTO
        {
            Nome = "Novo",
            Email = "novo@x.com",
            Telefone = "11999999999",
            Senha = "Senha@123",
            Nivel = "Junior",
            DataContratacao = DateTime.Today
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location!.ToString().Should().Contain(id.ToString());
    }

    [Fact]
    public async Task Criar_ServicoFalha_RetornaBadRequest()
    {
        _serviceMock.Setup(s => s.AddAsync(It.IsAny<CriarVendedorDTO>()))
            .ReturnsAsync(Result<Guid>.Fail("Email duplicado"));

        var response = await ClienteAdmin().PostAsJsonAsync("/api/vendedor", new CriarVendedorDTO());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ==================== PUT ====================

    [Fact]
    public async Task Atualizar_Sucesso_RetornaNoContent()
    {
        _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<AtualizarVendedorDTO>()))
            .ReturnsAsync(Result.Ok());

        var id = Guid.NewGuid();
        var response = await ClienteAdmin()
            .PutAsJsonAsync($"/api/vendedor/{id}", new AtualizarVendedorDTO
            {
                Email = "v@v.com", Telefone = "11999990000", Nivel = "Pleno"
            });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _serviceMock.Verify(s => s.UpdateAsync(It.Is<AtualizarVendedorDTO>(d => d.Id == id)), Times.Once);
    }

    // ==================== DELETE ====================

    [Fact]
    public async Task Remover_Sucesso_RetornaNoContent()
    {
        _serviceMock.Setup(s => s.RemoveAsync(It.IsAny<Guid>())).ReturnsAsync(Result.Ok());

        var response = await ClienteAdmin().DeleteAsync($"/api/vendedor/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Remover_Inexistente_RetornaNotFound()
    {
        _serviceMock.Setup(s => s.RemoveAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Result.Fail("Não encontrado"));

        var response = await ClienteAdmin().DeleteAsync($"/api/vendedor/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
