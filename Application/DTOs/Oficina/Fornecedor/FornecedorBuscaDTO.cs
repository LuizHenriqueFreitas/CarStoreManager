namespace CarStoreManager.Application.DTOs.Oficina.Fornecedor;

/// <summary>Resultado leve de busca — alimenta o campo de pesquisa de fornecedor no cadastro de componente.</summary>
public class FornecedorBuscaDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
}
