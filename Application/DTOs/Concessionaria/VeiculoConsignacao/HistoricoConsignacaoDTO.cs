namespace CarStoreManager.Application.DTOs.Concessionaria.VeiculoConsignacao;

public class HistoricoConsignacaoDTO
{
    public Guid Id { get; set; }
    public string TipoEvento { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public DateTime Data { get; set; }
}
