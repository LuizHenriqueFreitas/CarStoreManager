namespace CarStoreManager.Application.DTOs.Concessionaria.TestDrive;

public class TestDriveListaDTO
{
    public Guid Id { get; set; }
    public Guid VeiculoVendaId { get; set; }
    /// <summary>"VeiculoVenda" ou "VeiculoConsignacao".</summary>
    public string VeiculoEntidadeTipo { get; set; } = "VeiculoVenda";
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
    /// <summary>"VeiculoVenda" (padrão) ou "VeiculoConsignacao".</summary>
    public string VeiculoEntidadeTipo { get; set; } = "VeiculoVenda";
    public Guid ClienteId { get; set; }
    public Guid VendedorId { get; set; }
    public DateTime DataHora { get; set; }
    public string? Observacao { get; set; }

    /// <summary>Texto do termo de responsabilidade do test drive, redigido
    /// no mesmo formulário do agendamento — cria o termo (Rascunho) junto
    /// com o test drive. Obrigatório.</summary>
    public string TextoTermo { get; set; } = "";
}

public class AtualizarStatusTestDriveDTO
{
    public Guid Id { get; set; }
    public string Status { get; set; } = "";
}

public class TermoTestDriveDTO
{
    public Guid Id { get; set; }
    public Guid TestDriveId { get; set; }
    public string TextoTermo { get; set; } = "";
    public string Status { get; set; } = "Rascunho";
    public DateTime DataRedacao { get; set; }
    public DateTime? DataUltimaEdicao { get; set; }
    public string? TokenAssinatura { get; set; }
    public DateTime? DataAssinatura { get; set; }
    public string? AssinaturaNomeCliente { get; set; }
    public string? AssinaturaCpfCliente { get; set; }
    public string? AssinaturaIp { get; set; }
}

public class EditarTermoTestDriveDTO
{
    public string TextoTermo { get; set; } = "";
}

public class AssinarTermoTestDriveDTO
{
    public string NomeCliente { get; set; } = "";
    public string CpfCliente { get; set; } = "";
    public bool Aceite { get; set; }
}
