namespace CarStoreManager.Application.DTOs.Shared.Cliente;

/// <summary>Ficha 360° do cliente. Ver docs/redesign/07-cadastro-clientes.md §4.2.</summary>
public class ClienteFichaDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public string Cpf { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Endereco { get; set; } = "";
    public DateTime ClienteDesde { get; set; }

    public int TotalOrdensServico { get; set; }
    public int OrdensServicoAbertas { get; set; }
    public int TotalPropostas { get; set; }
    public int ComprasConcluidas { get; set; }
    public decimal TotalGastoOficina { get; set; }
    public decimal TotalGastoConcessionaria { get; set; }
    public DateTime? UltimoAtendimento { get; set; }
    public string UltimoAtendimentoSetor { get; set; } = "";

    public List<FichaVeiculoDTO> Veiculos { get; set; } = new();
    public List<FichaEventoDTO> HistoricoOficina { get; set; } = new();
    public List<FichaEventoDTO> HistoricoConcessionaria { get; set; } = new();
}

public class FichaVeiculoDTO
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = "";
    public string Placa { get; set; } = "";
    public int Atendimentos { get; set; }
}

public class FichaEventoDTO
{
    public Guid Id { get; set; }
    public DateTime Data { get; set; }
    public string Referencia { get; set; } = "";   // nº OS / proposta
    public string Descricao { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal Valor { get; set; }
}
