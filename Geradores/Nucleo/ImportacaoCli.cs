using System.Diagnostics;
using System.Text.Json;
using CarStoreManager.Application.DTOs.Sistema.Importacao;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace CarStoreManager.Geradores.Nucleo;

/// <summary>
/// Modo CLI de importação/exportação do arquivo de dados (formato
/// chave/cenario — o mesmo de Configurações → Importar dados):
///
///   dotnet run --project Geradores -- --importar &lt;arquivo.json&gt; [--banco &lt;arquivo.db&gt;]
///   dotnet run --project Geradores -- --exportar &lt;arquivo.json&gt; [--banco &lt;arquivo.db&gt;]
///   dotnet run --project Geradores -- --backup &lt;arquivo.db&gt;     [--banco &lt;arquivo.db&gt;]
///   dotnet run --project Geradores -- --restaurar &lt;arquivo.db&gt;  [--banco &lt;arquivo.db&gt;]
///
/// --backup/--restaurar usam o IBackupBancoService (cópia fiel do SQLite;
/// restaurar guarda antes uma cópia de segurança do banco atual).
///
/// Antes de importar, aplica migrations + seed do 1º run EXATAMENTE como o
/// Web (Infrastructure/Data/SeedInicial.cs — admin@teste.com, presets,
/// templates padrão), então funciona num banco recém-apagado sem subir o site.
/// </summary>
public static class ImportacaoCli
{
    // Mesmas opções da UI (Configuracoes.razor → ImportarArquivo).
    private static readonly JsonSerializerOptions OpcoesJson = new() { PropertyNameCaseInsensitive = true };

    public static bool EhComando(string[] args)
        => args.Any(a => a is "--importar" or "--exportar" or "--backup" or "--restaurar");

    public static async Task<int> ExecutarAsync(string[] args)
    {
        string? Valor(string chave)
        {
            var i = Array.IndexOf(args, chave);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
        }

        var banco = Valor("--banco");
        var importar = Valor("--importar");
        var exportar = Valor("--exportar");
        var backup = Valor("--backup");
        var restaurar = Valor("--restaurar");

        foreach (var (opcao, valor) in new[] { ("--importar", importar), ("--exportar", exportar), ("--backup", backup), ("--restaurar", restaurar) })
        {
            if (args.Contains(opcao) && string.IsNullOrWhiteSpace(valor))
            {
                Console.Error.WriteLine($"Informe o caminho do arquivo: {opcao} <arquivo>");
                return 2;
            }
        }

        var provider = ComposicaoServicos.Construir(banco);

        var codigo = 0;
        if (restaurar is not null) codigo = await RestaurarAsync(provider, restaurar);
        if (codigo == 0 && importar is not null) codigo = await ImportarAsync(provider, importar);
        if (codigo == 0 && exportar is not null) codigo = await ExportarAsync(provider, exportar);
        if (codigo == 0 && backup is not null) codigo = await BackupAsync(provider, backup);
        return codigo;
    }

    private static async Task<int> BackupAsync(IServiceProvider provider, string caminho)
    {
        caminho = Path.GetFullPath(caminho);
        using var scope = provider.CreateScope();
        var servico = scope.ServiceProvider.GetRequiredService<IBackupBancoService>();
        var r = await servico.GerarBackupAsync();
        if (!r.IsSuccess || r.Value is null)
        {
            Console.Error.WriteLine($"FALHA ao gerar backup: {r.Error}");
            return 1;
        }
        Directory.CreateDirectory(Path.GetDirectoryName(caminho)!);
        await File.WriteAllBytesAsync(caminho, r.Value.Conteudo);
        Console.WriteLine($"Backup gravado em {caminho} ({r.Value.Conteudo.Length / 1024} KB).");
        return 0;
    }

    private static async Task<int> RestaurarAsync(IServiceProvider provider, string caminho)
    {
        caminho = Path.GetFullPath(caminho);
        if (!File.Exists(caminho))
        {
            Console.Error.WriteLine($"Arquivo não encontrado: {caminho}");
            return 2;
        }
        using var scope = provider.CreateScope();
        var servico = scope.ServiceProvider.GetRequiredService<IBackupBancoService>();
        await using var stream = File.OpenRead(caminho);
        var r = await servico.RestaurarAsync(stream);
        if (!r.IsSuccess || r.Value is null)
        {
            Console.Error.WriteLine($"FALHA ao restaurar: {r.Error}");
            return 1;
        }
        Console.WriteLine($"Banco restaurado de {caminho}. Cópia de segurança do anterior: {r.Value.CaminhoCopiaSeguranca}. Migrations aplicadas: {r.Value.MigrationsAplicadas}.");
        return 0;
    }

