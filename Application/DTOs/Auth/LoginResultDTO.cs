namespace CarStoreManager.Application.DTOs.Auth;

public class LoginResultDTO
{
    public string Token { get; set; } = null!;
    public string Nome { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime Expiracao { get; set; }
}