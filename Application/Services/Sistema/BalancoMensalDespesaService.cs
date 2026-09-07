using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;

namespace CarStoreManager.Application.Services.Sistema;

public class BalancoMensalDespesaService : IBalancoMensalDespesaService
{
    private readonly IBalancoMensalDespesaRepository _repo;
    private readonly IDespesaRepository _modelo;
    private readonly IConfiguracaoSistemaRepository _config;

    public BalancoMensalDespesaService(
        IBalancoMensalDespesaRepository repo,
        IDespesaRepository modelo,
        IConfiguracaoSistemaRepository config)
    {
        _repo = repo;
        _modelo = modelo;
        _config = config;
    }

    private static DateOnly Comp(int ano, int mes) => new(ano, mes, 1);

    public async Task<Result<BalancoMensalDespesaDTO>> ObterAsync(int ano, int mes)
    {
        var b = await _repo.ObterPorCompetenciaAsync(Comp(ano, mes));
        if (b is not null) return Result<BalancoMensalDespesaDTO>.Ok(ToDto(b, existe: true));

        // Ainda não existe — devolve um "vazio" com as linhas do modelo como sugestão.
        var vazio = new BalancoMensalDespesaDTO { Ano = ano, Mes = mes, ExisteNoBanco = false };
        return Result<BalancoMensalDespesaDTO>.Ok(vazio);
    }

    public async Task<Result<IEnumerable<BalancoMensalDespesaDTO>>> HistoricoAsync()
    {
        var lista = (await _repo.ListarAsync())
            .Select(b => ToDto(b, existe: true))
            .ToList();
        return Result<IEnumerable<BalancoMensalDespesaDTO>>.Ok(lista);
    }

    public async Task<Result<BalancoMensalDespesaDTO>> GerarDoModeloAsync(int ano, int mes)
    {
        var existente = await _repo.ObterPorCompetenciaAsync(Comp(ano, mes));
        if (existente is not null)
            return Result<BalancoMensalDespesaDTO>.Fail("Já existe um balanço para este mês.");

        var balanco = new BalancoMensalDespesa(Comp(ano, mes));
        foreach (var m in (await _modelo.GetAtivasAsync()))
            balanco.AdicionarItem(m.Nome, m.Setor, m.Categoria, m.GetValor(), doModelo: true);

        await _repo.AddAsync(balanco);
        await _repo.SaveChangesAsync();
        return Result<BalancoMensalDespesaDTO>.Ok(ToDto(balanco, existe: true));
    }

    public async Task<Result> SalvarItemAsync(SalvarItemBalancoDTO dto)
    {
        var balanco = await _repo.ObterPorCompetenciaAsync(Comp(dto.Ano, dto.Mes));
        var novo = balanco is null;
        balanco ??= new BalancoMensalDespesa(Comp(dto.Ano, dto.Mes));

        if (balanco.Fechado)
            return Result.Fail("Balanço fechado — reabra para editar.");

        var setor = Enum.TryParse<SetorDespesa>(dto.Setor, true, out var s) ? s : SetorDespesa.Geral;

        try
        {
            if (dto.ItemId is { } id && balanco.Itens.FirstOrDefault(i => i.Id == id) is { } item)
                item.Atualizar(dto.Nome, setor, dto.Categoria, dto.Valor);
            else
                balanco.AdicionarItem(dto.Nome, setor, dto.Categoria, dto.Valor);
        }
        catch (ArgumentException ex) { return Result.Fail(ex.Message); }

        if (novo) await _repo.AddAsync(balanco); else _repo.Update(balanco);
        await _repo.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> RemoverItemAsync(int ano, int mes, Guid itemId)
    {
        var balanco = await _repo.ObterPorCompetenciaAsync(Comp(ano, mes));
        if (balanco is null) return Result.Fail("Balanço não encontrado.");
        if (balanco.Fechado) return Result.Fail("Balanço fechado — reabra para editar.");
        balanco.RemoverItem(itemId);
        _repo.Update(balanco);
        await _repo.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> FecharAsync(int ano, int mes)
    {
        var balanco = await _repo.ObterPorCompetenciaAsync(Comp(ano, mes));
        if (balanco is null) return Result.Fail("Não há balanço para fechar.");
        balanco.Fechar();
        _repo.Update(balanco);
        await _repo.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> ReabrirAsync(int ano, int mes)
    {
        var balanco = await _repo.ObterPorCompetenciaAsync(Comp(ano, mes));
        if (balanco is null) return Result.Fail("Balanço não encontrado.");
        balanco.Reabrir();
        _repo.Update(balanco);
        await _repo.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result<decimal>> TotalDoMesAsync(int ano, int mes)
    {
        var balanco = await _repo.ObterPorCompetenciaAsync(Comp(ano, mes));
        if (balanco is not null)
            return Result<decimal>.Ok(balanco.Total());

        // Sem balanço → usa a soma do formulário-modelo ativo como estimativa.
        var soma = (await _modelo.GetAtivasAsync()).Sum(m => m.GetValor());
        return Result<decimal>.Ok(soma);
    }

    public async Task<int?> DiasParaFechamentoAsync()
    {
        var cfg = await _config.ObterAsync();
        var dia = cfg.DiaFechamentoDespesas;
        var hoje = DateTime.Today;

        var proximo = new DateTime(hoje.Year, hoje.Month, Math.Min(dia, DateTime.DaysInMonth(hoje.Year, hoje.Month)));
        if (proximo < hoje)
            proximo = proximo.AddMonths(1);

        var dias = (proximo.Date - hoje.Date).Days;
        return dias <= 5 ? dias : null;
    }

    // ---- mapeamento ----
    private static BalancoMensalDespesaDTO ToDto(BalancoMensalDespesa b, bool existe) => new()
    {
        Id = b.Id,
        Ano = b.Competencia.Year,
        Mes = b.Competencia.Month,
        Fechado = b.Fechado,
        DataFechamento = b.DataFechamento,
        ExisteNoBanco = existe,
        Total = b.Total(),
        TotalGeral = b.TotalPorSetor(SetorDespesa.Geral),
        TotalOficina = b.TotalPorSetor(SetorDespesa.Oficina),
        TotalConcessionaria = b.TotalPorSetor(SetorDespesa.Concessionaria),
        Itens = b.Itens.Select(i => new ItemBalancoDespesaDTO
        {
            Id = i.Id, Nome = i.Nome, Setor = i.Setor.ToString(),
            Categoria = i.Categoria, Valor = i.Valor.GetValorDinheiro(), DoModelo = i.DoModelo
        }).OrderBy(i => i.Setor).ThenBy(i => i.Nome).ToList()
    };
}
