using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Oficina;

/*
    Chefe de oficina — gestor da área da oficina. Abaixo do admin na
    hierarquia: tem acesso às telas administrativas da oficina (compõe
    estoque, encomenda peças, vê relatórios da oficina), mas não pode
    configurar o sistema nem mexer em usuários.
*/
public class ChefeOficina : Usuario
{
    public DadosFuncionario DadosFuncionario { get; private set; } = null!;

    protected ChefeOficina() { }

    public ChefeOficina(
        string nome,
        string email,
        string telefone,
        string senha,
        NivelFuncionario nivel,
        DateTime dataContratacao)
        : base(nome, email, telefone, senha, RoleUsuario.ChefeOficina)
    {
        DadosFuncionario = new DadosFuncionario(nivel, dataContratacao);
    }

    public string GetNivel() => DadosFuncionario.GetNivel().ToString();
    public DateTime GetDataContratacao() => DadosFuncionario.GetDataContratacao();

    public void AtualizarNivel(NivelFuncionario nivel)
        => DadosFuncionario.AtualizarNivelFuncionario(nivel);
}
