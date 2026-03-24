namespace CarStoreManager.Application.DTOs.Oficina.Componente;

public class ComponenteListaDTO
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public int QuantidadeEstoque { get; set; }

    public decimal Valor { get; set; }

    public string Sistema { get; set; } = string.Empty;

    public bool EstoqueBaixo { get; set; }
}