using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico.Pagamento;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Geradores.Nucleo;
using Microsoft.Extensions.DependencyInjection;
using static CarStoreManager.Geradores.Entidades.VeiculoClienteGerador;

namespace CarStoreManager.Geradores.Entidades;

/// <summary>
/// Gera ordens de serviço densas via os Application Services reais e as
/// distribui por TODO o ciclo de vida da oficina, cobrindo:
///  - processos concluídos com sucesso (serviço feito, pago e entregue);
///  - processos que tiveram problema no meio do caminho e mesmo assim
///    fecharam (requisição de peça, pausa por aumento de escopo, pagamento
///    parcial em aberto);
///  - processos não concluídos, parados em cada etapa do fluxo
///    (orçamento, revisão, aprovação, execução, busca de peças, pausa).
/// Cada OS ganha checklist (a partir de um preset), itens de peça do estoque
/// e, quando o cenário pede, pagamentos — para as telas e gráficos ficarem
/// densos de verdade. Datas espalhadas pelo período histórico simulado.
/// </summary>
public static class OrdemServicoGerador
{
    private static readonly string[] Descricoes =
    {
        "Barulho estranho ao frear", "Revisão dos 10.000 km", "Troca de óleo e filtros",
        "Carro não liga pela manhã", "Ar-condicionado não gela", "Vazamento de óleo identificado",
        "Luz de injeção acesa no painel", "Vibração no volante em alta velocidade",
        "Cliente relata consumo elevado de combustível", "Pastilhas de freio gastas",
        "Revisão pré-viagem", "Suspensão fazendo ruído em buracos", "Troca da correia dentada",
        "Embreagem patinando", "Superaquecimento do motor", "Revisão dos 40.000 km",
        "Direção hidráulica pesada", "Farol queimado e revisão elétrica"
    };

    private static readonly string[] PecasRequisitadas =
    {
        "Sensor de rotação específico do modelo", "Bomba d'água original",
        "Kit de correia com tensor", "Módulo da injeção eletrônica",
        "Cabeçote retificado", "Turbina recondicionada", "Coxim do motor original"
    };

    private static readonly string[] ModosPagamento =
        { "Dinheiro", "Pix", "CartaoDebito", "CartaoCredito", "Transferencia" };

    public static async Task<List<Guid>> GerarAsync(
        IServiceProvider provider,
        int quantidade,
        Random rng,
        IReadOnlyList<VeiculoDoCliente> veiculosClientes,
        IReadOnlyList<Guid> mecanicoIds,
        IReadOnlyList<Guid> componenteIds,
        IReadOnlyList<Guid> presetIds,
        Guid adminId,
        DateTime periodoInicio,
        DateTime periodoFim)
    {
        if (veiculosClientes.Count == 0 || mecanicoIds.Count == 0)
        {
            Console.WriteLine("  Ordens de serviço: pulado — faltam veículos de cliente ou mecânicos.");
            return new List<Guid>();
        }

        var ids = new List<Guid>(quantidade);
        var contadores = new Dictionary<string, int>();
        void Conta(string chave) => contadores[chave] = contadores.GetValueOrDefault(chave) + 1;

        for (var i = 0; i < quantidade; i++)
        {
            var veiculo = veiculosClientes[rng.Next(veiculosClientes.Count)];
            var mecanicoId = mecanicoIds[rng.Next(mecanicoIds.Count)];
            var presetId = presetIds.Count > 0 && rng.NextDouble() < 0.75
                ? presetIds[rng.Next(presetIds.Count)]
                : (Guid?)null;

            Guid osId;
            using (var scope = provider.CreateScope())
            {
                var servico = scope.ServiceProvider.GetRequiredService<IOrdemServicoService>();
                var criar = new CriarOrdemServicoDTO
                {
                    VeiculoClienteId = veiculo.VeiculoClienteId,
                    ClienteId = veiculo.ClienteId,
                    MecanicoId = mecanicoId,
                    Tipo = Enum.GetValues<TipoServico>()[rng.Next(Enum.GetValues<TipoServico>().Length)].ToString(),
                    Descricao = Descricoes[rng.Next(Descricoes.Length)],
                    PrazoEstimado = DateTime.UtcNow.AddDays(rng.Next(3, 45)),
                    CustoServico = DocumentoUtils.ValorRedondo(rng, 100, 1800, 50),
                    ChecklistPresetId = presetId,
                    Itens = new List<ItemOrdemServicoDTO>()
                };
                var r = await servico.AddAsync(criar);
                if (!r.IsSuccess || r.Value == Guid.Empty) { Conta("falha_criacao"); continue; }
                osId = r.Value;
            }

            ids.Add(osId);

            // Data no passado, dentro do período histórico simulado.
            var dataCriacao = BackdateHelper.DataAleatoriaNoPeriodo(rng, periodoInicio, periodoFim);
            await BackdateHelper.AplicarAsync<CarStoreManager.Domain.Entities.Oficina.OrdemServico>(
                provider, osId,
                ("DataCriacao", dataCriacao),
                ("PrazoEstimado", dataCriacao.AddDays(rng.Next(3, 20))));

            var cenario = rng.NextDouble();
            var rotulo = await ExecutarCenarioAsync(
                provider, rng, osId, mecanicoId, adminId, componenteIds, cenario);
            Conta(rotulo);
        }

        Console.WriteLine($"  Ordens de serviço: {ids.Count}/{quantidade} criadas");
        foreach (var (chave, n) in contadores.OrderByDescending(kv => kv.Value))
            Console.WriteLine($"    - {chave}: {n}");

        return ids;
    }

