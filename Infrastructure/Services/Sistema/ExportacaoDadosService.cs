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
                ChecklistPresets = await ExportarChecklistPresetsAsync(ct),
                TemplatesDocumento = await ExportarTemplatesDocumentoAsync(ct),
                Despesas = await ExportarDespesasAsync(ct),
                VeiculosVenda = await ExportarVeiculosVendaAsync(ct),
                VeiculosConsignados = await ExportarVeiculosConsignadosAsync(ct),
                VeiculosCliente = await ExportarVeiculosClienteAsync(ct),
                PropostasVenda = await ExportarPropostasVendaAsync(ct),
                OrdensServico = await ExportarOrdensServicoAsync(ct),
                TestDrives = await ExportarTestDrivesAsync(ct),
                DespesasExtras = await ExportarDespesasExtrasAsync(ct),
            };

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
        var despesas = await _db.Despesas.AsNoTracking().Where(d => d.Ativa).ToListAsync(ct);
        return despesas.Select(d => new DespesaImportDTO
        {
            Nome = d.Nome,
            Valor = d.Valor.GetValorDinheiro(),
            Setor = d.Setor.ToString(),
            Tipo = d.Tipo.ToString(),
        }).ToList();
    }

    // Categorias lançadas automaticamente por VeiculoVendaService.AddAsync e
    // EstoqueService.EntradaAsync a cada veículo/entrada de estoque — reimportar
    // as seções VeiculosVenda/Componentes já as recria sozinho; reexportá-las
    // aqui também duplicaria a despesa (era a causa de despesasExtras quase
    // dobrar num segundo round-trip).
    private static readonly HashSet<string> CategoriasAutoGeradas = new()
    {
        "Compra de veículo",
        "Compra de componentes",
    };

    private async Task<List<DespesaExtraImportDTO>> ExportarDespesasExtrasAsync(CancellationToken ct)
    {
        var balancos = await _db.BalancosMensaisDespesa.AsNoTracking().Include(b => b.Itens).ToListAsync(ct);
        var lista = new List<DespesaExtraImportDTO>();

        foreach (var balanco in balancos)
        {
            var data = balanco.Competencia.ToDateTime(TimeOnly.MinValue);
            // Itens com DoModelo=true nascem de novo quando a seção "Despesas" é
            // reimportada (GerarDoModeloAsync) — reexportá-los duplicaria o gasto.
            foreach (var item in balanco.Itens.Where(i => !i.DoModelo && !CategoriasAutoGeradas.Contains(i.Categoria ?? "")))
            {
                lista.Add(new DespesaExtraImportDTO
                {
                    Data = data,
                    Nome = item.Nome,
                    Valor = item.Valor.GetValorDinheiro(),
                    Setor = item.Setor.ToString(),
                    Categoria = item.Categoria,
                });
            }
        }

        return lista;
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
    private async Task<List<PropostaVendaImportDTO>> ExportarPropostasVendaAsync(CancellationToken ct)
    {
        var propostas = await _db.PropostasVenda.AsNoTracking().ToListAsync(ct);
        var termosPorProposta = await _db.TermosEntrega.AsNoTracking()
            .ToDictionaryAsync(t => t.PropostaVendaId, ct);

        return propostas.Select(p =>
        {
            termosPorProposta.TryGetValue(p.Id, out var termo);
            return new PropostaVendaImportDTO
            {
                Chave = p.Id.ToString(),
                VeiculoVendaChave = p.VeiculoVendaId.ToString(),
                ClienteChave = p.ClienteId.ToString(),
                VendedorChave = p.VendedorId.ToString(),
                ValorBase = p.ValorBase.GetValorDinheiro(),
                DescontoPercentual = p.Desconto.GetDescontoValor(),
                ModoPagamento = p.ModoPagamento == ModoPagamento.NaoDefinido ? null : p.ModoPagamento.ToString(),
                TextoPropostaFinanciadora = p.PropostaFinanciadoraTexto,
                MotivoRejeicao = p.MotivoRejeicao,
                DataCriacao = p.DataCriacao,
                DataAprovacao = p.DataAprovacao,
                Cenario = CenarioProposta(p, termo),
            };
        }).ToList();
    }

    private static string CenarioProposta(PropostaVenda p, TermoEntrega? termo) => p.Status switch
    {
        StatusPropostaVenda.Rejeitada => "rejeitada",
        StatusPropostaVenda.Aprovada => "aprovada",
        StatusPropostaVenda.VistoriaConcluida => "vistoriada",
        StatusPropostaVenda.AguardandoAssinaturaTermo =>
            termo?.Status == StatusTermoEntrega.AguardandoAssinatura ? "termoEnviado" : "termoRedigido",
        StatusPropostaVenda.Concluida =>
            p.ModoPagamento == ModoPagamento.Financiamento ? "concluidaFinanciada" : "concluidaAVista",
        // Rascunho, Criada, Enviada (legado), Cancelada, Expirada, e os estágios
        // intermediários de financiamento (AguardandoFinanciadora,
        // PropostaFinanciadoraRecebida) não têm um cenário resumível seguro —
        // fica em "criada", ponto de partida que nunca tenta uma transição inválida.
        _ => "criada",
    };

    // ============================================================
    // ORDENS DE SERVIÇO
    // ============================================================
    private async Task<List<OrdemServicoImportDTO>> ExportarOrdensServicoAsync(CancellationToken ct)
    {
        var ordens = await _db.OrdensServico.AsNoTracking().Include(o => o.Itens).ToListAsync(ct);
        var pagamentoPorOrdem = await _db.PagamentosOrdemServico.AsNoTracking()
            .GroupBy(p => p.OrdemServicoId)
            .ToDictionaryAsync(g => g.Key, g => g.First(), ct);
        var ordensComAlerta = (await _db.AlertasOS.AsNoTracking().Select(a => a.OrdemServicoId).ToListAsync(ct))
            .ToHashSet();

        return ordens.Select(os =>
        {
            pagamentoPorOrdem.TryGetValue(os.Id, out var pagamento);
            return new OrdemServicoImportDTO
            {
                Chave = os.Id.ToString(),
                VeiculoClienteChave = os.VeiculoClienteId.ToString(),
                ClienteChave = os.ClienteId.ToString(),
                MecanicoChave = os.MecanicoId.ToString(),
                Tipo = os.Tipo.ToString(),
                Descricao = os.Descricao,
                PrazoDiasAPartirDaCriacao = Math.Max(1, (os.PrazoEstimado - os.DataCriacao).Days),
                CustoServico = os.CustoServico.GetValorDinheiro(),
                ModoPagamento = pagamento?.ModoPagamento.ToString(),
                DataCriacao = os.DataCriacao,
                Cenario = CenarioOrdemServico(os.Status),
                // Item com ComponenteId nulo (peça do cliente sem cadastro na
                // oficina) não tem "chave" pra referenciar — não dá pra reexportar.
                Itens = os.Itens
                    .Where(i => i.ComponenteId.HasValue)
                    .Select(i => new ItemOrdemServicoImportDTO
                    {
                        ComponenteChave = i.ComponenteId!.Value.ToString(),
                        Quantidade = i.Quantidade,
                        Origem = i.Origem.ToString(),
                    })
                    .ToList(),
                ComAlerta = ordensComAlerta.Contains(os.Id),
                // O preset de checklist usado na criação não fica registrado na OS
                // (só no momento da criação) — não há como recuperá-lo depois.
                ChecklistPresetChave = null,
            };
        }).ToList();
    }

    private static string CenarioOrdemServico(StatusOrdemServico status) => status switch
    {
        StatusOrdemServico.Pendente => "pendente",
        StatusOrdemServico.Cancelada => "cancelada",
        StatusOrdemServico.PagamentoPendente => "finalizadaPendente",
        StatusOrdemServico.Finalizada => "finalizadaPaga",
        StatusOrdemServico.Entregue => "entregue",
        // EmVistoria, BuscandoPecasParaOrcamento, AguardandoCliente, Aprovada,
        // EmAndamento, Pausada — todos são sub-estágios de "em andamento" no
        // funil do importador, que não os distingue.
        _ => "emAndamento",
    };

    // ============================================================
    // TEST DRIVES (CONCESSIONÁRIA)
    // ============================================================
    private async Task<List<TestDriveImportDTO>> ExportarTestDrivesAsync(CancellationToken ct)
    {
        var testDrives = await _db.TestDrives.AsNoTracking().ToListAsync(ct);
        return testDrives.Select(t => new TestDriveImportDTO
        {
            Chave = t.Id.ToString(),
            VeiculoVendaChave = t.VeiculoVendaId.ToString(),
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
        }).ToList();
    }
}
