using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;
using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda.Pagamento;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Geradores.Nucleo;
using Microsoft.Extensions.DependencyInjection;

namespace CarStoreManager.Geradores.Entidades;

/// <summary>
/// Gera propostas de venda via IPropostaVendaService.AddAsync. Só pode usar
/// veículos com Disponibilidade = Disponivel (por isso recebe a lista de
/// veículos já liberados pelo VeiculoVendaGerador), um Vendedor e um Cliente.
/// </summary>
public static class PropostaVendaGerador
{
    public static async Task<List<Guid>> GerarAsync(
        IServiceProvider provider,
        int quantidade,
        Random rng,
        IReadOnlyList<Guid> veiculoVendaDisponivelIds,
        IReadOnlyList<Guid> vendedorIds,
        IReadOnlyList<Guid> clienteIds)
    {
        if (veiculoVendaDisponivelIds.Count == 0 || vendedorIds.Count == 0 || clienteIds.Count == 0)
        {
            Console.WriteLine("  Propostas de venda: pulado — faltam veículos disponíveis, vendedores ou clientes.");
            return new List<Guid>();
        }

        // Não repete o mesmo veículo em mais de uma proposta simultânea —
        // cada proposta "reserva" o veículo até ser aprovada/rejeitada.
        var disponiveis = veiculoVendaDisponivelIds.OrderBy(_ => rng.Next()).ToList();
        var alvo = Math.Min(quantidade, disponiveis.Count);

        var ids = await ExecutorLote.ExecutarAsync<IPropostaVendaService>(
            provider,
            alvo,
            (servico, i) => servico.AddAsync(MontarDto(rng, disponiveis[i], vendedorIds, clienteIds)),
            "Propostas de venda");

        if (alvo < quantidade)
            Console.WriteLine($"  Propostas de venda: limitado a {alvo} — não há veículos disponíveis suficientes.");

        return ids;
    }

    private static CriarPropostaVendaDTO MontarDto(
        Random rng, Guid veiculoId, IReadOnlyList<Guid> vendedorIds, IReadOnlyList<Guid> clienteIds)
    {
        return new CriarPropostaVendaDTO
        {
            VendedorId = vendedorIds[rng.Next(vendedorIds.Count)],
            VeiculoVendaId = veiculoId,
            ClienteId = clienteIds[rng.Next(clienteIds.Count)],
            ValorBase = DocumentoUtils.ValorRedondo(rng, 35_000, 200_000, 500),
            DescontoPercentual = rng.Next(0, 16)
        };
    }

    private static readonly string[] ModosAVista = { "Dinheiro", "Pix", "CartaoDebito", "Boleto", "Transferencia" };

