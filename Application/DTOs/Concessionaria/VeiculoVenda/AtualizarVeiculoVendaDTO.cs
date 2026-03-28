namespace CarStoreManager.Application.DTOs.Concessionaria.VeiculoVenda;

public class AtualizarVeiculoVendaDTO
{
    public Guid Id { get; set; }
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public string Cor { get; set; } = null!;
    public string Motorizacao { get; set; } = null!;
    public int Quilometragem { get; set; }
    public string Estado { get; set; } = null!;
    public string Disponibilidade { get; set; } = null!;
    public string Cambio { get; set; } = null!;
    public string Combustivel { get; set; } = null!;
    public decimal Valor { get; set; }
    public List<string> Acessorios { get; set; } = new();
}