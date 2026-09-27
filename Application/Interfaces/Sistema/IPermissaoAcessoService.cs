using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Application.Interfaces.Sistema;

public interface IPermissaoAcessoService
{
    /// <summary>
    /// True se o papel pode acessar o recurso — consulta o override salvo
    /// pelo admin (tabela PermissaoAcesso); se não houver override, cai no
    /// padrão do catálogo de código (RolesPermitidos daquele recurso).
    /// </summary>
    Task<bool> PodeAcessarAsync(RoleUsuario role, string recursoChave);

    /// <summary>Matriz completa (todo recurso do catálogo × todo papel elegível pra ele) — pra tela de admin.</summary>
    Task<List<RecursoPermissaoDTO>> ObterMatrizAsync();

    /// <summary>Grava/atualiza o override de um papel pra um recurso.</summary>
    Task<Result> AtualizarAsync(RoleUsuario role, string recursoChave, bool permitido);

    /// <summary>
    /// True se ESSE usuário específico pode acessar o recurso — consulta
    /// primeiro a exceção pessoal (PermissaoIndividual); se existir, vale
    /// ela (pode ampliar além do papel). Sem exceção, cai no
    /// PodeAcessarAsync(role, chave) de sempre.
    /// </summary>
    Task<bool> PodeAcessarAsync(Guid usuarioId, RoleUsuario role, string recursoChave);

    /// <summary>Matriz individual (TODO o catálogo) pra um usuário — tela "Acessos" em /equipe.</summary>
    Task<List<RecursoPermissaoIndividualDTO>> ObterMatrizIndividualAsync(Guid usuarioId, RoleUsuario roleDoUsuario);

    /// <summary>Grava/remove a exceção pessoal de um usuário pra um recurso. null = remove (volta ao padrão do papel).</summary>
    Task<Result> AtualizarIndividualAsync(Guid usuarioId, string recursoChave, bool? permitido);
}
