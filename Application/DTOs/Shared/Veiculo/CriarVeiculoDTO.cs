namespace CarStoreManager.Application.DTOs.Shared.Veiculo;

public class CriarVeiculoDTO
{
    public string Marca { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public int Ano { get; set; }

    public string Cor { get; set; } = string.Empty;

    public int Quilometragem { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string Placa { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public string Disponibilidade { get; set; } = string.Empty;
}