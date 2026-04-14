using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Auth;

namespace CarStoreManager.Application.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResultDTO>> LoginAsync(LoginDTO dto);
    Task<Result<Guid>> CriarUsuarioAsync(CriarUsuarioDTO dto);
    Task<Result> VerificarSenhaAsync(Guid usuarioId, string senha);
    Task<Result> DesativarUsuarioAsync(Guid usuarioId);
}