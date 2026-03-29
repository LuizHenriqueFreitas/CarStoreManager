using CarStoreManager.Application.DTOs.Concessionaria.Vendedor;

namespace CarStoreManager.Application.Interfaces;

public interface IVendedorService : IService<
    VendedorDTO,
    VendedorListaDTO,
    CriarVendedorDTO,
    AtualizarVendedorDTO>
{}