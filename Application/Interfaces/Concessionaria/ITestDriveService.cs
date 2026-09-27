using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.TestDrive;

namespace CarStoreManager.Application.Interfaces;

public interface ITestDriveService
{
    Task<Result<IEnumerable<TestDriveListaDTO>>> GetAllAsync();
    Task<Result<TestDriveListaDTO>> GetByIdAsync(Guid id);
    Task<Result<IEnumerable<TestDriveListaDTO>>> ObterPorVeiculoAsync(Guid veiculoVendaId);
    Task<Result<Guid>> AgendarAsync(CriarTestDriveDTO dto);
    Task<Result> AtualizarStatusAsync(AtualizarStatusTestDriveDTO dto);
    Task<Result> ReagendarAsync(Guid id, DateTime novaDataHora);
    Task<Result> RemoverAsync(Guid id);

    /// <summary>Reatribui o vendedor responsável (Admin/GerenteVendas) —
    /// útil quando o vendedor original se ausenta da operação. Sem
    /// restrição de status no domínio; a UI decide quando mostrar a ação.</summary>
    Task<Result> TrocarVendedorAsync(Guid id, Guid novoVendedorId);

    // ===== Termo de responsabilidade (assinatura eletrônica) =====
    Task<Result<TermoTestDriveDTO>> ObterTermoAsync(Guid testDriveId);
    Task<Result<TermoTestDriveDTO>> EditarTermoAsync(Guid testDriveId, EditarTermoTestDriveDTO dto);
    Task<Result> EnviarTermoParaAssinaturaAsync(Guid testDriveId);
    Task<Result<TermoTestDriveDTO>> ObterTermoPorTokenAsync(string token);
    Task<Result> AssinarTermoAsync(string token, AssinarTermoTestDriveDTO dto, string ipOrigem);
}
