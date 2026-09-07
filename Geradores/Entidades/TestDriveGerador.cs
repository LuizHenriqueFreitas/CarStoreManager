using CarStoreManager.Application.DTOs.Concessionaria.TestDrive;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Geradores.Nucleo;
using Microsoft.Extensions.DependencyInjection;

namespace CarStoreManager.Geradores.Entidades;

/// <summary>
/// Gera test drives via ITestDriveService — liga um veículo do salão a um
/// cliente e a um vendedor. Explora todos os status (Agendado, Realizado,
/// Cancelado, NãoCompareceu) e o reagendamento, espalhados pelo período
/// histórico. Deve rodar ANTES das propostas serem concluídas (senão o
/// veículo já está "Vendido" e o agendamento é recusado).
/// </summary>
public static class TestDriveGerador
{
    private static readonly string[] Observacoes =
    {
        "Cliente quer testar em rodovia.", "Test drive de rotina antes da proposta.",
        "Cliente trouxe familiar para avaliar o espaço interno.", "Interesse em financiamento após o test drive.",
        "Cliente comparando com outro modelo.", "Reagendado a pedido do cliente.",
        "", "", ""
    };

    public static async Task<List<Guid>> GerarAsync(
        IServiceProvider provider,
        int quantidade,
        Random rng,
        IReadOnlyList<Guid> veiculoVendaIds,
        IReadOnlyList<Guid> clienteIds,
        IReadOnlyList<Guid> vendedorIds,
        DateTime periodoInicio,
        DateTime periodoFim)
    {
        if (veiculoVendaIds.Count == 0 || clienteIds.Count == 0 || vendedorIds.Count == 0)
        {
            Console.WriteLine("  Test drives: pulado — faltam veículos, clientes ou vendedores.");
            return new List<Guid>();
        }

        var ids = new List<Guid>(quantidade);
        var contadores = new Dictionary<string, int>();
        void Conta(string k) => contadores[k] = contadores.GetValueOrDefault(k) + 1;

        for (var i = 0; i < quantidade; i++)
        {
            // ~15% no futuro próximo (agendados de verdade), o resto no passado.
            var futuro = rng.NextDouble() < 0.15;
            var dataHora = futuro
                ? DateTime.Now.AddDays(rng.Next(1, 21)).Date.AddHours(rng.Next(8, 18))
                : BackdateHelper.DataAleatoriaNoPeriodo(rng, periodoInicio, periodoFim);

            Guid id;
            using (var scope = provider.CreateScope())
            {
                var servico = scope.ServiceProvider.GetRequiredService<ITestDriveService>();
                var r = await servico.AgendarAsync(new CriarTestDriveDTO
                {
                    VeiculoVendaId = veiculoVendaIds[rng.Next(veiculoVendaIds.Count)],
                    ClienteId = clienteIds[rng.Next(clienteIds.Count)],
                    VendedorId = vendedorIds[rng.Next(vendedorIds.Count)],
                    DataHora = dataHora,
                    Observacao = Observacoes[rng.Next(Observacoes.Length)] is { Length: > 0 } o ? o : null
                });
                if (!r.IsSuccess || r.Value == Guid.Empty) { Conta("falha"); continue; }
                id = r.Value;
            }

            ids.Add(id);
            await BackdateHelper.AplicarAsync<CarStoreManager.Domain.Entities.Concessionaria.TestDrive>(
                provider, id, ("DataCriacao", dataHora.AddDays(-rng.Next(0, 4))));

            if (futuro)
            {
                Conta("agendado (futuro)");
                continue;
            }

            // Passado — resolve o status.
            var r2 = rng.NextDouble();
            using var scope2 = provider.CreateScope();
            var svc = scope2.ServiceProvider.GetRequiredService<ITestDriveService>();

            if (r2 < 0.10)
            {
                // reagendado e depois realizado
                await svc.ReagendarAsync(id, dataHora.AddDays(rng.Next(1, 8)));
                await svc.AtualizarStatusAsync(new AtualizarStatusTestDriveDTO { Id = id, Status = "Realizado" });
                Conta("reagendado e realizado");
            }
            else if (r2 < 0.62)
            {
                await svc.AtualizarStatusAsync(new AtualizarStatusTestDriveDTO { Id = id, Status = "Realizado" });
                Conta("realizado");
            }
            else if (r2 < 0.82)
            {
                await svc.AtualizarStatusAsync(new AtualizarStatusTestDriveDTO { Id = id, Status = "Cancelado" });
                Conta("cancelado");
            }
            else
            {
                await svc.AtualizarStatusAsync(new AtualizarStatusTestDriveDTO { Id = id, Status = "NaoCompareceu" });
                Conta("não compareceu");
            }
        }

        Console.WriteLine($"  Test drives: {ids.Count}/{quantidade} criados");
        foreach (var (k, n) in contadores.OrderByDescending(kv => kv.Value))
            Console.WriteLine($"    - {k}: {n}");

        return ids;
    }
}
