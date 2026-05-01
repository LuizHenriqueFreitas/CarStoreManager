using CarStoreManager.Domain.Exceptions;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Domain.ValueObjects;

public class AnoTest
{
    [Fact]
    public void Valida_Limite_De_Ano_Atual()
    {
        var ano = new Ano(DateTime.Now.Year);

        bool anoValido = ano.ValidaAno(ano.Valor);

        Assert.True(anoValido);
    }

    [Fact]
    public void Valida_Limite_De_Ano_1900()
    {
        var ano = new Ano(1900);

        bool anoValido = ano.ValidaAno(ano.Valor);

        Assert.True(anoValido);
    }

    [Fact]
    public void Deve_Negar_Ano_Muito_Antigo()
    {
        Assert.Throws<AnoInvalidoException>(() => new Ano(1899));
    }

    [Fact]
    public void Deve_Negar_Ano_Futuro()
    {
        var anoFuturo = DateTime.Now.Year + 1;

        Assert.Throws<AnoInvalidoException>(() => new Ano(anoFuturo));
    }
}