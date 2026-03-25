namespace CarStoreManager.Application.DTOs.Auth;

public class LoginDTO
{
    public string Email { get; set; } = null!;
    public string Senha { get; set; } = null!;
}