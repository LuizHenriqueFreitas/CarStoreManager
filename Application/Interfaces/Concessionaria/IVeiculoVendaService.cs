using System.IO.Compression;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.VeiculoVenda;

namespace CarStoreManager.Application.Interfaces;

public interface IVeiculoVendaService : IService<
    VeiculoVendaDTO, 
    VeiculoVendaListaDTO,
    CriarVeiculoVendaDTO,
    AtualizarVeiculoVendaDTO>
{
    Task<Result<IEnumerable<VeiculoVendaListaDTO>>> ObterDisponiveisAsync();
    Task<Result> MarcarComoVendidoAsync(Guid id);
    Task<Result> MarcarComoDisponivelAsync(Guid id);
    Task<Result> LiberarParaVendaAsync(Guid id);
    Task<Result> VoltarParaPreparacaoAsync(Guid id);
    Task<Result> AtualizarQuilometragemAsync(Guid id, int km);
    Task<Result> AdicionarFotoAsync(Guid id, string url);
    Task<Result> ReordenarFotosAsync(Guid id, List<Guid> fotos);
    Task<Result> RemoverFotoAsync(Guid id, Guid fotoId);

    /// <summary>Lista marcas distintas (já normalizadas) para alimentar dropdowns de filtro.</summary>
    Task<Result<IEnumerable<string>>> ListarMarcasDistintasAsync();
}