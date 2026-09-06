using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema.TemplateDocumento;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;

namespace CarStoreManager.Application.Services.Sistema;

public class TemplateDocumentoService : ITemplateDocumentoService
{
    private readonly ITemplateDocumentoRepository _repo;

    public TemplateDocumentoService(ITemplateDocumentoRepository repo) => _repo = repo;

    public async Task<Result<IEnumerable<TemplateDocumentoDTO>>> GetAllAsync()
    {
        var lista = await _repo.GetAllAsync();
        return Result<IEnumerable<TemplateDocumentoDTO>>.Ok(lista.Select(MapToDto));
    }

    public async Task<Result<IEnumerable<TemplateDocumentoLookupDTO>>> GetLookupAtivosAsync()
    {
        var lista = await _repo.GetAtivosAsync();
        return Result<IEnumerable<TemplateDocumentoLookupDTO>>.Ok(
            lista.Select(t => new TemplateDocumentoLookupDTO { Id = t.Id, Nome = t.Nome }));
    }

    public async Task<Result<TemplateDocumentoDTO>> GetByIdAsync(Guid id)
    {
        var t = await _repo.GetByIdAsync(id);
        return t is null
            ? Result<TemplateDocumentoDTO>.Fail("Template não encontrado.")
            : Result<TemplateDocumentoDTO>.Ok(MapToDto(t));
    }

    public async Task<Result<Guid>> AddAsync(SalvarTemplateDocumentoDTO dto)
    {
        try
        {
            var template = new Domain.Entities.Sistema.TemplateDocumento(dto.Nome, dto.Conteudo);
            if (!dto.Ativo) template.Desativar();

            await _repo.AddAsync(template);
            await _repo.SaveChangesAsync();
            return Result<Guid>.Ok(template.Id);
        }
        catch (ArgumentException ex) { return Result<Guid>.Fail(ex.Message); }
        catch (Exception) { return Result<Guid>.Fail("Não foi possível criar o template. Tente novamente em instantes."); }
    }

    public async Task<Result> UpdateAsync(SalvarTemplateDocumentoDTO dto)
    {
        var template = await _repo.GetByIdAsync(dto.Id);
        if (template is null) return Result.Fail("Template não encontrado.");

        try
        {
            template.AtualizarNome(dto.Nome);
            template.AtualizarConteudo(dto.Conteudo);
            if (dto.Ativo && !template.Ativo) template.Reativar();
            else if (!dto.Ativo && template.Ativo) template.Desativar();

            _repo.Update(template);
            await _repo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (ArgumentException ex) { return Result.Fail(ex.Message); }
        catch (Exception) { return Result.Fail("Não foi possível salvar o template. Tente novamente em instantes."); }
    }

    public async Task<Result> RemoveAsync(Guid id)
    {
        var template = await _repo.GetByIdAsync(id);
        if (template is null) return Result.Fail("Template não encontrado.");

        try
        {
            _repo.Remove(template);
            await _repo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception)
        {
            return Result.Fail("Não foi possível excluir o template.");
        }
    }

    private static TemplateDocumentoDTO MapToDto(Domain.Entities.Sistema.TemplateDocumento t) => new()
    {
        Id = t.Id,
        Nome = t.Nome,
        Conteudo = t.Conteudo,
        Ativo = t.Ativo,
        DataUltimaAtualizacao = t.DataUltimaAtualizacao
    };
}
