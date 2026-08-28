using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities.Concessionaria;

/// <summary>Log imutável de eventos de uma consignação. Usa Entity.DataCriacao como timestamp do evento.</summary>
public class HistoricoConsignacao : Entity
{
    public Guid VeiculoConsignacaoId { get; private set; }
    public TipoEventoConsignacao TipoEvento { get; private set; }
    public string Descricao { get; private set; } = null!;
    public Guid? UsuarioResponsavelId { get; private set; }

    protected HistoricoConsignacao() { }

    public HistoricoConsignacao(
        Guid veiculoConsignacaoId,
        TipoEventoConsignacao tipoEvento,
        string descricao,
        Guid? usuarioResponsavelId = null)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição é obrigatória.", nameof(descricao));

        VeiculoConsignacaoId = veiculoConsignacaoId;
        TipoEvento = tipoEvento;
        Descricao = descricao.Trim();
        UsuarioResponsavelId = usuarioResponsavelId;
    }
}
