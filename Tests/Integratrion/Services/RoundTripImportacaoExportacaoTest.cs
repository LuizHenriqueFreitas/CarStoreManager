using System.Text.Json;
using CarStoreManager.Application.DTOs.Sistema.Importacao;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Infrastructure.Data;
using CarStoreManager.Infrastructure.DependencyInjection;
using CarStoreManager.Web.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CarStoreManager.Tests.Integration.Services;

/// <summary>
/// Round-trip do arquivo de dados (contrato v2 — PLANO_BASE_DEMO §8.2):
/// importa o JSON de exemplo num banco SQLite novo → exporta → importa o
/// exportado em OUTRO banco novo → exporta de novo. Os dois bancos têm de
/// bater em: contagem por status (OS, proposta, consignação, test drive,
/// alerta, requisição, termos, veículos), receita de OS por mês
/// (DataCriacao), vendas por mês (DataAprovacao), despesa por competência
/// (com fechamento), pagamentos por mês, checklist, itens, estoque. Usa o
/// mesmo grafo de DI do Web (AddApplicationServices + AddInfrastructure) e
/// o mesmo seed do 1º run (SeedInicial) — nada mockado além do ambiente web.
/// </summary>
public class RoundTripImportacaoExportacaoTests : IDisposable
{
    private static readonly JsonSerializerOptions OpcoesJson = new() { PropertyNameCaseInsensitive = true };
    private readonly string _pasta = Path.Combine(Path.GetTempPath(), "carstore-roundtrip-" + Guid.NewGuid().ToString("N"));

