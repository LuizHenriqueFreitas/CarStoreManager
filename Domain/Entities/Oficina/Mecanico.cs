//classe base do mecanico herda de Uduario.cs

using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Oficina;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de Mecanico.cs

    Esta classe tem testes automaticos implementados para:
        Nada ainda
*/

public class Mecanico : Usuario
{
    public EspecialidadeMecanico Especialidade { get; private set; }
    public NivelOcupacaoMecanico Ocupado { get; private set; }
    public DadosFuncionario DadosFuncionario { get; private set; } = null!;
    public List<Guid> TrabalhosAtivos {get; private set; } = new ();

    protected Mecanico() { }

    public Mecanico(
        string nome,
        string email,
        string telefone,
        string senhaHash,
        EspecialidadeMecanico especialidade,
        NivelFuncionario nivel,
        DateTime dataContratacao)
        : base(nome, email, telefone, senhaHash, RoleUsuario.Mecanico)
    {
        Especialidade = especialidade;
        DadosFuncionario = new DadosFuncionario(nivel, dataContratacao);
    }

    /* ================================
        metodos GETTERS dos atributos
     ================================*/
    public string GetEspecialidade() => Especialidade.ToString();
    public string GetOcupado() => Ocupado.ToString();
    public string GetNivelExperiencia() => DadosFuncionario.GetNivel().ToString();
    public DateTime GetDataContratacao() => DadosFuncionario.GetDataContratacao();

    /* =====================================
        metodos SETTERS de cada atributo
        com regras de negocio aplicadas
     =====================================*/

    /*
        Altera o nivela de ocupacao de acordo 
        com o numero de trabalhos ativos
    */
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

    public void AtualizarDadosFuncionario(NivelFuncionario nivel)
        => DadosFuncionario.AtualizarNivelFuncionario(nivel);
}