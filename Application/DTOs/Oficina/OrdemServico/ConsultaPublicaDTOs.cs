namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

/// <summary>Resultado da consulta pública por CPF + placa. Ver docs/redesign/08-consulta-cliente-cpf.md.</summary>
public class ConsultaPublicaResultadoDTO
{
    public string PrimeiroNome { get; set; } = "";
    public List<VeiculoHistoricoPublicoDTO> Veiculos { get; set; } = new();

    /// <summary>OS em andamento (não entregue/cancelada), destacada no topo.</summary>
    public OrdemServicoPublicaDTO? OrdemEmAberto { get; set; }
}

public class VeiculoHistoricoPublicoDTO
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = "";
    public string Placa { get; set; } = "";
    public List<OrdemServicoPublicaDTO> Historico { get; set; } = new();
}
