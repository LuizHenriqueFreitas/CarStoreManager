using CarStoreManager.Application.DTOs.Auth;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Geradores.Dados;
using CarStoreManager.Geradores.Nucleo;

namespace CarStoreManager.Geradores.Entidades;

/// <summary>
/// Gera os 6 tipos de usuário (Vendedor, Mecânico, Recepcionista, Chefe de
/// oficina, Gerente de vendas, Admin) usando o MESMO caminho unificado que a
/// tela "Novo usuário" usa (IAuthService.CriarUsuarioAsync) — respeita a
/// checagem de e-mail duplicado e as mesmas regras de validação da UI.
///
/// Senha de todos os usuários gerados: "Teste@123" (documentada no README).
/// </summary>
public static class UsuarioGerador
{
    public const string SenhaPadrao = "Teste@123";

    public static async Task<Dictionary<string, List<Guid>>> GerarAsync(
        IServiceProvider provider,
        Random rng,
        UniquePool emailPool,
        UniquePool telefonePool,
        Dictionary<string, int> quantidadePorTipo)
    {
        var resultado = new Dictionary<string, List<Guid>>();

        foreach (var (role, quantidade) in quantidadePorTipo)
        {
            var ids = await ExecutorLote.ExecutarAsync<IAuthService>(
                provider,
                quantidade,
                (servico, _) => servico.CriarUsuarioAsync(MontarDto(role, rng, emailPool, telefonePool)),
                $"Usuários ({RoleLabel(role)})");

            resultado[role] = ids;
        }

        return resultado;
    }

    private static CriarUsuarioDTO MontarDto(string role, Random rng, UniquePool emailPool, UniquePool telefonePool)
    {
        var nome = NomesPt.NomeCompletoAleatorio(rng);
        var email = emailPool.Reservar(() => NomesPt.EmailPara(nome, "carstore.com.br", rng));
        var telefone = telefonePool.Reservar(() => DocumentoUtils.GerarTelefoneCelular(rng));

        var dto = new CriarUsuarioDTO
        {
            Nome = nome,
            Email = email,
            Telefone = telefone,
            Senha = SenhaPadrao,
            Role = role
        };

        if (role != "Admin")
        {
            dto.Nivel = new[] { "Junior", "Pleno", "Senior" }[rng.Next(3)];
            // DadosFuncionario só aceita contratações entre hoje e +31 dias.
            dto.DataContratacao = DateTime.Now.Date.AddDays(rng.Next(0, 31));
        }

        if (role == "Mecanico")
        {
            var especialidades = Enum.GetValues<EspecialidadeMecanico>();
            dto.Especialidade = especialidades[rng.Next(especialidades.Length)].ToString();
        }

        return dto;
    }

    private static string RoleLabel(string role) => role switch
    {
        "Vendedor" => "Vendedor",
        "Mecanico" => "Mecânico",
        "Recepcionista" => "Recepcionista",
        "ChefeOficina" => "Chefe de oficina",
        "GerenteVendas" => "Gerente de vendas",
        "Admin" => "Admin",
        _ => role
    };
}
