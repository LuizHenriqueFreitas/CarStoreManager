namespace CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

/// <summary>
/// Um valor de uma lista fechada de um atributo de categoria (ex.: para o
/// atributo "Câmbio", os valores podem ser {"7085308","Manual"} e
/// {"7085309","Automático"}). O <c>Id</c> é o que a API do ML espera de volta
/// em <c>value_id</c> — nunca o texto.
/// </summary>
public class ValorCatalogoDTO
{
    public string Id { get; set; } = "";
    public string Nome { get; set; } = "";
}

/// <summary>
/// Um atributo de categoria do ML, já classificado entre obrigatório/opcional
/// pelo <see cref="MercadoLivreCatalogoService"/> (a partir da tag "required"
/// devolvida por GET /categories/{id}/attributes).
/// </summary>
public class AtributoCategoriaDTO
{
    public string Id { get; set; } = "";
    public string Nome { get; set; } = "";

    /// <summary>
    /// Verdadeiro quando o atributo aceita texto livre (value_type diferente de
    /// "list", ou lista sem valores fechados) — nesses casos manda-se
    /// "value_name" em vez de procurar um "value_id" no catálogo.
    /// </summary>
    public bool AceitaTextoLivre { get; set; }

    /// <summary>Só populado quando o atributo tem lista fechada de valores.</summary>
    public List<ValorCatalogoDTO> ValoresPermitidos { get; set; } = new();
}

/// <summary>
/// Objeto consolidado com tudo que a categoria dita sobre como montar um
/// anúncio nela — é o retorno do método de conveniência do
/// <see cref="MercadoLivreCatalogoService"/> que os construtores de payload
/// (<see cref="CarStoreManager.Application.Interfaces.IConstrutorPayloadAnuncio"/>)
/// consultam em vez de decidir isso por conta própria.
/// </summary>
public class CategoriaMetadadosDTO
{
    public string CategoriaId { get; set; } = "";
    public string NomeCategoria { get; set; } = "";

    /// <summary>Vem de settings.buying_modes conter "classified" — muda todo o resto do payload (sem shipping, atributos descritivos obrigatórios, etc).</summary>
    public bool EhClassificado { get; set; }

    public string BuyingMode { get; set; } = "";

    /// <summary>Modalidade escolhida entre as disponíveis para a categoria+conta — a mais barata (gratuita, se houver). Nunca fixo no código.</summary>
    public string ListingTypeId { get; set; } = "";

    public List<AtributoCategoriaDTO> AtributosObrigatorios { get; set; } = new();
    public List<AtributoCategoriaDTO> AtributosOpcionais { get; set; } = new();
}

/// <summary>Resultado de GET /sites/{site}/domain_discovery/search para um título — categoria mais provável segundo o ML.</summary>
public class SugestaoCategoriaDTO
{
    public string CategoriaId { get; set; } = "";
    public string NomeCategoria { get; set; } = "";
}
