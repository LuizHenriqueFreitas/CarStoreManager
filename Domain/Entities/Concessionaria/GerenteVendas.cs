using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Concessionaria;

/*
    Gerente de vendas — gestor da área da concessionária. Abaixo do
    admin na hierarquia: tem acesso às telas administrativas da
    concessionária (cadastra veículos novos, vê relatórios de vendas),
    mas não pode configurar o sistema nem mexer em usuários.
*/
public class GerenteVendas : Usuario
{
    public DadosFuncionario DadosFuncionario { get; private set; } = null!;

    protected GerenteVendas() { }

    public GerenteVendas(
        string nome,
        string email,
        string telefone,
        string senha,
        NivelFuncionario nivel,
        DateTime dataContratacao)
        : base(nome, email, telefone, senha, RoleUsuario.GerenteVendas)
    {
        DadosFuncionario = new DadosFuncionario(nivel, dataContratacao);
    }

    public string GetNivel() => DadosFuncionario.GetNivel().ToString();
    public DateTime GetDataContratacao() => DadosFuncionario.GetDataContratacao();

    public void AtualizarNivel(NivelFuncionario nivel)
        => DadosFuncionario.AtualizarNivelFuncionario(nivel);
}
