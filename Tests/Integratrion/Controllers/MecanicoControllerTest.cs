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
using CarStoreManager.Application.DTOs.Oficina.Mecanico;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Tests.Integratrion.Helpers;
using CarStoreManager.Web;

namespace CarStoreManager.Tests.Integratrion.Controllers;

public class MecanicoControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IMecanicoService> _serviceMock;

    public MecanicoControllerTests(WebApplicationFactory<Program> factory)
    {
        _serviceMock = new Mock<IMecanicoService>();
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // "Testing" faz o Program.cs pular migrate/seed do banco real —
            // esses testes mockam o service, nunca tocam o AppDbContext de verdade.
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IMecanicoService>();
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
    public async Task GetTodos_SemAutenticacao_RetornaUnauthorized()
    {
        var r = await _factory.CreateClient().GetAsync("/api/mecanico");
        r.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTodos_RoleNaoAdmin_RetornaForbidden()
    {
        var c = _factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "Mecanico");
        var r = await c.GetAsync("/api/mecanico");
        r.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ==================== CRUD ====================

    [Fact]
    public async Task GetTodos_RetornaListaMesmoVazia()
    {
        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result<IEnumerable<MecanicoListaDTO>>.Ok(new List<MecanicoListaDTO>()));

        var r = await ClienteAdmin().GetAsync("/api/mecanico");

        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDisponiveis_RetornaApenasLookupDTOs()
    {
        _serviceMock.Setup(s => s.ObterDisponiveisAsync())
            .ReturnsAsync(Result<IEnumerable<MecanicoLookupDTO>>.Ok(new List<MecanicoLookupDTO>
            {
                new() { Id = Guid.NewGuid(), Nome = "Maria Mecânica" }
            }));

        var r = await ClienteAdmin().GetAsync("/api/mecanico/disponiveis");

        r.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await r.Content.ReadFromJsonAsync<List<MecanicoLookupDTO>>();
        lista.Should().ContainSingle(m => m.Nome == "Maria Mecânica");
    }

    [Fact]
    public async Task GetPorId_Existente_RetornaOk()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByIdAsync(id))
            .ReturnsAsync(Result<MecanicoDTO>.Ok(new MecanicoDTO { Id = id, Nome = "Carlos" }));

        var r = await ClienteAdmin().GetAsync($"/api/mecanico/{id}");

        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPorId_Inexistente_RetornaNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Result<MecanicoDTO>.Fail("Não encontrado"));

        var r = await ClienteAdmin().GetAsync($"/api/mecanico/{Guid.NewGuid()}");

        r.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Criar_DTOValido_RetornaCreated()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.AddAsync(It.IsAny<CriarMecanicoDTO>()))
            .ReturnsAsync(Result<Guid>.Ok(id));

        var dto = new CriarMecanicoDTO
        {
            Nome = "Novo Mec.",
            Email = "mec@x.com",
            Telefone = "11988888888",
            Senha = "Senha@123",
            Especialidade = "Motor",
            Nivel = "Pleno",
            DataContratacao = DateTime.Today
        };
        var r = await ClienteAdmin().PostAsJsonAsync("/api/mecanico", dto);

        r.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Criar_ServicoFalha_RetornaBadRequest()
    {
        _serviceMock.Setup(s => s.AddAsync(It.IsAny<CriarMecanicoDTO>()))
            .ReturnsAsync(Result<Guid>.Fail("Email duplicado"));

        var r = await ClienteAdmin().PostAsJsonAsync("/api/mecanico", new CriarMecanicoDTO());

        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Atualizar_Sucesso_RetornaNoContent()
    {
        _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<AtualizarMecanicoDTO>()))
            .ReturnsAsync(Result.Ok());

        var r = await ClienteAdmin().PutAsJsonAsync("/api/mecanico", new AtualizarMecanicoDTO
        {
            Id = Guid.NewGuid(), Email = "x@x.com", Telefone = "1", Especialidade = "y", Nivel = "Senior"
        });

        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Remover_Sucesso_RetornaNoContent()
    {
        _serviceMock.Setup(s => s.RemoveAsync(It.IsAny<Guid>())).ReturnsAsync(Result.Ok());

        var r = await ClienteAdmin().DeleteAsync($"/api/mecanico/{Guid.NewGuid()}");

        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Remover_Inexistente_RetornaNotFound()
    {
        _serviceMock.Setup(s => s.RemoveAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Result.Fail("Não existe"));

        var r = await ClienteAdmin().DeleteAsync($"/api/mecanico/{Guid.NewGuid()}");

        r.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
