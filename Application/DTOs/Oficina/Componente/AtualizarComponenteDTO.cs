namespace CarStoreManager.Application.DTOs.Oficina.Componente;

public class AtualizarComponenteDTO
{
    public Guid Id { get; set; }

    public int QuantidadeEstoque { get; set; }

    public int EstoqueMinimo { get; set; }

    public decimal Valor { get; set; }
}