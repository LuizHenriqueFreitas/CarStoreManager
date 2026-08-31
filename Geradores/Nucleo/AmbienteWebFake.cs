using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace CarStoreManager.Geradores.Nucleo;

/// <summary>
/// Stub mínimo de IWebHostEnvironment — algumas peças do grafo de DI da
/// aplicação (ex.: ArquivoStorageService, usado por IFotoService) dependem
/// dele mesmo fora de um host ASP.NET Core real. O Geradores não sobe um
/// servidor web, então registramos isso apontando pro wwwroot real do Web
/// para o caso de algum fluxo futuro vir a gravar arquivo.
/// </summary>
public sealed class AmbienteWebFake : IWebHostEnvironment
{
    public string EnvironmentName { get; set; } = "Production";
    public string ApplicationName { get; set; } = "CarStoreManager.Geradores";
    public string WebRootPath { get; set; }
    public IFileProvider WebRootFileProvider { get; set; }
    public string ContentRootPath { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; }

    public AmbienteWebFake()
    {
        ContentRootPath = Path.Combine(CaminhosProjeto.RaizSolucao(), "Web");
        WebRootPath = Path.Combine(ContentRootPath, "wwwroot");
        Directory.CreateDirectory(WebRootPath);

        ContentRootFileProvider = new PhysicalFileProvider(ContentRootPath);
        WebRootFileProvider = new PhysicalFileProvider(WebRootPath);
    }
}
