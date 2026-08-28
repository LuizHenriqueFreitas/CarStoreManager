namespace CarStoreManager.Application.Interfaces;

/// <summary>
/// Criptografa/descriptografa segredos reversíveis antes de persistir no BD
/// (ex.: access/refresh token do Mercado Livre). Este codebase não tinha
/// nenhum mecanismo de criptografia em repouso antes desta integração. Para
/// os tokens OAuth do ML (credenciais de acesso reais à conta), usar
/// IDataProtector (nativo do ASP.NET Core, sem dependência nova) é uma
/// melhoria deliberada, não a convenção já existente no projeto.
/// </summary>
public interface ITokenCriptografiaService
{
    string Proteger(string valor);
    string Desproteger(string valorProtegido);
}
