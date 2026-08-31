using CarStoreManager.Application.DTOs.Shared.Cliente;

namespace CarStoreManager.Application.DTOs.Oficina.Fornecedor;

public class CriarFornecedorDTO
{
    public string Nome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public EnderecoDTO? Endereco { get; set; }
}
