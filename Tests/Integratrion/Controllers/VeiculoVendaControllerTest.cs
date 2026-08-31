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
using CarStoreManager.Application.DTOs.Concessionaria.VeiculoVenda;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Tests.Integratrion.Helpers;
using CarStoreManager.Web;

namespace CarStoreManager.Tests.Integratrion.Controllers;

/// <summary>
/// Testes de integração do VeiculoVendaController.
///
/// O controller tem 3 níveis de autorização sobrepostos:
///   • Leitura       → Admin, GerenteVendas, Vendedor
///   • Escrita base  → Admin, GerenteVendas
///   • Marcações operacionais (vendido/disponível/km) → Admin, GerenteVendas, Vendedor
///
/// Testamos cada combinação para garantir que vendedor não consiga, por exemplo,
/// criar/excluir veículo, e que mecânico/recepcionista não tenha acesso algum.
/// </summary>
public class VeiculoVendaControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IVeiculoVendaService> _serviceMock;

    public VeiculoVendaControllerTests(WebApplicationFactory<Program> factory)
    {
        _serviceMock = new Mock<IVeiculoVendaService>();
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // "Testing" faz o Program.cs pular migrate/seed do banco real —
            // esses testes mockam o service, nunca tocam o AppDbContext de verdade.
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IVeiculoVendaService>();
                services.AddScoped(_ => _serviceMock.Object);

                services.AddTestAuth();
            });
        });
    }

    private HttpClient ClienteCom(string role)
    {
        var c = _factory.CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", role);
        return c;
    }

    // ==================== LEITURA ====================

    [Theory]
    [InlineData("Admin")]
    [InlineData("GerenteVendas")]
    [InlineData("Vendedor")]
    public async Task GetTodos_RolesPermitidas_RetornaOk(string role)
    {
        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result<IEnumerable<VeiculoVendaListaDTO>>.Ok(new List<VeiculoVendaListaDTO>()));

        var r = await ClienteCom(role).GetAsync("/api/veiculovenda");

        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("Mecanico")]
    [InlineData("Recepcionista")]
    [InlineData("ChefeOficina")]
    public async Task GetTodos_RolesProibidas_RetornaForbidden(string role)
    {
        var r = await ClienteCom(role).GetAsync("/api/veiculovenda");
        r.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDisponiveis_RetornaOk()
    {
        _serviceMock.Setup(s => s.ObterDisponiveisAsync())
            .ReturnsAsync(Result<IEnumerable<VeiculoVendaListaDTO>>.Ok(new List<VeiculoVendaListaDTO>()));

        var r = await ClienteCom("Vendedor").GetAsync("/api/veiculovenda/disponiveis");

        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListarMarcas_RetornaListaDeStrings()
    {
        _serviceMock.Setup(s => s.ListarMarcasDistintasAsync())
            .ReturnsAsync(Result<IEnumerable<string>>.Ok(new[] { "Fiat", "Volkswagen" }));

        var r = await ClienteCom("Vendedor").GetAsync("/api/veiculovenda/marcas");

        r.StatusCode.Should().Be(HttpStatusCode.OK);
        (await r.Content.ReadFromJsonAsync<List<string>>())
            .Should().BeEquivalentTo(new[] { "Fiat", "Volkswagen" });
    }

    [Fact]
    public async Task GetPorId_Existente_RetornaOk()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByIdAsync(id))
            .ReturnsAsync(Result<VeiculoVendaDTO>.Ok(new VeiculoVendaDTO { Id = id, Marca = "Fiat" }));

        var r = await ClienteCom("Vendedor").GetAsync($"/api/veiculovenda/{id}");

        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPorId_Inexistente_RetornaNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Result<VeiculoVendaDTO>.Fail("Veículo não encontrado"));

        var r = await ClienteCom("Vendedor").GetAsync($"/api/veiculovenda/{Guid.NewGuid()}");

        r.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ==================== ESCRITA (Admin/Gerente) ====================

    [Fact]
    public async Task Criar_ComoAdmin_RetornaCreated()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.AddAsync(It.IsAny<CriarVeiculoVendaDTO>()))
            .ReturnsAsync(Result<Guid>.Ok(id));

        var r = await ClienteCom("Admin").PostAsJsonAsync("/api/veiculovenda", new CriarVeiculoVendaDTO
        {
            Marca = "Fiat", Modelo = "Uno", Cor = "Branco", Motorizacao = "1.0",
            Ano = 2020, Quilometragem = 1000, Placa = "ABC1D23", Renavam = "12345678901",
            Cambio = "Manual", Combustivel = "Flex", Valor = 35000m,
            ValorAquisicao = 28000m, AnoUltimoIpvaPago = 2024
        });

        r.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Criar_ComoVendedor_RetornaForbidden()
    {
        var r = await ClienteCom("Vendedor")
            .PostAsJsonAsync("/api/veiculovenda", new CriarVeiculoVendaDTO());

        r.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Remover_ComoVendedor_RetornaForbidden()
    {
        var r = await ClienteCom("Vendedor").DeleteAsync($"/api/veiculovenda/{Guid.NewGuid()}");
        r.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Remover_ComoAdmin_Sucesso_RetornaNoContent()
    {
        _serviceMock.Setup(s => s.RemoveAsync(It.IsAny<Guid>())).ReturnsAsync(Result.Ok());

        var r = await ClienteCom("Admin").DeleteAsync($"/api/veiculovenda/{Guid.NewGuid()}");
        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ==================== AÇÕES OPERACIONAIS ====================

    [Fact]
    public async Task MarcarComoVendido_ComoVendedor_Sucesso()
    {
        _serviceMock.Setup(s => s.MarcarComoVendidoAsync(It.IsAny<Guid>())).ReturnsAsync(Result.Ok());

        var r = await ClienteCom("Vendedor")
            .PatchAsync($"/api/veiculovenda/{Guid.NewGuid()}/vendido", null);

        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task MarcarComoDisponivel_ServicoFalha_RetornaBadRequest()
    {
        _serviceMock.Setup(s => s.MarcarComoDisponivelAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Result.Fail("Veículo já está disponível"));

        var r = await ClienteCom("Admin")
            .PatchAsync($"/api/veiculovenda/{Guid.NewGuid()}/disponivel", null);

        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LiberarParaVenda_ComoVendedor_RetornaForbidden()
    {
        // Só Admin/GerenteVendas podem liberar
        var r = await ClienteCom("Vendedor")
            .PatchAsync($"/api/veiculovenda/{Guid.NewGuid()}/liberar-venda", null);

        r.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task LiberarParaVenda_ComoGerente_Sucesso()
    {
        _serviceMock.Setup(s => s.LiberarParaVendaAsync(It.IsAny<Guid>())).ReturnsAsync(Result.Ok());

        var r = await ClienteCom("GerenteVendas")
            .PatchAsync($"/api/veiculovenda/{Guid.NewGuid()}/liberar-venda", null);

        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AtualizarQuilometragem_ComoVendedor_Sucesso()
    {
        _serviceMock.Setup(s => s.AtualizarQuilometragemAsync(It.IsAny<Guid>(), 12345))
            .ReturnsAsync(Result.Ok());

        var r = await ClienteCom("Vendedor")
            .PatchAsJsonAsync($"/api/veiculovenda/{Guid.NewGuid()}/quilometragem", 12345);

        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AdicionarFoto_DTOValido_RetornaNoContent()
    {
        _serviceMock.Setup(s => s.AdicionarFotoAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Ok());

        var r = await ClienteCom("Vendedor")
            .PostAsJsonAsync($"/api/veiculovenda/{Guid.NewGuid()}/fotos",
                new AdicionarFotoDTO { Url = "https://x.com/foto.jpg" });

        r.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoverFoto_FotoInexistente_RetornaNotFound()
    {
        _serviceMock.Setup(s => s.RemoverFotoAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(Result.Fail("Foto não encontrada"));

        var r = await ClienteCom("Admin")
            .DeleteAsync($"/api/veiculovenda/{Guid.NewGuid()}/fotos/{Guid.NewGuid()}");

        r.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
