namespace CarStoreManager.Application.DTOs.Oficina.Componente;

public class CriarComponenteDTO
{
    public string Nome { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public int QuantidadeEstoque { get; set; }

    public int EstoqueMinimo { get; set; }

    public decimal Valor { get; set; }

    public string Sistema { get; set; } = string.Empty;
}