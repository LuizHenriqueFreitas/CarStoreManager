namespace CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;

public class PropostaVendaListaDTO
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid VeiculoVendaId { get; set; }
    public decimal ValorFinal { get; set; }
    public string Status { get; set; } = null!;
    public DateTime DataCriacao { get; set; }

    /// <summary>Modo de pagamento escolhido na criação.</summary>
    public string ModoPagamento { get; set; } = "NaoDefinido";

    /// <summary>Snapshots leves para a listagem (evita N requisições do front).</summary>
    public string ClienteNome { get; set; } = "";
    public string VeiculoMarca { get; set; } = "";
    public string VeiculoModelo { get; set; } = "";

    /// <summary>Quantos dias até expirar (negativo = passou do prazo).</summary>
    public int DiasRestantes { get; set; }
}
