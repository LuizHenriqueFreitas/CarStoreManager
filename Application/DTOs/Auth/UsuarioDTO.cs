namespace CarStoreManager.Application.DTOs.Auth;

public class UsuarioDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Role { get; set; } = null!;
}