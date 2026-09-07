using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.TestDrive;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

public class TestDriveService : ITestDriveService
{
    private readonly ITestDriveRepository _repo;
    private readonly IVeiculoVendaRepository _veiculos;
    private readonly IClienteRepository _clientes;
    private readonly IVendedorRepository _vendedores;

    public TestDriveService(
        ITestDriveRepository repo,
        IVeiculoVendaRepository veiculos,
        IClienteRepository clientes,
        IVendedorRepository vendedores)
    {
        _repo = repo;
        _veiculos = veiculos;
        _clientes = clientes;
        _vendedores = vendedores;
    }

    public async Task<Result<IEnumerable<TestDriveListaDTO>>> GetAllAsync()
    {
        var lista = (await _repo.GetAllAsync()).ToList();
        return Result<IEnumerable<TestDriveListaDTO>>.Ok(await MapearAsync(lista));
    }

    public async Task<Result<IEnumerable<TestDriveListaDTO>>> ObterPorVeiculoAsync(Guid veiculoVendaId)
    {
        var lista = (await _repo.ObterPorVeiculoAsync(veiculoVendaId)).ToList();
        return Result<IEnumerable<TestDriveListaDTO>>.Ok(await MapearAsync(lista));
    }

    public async Task<Result<Guid>> AgendarAsync(CriarTestDriveDTO dto)
    {
        try
        {
            var veiculo = await _veiculos.GetByIdAsync(dto.VeiculoVendaId);
            if (veiculo is null) return Result<Guid>.Fail("Veículo não encontrado.");
            if (veiculo.Disponibilidade == DisponibilidadeVeiculo.Vendido)
                return Result<Guid>.Fail("Não é possível agendar test drive de um veículo já vendido.");

            if (await _clientes.GetByIdAsync(dto.ClienteId) is null)
                return Result<Guid>.Fail("Cliente não encontrado.");
            if (await _vendedores.GetByIdAsync(dto.VendedorId) is null)
                return Result<Guid>.Fail("Vendedor não encontrado.");

            var td = new TestDrive(dto.VeiculoVendaId, dto.ClienteId, dto.VendedorId, dto.DataHora, dto.Observacao);
            await _repo.AddAsync(td);
            await _repo.SaveChangesAsync();
            return Result<Guid>.Ok(td.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail(ex.Message);
        }
    }

    public async Task<Result> AtualizarStatusAsync(AtualizarStatusTestDriveDTO dto)
    {
        var td = await _repo.GetByIdAsync(dto.Id);
        if (td is null) return Result.Fail("Test drive não encontrado.");

        if (!Enum.TryParse<StatusTestDrive>(dto.Status, true, out var status))
            return Result.Fail($"Status inválido: {dto.Status}");

        switch (status)
        {
            case StatusTestDrive.Realizado: td.MarcarRealizado(); break;
            case StatusTestDrive.Cancelado: td.Cancelar(); break;
            case StatusTestDrive.NaoCompareceu: td.MarcarNaoCompareceu(); break;
            case StatusTestDrive.Agendado: break;
        }

        _repo.Update(td);
        await _repo.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> ReagendarAsync(Guid id, DateTime novaDataHora)
    {
        var td = await _repo.GetByIdAsync(id);
        if (td is null) return Result.Fail("Test drive não encontrado.");
        try
        {
            td.Reagendar(novaDataHora);
            _repo.Update(td);
            await _repo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> RemoverAsync(Guid id)
    {
        var td = await _repo.GetByIdAsync(id);
        if (td is null) return Result.Fail("Test drive não encontrado.");
        _repo.Remove(td);
        await _repo.SaveChangesAsync();
        return Result.Ok();
    }

    private async Task<List<TestDriveListaDTO>> MapearAsync(List<TestDrive> lista)
    {
        var veiculoIds = lista.Select(t => t.VeiculoVendaId).Distinct().ToList();
        var clienteIds = lista.Select(t => t.ClienteId).Distinct().ToList();
        var vendedorIds = lista.Select(t => t.VendedorId).Distinct().ToList();

        var veiculos = new Dictionary<Guid, string>();
        foreach (var vid in veiculoIds)
        {
            var v = await _veiculos.GetByIdAsync(vid);
            if (v is not null) veiculos[vid] = $"{v.GetMarca()} {v.GetModelo()} {v.GetAno()}";
        }

        var clientes = new Dictionary<Guid, string>();
        foreach (var cid in clienteIds)
        {
            var c = await _clientes.GetByIdAsync(cid);
            if (c is not null) clientes[cid] = c.GetNome();
        }

        var vendedores = new Dictionary<Guid, string>();
        foreach (var vd in vendedorIds)
        {
            var v = await _vendedores.GetByIdAsync(vd);
            if (v is not null) vendedores[vd] = v.GetNome();
        }

        return lista.Select(t => new TestDriveListaDTO
        {
            Id = t.Id,
            VeiculoVendaId = t.VeiculoVendaId,
            ClienteId = t.ClienteId,
            VendedorId = t.VendedorId,
            DataHora = t.DataHora,
            Status = t.Status.ToString(),
            Observacao = t.Observacao,
            VeiculoDescricao = veiculos.GetValueOrDefault(t.VeiculoVendaId, "—"),
            ClienteNome = clientes.GetValueOrDefault(t.ClienteId, "—"),
            VendedorNome = vendedores.GetValueOrDefault(t.VendedorId, "—"),
        }).ToList();
    }
}
