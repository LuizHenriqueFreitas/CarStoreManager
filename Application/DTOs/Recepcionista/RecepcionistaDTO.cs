namespace CarStoreManager.Application.DTOs.Recepcionista;

public class RecepcionistaDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Nivel { get; set; } = null!;
    public DateTime DataContratacao { get; set; }
    public int AnosEmpresa { get; set; }
}

public class RecepcionistaListaDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Nivel { get; set; } = null!;
}

public class CriarRecepcionistaDTO
{
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Senha { get; set; } = null!;
    public string Nivel { get; set; } = null!;
    public DateTime DataContratacao { get; set; }
}

public class AtualizarRecepcionistaDTO
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Nivel { get; set; } = null!;
}
