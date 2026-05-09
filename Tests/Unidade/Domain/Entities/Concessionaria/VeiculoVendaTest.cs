// Este arquivo nao foi revisado nem documentado
using FluentAssertions;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;
using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Exceptions;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Concessionaria;

public class VeiculoVendaTest
{
    // ==================== CONSTRUTOR ====================

    [Fact]
    public void Construtor_CamposValidos_CriaInstancia()
    {
        var veiculo = CriarVeiculoValido();

        veiculo.Marca.Should().Be("Honda");
        veiculo.Modelo.Should().Be("Civic");
        veiculo.Cor.Should().Be("Preto");
        veiculo.Motorizacao.Should().Be("2.0 Turbo");
        veiculo.Ano.GetValorAno().Should().Be(2023);
        veiculo.Quilometragem.GetQuilometragem().Should().Be(15000);
        veiculo.Placa.GetPlaca().Should().Be("ABC1D23");
        veiculo.Cambio.Should().Be(TipoCambio.Automatico);
        veiculo.Combustivel.Should().Be(TipoCombustivel.Flex);
        veiculo.Valor.GetValorDinheiro().Should().Be(85000.00m);
        veiculo.Disponibilidade.Should().Be(DisponibilidadeVeiculo.Disponivel);
        veiculo.GetAcessoriosVeiculo().Should().Be(AcessoriosVeiculo.ArCondicionado); // passado no auxiliar
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_MarcaInvalida_LancaArgumentException(string marca)
    {
        Action act = () => new VeiculoVenda(marca, "Modelo", "Cor", "Motor", 2020, 0, "ABC1234",
            TipoCambio.Manual, TipoCombustivel.Gasolina, 10000, AcessoriosVeiculo.Nenhum);
        act.Should().Throw<ArgumentException>().WithMessage("*Marca*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ModeloInvalido_LancaArgumentException(string modelo)
    {
        Action act = () => new VeiculoVenda("Marca", modelo, "Cor", "Motor", 2020, 0, "ABC1234",
            TipoCambio.Manual, TipoCombustivel.Gasolina, 10000, AcessoriosVeiculo.Nenhum);
        act.Should().Throw<ArgumentException>().WithMessage("*Modelo*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_CorInvalida_LancaArgumentException(string cor)
    {
        Action act = () => new VeiculoVenda("Marca", "Modelo", cor, "Motor", 2020, 0, "ABC1234",
            TipoCambio.Manual, TipoCombustivel.Gasolina, 10000, AcessoriosVeiculo.Nenhum);
        act.Should().Throw<ArgumentException>().WithMessage("*Cor*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_MotorizacaoInvalida_LancaArgumentException(string motor)
    {
        Action act = () => new VeiculoVenda("Marca", "Modelo", "Cor", motor, 2020, 0, "ABC1234",
            TipoCambio.Manual, TipoCombustivel.Gasolina, 10000, AcessoriosVeiculo.Nenhum);
        act.Should().Throw<ArgumentException>().WithMessage("*Motorização*");
    }

    [Fact]
    public void Construtor_AnoNegativo_LancaArgumentException()
    {
        // O Value Object Ano valida isso; testamos indiretamente
        Action act = () => new VeiculoVenda("Marca", "Modelo", "Cor", "Motor", -1, 0, "ABC1234",
            TipoCambio.Manual, TipoCombustivel.Gasolina, 10000, AcessoriosVeiculo.Nenhum);
        act.Should().Throw<AnoInvalidoException>();
    }

    [Fact]
    public void Construtor_QuilometragemNegativa_LancaArgumentException()
    {
        Action act = () => new VeiculoVenda("Marca", "Modelo", "Cor", "Motor", 2020, -100, "ABC1234",
            TipoCambio.Manual, TipoCombustivel.Gasolina, 10000, AcessoriosVeiculo.Nenhum);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Construtor_PlacaInvalida_LancaArgumentException()
    {
        Action act = () => new VeiculoVenda("Marca", "Modelo", "Cor", "Motor", 2020, 0, "",
            TipoCambio.Manual, TipoCombustivel.Gasolina, 10000, AcessoriosVeiculo.Nenhum);
        act.Should().Throw<PlacaVeiculoInvalidaException>();
    }

    [Fact]
    public void Construtor_ValorNegativo_LancaArgumentException()
    {
        Action act = () => new VeiculoVenda("Marca", "Modelo", "Cor", "Motor", 2020, 0, "ABC1234",
            TipoCambio.Manual, TipoCombustivel.Gasolina, -1, AcessoriosVeiculo.Nenhum);
        act.Should().Throw<ArgumentException>();
    }

    // ==================== GETTERS ====================

    [Fact]
    public void GetAcessoriosLista_SemAcessorios_RetornaListaVazia()
    {
        var veiculo = new VeiculoVenda("A", "B", "C", "M", 2020, 0, "XYZ1A23",
            TipoCambio.Manual, TipoCombustivel.Gasolina, 1000, AcessoriosVeiculo.Nenhum);
        veiculo.GetAcessoriosLista().Should().BeEmpty();
    }

    [Fact]
    public void GetAcessoriosLista_ComAcessorios_RetornaListaCorreta()
    {
        var veiculo = CriarVeiculoValido(acessorios: AcessoriosVeiculo.ArCondicionado | AcessoriosVeiculo.DirecaoHidraulica);
        var lista = veiculo.GetAcessoriosLista();
        lista.Should().Contain(new[] { "ArCondicionado", "DirecaoHidraulica" });
        lista.Should().HaveCount(2);
    }

    // ==================== SETTERS BÁSICOS ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AlterarMarca_Invalida_LancaArgumentException(string marca)
    {
        var veiculo = CriarVeiculoValido();
        Action act = () => veiculo.AlterarMarca(marca);
        act.Should().Throw<ArgumentException>().WithMessage("*Marca*");
    }

    [Fact]
    public void AlterarMarca_Valida_AtualizaMarca()
    {
        var veiculo = CriarVeiculoValido();
        veiculo.AlterarMarca("Toyota");
        veiculo.Marca.Should().Be("Toyota");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AlterarModelo_Invalido_LancaArgumentException(string modelo)
    {
        var veiculo = CriarVeiculoValido();
        Action act = () => veiculo.AlterarModelo(modelo);
        act.Should().Throw<ArgumentException>().WithMessage("*Modelo*");
    }

    [Fact]
    public void AlterarModelo_Valido_AtualizaModelo()
    {
        var veiculo = CriarVeiculoValido();
        veiculo.AlterarModelo("Corolla");
        veiculo.Modelo.Should().Be("Corolla");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AlterarCor_Invalida_LancaArgumentException(string cor)
    {
        var veiculo = CriarVeiculoValido();
        Action act = () => veiculo.AlterarCor(cor);
        act.Should().Throw<ArgumentException>().WithMessage("*Cor*");
    }

    [Fact]
    public void AlterarCor_Valida_AtualizaCor()
    {
        var veiculo = CriarVeiculoValido();
        veiculo.AlterarCor("Azul");
        veiculo.Cor.Should().Be("Azul");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AlterarMotorizacao_Invalida_LancaArgumentException(string motor)
    {
        var veiculo = CriarVeiculoValido();
        Action act = () => veiculo.AlterarMotorizacao(motor);
        act.Should().Throw<ArgumentException>().WithMessage("*Motorização*");
    }

    [Fact]
    public void AlterarMotorizacao_Valida_AtualizaMotorizacao()
    {
        var veiculo = CriarVeiculoValido();
        veiculo.AlterarMotorizacao("1.8 Flex");
        veiculo.Motorizacao.Should().Be("1.8 Flex");
    }

    // ==================== QUILOMETRAGEM ====================

    [Fact]
    public void AlterarQuilometragem_ValorValido_Atualiza()
    {
        var veiculo = CriarVeiculoValido();
        veiculo.AlterarQuilometragem(20000);
        veiculo.GetQuilometragem().Should().Be(20000);
    }

    [Fact]
    public void AlterarQuilometragem_Negativa_LancaArgumentException()
    {
        var veiculo = CriarVeiculoValido();
        Action act = () => veiculo.AlterarQuilometragem(-1);
        act.Should().Throw<InvalidOperationException>();
    }

    // ==================== VALOR ====================

    [Fact]
    public void AtualizarValor_ValorPositivo_Atualiza()
    {
        var veiculo = CriarVeiculoValido();
        veiculo.AtualizarValor(new Dinheiro(95000));
        veiculo.GetValor().Should().Be(95000);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AtualizarValor_ZeroOuNegativo_LancaArgumentException(decimal valor)
    {
        var veiculo = CriarVeiculoValido();
        Action act = () => veiculo.AtualizarValor(new Dinheiro(valor));
        act.Should().Throw<ArgumentException>().WithMessage("*Valor*");
    }

    // ==================== ACESSÓRIOS ====================

    [Fact]
    public void AdicionarAcessorio_SetaFlagCorretamente()
    {
        var veiculo = CriarVeiculoValido(AcessoriosVeiculo.Nenhum);
        veiculo.AdicionarAcessorio(AcessoriosVeiculo.TetoSolar);
        veiculo.GetAcessoriosVeiculo().Should().HaveFlag(AcessoriosVeiculo.TetoSolar);
    }

    [Fact]
    public void RemoverAcessorio_RemoveFlag()
    {
        var veiculo = CriarVeiculoValido(AcessoriosVeiculo.BancoCouro | AcessoriosVeiculo.Alarme);
        veiculo.RemoverAcessorio(AcessoriosVeiculo.Alarme);
        veiculo.GetAcessoriosVeiculo().Should().NotHaveFlag(AcessoriosVeiculo.Alarme);
        veiculo.GetAcessoriosVeiculo().Should().HaveFlag(AcessoriosVeiculo.BancoCouro);
    }

    [Fact]
    public void DefinirAcessorios_SubstituiTodos()
    {
        var veiculo = CriarVeiculoValido(AcessoriosVeiculo.ArCondicionado);
        veiculo.DefinirAcessorios(AcessoriosVeiculo.VidrosEletricos | AcessoriosVeiculo.CentralMultimidia);
        veiculo.GetAcessoriosVeiculo().Should().Be(AcessoriosVeiculo.VidrosEletricos | AcessoriosVeiculo.CentralMultimidia);
    }

    // ==================== DISPONIBILIDADE ====================

    [Fact]
    public void MarcarComoVendido_AlteraParaVendido()
    {
        var veiculo = CriarVeiculoValido();
        veiculo.MarcarComoVendido();
        veiculo.Disponibilidade.Should().Be(DisponibilidadeVeiculo.Vendido);
    }

    [Fact]
    public void MarcarComoDisponivel_AlteraParaDisponivel()
    {
        var veiculo = CriarVeiculoValido();
        veiculo.MarcarComoVendido();
        veiculo.MarcarComoDisponivel();
        veiculo.Disponibilidade.Should().Be(DisponibilidadeVeiculo.Disponivel);
    }

    [Fact]
    public void AlterarDisponibilidade_DefineStatus()
    {
        var veiculo = CriarVeiculoValido();
        veiculo.AlterarDisponibilidade(DisponibilidadeVeiculo.Reservado);
        veiculo.Disponibilidade.Should().Be(DisponibilidadeVeiculo.Reservado);
    }

    // ==================== ATUALIZAR DADOS COMBINADOS ====================

    [Fact]
    public void AtualizarVeiculoVendaDados_AtualizaValorEDisponibilidade()
    {
        var veiculo = CriarVeiculoValido();
        veiculo.AtualizarVeiculoVendaDados(new Dinheiro(80000), DisponibilidadeVeiculo.Vendido);
        veiculo.GetValor().Should().Be(80000);
        veiculo.Disponibilidade.Should().Be(DisponibilidadeVeiculo.Vendido);
    }

    // ==================== GERENCIAMENTO DE FOTOS ====================

    [Fact]
    public void AdicionarFoto_UrlValida_AdicionaFotoEOrdena()
    {
        var veiculo = CriarVeiculoValidoComId();
        veiculo.AdicionarFoto("http://fotos.com/1.jpg");
        veiculo.Fotos.Should().ContainSingle();
        veiculo.Fotos[0].Ordem.Should().Be(0);
        veiculo.Fotos[0].Url.Should().Be("http://fotos.com/1.jpg");
    }

    [Fact]
    public void AdicionarFoto_VariasFotos_IncrementaOrdem()
    {
        var veiculo = CriarVeiculoValidoComId();
        veiculo.AdicionarFoto("foto1.jpg");
        veiculo.AdicionarFoto("foto2.jpg");
        veiculo.AdicionarFoto("foto3.jpg");
        veiculo.Fotos.Should().HaveCount(3);
        veiculo.Fotos.Select(f => f.Ordem).Should().Equal(0, 1, 2);
    }

    [Fact]
    public void RemoverFoto_FotoExistente_RemoveEReordena()
    {
        var veiculo = CriarVeiculoValidoComId();
        veiculo.AdicionarFoto("foto1.jpg");
        veiculo.AdicionarFoto("foto2.jpg");
        var fotoARemover = veiculo.Fotos[0];
        veiculo.RemoverFoto(fotoARemover.Id);
        veiculo.Fotos.Should().HaveCount(1);
        veiculo.Fotos[0].Ordem.Should().Be(0); // reordenou
    }

    [Fact]
    public void RemoverFoto_FotoInexistente_LancaInvalidOperationException()
    {
        var veiculo = CriarVeiculoValidoComId();
        Action act = () => veiculo.RemoverFoto(Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>().WithMessage("*Foto não encontrada*");
    }

    [Fact]
    public void RemoverFoto_UltimaFoto_NaoLancaExcecao()
    {
        var veiculo = CriarVeiculoValidoComId();
        veiculo.AdicionarFoto("unica.jpg");
        var foto = veiculo.Fotos[0];
        veiculo.RemoverFoto(foto.Id);
        veiculo.Fotos.Should().BeEmpty();
    }

    // ==================== MÉTODOS AUXILIARES ====================

    private static VeiculoVenda CriarVeiculoValido(
        AcessoriosVeiculo acessorios = AcessoriosVeiculo.ArCondicionado)
    {
        return new VeiculoVenda(
            "Honda", "Civic", "Preto", "2.0 Turbo", 2023, 15000, "ABC1D23",
            TipoCambio.Automatico, TipoCombustivel.Flex, 85000.00m, acessorios);
    }

    private static VeiculoVenda CriarVeiculoValidoComId(
        AcessoriosVeiculo acessorios = AcessoriosVeiculo.ArCondicionado)
    {
        var veiculo = CriarVeiculoValido(acessorios);
        // Define um Id arbitrário para os testes de foto (necessário para o construtor de FotoVeiculo)
        typeof(Entity).GetProperty("Id")?.SetValue(veiculo, Guid.NewGuid());
        return veiculo;
    }
}