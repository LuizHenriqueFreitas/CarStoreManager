using System.Diagnostics;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Auth;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace CarStoreManager.Application.Services;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de AuthService.cs.

    Esta classe tem testes automaticos implementados para:
        Bloquear login com email e senha incorretos/vazios,
        Bloquear login com email inexistente,
        Bloquear login com senha incorreta,
        Bloquear login de usuario inativo,
        Validar login de usuario valido e retornar Token + Role,
        Bloquear criar novo usuario com email ja cadastrado,
        Valida criação de usuario role Admin,
        Valida criação de usuario role Vendedor,
        Valida criação de usuario role Mecanico,
        Bloquear usuarios nao encontrados,
        Valida cadastro de usuarios com senha correta,
        Bloquear tentativa de inativar usuario inexistente,
        Desativar e salvar usuario existente,
        Obter um usuario e retornar um DTO com suas infromações,
        Bloquear atualização de usuario inexistente,
        Validar atualização de usuario existente e salvar,
        Deve bloquear caso a senha atual esteja incorreta,
        Valida atualização e salva nova senha,
        Bloqueia logout de usuario inexistente,
        Valida logout de usuario
*/

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

    /*
        Metodo de login
        verifica que os campos de email e senha foram preenchidos
        verifica se o usuario existe com base no email e se a senha esta correta
        verifica se o usuario esta ativo e pode ser acessado
    */
    public async Task<Result<LoginResultDTO>> LoginAsync(LoginDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Senha))
            return Result<LoginResultDTO>.Fail("Email e senha são obrigatórios");

        var usuario = await _repository.ObterPorEmailAsync(dto.Email.ToLower());

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.GetSenhaHash()))
            return Result<LoginResultDTO>.Fail("Email ou senha inválidos");

        if (!usuario.Ativo)
            return Result<LoginResultDTO>.Fail("Usuário inativo");

        var token = _jwtService.GerarToken(usuario);

        return Result<LoginResultDTO>.Ok(new LoginResultDTO
        {
            Token = token,
            Nome = usuario.GetNome(),
            Role = usuario.GetRole(),
            Expiracao = DateTime.UtcNow.AddHours(_jwtSettings.ExpiracaoHoras)
        });
    }

    /*
        metodo de criar usuario
        verifica que não há outro usuario cadastrado com o mesmo email
        verifica a role enviada, deve ser:
        Admin, Vendedor ou Mecanico.
    */
    public async Task<Result<Guid>> CriarUsuarioAsync(CriarUsuarioDTO dto)
    {
        if (await _repository.EmailExisteAsync(dto.Email))
            return Result<Guid>.Fail("Email já cadastrado");

        if (!Enum.TryParse<RoleUsuario>(dto.Role, true, out var role))
            return Result<Guid>.Fail("Role inválido. Use 'Vendedor', 'Mecanico' ou 'Admin'");

        try
        {
            Usuario usuario = role switch
            {
                RoleUsuario.Vendedor => CriarVendedor(dto),
                RoleUsuario.Mecanico => CriarMecanico(dto),
                RoleUsuario.Admin => CriarAdmin(dto),
                RoleUsuario.Recepcionista => CriarRecepcionista(dto),
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

    /*
        metodo de verificação de senha
        busca o usuario por Id e verifica a senha cadastrada
        com a informada
    */
    public async Task<Result> VerificarSenhaAsync(Guid usuarioId, string senha)
    {
        var usuario = await _repository.GetByIdAsync(usuarioId);
        if (usuario is null)
            return Result.Fail("Usuário não encontrado");

        return BCrypt.Net.BCrypt.Verify(senha, usuario.GetSenhaHash())
            ? Result.Ok()
            : Result.Fail("Senha incorreta");
    }

    /*
        Abaixo temos os metodos auxiliares para criação
        detalhada de Vendedor ou Mecanico que tem parametros
        a mais. Como Nivel de experiencia e 
        DataContratacao, sendo que o mecanico ainda tem 
        uma especialidade.
    */
    private static Vendedor CriarVendedor(CriarUsuarioDTO dto)
    {
        //pega o nivel de experiencia correspondente a entrada
        if (!Enum.TryParse<NivelFuncionario>(dto.Nivel, true, out var nivel))
            throw new ArgumentException("Nível inválido");
        //verifica que a data de contratação esteja preenchida
        if (dto.DataContratacao is null)
            throw new ArgumentException("Data de contratação obrigatória para vendedor");

        return new Vendedor(
            dto.Nome,
            dto.Email,
            dto.Telefone,
            dto.Senha,
            nivel,
            dto.DataContratacao.Value);
    }

    private static Mecanico CriarMecanico(CriarUsuarioDTO dto)
    {
        //pega a especialidade correspondente a entrada
        if (!Enum.TryParse<EspecialidadeMecanico>(dto.Especialidade, true, out var especialidade))
            throw new ArgumentException("Especialidade inválida");
        //pega o nivel de experiencia correspondente a entrada
        if (!Enum.TryParse<NivelFuncionario>(dto.Nivel, true, out var nivel))
            throw new ArgumentException("Nível inválido");
        //verifica que a data de contratação esteja preenchida
        if (dto.DataContratacao is null)
            throw new ArgumentException("Data de contratação obrigatória para mecânico");

        return new Mecanico(
            dto.Nome,
            dto.Email,
            dto.Telefone,
            dto.Senha,
            especialidade,
            nivel,
            dto.DataContratacao!.Value);
    }

    private static Recepcionista CriarRecepcionista(CriarUsuarioDTO dto)
    {
        if (!Enum.TryParse<NivelFuncionario>(dto.Nivel, true, out var nivel))
            throw new ArgumentException("Nível inválido");
        if (dto.DataContratacao is null)
            throw new ArgumentException("Data de contratação obrigatória para recepcionista");

        return new Recepcionista(
            dto.Nome,
            dto.Email,
            dto.Telefone,
            dto.Senha,
            nivel,
            dto.DataContratacao.Value);
    }

    /*
        metodo para criar usuario da role Admin
        Ele é basicamente o construtor do usuario mesmo.
    */
    private static Admin CriarAdmin(
        CriarUsuarioDTO dto)
    {
        return new Admin(
            dto.Nome,
            dto.Email,
            dto.Telefone,
            dto.Senha
        );
    }

    //metodo para desativar usuario
    public async Task<Result> DesativarUsuarioAsync(Guid usuarioId)
    {
        var usuario = await _repository.GetByIdAsync(usuarioId);
        if (usuario is null) 
            return Result.Fail("Usuário não encontrado");

        usuario.Desativar();
        _repository.Update(usuario);

        await _repository.SaveChangesAsync();
        return Result.Ok();
    }

    //metodo para listar usuarios — opcionalmente filtrado por role
    public async Task<Result<IEnumerable<UsuarioDTO>>> ListarUsuariosAsync(string? role = null)
    {
        var todos = await _repository.GetAllAsync();
        var filtrados = todos.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(role))
            filtrados = filtrados.Where(u => string.Equals(u.GetRole(), role, StringComparison.OrdinalIgnoreCase));

        var dtos = filtrados.Select(u => new UsuarioDTO
        {
            Id = u.Id,
            Nome = u.GetNome(),
            Email = u.GetEmail(),
            Telefone = u.GetTelefone(),
            Role = u.GetRole()
        });

        return Result<IEnumerable<UsuarioDTO>>.Ok(dtos);
    }

    //metodo para obter usuario
    public async Task<Result<UsuarioDTO>> ObterUsuarioAsync(Guid id)
    {
        var usuario = await _repository.GetByIdAsync(id);
        if (usuario is null)
            return Result<UsuarioDTO>.Fail("Usuário não encontrado");

        return Result<UsuarioDTO>.Ok(new UsuarioDTO
        {
            Id = usuario.Id,
            Nome = usuario.GetNome(),
            Email = usuario.GetEmail(),
            Telefone = usuario.GetTelefone(),
            Role = usuario.GetRole()
        });
    }

    //metodo para atualizar usuario
    public async Task<Result> AtualizarUsuarioAsync(Guid id, AtualizarUsuarioDTO dto)
    {
        var usuario = await _repository.GetByIdAsync(id);
        if (usuario is null)
            return Result.Fail("Usuário não encontrado");

        try
        {
            usuario.AtualizarDadosPessoais(dto.Nome, dto.Email, dto.Telefone);
            _repository.Update(usuario);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Erro ao atualizar: {ex.Message}");
        }
    }

    /*
        Metodo para trocar senha do usuario
        requer confirmação da senha atual antes de trocar.
    */
    public async Task<Result> AlterarSenhaAsync(Guid id, string senhaAtual, string novaSenha)
    {
        var usuario = await _repository.GetByIdAsync(id);
        if (usuario is null)
            return Result.Fail("Usuário não encontrado");

        if (!BCrypt.Net.BCrypt.Verify(senhaAtual, usuario.GetSenhaHash()))
            return Result.Fail("Senha atual incorreta");

        usuario.AtualizarSenha(novaSenha);
        _repository.Update(usuario);
        await _repository.SaveChangesAsync();
        return Result.Ok();
    }

    //o logout é client side, mas temos esse metodo para implementar quando necessaio
    public async Task<Result> LogoutAsync(Guid usuarioId)
    {
        var usuario = await _repository.GetByIdAsync(usuarioId);
        if (usuario is null) 
            return Result.Fail("Usuário não encontrado");

        return Result.Ok();
    }
}