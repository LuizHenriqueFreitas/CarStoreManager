using CarStoreManager.Domain.Base;

namespace CarStoreManager.Domain.Entities.Sistema;

/// <summary>
/// Exceção de acesso pra UM usuário específico, acima ou abaixo do que o
/// papel dele permitiria (ver PermissaoAcesso). Ao contrário de
/// PermissaoAcesso — que só pode RESTRINGIR dentro do teto do catálogo —
/// esta entidade pode AMPLIAR o acesso além do papel: é o mecanismo de
/// "alçada pessoal" (ex.: um Vendedor específico ganhar acesso a uma tela
/// que só ChefeOficina/Admin veem, sem virar ChefeOficina). Ausência de
/// registro = comportamento normal do papel, sem nenhuma mudança.
/// </summary>
public class PermissaoIndividual : Entity
{
    public Guid UsuarioId { get; private set; }
    public string RecursoChave { get; private set; } = null!;
    public bool Permitido { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    protected PermissaoIndividual() { }

    public PermissaoIndividual(Guid usuarioId, string recursoChave, bool permitido)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("Usuário é obrigatório.", nameof(usuarioId));
        if (string.IsNullOrWhiteSpace(recursoChave))
            throw new ArgumentException("Chave do recurso é obrigatória.", nameof(recursoChave));

        UsuarioId = usuarioId;
        RecursoChave = recursoChave.Trim();
        Permitido = permitido;
    }

    public void Atualizar(bool permitido)
    {
        Permitido = permitido;
        DataAtualizacao = DateTime.UtcNow;
    }
}
