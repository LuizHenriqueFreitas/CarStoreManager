using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using CarStoreManager.Application.Services;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using Xunit;

namespace CarStoreManager.Tests.Unidade.Services;

public class ClienteConsultaServiceTest
{
    private readonly Mock<IClienteRepository> _cli = new();
    private readonly Mock<IVeiculoClienteRepository> _veic = new();
    private readonly Mock<IOrdemServicoRepository> _os = new();
    private readonly Mock<IPropostaVendaRepository> _prop = new();
    private readonly ClienteConsultaService _service;

    public ClienteConsultaServiceTest()
        => _service = new ClienteConsultaService(_cli.Object, _veic.Object, _os.Object, _prop.Object);

    private static Cliente Cliente() => new(
        "João Carlos Pereira", "j@x.com", "11988887777", "39053344705",
        new Endereco("Rua", "1", null, "Centro", "Cidade", "SP", "01001000"));

    [Fact]
    public async Task ObterFichaAsync_ClienteInexistente_Falha()
    {
        _cli.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);

        var r = await _service.ObterFichaAsync(Guid.NewGuid());

        r.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ObterFichaAsync_AgregaOsPropostasEVeiculos()
    {
        var cliente = Cliente();
        var veic = new VeiculoCliente(cliente.Id, "Fiat", "Uno", "Branco", 2015, "ABC1D23");
        var os = new OrdemServico(veic.Id, Guid.NewGuid(), cliente.Id, TipoServico.Revisao,
            "Revisão", DateTime.UtcNow.AddDays(2), 300m);

        _cli.Setup(x => x.GetByIdAsync(cliente.Id)).ReturnsAsync(cliente);
        _veic.Setup(x => x.ObterPorClienteAsync(cliente.Id)).ReturnsAsync(new[] { veic });
        _os.Setup(x => x.ObterPorClienteAsync(cliente.Id)).ReturnsAsync(new[] { os });
        _prop.Setup(x => x.ObterPorClienteAsync(cliente.Id)).ReturnsAsync(Array.Empty<PropostaVenda>());

        var r = await _service.ObterFichaAsync(cliente.Id);

        r.IsSuccess.Should().BeTrue();
        var ficha = r.Value!;
        ficha.Nome.Should().Be("João Carlos Pereira");
        ficha.TotalOrdensServico.Should().Be(1);
        ficha.OrdensServicoAbertas.Should().Be(1);
        ficha.Veiculos.Should().ContainSingle();
        ficha.Veiculos[0].Atendimentos.Should().Be(1);
        ficha.HistoricoOficina.Should().ContainSingle();
        ficha.UltimoAtendimento.Should().NotBeNull();
        ficha.UltimoAtendimentoSetor.Should().Be("Oficina");
    }
}