    public RoundTripImportacaoExportacaoTests() => Directory.CreateDirectory(_pasta);

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(_pasta, recursive: true); } catch { /* temp */ }
    }

    [Fact]
    public async Task ImportarExportarImportar_ContratoV2_BancosFicamEquivalentes()
    {
        var json = await File.ReadAllBytesAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", "contrato_v2_exemplo.json"));

        var banco1 = Path.Combine(_pasta, "banco1.db");
        var (avisos1, exportado1) = await ImportarEExportarAsync(banco1, json);
        avisos1.Should().BeEmpty("o JSON de exemplo exercita todos os cenários do contrato v2 sem erro");

        var banco2 = Path.Combine(_pasta, "banco2.db");
        var (avisos2, exportado2) = await ImportarEExportarAsync(banco2, exportado1);
        avisos2.Should().BeEmpty("o arquivo exportado tem de reimportar limpo");

        var m1 = Metricas(banco1);
        var m2 = Metricas(banco2);
        foreach (var (chave, valor) in m1)
            m2[chave].Should().BeEquivalentTo(valor, options => options.WithStrictOrdering(), $"a métrica \"{chave}\" tem de sobreviver ao round-trip");

        // Sanidade: o exemplo cobre de fato os status que o round-trip compara.
        m1["status.OrdensServico"].Should().HaveCountGreaterThan(8);
        m1["despesa_competencia"].Should().Contain(l => l.Contains("|1|"), "há mês fechado no exemplo");

        // Exportar de novo (2º banco) dá o mesmo roteiro (mesmos cenários).
        Cenarios(exportado2).Should().BeEquivalentTo(Cenarios(exportado1));
    }

    private async Task<(List<string> Avisos, byte[] Exportado)> ImportarEExportarAsync(string caminhoBanco, byte[] json)
    {
        await using var provider = Construir(caminhoBanco);

        using (var scope = provider.CreateScope())
            await SeedInicial.AplicarAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());

        var dados = JsonSerializer.Deserialize<ImportacaoDadosDTO>(json, OpcoesJson)!;
        List<string> avisos;
        using (var scope = provider.CreateScope())
        {
            var r = await scope.ServiceProvider.GetRequiredService<IImportacaoDadosService>().ImportarAsync(dados);
            r.IsSuccess.Should().BeTrue(r.Error);
            avisos = r.Value!.Avisos;
        }

        using (var scope = provider.CreateScope())
        {
            var r = await scope.ServiceProvider.GetRequiredService<IExportacaoDadosService>().ExportarJsonAsync();
            r.IsSuccess.Should().BeTrue(r.Error);
            return (avisos, r.Value!.Conteudo);
        }
    }

    private ServiceProvider Construir(string caminhoBanco)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={caminhoBanco}",
            })
            .Build();

        var ambiente = new Mock<IWebHostEnvironment>();
        ambiente.SetupGet(a => a.WebRootPath).Returns(_pasta);
        ambiente.SetupGet(a => a.ContentRootPath).Returns(_pasta);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationServices();
        services.AddInfrastructure(config);
        services.AddSingleton(ambiente.Object);
        return services.BuildServiceProvider();
    }

    private static Dictionary<string, List<string>> Metricas(string caminhoBanco)
    {
        var consultas = new Dictionary<string, string>
        {
            ["status.OrdensServico"] = "select Status, count(*) from OrdensServico group by 1 order by 1",
            ["status.PropostasVenda"] = "select Status, count(*) from PropostasVenda group by 1 order by 1",
            ["status.VeiculosConsignacao"] = "select Status, count(*) from VeiculosConsignacao group by 1 order by 1",
            ["status.TestDrives"] = "select Status, VeiculoEntidadeTipo, count(*) from TestDrives group by 1, 2 order by 1, 2",
            ["status.AlertasOS"] = "select Status, count(*) from AlertasOS group by 1 order by 1",
            ["status.RequisicoesPeca"] = "select Status, count(*) from RequisicoesPeca group by 1 order by 1",
            ["status.TermosEntrega"] = "select Status, count(*) from TermosEntrega group by 1 order by 1",
            ["status.TermosTestDrive"] = "select Status, count(*) from TermosTestDrive group by 1 order by 1",
            ["status.VeiculosVenda"] = "select Disponibilidade, count(*) from VeiculosVenda group by 1 order by 1",
            ["receita_os_mes"] = "select substr(DataCriacao, 1, 7), round(sum(ValorTotal), 2) from OrdensServico group by 1 order by 1",
            ["vendas_mes"] = "select substr(DataAprovacao, 1, 7), round(sum(ValorFinal), 2) from PropostasVenda where Status = 12 group by 1 order by 1",
            ["despesa_competencia"] = "select b.Competencia, b.Fechado, substr(b.DataFechamento, 1, 19), round(sum(i.Valor), 2), count(i.Id) from BalancosMensaisDespesa b left join ItemBalancoDespesa i on i.BalancoId = b.Id group by b.Id order by 1",
            ["pagamentos_os_mes"] = "select substr(DataPagamento, 1, 7), round(sum(Valor), 2), count(*) from PagamentosOrdemServico group by 1 order by 1",
            ["pagamentos_proposta_mes"] = "select substr(DataPagamento, 1, 7), round(sum(Valor), 2), count(*) from PagamentosProposta group by 1 order by 1",
            ["checklist"] = "select Status, count(*) from ChecklistItens group by 1 order by 1",
            ["itens_os"] = "select Origem, count(*), round(sum(ValorUnitario * Quantidade), 2) from ItensOrdemServico group by 1 order by 1",
            ["estoque"] = "select sum(QuantidadeAtual) from EstoqueComponentes",
            ["equivalencias"] = "select count(*) from ComponentesEquivalentes",
            ["despesas_modelo"] = "select Ativa, count(*), round(sum(Valor), 2) from Despesas group by 1 order by 1",
            ["vistorias_os"] = "select Concluida, count(*) from VistoriasOrdemServico group by 1 order by 1",
            ["historico_consignacao"] = "select TipoEvento, substr(DataCriacao, 1, 10), count(*) from HistoricosConsignacao group by 1, 2 order by 1, 2",
            ["cadastros"] = "select (select count(*) from Usuarios), (select count(*) from Clientes), (select count(*) from ChecklistPresets), (select count(*) from TemplatesDocumento)",
        };

        var resultado = new Dictionary<string, List<string>>();
        using var conexao = new SqliteConnection($"Data Source={caminhoBanco};Mode=ReadOnly;Pooling=False");
        conexao.Open();
        foreach (var (chave, sql) in consultas)
        {
            using var cmd = conexao.CreateCommand();
            cmd.CommandText = sql;
            using var leitor = cmd.ExecuteReader();
            var linhas = new List<string>();
            while (leitor.Read())
                linhas.Add(string.Join("|", Enumerable.Range(0, leitor.FieldCount).Select(i => leitor.IsDBNull(i) ? "" : Convert.ToString(leitor.GetValue(i), System.Globalization.CultureInfo.InvariantCulture))));
            resultado[chave] = linhas;
        }
        return resultado;
    }

    private static List<string> Cenarios(byte[] exportado)
    {
        using var doc = JsonDocument.Parse(exportado);
        var lista = new List<string>();
        foreach (var secao in new[] { "propostasVenda", "ordensServico", "testDrives", "veiculosConsignados" })
            foreach (var item in doc.RootElement.GetProperty(secao).EnumerateArray())
                lista.Add($"{secao}:{item.GetProperty("cenario").GetString()}");
        lista.Sort(StringComparer.Ordinal);
        return lista;
    }
}
