namespace CarStoreManager.Application.DTOs.Auth;

public class CriarUsuarioDTO
{
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Senha { get; set; } = null!;
    public string Role { get; set; } = null!; // "Vendedor" ou "Mecanico"

    // campos extras para Mecanico
    public string? Especialidade { get; set; }

    // campos para Vendedor e Mecanico
    public string? Nivel { get; set; }
    public DateTime? DataContratacao { get; set; }
}