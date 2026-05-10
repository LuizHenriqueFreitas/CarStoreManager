// Application/Interfaces/Repositories/IFotoRepository.cs
using CarStoreManager.Domain.Entities;

namespace CarStoreManager.Application.Interfaces.Repositories;

public interface IFotoRepository
{
    Task<Foto?> GetByIdAsync(Guid id);
    Task<List<Foto>> GetByEntidadeAsync(string entidadeTipo, Guid entidadeId);
    Task AddAsync(Foto foto);
    Task UpdateAsync(Foto foto);
    Task DeleteAsync(Foto foto);
    Task<int> GetNextOrdemAsync(string entidadeTipo, Guid entidadeId);
    Task SaveChangesAsync();
}