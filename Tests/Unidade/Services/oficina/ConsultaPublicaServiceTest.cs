using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using CarStoreManager.Application.Services;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using Xunit;

namespace CarStoreManager.Tests.Unidade.Services.Oficina;

public class ConsultaPublicaServiceTest
{
    private readonly Mock<IClienteRepository> _cli = new();
    private readonly Mock<IVeiculoClienteRepository> _veic = new();
    private readonly Mock<IOrdemServicoRepository> _os = new();
    private readonly ConsultaPublicaService _service;

    private const string Cpf = "39053344705";

    public ConsultaPublicaServiceTest()
        => _service = new ConsultaPublicaService(_cli.Object, _veic.Object, _os.Object);

    private static Cliente ClienteValido() => new(
        "Maria Silva Souza", "maria@x.com", "11988887777", Cpf,
        new Endereco("Rua", "1", null, "Centro", "Cidade", "SP", "01001000"));

    private static VeiculoCliente VeiculoDoCliente(Guid clienteId, string placa = "ABC1D23")
        => new(clienteId, "Fiat", "Uno", "Branco", 2015, placa);

    [Fact]
    public async Task ConsultarPorCpfEPlaca_CamposVazios_Falha()
    {
        var r = await _service.ConsultarPorCpfEPlacaAsync("", "");
        r.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ConsultarPorCpfEPlaca_CpfInexistente_FalhaGenerica()
    {
        _cli.Setup(x => x.ObterPorCpfAsync(It.IsAny<string>())).ReturnsAsync((Cliente?)null);

        var r = await _service.ConsultarPorCpfEPlacaAsync(Cpf, "ABC1D23");

        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Be("Não encontramos nenhum cadastro com esse CPF e placa. Confira os dados e tente novamente.");
    }

    [Fact]
    public async Task ConsultarPorCpfEPlaca_PlacaNaoPertenceAoCliente_Falha()
    {
        var cliente = ClienteValido();
        _cli.Setup(x => x.ObterPorCpfAsync(It.IsAny<string>())).ReturnsAsync(cliente);
        _veic.Setup(x => x.ObterPorClienteAsync(cliente.Id))
             .ReturnsAsync(new[] { VeiculoDoCliente(cliente.Id, "XYZ9Z99") });

        var r = await _service.ConsultarPorCpfEPlacaAsync(Cpf, "ABC1D23");

        r.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ConsultarPorCpfEPlaca_CpfMaisPlacaConferem_RetornaResultado()
    {
        var cliente = ClienteValido();
        var veiculo = VeiculoDoCliente(cliente.Id, "ABC1D23");
        _cli.Setup(x => x.ObterPorCpfAsync(It.IsAny<string>())).ReturnsAsync(cliente);
        _veic.Setup(x => x.ObterPorClienteAsync(cliente.Id)).ReturnsAsync(new[] { veiculo });
        _os.Setup(x => x.ObterPorClienteAsync(cliente.Id))
           .ReturnsAsync(new[] { OsDoVeiculo(veiculo.Id, cliente.Id) });

        var r = await _service.ConsultarPorCpfEPlacaAsync(Cpf, "abc-1d23"); // formatação livre

        r.IsSuccess.Should().BeTrue();
        r.Value!.PrimeiroNome.Should().Be("Maria");
        r.Value.Veiculos.Should().ContainSingle();
        r.Value.Veiculos[0].Historico.Should().ContainSingle();
        r.Value.OrdemEmAberto.Should().NotBeNull(); // a OS criada está "Pendente"
    }

    private static OrdemServico OsDoVeiculo(Guid veiculoId, Guid clienteId)
        => new(veiculoId, Guid.NewGuid(), clienteId, TipoServico.Revisao,
               "Revisão", DateTime.UtcNow.AddDays(2), 150m);
}
