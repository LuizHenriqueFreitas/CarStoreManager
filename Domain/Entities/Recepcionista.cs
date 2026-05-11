using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities;

/*
    Recepcionista — funcionário responsável pela recepção da oficina:
    cadastra cliente, registra veículo do cliente, abre OS e a atribui
    a um mecânico, gera e comunica orçamentos.
*/
public class Recepcionista : Usuario
{
    public DadosFuncionario DadosFuncionario { get; private set; } = null!;

    protected Recepcionista() { }

    public Recepcionista(
        string nome,
        string email,
        string telefone,
        string senhaHash,
        decimal salario,
        NivelFuncionario nivel,
        DateTime dataContratacao)
        : base(nome, email, telefone, senhaHash, salario, RoleUsuario.Recepcionista)
    {
        DadosFuncionario = new DadosFuncionario(nivel, dataContratacao);
    }

    public string GetNivel() => DadosFuncionario.GetNivel().ToString();
    public DateTime GetDataContratacao() => DadosFuncionario.GetDataContratacao();

    public void AtualizarNivel(NivelFuncionario nivel)
        => DadosFuncionario.AtualizarNivelFuncionario(nivel);
}
