//classe base do mecanico herda de Uduario.cs

using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Oficina;

public class Mecanico : Usuario
{
    public EspecialidadeMecanico Especialidade { get; private set; }
    public NivelOcupacaoMecanico Ocupado { get; private set; }
    public DadosFuncionario DadosFuncionario { get; private set; } = null!;
    public List<Guid> TrabalhosAtivos {get; private set; } = new ();

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

    // =====================
    // GETTERS
    // =====================

    public string GetEspecialidade() => Especialidade.ToString();
    public string GetOcupado() => Ocupado.ToString();
    public string GetNivel() => DadosFuncionario.Nivel.ToString();
    public DateTime GetDataContratacao() => DadosFuncionario.DataContratacao;

    // =============================
    // REGRAS DE NEGOCIOS - SETERS
    // =============================

    public void AlterarOcupado()
    {
        if (TrabalhosAtivos.Count >= 5)
            Ocupado = NivelOcupacaoMecanico.Indisponivel;

        else if (TrabalhosAtivos.Count >= 3)
            Ocupado = NivelOcupacaoMecanico.Ocupado;
        else 
            Ocupado = NivelOcupacaoMecanico.Disponivel;
    }

    public void AlterarEspecialidade(EspecialidadeMecanico especialidade)
        => Especialidade = especialidade;

    public void AtualizarDadosFuncionario(NivelFuncionario nivel, DateTime dataContratacao)
        => DadosFuncionario = new DadosFuncionario(nivel, dataContratacao);
}