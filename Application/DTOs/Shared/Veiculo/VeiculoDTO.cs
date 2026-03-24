namespace CarStoreManager.Application.DTOs.Shared.Veiculo;

public class VeiculoDTO
{
    public Guid Id { get; set; }

    public string Marca { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public string Cor { get; set; } = string.Empty;

    public int Ano { get; set; }

    public int Quilometragem { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string Placa { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public string Disponibilidade { get; set; } = string.Empty;

    public string NomeCompleto => $"{Marca} {Modelo}";
}