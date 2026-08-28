using CarStoreManager.Application.DTOs;

namespace CarStoreManager.Application.DTOs.Concessionaria.VeiculoConsignacao;

public class VeiculoConsignacaoDTO
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
    public List<string> Acessorios { get; set; } = new();

    public Guid ClienteProprietarioId { get; set; }
    public string ClienteProprietarioNome { get; set; } = "";
    public Guid VendedorResponsavelId { get; set; }
    public string VendedorResponsavelNome { get; set; } = "";

    public string TipoComissao { get; set; } = null!;
    public decimal ValorVendaEsperado { get; set; }
    public decimal? ValorFixoProprietario { get; set; }

    /// <summary>Escala 0-100.</summary>
    public decimal? PorcentagemProprietario { get; set; }

    public string TextoContrato { get; set; } = "";
    public string? UrlContratoPdf { get; set; }

    public DateTime DataInicio { get; set; }
    public DateTime DataVencimento { get; set; }
    public int DiasRestantes { get; set; }
    public bool EstaVencido { get; set; }
    public string Status { get; set; } = null!;

    public List<HistoricoConsignacaoDTO> Historico { get; set; } = new();

    /// <summary>Fotos vêm do IFotoService (EntidadeTipo="VeiculoConsignacao"), não de navegação da entidade.</summary>
    public List<FotoDto> Fotos { get; set; } = new();
}
