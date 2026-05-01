using System.Runtime.CompilerServices;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Shared.Cliente;

namespace CarStoreManager.Application.Interfaces;

public interface IClienteService : IService<
    ClienteDTO,
    ClienteListaDTO,
    CriarClienteDTO,
    AtualizarClienteDTO>
{
    Task<Result<ClienteDTO>> ObterPorCpfAsync(string cpf);

    Task<Result<List<ClienteListaDTO>>> PesquisarAsync(string termo);
}