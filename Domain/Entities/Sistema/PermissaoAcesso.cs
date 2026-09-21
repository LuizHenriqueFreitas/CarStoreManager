using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities.Sistema;

/// <summary>
/// Override de acesso configurado pelo administrador para um papel × recurso
/// protegível (página ou ação). Quando não existe registro pra uma
/// combinação Role×RecursoChave, o sistema cai no padrão definido no
/// catálogo de código (ver Application/Services/Sistema/
/// CatalogoRecursosProtegiveis.cs) — esta tabela só guarda as EXCEÇÕES que
/// o admin decidiu mudar, não a matriz inteira. Ver
/// docs/redesign/23-permissoes-dinamicas-por-papel.md.
/// </summary>
public class PermissaoAcesso : Entity
{
    public RoleUsuario Role { get; private set; }
    public string RecursoChave { get; private set; } = null!;
    public bool Permitido { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    protected PermissaoAcesso() { }

    public PermissaoAcesso(RoleUsuario role, string recursoChave, bool permitido)
    {
        if (string.IsNullOrWhiteSpace(recursoChave))
            throw new ArgumentException("Chave do recurso é obrigatória.", nameof(recursoChave));

        Role = role;
        RecursoChave = recursoChave.Trim();
        Permitido = permitido;
    }

    public void Atualizar(bool permitido)
    {
        Permitido = permitido;
        DataAtualizacao = DateTime.UtcNow;
    }
}
