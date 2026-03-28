using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Auth;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace CarStoreManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _repository;
    private readonly IJwtService _jwtService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUsuarioRepository repository,
        IJwtService jwtService,
        IOptions<JwtSettings> jwtSettings)
    {
        _repository = repository;
        _jwtService = jwtService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<Result<LoginResultDTO>> LoginAsync(LoginDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Senha))
            return Result<LoginResultDTO>.Fail("Email e senha são obrigatórios");

        var usuario = await _repository.ObterPorEmailAsync(dto.Email.ToLower());

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
            return Result<LoginResultDTO>.Fail("Email ou senha inválidos");

        if (!usuario.Ativo)
            return Result<LoginResultDTO>.Fail("Usuário inativo");

        var token = _jwtService.GerarToken(usuario);

        return Result<LoginResultDTO>.Ok(new LoginResultDTO
        {
            Token = token,
            Nome = usuario.Nome,
            Role = usuario.Role.ToString(),
            Expiracao = DateTime.UtcNow.AddHours(_jwtSettings.ExpiracaoHoras)
        });
    }

    public async Task<Result<Guid>> CriarUsuarioAsync(CriarUsuarioDTO dto)
    {
        if (await _repository.EmailExisteAsync(dto.Email))
            return Result<Guid>.Fail("Email já cadastrado");

        if (!Enum.TryParse<RoleUsuario>(dto.Role, true, out var role))
            return Result<Guid>.Fail("Role inválido. Use 'Vendedor' ou 'Mecanico'");

        if (role == RoleUsuario.Admin)
            return Result<Guid>.Fail("Não é permitido criar novos admins por esta rota");

        try
        {
            var email = new Email(dto.Email);
            var telefone = new Telefone(dto.Telefone);
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);

            Usuario usuario = role switch
            {
                RoleUsuario.Vendedor => CriarVendedor(dto, email, telefone, senhaHash),
                RoleUsuario.Mecanico => CriarMecanico(dto, email, telefone, senhaHash),
                _ => throw new ArgumentException("Role inválido")
            };

            await _repository.AddAsync(usuario);
            await _repository.SaveChangesAsync();

            return Result<Guid>.Ok(usuario.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar usuário: {ex.Message}");
        }
    }

    public async Task<Result> VerificarSenhaAsync(Guid usuarioId, string senha)
    {
        var usuario = await _repository.GetByIdAsync(usuarioId);
        if (usuario is null)
            return Result.Fail("Usuário não encontrado");

        return BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash)
            ? Result.Ok()
            : Result.Fail("Senha incorreta");
    }

    // =========================
    // PRIVADOS
    // =========================

    private static Vendedor CriarVendedor(
        CriarUsuarioDTO dto, Email email, Telefone telefone, string senhaHash)
    {
        if (!Enum.TryParse<NivelFuncionario>(dto.Nivel, true, out var nivel))
            throw new ArgumentException("Nível inválido");

        if (dto.DataContratacao is null)
            throw new ArgumentException("Data de contratação obrigatória para vendedor");

        return new Vendedor(dto.Nome, email, telefone, senhaHash,
            nivel, dto.DataContratacao.Value);
    }

    private static Mecanico CriarMecanico(
        CriarUsuarioDTO dto, Email email, Telefone telefone, string senhaHash)
    {
        if (!Enum.TryParse<EspecialidadeMecanico>(dto.Especialidade, true, out var especialidade))
            throw new ArgumentException("Especialidade inválida");

        if (!Enum.TryParse<NivelFuncionario>(dto.Nivel, true, out var nivel))
            throw new ArgumentException("Nível inválido");

        if (dto.DataContratacao is null)
            throw new ArgumentException("Data de contratação obrigatória para mecânico");

        return new Mecanico(dto.Nome, email, telefone, senhaHash,
            especialidade, nivel, dto.DataContratacao.Value);
    }
}