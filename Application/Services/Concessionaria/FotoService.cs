using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Interfaces.Repositories;
using CarStoreManager.Domain.Entities;

namespace CarStoreManager.Application.Services;

public class FotoService : IFotoService
{
    private const long TamanhoMaximoBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly HashSet<string> ContentTypesPermitidos =
        new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png" };

    private readonly IFotoRepository _fotoRepo;
    private readonly IArquivoStorage _storage;

    public FotoService(IFotoRepository fotoRepo, IArquivoStorage storage)
    {
        _fotoRepo = fotoRepo;
        _storage = storage;
    }

    public async Task<Result<List<FotoDto>>> UploadFotosAsync(
        string entidadeTipo, Guid entidadeId, IList<ArquivoUpload> arquivos)
    {
        if (arquivos is null || arquivos.Count == 0)
            return Result<List<FotoDto>>.Fail("Nenhum arquivo enviado.");

        var ordemAtual = await _fotoRepo.GetNextOrdemAsync(entidadeTipo, entidadeId);
        var fotosSalvas = new List<Foto>();

        foreach (var arquivo in arquivos)
        {
            if (arquivo.Tamanho > TamanhoMaximoBytes)
                return Result<List<FotoDto>>.Fail($"Arquivo {arquivo.NomeArquivo} excede 5MB.");

            if (!ContentTypesPermitidos.Contains(arquivo.ContentType))
                return Result<List<FotoDto>>.Fail($"Formato não suportado: {arquivo.NomeArquivo}. Use JPEG ou PNG.");

            var url = await _storage.SalvarAsync(arquivo, entidadeTipo.ToLowerInvariant());

            var foto = new Foto(
                entidadeTipo,
                entidadeId,
                url,
                arquivo.NomeArquivo,
                arquivo.Tamanho,
                arquivo.ContentType,
                ordemAtual++
            );
            await _fotoRepo.AddAsync(foto);
            fotosSalvas.Add(foto);
        }

        await _fotoRepo.SaveChangesAsync();

        var dtos = fotosSalvas.Select(MapToDto).ToList();
        return Result<List<FotoDto>>.Ok(dtos);
    }

    public async Task<Result> RemoverFotoAsync(Guid fotoId)
    {
        var foto = await _fotoRepo.GetByIdAsync(fotoId);
        if (foto is null)
            return Result.Fail("Foto não encontrada.");

        await _storage.RemoverAsync(foto.Url);
        await _fotoRepo.DeleteAsync(foto);
        await _fotoRepo.SaveChangesAsync();

        await ReordenarAposRemocao(foto.EntidadeTipo, foto.EntidadeId);

        return Result.Ok();
    }

    private async Task ReordenarAposRemocao(string entidadeTipo, Guid entidadeId)
    {
        var fotos = await _fotoRepo.GetByEntidadeAsync(entidadeTipo, entidadeId);
        for (var i = 0; i < fotos.Count; i++)
        {
            if (fotos[i].Ordem != i)
            {
                fotos[i].AtualizarOrdem(i);
                await _fotoRepo.UpdateAsync(fotos[i]);
            }
        }
        await _fotoRepo.SaveChangesAsync();
    }

    public async Task<Result> ReordenarFotosAsync(string entidadeTipo, Guid entidadeId, List<Guid> ordemIds)
    {
        var fotos = await _fotoRepo.GetByEntidadeAsync(entidadeTipo, entidadeId);
        if (fotos.Count != ordemIds.Count)
            return Result.Fail("Número de IDs não corresponde à quantidade de fotos.");

        var ordem = 0;
        foreach (var id in ordemIds)
        {
            var foto = fotos.FirstOrDefault(f => f.Id == id);
            if (foto is null)
                return Result.Fail($"Foto com ID {id} não pertence a esta entidade.");
            foto.AtualizarOrdem(ordem++);
            await _fotoRepo.UpdateAsync(foto);
        }
        await _fotoRepo.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result<List<FotoDto>>> GetFotosByEntidadeAsync(string entidadeTipo, Guid entidadeId)
    {
        var fotos = await _fotoRepo.GetByEntidadeAsync(entidadeTipo, entidadeId);
        return Result<List<FotoDto>>.Ok(fotos.Select(MapToDto).ToList());
    }

    private static FotoDto MapToDto(Foto f) => new()
    {
        Id = f.Id,
        Url = f.Url,
        Ordem = f.Ordem,
        NomeArquivo = f.NomeArquivo,
        DataUpload = f.DataUpload
    };
}
