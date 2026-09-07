using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Sistema;

/// <summary>
/// Balanço de despesas de um mês (competência). O gerente preenche os valores
/// reais a partir do formulário-modelo, pode editar e adicionar linhas, e
/// "fecha" o balanço no dia configurado. Cada competência fica guardada para
/// sempre — base para análises retro. Ver docs/redesign/11-batch2-melhorias.md §G.
/// </summary>
public class BalancoMensalDespesa : Entity
{
    /// <summary>Primeiro dia do mês de competência (ex.: 2026-09-01).</summary>
    public DateOnly Competencia { get; private set; }
    public bool Fechado { get; private set; }
    public DateTime? DataFechamento { get; private set; }

    public List<ItemBalancoDespesa> Itens { get; private set; } = new();

    protected BalancoMensalDespesa() { }

    public BalancoMensalDespesa(DateOnly competencia)
    {
        Competencia = new DateOnly(competencia.Year, competencia.Month, 1);
    }

    public decimal Total() => Itens.Sum(i => i.Valor.GetValorDinheiro());
    public decimal TotalPorSetor(SetorDespesa setor) =>
        Itens.Where(i => i.Setor == setor).Sum(i => i.Valor.GetValorDinheiro());

    public ItemBalancoDespesa AdicionarItem(string nome, SetorDespesa setor, string? categoria, decimal valor, bool doModelo = false)
    {
        if (Fechado) throw new InvalidOperationException("Balanço fechado — reabra para editar.");
        var item = new ItemBalancoDespesa(Id, nome, setor, categoria, valor, doModelo);
        Itens.Add(item);
        return item;
    }

    public void RemoverItem(Guid itemId)
    {
        if (Fechado) throw new InvalidOperationException("Balanço fechado — reabra para editar.");
        var item = Itens.FirstOrDefault(i => i.Id == itemId);
        if (item is not null) Itens.Remove(item);
    }

    public void Fechar()
    {
        Fechado = true;
        DataFechamento = DateTime.UtcNow;
    }

    public void Reabrir()
    {
        Fechado = false;
        DataFechamento = null;
    }
}

public class ItemBalancoDespesa : Entity
{
    public Guid BalancoId { get; private set; }
    public string Nome { get; private set; } = null!;
    public SetorDespesa Setor { get; private set; }
    public string? Categoria { get; private set; }
    public Dinheiro Valor { get; private set; } = null!;

    /// <summary>Se a linha veio do formulário-modelo (para diferenciar visualmente).</summary>
    public bool DoModelo { get; private set; }

    protected ItemBalancoDespesa() { }

    public ItemBalancoDespesa(Guid balancoId, string nome, SetorDespesa setor, string? categoria, decimal valor, bool doModelo)
    {
        BalancoId = balancoId;
        Atualizar(nome, setor, categoria, valor);
        DoModelo = doModelo;
    }

    public void Atualizar(string nome, SetorDespesa setor, string? categoria, decimal valor)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da despesa é obrigatório.", nameof(nome));
        Nome = nome.Trim();
        Setor = setor;
        Categoria = string.IsNullOrWhiteSpace(categoria) ? null : categoria.Trim();
        Valor = new Dinheiro(valor);
    }
}
