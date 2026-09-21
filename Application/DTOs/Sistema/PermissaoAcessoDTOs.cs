using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Application.DTOs.Sistema;

public enum TipoRecursoProtegivel
{
    Pagina,
    Acao
}

/// <summary>
/// Entrada do catálogo de recursos protegíveis — é CÓDIGO (ver
/// CatalogoRecursosProtegiveis), não fica no banco. <see cref="RolesPermitidos"/>
/// é o teto (quem o `[Authorize(Roles=)]`/`AuthorizeView` de hoje já libera)
/// e também o valor padrão — o admin só pode tirar acesso de alguém dentro
/// desse conjunto, nunca dar acesso a um papel fora dele.
/// </summary>
public class RecursoProtegivel
{
    public string Chave { get; init; } = "";
    public TipoRecursoProtegivel Tipo { get; init; }
    public string Area { get; init; } = "";
    public string Rotulo { get; init; } = "";
    public string? RotaPagina { get; init; }
    public HashSet<RoleUsuario> RolesPermitidos { get; init; } = new();
}

/// <summary>Uma linha da matriz de permissões, pronta pra tela de admin.</summary>
public class RecursoPermissaoDTO
{
    public string Chave { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Area { get; set; } = "";
    public string Rotulo { get; set; } = "";

    /// <summary>Papel → (pode configurar esse papel pra esse recurso, valor atual).</summary>
    public Dictionary<string, bool> PermitidoPorPapel { get; set; } = new();
}
