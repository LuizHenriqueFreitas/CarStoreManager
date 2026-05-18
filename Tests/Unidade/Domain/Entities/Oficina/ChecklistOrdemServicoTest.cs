// Este arquivo nao foi revisado nem documentado
using FluentAssertions;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Oficina;

public class ChecklistOrdemServicoTests
{
    // ==================== CONSTRUTOR ====================

    [Fact]
    public void Construtor_CamposValidos_CriaItemPendente()
    {
        var ordemServicoId = Guid.NewGuid();
        var descricao = "Troca de óleo";
        var origem = OrigemChecklistItem.Manual;
        var ordemExibicao = 1;

        var item = new ChecklistOrdemServico(ordemServicoId, descricao, origem, ordemExibicao);

        item.OrdemServicoId.Should().Be(ordemServicoId);
        item.Descricao.Should().Be(descricao);
        item.Origem.Should().Be(origem);
        item.OrdemExibicao.Should().Be(ordemExibicao);
        item.Status.Should().Be(StatusChecklistItem.Pendente);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DescricaoInvalida_LancaArgumentException(string descricao)
    {
        Action act = () => new ChecklistOrdemServico(Guid.NewGuid(), descricao, OrigemChecklistItem.Manual, 1);
        act.Should().Throw<ArgumentException>().WithMessage("*Descrição*");
    }

    [Fact]
    public void Construtor_DescricaoComEspacos_TrimAplicado()
    {
        var item = new ChecklistOrdemServico(Guid.NewGuid(), "  Verificar freios  ", OrigemChecklistItem.Manual, 1);
        item.Descricao.Should().Be("Verificar freios");
    }

    // ==================== INICIAR ITEM ====================

    [Fact]
    public void IniciarItem_StatusPendente_MudaParaEmAndamento()
    {
        var item = CriarItem(StatusChecklistItem.Pendente);
        item.IniciarItem();
        item.Status.Should().Be(StatusChecklistItem.EmAndamento);
    }

    [Fact]
    public void IniciarItem_StatusEmAndamento_LancaInvalidOperationException()
    {
        var item = CriarItem(StatusChecklistItem.EmAndamento);
        Action act = () => item.IniciarItem();
        act.Should().Throw<InvalidOperationException>().WithMessage("*iniciado*");
    }

    [Fact]
    public void IniciarItem_StatusConcluido_LancaInvalidOperationException()
    {
        var item = CriarItem(StatusChecklistItem.Concluido);
        Action act = () => item.IniciarItem();
        act.Should().Throw<InvalidOperationException>().WithMessage("*iniciado*");
    }

    // ==================== CONCLUIR ITEM ====================

    [Fact]
    public void ConcluirItem_StatusPendente_MudaParaConcluido()
    {
        var item = CriarItem(StatusChecklistItem.Pendente);
        item.ConcluirItem();
        item.Status.Should().Be(StatusChecklistItem.Concluido);
    }

    [Fact]
    public void ConcluirItem_StatusEmAndamento_MudaParaConcluido()
    {
        var item = CriarItem(StatusChecklistItem.EmAndamento);
        item.ConcluirItem();
        item.Status.Should().Be(StatusChecklistItem.Concluido);
    }

    [Fact]
    public void ConcluirItem_StatusConcluido_LancaInvalidOperationException()
    {
        var item = CriarItem(StatusChecklistItem.Concluido);
        Action act = () => item.ConcluirItem();
        act.Should().Throw<InvalidOperationException>().WithMessage("*já concluído*");
    }

    // ==================== ATUALIZAR DESCRIÇÃO ====================

    [Fact]
    public void AtualizarDescricao_ItemManual_DescricaoValida_Atualiza()
    {
        var item = CriarItem(origem: OrigemChecklistItem.Manual);
        item.AtualizarDescricao("Nova descrição");
        item.Descricao.Should().Be("Nova descrição");
    }

    [Fact]
    public void AtualizarDescricao_ItemAutomatico_PermiteEdicao()
    {
        // Mecânico tem controle total sobre o checklist da OS — pode editar
        // tanto itens manuais quanto os vindos do preset.
        var item = CriarItem(origem: OrigemChecklistItem.Automatico);
        item.AtualizarDescricao("Nova descrição");
        item.Descricao.Should().Be("Nova descrição");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AtualizarDescricao_DescricaoInvalida_LancaArgumentException(string novaDescricao)
    {
        var item = CriarItem(origem: OrigemChecklistItem.Manual);
        Action act = () => item.AtualizarDescricao(novaDescricao);
        act.Should().Throw<ArgumentException>().WithMessage("*Descrição*");
    }

    [Fact]
    public void AtualizarDescricao_DescricaoComEspacos_TrimAplicado()
    {
        var item = CriarItem(origem: OrigemChecklistItem.Manual);
        item.AtualizarDescricao("  Alinhamento  ");
        item.Descricao.Should().Be("Alinhamento");
    }

    // ==================== MÉTODO AUXILIAR ====================

    private static ChecklistOrdemServico CriarItem(
        StatusChecklistItem? status = null,
        OrigemChecklistItem origem = OrigemChecklistItem.Manual)
    {
        var item = new ChecklistOrdemServico(Guid.NewGuid(), "Verificar freios", origem, 1);
        if (status != null)
        {
            typeof(ChecklistOrdemServico)
                .GetProperty("Status")
                ?.SetValue(item, status);
        }
        return item;
    }
}