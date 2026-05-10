// Application/DTOs/FotoDto.cs
namespace CarStoreManager.Application.DTOs;

public class FotoDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public DateTime DataUpload { get; set; }
}