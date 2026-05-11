namespace CarStoreManager.Domain.Enums;

/// <summary>
/// Ciclo de vida da proposta de venda.
///
/// Fluxo feliz:
/// Rascunho → Criada → [opcional: AguardandoFinanciadora → PropostaFinanciadoraRecebida]
///                  → Aprovada → AguardandoVistoria → VistoriaConcluida
///                  → AguardandoAssinaturaTermo → Concluida
///
/// Terminais: Concluida, Rejeitada, Cancelada, Expirada.
/// Rejeitada / Cancelada podem acontecer a qualquer momento (exceto se já terminal).
/// Expirada é setada automaticamente após 7 dias sem progresso.
/// </summary>
public enum StatusPropostaVenda
{
    Rascunho = 0,
    Criada = 1,

    AguardandoFinanciadora = 6,
    PropostaFinanciadoraRecebida = 7,

    Aprovada = 3,

    AguardandoVistoria = 9,
    VistoriaConcluida = 10,
    AguardandoAssinaturaTermo = 11,
    Concluida = 12,

    Rejeitada = 4,
    Cancelada = 5,
    Expirada = 13,

    // === Legado (não use em código novo) ===
    Enviada = 2
}
