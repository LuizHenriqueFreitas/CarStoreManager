using CarStoreManager.Domain.Base;

namespace CarStoreManager.Domain.Entities.Sistema;

/// <summary>
/// Tipo/categoria de despesa cadastrável pelo administrador (ex.: Aluguel, Água,
/// Energia, Internet, Salários, Manutenção, Investimento). Substitui a antiga
/// divisão por setor — agora as despesas são classificadas por tipo, o que gera
/// dados mais relevantes para relatórios.
/// </summary>
public class TipoDespesa : Entity
{
    public string Nome { get; private set; } = null!;
    public bool Ativo { get; private set; }
    public DateTime? DataUltimaAtualizacao { get; private set; }

    protected TipoDespesa() { }

    public TipoDespesa(string nome)
    {
        Renomear(nome);
        Ativo = true;
    }

    public void Renomear(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do tipo de despesa é obrigatório.", nameof(nome));

        Nome = nome.Trim();
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    public void Desativar()
    {
        Ativo = false;
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    public void Reativar()
    {
        Ativo = true;
        DataUltimaAtualizacao = DateTime.UtcNow;
    }
}
