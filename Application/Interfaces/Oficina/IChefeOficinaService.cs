using CarStoreManager.Application.DTOs.Oficina.ChefeOficina;

namespace CarStoreManager.Application.Interfaces;

public interface IChefeOficinaService : IService<
    ChefeOficinaDTO,
    ChefeOficinaListaDTO,
    CriarChefeOficinaDTO,
    AtualizarChefeOficinaDTO>
{ }
