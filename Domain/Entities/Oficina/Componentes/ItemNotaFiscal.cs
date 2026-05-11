using CarStoreManager.Domain.Base;

namespace CarStoreManager.Domain.Entities.Oficina;

/// <summary>
/// Linha (det) da NF-e. <see cref="ComponenteId"/> é nullable porque o admin
/// só faz a vinculação após a importação — o XML traz o código do FORNECEDOR
/// (cProd/xProd), que precisa ser conciliado com o cadastro interno.
/// </summary>
public class ItemNotaFiscal : Entity
{
    public Guid NotaFiscalId { get; private set; }
    public NotaFiscal NotaFiscal { get; private set; } = null!;

    public Guid? ComponenteId { get; private set; }
    public Componente? Componente { get; private set; }

    /// <summary>cProd — código do produto conforme o fornecedor.</summary>
    public string CodigoProdutoFornecedor { get; private set; } = null!;

    /// <summary>xProd — descrição do produto conforme o fornecedor.</summary>
    public string DescricaoProdutoFornecedor { get; private set; } = null!;

    public string Ncm { get; private set; } = null!;
    public string Unidade { get; private set; } = null!;

    public int Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public decimal ValorTotal { get; private set; }

    public string Cfop { get; private set; } = null!;
    public string? Cst { get; private set; }
    public string? Csosn { get; private set; }
    public decimal AliquotaIcms { get; private set; }
    public decimal ValorIcms { get; private set; }

    /// <summary>Número do lote informado pelo fornecedor (rastro do XML, opcional).</summary>
    public string? NumeroLoteFornecedor { get; private set; }
    public DateTime? DataFabricacao { get; private set; }
    public DateTime? DataValidade { get; private set; }

    protected ItemNotaFiscal() { }

    public ItemNotaFiscal(
        Guid notaFiscalId,
        string codigoProdutoFornecedor,
        string descricaoProdutoFornecedor,
        string ncm,
        string unidade,
        int quantidade,
        decimal valorUnitario,
        string cfop,
        string? cst,
        string? csosn,
        decimal aliquotaIcms,
        decimal valorIcms,
        string? numeroLoteFornecedor = null,
        DateTime? dataFabricacao = null,
        DateTime? dataValidade = null)
    {
        if (string.IsNullOrWhiteSpace(codigoProdutoFornecedor))
            throw new ArgumentException("Código do produto é obrigatório.", nameof(codigoProdutoFornecedor));
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser positiva.", nameof(quantidade));
        if (valorUnitario < 0)
            throw new ArgumentException("Valor unitário não pode ser negativo.", nameof(valorUnitario));

        NotaFiscalId = notaFiscalId;
        CodigoProdutoFornecedor = codigoProdutoFornecedor.Trim();
        DescricaoProdutoFornecedor = descricaoProdutoFornecedor?.Trim() ?? string.Empty;
        Ncm = ncm?.Trim() ?? string.Empty;
        Unidade = unidade?.Trim() ?? "UN";
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
        ValorTotal = quantidade * valorUnitario;
        Cfop = cfop?.Trim() ?? string.Empty;
        Cst = cst?.Trim();
        Csosn = csosn?.Trim();
        AliquotaIcms = aliquotaIcms;
        ValorIcms = valorIcms;
        NumeroLoteFornecedor = numeroLoteFornecedor?.Trim();
        DataFabricacao = dataFabricacao;
        DataValidade = dataValidade;
    }

    public void VincularComponente(Guid componenteId)
    {
        if (componenteId == Guid.Empty)
            throw new ArgumentException("ComponenteId inválido.", nameof(componenteId));
        ComponenteId = componenteId;
    }

    public void DesvincularComponente() => ComponenteId = null;

    public void AlterarQuantidade(int novaQuantidade)
    {
        if (novaQuantidade <= 0)
            throw new ArgumentException("Quantidade deve ser positiva.", nameof(novaQuantidade));
        Quantidade = novaQuantidade;
        ValorTotal = novaQuantidade * ValorUnitario;
    }
}
