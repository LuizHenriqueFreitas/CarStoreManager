namespace CarStoreManager.Application.DTOs.Sistema.Importacao;

public class ImportacaoResultadoDTO
{
    public int UsuariosCriados { get; set; }
    public int ClientesCriados { get; set; }
    public int FornecedoresCriados { get; set; }
    public int ComponentesCriados { get; set; }
    public int ChecklistPresetsCriados { get; set; }
    public int DespesasCriadas { get; set; }
    public int VeiculosVendaCriados { get; set; }
    public int VeiculosConsignadosCriados { get; set; }
    public int VeiculosClienteCriados { get; set; }
    public int PropostasVendaCriadas { get; set; }
    public int OrdensServicoCriadas { get; set; }

    public List<string> Avisos { get; set; } = new();

    public int TotalCriado =>
        UsuariosCriados + ClientesCriados + FornecedoresCriados + ComponentesCriados + ChecklistPresetsCriados +
        DespesasCriadas + VeiculosVendaCriados + VeiculosConsignadosCriados + VeiculosClienteCriados +
        PropostasVendaCriadas + OrdensServicoCriadas;
}
