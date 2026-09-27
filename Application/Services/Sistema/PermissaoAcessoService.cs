using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;

namespace CarStoreManager.Application.Services.Sistema;

public class PermissaoAcessoService : IPermissaoAcessoService
{
    private readonly IPermissaoAcessoRepository _repository;
    private readonly IPermissaoIndividualRepository _repositorioIndividual;

    public PermissaoAcessoService(IPermissaoAcessoRepository repository, IPermissaoIndividualRepository repositorioIndividual)
    {
        _repository = repository;
        _repositorioIndividual = repositorioIndividual;
    }

    public async Task<bool> PodeAcessarAsync(RoleUsuario role, string recursoChave)
    {
        var recurso = CatalogoRecursosProtegiveis.ObterPorChave(recursoChave);
        // Recurso desconhecido (não devia acontecer — catálogo é a fonte de
        // verdade) não é bloqueado por padrão: falha aberta pra não travar
        // uma tela por um erro de digitação na chave, mas nunca dá acesso a
        // um papel que nem existe no teto do catálogo.
        if (recurso is null) return true;
        if (!recurso.RolesPermitidos.Contains(role)) return false;

        var overrideSalvo = await _repository.GetAsync(role, recursoChave);
        return overrideSalvo?.Permitido ?? true;
    }

    public async Task<List<RecursoPermissaoDTO>> ObterMatrizAsync()
    {
        var overrides = (await _repository.GetAllAsync())
            .ToDictionary(p => (p.Role, p.RecursoChave), p => p.Permitido);

        return CatalogoRecursosProtegiveis.Itens.Select(recurso => new RecursoPermissaoDTO
        {
            Chave = recurso.Chave,
            Tipo = recurso.Tipo.ToString(),
            Area = recurso.Area,
            Rotulo = recurso.Rotulo,
            PermitidoPorPapel = recurso.RolesPermitidos.ToDictionary(
                role => role.ToString(),
                role => overrides.TryGetValue((role, recurso.Chave), out var permitido) ? permitido : true)
        }).ToList();
    }

    public async Task<Result> AtualizarAsync(RoleUsuario role, string recursoChave, bool permitido)
    {
        var recurso = CatalogoRecursosProtegiveis.ObterPorChave(recursoChave);
        if (recurso is null)
            return Result.Fail("Recurso não encontrado no catálogo.");
        if (!recurso.RolesPermitidos.Contains(role))
            return Result.Fail($"O papel {role} não é elegível para este recurso — o sistema já bloqueia esse acesso de forma fixa.");

        try
        {
            var existente = await _repository.GetAsync(role, recursoChave);
            if (existente is null)
                await _repository.AddAsync(new PermissaoAcesso(role, recursoChave, permitido));
            else
            {
                existente.Atualizar(permitido);
                _repository.Update(existente);
            }

            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Erro ao salvar permissão: {ex.Message}");
        }
    }

    public async Task<bool> PodeAcessarAsync(Guid usuarioId, RoleUsuario role, string recursoChave)
    {
        var individual = await _repositorioIndividual.GetAsync(usuarioId, recursoChave);
        if (individual is not null) return individual.Permitido;

        return await PodeAcessarAsync(role, recursoChave);
    }

    public async Task<List<RecursoPermissaoIndividualDTO>> ObterMatrizIndividualAsync(Guid usuarioId, RoleUsuario roleDoUsuario)
    {
        var overrides = (await _repositorioIndividual.GetAllPorUsuarioAsync(usuarioId))
            .ToDictionary(p => p.RecursoChave, p => p.Permitido);

        var lista = new List<RecursoPermissaoIndividualDTO>();
        foreach (var recurso in CatalogoRecursosProtegiveis.Itens)
        {
            lista.Add(new RecursoPermissaoIndividualDTO
            {
                Chave = recurso.Chave,
                Tipo = recurso.Tipo.ToString(),
                Area = recurso.Area,
                Rotulo = recurso.Rotulo,
                PadraoDoPapel = await PodeAcessarAsync(roleDoUsuario, recurso.Chave),
                OverrideIndividual = overrides.TryGetValue(recurso.Chave, out var permitido) ? permitido : (bool?)null
            });
        }
        return lista;
    }

    public async Task<Result> AtualizarIndividualAsync(Guid usuarioId, string recursoChave, bool? permitido)
    {
        var recurso = CatalogoRecursosProtegiveis.ObterPorChave(recursoChave);
        if (recurso is null)
            return Result.Fail("Recurso não encontrado no catálogo.");

        try
        {
            var existente = await _repositorioIndividual.GetAsync(usuarioId, recursoChave);

            if (permitido is null)
            {
                if (existente is not null)
                {
                    _repositorioIndividual.Remove(existente);
                    await _repositorioIndividual.SaveChangesAsync();
                }
                return Result.Ok();
            }

            if (existente is null)
                await _repositorioIndividual.AddAsync(new PermissaoIndividual(usuarioId, recursoChave, permitido.Value));
            else
            {
                existente.Atualizar(permitido.Value);
                _repositorioIndividual.Update(existente);
            }

            await _repositorioIndividual.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Erro ao salvar exceção individual: {ex.Message}");
        }
    }
}
