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
    public string Renavam { get; set; } = null!;
    public string Cambio { get; set; } = null!;
    public string Combustivel { get; set; } = null!;
    public decimal Valor { get; set; }

    /// <summary>
    /// Custo de aquisição do veículo — quanto a concessionária pagou para
    /// comprá-lo. Lançado como "Compra de material" nos gastos da
    /// concessionária. Opcional: quando não informado, vale 0.
    /// </summary>
    public decimal CustoAquisicao { get; set; }

    public List<string> Acessorios { get; set; } = new();
    public int? AnoUltimoIpvaPago { get; set; }

    /// <summary>
    /// Texto preliminar do termo de entrega — preenchido pelo admin no
    /// cadastro. Vira a base do termo quando a proposta for gerada.
    /// </summary>
    public string TextoTermoPreliminar { get; set; } = "";
}
