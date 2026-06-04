using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Sistema;

/// <summary>
/// Despesa mensal recorrente cadastrada pelo administrador na tela do dashboard.
/// Compõe a planilha usada para cálculo de custos (luz, água, aluguel, salários,
/// etc.). Cada despesa é classificada por um <see cref="TipoDespesa"/> cadastrável.
/// </summary>
public class Despesa : Entity
{
    public string Nome { get; private set; } = null!;
    public Dinheiro Valor { get; private set; } = null!;
    public bool Ativa { get; private set; }

    /// <summary>Tipo/categoria da despesa (Aluguel, Água, Salários, etc.).</summary>
    public Guid TipoDespesaId { get; private set; }

    public DateTime? DataUltimaAtualizacao { get; private set; }

    protected Despesa() { }

    public Despesa(string nome, decimal valor, Guid tipoDespesaId)
    {
        AtualizarNome(nome);
        Valor = new Dinheiro(valor);
        TipoDespesaId = tipoDespesaId;
        Ativa = true;
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

    public void AtualizarTipo(Guid tipoDespesaId)
    {
        if (tipoDespesaId == Guid.Empty)
            throw new ArgumentException("Tipo de despesa é obrigatório.", nameof(tipoDespesaId));

        TipoDespesaId = tipoDespesaId;
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
