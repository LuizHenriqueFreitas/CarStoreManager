using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Auth;

namespace CarStoreManager.Application.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResultDTO>> LoginAsync(LoginDTO dto);
    Task<Result<Guid>> CriarUsuarioAsync(CriarUsuarioDTO dto);
    Task<Result> VerificarSenhaAsync(Guid usuarioId, string senha);
    Task<Result> DesativarUsuarioAsync(Guid usuarioId);

    Task<Result<UsuarioDTO>> ObterUsuarioAsync(Guid id);
    Task<Result> AtualizarUsuarioAsync(Guid id, AtualizarUsuarioDTO dto);
    Task<Result> AlterarSenhaAsync(Guid id, string senhaAtual, string novaSenha);
    Task<Result> LogoutAsync(Guid usuarioId);
}