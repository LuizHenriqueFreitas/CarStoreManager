using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Oficina;

public class Mecanico : Usuario
{
    public EspecialidadeMecanico Especialidade { get; private set; }
    public DadosFuncionario DadosFuncionario { get; private set; } = null!;

    protected Mecanico() { }

    public Mecanico(
        string nome,
        Email email,
        Telefone telefone,
        string senhaHash,
        EspecialidadeMecanico especialidade,
        NivelFuncionario nivel,
        DateTime dataContratacao)
        : base(nome, email, telefone, senhaHash, RoleUsuario.Mecanico)
    {
        Especialidade = especialidade;
        DadosFuncionario = new DadosFuncionario(nivel, dataContratacao);
    }

    public void AlterarEspecialidade(EspecialidadeMecanico especialidade)
        => Especialidade = especialidade;

    public void AtualizarDadosFuncionario(NivelFuncionario nivel, DateTime dataContratacao)
        => DadosFuncionario = new DadosFuncionario(nivel, dataContratacao);
}