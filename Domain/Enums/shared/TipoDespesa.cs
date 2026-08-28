namespace CarStoreManager.Domain.Enums;

/// <summary>
/// Classificação da despesa por natureza contábil — usada para separar, por
/// exemplo, quanto é gasto com salários versus manutenção versus investimento.
/// Ortogonal a <see cref="SetorDespesa"/> (que classifica por setor).
/// </summary>
public enum TipoDespesa
{
    Salario = 1,
    Aluguel = 2,
    Utilidades = 3,
    Manutencao = 4,
    Marketing = 5,
    Impostos = 6,
    Seguro = 7,
    Investimento = 8,
    Servicos = 9,
    Outros = 10
}
