using CarStoreManager.Domain.Base;

namespace CarStoreManager.Domain.Entities.Concessionaria;

/// <summary>
/// Registro de vistoria do veículo, realizada antes da entrega.
/// Ciclo de vida:
///   1) Admin clica "iniciar vistoria" → cria a Vistoria com Concluida=false.
///      Nesse momento o admin pode anexar fotos (via Foto.EntidadeTipo="Vistoria").
///   2) Admin registra resultado (observações + aprovado/reprovado) → Concluida=true.
/// Quando reprovada, uma nova Vistoria pode ser aberta para a mesma proposta.
/// </summary>
public class Vistoria : Entity
{
    public Guid PropostaVendaId { get; private set; }

    public DateTime DataRealizada { get; private set; }
    public Guid VistoriadorId { get; private set; }
    public string Observacoes { get; private set; } = "";
    public bool Aprovado { get; private set; }

    /// <summary>
    /// False enquanto o admin está fazendo a vistoria (anexando fotos, observando).
    /// Vira true quando ele clica Aprovar ou Reprovar.
    /// </summary>
    public bool Concluida { get; private set; }

    public DateTime? DataConclusao { get; private set; }

    protected Vistoria() { }

    /// <summary>
    /// Cria uma vistoria preliminar (em andamento) — ainda sem resultado.
    /// O admin anexa fotos e depois chama <see cref="Registrar"/>.
    /// </summary>
    public Vistoria(Guid propostaVendaId, Guid vistoriadorId)
    {
        PropostaVendaId = propostaVendaId;
        VistoriadorId = vistoriadorId;
        DataRealizada = DateTime.UtcNow;
        Concluida = false;
    }

    /// <summary>
    /// Conclui a vistoria com observações + decisão de aprovado/reprovado.
    /// Idempotência: lança se chamada após já concluída.
    /// </summary>
    public void Registrar(string observacoes, bool aprovado)
    {
        if (Concluida)
            throw new InvalidOperationException("Vistoria já foi concluída.");
        if (string.IsNullOrWhiteSpace(observacoes))
            throw new ArgumentException("Observações da vistoria são obrigatórias.", nameof(observacoes));

        Observacoes = observacoes.Trim();
        Aprovado = aprovado;
        Concluida = true;
        DataConclusao = DateTime.UtcNow;
    }
}
