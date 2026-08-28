namespace CarStoreManager.Application.DTOs.Concessionaria.VeiculoConsignacao;

public class VeiculoConsignacaoListaDTO
{
    public Guid Id { get; set; }
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public int Ano { get; set; }
    public int Quilometragem { get; set; }
    public string Motorizacao { get; set; } = "";
    public string Combustivel { get; set; } = "";
    public string Placa { get; set; } = "";
    public string Status { get; set; } = null!;
    public int DiasRestantes { get; set; }
    public bool EstaVencido { get; set; }
    public decimal ValorVendaEsperado { get; set; }
    public string ClienteProprietarioNome { get; set; } = "";
    public string VendedorResponsavelNome { get; set; } = "";
    public string? FotoPrincipal { get; set; }
    public DateTime DataCriacao { get; set; }
}