    /// <summary>
    /// Empurra cada proposta já criada por um ponto diferente do funil de
    /// vendas (recém-criada, aprovada, em vistoria, aguardando assinatura,
    /// concluída — parte delas via financiamento — ou rejeitada), e redata
    /// DataCriacao/DataAprovacao dentro do período histórico simulado. Só
    /// chama métodos do IPropostaVendaService/IPagamentoPropostaService reais
    /// — nenhuma etapa do funil é pulada por fora das regras de negócio.
    /// </summary>
    public static async Task AvancarStatusEDatasAsync(
        IServiceProvider provider,
        IReadOnlyList<Guid> propostaIds,
        Random rng,
        DateTime periodoInicio,
        DateTime periodoFim,
        Guid adminId,
        IReadOnlyList<Guid> vendedorIds)
    {
        var concluidas = 0;
        var financiadas = 0;
        var rejeitadas = 0;
        var emAndamento = 0;
        var recentes = 0;

        // Proposta expira 7 dias após DataCriacao (PropostaVenda.PrazoValidadeDias)
        // e todo método de transição bloqueia proposta expirada — por isso o
        // funil inteiro roda PRIMEIRO com a data real de criação (agora), e só
        // DEPOIS a data é jogada pro passado. Se redatássemos antes, toda
        // chamada veria uma proposta "criada há 2 anos" e falharia com
        // "Proposta expirou." já na primeira transição.
        var recenteInicio = periodoFim.AddDays(-6);

        foreach (var id in propostaIds)
        {
            var r = rng.NextDouble();
            var vendedorId = vendedorIds[rng.Next(vendedorIds.Count)];

            if (r < 0.08)
            {
                // Recém-criada — deixa em "Criada", dentro da janela de validade.
                var dataRecente = BackdateHelper.DataAleatoriaNoPeriodo(rng, recenteInicio, periodoFim);
                await BackdateHelper.AplicarAsync<CarStoreManager.Domain.Entities.Concessionaria.PropostaVenda>(
                    provider, id, ("DataCriacao", dataRecente));
                recentes++;
                continue;
            }

            var dataCriacaoHistorica = BackdateHelper.DataAleatoriaNoPeriodo(rng, periodoInicio, recenteInicio);

            using var scope = provider.CreateScope();
            var servico = scope.ServiceProvider.GetRequiredService<IPropostaVendaService>();
            var pagamentoServico = scope.ServiceProvider.GetRequiredService<IPagamentoPropostaService>();

            if (r < 0.15)
            {
                var rr = await servico.RejeitarAsync(id, "Cliente desistiu da compra.");
                if (rr.IsSuccess)
                {
                    rejeitadas++;
                    await BackdateHelper.AplicarAsync<CarStoreManager.Domain.Entities.Concessionaria.PropostaVenda>(
                        provider, id, ("DataCriacao", dataCriacaoHistorica));
                }
                continue;
            }

            var usaFinanciamento = rng.NextDouble() < 0.30;
            var modo = usaFinanciamento ? "Financiamento" : ModosAVista[rng.Next(ModosAVista.Length)];

            var rm = await servico.DefinirModoPagamentoAsync(id, modo);
            if (!rm.IsSuccess) continue;
            if (r < 0.20)
            {
                await BackdateHelper.AplicarAsync<CarStoreManager.Domain.Entities.Concessionaria.PropostaVenda>(
                    provider, id, ("DataCriacao", dataCriacaoHistorica));
                continue; // parou logo após escolher a forma de pagamento
            }

            if (usaFinanciamento)
            {
                var rsf = await servico.SolicitarFinanciamentoAsync(id);
                if (!rsf.IsSuccess) continue;

                var parcelas = new[] { 12, 24, 36, 48, 60 }[rng.Next(5)];
                var rrf = await servico.RegistrarRespostaFinanciadoraAsync(id, new RegistrarRespostaFinanciadoraDTO
                {
                    Parcelas = parcelas,
                    ValorParcela = DocumentoUtils.ValorRedondo(rng, 500, 4000, 50),
                    TaxaJurosMensal = 1m + rng.Next(0, 3),
                    Observacoes = "Financiamento pré-aprovado pela financeira parceira."
                });
                if (!rrf.IsSuccess) continue;
            }

            var ra = await servico.AprovarAsync(id);
            if (!ra.IsSuccess) continue;

            var dataAprovacaoHistorica = dataCriacaoHistorica.AddDays(rng.Next(1, 7));
            if (dataAprovacaoHistorica > periodoFim) dataAprovacaoHistorica = periodoFim;

            if (r < 0.30)
            {
                emAndamento++;
                await BackdateHelper.AplicarAsync<CarStoreManager.Domain.Entities.Concessionaria.PropostaVenda>(
                    provider, id, ("DataCriacao", dataCriacaoHistorica), ("DataAprovacao", dataAprovacaoHistorica));
                continue; // aprovada, aguardando vistoria
            }

            var riv = await servico.IniciarVistoriaAsync(id, vendedorId);
            if (!riv.IsSuccess) continue;

            var rv = await servico.RegistrarVistoriaAsync(id, vendedorId, new RegistrarVistoriaDTO
            {
                Observacoes = "Veículo vistoriado, sem avarias relevantes.",
                Aprovado = true
            });
            if (!rv.IsSuccess) continue;

            if (r < 0.40)
            {
                emAndamento++;
                await BackdateHelper.AplicarAsync<CarStoreManager.Domain.Entities.Concessionaria.PropostaVenda>(
                    provider, id, ("DataCriacao", dataCriacaoHistorica), ("DataAprovacao", dataAprovacaoHistorica));
                continue; // vistoria concluída, aguardando pagamento/termo
            }

            var detalhe = await servico.GetByIdAsync(id);
            if (!detalhe.IsSuccess || detalhe.Value is null) continue;

            var rp = await pagamentoServico.RegistrarPagamentoAsync(id, vendedorId, new RegistrarPagamentoPropostaDTO
            {
                ModoPagamento = modo,
                Valor = detalhe.Value.ValorFinal,
                Observacoes = "Pagamento integral registrado."
            });
            if (!rp.IsSuccess) continue;

            var rt = await servico.CriarOuEditarTermoAsync(id, adminId, new CriarOuEditarTermoDTO
            {
                TextoTermo = "Termo de entrega gerado automaticamente para popular a base de demonstração."
            });
            if (!rt.IsSuccess) continue;

            if (r < 0.48)
            {
                emAndamento++;
                await BackdateHelper.AplicarAsync<CarStoreManager.Domain.Entities.Concessionaria.PropostaVenda>(
                    provider, id, ("DataCriacao", dataCriacaoHistorica), ("DataAprovacao", dataAprovacaoHistorica));
                continue; // termo redigido, ainda não enviado
            }

            var re = await servico.EnviarTermoParaAssinaturaAsync(id);
            if (!re.IsSuccess) continue;

            if (r < 0.55)
            {
                emAndamento++;
                await BackdateHelper.AplicarAsync<CarStoreManager.Domain.Entities.Concessionaria.PropostaVenda>(
                    provider, id, ("DataCriacao", dataCriacaoHistorica), ("DataAprovacao", dataAprovacaoHistorica));
                continue; // enviado, aguardando assinatura do cliente
            }

            var termoAtual = await servico.ObterTermoAsync(id);
            if (!termoAtual.IsSuccess || string.IsNullOrEmpty(termoAtual.Value?.TokenAssinatura))
            {
                emAndamento++;
                await BackdateHelper.AplicarAsync<CarStoreManager.Domain.Entities.Concessionaria.PropostaVenda>(
                    provider, id, ("DataCriacao", dataCriacaoHistorica), ("DataAprovacao", dataAprovacaoHistorica));
                continue;
            }

            var rassin = await servico.AssinarTermoAsync(
                termoAtual.Value!.TokenAssinatura!,
                new AssinarTermoDTO
                {
                    NomeCliente = "Cliente Demonstração",
                    CpfCliente = DocumentoUtils.GerarCpf(rng),
                    Aceite = true
                },
                "127.0.0.1");

            await BackdateHelper.AplicarAsync<CarStoreManager.Domain.Entities.Concessionaria.PropostaVenda>(
                provider, id, ("DataCriacao", dataCriacaoHistorica), ("DataAprovacao", dataAprovacaoHistorica));

            if (rassin.IsSuccess)
            {
                concluidas++;
                if (usaFinanciamento) financiadas++;
            }
            else
            {
                emAndamento++;
            }
        }

        Console.WriteLine(
            $"  Propostas — funil: {concluidas} concluída(s) ({financiadas} via financiamento), " +
            $"{rejeitadas} rejeitada(s), {emAndamento} em andamento, {recentes} recém-criada(s)");
    }
}
