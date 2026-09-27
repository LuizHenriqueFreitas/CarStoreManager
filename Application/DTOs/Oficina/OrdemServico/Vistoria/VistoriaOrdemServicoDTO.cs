namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class VistoriaOrdemServicoDTO
{
    public Guid Id { get; set; }
    public Guid OrdemServicoId { get; set; }
    public Guid RecepcionistaId { get; set; }
    public string TextoContrato { get; set; } = "";
    public DateTime DataInicio { get; set; }
    public DateTime? DataConclusao { get; set; }
    public bool Concluida { get; set; }
}
