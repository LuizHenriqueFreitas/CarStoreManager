using CarStoreManager.Application.DTOs;

namespace CarStoreManager.Application.DTOs.Concessionaria.VeiculoVenda;

public class VeiculoVendaDTO
{
    public Guid Id { get; set; }
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
    public string Disponibilidade { get; set; } = null!;
    public decimal Valor { get; set; }

    /// <summary>
    /// Custo pago pela concessionária para adquirir o veículo. Compõe os
    /// gastos da concessionária ("Compra de material") no dashboard.
    /// </summary>
    public decimal CustoAquisicao { get; set; }

    public int? AnoUltimoIpvaPago { get; set; }
    public List<string> Acessorios { get; set; } = new();

    /// <summary>
    /// Fotos do veículo já ordenadas, com Id real — necessário para
    /// reordenar/remover fotos a partir da tela de detalhe.
    /// </summary>
    public List<FotoDto> Fotos { get; set; } = new();

    public string TextoTermoPreliminar { get; set; } = "";
}
