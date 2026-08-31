namespace CarStoreManager.Geradores.Nucleo;

/// <summary>
/// Resolve caminhos absolutos do repositório (raiz da solução, appsettings*.json
/// do Web) a partir do diretório de execução do Geradores — funciona
/// independente de onde o `dotnet run` é disparado.
/// </summary>
public static class CaminhosProjeto
{
    public static string RaizSolucao()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null && !dir.GetFiles("CarStoreManager.sln").Any())
            dir = dir.Parent;

        if (dir is null)
            throw new InvalidOperationException(
                "Não foi possível localizar CarStoreManager.sln a partir do diretório de execução.");

        return dir.FullName;
    }

    public static string AppSettingsWeb()
        => Path.Combine(RaizSolucao(), "Web", "appsettings.json");

    public static string AppSettingsDevelopment()
        => Path.Combine(RaizSolucao(), "Web", "appsettings.Development.json");
}
