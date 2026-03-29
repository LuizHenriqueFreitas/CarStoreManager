//classe base do vendedor = herda de Unuario.cs

using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Concessionaria;

public class Vendedor : Usuario
{
    public DadosFuncionario DadosFuncionario { get; private set; } = null!;

    protected Vendedor() { }

    public Vendedor(
        string nome,
        Email email,
        Telefone telefone,
        string senhaHash,
        NivelFuncionario nivel,
        DateTime dataContratacao)
        : base(nome, email, telefone, senhaHash, RoleUsuario.Vendedor)
    {
        DadosFuncionario = new DadosFuncionario(nivel, dataContratacao);
    }

    // =====================
    // GETTERS
    // =====================

    public string GetNivel() => DadosFuncionario.Nivel.ToString();

    public DateTime GetDataContratacao() => DadosFuncionario.DataContratacao;

    // =============================
    // REGRAS DE NEGOCIOS - SETERS
    // =============================

    public void AtualizarDadosFuncionario(NivelFuncionario nivel, DateTime dataContratacao)
        => DadosFuncionario = new DadosFuncionario(nivel, dataContratacao);
}