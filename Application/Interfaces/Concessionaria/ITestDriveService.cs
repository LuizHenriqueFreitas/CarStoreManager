using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.TestDrive;

namespace CarStoreManager.Application.Interfaces;

public interface ITestDriveService
{
    Task<Result<IEnumerable<TestDriveListaDTO>>> GetAllAsync();
    Task<Result<IEnumerable<TestDriveListaDTO>>> ObterPorVeiculoAsync(Guid veiculoVendaId);
    Task<Result<Guid>> AgendarAsync(CriarTestDriveDTO dto);
    Task<Result> AtualizarStatusAsync(AtualizarStatusTestDriveDTO dto);
    Task<Result> ReagendarAsync(Guid id, DateTime novaDataHora);
    Task<Result> RemoverAsync(Guid id);
}
