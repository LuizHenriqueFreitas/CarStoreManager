using System;
using FluentAssertions;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using Xunit;

namespace CarStoreManager.Tests.Unidade.Mappings;

public class OrdemServicoPublicaMappingTest
{
    private static OrdemServico NovaOs(decimal custoServico = 200m)
        => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
               TipoServico.Revisao, "Revisão dos 10 mil", DateTime.UtcNow.AddDays(3), custoServico);

    // Regressão do bug need_to_do.txt:100 — ToPublicaDto nunca preenchia ValorTotal,
    // então a consulta pública mostrava sempre R$ 0,00.
    [Fact]
    public void ToPublicaDto_PreencheValorTotal_IgualAoTotalDaOS()
    {
        var os = NovaOs(350m);

        var dto = OrdemServicoMapping.ToPublicaDto(os);

        dto.ValorTotal.Should().Be(os.GetValorTotal());
        dto.ValorTotal.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ToPublicaDto_ComPecaCobrada_SomaNoValorTotal()
    {
        var os = NovaOs(100m);
        os.AdicionarItem(new ItemOrdemServico(Guid.NewGuid(), os.Id, 2, 50m));

        var dto = OrdemServicoMapping.ToPublicaDto(os);

        dto.ValorTotal.Should().Be(os.GetValorTotal());
        dto.ValorTotal.Should().Be(200m); // 100 serviço + 2×50 peça
    }

    [Fact]
    public void ToPublicaDto_PecaTrazidaPeloCliente_NaoAlteraValorTotal()
    {
        var os = NovaOs(100m);
        os.AdicionarItem(new ItemOrdemServico(os.Id, 1, "Filtro de óleo do cliente"));

        var dto = OrdemServicoMapping.ToPublicaDto(os);

        dto.ValorTotal.Should().Be(100m);
    }

    [Fact]
    public void ToPublicaDto_CopiaChecklistOrdenado()
    {
        var os = NovaOs();
        os.AdicionarItemChecklist("Trocar óleo");
        os.AdicionarItemChecklist("Verificar freios");

        var dto = OrdemServicoMapping.ToPublicaDto(os);

        dto.Checklist.Should().HaveCount(2);
        dto.NumeroPublico.Should().Be(os.NumeroPublico);
    }
}
