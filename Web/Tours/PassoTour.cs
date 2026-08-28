namespace CarStoreManager.Web.Tours;

/// <summary>
/// Um passo do tour guiado de uma tela. Ver <see cref="RegistroDeTours"/> para
/// o porquê de os textos viverem em código (não no banco).
/// </summary>
public sealed class PassoTour
{
    /// <summary>
    /// Valor do atributo <c>data-tour</c> do elemento destacado no markup da
    /// página (ex.: <c>data-tour="campo-placa"</c> → Seletor = "campo-placa").
    /// Null = passo centralizado na tela, sem destacar nada — usado na
    /// introdução e no encerramento de cada tour.
    /// </summary>
    public string? Seletor { get; init; }

    /// <summary>Título curto do passo (até ~40 caracteres).</summary>
    public required string Titulo { get; init; }

    /// <summary>Explicação do elemento. 1 a 3 frases.</summary>
    public required string Texto { get; init; }

    /// <summary>Posição preferida do balão: "auto", "acima", "abaixo", "esquerda", "direita".</summary>
    public string Posicao { get; init; } = "auto";
}