    /// <summary>
    /// Executa um dos cenários de ciclo de vida sobre a OS recém-criada e
    /// devolve um rótulo curto pro resumo. Toda transição passa pelos
    /// Application Services reais.
    /// </summary>
    private static async Task<string> ExecutarCenarioAsync(
        IServiceProvider provider, Random rng, Guid osId, Guid mecanicoId, Guid adminId,
        IReadOnlyList<Guid> componenteIds, double cenario)
    {
        // ---------- NÃO CONCLUÍDAS (paradas em cada etapa) ----------
        if (cenario < 0.05) return "pendente (orçamento em aberto)";

        if (cenario < 0.09)
        {
            await Os(provider, s => s.EnviarParaRevisaoAsync(osId));
            return "em análise do mecânico";
        }

        if (cenario < 0.12)
        {
            await Os(provider, s => s.EnviarParaRevisaoAsync(osId));
            await Os(provider, s => s.AprovarPeloMecanicoAsync(osId));
            return "aguardando decisão do cliente";
        }

        if (cenario < 0.15)
        {
            await AprovarPeloFluxoAsync(provider, osId);
            return "aprovada, aguardando início";
        }

        if (cenario < 0.20)
        {
            await AdicionarItensEstoqueAsync(provider, rng, osId, componenteIds, 1, 3);
            await Os(provider, s => s.IniciarAsync(osId));
            await ConcluirParteDoChecklistAsync(provider, rng, osId, parcial: true);
            return "em andamento (mecânico trabalhando)";
        }

        if (cenario < 0.24)
        {
            await AbrirRequisicaoAsync(provider, rng, osId, mecanicoId);
            return "parada — buscando peças p/ orçamento";
        }

        if (cenario < 0.28)
        {
            await AdicionarItensEstoqueAsync(provider, rng, osId, componenteIds, 1, 2);
            await Os(provider, s => s.IniciarAsync(osId));
            await PausarPorAlertaAsync(provider, osId, mecanicoId);
            return "pausada — aguardando cliente decidir escopo";
        }

        // ---------- CONCLUÍDAS COM PROBLEMA NO MEIO ----------
        if (cenario < 0.36)
        {
            // cancelada de um ponto qualquer do fluxo
            var ponto = rng.Next(4);
            if (ponto >= 1) await Os(provider, s => s.EnviarParaRevisaoAsync(osId));
            if (ponto >= 2) await Os(provider, s => s.AprovarPeloMecanicoAsync(osId));
            if (ponto >= 3)
            {
                await Os(provider, s => s.RegistrarAprovacaoDoClienteAsync(osId));
                await AdicionarItensEstoqueAsync(provider, rng, osId, componenteIds, 1, 2);
                await Os(provider, s => s.IniciarAsync(osId));
            }
            await Os(provider, s => s.CancelarAsync(osId));
            return "cancelada no meio do processo";
        }

        if (cenario < 0.44)
        {
            // requisição de peça → atendida pelo admin → liberada → conclui OK
            var reqId = await AbrirRequisicaoAsync(provider, rng, osId, mecanicoId);
            if (reqId is { } rid && componenteIds.Count > 0)
            {
                await Req(provider, s => s.AtenderAsync(rid, adminId, new AtenderRequisicaoDTO
                {
                    ComponenteId = componenteIds[rng.Next(componenteIds.Count)],
                    Quantidade = rng.Next(1, 3),
                    Observacao = "Peça cotada e encomendada junto ao fornecedor."
                }));
                await Req(provider, s => s.LiberarOrdemAsync(osId));
            }
            await AdicionarItensEstoqueAsync(provider, rng, osId, componenteIds, 0, 2);
            await ConcluirComPagamentoAsync(provider, rng, osId, adminId, entregar: true);
            return "concluída após requisição de peça";
        }

        if (cenario < 0.50)
        {
            // pausa por aumento de escopo → retomada → conclui OK
            await AdicionarItensEstoqueAsync(provider, rng, osId, componenteIds, 1, 2);
            await Os(provider, s => s.IniciarAsync(osId));
            await PausarPorAlertaAsync(provider, osId, mecanicoId);
            await RetomarAlertaAsync(provider, osId, adminId);
            await ConcluirParteDoChecklistAsync(provider, rng, osId, parcial: false);
            await FinalizarAsync(provider, osId);
            await ConcluirComPagamentoAsync(provider, rng, osId, adminId, entregar: true);
            return "concluída após pausa por aumento de escopo";
        }

        if (cenario < 0.58)
        {
            // serviço técnico pronto, cliente pagou só uma parte — saldo em aberto
            await PrepararEIniciarAsync(provider, rng, osId, componenteIds);
            await ConcluirParteDoChecklistAsync(provider, rng, osId, parcial: false);
            await FinalizarAsync(provider, osId);
            await RegistrarPagamentoParcialAsync(provider, rng, osId, adminId);
            return "pagamento pendente — saldo parcial em aberto";
        }

        if (cenario < 0.66)
        {
            // serviço técnico pronto, aguardando a recepção cobrar (nada pago)
            await PrepararEIniciarAsync(provider, rng, osId, componenteIds);
            await ConcluirParteDoChecklistAsync(provider, rng, osId, parcial: false);
            await FinalizarAsync(provider, osId);
            return "pagamento pendente — aguardando cobrança";
        }

        // ---------- CONCLUÍDAS COM SUCESSO ----------
        if (cenario < 0.80)
        {
            await PrepararEIniciarAsync(provider, rng, osId, componenteIds);
            await ConcluirParteDoChecklistAsync(provider, rng, osId, parcial: false);
            await FinalizarAsync(provider, osId);
            await ConcluirComPagamentoAsync(provider, rng, osId, adminId, entregar: true);
            return "concluída, paga e entregue";
        }

        if (cenario < 0.90)
        {
            await PrepararEIniciarAsync(provider, rng, osId, componenteIds);
            await ConcluirParteDoChecklistAsync(provider, rng, osId, parcial: false);
            await FinalizarAsync(provider, osId);
            await ConcluirComPagamentoAsync(provider, rng, osId, adminId, entregar: false);
            return "concluída e paga, aguardando retirada";
        }

        // fluxo de aprovação completo (recepção → mecânico → cliente) e entrega
        await AprovarPeloFluxoAsync(provider, osId);
        await AdicionarItensEstoqueAsync(provider, rng, osId, componenteIds, 1, 3);
        await Os(provider, s => s.IniciarAsync(osId));
        await ConcluirParteDoChecklistAsync(provider, rng, osId, parcial: false);
        await FinalizarAsync(provider, osId);
        await ConcluirComPagamentoAsync(provider, rng, osId, adminId, entregar: true);
        return "concluída via fluxo de aprovação completo";
    }

