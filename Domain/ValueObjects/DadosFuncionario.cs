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
        => (int)((DateTime.UtcNow - DataContratacao).TotalDays / 365);
}