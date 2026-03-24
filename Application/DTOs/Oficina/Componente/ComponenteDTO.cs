namespace CarStoreManager.Application.DTOs.Oficina.Componente;

public class ComponenteDTO
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public string Sistema { get; set; } = string.Empty;

    public int QuantidadeEstoque { get; set; }

    public int EstoqueMinimo { get; set; }

    public decimal Valor { get; set; }

    public bool EstoqueBaixo { get; set; }
}