    // ======================= helpers de fluxo =======================

    private static async Task PrepararEIniciarAsync(
        IServiceProvider provider, Random rng, Guid osId, IReadOnlyList<Guid> componenteIds)
    {
        await AdicionarItensEstoqueAsync(provider, rng, osId, componenteIds, 1, 3);

        // ~40% passam pelo fluxo de aprovação; o resto é atalho recepção→início.
        if (rng.NextDouble() < 0.4)
            await AprovarPeloFluxoAsync(provider, osId);

        await Os(provider, s => s.IniciarAsync(osId));
    }

    private static async Task AprovarPeloFluxoAsync(IServiceProvider provider, Guid osId)
    {
        await Os(provider, s => s.EnviarParaRevisaoAsync(osId));
        await Os(provider, s => s.AprovarPeloMecanicoAsync(osId));
        await Os(provider, s => s.RegistrarAprovacaoDoClienteAsync(osId));
    }

    private static async Task AdicionarItensEstoqueAsync(
        IServiceProvider provider, Random rng, Guid osId, IReadOnlyList<Guid> componenteIds, int min, int max)
    {
        if (componenteIds.Count == 0) return;
        var n = rng.Next(min, max + 1);
        for (var k = 0; k < n; k++)
        {
            await Os(provider, s => s.AdicionarItemAsync(new AdicionarItemOrdemServicoDTO
            {
                OrdemServicoId = osId,
                ComponenteId = componenteIds[rng.Next(componenteIds.Count)],
                Quantidade = rng.Next(1, 4),
                Origem = "Estoque"
            }));
        }

        // ~1 em 6 OS também registra uma peça trazida pelo próprio cliente
        if (rng.Next(6) == 0)
        {
            await Os(provider, s => s.AdicionarItemAsync(new AdicionarItemOrdemServicoDTO
            {
                OrdemServicoId = osId,
                Quantidade = 1,
                Origem = "Cliente",
                DescricaoLivre = "Peça trazida pelo cliente"
            }));
        }
    }

