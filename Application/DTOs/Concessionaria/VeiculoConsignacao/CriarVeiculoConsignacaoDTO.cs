namespace CarStoreManager.Application.DTOs.Concessionaria.VeiculoConsignacao;

public class CriarVeiculoConsignacaoDTO
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
    public List<string> Acessorios { get; set; } = new();

    public Guid ClienteProprietarioId { get; set; }
    public Guid VendedorResponsavelId { get; set; }

    /// <summary>"Fixo" ou "Porcentagem".</summary>
    public string TipoComissao { get; set; } = null!;
    public decimal ValorVendaEsperado { get; set; }
    public decimal? ValorFixoProprietario { get; set; }

    /// <summary>Escala 0-100 (ex.: 85 = 85%), não 0-1.</summary>
    public decimal? PorcentagemProprietario { get; set; }

    public string TextoContrato { get; set; } = "";
    public string? UrlContratoPdf { get; set; }
    public int PrazoDias { get; set; } = 90;
}
