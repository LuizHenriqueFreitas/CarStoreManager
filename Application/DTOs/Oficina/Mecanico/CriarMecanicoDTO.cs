namespace CarStoreManager.Application.DTOs.Oficina.Mecanico;

public class CriarMecanicoDTO
{
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Senha { get; set; } = null!;
    public string Especialidade { get; set; } = null!;
    public string Nivel { get; set; } = null!;
    public DateTime DataCriacao { get; set;}
}