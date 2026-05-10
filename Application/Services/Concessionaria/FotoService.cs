// Application/Services/FotoService.cs
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Interfaces.Repositories;
using CarStoreManager.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace CarStoreManager.Application.Services;

public class FotoService : IFotoService
{
    private readonly IFotoRepository _fotoRepo;
    private readonly IWebHostEnvironmen _env;
    private const string UPLOADS_FOLDER = "uploads";

    public FotoService(IFotoRepository fotoRepo, IWebHostEnvironmen env)
    {
        _fotoRepo = fotoRepo;
        _env = env;
    }

    public async Task<Result<List<FotoDto>>> UploadFotosAsync(string entidadeTipo, Guid entidadeId, IList<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return Result<List<FotoDto>>.Failure("Nenhum arquivo enviado.");

        var uploadsPath = Path.Combine(_env.WebRootPath, UPLOADS_FOLDER, entidadeTipo.ToLower());
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        var fotosSalvas = new List<Foto>();
        var ordemAtual = await _fotoRepo.GetNextOrdemAsync(entidadeTipo, entidadeId);

        foreach (var file in files)
        {
            // Validações
            if (file.Length > 5 * 1024 * 1024) // 5 MB
                return Result<List<FotoDto>>.Failure($"Arquivo {file.FileName} excede 5MB.");
            
            var contentType = file.ContentType.ToLower();
            if (contentType != "image/jpeg" && contentType != "image/png")
                return Result<List<FotoDto>>.Failure($"Formato não suportado: {file.FileName}. Use JPEG ou PNG.");

            // Gerar nome único
            var extensao = Path.GetExtension(file.FileName);
            var nomeUnico = $"{Guid.NewGuid()}{extensao}";
            var caminhoCompleto = Path.Combine(uploadsPath, nomeUnico);
            
            using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/{UPLOADS_FOLDER}/{entidadeTipo.ToLower()}/{nomeUnico}";
            var foto = new Foto(
                entidadeTipo, 
                entidadeId, 
                url, 
                file.FileName, 
                file.Length, 
                contentType, 
                ordemAtual++
            );
            await _fotoRepo.AddAsync(foto);
            fotosSalvas.Add(foto);
        }

        await _fotoRepo.SaveChangesAsync();

        var dtos = fotosSalvas.Select(f => new FotoDto
        {
            Id = f.Id,
            Url = f.Url,
            Ordem = f.Ordem,
            NomeArquivo = f.NomeArquivo,
            DataUpload = f.DataUpload
        }).ToList();

        return Result<List<FotoDto>>.Success(dtos);
    }

    public async Task<Result> RemoverFotoAsync(Guid fotoId)
    {
        var foto = await _fotoRepo.GetByIdAsync(fotoId);
        if (foto == null)
            return Result.Failure("Foto não encontrada.");

        // Remover arquivo físico
        var caminhoRelativo = foto.Url.TrimStart('/');
        var caminhoAbsoluto = Path.Combine(_env.WebRootPath, caminhoRelativo.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(caminhoAbsoluto))
            File.Delete(caminhoAbsoluto);

        await _fotoRepo.DeleteAsync(foto);
        await _fotoRepo.SaveChangesAsync();

        // Reordenar as fotos restantes (opcional)
        await ReordenarAposRemocao(foto.EntidadeTipo, foto.EntidadeId);

        return Result.Success();
    }

    private async Task ReordenarAposRemocao(string entidadeTipo, Guid entidadeId)
    {
        var fotos = await _fotoRepo.GetByEntidadeAsync(entidadeTipo, entidadeId);
        for (int i = 0; i < fotos.Count; i++)
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
            return Result.Failure("Número de IDs não corresponde à quantidade de fotos.");

        var ordem = 0;
        foreach (var id in ordemIds)
        {
            var foto = fotos.FirstOrDefault(f => f.Id == id);
            if (foto == null)
                return Result.Failure($"Foto com ID {id} não pertence a esta entidade.");
            foto.AtualizarOrdem(ordem++);
            await _fotoRepo.UpdateAsync(foto);
        }
        await _fotoRepo.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<List<FotoDto>>> GetFotosByEntidadeAsync(string entidadeTipo, Guid entidadeId)
    {
        var fotos = await _fotoRepo.GetByEntidadeAsync(entidadeTipo, entidadeId);
        var dtos = fotos.Select(f => new FotoDto
        {
            Id = f.Id,
            Url = f.Url,
            Ordem = f.Ordem,
            NomeArquivo = f.NomeArquivo,
            DataUpload = f.DataUpload
        }).ToList();
        return Result<List<FotoDto>>.Success(dtos);
    }
}