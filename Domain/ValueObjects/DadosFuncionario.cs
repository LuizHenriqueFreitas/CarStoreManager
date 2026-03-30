using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.ValueObjects;

public class DadosFuncionario
{
    public NivelFuncionario Nivel { get; private set; }
    public DateTime DataContratacao { get; private set; }

    protected DadosFuncionario() { }

    public DadosFuncionario(NivelFuncionario nivel, DateTime dataContratacao)
    {
        if (dataContratacao > DateTime.UtcNow)
            throw new ArgumentException("Data de contratação inválida");

        Nivel = nivel;
        DataContratacao = dataContratacao;
    }

    public int GetAnosEmpresa()
{
    var hoje = DateTime.UtcNow;
    var anos = hoje.Year - DataContratacao.Year;

    if (hoje < DataContratacao.AddYears(anos))
        anos--;

    return anos;
}
}