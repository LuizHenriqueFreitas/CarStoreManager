using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Sistema;

/// <summary>
/// Despesa mensal recorrente cadastrada pelo administrador na tela de
/// configurações. Compõe a planilha usada para cálculo de custos da oficina
/// (luz, água, aluguel, salários, etc.).
/// </summary>
public class Despesa : Entity
{
    public string Nome { get; private set; } = null!;
    public Dinheiro Valor { get; private set; } = null!;
    public bool Ativa { get; private set; }
    public SetorDespesa Setor { get; private set; } = SetorDespesa.Geral;
    public TipoDespesa Tipo { get; private set; } = TipoDespesa.Outros;

    /// <summary>
    /// Categoria em texto livre escrita pelo administrador (energia, aluguel,
    /// investimento, perda de operação...). É o que o usuário vê e edita — o
    /// enum <see cref="Tipo"/> fica só por compatibilidade.
    /// Ver docs/redesign/11-batch2-melhorias.md §G.
    /// </summary>
    public string? Categoria { get; private set; }

    public DateTime? DataUltimaAtualizacao { get; private set; }

    protected Despesa() { }

    public Despesa(string nome, decimal valor, SetorDespesa setor = SetorDespesa.Geral,
        TipoDespesa tipo = TipoDespesa.Outros, string? categoria = null)
    {
        AtualizarNome(nome);
        Valor = new Dinheiro(valor);
        Setor = setor;
        Tipo = tipo;
        Categoria = string.IsNullOrWhiteSpace(categoria) ? null : categoria.Trim();
        Ativa = true;
    }

    public void AtualizarCategoria(string? categoria)
    {
        Categoria = string.IsNullOrWhiteSpace(categoria) ? null : categoria.Trim();
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    public void AtualizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da despesa é obrigatório.", nameof(nome));

        Nome = nome.Trim();
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    public void AtualizarValor(decimal valor)
    {
        Valor = new Dinheiro(valor);
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    public void Atualizar(string nome, decimal valor)
    {
        AtualizarNome(nome);
        AtualizarValor(valor);
    }

    public void AtualizarSetor(SetorDespesa setor)
    {
        Setor = setor;
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    public void AtualizarTipo(TipoDespesa tipo)
    {
        Tipo = tipo;
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    public void Desativar()
    {
        Ativa = false;
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    public void Reativar()
    {
        Ativa = true;
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    public decimal GetValor() => Valor.GetValorDinheiro();
}
