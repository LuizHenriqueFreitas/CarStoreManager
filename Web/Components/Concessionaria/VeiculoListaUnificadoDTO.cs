namespace CarStoreManager.Web.Components.Concessionaria;

/// <summary>
/// View-model de apresentação que unifica VeiculoVendaListaDTO e
/// VeiculoConsignacaoListaDTO num único formato para a grade de estoque —
/// a concessionária mostra veículos próprios e consignados juntos, com um
/// filtro a mais ("Consignados"), em vez de telas separadas.
/// </summary>
public class VeiculoListaUnificadoDTO
{
    public Guid Id { get; set; }
    public bool IsConsignado { get; set; }
    public string Marca { get; set; } = "";
    public string Modelo { get; set; } = "";
    public int Ano { get; set; }
    public int Quilometragem { get; set; }
    public string Motorizacao { get; set; } = "";
    public string Combustivel { get; set; } = "";
    public string Placa { get; set; } = "";
    public List<string> Acessorios { get; set; } = new();
    public decimal Valor { get; set; }
    public string? FotoPrincipal { get; set; }
    public int? AnoUltimoIpvaPago { get; set; }
    public DateTime DataCriacao { get; set; }

    /// <summary>Disponibilidade (veículo próprio) ou Status de consignação — usado no badge do card.</summary>
    public string StatusLabel { get; set; } = "";

    // Só relevantes quando IsConsignado — usados pelo aviso de vencimento.
    public bool EstaVencido { get; set; }
    public int DiasRestantes { get; set; }
}
