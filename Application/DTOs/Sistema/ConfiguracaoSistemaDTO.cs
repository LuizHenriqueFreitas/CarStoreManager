namespace CarStoreManager.Application.DTOs.Sistema;

public class ConfiguracaoSistemaDTO
{
    public DateTime? DataUltimaAtualizacao { get; set; }

    // === Modo operante — entrada mínima ===
    public bool ExigirEntradaMinima { get; set; } = false;
    public decimal PercentualEntradaMinima { get; set; } = 0m;
}

public class MargensDTO
{
    /// <summary>Mapa: nome do SistemaComponente (Motor, Freios, etc) → percentual.</summary>
    public Dictionary<string, decimal> MargensPorSistema { get; set; } = new();
    public decimal MargemPadraoGlobalPct { get; set; } = 30m;
}
