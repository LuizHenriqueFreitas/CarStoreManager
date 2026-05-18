using CarStoreManager.Application.DTOs.Recepcionista;

namespace CarStoreManager.Application.Interfaces;

public interface IRecepcionistaService : IService<
    RecepcionistaDTO,
    RecepcionistaListaDTO,
    CriarRecepcionistaDTO,
    AtualizarRecepcionistaDTO>
{ }
