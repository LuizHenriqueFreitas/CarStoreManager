namespace CarStoreManager.Application.DTOs.Sistema.TemplateDocumento;

public class TemplateDocumentoDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public string Conteudo { get; set; } = "";
    public bool Ativo { get; set; }
    public DateTime? DataUltimaAtualizacao { get; set; }
}

/// <summary>Usado nos seletores de template espalhados pelo sistema (consignação, termo de entrega, financiamento, etc.) — não carrega o conteúdo inteiro na lista, só o suficiente para montar o dropdown.</summary>
public class TemplateDocumentoLookupDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
}

public class SalvarTemplateDocumentoDTO
{
    /// <summary>Vazio/Empty no POST de criação; preenchido no PUT.</summary>
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public string Conteudo { get; set; } = "";
    public bool Ativo { get; set; } = true;
}
