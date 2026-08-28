using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace CarStoreManager.Infrastructure.Services;

public class TokenCriptografiaService : ITokenCriptografiaService
{
    private readonly IDataProtector _protector;

    public TokenCriptografiaService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("CarStoreManager.MercadoLivre.Tokens");
    }

    public string Proteger(string valor) => _protector.Protect(valor);

    public string Desproteger(string valorProtegido) => _protector.Unprotect(valorProtegido);
}