    private static async Task ConcluirParteDoChecklistAsync(
        IServiceProvider provider, Random rng, Guid osId, bool parcial)
    {
        List<ChecklistItemDTO> itens;
        using (var scope = provider.CreateScope())
        {
            var servico = scope.ServiceProvider.GetRequiredService<IOrdemServicoService>();
            var detalhe = await servico.GetByIdAsync(osId);
            itens = detalhe.IsSuccess && detalhe.Value is not null
                ? detalhe.Value.Checklist
                : new List<ChecklistItemDTO>();
        }
        if (itens.Count == 0) return;

        // parcial = deixa alguns pendentes (OS não pode finalizar);
        // não-parcial = conclui todos (pré-requisito de Finalizar).
        var alvo = parcial ? rng.Next(0, itens.Count) : itens.Count;
        for (var k = 0; k < alvo; k++)
        {
            var item = itens[k];
            await Os(provider, s => s.AtualizarStatusChecklistAsync(new AtualizarStatusChecklistDTO
            {
                OrdemServicoId = osId, ItemId = item.Id, NovoStatus = "Concluido"
            }));
        }
    }

    private static async Task FinalizarAsync(IServiceProvider provider, Guid osId)
        => await Os(provider, s => s.FinalizarAsync(osId));

    private static async Task ConcluirComPagamentoAsync(
        IServiceProvider provider, Random rng, Guid osId, Guid recebedor, bool entregar)
    {
        // paga o saldo cheio (o service confirma a OS pra Finalizada sozinho)
        using (var scope = provider.CreateScope())
        {
            var pag = scope.ServiceProvider.GetRequiredService<IPagamentoOrdemServicoService>();
            var resumo = await pag.ObterResumoAsync(osId);
            var restante = resumo.IsSuccess && resumo.Value is not null ? resumo.Value.ValorRestante : 0m;

            if (restante > 0m)
            {
                await pag.RegistrarPagamentoAsync(osId, recebedor, new RegistrarPagamentoDTO
                {
                    ModoPagamento = ModosPagamento[rng.Next(ModosPagamento.Length)],
                    Valor = restante,
                    Observacoes = "Pagamento integral registrado na recepção."
                });
            }
        }

        if (entregar)
            await Os(provider, s => s.EntregarAsync(osId));
    }