    private static async Task<int> ImportarAsync(IServiceProvider provider, string caminho)
    {
        caminho = Path.GetFullPath(caminho);
        if (!File.Exists(caminho))
        {
            Console.Error.WriteLine($"Arquivo não encontrado: {caminho}");
            return 2;
        }

        var cronometro = Stopwatch.StartNew();

        // Banco: migrations + seed do 1º run (mesmo código do Web/Program.cs).
        using (var scopeSeed = provider.CreateScope())
        {
            var context = scopeSeed.ServiceProvider.GetRequiredService<AppDbContext>();
            await SeedInicial.AplicarAsync(context);
        }
        Console.WriteLine("Banco migrado e semeado (admin, presets, templates padrão).");

        ImportacaoDadosDTO? dados;
        await using (var stream = File.OpenRead(caminho))
            dados = await JsonSerializer.DeserializeAsync<ImportacaoDadosDTO>(stream, OpcoesJson);
        if (dados is null)
        {
            Console.Error.WriteLine("O arquivo não contém um documento de importação válido.");
            return 2;
        }

        Console.WriteLine($"Importando {Path.GetFileName(caminho)}... (pode levar alguns minutos)");
        using var scope = provider.CreateScope();
        var servico = scope.ServiceProvider.GetRequiredService<IImportacaoDadosService>();
        var r = await servico.ImportarAsync(dados);
        cronometro.Stop();

        var caminhoAvisos = Path.Combine(Path.GetDirectoryName(caminho)!, "importacao_avisos.txt");
        if (!r.IsSuccess || r.Value is null)
        {
            Console.Error.WriteLine($"FALHA: {r.Error}");
            await File.WriteAllTextAsync(caminhoAvisos, $"FALHA: {r.Error}{Environment.NewLine}");
            return 1;
        }

        var res = r.Value;
        Console.WriteLine();
        Console.WriteLine("== Registros criados ==");
        foreach (var prop in typeof(ImportacaoResultadoDTO).GetProperties().Where(p => p.PropertyType == typeof(int) && p.Name != nameof(ImportacaoResultadoDTO.TotalCriado)))
            Console.WriteLine($"  {prop.Name,-32} {prop.GetValue(res),6}");
        Console.WriteLine($"  {"TOTAL",-32} {res.TotalCriado,6}");
        Console.WriteLine();

        Console.WriteLine($"== Avisos: {res.Avisos.Count} ==");
        foreach (var aviso in res.Avisos)
            Console.WriteLine($"  - {aviso}");

        await File.WriteAllLinesAsync(caminhoAvisos,
            new[] { $"Importação de {Path.GetFileName(caminho)} em {DateTime.Now:dd/MM/yyyy HH:mm:ss} — {res.TotalCriado} registro(s), {res.Avisos.Count} aviso(s)." }
                .Concat(res.Avisos.Select(a => $"- {a}")));
        Console.WriteLine();
        Console.WriteLine($"Avisos gravados em: {caminhoAvisos}");
        Console.WriteLine($"Tempo total: {cronometro.Elapsed:hh\\:mm\\:ss}");
        return 0;
    }

    private static async Task<int> ExportarAsync(IServiceProvider provider, string caminho)
    {
        caminho = Path.GetFullPath(caminho);
        var cronometro = Stopwatch.StartNew();
        using var scope = provider.CreateScope();
        var servico = scope.ServiceProvider.GetRequiredService<IExportacaoDadosService>();
        var r = await servico.ExportarJsonAsync();
        if (!r.IsSuccess || r.Value is null)
        {
            Console.Error.WriteLine($"FALHA ao exportar: {r.Error}");
            return 1;
        }
        await File.WriteAllBytesAsync(caminho, r.Value.Conteudo);
        Console.WriteLine($"Exportado para {caminho} ({r.Value.Conteudo.Length / 1024} KB) em {cronometro.Elapsed:hh\\:mm\\:ss}.");
        return 0;
    }
}
