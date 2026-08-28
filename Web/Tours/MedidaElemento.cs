namespace CarStoreManager.Web.Tours;

/// <summary>
/// Retorno de <c>tourGuiado.medir(seletor)</c> — geometria do elemento
/// destacado (em coordenadas de página, já somando o scroll) e o tamanho do
/// viewport, usados para calcular onde o destaque e o balão devem ficar.
/// </summary>
public sealed class MedidaElemento
{
    public bool Existe { get; set; }
    public double Top { get; set; }
    public double Left { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double ViewportWidth { get; set; }
    public double ViewportHeight { get; set; }
    public double ScrollY { get; set; }
}
