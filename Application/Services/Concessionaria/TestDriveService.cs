using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.TestDrive;
using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Concessionaria;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

public class TestDriveService : ITestDriveService
{
    private readonly ITestDriveRepository _repo;
    private readonly IVeiculoVendaRepository _veiculos;
    private readonly IVeiculoConsignacaoRepository _consignacoes;
    private readonly IClienteRepository _clientes;
    private readonly IVendedorRepository _vendedores;
    private readonly ITermoTestDriveRepository _termoRepo;
    private readonly IBalancoMensalDespesaService _balancoDespesaService;

    // Valor fixo do combustível bancado pela concessionária a cada test
    // drive concluído — pedido do dono: aumenta a chance de venda, então é
    // uma despesa da operação, não repassada ao cliente.
    private const decimal ValorCombustivelTestDrive = 50m;

    public TestDriveService(
        ITestDriveRepository repo,
        IVeiculoVendaRepository veiculos,
        IVeiculoConsignacaoRepository consignacoes,
        IClienteRepository clientes,
        IVendedorRepository vendedores,
        ITermoTestDriveRepository termoRepo,
        IBalancoMensalDespesaService balancoDespesaService)
    {
        _repo = repo;
        _veiculos = veiculos;
        _consignacoes = consignacoes;
        _clientes = clientes;
        _vendedores = vendedores;
        _termoRepo = termoRepo;
        _balancoDespesaService = balancoDespesaService;
    }

    public async Task<Result<IEnumerable<TestDriveListaDTO>>> GetAllAsync()
    {
        var lista = (await _repo.GetAllAsync()).ToList();
        return Result<IEnumerable<TestDriveListaDTO>>.Ok(await MapearAsync(lista));
    }

    public async Task<Result<TestDriveListaDTO>> GetByIdAsync(Guid id)
    {
        var td = await _repo.GetByIdAsync(id);
        if (td is null) return Result<TestDriveListaDTO>.Fail("Test drive não encontrado.");
        var mapeados = await MapearAsync(new List<TestDrive> { td });
        return Result<TestDriveListaDTO>.Ok(mapeados[0]);
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
            var ehConsignado = dto.VeiculoEntidadeTipo == "VeiculoConsignacao";
            if (ehConsignado)
            {
                var consignado = await _consignacoes.GetByIdAsync(dto.VeiculoVendaId);
                if (consignado is null) return Result<Guid>.Fail("Veículo consignado não encontrado.");

                if (consignado.TentarExpirar())
                {
                    _consignacoes.Update(consignado);
                    await _consignacoes.SaveChangesAsync();
                }
                if (consignado.Status != StatusConsignacao.Ativa)
                    return Result<Guid>.Fail("Só é possível agendar test drive de uma consignação ativa.");
            }
            else
            {
                var veiculo = await _veiculos.GetByIdAsync(dto.VeiculoVendaId);
                if (veiculo is null) return Result<Guid>.Fail("Veículo não encontrado.");
                if (veiculo.Disponibilidade == DisponibilidadeVeiculo.Vendido)
                    return Result<Guid>.Fail("Não é possível agendar test drive de um veículo já vendido.");
            }

            if (await _clientes.GetByIdAsync(dto.ClienteId) is null)
                return Result<Guid>.Fail("Cliente não encontrado.");
            if (await _vendedores.GetByIdAsync(dto.VendedorId) is null)
                return Result<Guid>.Fail("Vendedor não encontrado.");

            if (string.IsNullOrWhiteSpace(dto.TextoTermo))
                return Result<Guid>.Fail("O texto do termo de responsabilidade é obrigatório.");

            var td = new TestDrive(dto.VeiculoVendaId, dto.ClienteId, dto.VendedorId, dto.DataHora, dto.Observacao, dto.VeiculoEntidadeTipo);
            await _repo.AddAsync(td);
            await _repo.SaveChangesAsync();

            var termo = new TermoTestDrive(td.Id, dto.VendedorId, dto.TextoTermo);
            await _termoRepo.AddAsync(termo);
            await _termoRepo.SaveChangesAsync();

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

        // Combustível do test drive fica por conta da concessionária — vira
        // despesa automática só quando o passeio realmente acontece (nunca
        // em agendamento cancelado/não compareceu). Best-effort: se falhar
        // (ex.: balanço do mês já fechado), o status já foi salvo mesmo assim.
        if (status == StatusTestDrive.Realizado)
            await RegistrarDespesaCombustivelAsync(td);

        return Result.Ok();
    }

