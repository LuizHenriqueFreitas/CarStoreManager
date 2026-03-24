namespace CarStoreManager.Application.DTOs.Shared.Cliente;

public class ClienteDTO
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Cpf { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}