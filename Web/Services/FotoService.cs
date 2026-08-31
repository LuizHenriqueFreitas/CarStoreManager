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
        try
        {
            using var content = new MultipartFormDataContent();

            foreach (var file in files)
            {
                // OpenReadStream lança IOException se o arquivo passar do limite —
                // é o jeito do Blazor de recusar o arquivo, não um erro inesperado.
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

            return Result<List<FotoDto>>.Fail(await MensagemAmigavelAsync(response));
        }
        catch (IOException)
        {
            return Result<List<FotoDto>>.Fail("Um dos arquivos passa de 5MB. Escolha uma foto menor.");
        }
        catch (Exception)
        {
            return Result<List<FotoDto>>.Fail("Não foi possível enviar as fotos. Verifique sua conexão e tente novamente.");
        }
    }

    public async Task<Result> RemoverFotoAsync(Guid fotoId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/fotos/{fotoId}");
            return response.IsSuccessStatusCode
                ? Result.Ok()
                : Result.Fail(await MensagemAmigavelAsync(response));
        }
        catch (Exception)
        {
            return Result.Fail("Não foi possível remover a foto. Verifique sua conexão e tente novamente.");
        }
    }

    public async Task<Result> ReordenarFotosAsync(string entidadeTipo, Guid entidadeId, List<Guid> ordemIds)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/fotos/reordenar/{entidadeTipo}/{entidadeId}", ordemIds);
            return response.IsSuccessStatusCode
                ? Result.Ok()
                : Result.Fail(await MensagemAmigavelAsync(response));
        }
        catch (Exception)
        {
            return Result.Fail("Não foi possível reordenar as fotos. Verifique sua conexão e tente novamente.");
        }
    }

    /// <summary>
    /// O corpo de uma resposta de erro só é seguro de mostrar direto quando a
    /// API respondeu com um texto curto (o Result.Error que os controllers
    /// devolvem via BadRequest/NotFound). Qualquer coisa maior — página de
    /// erro HTML, JSON de framework — não é algo que um usuário leigo deva
    /// ver, então cai numa mensagem genérica.
    /// </summary>
    private static async Task<string> MensagemAmigavelAsync(HttpResponseMessage response)
    {
        var corpo = await response.Content.ReadAsStringAsync();
        if (!string.IsNullOrWhiteSpace(corpo) && corpo.Length < 300 && !corpo.TrimStart().StartsWith("<"))
            return corpo;

        return "Não foi possível concluir a operação. Tente novamente em instantes.";
    }
}