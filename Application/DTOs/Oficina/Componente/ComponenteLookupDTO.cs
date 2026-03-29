//provavelmente isso nao tera uso

namespace CarStoreManager.Application.DTOs.Oficina.Componente;

public class ComponenteLookupDTO
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;
    
    public decimal Valor { get; set; }

    public string Sistema { get; set; } = string.Empty;

}