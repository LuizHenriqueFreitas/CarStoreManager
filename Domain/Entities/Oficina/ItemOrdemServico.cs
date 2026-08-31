using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Oficina;

/*
    Cada item representa uma peça/componente atrelado a uma Ordem de Serviço.
    Pode vir de 3 origens (Estoque, Cliente, Encomenda) e quando é Encomenda
    seu StatusItem fica AguardandoChegada até a peça ser recebida no estoque.
*/
public class ItemOrdemServico : Entity
{
    // Null quando Origem=Cliente — peça trazida pelo cliente não tem cadastro
    // no catálogo, não passa pelo estoque e não tem valor. Só existe pra
    // registro (ver DescricaoLivre).
    public Guid? ComponenteId { get; private set; }
    public Guid OrdemServicoId { get; private set; }

    public int Quantidade { get; private set; }

    public Dinheiro ValorUnitario { get; private set; } = null!;
    public Dinheiro ValorTotal { get; private set; } = null!;

    public OrigemItemOrdemServico Origem { get; private set; }
    public StatusItemOrdemServico StatusItem { get; private set; }

    // Quando o item foi recebido (transição AguardandoChegada → Recebido).
    public DateTime? DataRecebimento { get; private set; }

    /// <summary>Nome digitado na hora — só preenchido quando Origem=Cliente (ComponenteId nulo).</summary>
    public string? DescricaoLivre { get; private set; }

    protected ItemOrdemServico() { }

    /// <summary>Peça do catálogo (Estoque ou Encomenda) — referencia um Componente real, com valor vindo do cadastro.</summary>
    public ItemOrdemServico(
        Guid componenteId,
        Guid ordemServicoId,
        int quantidade,
        decimal valorUnitario,
        OrigemItemOrdemServico origem = OrigemItemOrdemServico.Estoque)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade inválida");
        if (componenteId == Guid.Empty)
            throw new ArgumentException("Componente inválido.", nameof(componenteId));
        if (origem == OrigemItemOrdemServico.Cliente)
            throw new ArgumentException(
                "Peça trazida pelo cliente não referencia um componente do catálogo — use o construtor de item avulso.",
                nameof(origem));

        ComponenteId = componenteId;
        OrdemServicoId = ordemServicoId;
        Quantidade = quantidade;
        ValorUnitario = new Dinheiro(valorUnitario);
        Origem = origem;

        // Encomenda começa aguardando; demais já estão disponíveis.
        StatusItem = origem == OrigemItemOrdemServico.Encomenda
            ? StatusItemOrdemServico.AguardandoChegada
            : StatusItemOrdemServico.Disponivel;

        CalcularTotal();
    }

    /// <summary>
    /// Peça trazida pelo cliente — sem catálogo, sem estoque, sem valor. Só
    /// um nome digitado na hora da marcação na OS, pra fins de registro.
    /// </summary>
    public ItemOrdemServico(Guid ordemServicoId, int quantidade, string descricaoLivre)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade inválida");
        if (string.IsNullOrWhiteSpace(descricaoLivre))
            throw new ArgumentException("Descrição da peça é obrigatória.", nameof(descricaoLivre));

        ComponenteId = null;
        OrdemServicoId = ordemServicoId;
        Quantidade = quantidade;
        ValorUnitario = new Dinheiro(0);
        Origem = OrigemItemOrdemServico.Cliente;
        StatusItem = StatusItemOrdemServico.Disponivel;
        DescricaoLivre = descricaoLivre.Trim();

        CalcularTotal();
    }

    /* ================================
        metodos GETTERS dos atributos
     ================================*/
    public Guid? GetComponentId() => ComponenteId;
    public Guid GetOrdemServicoId() => OrdemServicoId;
    public int GetQuantidade() => Quantidade;
    public decimal GetValorUnitario() => ValorUnitario.GetValorDinheiro();
    public decimal GetValorTotal() => ValorTotal.GetValorDinheiro();
    public string GetOrigem() => Origem.ToString();
    public string GetStatusItem() => StatusItem.ToString();

    /* =====================================
        metodos SETTERS de cada atributo
        com regras de negocio aplicadas
     =====================================*/

    public void AlterarQuantidade(int novaQuantidade)
    {
        if (novaQuantidade <= 0)
            throw new ArgumentException("Quantidade inválida");

        Quantidade = novaQuantidade;
        CalcularTotal();
    }

    public void AtualizarValorUnitario(decimal novoValor)
    {
        ValorUnitario = new Dinheiro(novoValor);
        CalcularTotal();
    }

    /*
        Marca o item como recebido (encomenda chegou).
        Idempotente: chamar duas vezes não causa erro.
    */
    public void MarcarComoRecebido()
    {
        if (StatusItem == StatusItemOrdemServico.Recebido) return;

        if (Origem != OrigemItemOrdemServico.Encomenda)
            throw new InvalidOperationException(
                "Só itens em Encomenda podem ser marcados como Recebido.");

        if (StatusItem != StatusItemOrdemServico.AguardandoChegada)
            throw new InvalidOperationException(
                $"Status atual ({StatusItem}) não permite marcar como recebido.");

        StatusItem = StatusItemOrdemServico.Recebido;
        DataRecebimento = DateTime.UtcNow;
    }

    /*
        calcula o total, multiplica o valor unitário pela quantidade
        que foi informada no formulario da Ordem de Servico
    */
    private void CalcularTotal()
    {
        ValorTotal = ValorUnitario.Multiplicar(Quantidade);
    }
}
