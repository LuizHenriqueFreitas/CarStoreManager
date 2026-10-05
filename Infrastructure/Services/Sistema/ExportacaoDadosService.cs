using System.Text.Json;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Shared.Cliente;
using CarStoreManager.Application.DTOs.Sistema.Importacao;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarStoreManager.Infrastructure.Services.Sistema;

/// <summary>
/// (Contrato v2 — PLANO_BASE_DEMO §8/§8.2.) Exporta status exatos de OS,
/// proposta, consignação e test drive; pagamentos com modo/percentual/data;
/// alerta com decisão e requisição rejeitada; vistoria/termos com texto e
/// assinatura; checklist explícito e preço histórico dos itens da OS;
/// componentes compatíveis; balanços (fechamento, variação e ausência dos
/// itens do modelo) e TODAS as despesas lançadas — com
/// <c>despesasAutomaticasNoArquivo=true</c>, pro importador descartar as
/// despesas que as regras lançariam de novo (round-trip sem duplicar nem
/// empilhar no mês corrente). Perdas que restam: senha (hash), fotos,
/// permissões/configuração, reagendamento de test drive, preset de checklist
/// e descrição das requisições atendidas/pendentes.
///
/// Monta um <see cref="ImportacaoDadosDTO"/> a partir do estado atual do
/// banco — mesmo formato que <c>ImportacaoDadosService.ImportarAsync</c>
/// consome — e serializa em JSON. Cada registro usa o próprio Guid (como
/// texto) como "chave", o que resolve as referências entre seções sem
/// precisar inventar nomes: o importador só enxerga uma string opaca de
/// qualquer forma. "Cenario" é reconstruído a partir do Status atual de cada
/// entidade, sempre escolhendo o valor mais próximo alcançável pelo funil
/// real do importador (ver `ImportacaoDadosService.AvancarFunil*Async`) — em
/// alguns pontos isso é necessariamente perdoso (ex.: senha de usuário é um
/// hash unidirecional, o histórico de reposição de estoque não é guardado
/// separado do saldo atual, o preset de checklist usado na criação da OS não
/// fica registrado na própria OS) — documentado em cada trecho.
/// </summary>
public sealed class ExportacaoDadosService : IExportacaoDadosService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ExportacaoDadosService> _logger;

    private static readonly JsonSerializerOptions Opcoes = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public ExportacaoDadosService(AppDbContext db, ILogger<ExportacaoDadosService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<ExportacaoDadosArquivo>> ExportarJsonAsync(CancellationToken ct = default)
    {
        try
        {
            _db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var dados = new ImportacaoDadosDTO
            {
                Usuarios = await ExportarUsuariosAsync(ct),
                Clientes = await ExportarClientesAsync(ct),
                Fornecedores = await ExportarFornecedoresAsync(ct),
                Componentes = await ExportarComponentesAsync(ct),
                ComponentesEquivalentes = await ExportarComponentesEquivalentesAsync(ct),
                ChecklistPresets = await ExportarChecklistPresetsAsync(ct),
                TemplatesDocumento = await ExportarTemplatesDocumentoAsync(ct),
                Despesas = await ExportarDespesasAsync(ct),
                VeiculosVenda = await ExportarVeiculosVendaAsync(ct),
                VeiculosConsignados = await ExportarVeiculosConsignadosAsync(ct),
                VeiculosCliente = await ExportarVeiculosClienteAsync(ct),
                PropostasVenda = await ExportarPropostasVendaAsync(ct),
                OrdensServico = await ExportarOrdensServicoAsync(ct),
                TestDrives = await ExportarTestDrivesAsync(ct),
                DespesasExtras = new(),
                DespesasAutomaticasNoArquivo = true,
            };
            (dados.DespesasExtras, dados.FechamentosMensais) = await ExportarBalancosAsync(ct);

            // Consignado vendido por proposta: quem leva a consignação a
            // Vendida/Concluída na reimportação é o funil da própria proposta.
            var consignadosViaProposta = dados.PropostasVenda
                .Where(p => !string.IsNullOrEmpty(p.VeiculoConsignadoChave)
                    && p.Cenario is not ("criada" or "rejeitada" or "financiamentoNegado" or "cancelada" or "aguardandoFinanciadora" or "respostaFinanciadora"))
                .Select(p => p.VeiculoConsignadoChave!)
                .ToHashSet();
            foreach (var c in dados.VeiculosConsignados.Where(c => consignadosViaProposta.Contains(c.Chave)))
                c.Cenario = "ativa";

            var json = JsonSerializer.SerializeToUtf8Bytes(dados, Opcoes);
            var nome = $"delore-backup-{DateTime.Now:yyyy-MM-dd-HHmm}.json";
            return Result<ExportacaoDadosArquivo>.Ok(new ExportacaoDadosArquivo(nome, json));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao exportar o banco de dados.");
            return Result<ExportacaoDadosArquivo>.Fail("Não foi possível gerar o arquivo de exportação.");
        }
    }

    // ============================================================
    // USUÁRIOS
    // ============================================================
    private async Task<List<UsuarioImportDTO>> ExportarUsuariosAsync(CancellationToken ct)
    {
        var usuarios = await _db.Set<Usuario>().AsNoTracking().ToListAsync(ct);

        return usuarios.Select(u =>
        {
            var dto = new UsuarioImportDTO
            {
                Chave = u.Id.ToString(),
                Tipo = u.Role.ToString(),
                Nome = u.Nome,
                Email = u.Email.Endereco,
                Telefone = u.Telefone.GetTelefone(),
                // Hash de senha é unidirecional (BCrypt) — não há como recuperar o
                // texto original. Na reimportação, ficar sem Senha usa a senha
                // padrão de demonstração "Teste@123" (ver UsuarioImportDTO.Senha).
                Senha = null,
            };

            switch (u)
            {
                case Vendedor vendedor:
                    dto.Nivel = vendedor.DadosFuncionario.GetNivel().ToString();
                    dto.DataContratacao = vendedor.DadosFuncionario.GetDataContratacao();
                    break;
                case Mecanico mecanico:
                    dto.Nivel = mecanico.DadosFuncionario.GetNivel().ToString();
                    dto.Especialidade = mecanico.Especialidade.ToString();
                    dto.DataContratacao = mecanico.DadosFuncionario.GetDataContratacao();
                    break;
                case Recepcionista recepcionista:
                    dto.Nivel = recepcionista.DadosFuncionario.GetNivel().ToString();
                    dto.DataContratacao = recepcionista.DadosFuncionario.GetDataContratacao();
                    break;
                case ChefeOficina chefe:
                    dto.Nivel = chefe.DadosFuncionario.GetNivel().ToString();
                    dto.DataContratacao = chefe.DadosFuncionario.GetDataContratacao();
                    break;
                case GerenteVendas gerente:
                    dto.Nivel = gerente.DadosFuncionario.GetNivel().ToString();
                    dto.DataContratacao = gerente.DadosFuncionario.GetDataContratacao();
                    break;
                    // Admin não tem DadosFuncionario — Nivel/DataContratacao ficam nulos.
            }

            return dto;
        }).ToList();
    }

    // ============================================================
    // CLIENTES / FORNECEDORES / ENDEREÇO
    // ============================================================
    private async Task<List<ClienteImportDTO>> ExportarClientesAsync(CancellationToken ct)
    {
        var clientes = await _db.Clientes.AsNoTracking().Include(c => c.Endereco).ToListAsync(ct);
        return clientes.Select(c => new ClienteImportDTO
        {
            Chave = c.Id.ToString(),
            Nome = c.Nome,
            Cpf = c.Cpf.Numero,
            Telefone = c.Telefone.GetTelefone(),
            Email = c.Email.Endereco,
            Endereco = MapearEndereco(c.Endereco),
            DataCriacao = c.DataCriacao,
        }).ToList();
    }

    private async Task<List<FornecedorImportDTO>> ExportarFornecedoresAsync(CancellationToken ct)
    {
        var fornecedores = await _db.Fornecedores.AsNoTracking().Include(f => f.Endereco).ToListAsync(ct);
        return fornecedores.Select(f => new FornecedorImportDTO
        {
            Chave = f.Id.ToString(),
            Nome = f.Nome,
            Cnpj = f.Cnpj.Numero,
            Email = f.Email,
            Telefone = f.Telefone,
            Endereco = f.Endereco is null ? null : MapearEndereco(f.Endereco),
        }).ToList();
    }

    private static EnderecoDTO MapearEndereco(Endereco endereco) => new()
    {
        Logradouro = endereco.Logradouro,
        Numero = endereco.Numero,
        Complemento = endereco.Complemento,
        Bairro = endereco.Bairro,
        Cidade = endereco.Cidade,
        Uf = endereco.Uf,
        Cep = endereco.Cep,
    };

    // ============================================================
    // COMPONENTES (ESTOQUE DA OFICINA)
    // ============================================================
    private async Task<List<ComponenteImportDTO>> ExportarComponentesAsync(CancellationToken ct)
    {
        var componentes = await _db.Componentes.AsNoTracking().ToListAsync(ct);
        var estoques = await _db.EstoqueComponentes.AsNoTracking().ToDictionaryAsync(e => e.PecaId, ct);

        return componentes.Select(c =>
        {
            estoques.TryGetValue(c.Id, out var estoque);
            return new ComponenteImportDTO
            {
                Chave = c.Id.ToString(),
                FornecedorChave = c.FornecedorId.ToString(),
                SKUInterno = c.SKUInterno,
                Nome = c.Nome,
                Descricao = c.Descricao,
                MarcaFabricante = c.MarcaFabricante,
                PartNumber = c.PartNumber,
                CodigoOEM = c.CodigoOEM,
                CodigoBarras = c.CodigoBarras,
                NCM = c.NCM,
                CEST = c.CEST,
                Categoria = c.Categoria,
                Unidade = c.Unidade,
                Sistema = c.Sistema?.ToString(),
                Peso = c.Peso,
                GarantiaDias = c.GarantiaDias,
                CustoUnitario = c.CustoUnitario,
                MargemLucroPct = c.MargemLucroPct,
                // O histórico de reposições não fica registrado separado do saldo
                // atual — só dá pra exportar o total líquido como uma entrada única.
                QuantidadeEstoque = estoque?.QuantidadeAtual ?? 0,
                QuantidadeMinima = estoque?.QuantidadeMinima ?? 5,
            };
        }).ToList();
    }

    private async Task<List<ComponenteEquivalenteImportDTO>> ExportarComponentesEquivalentesAsync(CancellationToken ct)
    {
        var ligacoes = await _db.ComponentesEquivalentes.AsNoTracking().ToListAsync(ct);
        return ligacoes.Select(l => new ComponenteEquivalenteImportDTO
        {
            ComponenteChave = l.ComponenteOriginalId.ToString(),
            EquivalenteChave = l.ComponenteEquivalenteId.ToString(),
            TipoEquivalencia = l.TipoEquivalencia.ToString(),
        }).ToList();
    }

    // ============================================================
    // CHECKLIST PRESETS / TEMPLATES DE DOCUMENTO
    // ============================================================
    private async Task<List<ChecklistPresetImportDTO>> ExportarChecklistPresetsAsync(CancellationToken ct)
    {
        var presets = await _db.ChecklistPresets.AsNoTracking().Include(p => p.Itens).ToListAsync(ct);
        return presets.Select(p => new ChecklistPresetImportDTO
        {
            Chave = p.Id.ToString(),
            Nome = p.Nome,
            Itens = p.Itens.OrderBy(i => i.Ordem).Select(i => i.Descricao).ToList(),
        }).ToList();
    }

    private async Task<List<TemplateDocumentoImportDTO>> ExportarTemplatesDocumentoAsync(CancellationToken ct)
    {
        var templates = await _db.TemplatesDocumento.AsNoTracking().ToListAsync(ct);
        return templates.Select(t => new TemplateDocumentoImportDTO
        {
            Chave = t.Id.ToString(),
            Nome = t.Nome,
            Conteudo = t.Conteudo,
        }).ToList();
    }

    // ============================================================
    // DESPESAS (MODELO RECORRENTE) E DESPESAS EXTRAS (LANÇAMENTOS
    // PONTUAIS NO BALANÇO MENSAL — SÓ AS QUE NÃO VIERAM DO MODELO)
    // ============================================================
    private async Task<List<DespesaImportDTO>> ExportarDespesasAsync(CancellationToken ct)
    {
        // Inclui as inativas (Ativa=false) — o importador cria e desativa.
        var despesas = await _db.Despesas.AsNoTracking().ToListAsync(ct);
        return despesas.Select(d => new DespesaImportDTO
        {
            Nome = d.Nome,
            Valor = d.Valor.GetValorDinheiro(),
            Ativa = d.Ativa,
            Setor = d.Setor.ToString(),
            Tipo = d.Tipo.ToString(),
            Categoria = d.Categoria,
        }).ToList();
    }

    /// <summary>
    /// Balanços → (despesasExtras, fechamentosMensais). Cada competência com
    /// balanço vira um fechamento (regera do modelo na reimportação, com
    /// variação dos valores, remoção dos itens do modelo que não existiam e
    /// fechamento/data). Todo item fora do modelo ATIVO — inclusive as
    /// despesas automáticas (compra de veículo/componente, combustível de
    /// test drive) — vai como despesa extra na própria competência.
    /// </summary>
    private async Task<(List<DespesaExtraImportDTO>, List<FechamentoMensalImportDTO>)> ExportarBalancosAsync(CancellationToken ct)
    {
        var modelo = (await _db.Despesas.AsNoTracking().Where(d => d.Ativa).ToListAsync(ct))
            .GroupBy(d => d.Nome).ToDictionary(g => g.Key, g => g.First().Valor.GetValorDinheiro());
        var balancos = (await _db.BalancosMensaisDespesa.AsNoTracking().Include(b => b.Itens).ToListAsync(ct))
            .OrderBy(b => b.Competencia).ToList();

        var extras = new List<DespesaExtraImportDTO>();
        var fechamentos = new List<FechamentoMensalImportDTO>();
        foreach (var balanco in balancos)
        {
            var data = balanco.Competencia.ToDateTime(new TimeOnly(12, 0));
            var fechamento = new FechamentoMensalImportDTO
            {
                Ano = balanco.Competencia.Year,
                Mes = balanco.Competencia.Month,
                Fechar = balanco.Fechado,
                DataFechamento = balanco.DataFechamento,
                RemoverItensModelo = new(),
            };

            var doModeloPresentes = new HashSet<string>();
            foreach (var item in balanco.Itens)
            {
                var valor = item.Valor.GetValorDinheiro();
                if (item.DoModelo && modelo.TryGetValue(item.Nome, out var valorModelo) && doModeloPresentes.Add(item.Nome))
                {
                    if (valor != valorModelo)
                        fechamento.Variacoes.Add(new VariacaoDespesaImportDTO { Nome = item.Nome, Valor = valor });
                    continue;
                }

                extras.Add(new DespesaExtraImportDTO
                {
                    Data = data,
                    Nome = item.Nome,
                    Valor = valor,
                    Setor = item.Setor.ToString(),
                    Categoria = item.Categoria,
                });
            }

            fechamento.RemoverItensModelo.AddRange(modelo.Keys.Where(n => !doModeloPresentes.Contains(n)));
            fechamentos.Add(fechamento);
        }

        return (extras, fechamentos);
    }

    // ============================================================
    // VEÍCULOS (CONCESSIONÁRIA)
    // ============================================================
    private async Task<List<VeiculoVendaImportDTO>> ExportarVeiculosVendaAsync(CancellationToken ct)
    {
        var veiculos = await _db.VeiculosVenda.AsNoTracking().ToListAsync(ct);
        return veiculos.Select(v => new VeiculoVendaImportDTO
        {
            Chave = v.Id.ToString(),
            Marca = v.Marca,
            Modelo = v.Modelo,
            Cor = v.Cor,
            Motorizacao = v.Motorizacao,
            Ano = v.GetAno(),
            Quilometragem = v.GetQuilometragem(),
            Placa = v.GetPlacaCarro(),
            Renavam = v.GetRenavam(),
            Cambio = v.Cambio.ToString(),
            Combustivel = v.Combustivel.ToString(),
            Valor = v.GetValor(),
            ValorAquisicao = v.GetValorAquisicao(),
            Acessorios = v.GetAcessoriosLista(),
            AnoUltimoIpvaPago = v.AnoUltimoIpvaPago,
            TextoTermoPreliminar = v.TextoTermoPreliminar,
            DataCriacao = v.DataCriacao,
            // "Vendido"/"Reservado" não têm cenário próprio no importador — o
            // carro precisa estar Disponivel pra uma proposta poder referenciá-lo;
            // se a venda também for exportada (PropostasVenda), o funil da
            // proposta reconduz o veículo a Vendido na reimportação.
            Cenario = v.Disponibilidade == DisponibilidadeVeiculo.EmPreparacao ? "emPreparacao" : "disponivel",
        }).ToList();
    }

    private async Task<List<VeiculoConsignacaoImportDTO>> ExportarVeiculosConsignadosAsync(CancellationToken ct)
    {
        var veiculos = await _db.VeiculosConsignacao.AsNoTracking().ToListAsync(ct);
        var historico = (await _db.HistoricosConsignacao.AsNoTracking().ToListAsync(ct))
            .GroupBy(h => h.VeiculoConsignacaoId).ToDictionary(g => g.Key, g => g.OrderBy(h => h.DataCriacao).ToList());
        DateTime? Evento(Guid id, TipoEventoConsignacao tipo, bool ultimo = false)
        {
            if (!historico.TryGetValue(id, out var eventos)) return null;
            var doTipo = eventos.Where(e => e.TipoEvento == tipo).ToList();
            return doTipo.Count == 0 ? null : (ultimo ? doTipo[^1] : doTipo[0]).DataCriacao;
        }
        return veiculos.Select(v => new VeiculoConsignacaoImportDTO
        {
            Chave = v.Id.ToString(),
            Marca = v.Marca,
            Modelo = v.Modelo,
            Cor = v.Cor,
            Motorizacao = v.Motorizacao,
            Ano = v.GetAno(),
            Quilometragem = v.GetQuilometragem(),
            Placa = v.GetPlacaCarro(),
            Renavam = v.GetRenavam(),
            Cambio = v.Cambio.ToString(),
            Combustivel = v.Combustivel.ToString(),
            Acessorios = v.GetAcessoriosLista(),
            ClienteProprietarioChave = v.ClienteProprietarioId.ToString(),
            VendedorResponsavelChave = v.VendedorResponsavelId.ToString(),
            TipoComissao = v.Comissao.Tipo.ToString(),
            ValorVendaEsperado = v.Comissao.ValorVendaEsperado.GetValorDinheiro(),
            ValorFixoProprietario = v.Comissao.ValorFixoProprietario?.GetValorDinheiro(),
            PorcentagemProprietario = v.Comissao.PorcentagemProprietario?.GetDescontoValor(),
            TextoContrato = v.TextoContrato,
            PrazoDias = Math.Max(1, (v.DataVencimento - v.DataInicio).Days),
            DataCriacao = v.DataCriacao,
            Cenario = CenarioConsignacao(v.Status),
            DataVenda = Evento(v.Id, TipoEventoConsignacao.Venda),
            DataConclusao = v.Status == StatusConsignacao.Concluida ? Evento(v.Id, TipoEventoConsignacao.MudancaStatus, ultimo: true) : null,
            DataDevolucao = Evento(v.Id, TipoEventoConsignacao.Devolucao),
            DataCancelamento = Evento(v.Id, TipoEventoConsignacao.Cancelamento),
            MotivoCancelamento = historico.TryGetValue(v.Id, out var hs)
                ? hs.LastOrDefault(e => e.TipoEvento == TipoEventoConsignacao.Cancelamento)?.Descricao : null,
        }).ToList();
    }

    private static string CenarioConsignacao(StatusConsignacao status) => status switch
    {
        StatusConsignacao.VendidoAguardandoPagamento => "vendidaAguardando",
        StatusConsignacao.Concluida => "concluida",
        StatusConsignacao.Devolvida => "devolvida",
        StatusConsignacao.Cancelada => "cancelada",
        // Ativa e Expirada — não há um cenário de "expirar" à parte no importador.
        _ => "ativa",
    };

    // ============================================================
    // VEÍCULO DE CLIENTE (OFICINA)
    // ============================================================
    private async Task<List<VeiculoClienteImportDTO>> ExportarVeiculosClienteAsync(CancellationToken ct)
    {
        var veiculos = await _db.VeiculosCliente.AsNoTracking().ToListAsync(ct);
        return veiculos.Select(v => new VeiculoClienteImportDTO
        {
            Chave = v.Id.ToString(),
            ClienteChave = v.ClienteId.ToString(),
            Marca = v.Marca,
            Modelo = v.Modelo,
            Cor = v.Cor,
            Ano = v.GetAno(),
            Placa = v.Placa.GetPlaca(),
        }).ToList();
    }

    // ============================================================
    // PROPOSTAS DE VENDA
    // ============================================================
    private const string PrefixoNegativaFinanciadora = "Financiadora não aprovou o financiamento: ";

    private async Task<List<PropostaVendaImportDTO>> ExportarPropostasVendaAsync(CancellationToken ct)
    {
        // Ordem de criação: um veículo com proposta cancelada e depois vendida
        // precisa ser reencenado na mesma ordem.
        var propostas = await _db.PropostasVenda.AsNoTracking().OrderBy(p => p.DataCriacao).ToListAsync(ct);
        var termos = await _db.TermosEntrega.AsNoTracking().ToDictionaryAsync(t => t.PropostaVendaId, ct);
        var vistorias = (await _db.Vistorias.AsNoTracking().ToListAsync(ct))
            .GroupBy(v => v.PropostaVendaId).ToDictionary(g => g.Key, g => g.OrderBy(v => v.DataRealizada).Last());
        var pagamentos = (await _db.PagamentosProposta.AsNoTracking().ToListAsync(ct))
            .GroupBy(p => p.PropostaVendaId).ToDictionary(g => g.Key, g => g.OrderBy(p => p.DataPagamento).ToList());

        return propostas.Select(p =>
        {
            termos.TryGetValue(p.Id, out var termo);
            vistorias.TryGetValue(p.Id, out var vistoria);
            var pags = pagamentos.GetValueOrDefault(p.Id) ?? new();
            var valorFinal = p.ValorFinal.GetValorDinheiro();
            var cenario = CenarioProposta(p, termo, pags.Count);
            var negado = p.Status == StatusPropostaVenda.Rejeitada && (p.MotivoRejeicao?.StartsWith(PrefixoNegativaFinanciadora) ?? false);

            return new PropostaVendaImportDTO
            {
                Chave = p.Id.ToString(),
                VeiculoVendaChave = p.IsConsignado ? "" : p.VeiculoVendaId.ToString(),
                VeiculoConsignadoChave = p.IsConsignado ? p.VeiculoVendaId.ToString() : null,
                ClienteChave = p.ClienteId.ToString(),
                VendedorChave = p.VendedorId.ToString(),
                ValorBase = p.ValorBase.GetValorDinheiro(),
                DescontoPercentual = p.Desconto.GetDescontoValor(),
                ModoPagamento = p.ModoPagamento == ModoPagamento.NaoDefinido ? null : p.ModoPagamento.ToString(),
                ValorEntrada = p.Entrada.GetValorDinheiro() > 0 ? p.Entrada.GetValorDinheiro() : null,
                TextoPropostaFinanciadora = p.PropostaFinanciadoraTexto,
                MotivoRejeicao = negado ? p.MotivoRejeicao![PrefixoNegativaFinanciadora.Length..] : p.MotivoRejeicao,
                MotivoCancelamento = p.MotivoCancelamento,
                DataCriacao = p.DataCriacao,
                DataAprovacao = p.DataAprovacao,
                Cenario = cenario,
                Pagamentos = pags.Select(x => new PagamentoImportDTO
                {
                    Modo = x.ModoPagamento.ToString(),
                    Percentual = valorFinal > 0 ? x.Valor.GetValorDinheiro() / valorFinal * 100m : 0m,
                    Data = x.DataPagamento,
                }).ToList(),
                DataVistoria = vistoria?.DataRealizada,
                ObservacoesVistoria = string.IsNullOrWhiteSpace(vistoria?.Observacoes) ? null : vistoria!.Observacoes,
                TextoTermo = termo?.TextoTermo,
                DataTermo = termo?.DataRedacao,
                DataAssinatura = termo?.DataAssinatura,
                AssinaturaNome = termo?.AssinaturaNomeCliente,
                AssinaturaCpf = termo?.AssinaturaCpfCliente,
            };
        }).ToList();
    }

    private static string CenarioProposta(PropostaVenda p, TermoEntrega? termo, int qtdPagamentos) => p.Status switch
    {
        StatusPropostaVenda.Rejeitada =>
            p.MotivoRejeicao?.StartsWith(PrefixoNegativaFinanciadora) == true ? "financiamentoNegado" : "rejeitada",
        StatusPropostaVenda.Cancelada => "cancelada",
        StatusPropostaVenda.AguardandoFinanciadora => "aguardandoFinanciadora",
        StatusPropostaVenda.PropostaFinanciadoraRecebida => "respostaFinanciadora",
        StatusPropostaVenda.Aprovada => "aprovada",
        StatusPropostaVenda.AguardandoVistoria => "aguardandoVistoria",
        StatusPropostaVenda.VistoriaConcluida =>
            termo is not null ? "termoRedigido" : qtdPagamentos > 0 ? "pagamentoParcial" : "vistoriada",
        StatusPropostaVenda.AguardandoAssinaturaTermo => "termoEnviado",
        StatusPropostaVenda.Concluida =>
            p.ModoPagamento == ModoPagamento.Financiamento ? "concluidaFinanciada" : "concluidaAVista",
        // Criada / Rascunho / Enviada (legado) / Expirada — Expirada volta a
        // expirar sozinha na 1ª leitura (DataCriacao antiga).
        _ => "criada",
    };

    // ============================================================
    // ORDENS DE SERVIÇO
    // ============================================================
    private async Task<List<OrdemServicoImportDTO>> ExportarOrdensServicoAsync(CancellationToken ct)
    {
        var ordens = await _db.OrdensServico.AsNoTracking().Include(o => o.Itens).Include(o => o.Checklist)
            .OrderBy(o => o.DataCriacao).ToListAsync(ct);
        var pagamentos = (await _db.PagamentosOrdemServico.AsNoTracking().ToListAsync(ct))
            .GroupBy(p => p.OrdemServicoId).ToDictionary(g => g.Key, g => g.OrderBy(p => p.DataPagamento).ToList());
        var alertas = (await _db.AlertasOS.AsNoTracking().ToListAsync(ct))
            .GroupBy(a => a.OrdemServicoId).ToDictionary(g => g.Key, g => g.OrderBy(a => a.DataCriacao).Last());
        var rejeitadas = (await _db.RequisicoesPeca.AsNoTracking().Where(r => r.Status == StatusRequisicaoPeca.Rejeitada).ToListAsync(ct))
            .GroupBy(r => r.OrdemServicoId).ToDictionary(g => g.Key, g => g.OrderBy(r => r.DataCriacao).First());
        var vistorias = await _db.VistoriasOrdemServico.AsNoTracking().ToDictionaryAsync(v => v.OrdemServicoId, ct);

        return ordens.Select(os =>
        {
            var pags = pagamentos.GetValueOrDefault(os.Id) ?? new();
            alertas.TryGetValue(os.Id, out var alerta);
            rejeitadas.TryGetValue(os.Id, out var rejeitada);
            vistorias.TryGetValue(os.Id, out var vistoria);
            var total = os.ValorTotal.GetValorDinheiro();
            var checklistIniciado = os.Checklist.Any(c => c.Status != StatusChecklistItem.Pendente);

            return new OrdemServicoImportDTO
            {
                Chave = os.Id.ToString(),
                VeiculoClienteChave = os.VeiculoClienteId.ToString(),
                ClienteChave = os.ClienteId.ToString(),
                MecanicoChave = os.MecanicoId.ToString(),
                RecepcionistaChave = vistoria?.RecepcionistaId.ToString(),
                TextoVistoria = string.IsNullOrWhiteSpace(vistoria?.TextoContrato) ? null : vistoria!.TextoContrato,
                DataVistoria = vistoria?.DataInicio,
                DataAprovacaoCliente = vistoria?.DataConclusao,
                Tipo = os.Tipo.ToString(),
                Descricao = os.Descricao,
                PrazoDiasAPartirDaCriacao = Math.Max(1, (os.PrazoEstimado - os.DataCriacao).Days),
                CustoServico = os.CustoServico.GetValorDinheiro(),
                ModoPagamento = pags.FirstOrDefault()?.ModoPagamento.ToString(),
                DataCriacao = os.DataCriacao,
                Cenario = CenarioOrdemServico(os.Status, vistoria, alerta is not null || checklistIniciado),
                Itens = os.Itens.Select(i => new ItemOrdemServicoImportDTO
                {
                    ComponenteChave = i.ComponenteId?.ToString() ?? "",
                    Quantidade = i.Quantidade,
                    Origem = i.Origem.ToString(),
                    DescricaoLivre = i.DescricaoLivre,
                    ValorUnitario = i.ValorUnitario.GetValorDinheiro(),
                }).ToList(),
                // Checklist explícito (o preset usado na abertura não fica na OS).
                Checklist = os.Checklist.OrderBy(c => c.OrdemExibicao).Select(c => new ChecklistItemImportDTO
                {
                    Descricao = c.Descricao,
                    Concluido = c.Status == StatusChecklistItem.Concluido,
                }).ToList(),
                ChecklistPresetChave = null,
                Alerta = alerta is null ? null : new AlertaImportDTO
                {
                    Descricao = alerta.Descricao,
                    Decisao = alerta.Status switch
                    {
                        StatusAlertaOS.ClienteAprovou => "aprovado",
                        StatusAlertaOS.ClienteRecusou => "recusado",
                        _ => "pendente",
                    },
                    ObservacaoCliente = alerta.ObservacaoCliente,
                    Data = alerta.DataCriacao,
                    DataDecisao = alerta.DataResolucao,
                },
                RequisicaoRejeitada = rejeitada is null ? null : new RequisicaoRejeitadaImportDTO
                {
                    DescricaoPeca = rejeitada.DescricaoPeca,
                    Motivo = rejeitada.ObservacaoAdmin ?? "",
                    Data = rejeitada.DataCriacao,
                },
                Pagamentos = pags.Select(x => new PagamentoImportDTO
                {
                    Modo = x.ModoPagamento.ToString(),
                    Percentual = total > 0 ? x.Valor.GetValorDinheiro() / total * 100m : 0m,
                    Data = x.DataPagamento,
                }).ToList(),
            };
        }).ToList();
    }

    private static string CenarioOrdemServico(StatusOrdemServico status, VistoriaOrdemServico? vistoria, bool trabalhoIniciado) => status switch
    {
        StatusOrdemServico.Pendente => "pendente",
        // Estágios do fluxo com vistoria (doc 32) — sem registro de vistoria
        // (dado legado), o mais próximo reencenável é "pendente".
        StatusOrdemServico.EmVistoria => vistoria is not null ? "emVistoria" : "pendente",
        StatusOrdemServico.AguardandoCliente => vistoria is not null ? "aguardandoCliente" : "pendente",
        StatusOrdemServico.Aprovada => vistoria is not null ? "aprovada" : "pendente",
        StatusOrdemServico.BuscandoPecasParaOrcamento => "aguardandoPeca",
        StatusOrdemServico.EmAndamento => "emAndamento",
        StatusOrdemServico.Pausada => "pausada",
        // PagamentoPendente é legado (FinalizarAsync hoje exige pagamento) —
        // o mais próximo é checklist pronta, ainda EmAndamento.
        StatusOrdemServico.PagamentoPendente => "finalizadaPendente",
        StatusOrdemServico.Finalizada => "finalizadaPaga",
        StatusOrdemServico.Entregue => "entregue",
        StatusOrdemServico.Cancelada =>
            trabalhoIniciado ? "canceladaEmAndamento"
            : vistoria is { Concluida: true } ? "orcamentoRecusado"
            : "cancelada",
        _ => "pendente",
    };

    // ============================================================
    // TEST DRIVES (CONCESSIONÁRIA)
    // ============================================================
    private async Task<List<TestDriveImportDTO>> ExportarTestDrivesAsync(CancellationToken ct)
    {
        var testDrives = await _db.TestDrives.AsNoTracking().OrderBy(t => t.DataHora).ToListAsync(ct);
        var termos = await _db.TermosTestDrive.AsNoTracking().ToDictionaryAsync(t => t.TestDriveId, ct);
        return testDrives.Select(t =>
        {
            termos.TryGetValue(t.Id, out var termo);
            return new TestDriveImportDTO
            {
                Chave = t.Id.ToString(),
                // Consignado (doc 31) vai na chave própria — antes ia como
                // veiculoVendaChave e o registro era descartado na reimportação.
                VeiculoVendaChave = t.IsConsignado ? "" : t.VeiculoVendaId.ToString(),
                VeiculoConsignadoChave = t.IsConsignado ? t.VeiculoVendaId.ToString() : null,
                ClienteChave = t.ClienteId.ToString(),
                VendedorChave = t.VendedorId.ToString(),
                DataHora = t.DataHora,
                Observacao = t.Observacao,
                Cenario = t.Status switch
                {
                    StatusTestDrive.Realizado => "realizado",
                    StatusTestDrive.Cancelado => "cancelado",
                    StatusTestDrive.NaoCompareceu => "naoCompareceu",
                    _ => "agendado",
                },
                Termo = termo?.Status switch
                {
                    StatusTermoTestDrive.Assinado => "assinado",
                    StatusTermoTestDrive.AguardandoAssinatura => "enviado",
                    _ => "rascunho",
                },
            };
        }).ToList();
    }
}
