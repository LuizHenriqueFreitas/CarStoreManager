// Este arquivo nao foi revisado nem documentado
using FluentAssertions;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Concessionaria;

public class VeiculoConsignacaoTest
{
    private static VeiculoConsignacao CriarConsignacaoValida(int prazoDias = 90)
    {
        var comissao = ComissaoConsignacao.CriarFixo(50000, 2000);
        return new VeiculoConsignacao(
            "Marca", "Modelo", "Cor", "1.0", 2020, 10000, "ABC1234", "12345678900",
            TipoCambio.Manual, TipoCombustivel.Gasolina,
            Guid.NewGuid(), Guid.NewGuid(), comissao, "Contrato de teste", prazoDias: prazoDias);
    }

    // ==================== RenovarConsignacao ====================

    [Fact]
    public void RenovarConsignacao_AtivaComMaisDe10DiasRestantes_LancaExcecao()
    {
        var consignacao = CriarConsignacaoValida(prazoDias: 90);

        var acao = () => consignacao.RenovarConsignacao();

        acao.Should().Throw<InvalidOperationException>().WithMessage("*10 dia*");
        consignacao.Status.Should().Be(StatusConsignacao.Ativa);
    }

    [Fact]
    public void RenovarConsignacao_AtivaCom10DiasOuMenosRestantes_Renova()
    {
        var consignacao = CriarConsignacaoValida(prazoDias: 90);
        typeof(VeiculoConsignacao).GetProperty("DataVencimento")?.SetValue(consignacao, DateTime.UtcNow.AddDays(5));

        consignacao.RenovarConsignacao(90);

        consignacao.Status.Should().Be(StatusConsignacao.Ativa);
        consignacao.DataVencimento.Should().BeCloseTo(DateTime.UtcNow.AddDays(95), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void RenovarConsignacao_Expirada_RenovaIndependenteDosDias()
    {
        var consignacao = CriarConsignacaoValida(prazoDias: 90);
        typeof(VeiculoConsignacao).GetProperty("DataVencimento")?.SetValue(consignacao, DateTime.UtcNow.AddDays(-5));
        consignacao.TentarExpirar();
        consignacao.Status.Should().Be(StatusConsignacao.Expirada);

        consignacao.RenovarConsignacao(90);

        consignacao.Status.Should().Be(StatusConsignacao.Ativa);
    }
}
