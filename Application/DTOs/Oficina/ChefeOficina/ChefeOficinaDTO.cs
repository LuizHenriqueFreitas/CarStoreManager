namespace CarStoreManager.Application.DTOs.Oficina.ChefeOficina;

public class ChefeOficinaDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Nivel { get; set; } = null!;
    public DateTime DataContratacao { get; set; }
    public int AnosEmpresa { get; set; }
}

public class ChefeOficinaListaDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Nivel { get; set; } = null!;
}

public class CriarChefeOficinaDTO
{
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Senha { get; set; } = null!;
    public string Nivel { get; set; } = null!;
    public DateTime DataContratacao { get; set; }
}

public class AtualizarChefeOficinaDTO
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Nivel { get; set; } = null!;
}
