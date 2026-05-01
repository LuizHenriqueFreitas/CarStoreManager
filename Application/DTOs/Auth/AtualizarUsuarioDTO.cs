namespace CarStoreManager.Application.DTOs.Auth;

public class AtualizarUsuarioDTO
{
    public string Nome { get; set; } = null!;   // readonly na tela de configurações, mas o DTO aceita
    public string Email { get; set; } = null!;
    public string Telefone { get; set; } = null!;
}