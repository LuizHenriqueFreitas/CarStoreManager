namespace CarStoreManager.Application.DTOs.Concessionaria.VeiculoConsignacao;

public class AtualizarVeiculoConsignacaoDTO
{
    public Guid Id { get; set; }
    public string? TextoContrato { get; set; }
    public string? UrlContratoPdf { get; set; }
    public Guid? VendedorResponsavelId { get; set; }
}
