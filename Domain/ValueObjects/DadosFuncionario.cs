using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Exceptions;

namespace CarStoreManager.Domain.ValueObjects;

/*
    Neste arquivo temos as implementações de verificações 
    e regras de negócios do VO de Dados dos funcionarios, 
    alem dos metodos de: GetNivel() e GetDataContratacao().

    Testes automaticos implementados para:
        Validar um funcionario valido contratado hoje,
        Validar um funcionario valido contradado para até 30 dias no futuro,
        Bloquear funcionario cadastrado antes de hoje,
        Bloquear funcionario cadastrado mais de 30 dias no futuro,
        Validar criar um funcioanrio de nivel Junior,
        Validar criar um funcioanrio de nivel Pleno,
        Validar criar um funcioanrio de nivel Senior,
        Calculo de anos trabalhando na empresa.
*/

public class DadosFuncionario
{
    private NivelFuncionario Nivel { get; set; }
    private DateTime DataContratacao { get; set; }

    protected DadosFuncionario() { }

    public DadosFuncionario(NivelFuncionario nivel, DateTime dataContratacao)
    {
        //valida a data no construtor
        if (!ValidarDataContratacao(dataContratacao))
            throw new DataContratacaoInvalidoException(dataContratacao);
        //valida o nivel no construtor
        if (!ValidarNivelFuncionario(nivel))
            throw new NivelFuncionarioInvalidoException(nivel.ToString());
        
        Nivel = nivel;
        DataContratacao = dataContratacao;
    }

    //metodos getters
    public NivelFuncionario GetNivel()
    {
        return Nivel;
    }

    public DateTime GetDataContratacao()
    {
        return DataContratacao;
    }

    //metodo para atualizar o nivel, serve como um setter
    public void AtualizarNivelFuncionario(NivelFuncionario novoNivel)
    {
        if(novoNivel == NivelFuncionario.Junior ||
            novoNivel == NivelFuncionario.Pleno ||
            novoNivel == NivelFuncionario.Senior)
            Nivel = novoNivel;
        else 
            throw new ArgumentException("Nivel informado não é aceito");
    }

    /*
        verifica se a contratação esta no prazo permitido 
        de hoje até 30 dias no futuro - esse é o range permitido
    */
    public bool ValidarDataContratacao(DateTime data)
    {
        if (data > DateTime.Now.AddDays(31))
            return false;
        if (data < DateTime.Now)
            return false;

        return true;
    }

    //valida se o nivel do funcionario é um dos niveis do Enum de NivelFuncionario.cs
    public bool ValidarNivelFuncionario(NivelFuncionario nivel)
    {
        if (!(nivel == NivelFuncionario.Junior || 
              nivel == NivelFuncionario.Pleno  || 
              nivel == NivelFuncionario.Senior
            ))
            return false;
        return true;
    }

    /*
        Calcula os anos de empresa do funcionario
        Menos de 1 ano completo devera ser 0
    */
    public int CalcularAnosEmpresa()
    {
        var hoje = DateTime.UtcNow;
        var anos = hoje.Year - DataContratacao.Year;

        if (hoje < DataContratacao.AddYears(anos))
            anos--;

        return anos;
    }
}