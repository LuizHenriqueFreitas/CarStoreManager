using CarStoreManager.Application.Common;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace CarStoreManager.Infrastructure.Services;

/*
    Implementação concreta de IArquivoStorage que persiste arquivos em
    wwwroot/{pastaBase}/{pastaLogica}/{guid}.{ext}.
    A URL pública devolvida segue o mesmo path relativo, servida pelo
    StaticFiles middleware da AspNet (UseStaticFiles em Program.cs).
*/
public class ArquivoStorageService : IArquivoStorage
{
    private const string PastaBase = "uploads";

    private readonly IWebHostEnvironment _env;

    public ArquivoStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SalvarAsync(ArquivoUpload arquivo, string pastaLogica, CancellationToken ct = default)
    {
        var pastaDestino = Path.Combine(_env.WebRootPath, PastaBase, pastaLogica);
        if (!Directory.Exists(pastaDestino))
            Directory.CreateDirectory(pastaDestino);

        var extensao = Path.GetExtension(arquivo.NomeArquivo);
        var nomeUnico = $"{Guid.NewGuid()}{extensao}";
        var caminhoCompleto = Path.Combine(pastaDestino, nomeUnico);

        await using var destino = new FileStream(caminhoCompleto, FileMode.Create);
        await using var origem = arquivo.AbrirStream();
        await origem.CopyToAsync(destino, ct);

        return $"/{PastaBase}/{pastaLogica}/{nomeUnico}";
    }

    public Task RemoverAsync(string urlPublica, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(urlPublica))
            return Task.CompletedTask;

        var relativo = urlPublica.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var caminho = Path.Combine(_env.WebRootPath, relativo);

        if (File.Exists(caminho))
            File.Delete(caminho);

        return Task.CompletedTask;
    }
}
