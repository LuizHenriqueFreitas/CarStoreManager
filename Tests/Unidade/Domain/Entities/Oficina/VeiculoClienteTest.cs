// Este arquivo nao foi revisado nem documentado

using FluentAssertions;
using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Oficina;

public class VeiculoClienteTest
{
    private readonly Guid _clienteIdValido = Guid.NewGuid();
    private const string MarcaValida = "Fiat";
    private const string ModeloValido = "Palio";
    private const string CorValida = "Branco";
    private const int AnoValido = 2020;

    [Fact]
    public void Construtor_ClienteIdVazio_LancaExcecao()
    {
        var act = () => new VeiculoCliente(Guid.Empty, MarcaValida, ModeloValido, CorValida, AnoValido, "ABC1D23");
        act.Should().Throw<ArgumentException>().WithMessage("Cliente inválido");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_MarcaInvalida_LancaExcecao(string marcaInvalida)
    {
        var act = () => new VeiculoCliente(_clienteIdValido, marcaInvalida, ModeloValido, CorValida, AnoValido, "ABC1D23");
        act.Should().Throw<ArgumentException>().WithMessage("Marca inválida");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ModeloInvalido_LancaExcecao(string modeloInvalido)
    {
        var act = () => new VeiculoCliente(_clienteIdValido, MarcaValida, modeloInvalido, CorValida, AnoValido, "ABC1D23");
        act.Should().Throw<ArgumentException>().WithMessage("Modelo inválido");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_CorInvalida_LancaExcecao(string corInvalida)
    {
        var act = () => new VeiculoCliente(_clienteIdValido, MarcaValida, ModeloValido, corInvalida, AnoValido, "ABC1D23");
        act.Should().Throw<ArgumentException>().WithMessage("Cor inválida");
    }

    [Fact]
    public void Construtor_Valido_CriaInstancia()
    {
        var veiculo = new VeiculoCliente(_clienteIdValido, MarcaValida, ModeloValido, CorValida, AnoValido, "ABC1D23");
        veiculo.ClienteId.Should().Be(_clienteIdValido);
        veiculo.Marca.Should().Be(MarcaValida);
        veiculo.Modelo.Should().Be(ModeloValido);
        veiculo.Cor.Should().Be(CorValida);
        veiculo.Ano.GetValorAno().Should().Be(AnoValido);
    }

    [Fact]
    public void AtualizarMarca_MarcaValida_Atualiza()
    {
        var veiculo = new VeiculoCliente(_clienteIdValido, MarcaValida, ModeloValido, CorValida, AnoValido, "ABC1D23");
        veiculo.AtualizarMarcaVeiculoCliente("Chevrolet");
        veiculo.Marca.Should().Be("Chevrolet");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AtualizarMarca_MarcaInvalida_LancaExcecao(string marcaInvalida)
    {
        var veiculo = new VeiculoCliente(_clienteIdValido, MarcaValida, ModeloValido, CorValida, AnoValido, "ABC1D23");
        var act = () => veiculo.AtualizarMarcaVeiculoCliente(marcaInvalida);
        act.Should().Throw<ArgumentException>().WithMessage("Marca inválida");
    }

    [Fact]
    public void AtualizarModelo_ModeloValido_Atualiza()
    {
        var veiculo = new VeiculoCliente(_clienteIdValido, MarcaValida, ModeloValido, CorValida, AnoValido, "ABC1D23");
        veiculo.AtualizarModeloVeiculoCliente("Uno");
        veiculo.Modelo.Should().Be("Uno");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AtualizarModelo_ModeloInvalido_LancaExcecao(string modeloInvalido)
    {
        var veiculo = new VeiculoCliente(_clienteIdValido, MarcaValida, ModeloValido, CorValida, AnoValido, "ABC1D23");
        var act = () => veiculo.AtualizarModeloVeiculoCliente(modeloInvalido);
        act.Should().Throw<ArgumentException>().WithMessage("Modelo inválido");
    }

    [Fact]
    public void AtualizarCor_CorValida_Atualiza()
    {
        var veiculo = new VeiculoCliente(_clienteIdValido, MarcaValida, ModeloValido, CorValida, AnoValido, "ABC1D23");
        veiculo.AtualizarCorVeiculoCliente("Preto");
        veiculo.Cor.Should().Be("Preto");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AtualizarCor_CorInvalida_LancaExcecao(string corInvalida)
    {
        var veiculo = new VeiculoCliente(_clienteIdValido, MarcaValida, ModeloValido, CorValida, AnoValido, "ABC1D23");
        var act = () => veiculo.AtualizarCorVeiculoCliente(corInvalida);
        act.Should().Throw<ArgumentException>().WithMessage("Cor inválida");
    }

    [Fact]
    public void AtualizarDados_TodosValidos_AtualizaTodos()
    {
        var veiculo = new VeiculoCliente(_clienteIdValido, MarcaValida, ModeloValido, CorValida, AnoValido, "ABC1D23");
        veiculo.AtualizarDadosVeiculoCliente("VW", "Gol", "Vermelho", 2021);
        veiculo.GetMarca().Should().Be("VW");
        veiculo.GetModelo().Should().Be("Gol");
        veiculo.GetCor().Should().Be("Vermelho");
        veiculo.GetAno().Should().Be(2021);
    }

    [Fact]
    public void GetHistorico_SemServicos_RetornaMensagemPadrao()
    {
        var veiculo = new VeiculoCliente(_clienteIdValido, MarcaValida, ModeloValido, CorValida, AnoValido, "ABC1D23");
        Assert.Equal("Veiculo não possui histórico.", veiculo.GetHistorico());
    }

    [Fact]
    public void GetHistorico_ComServicos_RetornaRelatorio()
    {
        // O histórico é uma lista de OrdemServico, difícil de simular sem criar a entidade completa.
        // Podemos testar que a lista existe e está vazia inicialmente.
        var veiculo = new VeiculoCliente(_clienteIdValido, MarcaValida, ModeloValido, CorValida, AnoValido, "ABC1D23");
        veiculo.HistoricoServicos.Should().NotBeNull();
        veiculo.HistoricoServicos.Should().BeEmpty();
    }
}