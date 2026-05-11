using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;
using FluentAssertions;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Auth;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Web;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;

namespace CarStoreManager.Tests.Web.Controllers
{
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<IAuthService> _authServiceMock;

        public AuthControllerTests(WebApplicationFactory<Program> factory)
        {
            _authServiceMock = new Mock<IAuthService>();
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Remove a implementação real e injeta o mock
                    services.RemoveAll<IAuthService>();
                    services.AddScoped(_ => _authServiceMock.Object);

                    // Configura autenticação de teste para simular roles
                    services.AddAuthentication(defaultScheme: "Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Test", options => { });
                });
            });
        }

        // ==================== LOGIN ====================

        [Fact]
        public async Task Login_CredenciaisValidas_RetornaOkComToken()
        {
            // Arrange
            var loginDto = new LoginDTO { Email = "admin@teste.com", Senha = "Admin@123" };
            var loginResult = new LoginResultDTO
            {
                Token = "token-jwt-simulado",
                Nome = "Admin",
                Role = "Admin",
                Expiracao = DateTime.UtcNow.AddHours(8)
            };
            _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginDTO>()))
                .ReturnsAsync(Result<LoginResultDTO>.Ok(loginResult));

            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadFromJsonAsync<LoginResultDTO>();
            content!.Token.Should().Be("token-jwt-simulado");
            content.Nome.Should().Be("Admin");
        }

        [Fact]
        public async Task Login_CredenciaisInvalidas_RetornaUnauthorized()
        {
            // Arrange
            _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginDTO>()))
                .ReturnsAsync(Result<LoginResultDTO>.Fail("Email ou senha inválidos"));

            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/login",
                new LoginDTO { Email = "x@x.com", Senha = "errada" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ==================== CRIAR USUÁRIO ====================

        [Fact]
        public async Task CriarUsuario_ComRoleAdmin_RetornaOk()
        {
            // Arrange
            var dto = new CriarUsuarioDTO
            {
                Nome = "Novo Vendedor",
                Email = "vendedor@teste.com",
                Telefone = "11988888888",
                Senha = "Senha@123",
                Role = "Vendedor",
                Nivel = "Junior",
                DataContratacao = DateTime.Today
            };
            _authServiceMock.Setup(s => s.CriarUsuarioAsync(It.IsAny<CriarUsuarioDTO>()))
                .ReturnsAsync(Result<Guid>.Ok(Guid.NewGuid()));

            var client = _factory.CreateClient();
            // Adiciona token de autenticação com claim Admin
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Test", "Admin");

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/usuarios", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var id = await response.Content.ReadFromJsonAsync<Guid>();
            id.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CriarUsuario_SemAutenticacao_RetornaUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient(); // sem header
            var dto = new CriarUsuarioDTO();

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/usuarios", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CriarUsuario_AutenticadoMasNaoAdmin_RetornaForbidden()
        {
            // Arrange
            var dto = new CriarUsuarioDTO();
            _authServiceMock.Setup(s => s.CriarUsuarioAsync(It.IsAny<CriarUsuarioDTO>()))
                .ReturnsAsync(Result<Guid>.Ok(Guid.NewGuid()));

            var client = _factory.CreateClient();
            // Autentica como Vendedor (não Admin)
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Test", "Vendedor");

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/usuarios", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CriarUsuario_DadosInvalidos_RetornaBadRequest()
        {
            // Arrange
            var dto = new CriarUsuarioDTO(); // campos obrigatórios vazios
            _authServiceMock.Setup(s => s.CriarUsuarioAsync(It.IsAny<CriarUsuarioDTO>()))
                .ReturnsAsync(Result<Guid>.Fail("Email já cadastrado"));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Test", "Admin");

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/usuarios", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}

// Handler de autenticação de teste que converte o token em claims
internal class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Test "))
            return Task.FromResult(AuthenticateResult.Fail("Missing or invalid header"));

        var role = authHeader["Test ".Length..];
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}