    private static async Task RegistrarPagamentoParcialAsync(
        IServiceProvider provider, Random rng, Guid osId, Guid recebedor)
    {
        using var scope = provider.CreateScope();
        var pag = scope.ServiceProvider.GetRequiredService<IPagamentoOrdemServicoService>();
        var resumo = await pag.ObterResumoAsync(osId);
        if (!resumo.IsSuccess || resumo.Value is null || resumo.Value.ValorRestante <= 0m) return;

        var parcela = Math.Round(resumo.Value.ValorRestante * (decimal)(0.2 + rng.NextDouble() * 0.5), 2);
        if (parcela <= 0m) return;

        await pag.RegistrarPagamentoAsync(osId, recebedor, new RegistrarPagamentoDTO
        {
            ModoPagamento = ModosPagamento[rng.Next(ModosPagamento.Length)],
            Valor = parcela,
            Observacoes = "Entrada parcial — saldo a receber na retirada."
        });
    }

    private static async Task<Guid?> AbrirRequisicaoAsync(
        IServiceProvider provider, Random rng, Guid osId, Guid mecanicoId)
    {
        using var scope = provider.CreateScope();
        var servico = scope.ServiceProvider.GetRequiredService<IRequisicaoPecaService>();
        var r = await servico.AbrirAsync(osId, mecanicoId, new CriarRequisicaoPecaDTO
        {
            DescricaoPeca = PecasRequisitadas[rng.Next(PecasRequisitadas.Length)],
            Justificativa = "Peça não disponível no estoque atual — necessária para fechar o orçamento.",
            Quantidade = rng.Next(1, 3)
        });
        return r.IsSuccess ? r.Value!.Id : null;
    }

    private static async Task PausarPorAlertaAsync(IServiceProvider provider, Guid osId, Guid mecanicoId)
    {
        using var scope = provider.CreateScope();
        var servico = scope.ServiceProvider.GetRequiredService<IAlertaOSService>();
        await servico.EmitirAsync(osId, mecanicoId, new CriarAlertaOSDTO
        {
            Descricao = "Durante o serviço identifiquei outro problema que aumenta o escopo — preciso da decisão do cliente."
        });
    }

    private static async Task RetomarAlertaAsync(IServiceProvider provider, Guid osId, Guid resolvidoPor)
    {
        using var scope = provider.CreateScope();
        var servico = scope.ServiceProvider.GetRequiredService<IAlertaOSService>();
        var lista = await servico.ListarPorOrdemAsync(osId);
        var pendente = lista.IsSuccess
            ? lista.Value!.FirstOrDefault(a => a.Status == "Pendente")
            : null;
        if (pendente is null) return;

        await servico.ResolverAsync(pendente.Id, resolvidoPor, new ResolverAlertaDTO
        {
            Aprovou = true,
            ObservacaoCliente = "Cliente aprovou o serviço adicional."
        });
    }

    private static async Task Os(IServiceProvider provider, Func<IOrdemServicoService, Task> acao)
    {
        using var scope = provider.CreateScope();
        var servico = scope.ServiceProvider.GetRequiredService<IOrdemServicoService>();
        try { await acao(servico); } catch { /* cenário best-effort */ }
    }

    private static async Task Req(IServiceProvider provider, Func<IRequisicaoPecaService, Task> acao)
    {
        using var scope = provider.CreateScope();
        var servico = scope.ServiceProvider.GetRequiredService<IRequisicaoPecaService>();
        try { await acao(servico); } catch { }
    }
}
