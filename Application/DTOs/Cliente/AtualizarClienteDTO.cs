namespace CarStoreManager.Application.DTOs.Shared.Cliente;

public class AtualizarClienteDTO
{
    public Guid Id { get; set; }

    public string Telefone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}