namespace CarStoreManager.Application.DTOs.Concessionaria.VeiculoVenda;

public class AtualizarVeiculoVendaDTO
{
    public Guid Id { get; set; }
    public string Disponibilidade { get; set; } = null!;
    public decimal Valor { get; set; }

    /// <summary>
    /// Custo de aquisição do veículo. Pode ser corrigido após o cadastro.
    /// </summary>
    public decimal CustoAquisicao { get; set; }

    public string? TextoTermoPreliminar { get; set; }
}
