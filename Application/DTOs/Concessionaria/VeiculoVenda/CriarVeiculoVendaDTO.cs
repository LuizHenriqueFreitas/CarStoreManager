namespace CarStoreManager.Application.DTOs.Concessionaria.VeiculoVenda;

public class CriarVeiculoVendaDTO
{
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public string Cor { get; set; } = null!;
    public string Motorizacao { get; set; } = null!;
    public int Ano { get; set; }
    public int Quilometragem { get; set; }
    public string Placa { get; set; } = null!;
    public string Cambio { get; set; } = null!;
    public string Combustivel { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public decimal Valor { get; set; }
    public List<string> Acessorios { get; set; } = new();
}