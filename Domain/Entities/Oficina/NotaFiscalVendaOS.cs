using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Oficina;

/// <summary>
/// Registro fiscal de SAÍDA gerado automaticamente quando uma OS é finalizada.
/// Representa a nota de venda dos serviços + peças ao cliente.
///
/// Diferente da <see cref="NotaFiscal"/> (entrada de mercadoria), aqui não há
/// integração com SEFAZ — é um registro interno auditável. A geração de XML
/// real (NFC-e/NFS-e) requer assinatura digital + comunicação com fisco e
/// fica como extensão futura.
/// </summary>
public class NotaFiscalVendaOS : Entity
{
    public Guid OrdemServicoId { get; private set; }
    public Guid ClienteId { get; private set; }

    /// <summary>Número interno sequencial (legível, ex: NFV-2026-00042).</summary>
    public string Numero { get; private set; } = "";

    public DateTime DataEmissao { get; private set; }
    public DateTime? DataCancelamento { get; private set; }
    public string? MotivoCancelamento { get; private set; }

    public Dinheiro ValorServico { get; private set; } = null!;
    public Dinheiro ValorPecas { get; private set; } = null!;
    public Dinheiro ValorTotal { get; private set; } = null!;

    /// <summary>Snapshot dos itens (desnormalizado para preservar histórico).</summary>
    public string ItensJson { get; private set; } = "[]";

    /// <summary>Snapshot do cliente no momento da emissão.</summary>
    public string ClienteSnapshotJson { get; private set; } = "{}";

    public bool Cancelada => DataCancelamento.HasValue;

    protected NotaFiscalVendaOS() { }

    public NotaFiscalVendaOS(
        Guid ordemServicoId,
        Guid clienteId,
        string numero,
        decimal valorServico,
        decimal valorPecas,
        string itensJson,
        string clienteSnapshotJson)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("Número da nota é obrigatório.", nameof(numero));
        if (valorServico < 0 || valorPecas < 0)
            throw new ArgumentException("Valores não podem ser negativos.");

        OrdemServicoId = ordemServicoId;
        ClienteId = clienteId;
        Numero = numero;
        ValorServico = new Dinheiro(valorServico);
        ValorPecas = new Dinheiro(valorPecas);
        ValorTotal = new Dinheiro(valorServico + valorPecas);
        ItensJson = itensJson ?? "[]";
        ClienteSnapshotJson = clienteSnapshotJson ?? "{}";
        DataEmissao = DateTime.UtcNow;
    }

    public void Cancelar(string motivo)
    {
        if (Cancelada) throw new InvalidOperationException("Nota já cancelada.");
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Motivo é obrigatório.", nameof(motivo));
        DataCancelamento = DateTime.UtcNow;
        MotivoCancelamento = motivo.Trim();
    }
}
