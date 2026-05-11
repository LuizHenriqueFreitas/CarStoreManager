namespace CarStoreManager.Application.DTOs.Oficina.NotaFiscal;

/// <summary>
/// Resultado intermediário do parser — não é persistido. Mantém os dados
/// extraídos do XML para o service decidir: criar Fornecedor novo? Reusar
/// existente? Já existe NF-e com essa chave?
/// </summary>
public class XmlNotaFiscalParseado
{
    public string ChaveAcesso { get; set; } = "";
    public string Numero { get; set; } = "";
    public string Serie { get; set; } = "";
    public DateTime DataEmissao { get; set; }
    public string XmlOriginal { get; set; } = "";

    public decimal ValorProdutos { get; set; }
    public decimal ValorImpostos { get; set; }
    public decimal ValorTotal { get; set; }

    // Emitente / fornecedor
    public string EmitenteCnpj { get; set; } = "";
    public string EmitenteRazaoSocial { get; set; } = "";
    public string EmitenteNomeFantasia { get; set; } = "";
    public string EmitenteEmail { get; set; } = "";
    public string EmitenteTelefone { get; set; } = "";

    // Destinatário (precisa bater com a oficina, mas guardamos para conferência)
    public string DestinatarioCnpj { get; set; } = "";
    public string DestinatarioRazaoSocial { get; set; } = "";

    public List<XmlItemNotaFiscalParseado> Itens { get; set; } = new();
}

public class XmlItemNotaFiscalParseado
{
    public string CodigoProdutoFornecedor { get; set; } = "";
    public string DescricaoProdutoFornecedor { get; set; } = "";
    public string Ncm { get; set; } = "";
    public string Unidade { get; set; } = "UN";
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public string Cfop { get; set; } = "";
    public string? Cst { get; set; }
    public string? Csosn { get; set; }
    public decimal AliquotaIcms { get; set; }
    public decimal ValorIcms { get; set; }
    public string? NumeroLoteFornecedor { get; set; }
    public DateTime? DataFabricacao { get; set; }
    public DateTime? DataValidade { get; set; }
}

public class VincularComponenteDTO
{
    public Guid ComponenteId { get; set; }
}

public class AlterarQuantidadeItemDTO
{
    public int Quantidade { get; set; }
}

public class RejeitarNotaDTO
{
    public string Motivo { get; set; } = "";
}
