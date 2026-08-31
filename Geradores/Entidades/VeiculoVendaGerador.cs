using CarStoreManager.Application.DTOs.Concessionaria.VeiculoVenda;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Geradores.Dados;
using CarStoreManager.Geradores.Nucleo;
using Microsoft.Extensions.DependencyInjection;

namespace CarStoreManager.Geradores.Entidades;

/// <summary>
/// Gera veículos da concessionária (estoque de venda) via IVeiculoVendaService.AddAsync.
/// Todo veículo nasce "EmPreparacao" — para uma fração virar "Disponivel" (pré-requisito
/// para gerar PropostaVenda), chama LiberarParaVendaAsync depois da criação, na mesma
/// proporção que o admin faria manualmente na tela de detalhe do veículo.
/// </summary>
public static class VeiculoVendaGerador
{
    public static async Task<List<Guid>> GerarAsync(
        IServiceProvider provider,
        int quantidade,
        Random rng,
        UniquePool placaPool,
        UniquePool renavamPool,
        DateTime? periodoInicio = null,
        DateTime? periodoFim = null,
        double fracaoLiberarParaVenda = 0.7)
    {
        var ids = await ExecutorLote.ExecutarAsync<IVeiculoVendaService>(
            provider,
            quantidade,
            (servico, _) => servico.AddAsync(MontarDto(rng, placaPool, renavamPool)),
            "Veículos (concessionária)");

        var liberados = 0;
        foreach (var id in ids)
        {
            if (periodoInicio.HasValue && periodoFim.HasValue)
            {
                var dataAnuncio = BackdateHelper.DataAleatoriaNoPeriodo(rng, periodoInicio.Value, periodoFim.Value);
                await BackdateHelper.AplicarAsync<CarStoreManager.Domain.Entities.Concessionaria.VeiculoVenda>(
                    provider, id, ("DataCriacao", dataAnuncio));
            }

            if (rng.NextDouble() > fracaoLiberarParaVenda) continue;

            using var scope = provider.CreateScope();
            var servico = scope.ServiceProvider.GetRequiredService<IVeiculoVendaService>();
            var r = await servico.LiberarParaVendaAsync(id);
            if (r.IsSuccess) liberados++;
        }
        Console.WriteLine($"  Veículos (concessionária): {liberados}/{ids.Count} liberados para venda");

        return ids;
    }

    /// <summary>Ids que já foram liberados para venda (Disponivel) — usados pelo PropostaVendaGerador.</summary>
    public static async Task<List<Guid>> ObterDisponiveisAsync(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var servico = scope.ServiceProvider.GetRequiredService<IVeiculoVendaService>();
        var r = await servico.ObterDisponiveisAsync();
        return r.IsSuccess ? r.Value!.Select(v => v.Id).ToList() : new List<Guid>();
    }

    private static CriarVeiculoVendaDTO MontarDto(Random rng, UniquePool placaPool, UniquePool renavamPool)
    {
        var (marca, modelos) = NomesPt.MarcasEModelos[rng.Next(NomesPt.MarcasEModelos.Length)];
        var ano = rng.Next(2012, DateTime.Now.Year + 1);
        var valor = DocumentoUtils.ValorRedondo(rng, 35_000, 200_000, 500);

        return new CriarVeiculoVendaDTO
        {
            Marca = marca,
            Modelo = modelos[rng.Next(modelos.Length)],
            Cor = NomesPt.Cores[rng.Next(NomesPt.Cores.Length)],
            Motorizacao = NomesPt.Motorizacoes[rng.Next(NomesPt.Motorizacoes.Length)],
            Ano = ano,
            Quilometragem = rng.Next(0, 150_000),
            Placa = placaPool.Reservar(() => DocumentoUtils.GerarPlaca(rng)),
            Renavam = renavamPool.Reservar(() => DocumentoUtils.GerarRenavam(rng)),
            Cambio = Enum.GetValues<TipoCambio>()[rng.Next(2)].ToString(),
            Combustivel = Enum.GetValues<TipoCombustivel>()[rng.Next(Enum.GetValues<TipoCombustivel>().Length)].ToString(),
            Valor = valor,
            // Valor de aquisição: a concessionária compra abaixo do preço de venda
            // (margem de 10% a 30% sobre o custo de compra).
            ValorAquisicao = DocumentoUtils.ValorRedondo(rng, (int)(valor * 0.7m), (int)(valor * 0.9m), 500),
            Acessorios = AcessoriosAleatorios(rng),
            AnoUltimoIpvaPago = rng.Next(ano, DateTime.Now.Year + 1),
            TextoTermoPreliminar = "Veículo vendido no estado em que se encontra, conforme vistoria realizada na entrega."
        };
    }

    private static List<string> AcessoriosAleatorios(Random rng)
    {
        var todos = Enum.GetValues<AcessoriosVeiculo>().Where(a => a != AcessoriosVeiculo.Nenhum).ToArray();
        return todos.Where(_ => rng.Next(2) == 0).Select(a => a.ToString()).ToList();
    }
}
