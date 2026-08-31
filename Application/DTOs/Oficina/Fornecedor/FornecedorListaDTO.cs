namespace CarStoreManager.Application.DTOs.Oficina.Fornecedor;

public class FornecedorListaDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public bool Ativo { get; set; }
}
