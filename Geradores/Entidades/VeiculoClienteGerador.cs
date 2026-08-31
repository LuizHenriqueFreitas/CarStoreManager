using CarStoreManager.Application.DTOs.Oficina.VeiculoCliente;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Geradores.Dados;
using CarStoreManager.Geradores.Nucleo;
using Microsoft.Extensions.DependencyInjection;

namespace CarStoreManager.Geradores.Entidades;

/// <summary>
/// Gera veículos de cliente (trazidos à oficina para serviço) via
/// IVeiculoClienteService.AddAsync. Cada veículo pertence a um Cliente já
/// existente — por isso recebe a lista de ClienteIds já gerados.
/// </summary>
public static class VeiculoClienteGerador
{
    /// <summary>Par (VeiculoClienteId, ClienteId dono) — usado pelo OrdemServicoGerador para
    /// manter a OS consistente com o dono real do veículo.</summary>
    public readonly record struct VeiculoDoCliente(Guid VeiculoClienteId, Guid ClienteId);

    public static async Task<List<VeiculoDoCliente>> GerarAsync(
        IServiceProvider provider,
        int quantidade,
        Random rng,
        UniquePool placaPool,
        IReadOnlyList<Guid> clienteIds)
    {
        if (clienteIds.Count == 0)
        {
            Console.WriteLine("  Veículos (cliente): pulado — nenhum Cliente disponível.");
            return new List<VeiculoDoCliente>();
        }

        var gerados = new List<VeiculoDoCliente>(quantidade);
        var falhas = 0;

        for (var i = 0; i < quantidade; i++)
        {
            var clienteId = clienteIds[rng.Next(clienteIds.Count)];
            var dto = MontarDto(rng, placaPool, clienteId);

            using var scope = provider.CreateScope();
            var servico = scope.ServiceProvider.GetRequiredService<IVeiculoClienteService>();
            var resultado = await servico.AddAsync(dto);

            if (resultado.IsSuccess && resultado.Value != Guid.Empty)
                gerados.Add(new VeiculoDoCliente(resultado.Value, clienteId));
            else
                falhas++;
        }

        Console.WriteLine(
            falhas == 0
                ? $"  Veículos (cliente): {gerados.Count}/{quantidade} criados"
                : $"  Veículos (cliente): {gerados.Count}/{quantidade} criados ({falhas} falharam)");

        return gerados;
    }

    private static CriarVeiculoClienteDTO MontarDto(Random rng, UniquePool placaPool, Guid clienteId)
    {
        var (marca, modelos) = NomesPt.MarcasEModelos[rng.Next(NomesPt.MarcasEModelos.Length)];

        return new CriarVeiculoClienteDTO
        {
            ClienteId = clienteId,
            Marca = marca,
            Modelo = modelos[rng.Next(modelos.Length)],
            Cor = NomesPt.Cores[rng.Next(NomesPt.Cores.Length)],
            Ano = rng.Next(2008, DateTime.Now.Year + 1),
            Placa = placaPool.Reservar(() => DocumentoUtils.GerarPlaca(rng))
        };
    }
}
