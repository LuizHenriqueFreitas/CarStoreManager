namespace CarStoreManager.Application.DTOs.Oficina.NotaFiscal;

public class NotaFiscalDTO
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = "";
    public string Serie { get; set; } = "";
    public string ChaveAcesso { get; set; } = "";
    public DateTime DataEmissao { get; set; }
    public DateTime DataImportacao { get; set; }
    public DateTime? DataAprovacao { get; set; }
    public string Status { get; set; } = "";
    public string? MotivoRejeicao { get; set; }

    public Guid FornecedorId { get; set; }
    public string FornecedorRazaoSocial { get; set; } = "";
    public string FornecedorCnpj { get; set; } = "";

    public decimal ValorProdutos { get; set; }
    public decimal ValorImpostos { get; set; }
    public decimal ValorTotal { get; set; }

    public List<ItemNotaFiscalDTO> Itens { get; set; } = new();
}

public class ItemNotaFiscalDTO
{
    public Guid Id { get; set; }
    public Guid? ComponenteId { get; set; }
    public string? ComponenteNome { get; set; }
    public string? ComponentePartNumber { get; set; }

    public string CodigoProdutoFornecedor { get; set; } = "";
    public string DescricaoProdutoFornecedor { get; set; } = "";
    public string Ncm { get; set; } = "";
    public string Unidade { get; set; } = "UN";

    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }

    public string Cfop { get; set; } = "";
    public decimal AliquotaIcms { get; set; }
    public decimal ValorIcms { get; set; }

    public string? NumeroLoteFornecedor { get; set; }
    public DateTime? DataValidade { get; set; }

    /// <summary>true se o admin já vinculou este item a um Componente.</summary>
    public bool Mapeado => ComponenteId is not null;
}

public class NotaFiscalListaDTO
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = "";
    public string Serie { get; set; } = "";
    public string ChaveAcesso { get; set; } = "";
    public DateTime DataEmissao { get; set; }
    public DateTime DataImportacao { get; set; }
    public string Status { get; set; } = "";
    public string FornecedorRazaoSocial { get; set; } = "";
    public string FornecedorCnpj { get; set; } = "";
    public decimal ValorTotal { get; set; }
    public int TotalItens { get; set; }
    public int ItensMapeados { get; set; }
}
