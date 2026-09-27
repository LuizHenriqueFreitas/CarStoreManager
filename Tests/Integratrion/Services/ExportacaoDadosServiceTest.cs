using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Infrastructure.Data;
using CarStoreManager.Infrastructure.Services.Sistema;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CarStoreManager.Tests.Integration.Services;

/// <summary>
/// O export precisa produzir EXATAMENTE o formato que
/// <c>ImportacaoDadosService.ImportarAsync</c> consome (seções em camelCase
/// com "chave"/"cenario", não um retrato bruto das tabelas) — ver
/// docs/redesign/26-alinhamento-exportacao-importacao.md. Estes testes
/// verificam a forma do JSON; o round-trip de negócio completo (funil de
/// proposta/OS reconstruído de verdade) foi validado manualmente com o banco
/// de demonstração real (1819 registros, avisos só do admin semeado).
/// </summary>
public class ExportacaoDadosServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly ExportacaoDadosService _service;

    public ExportacaoDadosServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=False");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _service = new ExportacaoDadosService(_context, NullLogger<ExportacaoDadosService>.Instance);
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    [Fact]
    public async Task ExportarJsonAsync_BancoVazio_GeraTodasAsSecoesVazias()
    {
        var r = await _service.ExportarJsonAsync();

        r.IsSuccess.Should().BeTrue();
        r.Value!.NomeArquivo.Should().EndWith(".json");

        using var doc = JsonDocument.Parse(Encoding.UTF8.GetString(r.Value.Conteudo));
        var secoes = new[]
        {
            "usuarios", "clientes", "fornecedores", "componentes", "checklistPresets",
            "templatesDocumento", "despesas", "veiculosVenda", "veiculosConsignados",
            "veiculosCliente", "propostasVenda", "ordensServico", "testDrives", "despesasExtras",
        };
        foreach (var secao in secoes)
        {
            doc.RootElement.TryGetProperty(secao, out var valor).Should().BeTrue($"a seção \"{secao}\" deve existir (mesmo formato do importador)");
            valor.GetArrayLength().Should().Be(0);
        }
    }

    [Fact]
    public async Task ExportarJsonAsync_ComCliente_UsaChaveDeTextoEmVezDeGuidCru()
    {
        var cliente = new Cliente("Ricardo Exportado", "ricardo@email.com", "11988887777", "52998224725",
            new Endereco("Rua A", "1", null, "Centro", "São Paulo", "SP", "01001000"));
        _context.Set<Cliente>().Add(cliente);
        await _context.SaveChangesAsync();

        var r = await _service.ExportarJsonAsync();
        r.IsSuccess.Should().BeTrue();

        using var doc = JsonDocument.Parse(Encoding.UTF8.GetString(r.Value!.Conteudo));
        var clientes = doc.RootElement.GetProperty("clientes");
        clientes.GetArrayLength().Should().Be(1);

        var item = clientes[0];
        item.GetProperty("nome").GetString().Should().Be("Ricardo Exportado");
        item.GetProperty("cpf").GetString().Should().Be("52998224725");
        // "chave" precisa ser o próprio Guid do registro (usado pra resolver
        // referências entre seções na reimportação) — não um índice/slug.
        Guid.TryParse(item.GetProperty("chave").GetString(), out var chaveGuid).Should().BeTrue();
        chaveGuid.Should().Be(cliente.Id);
    }

    [Fact]
    public async Task ExportarJsonAsync_DespesaAutoGeradaPorCompraDeVeiculo_NaoEntraEmDespesasExtras()
    {
        // Reproduz a duplicação encontrada num round-trip real: reimportar
        // VeiculosVenda já relança essa mesma despesa sozinho (ver
        // VeiculoVendaService.AddAsync) — reexportá-la como DespesaExtra
        // duplicaria o gasto no próximo import.
        var balanco = new BalancoMensalDespesa(new DateOnly(2026, 3, 1));
        balanco.AdicionarItem(
            "Compra de veículo para concessionária: Fiat Uno — AAA1234",
            SetorDespesa.Concessionaria, "Compra de veículo", 45000m);
        balanco.AdicionarItem(
            "Compra de componente: Filtro de óleo — SKU-1 (x10)",
            SetorDespesa.Oficina, "Compra de componentes", 300m);
        balanco.AdicionarItem(
            "Troca do elevador hidráulico principal da oficina",
            SetorDespesa.Oficina, "Investimento", 8500m);
        _context.Set<BalancoMensalDespesa>().Add(balanco);
        await _context.SaveChangesAsync();

        var r = await _service.ExportarJsonAsync();
        r.IsSuccess.Should().BeTrue();

        using var doc = JsonDocument.Parse(Encoding.UTF8.GetString(r.Value!.Conteudo));
        var extras = doc.RootElement.GetProperty("despesasExtras");
        extras.GetArrayLength().Should().Be(1, "só a despesa genuinamente avulsa deve sobrar — as 2 auto-geradas por compra são recriadas pelas próprias seções VeiculosVenda/Componentes");
        extras[0].GetProperty("nome").GetString().Should().Be("Troca do elevador hidráulico principal da oficina");
    }

    [Fact]
    public async Task ExportarJsonAsync_ItemDoModelo_NaoEntraEmDespesasExtras()
    {
        var balanco = new BalancoMensalDespesa(new DateOnly(2026, 3, 1));
        balanco.AdicionarItem("Aluguel do prédio principal", SetorDespesa.Geral, null, 2430m, doModelo: true);
        _context.Set<BalancoMensalDespesa>().Add(balanco);
        await _context.SaveChangesAsync();

        var r = await _service.ExportarJsonAsync();
        r.IsSuccess.Should().BeTrue();

        using var doc = JsonDocument.Parse(Encoding.UTF8.GetString(r.Value!.Conteudo));
        doc.RootElement.GetProperty("despesasExtras").GetArrayLength().Should().Be(0);
    }
}
