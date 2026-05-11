using Moq;
using FluentAssertions;
using CarStoreManager.Application.DTOs.Shared.Cliente;
using CarStoreManager.Application.Services;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Tests.Unidade.Services;

/*
    Testes do fluxo de cadastro de cliente (cobrindo regressões
    de campos obrigatórios e validação de CPF).
*/
public class ClienteServiceCadastroTest
{
    private const string CpfValido = "11144477735";

    private readonly Mock<IClienteRepository> _repoMock = new();
    private readonly ClienteService _service;

    public ClienteServiceCadastroTest()
    {
        _service = new ClienteService(_repoMock.Object);
    }

    [Fact]
    public async Task AddAsync_DtoCompleto_CriaCliente()
    {
        _repoMock.Setup(r => r.CpfExisteAsync(It.IsAny<string>())).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Cliente>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new CriarClienteDTO
        {
            Nome = "João Silva",
            Cpf = CpfValido,
            Telefone = "11987654321",
            Email = "joao@email.com"
        };

        var result = await _service.AddAsync(dto);

        result.IsSuccess.Should().BeTrue();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Cliente>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_CpfInvalido_RetornaFalha()
    {
        _repoMock.Setup(r => r.CpfExisteAsync(It.IsAny<string>())).ReturnsAsync(false);

        var dto = new CriarClienteDTO
        {
            Nome = "João",
            Cpf = "00000000000",          // todos iguais → inválido
            Telefone = "11987654321",
            Email = "joao@x.com"
        };

        var result = await _service.AddAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("CPF");
    }

    [Fact]
    public async Task AddAsync_TelefoneVazio_RetornaFalha()
    {
        _repoMock.Setup(r => r.CpfExisteAsync(It.IsAny<string>())).ReturnsAsync(false);

        var dto = new CriarClienteDTO
        {
            Nome = "João",
            Cpf = CpfValido,
            Telefone = "",
            Email = "joao@x.com"
        };

        var result = await _service.AddAsync(dto);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_EmailVazio_RetornaFalha()
    {
        _repoMock.Setup(r => r.CpfExisteAsync(It.IsAny<string>())).ReturnsAsync(false);

        var dto = new CriarClienteDTO
        {
            Nome = "João",
            Cpf = CpfValido,
            Telefone = "11987654321",
            Email = ""
        };

        var result = await _service.AddAsync(dto);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_CpfJaCadastrado_RetornaFalha()
    {
        _repoMock.Setup(r => r.CpfExisteAsync(It.IsAny<string>())).ReturnsAsync(true);

        var dto = new CriarClienteDTO
        {
            Nome = "João",
            Cpf = CpfValido,
            Telefone = "11987654321",
            Email = "joao@x.com"
        };

        var result = await _service.AddAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("já cadastrado");
    }
}
