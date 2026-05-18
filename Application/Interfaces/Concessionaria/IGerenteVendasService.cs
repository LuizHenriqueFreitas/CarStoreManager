using CarStoreManager.Application.DTOs.Concessionaria.GerenteVendas;

namespace CarStoreManager.Application.Interfaces;

public interface IGerenteVendasService : IService<
    GerenteVendasDTO,
    GerenteVendasListaDTO,
    CriarGerenteVendasDTO,
    AtualizarGerenteVendasDTO>
{ }
