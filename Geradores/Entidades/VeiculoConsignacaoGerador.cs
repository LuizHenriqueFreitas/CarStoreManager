using CarStoreManager.Application.DTOs.Concessionaria.VeiculoConsignacao;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Geradores.Dados;
using CarStoreManager.Geradores.Nucleo;
using Microsoft.Extensions.DependencyInjection;

namespace CarStoreManager.Geradores.Entidades;

/// <summary>
/// Gera veículos consignados via IVeiculoConsignacaoService.AddAsync. Depende
/// de Clientes (proprietário) e Vendedores (responsável) já existentes.
/// </summary>
public static class VeiculoConsignacaoGerador
{
    public static async Task<List<Guid>> GerarAsync(
        IServiceProvider provider,
        int quantidade,
        Random rng,
        UniquePool placaPool,
        UniquePool renavamPool,
        IReadOnlyList<Guid> clienteIds,
        IReadOnlyList<Guid> vendedorIds,
        DateTime? periodoInicio = null,
        DateTime? periodoFim = null)
    {
        if (clienteIds.Count == 0 || vendedorIds.Count == 0)
        {
            Console.WriteLine("  Veículos consignados: pulado — faltam Clientes ou Vendedores.");
            return new List<Guid>();
        }

        var prazosPorId = new Dictionary<Guid, int>();

        var ids = await ExecutorLote.ExecutarAsync<IVeiculoConsignacaoService>(
            provider,
            quantidade,
            async (servico, _) =>
            {
                var dto = MontarDto(rng, placaPool, renavamPool, clienteIds, vendedorIds);
                var r = await servico.AddAsync(dto);
                if (r.IsSuccess) prazosPorId[r.Value] = dto.PrazoDias;
                return r;
            },
            "Veículos consignados");

        if (!periodoInicio.HasValue || !periodoFim.HasValue)
            return ids;

        // Avança uma fração pro ciclo completo (vendida/concluída) ou pra
        // devolução — o resto fica Ativa, simulando consignações de vários
        // estágios ao longo dos últimos ~2 anos, não só recém-cadastradas.
        var concluidas = 0;
        var vendidasAguardando = 0;
        var devolvidas = 0;
        foreach (var id in ids)
        {
            var dataInicio = BackdateHelper.DataAleatoriaNoPeriodo(rng, periodoInicio.Value, periodoFim.Value);
            var prazoDias = prazosPorId.GetValueOrDefault(id, 90);
            var dataVencimento = dataInicio.AddDays(prazoDias);

            await BackdateHelper.AplicarAsync<CarStoreManager.Domain.Entities.Concessionaria.VeiculoConsignacao>(
                provider, id,
                ("DataCriacao", dataInicio),
                ("DataInicio", dataInicio),
                ("DataVencimento", dataVencimento));

            var r = rng.NextDouble();
            using var scope = provider.CreateScope();
            var servico = scope.ServiceProvider.GetRequiredService<IVeiculoConsignacaoService>();

            if (r < 0.35)
            {
                // Fica Ativa (em aberto) — inclui tanto consignações recém-
                // criadas quanto antigas ainda não vendidas nem devolvidas.
                continue;
            }
            if (r < 0.75)
            {
                var rv = await servico.MarcarComoVendidaAsync(id);
                if (!rv.IsSuccess) continue;
                var rc = await servico.ConcluirVendaAsync(id);
                if (rc.IsSuccess) concluidas++;
            }
            else if (r < 0.90)
            {
                var rv = await servico.MarcarComoVendidaAsync(id);
                if (rv.IsSuccess) vendidasAguardando++;
            }
            else
            {
                var rd = await servico.DevolverAsync(id);
                if (rd.IsSuccess) devolvidas++;
            }
        }

        Console.WriteLine(
            $"  Veículos consignados: {concluidas} concluída(s), {vendidasAguardando} vendida(s) aguardando pagamento, {devolvidas} devolvida(s), resto ativas");

        return ids;
    }

    private static CriarVeiculoConsignacaoDTO MontarDto(
        Random rng,
        UniquePool placaPool,
        UniquePool renavamPool,
        IReadOnlyList<Guid> clienteIds,
        IReadOnlyList<Guid> vendedorIds)
    {
        var (marca, modelos) = NomesPt.MarcasEModelos[rng.Next(NomesPt.MarcasEModelos.Length)];
        var valorVenda = DocumentoUtils.ValorRedondo(rng, 30_000, 170_000, 500);
        var tipoFixo = rng.Next(2) == 0;

        var dto = new CriarVeiculoConsignacaoDTO
        {
            Marca = marca,
            Modelo = modelos[rng.Next(modelos.Length)],
            Cor = NomesPt.Cores[rng.Next(NomesPt.Cores.Length)],
            Motorizacao = NomesPt.Motorizacoes[rng.Next(NomesPt.Motorizacoes.Length)],
            Ano = rng.Next(2010, DateTime.Now.Year + 1),
            Quilometragem = rng.Next(0, 160_000),
            Placa = placaPool.Reservar(() => DocumentoUtils.GerarPlaca(rng)),
            Renavam = renavamPool.Reservar(() => DocumentoUtils.GerarRenavam(rng)),
            Cambio = Enum.GetValues<TipoCambio>()[rng.Next(2)].ToString(),
            Combustivel = Enum.GetValues<TipoCombustivel>()[rng.Next(Enum.GetValues<TipoCombustivel>().Length)].ToString(),
            ClienteProprietarioId = clienteIds[rng.Next(clienteIds.Count)],
            VendedorResponsavelId = vendedorIds[rng.Next(vendedorIds.Count)],
            ValorVendaEsperado = valorVenda,
            TextoContrato = "Contrato de consignação: o proprietário autoriza a concessionária a intermediar a venda do veículo pelo prazo estipulado.",
            PrazoDias = new[] { 60, 90, 120, 180 }[rng.Next(4)]
        };

        if (tipoFixo)
        {
            dto.TipoComissao = "Fixo";
            dto.ValorFixoProprietario = Math.Round(valorVenda * 0.85m, 2);
        }
        else
        {
            dto.TipoComissao = "Porcentagem";
            dto.PorcentagemProprietario = rng.Next(75, 91);
        }

        return dto;
    }
}
