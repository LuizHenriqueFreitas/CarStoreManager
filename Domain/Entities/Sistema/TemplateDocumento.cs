using CarStoreManager.Domain.Base;

namespace CarStoreManager.Domain.Entities.Sistema;

/// <summary>
/// Modelo de documento/contrato editável pelo admin na tela de configurações —
/// mesmo padrão de ChecklistPreset (Oficina): múltiplos presets nomeados, cada
/// um com um texto completo, e quem for redigir um contrato/termo em qualquer
/// tela do sistema (consignação, termo de entrega, resposta de financiadora,
/// termo de test-drive futuramente, etc.) escolhe um preset como ponto de
/// partida — ou escreve do zero — e o texto é copiado como snapshot editável
/// (alterações futuras no preset não afetam documentos já redigidos).
/// </summary>
public class TemplateDocumento : Entity
{
    public string Nome { get; private set; } = null!;
    public string Conteudo { get; private set; } = null!;
    public bool Ativo { get; private set; }
    public DateTime? DataUltimaAtualizacao { get; private set; }

    protected TemplateDocumento() { }

    public TemplateDocumento(string nome, string conteudo)
    {
        AtualizarNome(nome);
        AtualizarConteudo(conteudo);
        Ativo = true;
    }

    public void AtualizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do template é obrigatório.", nameof(nome));
        if (nome.Length > 150)
            throw new ArgumentException("Nome do template não pode ter mais de 150 caracteres.", nameof(nome));

        Nome = nome.Trim();
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    public void AtualizarConteudo(string conteudo)
    {
        if (string.IsNullOrWhiteSpace(conteudo))
            throw new ArgumentException("Conteúdo do template é obrigatório.", nameof(conteudo));

        Conteudo = conteudo.Trim();
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
