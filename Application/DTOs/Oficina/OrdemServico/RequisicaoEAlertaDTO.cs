namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class RequisicaoPecaDTO
{
    public Guid Id { get; set; }
    public Guid OrdemServicoId { get; set; }
    public Guid MecanicoId { get; set; }
    public string DescricaoPeca { get; set; } = "";
    public string Justificativa { get; set; } = "";
    public int Quantidade { get; set; }
    public string Status { get; set; } = "Pendente";
    public DateTime DataCriacao { get; set; }
    public DateTime? DataResolucao { get; set; }
    public Guid? ResolvidaPor { get; set; }
    public string? ObservacaoAdmin { get; set; }
    public Guid? ComponenteAtendidoId { get; set; }

    // Snapshot do veículo para o admin saber em que carro a peça vai
    public string? VeiculoMarcaModelo { get; set; }
    public string? VeiculoPlaca { get; set; }
    public int? VeiculoAno { get; set; }
}

public class CriarRequisicaoPecaDTO
{
    public string DescricaoPeca { get; set; } = "";
    public string Justificativa { get; set; } = "";
    public int Quantidade { get; set; } = 1;
}

public class AtenderRequisicaoDTO
{
    public Guid ComponenteId { get; set; }
    public string? Observacao { get; set; }
}

public class RejeitarRequisicaoDTO
{
    public string Motivo { get; set; } = "";
}

public class AlertaOSDTO
{
    public Guid Id { get; set; }
    public Guid OrdemServicoId { get; set; }
    public Guid MecanicoId { get; set; }
    public string Descricao { get; set; } = "";
    public string Status { get; set; } = "Pendente";
    public DateTime DataCriacao { get; set; }
    public DateTime? DataResolucao { get; set; }
    public Guid? ResolvidoPor { get; set; }
    public string? ObservacaoCliente { get; set; }
}

public class CriarAlertaOSDTO
{
    public string Descricao { get; set; } = "";
}

public class ResolverAlertaDTO
{
    public bool Aprovou { get; set; }
    public string? ObservacaoCliente { get; set; }
}
