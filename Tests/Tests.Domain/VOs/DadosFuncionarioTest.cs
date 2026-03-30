using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;
using FluentAssertions;

public class DadosFuncionarioTest
{
    [Fact]
    public void Deve_Criar_DadosFuncionario_Valido()
    {
        var data = DateTime.UtcNow.AddYears(-2);

        var dados = new DadosFuncionario(NivelFuncionario.Junior, data);

        dados.Nivel.Should().Be(NivelFuncionario.Junior);
        dados.DataContratacao.Should().Be(data);
    }

    [Fact]
    public void Deve_Rejeitar_Data_Futura()
    {
        var dataFutura = DateTime.UtcNow.AddDays(1);

        Action act = () => new DadosFuncionario(NivelFuncionario.Pleno, dataFutura);

        act.Should().Throw<ArgumentException>()
        .WithMessage("Data de contratação inválida");
    }

    [Fact]
    public void Deve_Calcular_Anos_De_Empresa()
    {
        var data = DateTime.UtcNow.AddYears(-3);

        var dados = new DadosFuncionario(NivelFuncionario.Senior, data);

        var anos = dados.GetAnosEmpresa();

        anos.Should().BeInRange(2, 3);
    }

    [Fact]
    public void Deve_Retornar_Zero_Para_Recem_Contratado()
    {
        var data = DateTime.UtcNow;

        var dados = new DadosFuncionario(NivelFuncionario.Junior, data);

        dados.GetAnosEmpresa().Should().Be(0);
    }

    [Fact]
    public void Deve_Calcular_Muitos_Anos_De_Empresa()
    {
        var data = DateTime.UtcNow.AddYears(-10);

        var dados = new DadosFuncionario(NivelFuncionario.Senior, data);

        dados.GetAnosEmpresa().Should().BeGreaterThanOrEqualTo(9);
    }
}