namespace CarStoreManager.Domain.Enums;

/// <summary>
/// Classificação da despesa por setor — usada pelo dashboard para alocar a
/// despesa ao lucro do setor correto.
/// - Geral: compartilhada entre os dois setores (luz, água do prédio, etc.)
/// - Oficina: exclusiva da oficina (aluguel do galpão, ferramentas, salário do mecânico)
/// - Concessionaria: exclusiva da loja (showroom, salário do vendedor)
/// </summary>
public enum SetorDespesa
{
    Geral = 1,
    Oficina = 2,
    Concessionaria = 3
}
