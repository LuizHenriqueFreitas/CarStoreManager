using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Repositories;

public interface IVeiculoVendaRepository : IRepository<VeiculoVenda>
{
    Task<IEnumerable<VeiculoVenda>> ObterDisponiveisAsync();
    Task<IEnumerable<VeiculoVenda>> ObterPorDisponibilidadeAsync(DisponibilidadeVeiculo disponibilidade);
    Task<VeiculoVenda?> ObterPorPlacaAsync(string placa);
}