using CarStoreManager.Domain.Base;

namespace CarStoreManager.Domain.Entities.Oficina;

public class EstoqueComponente : Entity
{
    public Guid PecaId { get; private set; }

    public Componente Componente { get; private set; } = null!;

    public int QuantidadeAtual { get; private set; }

    public int QuantidadeMinima { get; private set; }

    protected EstoqueComponente() {}

    public EstoqueComponente(
        Guid pecaId,
        int quantidadeMinima)
    {
        PecaId = pecaId;
        QuantidadeMinima = quantidadeMinima;
    }

    public void Adicionar(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser positiva.", nameof(quantidade));
        QuantidadeAtual += quantidade;
    }

    public void Remover(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser positiva.", nameof(quantidade));
        if (QuantidadeAtual < quantidade)
            throw new InvalidOperationException("Estoque insuficiente");

        QuantidadeAtual -= quantidade;
    }

    public void DefinirMinimo(int quantidadeMinima)
    {
        if (quantidadeMinima < 0)
            throw new ArgumentException("Quantidade mínima não pode ser negativa.", nameof(quantidadeMinima));
        QuantidadeMinima = quantidadeMinima;
    }
}