using FluentAssertions;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;
using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Exceptions;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Concessionaria;

public class VeiculoVendaTest
{
    // RENAVAM válido pré-calculado (DV = 0 para 1234567890)
    private const string RenavamValido = "12345678900";

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
        veiculo.GetRenavam().Should().Be(RenavamValido);
        veiculo.Cambio.Should().Be(TipoCambio.Automatico);
        veiculo.Combustivel.Should().Be(TipoCombustivel.Flex);
        veiculo.Valor.GetValorDinheiro().Should().Be(85000.00m);
        veiculo.Disponibilidade.Should().Be(DisponibilidadeVeiculo.Disponivel);
        veiculo.GetAcessoriosVeiculo().Should().Be(AcessoriosVeiculo.ArCondicionado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_MarcaInvalida_LancaArgumentException(string marca)
    {
        Action act = () => CriarVeiculo(marca: marca);
        act.Should().Throw<ArgumentException>().WithMessage("*Marca*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ModeloInvalido_LancaArgumentException(string modelo)
    {
        Action act = () => CriarVeiculo(modelo: modelo);
        act.Should().Throw<ArgumentException>().WithMessage("*Modelo*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_CorInvalida_LancaArgumentException(string cor)
    {
        Action act = () => CriarVeiculo(cor: cor);
        act.Should().Throw<ArgumentException>().WithMessage("*Cor*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_MotorizacaoInvalida_LancaArgumentException(string motor)
    {
        Action act = () => CriarVeiculo(motor: motor);
        act.Should().Throw<ArgumentException>().WithMessage("*Motorização*");
    }

    [Fact]
    public void Construtor_AnoNegativo_LancaAnoInvalidoException()
    {
        Action act = () => CriarVeiculo(ano: -1);
        act.Should().Throw<AnoInvalidoException>();
    }

    [Fact]
    public void Construtor_QuilometragemNegativa_LancaArgumentException()
    {
        Action act = () => CriarVeiculo(km: -100);
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Construtor_PlacaInvalida_LancaPlacaVeiculoInvalidaException()
    {
        Action act = () => CriarVeiculo(placa: "");
        act.Should().Throw<PlacaVeiculoInvalidaException>();
    }

    [Fact]
    public void Construtor_RenavamInvalido_LancaRenavamInvalidoException()
    {
        Action act = () => CriarVeiculo(renavam: "12345");
        act.Should().Throw<RenavamInvalidoException>();
    }

    [Fact]
    public void Construtor_RenavamComDvErrado_LancaRenavamInvalidoException()
    {
        // 12345678901 — DV correto seria 0, não 1
        Action act = () => CriarVeiculo(renavam: "12345678901");
        act.Should().Throw<RenavamInvalidoException>();
    }

    [Fact]
    public void Construtor_ValorNegativo_LancaArgumentException()
    {
        Action act = () => CriarVeiculo(valor: -1);
        act.Should().Throw<ArgumentException>();
    }

    // ==================== IPVA ====================

    [Fact]
    public void Construtor_SemAnoIpva_DefineAnoUltimoIpvaPagoComoNull()
    {
        var veiculo = CriarVeiculoValido();
        veiculo.AnoUltimoIpvaPago.Should().BeNull();
    }

    [Fact]
    public void Construtor_ComAnoIpva_DefineAnoUltimoIpvaPago()
    {
        var veiculo = CriarVeiculo(anoIpva: 2024);
        veiculo.AnoUltimoIpvaPago.Should().Be(2024);
    }

    [Fact]
    public void RegistrarPagamentoIpva_AnoValido_AtualizaCampo()
    {
        var veiculo = CriarVeiculoValido();
        veiculo.RegistrarPagamentoIpva(2025);
        veiculo.AnoUltimoIpvaPago.Should().Be(2025);
    }

    [Fact]
    public void RegistrarPagamentoIpva_AnoAnteriorAoUltimo_LancaInvalidOperationException()
    {
        var veiculo = CriarVeiculo(anoIpva: 2024);
        Action act = () => veiculo.RegistrarPagamentoIpva(2023);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RegistrarPagamentoIpva_AnoMuitoAntigo_LancaArgumentException()
    {
        var veiculo = CriarVeiculoValido();
        Action act = () => veiculo.RegistrarPagamentoIpva(1800);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IpvaEmDia_AnoUltimoMaiorOuIgual_RetornaTrue()
    {
        var veiculo = CriarVeiculo(anoIpva: 2024);
        veiculo.IpvaEmDia(2024).Should().BeTrue();
        veiculo.IpvaEmDia(2023).Should().BeTrue();
    }

    [Fact]
    public void IpvaEmDia_AnoUltimoMenor_RetornaFalse()
    {
        var veiculo = CriarVeiculo(anoIpva: 2023);
        veiculo.IpvaEmDia(2024).Should().BeFalse();
    }

    [Fact]
    public void IpvaEmDia_SemAnoUltimo_RetornaFalse()
    {
        var veiculo = CriarVeiculoValido();
        veiculo.IpvaEmDia(2024).Should().BeFalse();
    }

    // ==================== GETTERS ====================

    [Fact]
    public void GetAcessoriosLista_SemAcessorios_RetornaListaVazia()
    {
        var veiculo = CriarVeiculoValido(acessorios: AcessoriosVeiculo.Nenhum);
        veiculo.GetAcessoriosLista().Should().BeEmpty();
    }

    [Fact]
    public void GetAcessoriosLista_ComAcessorios_RetornaListaCorreta()
    {
        var veiculo = CriarVeiculoValido(
            acessorios: AcessoriosVeiculo.ArCondicionado | AcessoriosVeiculo.DirecaoHidraulica);
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
    public void AlterarQuilometragem_Negativa_Lanca()
    {
        var veiculo = CriarVeiculoValido();
        Action act = () => veiculo.AlterarQuilometragem(-1);
        act.Should().Throw<Exception>();
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
        var veiculo = CriarVeiculoValido(acessorios: AcessoriosVeiculo.Nenhum);
        veiculo.AdicionarAcessorio(AcessoriosVeiculo.TetoSolar);
        veiculo.GetAcessoriosVeiculo().Should().HaveFlag(AcessoriosVeiculo.TetoSolar);
    }

    [Fact]
    public void RemoverAcessorio_RemoveFlag()
    {
        var veiculo = CriarVeiculoValido(
            acessorios: AcessoriosVeiculo.BancoCouro | AcessoriosVeiculo.Alarme);
        veiculo.RemoverAcessorio(AcessoriosVeiculo.Alarme);
        veiculo.GetAcessoriosVeiculo().Should().NotHaveFlag(AcessoriosVeiculo.Alarme);
        veiculo.GetAcessoriosVeiculo().Should().HaveFlag(AcessoriosVeiculo.BancoCouro);
    }

    [Fact]
    public void DefinirAcessorios_SubstituiTodos()
    {
        var veiculo = CriarVeiculoValido(acessorios: AcessoriosVeiculo.ArCondicionado);
        veiculo.DefinirAcessorios(
            AcessoriosVeiculo.VidrosEletricos | AcessoriosVeiculo.CentralMultimidia);
        veiculo.GetAcessoriosVeiculo().Should().Be(
            AcessoriosVeiculo.VidrosEletricos | AcessoriosVeiculo.CentralMultimidia);
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
        var veiculo = CriarVeiculoValido();
        veiculo.AdicionarFoto("http://fotos.com/1.jpg");
        veiculo.Fotos.Should().ContainSingle();
        veiculo.Fotos[0].Ordem.Should().Be(0);
        veiculo.Fotos[0].Url.Should().Be("http://fotos.com/1.jpg");
    }

    [Fact]
    public void AdicionarFoto_VariasFotos_IncrementaOrdem()
    {
        var veiculo = CriarVeiculoValido();
        veiculo.AdicionarFoto("foto1.jpg");
        veiculo.AdicionarFoto("foto2.jpg");
        veiculo.AdicionarFoto("foto3.jpg");
        veiculo.Fotos.Should().HaveCount(3);
        veiculo.Fotos.Select(f => f.Ordem).Should().Equal(0, 1, 2);
    }

    [Fact]
    public void RemoverFoto_FotoExistente_RemoveEReordena()
    {
        var veiculo = CriarVeiculoValido();
        veiculo.AdicionarFoto("foto1.jpg");
        veiculo.AdicionarFoto("foto2.jpg");
        var fotoARemover = veiculo.Fotos[0];
        veiculo.RemoverFoto(fotoARemover.Id);
        veiculo.Fotos.Should().HaveCount(1);
        veiculo.Fotos[0].Ordem.Should().Be(0);
    }

    [Fact]
    public void RemoverFoto_FotoInexistente_LancaInvalidOperationException()
    {
        var veiculo = CriarVeiculoValido();
        Action act = () => veiculo.RemoverFoto(Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>().WithMessage("*Foto não encontrada*");
    }

    [Fact]
    public void RemoverFoto_UltimaFoto_NaoLancaExcecao()
    {
        var veiculo = CriarVeiculoValido();
        veiculo.AdicionarFoto("unica.jpg");
        var foto = veiculo.Fotos[0];
        veiculo.RemoverFoto(foto.Id);
        veiculo.Fotos.Should().BeEmpty();
    }

    // ==================== PLACA — formatos aceitos ====================

    [Theory]
    [InlineData("ABC1D23")]      // Mercosul sem hífen
    [InlineData("ABC-1D23")]     // Mercosul com hífen
    [InlineData("ABC1234")]      // Antigo sem hífen
    [InlineData("ABC-1234")]     // Antigo com hífen
    public void Construtor_PlacaEmFormatosAceitos_NaoLanca(string placa)
    {
        Action act = () => CriarVeiculo(placa: placa);
        act.Should().NotThrow();
    }

    // ==================== HELPERS ====================

    private static VeiculoVenda CriarVeiculoValido(
        AcessoriosVeiculo acessorios = AcessoriosVeiculo.ArCondicionado,
        int? anoIpva = null)
    {
        return new VeiculoVenda(
            "Honda", "Civic", "Preto", "2.0 Turbo", 2023, 15000, "ABC1D23", RenavamValido,
            TipoCambio.Automatico, TipoCombustivel.Flex, 85000.00m, acessorios, anoIpva);
    }

    private static VeiculoVenda CriarVeiculo(
        string marca = "Marca",
        string modelo = "Modelo",
        string cor = "Cor",
        string motor = "Motor",
        int ano = 2020,
        int km = 0,
        string placa = "ABC1234",
        string renavam = RenavamValido,
        TipoCambio cambio = TipoCambio.Manual,
        TipoCombustivel combustivel = TipoCombustivel.Gasolina,
        decimal valor = 10000m,
        AcessoriosVeiculo acessorios = AcessoriosVeiculo.Nenhum,
        int? anoIpva = null)
    {
        return new VeiculoVenda(marca, modelo, cor, motor, ano, km, placa, renavam,
            cambio, combustivel, valor, acessorios, anoIpva);
    }
}
