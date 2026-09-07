namespace CarStoreManager.Application.DTOs.Concessionaria.TestDrive;

public class TestDriveListaDTO
{
    public Guid Id { get; set; }
    public Guid VeiculoVendaId { get; set; }
    public Guid ClienteId { get; set; }
    public Guid VendedorId { get; set; }
    public DateTime DataHora { get; set; }
    public string Status { get; set; } = "Agendado";
    public string? Observacao { get; set; }

    public string VeiculoDescricao { get; set; } = "";
    public string ClienteNome { get; set; } = "";
    public string VendedorNome { get; set; } = "";
}

public class CriarTestDriveDTO
{
    public Guid VeiculoVendaId { get; set; }
    public Guid ClienteId { get; set; }
    public Guid VendedorId { get; set; }
    public DateTime DataHora { get; set; }
    public string? Observacao { get; set; }
}

public class AtualizarStatusTestDriveDTO
{
    public Guid Id { get; set; }
    public string Status { get; set; } = "";
}
