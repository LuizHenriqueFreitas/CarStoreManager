// Web/Services/FotoFrontService.cs
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs;
using Microsoft.AspNetCore.Components.Forms;

public class FotoFrontService
{
    private readonly HttpClient _httpClient;

    public FotoFrontService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<List<FotoDto>>> UploadFotosAsync(Guid entidadeId, string entidadeTipo, IEnumerable<IBrowserFile> files)
    {
        using var content = new MultipartFormDataContent();

        foreach (var file in files)
        {
            var stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "files", file.Name);
        }

        var response = await _httpClient.PostAsync($"api/fotos/upload/{entidadeTipo}/{entidadeId}", content);
        if (response.IsSuccessStatusCode)
        {
            var fotos = await response.Content.ReadFromJsonAsync<List<FotoDto>>();
            return Result<List<FotoDto>>.Ok(fotos);
        }
        var error = await response.Content.ReadAsStringAsync();
        return Result<List<FotoDto>>.Fail(error);
    }

    public async Task<Result> RemoverFotoAsync(Guid fotoId)
    {
        var response = await _httpClient.DeleteAsync($"api/fotos/{fotoId}");
        return response.IsSuccessStatusCode ? Result.Ok() : Result.Fail("Erro ao remover foto");
    }

    public async Task<Result> ReordenarFotosAsync(string entidadeTipo, Guid entidadeId, List<Guid> ordemIds)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/fotos/reordenar/{entidadeTipo}/{entidadeId}", ordemIds);
        return response.IsSuccessStatusCode ? Result.Ok() : Result.Fail("Erro ao reordenar");
    }
}