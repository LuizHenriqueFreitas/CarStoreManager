//classe de base para os itens da ordem de servico, usado na ordem de servico

using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Oficina;

public class ItemOrdemServico : Entity
{
    public Guid ComponenteId { get; private set; }
    public Guid OrdemServicoId { get; private set; }

    public int Quantidade { get; private set; }

    public Dinheiro ValorUnitario { get; private set; } = null!;
    public Dinheiro ValorTotal { get; private set; } = null!;

    protected ItemOrdemServico() { }

    public ItemOrdemServico(
        Guid componenteId,
        Guid ordemServicoId,
        int quantidade,
        Dinheiro valorUnitario)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade inválida");

        ComponenteId = componenteId;
        OrdemServicoId = ordemServicoId;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;

        CalcularTotal();
    }

    // =========================
    // GETERS
    // =========================

    public Guid GetComponentId() => ComponenteId;
    public Guid GetOrdemServicoId() => OrdemServicoId;
    public int GetQuantidade() => Quantidade;
    public decimal GetValorUnitario() => ValorUnitario.Valor;
    public decimal GetValorTotal() => ValorTotal.Valor;

    // =========================
    // REGRAS DE NEGÓCIO - SETERS
    // =========================

    public void AlterarQuantidade(int novaQuantidade)
    {
        if (novaQuantidade <= 0)
            throw new ArgumentException("Quantidade inválida");

        Quantidade = novaQuantidade;
        CalcularTotal();
    }

    public void AtualizarValorUnitario(Dinheiro novoValor)
    {
        ValorUnitario = novoValor;
        CalcularTotal();
    }

    private void CalcularTotal()
    {
        ValorTotal = ValorUnitario.Multiplicar(Quantidade);
    }
}