using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Geradores.Nucleo;
using Microsoft.Extensions.DependencyInjection;

namespace CarStoreManager.Geradores.Entidades;

/// <summary>
/// Gera os balanços mensais de despesas (Financeiro → Despesas do mês) para
/// todo o período histórico simulado, via IBalancoMensalDespesaService. Cada
/// competência é criada a partir do formulário-modelo, ganha algumas linhas
/// extras com "valores reais" (variação mensal) e é fechada — só as duas
/// competências mais recentes ficam abertas, como aconteceria numa operação
/// real. É a base das análises retro-avaliativas.
/// </summary>
public static class BalancoDespesaGerador
{
    private static readonly (string Nome, string Setor, string Categoria)[] LinhasExtras =
    {
        ("Hora extra da equipe", "Oficina", "folha"),
        ("Descarte de óleo e resíduos", "Oficina", "operacional"),
        ("Peça danificada em bancada", "Oficina", "perda"),
        ("Brindes e cortesias para clientes", "Concessionaria", "marketing"),
        ("Comissão extra de campanha", "Concessionaria", "folha"),
        ("Lavagem e detalhamento de estoque", "Concessionaria", "operacional"),
        ("Conta de energia (ajuste)", "Geral", "energia"),
        ("Manutenção predial", "Geral", "manutencao"),
        ("Taxa bancária e maquininha", "Geral", "servicos"),
        ("Material de limpeza e copa", "Geral", "operacional"),
    };

    public static async Task<int> GerarAsync(
        IServiceProvider provider,
        Random rng,
        DateTime periodoInicio,
        DateTime periodoFim)
    {
        var competencias = new List<(int Ano, int Mes)>();
        var cursor = new DateTime(periodoInicio.Year, periodoInicio.Month, 1);
        var limite = new DateTime(periodoFim.Year, periodoFim.Month, 1);
        while (cursor <= limite)
        {
            competencias.Add((cursor.Year, cursor.Month));
            cursor = cursor.AddMonths(1);
        }

        var criados = 0;
        var fechados = 0;

        for (var idx = 0; idx < competencias.Count; idx++)
        {
            var (ano, mes) = competencias[idx];
            var ehRecente = idx >= competencias.Count - 2; // últimas 2 ficam abertas

            using (var scope = provider.CreateScope())
            {
                var servico = scope.ServiceProvider.GetRequiredService<IBalancoMensalDespesaService>();

                var gerado = await servico.GerarDoModeloAsync(ano, mes);
                if (!gerado.IsSuccess) continue;
                criados++;

                // 2 a 5 linhas extras com valores reais do mês
                var extras = rng.Next(2, 6);
                for (var k = 0; k < extras; k++)
                {
                    var linha = LinhasExtras[rng.Next(LinhasExtras.Length)];
                    await servico.SalvarItemAsync(new SalvarItemBalancoDTO
                    {
                        Ano = ano,
                        Mes = mes,
                        Nome = linha.Nome,
                        Setor = linha.Setor,
                        Categoria = linha.Categoria,
                        Valor = DocumentoUtils.ValorRedondo(rng, 120, 6000, 20)
                    });
                }

                if (!ehRecente)
                {
                    var f = await servico.FecharAsync(ano, mes);
                    if (f.IsSuccess) fechados++;
                }
            }

            // Redata a criação (e o fechamento) para dentro da competência.
            var dataCriacao = new DateTime(ano, mes, Math.Min(3, DateTime.DaysInMonth(ano, mes)))
                .AddDays(rng.Next(0, 3)).AddHours(rng.Next(8, 18));
            var valores = ehRecente
                ? new (string, object?)[] { ("DataCriacao", dataCriacao) }
                : new (string, object?)[]
                {
                    ("DataCriacao", dataCriacao),
                    ("DataFechamento", dataCriacao.AddDays(rng.Next(28, 34)))
                };

            await BalancoBackdateAsync(provider, ano, mes, valores);
        }

        Console.WriteLine($"  Balanços mensais de despesas: {criados} competência(s) criada(s), {fechados} fechada(s)");
        return criados;
    }

    /// <summary>
    /// Backdate por competência (ano/mês) em vez de por Id — o balanço não é
    /// exposto por Id pelo service, mas a competência é a chave natural.
    /// </summary>
    private static async Task BalancoBackdateAsync(
        IServiceProvider provider, int ano, int mes, (string Propriedade, object? Valor)[] valores)
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CarStoreManager.Infrastructure.Data.AppDbContext>();

        var competencia = new DateOnly(ano, mes, 1);
        var balanco = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstOrDefaultAsync(context.BalancosMensaisDespesa, b => b.Competencia == competencia);
        if (balanco is null) return;

        var entry = context.Entry(balanco);
        foreach (var (propriedade, valor) in valores)
            entry.Property(propriedade).CurrentValue = valor;

        await context.SaveChangesAsync();
    }
}
