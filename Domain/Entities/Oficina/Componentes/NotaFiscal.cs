using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities.Oficina;

/// <summary>
/// Cabeçalho da NF-e de entrada (modelo 55). O XML original é preservado para
/// fins de auditoria fiscal (chave + DANFE são reconstruíveis a partir dele).
/// </summary>
public class NotaFiscal : Entity
{
    public TipoNotaFiscal Tipo { get; private set; }

    public string Numero { get; private set; } = null!;
    public string Serie { get; private set; } = null!;

    /// <summary>
    /// Chave de acesso de 44 dígitos — identificador fiscal único da NF-e.
    /// </summary>
    public string ChaveAcesso { get; private set; } = null!;

    public DateTime DataEmissao { get; private set; }

    /// <summary>XML original conforme recebido (não normalizado).</summary>
    public string XmlConteudo { get; private set; } = null!;

    public decimal ValorProdutos { get; private set; }
    public decimal ValorImpostos { get; private set; }
    public decimal ValorTotal { get; private set; }

    public StatusNotaFiscal Status { get; private set; }

    public DateTime DataImportacao { get; private set; }
    public DateTime? DataAprovacao { get; private set; }
    public string? MotivoRejeicao { get; private set; }

    public Guid FornecedorId { get; private set; }
    public Fornecedor Fornecedor { get; private set; } = null!;

    public ICollection<ItemNotaFiscal> Itens { get; private set; }
        = new List<ItemNotaFiscal>();

    protected NotaFiscal() { }

    public NotaFiscal(
        TipoNotaFiscal tipo,
        string numero,
        string serie,
        string chaveAcesso,
        DateTime dataEmissao,
        string xmlConteudo,
        decimal valorProdutos,
        decimal valorImpostos,
        decimal valorTotal,
        Guid fornecedorId)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("Número da nota é obrigatório.", nameof(numero));
        if (string.IsNullOrWhiteSpace(chaveAcesso) || chaveAcesso.Length != 44)
            throw new ArgumentException("Chave de acesso da NF-e deve ter 44 dígitos.", nameof(chaveAcesso));
        if (string.IsNullOrWhiteSpace(xmlConteudo))
            throw new ArgumentException("XML original é obrigatório.", nameof(xmlConteudo));

        Tipo = tipo;
        Numero = numero;
        Serie = serie ?? string.Empty;
        ChaveAcesso = chaveAcesso;
        DataEmissao = dataEmissao;
        XmlConteudo = xmlConteudo;
        ValorProdutos = valorProdutos;
        ValorImpostos = valorImpostos;
        ValorTotal = valorTotal;
        FornecedorId = fornecedorId;

        Status = StatusNotaFiscal.ImportadaAguardandoAprovacao;
        DataImportacao = DateTime.UtcNow;
    }

    public void AdicionarItem(ItemNotaFiscal item)
    {
        if (Status != StatusNotaFiscal.ImportadaAguardandoAprovacao)
            throw new InvalidOperationException("Itens só podem ser adicionados enquanto a nota está pendente.");
        Itens.Add(item);
    }

    /// <summary>
    /// Aprova a nota — pré-condição: TODOS os itens estão mapeados a um Componente.
    /// O efeito de estoque (criar lotes + adicionar quantidade) é executado pelo
    /// service, não aqui (a entidade não conhece outros agregados).
    /// </summary>
    public void Aprovar()
    {
        if (Status != StatusNotaFiscal.ImportadaAguardandoAprovacao)
            throw new InvalidOperationException($"Nota não pode ser aprovada no status atual: {Status}.");

        if (Itens.Count == 0)
            throw new InvalidOperationException("Nota sem itens não pode ser aprovada.");

        if (Itens.Any(i => i.ComponenteId is null))
            throw new InvalidOperationException("Todos os itens precisam estar vinculados a um componente antes da aprovação.");

        Status = StatusNotaFiscal.Aprovada;
        DataAprovacao = DateTime.UtcNow;
    }

    public void Rejeitar(string motivo)
    {
        if (Status != StatusNotaFiscal.ImportadaAguardandoAprovacao)
            throw new InvalidOperationException($"Nota não pode ser rejeitada no status atual: {Status}.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Motivo da rejeição é obrigatório.", nameof(motivo));

        Status = StatusNotaFiscal.Rejeitada;
        MotivoRejeicao = motivo.Trim();
    }
}
