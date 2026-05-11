using CarStoreManager.Domain.Base;

namespace CarStoreManager.Domain.Entities.Concessionaria;

/// <summary>
/// Registro de vistoria do veículo, realizada antes da entrega.
/// Uma proposta tem no máximo uma vistoria. Pode reprovar (Aprovado=false),
/// caso em que admin pode lançar nova vistoria com a mesma proposta.
/// </summary>
public class Vistoria : Entity
{
    public Guid PropostaVendaId { get; private set; }

    public DateTime DataRealizada { get; private set; }
    public Guid VistoriadorId { get; private set; }
    public string Observacoes { get; private set; } = "";
    public bool Aprovado { get; private set; }

    protected Vistoria() { }

    public Vistoria(
        Guid propostaVendaId,
        Guid vistoriadorId,
        string observacoes,
        bool aprovado)
    {
        if (string.IsNullOrWhiteSpace(observacoes))
            throw new ArgumentException("Observações da vistoria são obrigatórias.", nameof(observacoes));

        PropostaVendaId = propostaVendaId;
        VistoriadorId = vistoriadorId;
        DataRealizada = DateTime.UtcNow;
        Observacoes = observacoes.Trim();
        Aprovado = aprovado;
    }
}
