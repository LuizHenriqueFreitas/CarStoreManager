using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico.Pagamento;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Geradores.Nucleo;
using Microsoft.Extensions.DependencyInjection;
using static CarStoreManager.Geradores.Entidades.VeiculoClienteGerador;

namespace CarStoreManager.Geradores.Entidades;

/// <summary>
/// Gera ordens de serviço via IOrdemServicoService.AddAsync. Depende de
/// VeiculoCliente (com o Cliente dono correspondente) e de Mecânicos já
/// existentes. Sem itens/peças — a OS nasce só com os dados básicos, igual a
/// uma OS recém aberta pela recepção antes do mecânico montar a lista de peças.
/// </summary>
public static class OrdemServicoGerador
{
    private static readonly string[] Descricoes =
    {
        "Barulho estranho ao frear", "Revisão dos 10.000 km", "Troca de óleo e filtros",
        "Carro não liga pela manhã", "Ar-condicionado não gela", "Vazamento de óleo identificado",
        "Luz de injeção acesa no painel", "Vibração no volante em alta velocidade",
        "Cliente relata consumo elevado de combustível", "Pastilhas de freio gastas",
        "Revisão pré-viagem", "Suspensão fazendo ruído em buracos"
    };

    public static async Task<List<Guid>> GerarAsync(
        IServiceProvider provider,
        int quantidade,
        Random rng,
        IReadOnlyList<VeiculoDoCliente> veiculosClientes,
        IReadOnlyList<Guid> mecanicoIds,
        DateTime? periodoInicio = null,
        DateTime? periodoFim = null)
    {
        if (veiculosClientes.Count == 0 || mecanicoIds.Count == 0)
        {
            Console.WriteLine("  Ordens de serviço: pulado — faltam veículos de cliente ou mecânicos.");
            return new List<Guid>();
        }

        var ids = await ExecutorLote.ExecutarAsync<IOrdemServicoService>(
            provider,
            quantidade,
            (servico, _) => servico.AddAsync(MontarDto(rng, veiculosClientes, mecanicoIds)),
            "Ordens de serviço");

        if (!periodoInicio.HasValue || !periodoFim.HasValue)
            return ids;

        // Espalha as OS pelos últimos ~2 anos e avança boa parte pelo fluxo
        // recepção → mecânico → finalizada, deixando uma fatia em cada estágio
        // (recém-aberta, em andamento, pagamento pendente, finalizada,
        // cancelada) em vez de tudo parado em "Pendente" — isso é o que
        // alimenta os gráficos mensais.
        var finalizadas = 0;
        var pagamentoPendente = 0;
        var emAndamento = 0;
        var canceladas = 0;
        foreach (var id in ids)
        {
            var dataCriacao = BackdateHelper.DataAleatoriaNoPeriodo(rng, periodoInicio.Value, periodoFim.Value);
            var diasPrazo = rng.Next(3, 20);

            await BackdateHelper.AplicarAsync<CarStoreManager.Domain.Entities.Oficina.OrdemServico>(
                provider, id,
                ("DataCriacao", dataCriacao),
                ("PrazoEstimado", dataCriacao.AddDays(diasPrazo)));

            var r = rng.NextDouble();
            using var scope = provider.CreateScope();
            var servico = scope.ServiceProvider.GetRequiredService<IOrdemServicoService>();
            var pagamentoServico = scope.ServiceProvider.GetRequiredService<IPagamentoOrdemServicoService>();

            if (r < 0.10)
            {
                continue; // recém-aberta, ainda Pendente
            }
            if (r < 0.15)
            {
                var rc = await servico.CancelarAsync(id);
                if (rc.IsSuccess) canceladas++;
                continue;
            }

            // Atalho legítimo do domínio: Iniciar() aceita tanto Pendente
            // quanto Aprovada (ver OrdemServico.Iniciar) — não precisa passar
            // pelas etapas intermediárias de revisão pra simular volume.
            var ri = await servico.IniciarAsync(id);
            if (!ri.IsSuccess) continue;

            if (r < 0.30)
            {
                emAndamento++;
                continue; // meio do caminho — mecânico ainda trabalhando
            }

            // Finalizar() sempre cai em PagamentoPendente (nunca direto em
            // Finalizada — ver OrdemServico.Finalizar). Pra boa parte delas,
            // cobra na hora (recepção recebendo o valor cheio), o que já
            // confirma a OS pra Finalizada de verdade via
            // PagamentoOrdemServicoService.RegistrarPagamentoAsync. O resto
            // fica em PagamentoPendente mesmo — cobrança em aberto de verdade,
            // não um artefato de dado incompleto.
            var rf = await servico.FinalizarAsync(id);
            if (!rf.IsSuccess) continue;

            if (r < 0.75)
            {
                var detalhe = await servico.GetByIdAsync(id);
                if (detalhe.IsSuccess && detalhe.Value is not null)
                {
                    var rp = await pagamentoServico.RegistrarPagamentoAsync(id, mecanicoIds[rng.Next(mecanicoIds.Count)],
                        new RegistrarPagamentoDTO
                        {
                            ModoPagamento = new[] { "Dinheiro", "Pix", "CartaoDebito", "CartaoCredito", "Transferencia" }[rng.Next(5)],
                            Valor = detalhe.Value.ValorTotal,
                            Observacoes = "Pagamento integral registrado na recepção."
                        });
                    if (rp.IsSuccess) { finalizadas++; continue; }
                }
            }

            pagamentoPendente++;
        }

        Console.WriteLine(
            $"  Ordens de serviço: {finalizadas} finalizada(s) e paga(s), {pagamentoPendente} com pagamento pendente, " +
            $"{emAndamento} em andamento, {canceladas} cancelada(s), resto pendente");

        return ids;
    }

    private static CriarOrdemServicoDTO MontarDto(
        Random rng, IReadOnlyList<VeiculoDoCliente> veiculosClientes, IReadOnlyList<Guid> mecanicoIds)
    {
        var veiculo = veiculosClientes[rng.Next(veiculosClientes.Count)];
        var tipos = Enum.GetValues<TipoServico>();

        return new CriarOrdemServicoDTO
        {
            VeiculoClienteId = veiculo.VeiculoClienteId,
            ClienteId = veiculo.ClienteId,
            MecanicoId = mecanicoIds[rng.Next(mecanicoIds.Count)],
            Tipo = tipos[rng.Next(tipos.Length)].ToString(),
            Descricao = Descricoes[rng.Next(Descricoes.Length)],
            // OrdemServico exige prazo estritamente no futuro.
            PrazoEstimado = DateTime.UtcNow.AddDays(rng.Next(3, 45)),
            CustoServico = DocumentoUtils.ValorRedondo(rng, 100, 1300, 50),
            Itens = new List<ItemOrdemServicoDTO>()
        };
    }
}