    private async Task RegistrarDespesaCombustivelAsync(TestDrive td)
    {
        try
        {
            var veiculoDescricao = await ObterDescricaoVeiculoAsync(td);
            var cliente = await _clientes.GetByIdAsync(td.ClienteId);
            var clienteNome = cliente?.GetNome() ?? "cliente";

            var hoje = DateTime.Today;
            await _balancoDespesaService.GerarDoModeloAsync(hoje.Year, hoje.Month);
            await _balancoDespesaService.SalvarItemAsync(new SalvarItemBalancoDTO
            {
                Ano = hoje.Year,
                Mes = hoje.Month,
                Nome = $"Combustível test drive: {veiculoDescricao} — cliente {clienteNome}",
                Setor = "Concessionaria",
                Categoria = "Combustível - Test Drive",
                Valor = ValorCombustivelTestDrive
            });
        }
        catch { /* best-effort — não desfaz a conclusão do test drive por causa disso */ }
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

    public async Task<Result> TrocarVendedorAsync(Guid id, Guid novoVendedorId)
    {
        var td = await _repo.GetByIdAsync(id);
        if (td is null) return Result.Fail("Test drive não encontrado.");

        if (await _vendedores.GetByIdAsync(novoVendedorId) is null)
            return Result.Fail("Vendedor não encontrado.");

        try
        {
            td.TrocarVendedor(novoVendedorId);
            _repo.Update(td);
            await _repo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    private async Task<string> ObterDescricaoVeiculoAsync(TestDrive td)
    {
        if (td.IsConsignado)
        {
            var c = await _consignacoes.GetByIdAsync(td.VeiculoVendaId);
            return c is null ? "veículo" : $"{c.GetMarca()} {c.GetModelo()}";
        }

        var v = await _veiculos.GetByIdAsync(td.VeiculoVendaId);
        return v is null ? "veículo" : $"{v.GetMarca()} {v.GetModelo()}";
    }

    // ============================================================
    // TERMO DE RESPONSABILIDADE
    // ============================================================

    public async Task<Result<TermoTestDriveDTO>> ObterTermoAsync(Guid testDriveId)
    {
        var termo = await _termoRepo.ObterPorTestDriveAsync(testDriveId);
        if (termo is null) return Result<TermoTestDriveDTO>.Fail("Termo não encontrado para este test drive.");
        return Result<TermoTestDriveDTO>.Ok(MapTermo(termo));
    }

    public async Task<Result<TermoTestDriveDTO>> EditarTermoAsync(Guid testDriveId, EditarTermoTestDriveDTO dto)
    {
        var termo = await _termoRepo.ObterPorTestDriveAsync(testDriveId);
        if (termo is null) return Result<TermoTestDriveDTO>.Fail("Termo não encontrado para este test drive.");

        try
        {
            termo.EditarTexto(dto.TextoTermo);
            _termoRepo.Update(termo);
            await _termoRepo.SaveChangesAsync();
            return Result<TermoTestDriveDTO>.Ok(MapTermo(termo));
        }
        catch (Exception ex) { return Result<TermoTestDriveDTO>.Fail(ex.Message); }
    }

    public async Task<Result> EnviarTermoParaAssinaturaAsync(Guid testDriveId)
    {
        var termo = await _termoRepo.ObterPorTestDriveAsync(testDriveId);
        if (termo is null) return Result.Fail("Crie o termo antes de enviá-lo para assinatura.");

        try
        {
            termo.EnviarParaAssinatura();
            _termoRepo.Update(termo);
            await _termoRepo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result<TermoTestDriveDTO>> ObterTermoPorTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Result<TermoTestDriveDTO>.Fail("Token inválido.");

        var termo = await _termoRepo.ObterPorTokenAsync(token);
        if (termo is null)
            return Result<TermoTestDriveDTO>.Fail("Termo não encontrado ou link inválido.");

        return Result<TermoTestDriveDTO>.Ok(MapTermo(termo));
    }

    public async Task<Result> AssinarTermoAsync(string token, AssinarTermoTestDriveDTO dto, string ipOrigem)
    {
        if (!dto.Aceite)
            return Result.Fail("É necessário marcar o aceite explícito do termo.");

        var termo = await _termoRepo.ObterPorTokenAsync(token);
        if (termo is null) return Result.Fail("Termo não encontrado ou link inválido.");

        try
        {
            termo.Assinar(dto.NomeCliente, dto.CpfCliente, ipOrigem);
            _termoRepo.Update(termo);
            await _termoRepo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    private static TermoTestDriveDTO MapTermo(TermoTestDrive termo) => new()
    {
        Id = termo.Id,
        TestDriveId = termo.TestDriveId,
        TextoTermo = termo.TextoTermo,
        Status = termo.Status.ToString(),
        DataRedacao = termo.DataRedacao,
        DataUltimaEdicao = termo.DataUltimaEdicao,
        TokenAssinatura = termo.TokenAssinatura,
        DataAssinatura = termo.DataAssinatura,
        AssinaturaNomeCliente = termo.AssinaturaNomeCliente,
        AssinaturaCpfCliente = termo.AssinaturaCpfCliente,
        AssinaturaIp = termo.AssinaturaIp,
    };

    private async Task<List<TestDriveListaDTO>> MapearAsync(List<TestDrive> lista)
    {
        var veiculoVendaIds = lista.Where(t => !t.IsConsignado).Select(t => t.VeiculoVendaId).Distinct().ToList();
        var consignadoIds = lista.Where(t => t.IsConsignado).Select(t => t.VeiculoVendaId).Distinct().ToList();
        var clienteIds = lista.Select(t => t.ClienteId).Distinct().ToList();
        var vendedorIds = lista.Select(t => t.VendedorId).Distinct().ToList();

        var veiculos = new Dictionary<Guid, string>();
        foreach (var vid in veiculoVendaIds)
        {
            var v = await _veiculos.GetByIdAsync(vid);
            if (v is not null) veiculos[vid] = $"{v.GetMarca()} {v.GetModelo()} {v.GetAno()}";
        }
        foreach (var vid in consignadoIds)
        {
            var v = await _consignacoes.GetByIdAsync(vid);
            if (v is not null) veiculos[vid] = $"{v.GetMarca()} {v.GetModelo()} {v.GetAno()} (consignado)";
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
            VeiculoEntidadeTipo = t.VeiculoEntidadeTipo,
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
