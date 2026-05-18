using Moq;
using FluentAssertions;
using Microsoft.Extensions.Options;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Auth;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Services;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Tests.Unidade.Services;

/*
    Este arquivo implementa os testes automaticos da classe AuthService.cs
    um VO do sistema, que inclui regra internas de funcionamento.

    Esta classe tem testes automaticos implementados para:
        Bloquear login com email e senha incorretos/vazios,
        Bloquear login com email inexistente,
        Bloquear login com senha incorreta,
        Bloquear login de usuario inativo,
        Validar login de usuario valido e retornar Token + Role,
        Bloquear criar novo usuario com email ja cadastrado,
        Valida criação de usuario role Admin,
        Valida criação de usuario role Vendedor,
        Valida criação de usuario role Mecanico,
        Bloquear usuarios nao encontrados,
        Valida cadastro de usuarios com senha correta,
        Bloquear tentativa de inativar usuario inexistente,
        Desativar e salvar usuario existente,
        Obter um usuario e retornar um DTO com suas infromações,
        Bloquear atualização de usuario inexistente,
        Validar atualização de usuario existente e salvar,
        Deve bloquear caso a senha atual esteja incorreta,
        Valida atualização e salva nova senha,
        Bloqueia logout de usuario inexistente,
        Valida logout de usuario
*/

