namespace CarStoreManager.Domain.Enums;

/// <summary>
/// Estado de um alerta emitido pelo mecânico durante a execução da OS.
/// Cliente pode aprovar (escopo aumentado segue) ou recusar (continua só com o original).
/// </summary>
public enum StatusAlertaOS
{
    Pendente = 0,
    ClienteAprovou = 1,
    ClienteRecusou = 2
}
