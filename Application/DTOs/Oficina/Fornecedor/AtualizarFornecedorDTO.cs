using CarStoreManager.Application.DTOs.Shared.Cliente;

namespace CarStoreManager.Application.DTOs.Oficina.Fornecedor;

/// <summary>Atualiza só os dados de contato do fornecedor — nome e CNPJ não mudam depois de cadastrados.</summary>
public class AtualizarFornecedorDTO
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public EnderecoDTO? Endereco { get; set; }
}
