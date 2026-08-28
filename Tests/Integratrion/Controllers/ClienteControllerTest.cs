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
using CarStoreManager.Application.DTOs.Shared.Cliente;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Tests.Integratrion.Helpers;
using CarStoreManager.Web;

namespace CarStoreManager.Tests.Integratrion.Controllers;

/// <summary>
/// Testes de integração do ClienteController.
///
/// Mocka o IClienteService — interessa apenas a tradução HTTP do controller
/// (verbos, status codes, papéis exigidos pelos atributos [Authorize]).
/// A regra de negócio em si vive em ClienteServiceTest (unidade).
/// </summary>
public class ClienteControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IClienteService> _serviceMock;

    public ClienteControllerTests(WebApplicationFactory<Program> factory)
    {
        _serviceMock = new Mock<IClienteService>();
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // "Testing" faz o Program.cs pular migrate/seed do banco real —
            // esses testes mockam o service, nunca tocam o AppDbContext de verdade.
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IClienteService>();
                services.AddScoped(_ => _serviceMock.Object);

                services.AddTestAuth();
            });
        });
    }

    private HttpClient ClienteAutenticado(string role = "Admin")
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", role);
        return client;
    }

    // ==================== GET TODOS ====================

    [Fact]
    public async Task GetTodos_Autenticado_RetornaOkComLista()
    {
        var lista = new List<ClienteListaDTO>
        {
            new() { Id = Guid.NewGuid(), Nome = "Maria", Cpf = "11144477735" }
        };
        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result<IEnumerable<ClienteListaDTO>>.Ok(lista));

        var response = await ClienteAutenticado().GetAsync("/api/cliente");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<ClienteListaDTO>>();
        body.Should().HaveCount(1);
        body![0].Nome.Should().Be("Maria");
    }

    [Fact]
    public async Task GetTodos_SemAutenticacao_RetornaUnauthorized()
    {
        var response = await _factory.CreateClient().GetAsync("/api/cliente");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTodos_ServicoFalha_RetornaBadRequest()
    {
        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result<IEnumerable<ClienteListaDTO>>.Fail("Falha no banco"));

        var response = await ClienteAutenticado().GetAsync("/api/cliente");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ==================== GET POR ID ====================

    [Fact]
    public async Task GetPorId_ClienteExistente_RetornaOk()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByIdAsync(id))
            .ReturnsAsync(Result<ClienteDTO>.Ok(new ClienteDTO { Id = id, Nome = "João" }));

        var response = await ClienteAutenticado().GetAsync($"/api/cliente/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ClienteDTO>();
        body!.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetPorId_ClienteInexistente_RetornaNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Result<ClienteDTO>.Fail("Cliente não encontrado"));

        var response = await ClienteAutenticado().GetAsync($"/api/cliente/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ==================== GET POR CPF ====================

    [Fact]
    public async Task GetPorCpf_CpfExistente_RetornaOk()
    {
        const string cpf = "11144477735";
        _serviceMock.Setup(s => s.ObterPorCpfAsync(cpf))
            .ReturnsAsync(Result<ClienteDTO>.Ok(new ClienteDTO { Cpf = cpf }));

        var response = await ClienteAutenticado().GetAsync($"/api/cliente/cpf/{cpf}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPorCpf_NaoEncontrado_RetornaNotFound()
    {
        _serviceMock.Setup(s => s.ObterPorCpfAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<ClienteDTO>.Fail("Cliente não encontrado"));

        var response = await ClienteAutenticado().GetAsync("/api/cliente/cpf/00000000000");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ==================== PESQUISAR ====================

    [Fact]
    public async Task Pesquisar_ComTermo_RetornaResultados()
    {
        _serviceMock.Setup(s => s.PesquisarAsync("Mar"))
            .ReturnsAsync(Result<List<ClienteListaDTO>>.Ok(new List<ClienteListaDTO>
            {
                new() { Nome = "Maria" }
            }));

        var response = await ClienteAutenticado().GetAsync("/api/cliente/termo?termo=Mar");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<ClienteListaDTO>>();
        body.Should().ContainSingle(c => c.Nome == "Maria");
    }

    // ==================== CRIAR ====================

    [Fact]
    public async Task Criar_DTOValido_RetornaCreatedAtAction()
    {
        var novoId = Guid.NewGuid();
        _serviceMock.Setup(s => s.AddAsync(It.IsAny<CriarClienteDTO>()))
            .ReturnsAsync(Result<Guid>.Ok(novoId));

        var dto = new CriarClienteDTO
        {
            Nome = "João",
            Cpf = "11144477735",
            Email = "joao@x.com",
            Telefone = "11999990000"
        };
        var response = await ClienteAutenticado().PostAsJsonAsync("/api/cliente", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain(novoId.ToString());
    }

    [Fact]
    public async Task Criar_ServicoFalha_RetornaBadRequest()
    {
        _serviceMock.Setup(s => s.AddAsync(It.IsAny<CriarClienteDTO>()))
            .ReturnsAsync(Result<Guid>.Fail("CPF já cadastrado"));

        var response = await ClienteAutenticado()
            .PostAsJsonAsync("/api/cliente", new CriarClienteDTO());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ==================== ATUALIZAR ====================

    [Fact]
    public async Task Atualizar_DTOValido_RetornaNoContent()
    {
        _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<AtualizarClienteDTO>()))
            .ReturnsAsync(Result.Ok());

        var id = Guid.NewGuid();
        var dto = new AtualizarClienteDTO { Nome = "Maria atualizada" };

        var response = await ClienteAutenticado().PutAsJsonAsync($"/api/cliente/{id}", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        // Confirma que o controller injeta o id da rota no DTO
        _serviceMock.Verify(s => s.UpdateAsync(It.Is<AtualizarClienteDTO>(d => d.Id == id)), Times.Once);
    }

    [Fact]
    public async Task Atualizar_ServicoFalha_RetornaBadRequest()
    {
        _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<AtualizarClienteDTO>()))
            .ReturnsAsync(Result.Fail("Email inválido"));

        var response = await ClienteAutenticado()
            .PutAsJsonAsync($"/api/cliente/{Guid.NewGuid()}", new AtualizarClienteDTO());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ==================== REMOVER ====================

    [Fact]
    public async Task Remover_ComoAdmin_RetornaNoContent()
    {
        _serviceMock.Setup(s => s.RemoveAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Result.Ok());

        var response = await ClienteAutenticado("Admin")
            .DeleteAsync($"/api/cliente/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Theory]
    [InlineData("Vendedor")]
    [InlineData("Recepcionista")]
    [InlineData("Mecanico")]
    public async Task Remover_RoleSemPermissao_RetornaForbidden(string role)
    {
        var response = await ClienteAutenticado(role)
            .DeleteAsync($"/api/cliente/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Remover_ClienteInexistente_RetornaNotFound()
    {
        _serviceMock.Setup(s => s.RemoveAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Result.Fail("Cliente não encontrado"));

        var response = await ClienteAutenticado("Admin")
            .DeleteAsync($"/api/cliente/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