public class AuthServiceTests
{
    /*
        estabelecendo contexto mockado para executar os testes
    */
    private readonly Mock<IUsuarioRepository> _repoMock;
    private readonly Mock<IJwtService> _jwtMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _repoMock = new Mock<IUsuarioRepository>();
        _jwtMock = new Mock<IJwtService>();
        var jwtSettings = Options.Create(new JwtSettings
        {
            SecretKey = "ChaveSuperSecreta123456",
            Issuer = "CarStore",
            Audience = "CarStore",
            ExpiracaoHoras = 8
        });
        _authService = new AuthService(_repoMock.Object, _jwtMock.Object, jwtSettings);
    }

    /*======================================
        Abaixo temos o inicio dos testes.
        Começando por testes de LOGIN.
     ======================================*/

    // Deve bloquear login com email e senha incorretos/vazios 
    [Fact]
    public async Task Login_EmailOuSenhaVazios_RetornaErro()
    {
        // verificação
        var result = await _authService.LoginAsync(new LoginDTO { Email = "", Senha = "123" });
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Email e senha são obrigatórios");
    }

    // Deve bloquear login com email inexistente
    [Fact]
    public async Task Login_EmailNaoExiste_RetornaErro()
    {
        // aplicação
        _repoMock.Setup(r => r.ObterPorEmailAsync("x@x.com")).ReturnsAsync((Usuario?)null);
        
        // verificação
        var result = await _authService.LoginAsync(new LoginDTO { Email = "x@x.com", Senha = "123" });
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Email ou senha inválidos");
    }

    // Deve bloquear login com senha incorreta
    [Fact]
    public async Task Login_SenhaIncorreta_RetornaErro()
    {
        // cenario
        var usuario = new Vendedor(
            "V", 
            "v@v.com", 
            "11999999999", 
            "Senha1",
            NivelFuncionario.Pleno,
            DateTime.UtcNow
        );

        // aplicação
        _repoMock.Setup(r => r.ObterPorEmailAsync("v@v.com")).ReturnsAsync(usuario);
        
        // verificação
        var result = await _authService.LoginAsync(new LoginDTO { Email = "v@v.com", Senha = "errada" });
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Email ou senha inválidos");
    }

    // Deve bloquear login de usuario inativo
    [Fact]
    public async Task Login_UsuarioInativo_RetornaErro()
    {
        // cenario
        var usuario = new Vendedor(
            "V", 
            "v@v.com", 
            "11999999999", 
            "Senha1", 
            NivelFuncionario.Pleno, 
            DateTime.UtcNow
        );

        // aplicação
        usuario.Desativar();
        _repoMock.Setup(r => r.ObterPorEmailAsync("v@v.com")).ReturnsAsync(usuario);

        // validação
        var result = await _authService.LoginAsync(new LoginDTO { Email = "v@v.com", Senha = "Senha1" });
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Usuário inativo");
    }

    //Deve permitir login de usuario valido e retonar Token + Role
    [Fact]
    public async Task Login_Valido_RetornaTokenERole()
    {
        // cenario
        var usuario = new Vendedor(
            "Ana", 
            "ana@v.com", 
            "11999999999", 
            "Senha1", 
            NivelFuncionario.Junior, 
            DateTime.UtcNow
        );

        // aplicação
        _repoMock.Setup(r => r.ObterPorEmailAsync("ana@v.com")).ReturnsAsync(usuario);
        _jwtMock.Setup(j => j.GerarToken(usuario)).Returns("token-ana");

        // validação
        var result = await _authService.LoginAsync(new LoginDTO { Email = "ana@v.com", Senha = "Senha1" });
        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("token-ana");
        result.Value.Nome.Should().Be("Ana");
        result.Value.Role.Should().Be("Vendedor");
        result.Value.Expiracao.Should().BeCloseTo(DateTime.UtcNow.AddHours(8), TimeSpan.FromMinutes(1));
    }

    /* ==================================================
        Abaixo seguem os testes para diferentes
        cenarios de CRIACAO DE USUARIO
     ==================================================*/

    //Deve bloquear criar novo usario com email ja cadastrado
    [Fact]
    public async Task CriarUsuario_EmailJaExiste_RetornaErro()
    {  
        // cenario
        _repoMock.Setup(r => r.EmailExisteAsync("existe@teste.com")).ReturnsAsync(true);
        var dto = new CriarUsuarioDTO { Email = "existe@teste.com", Role = "Vendedor" };

        // validação
        var result = await _authService.CriarUsuarioAsync(dto);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Email já cadastrado");
    }

    //Valida criação de usuario role Admin
    [Fact]
    public async Task CriarUsuario_Admin_Sucesso()
    {
        // cenario
        var dto = new CriarUsuarioDTO
        {
            Nome = "Carlos",
            Email = "carlos@v1.com",
            Telefone = "11988888888",
            Senha = "Senha@123",
            Role = "Admin",
        };

        // aplicação
        _repoMock.Setup(r => r.EmailExisteAsync(dto.Email)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Usuario>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // validação
        var result = await _authService.CriarUsuarioAsync(dto);
        result.IsSuccess.Should().BeTrue();
        _repoMock.Verify(r => r.AddAsync(It.Is<Admin>(v => v.Nome == "Carlos")), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    //Valida criação de usuario role Vendedor
    [Fact]
    public async Task CriarUsuario_Vendedor_Sucesso()
    {
        // cenario
        var dto = new CriarUsuarioDTO
        {
            Nome = "Carlos",
            Email = "carlos@v2.com",
            Telefone = "11988888888",
            Senha = "Senha@123",
            Role = "Vendedor",
            Nivel = "Pleno",
            DataContratacao = DateTime.UtcNow
        };

        // aplicação
        _repoMock.Setup(r => r.EmailExisteAsync(dto.Email)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Usuario>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // validação
        var result = await _authService.CriarUsuarioAsync(dto);
        result.IsSuccess.Should().BeTrue();
        _repoMock.Verify(r => r.AddAsync(It.Is<Vendedor>(v => v.Nome == "Carlos")), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    //Valida criação de usuario role Mecanico
    [Fact]
    public async Task CriarUsuario_Mecanico_Sucesso()
    {
        // cenario
        var dto = new CriarUsuarioDTO
        {
            Nome = "Jose Carlos",
            Email = "carlos@v3.com",
            Telefone = "11988888888",
            Senha = "Senha@123",
            Role = "Mecanico",
            Especialidade = "Funilaria",
            Nivel = "Pleno",
            DataContratacao = DateTime.UtcNow
        };
        
        // aplicação
        _repoMock.Setup(r => r.EmailExisteAsync(dto.Email)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Usuario>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // validação
        var result = await _authService.CriarUsuarioAsync(dto);
        result.IsSuccess.Should().BeTrue(string.IsNullOrEmpty(result.Error) ? "" : result.Error);
        _repoMock.Verify(r => r.AddAsync(It.Is<Mecanico>(v => v.Nome == "Jose Carlos")), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    /* ==================================================
        Abaixo seguem os testes para diferentes
        cenarios de VALIDACAO DE SENHA
     ==================================================*/

    //Deve bloquear usuarios nao encontrados
    [Fact]
    public async Task VerificarSenha_UsuarioNaoEncontrado_RetornaErro()
    {
        // aplicação
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Usuario?)null);
        
        // validação
        var result = await _authService.VerificarSenhaAsync(Guid.NewGuid(), "Senha1");
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Usuário não encontrado");
    }

    //Valida cadastro de usuarios com senha correta
    [Fact]
    public async Task VerificarSenha_SenhaCorreta_RetornaSucesso()
    {
        // cenario
        var usuario = new Vendedor(
            "X", 
            "x@x.com", 
            "11999999999", 
            "Senha1", 
            NivelFuncionario.Junior,
            DateTime.UtcNow
        );

        // aplicação
        _repoMock.Setup(r => r.GetByIdAsync(usuario.Id)).ReturnsAsync(usuario);

        // validação
        var result = await _authService.VerificarSenhaAsync(usuario.Id, "Senha1");
        result.IsSuccess.Should().BeTrue();
    }

    /* ==================================================
        Abaixo seguem os testes para diferentes
        cenarios de ATIVAR E DESATIVAR USUARIO
     ==================================================*/

    //Deve bloquear tentativa de inativar usuario inexistente
    [Fact]
    public async Task DesativarUsuario_Inexistente_RetornaErro()
    {
        // aplicação
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Usuario?)null);
        
        // validação
        var result = await _authService.DesativarUsuarioAsync(Guid.NewGuid());
        result.IsSuccess.Should().BeFalse();
    }

    //Deve permitir desativar e salvar usuario existente
    [Fact]
    public async Task DesativarUsuario_Existente_DesativaESalva()
    {
        // cenario
        var usuario = new Vendedor(
            "V", 
            "v@v.com", 
            "11999999999", 
            "Senha1", 
            NivelFuncionario.Senior, 
            DateTime.UtcNow
        );

        // aplicação
        _repoMock.Setup(r => r.GetByIdAsync(usuario.Id)).ReturnsAsync(usuario);
        _repoMock.Setup(r => r.Update(usuario)).Verifiable();
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // validação
        var result = await _authService.DesativarUsuarioAsync(usuario.Id);
        result.IsSuccess.Should().BeTrue();
        usuario.Ativo.Should().BeFalse();
        _repoMock.Verify(r => r.Update(usuario), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    /* ==================================================
        Abaixo seguem os testes para
        cenarios de OBTER USUARIO
     ==================================================*/

    //Deve obter um usuario e retornar um DTO com suas infromações
    [Fact]
    public async Task ObterUsuario_Existente_RetornaDTO()
    {
        // cenario
        var usuario = new Vendedor(
            "João", 
            "joao@email.com", 
            "11988888888", 
            "Senha1",
            NivelFuncionario.Junior, 
            DateTime.UtcNow
        );

        // aplicação
        _repoMock.Setup(r => r.GetByIdAsync(usuario.Id)).ReturnsAsync(usuario);

        // validação
        var result = await _authService.ObterUsuarioAsync(usuario.Id);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Nome.Should().Be("João");
        result.Value.Email.Should().Be("joao@email.com");
        Assert.Equal("(11) 98888-8888", result.Value.Telefone);
    }

    /* ==================================================
        Abaixo seguem os testes para diferentes
        cenarios de ATUALIZAR USUARIO
     ==================================================*/

    //Bloquear atualização de usuario inexistente
    [Fact]
    public async Task AtualizarUsuario_Inexistente_RetornaErro()
    {
        // aplicação
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Usuario?)null);
        var dto = new AtualizarUsuarioDTO { Nome = "", Email = "", Telefone = "" };
        
        // validação
        var result = await _authService.AtualizarUsuarioAsync(Guid.NewGuid(), dto);
        result.IsSuccess.Should().BeFalse();
    }

    //Validar atualização de usuario existente e salvar
    [Fact]
    public async Task AtualizarUsuario_Valido_AtualizaESalva()
    {
        // cenario
        var usuario = new Vendedor(
            "Antigo", 
            "antigo@email.com", 
            "11912345678", 
            "Senha1", 
            NivelFuncionario.Junior, 
            DateTime.UtcNow
        );
        
        // aplicação
        _repoMock.Setup(r => r.GetByIdAsync(usuario.Id)).ReturnsAsync(usuario);
        _repoMock.Setup(r => r.Update(usuario)).Verifiable();
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var dto = new AtualizarUsuarioDTO { Nome = "Novo Nome", Email = "novo@email.com", Telefone = "11987654321" };

        // validação
        var result = await _authService.AtualizarUsuarioAsync(usuario.Id, dto);
        result.IsSuccess.Should().BeTrue();
        usuario.GetNome().Should().Be("Novo Nome");
        usuario.GetEmail().Should().Be("novo@email.com");
        Assert.Equal("(11) 98765-4321", usuario.GetTelefone());
        _repoMock.Verify(r => r.Update(usuario), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    /* ==================================================
        Abaixo seguem os testes para diferentes
        cenarios de ALTERAR DE SENHA
     ==================================================*/

    //Deve bloquear caso a senha atual esteja incorreta
    [Fact]
    public async Task AlterarSenha_SenhaAtualIncorreta_RetornaErro()
    {
        // cenario
        var usuario = new Vendedor(
            "X", 
            "x@x.com", 
            "11999999999", 
            "Senha1", 
            NivelFuncionario.Junior, 
            DateTime.UtcNow
        );

        // aplicação
        _repoMock.Setup(r => r.GetByIdAsync(usuario.Id)).ReturnsAsync(usuario);

        // validação
        var result = await _authService.AlterarSenhaAsync(usuario.Id, "errada", "nova");
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Senha atual incorreta");
    }

    //Valida atualização e salva nova senha
    [Fact]
    public async Task AlterarSenha_Valido_AtualizaHashESalva()
    {
        // cenario
        var usuario = new Vendedor(
            "X", 
            "x@x.com", 
            "11999999999", 
            "Senha1", 
            NivelFuncionario.Junior, 
            DateTime.UtcNow
        );

        // aplicação
        _repoMock.Setup(r => r.GetByIdAsync(usuario.Id)).ReturnsAsync(usuario);
        _repoMock.Setup(r => r.Update(usuario)).Verifiable();
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // validação
        var result = await _authService.AlterarSenhaAsync(usuario.Id, "Senha1", "Senha2");
        result.IsSuccess.Should().BeTrue();
        BCrypt.Net.BCrypt.Verify("Senha2", usuario.GetSenhaHash()).Should().BeTrue();
    }

    /* ==================================================
        Abaixo seguem os testes para diferentes
        cenarios de LOGOUT
     ==================================================*/

    //Bloqueia logout de usuario inexistente
    [Fact]
    public async Task Logout_UsuarioInexistente_RetornaErro()
    {
        // aplicação
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Usuario?)null);
        
        // validação
        var result = await _authService.LogoutAsync(Guid.NewGuid());
        result.IsSuccess.Should().BeFalse();
    }

    //Valida logout de usuario
    [Fact]
    public async Task Logout_UsuarioExistente_RetornaSucesso()
    {
        // cenario
        var usuario = new Vendedor(
            "X", 
            "x@x.com", 
            "11999999999", 
            "Senha1", 
            NivelFuncionario.Junior, 
            DateTime.UtcNow
        );

        // aplicação
        _repoMock.Setup(r => r.GetByIdAsync(usuario.Id)).ReturnsAsync(usuario);
        
        // validação
        var result = await _authService.LogoutAsync(usuario.Id);
        result.IsSuccess.Should().BeTrue();
    }
}