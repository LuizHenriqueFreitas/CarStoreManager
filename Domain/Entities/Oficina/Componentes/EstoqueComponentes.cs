using CarStoreManager.Domain.Base;

namespace Oficina.Domain.Entities;

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
        QuantidadeAtual += quantidade;
    }

    public void Remover(int quantidade)
    {
        if (QuantidadeAtual < quantidade)
            throw new Exception("Estoque insuficiente");

        QuantidadeAtual -= quantidade;
    }
}