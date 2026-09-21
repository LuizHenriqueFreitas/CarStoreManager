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
}
