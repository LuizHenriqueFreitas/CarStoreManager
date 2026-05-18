//classe base do vendedor = herda de Unuario.cs

using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Concessionaria;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de Cliente.cs

    Esta classe tem testes automaticos implementados para:
        Nada ainda
*/

public class Vendedor : Usuario
{
    public DadosFuncionario DadosFuncionario { get; private set; } = null!;

    protected Vendedor() { }

    //construtor herdado de Usuario.cs
    public Vendedor(
        string nome,
        string email,
        string telefone,
        string senhaHash,
        NivelFuncionario nivel,
        DateTime dataContratacao)
        : base(nome, email, telefone, senhaHash, RoleUsuario.Vendedor)
    {
        DadosFuncionario = new DadosFuncionario(nivel, dataContratacao);
    }

    // getters
    public string GetNivel() => DadosFuncionario.GetNivel().ToString();
    public DateTime GetDataContratacao() => DadosFuncionario.GetDataContratacao();

    // setter de nivel
    public void AtualizarNivelVendedor(NivelFuncionario nivel)
        => DadosFuncionario.AtualizarNivelFuncionario(nivel);
}