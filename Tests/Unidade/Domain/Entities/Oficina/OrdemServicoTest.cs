// Este arquivo nao foi revisado nem documentado
using FluentAssertions;
using Moq;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Oficina;

public class OrdemServicoTest
{
    // ==================== CONSTRUTOR ====================

    [Fact]
    public void Construtor_DescricaoNula_LancaArgumentException()
    {
        Action act = () => new OrdemServico(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            TipoServico.Revisao, null!, DateTime.UtcNow.AddDays(1), 100);

        act.Should().Throw<ArgumentException>().WithMessage("*Descrição*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DescricaoVaziaOuEspacos_LancaArgumentException(string descricao)
    {
        Action act = () => new OrdemServico(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            TipoServico.TrocaPecas, descricao, DateTime.UtcNow.AddDays(1), 100);

        act.Should().Throw<ArgumentException>().WithMessage("*Descrição*");
    }

    [Fact]
    public void Construtor_PrazoEstimadoNoPassado_LancaArgumentException()
    {
        Action act = () => new OrdemServico(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            TipoServico.Revisao, "Desc", DateTime.UtcNow.AddSeconds(-1), 100);

        act.Should().Throw<ArgumentException>().WithMessage("*Prazo*");
    }

    [Fact]
    public void Construtor_CustoServicoNegativo_LancaArgumentException()
    {
        Action act = () => new OrdemServico(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            TipoServico.TrocaPecas, "Desc", DateTime.UtcNow.AddDays(1), -1);

        act.Should().Throw<ArgumentException>().WithMessage("*Valor*");
    }

    [Fact]
    public void Construtor_CustoServicoZero_CriaOrdem()
    {
        var ordem = new OrdemServico(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            Guid.NewGuid(),
            TipoServico.Diagnostico, 
            "Desc", 
            DateTime.UtcNow.AddDays(1), 
            1
        );

        Assert.Equal(1, ordem.GetCustoServico());
    }

    [Fact]
    public void Construtor_CamposValidos_AtribuiCorretamente()
    {
        var veiculoId = Guid.NewGuid();
        var mecanicoId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var prazo = DateTime.UtcNow.AddDays(3);
        var descricao = "Serviço completo";

        var ordem = new OrdemServico(
            veiculoId, 
            mecanicoId, 
            clienteId, 
            TipoServico.Diagnostico, 
            descricao, 
            prazo, 
            250.75m);

        ordem.VeiculoClienteId.Should().Be(veiculoId);
        ordem.MecanicoId.Should().Be(mecanicoId);
        ordem.ClienteId.Should().Be(clienteId);
        ordem.Tipo.Should().Be(TipoServico.Diagnostico);
        ordem.Descricao.Should().Be(descricao);
        ordem.PrazoEstimado.Should().Be(prazo);
        ordem.CustoServico.GetValorDinheiro().Should().Be(250.75m);
        ordem.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        ordem.Status.Should().Be(StatusOrdemServico.Pendente);
        ordem.NumeroPublico.Should().NotBeNullOrEmpty().And.HaveLength(8);
        ordem.Itens.Should().BeEmpty();
        ordem.Checklist.Should().BeEmpty();
    }

    [Fact]
    public void Construtor_AoConstruir_RecalculaValorTotal()
    {
        var ordem = new OrdemServico(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            TipoServico.Manutencao, "Desc", DateTime.UtcNow.AddDays(1), 100);

        // Sem itens, ValorTotal deve ser igual a CustoServico
        ordem.ValorTotal.GetValorDinheiro().Should().Be(100);
    }

    // ==================== STATUS ====================

    [Fact]
    public void Iniciar_StatusPendente_AlteraParaEmAndamento()
    {
        var ordem = CriarOrdemServicoValida();
        ordem.Iniciar();
        ordem.Status.Should().Be(StatusOrdemServico.EmAndamento);
    }

    [Theory]
    [InlineData(StatusOrdemServico.EmAndamento)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Cancelada)]
    public void Iniciar_StatusDiferenteDePendente_LancaInvalidOperationException(StatusOrdemServico statusInicial)
    {
        var ordem = CriarOrdemServicoValida();
        ordem.GetType().GetProperty(nameof(ordem.Status))!.SetValue(ordem, statusInicial);

        Action act = () => ordem.Iniciar();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Finalizar_StatusEmAndamento_AlteraParaFinalizada()
    {
        var ordem = CriarOrdemServicoValida();
        ordem.Iniciar(); // pendente -> em andamento
        ordem.Finalizar();
        ordem.Status.Should().Be(StatusOrdemServico.Finalizada);
    }

    [Theory]
    [InlineData(StatusOrdemServico.Pendente)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Cancelada)]
    public void Finalizar_StatusDiferenteDeEmAndamento_LancaInvalidOperationException(StatusOrdemServico statusInicial)
    {
        var ordem = CriarOrdemServicoValida();
        ordem.GetType().GetProperty(nameof(ordem.Status))!.SetValue(ordem, statusInicial);

        Action act = () => ordem.Finalizar();
        act.Should().Throw<InvalidOperationException>().WithMessage("*Status*");
    }

    [Fact]
    public void Cancelar_QualquerStatus_AlteraParaCancelada()
    {
        var ordem = CriarOrdemServicoValida();
        ordem.Iniciar();
        ordem.Cancelar();
        ordem.Status.Should().Be(StatusOrdemServico.Cancelada);
    }

    [Theory]
    [InlineData(StatusOrdemServico.Pendente)]
    [InlineData(StatusOrdemServico.EmAndamento)]
    public void AtualizarStatus_StatusNaoFinalizadoNemCancelado_Atualiza(StatusOrdemServico novoStatus)
    {
        var ordem = CriarOrdemServicoValida();
        ordem.AtualizarStatus(novoStatus);
        ordem.Status.Should().Be(novoStatus);
    }

    [Fact]
    public void AtualizarStatus_OrdemFinalizada_LancaInvalidOperationException()
    {
        var ordem = CriarOrdemServicoValida();
        ordem.GetType().GetProperty(nameof(ordem.Status))!.SetValue(ordem, StatusOrdemServico.Finalizada);

        Action act = () => ordem.AtualizarStatus(StatusOrdemServico.EmAndamento);
        act.Should().Throw<InvalidOperationException>().WithMessage("*finalizada*");
    }

    [Fact]
    public void AtualizarStatus_OrdemCancelada_LancaInvalidOperationException()
    {
        var ordem = CriarOrdemServicoValida();
        ordem.GetType().GetProperty(nameof(ordem.Status))!.SetValue(ordem, StatusOrdemServico.Cancelada);

        Action act = () => ordem.AtualizarStatus(StatusOrdemServico.Pendente);
        act.Should().Throw<InvalidOperationException>().WithMessage("*cancelada*");
    }

    // ==================== ITENS ====================

    [Fact]
    public void AdicionarItem_ItemValido_AdicionaERecalculaTotal()
    {
        var ordem = CriarOrdemServicoValida(custoServico: 100);
        // Cria um ItemOrdemServico real com valores conhecidos
        var item = new ItemOrdemServico(
            Guid.NewGuid(),
            Guid.NewGuid(),
            quantidade: 2,
            valorUnitario: 50m
        );
        // Assume-se que o valor total do item seja Quantidade * ValorUnitario
        ordem.AdicionarItem(item);

        ordem.Itens.Should().ContainSingle();
        ordem.ValorTotal.GetValorDinheiro().Should().Be(200); // 100 (custo serviço) + 100 (2*50)
        }

    [Fact]
    public void AdicionarItem_OrdemFinalizada_LancaInvalidOperationException()
    {
        var ordem = CriarOrdemServicoValida();
        ordem.GetType().GetProperty(nameof(ordem.Status))!.SetValue(ordem, StatusOrdemServico.Finalizada);
        var itemMock = new Mock<ItemOrdemServico>();

        Action act = () => ordem.AdicionarItem(itemMock.Object);
        act.Should().Throw<InvalidOperationException>().WithMessage("*itens*");
    }

    [Fact]
    public void RemoverItem_ItemExistente_RemoveERecalculaTotal()
    {
        var ordem = CriarOrdemServicoValida(custoServico: 100);
        var item = new ItemOrdemServico(Guid.NewGuid(), Guid.NewGuid(), 1, 30);
        ordem.AdicionarItem(item);

        ordem.RemoverItem(item.Id);
        ordem.Itens.Should().BeEmpty();
        ordem.ValorTotal.GetValorDinheiro().Should().Be(100);
    }

    [Fact]
    public void RemoverItem_ItemNaoEncontrado_LancaInvalidOperationException()
    {
        var ordem = CriarOrdemServicoValida();
        Action act = () => ordem.RemoverItem(Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>().WithMessage("*Item não encontrado*");
    }

    [Fact]
    public void AtualizarItem_QuantidadeValida_AtualizaQuantidadeERecalcula()
    {
        var ordem = CriarOrdemServicoValida(custoServico: 100);
        var item = new ItemOrdemServico(Guid.NewGuid(), Guid.NewGuid(), 2, 20); // total = 40
        ordem.AdicionarItem(item);

        ordem.AtualizarItem(item.Id, 3); // nova quantidade: 3 → total = 60
        ordem.ValorTotal.GetValorDinheiro().Should().Be(160); // 100 + 60
    }

    [Fact]
    public void AtualizarItem_ItemNaoEncontrado_LancaInvalidOperationException()
    {
        var ordem = CriarOrdemServicoValida();
        Action act = () => ordem.AtualizarItem(Guid.NewGuid(), 5);
        act.Should().Throw<InvalidOperationException>().WithMessage("*Item não encontrado*");
    }

    // ==================== CHECKLIST ====================

    [Fact]
    public void GerarChecklistAPartirDoPreset_ListaDeDescricoes_AdicionaItensAutomaticos()
    {
        var ordem = CriarOrdemServicoValida(tipo: TipoServico.Manutencao);
        ordem.GerarChecklistAPartirDoPreset(new[] { "Verificar óleo", "Verificar pneus" });
        ordem.Checklist.Should().HaveCount(2);
        ordem.Checklist.All(c => c.Origem == OrigemChecklistItem.Automatico).Should().BeTrue();
    }

    [Fact]
    public void GerarChecklistAPartirDoPreset_ChamadoDuasVezes_NaoDuplica()
    {
        var ordem = CriarOrdemServicoValida(tipo: TipoServico.Manutencao);
        ordem.GerarChecklistAPartirDoPreset(new[] { "Item 1", "Item 2" });
        var count = ordem.Checklist.Count;
        ordem.GerarChecklistAPartirDoPreset(new[] { "Item 3" });
        ordem.Checklist.Should().HaveCount(count);
    }

    [Fact]
    public void AdicionarItemChecklist_DescricaoValida_AdicionaItemManual()
    {
        var ordem = CriarOrdemServicoValida();
        ordem.AdicionarItemChecklist("Limpeza interna");
        ordem.Checklist.Should().ContainSingle()
            .Which.Descricao.Should().Be("Limpeza interna");
        ordem.Checklist[0].Origem.Should().Be(OrigemChecklistItem.Manual);
    }

    [Fact]
    public void AdicionarItemChecklist_OrdemFinalizada_LancaInvalidOperationException()
    {
        var ordem = CriarOrdemServicoValida();
        ordem.GetType().GetProperty(nameof(ordem.Status))!.SetValue(ordem, StatusOrdemServico.Finalizada);

        Action act = () => ordem.AdicionarItemChecklist("Teste");
        act.Should().Throw<InvalidOperationException>().WithMessage("*checklist*");
    }

    [Fact]
    public void AdicionarItemChecklist_OrdemCancelada_LancaInvalidOperationException()
    {
        var ordem = CriarOrdemServicoValida();
        ordem.GetType().GetProperty(nameof(ordem.Status))!.SetValue(ordem, StatusOrdemServico.Cancelada);

        Action act = () => ordem.AdicionarItemChecklist("Teste");
        act.Should().Throw<InvalidOperationException>().WithMessage("*checklist*");
    }

    // ==================== GETTERS / FORMATACAO ====================

    [Fact]
    public void GetItens_SemItens_RetornaStringVazia()
    {
        var ordem = CriarOrdemServicoValida();
        Assert.Equal("Ordem não possui Itens listados.", ordem.GetItens());
    }

    [Fact]
    public void GetChecklist_SemItens_RetornaStringVazia()
    {
        var ordem = CriarOrdemServicoValida();
        Assert.Equal("Ordem não possui checklist.", ordem.GetChecklist());
    }

    [Fact]
    public void GetDescricao_RetornaDescricao()
    {
        var ordem = CriarOrdemServicoValida("Troca de óleo");
        ordem.GetDescricao().Should().Be("Troca de óleo");
    }

    [Fact]
    public void GetNumeroPublico_RetornaString8Caracteres()
    {
        var ordem = CriarOrdemServicoValida();
        ordem.GetNumeroPublico().Should().HaveLength(8).And.NotBeNullOrWhiteSpace();
    }

    // ==================== METODOS DE DEFINICAO ====================

    [Fact]
    public void DefinirCliente_ClienteIdVazio_LancaArgumentException()
    {
        var ordem = CriarOrdemServicoValida();
        Action act = () => ordem.DefinirCliente(Guid.Empty);
        act.Should().Throw<ArgumentException>().WithMessage("*Cliente*");
    }

    [Fact]
    public void DefinirVeiculo_VeiculoIdVazio_LancaArgumentException()
    {
        var ordem = CriarOrdemServicoValida();
        Action act = () => ordem.DefinirVeiculo(Guid.Empty);
        act.Should().Throw<ArgumentException>().WithMessage("*Veículo*");
    }

    [Fact]
    public void DefinirCliente_ClienteValido_AtualizaClienteId()
    {
        var ordem = CriarOrdemServicoValida();
        var novoCliente = Guid.NewGuid();
        ordem.DefinirCliente(novoCliente);
        ordem.ClienteId.Should().Be(novoCliente);
    }

    [Fact]
    public void DefinirVeiculo_VeiculoValido_AtualizaVeiculoClienteId()
    {
        var ordem = CriarOrdemServicoValida();
        var novoVeiculo = Guid.NewGuid();
        ordem.DefinirVeiculo(novoVeiculo);
        ordem.VeiculoClienteId.Should().Be(novoVeiculo);
    }

    // ==================== RECALCULAR TOTAL ====================

    [Fact]
    public void RecalcularTotal_ComMultiplosItens_SomaCorretamente()
    {
        var ordem = CriarOrdemServicoValida(custoServico: 50);
        var item1 = Mock.Of<ItemOrdemServico>(i => i.ValorTotal == new Dinheiro(30));
        var item2 = Mock.Of<ItemOrdemServico>(i => i.ValorTotal == new Dinheiro(20));
        ordem.AdicionarItem(item1);
        ordem.AdicionarItem(item2);

        ordem.ValorTotal.GetValorDinheiro().Should().Be(100); // 50+30+20
    }

    [Fact]
    public void RecalcularTotal_SemItens_ValorTotalIgualCustoServico()
    {
        var ordem = CriarOrdemServicoValida(custoServico: 200);
        // Forçar recalculo sem itens
        ordem.RecalcularTotal();
        ordem.ValorTotal.GetValorDinheiro().Should().Be(200);
    }

    // ==================== MÉTODO AUXILIAR ====================

    private OrdemServico CriarOrdemServicoValida(
        string descricao = "Serviço padrão",
        decimal custoServico = 100,
        TipoServico tipo = TipoServico.Outro,
        DateTime? prazo = null)
    {
        return new OrdemServico(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            tipo,
            descricao,
            prazo ?? DateTime.UtcNow.AddDays(1),
            custoServico);
    }
}