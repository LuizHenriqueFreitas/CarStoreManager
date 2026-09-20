using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Auth;
using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;
using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda.Pagamento;
using CarStoreManager.Application.DTOs.Concessionaria.TestDrive;
using CarStoreManager.Application.DTOs.Concessionaria.VeiculoConsignacao;
using CarStoreManager.Application.DTOs.Concessionaria.VeiculoVenda;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico.Pagamento;
using CarStoreManager.Application.DTOs.Oficina.VeiculoCliente;
using CarStoreManager.Application.DTOs.Shared.Cliente;
using CarStoreManager.Application.DTOs.Oficina.ChecklistPreset;
using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.DTOs.Sistema.Importacao;
using CarStoreManager.Application.DTOs.Sistema.TemplateDocumento;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Interfaces.Oficina;
using CarStoreManager.Application.Interfaces.Sistema;
using Microsoft.Extensions.Logging;

namespace CarStoreManager.Application.Services.Sistema;

/// <summary>
/// Importa um documento de dados de demonstração (Configurações → Importar
/// dados) criando tudo pelas MESMAS regras de negócio da aplicação real —
/// via Application Services, nunca inserção direta — igual ao projeto
/// Geradores, só que dirigido por um arquivo em vez de aleatoriedade.
///
/// Cada seção é processada em ordem de dependência (usuários → clientes →
/// veículos → propostas/OS), resolvendo as "chaves" de texto do arquivo pros
/// Guids reais criados nesta mesma importação. Um registro que falhar (chave
/// não encontrada, validação de negócio rejeitando os dados) vira um aviso
/// no resultado e a importação segue pros próximos — não aborta tudo por
/// causa de uma linha ruim.
/// </summary>
public class ImportacaoDadosService : IImportacaoDadosService
{
    // PropostaVenda só aceita esses modos além de Financiamento — Dinheiro e
    // cartão ficam de fora por não serem meio de pagamento realista pro valor
    // de um veículo inteiro (ver PropostaVenda.ModosAceitos).
    private static readonly string[] ModosAVista = { "Pix", "Transferencia", "Boleto" };

    private readonly IAuthService _authService;
    private readonly IClienteService _clienteService;
    private readonly IFornecedorService _fornecedorService;
    private readonly IComponenteService _componenteService;
    private readonly IEstoqueService _estoqueService;
    private readonly IChecklistPresetService _checklistPresetService;
    private readonly ITemplateDocumentoService _templateDocumentoService;
    private readonly IDespesaService _despesaService;
    private readonly IVeiculoVendaService _veiculoVendaService;
    private readonly IVeiculoConsignacaoService _veiculoConsignacaoService;
    private readonly IVeiculoClienteService _veiculoClienteService;
    private readonly IPropostaVendaService _propostaService;
    private readonly IPagamentoPropostaService _pagamentoPropostaService;
    private readonly IOrdemServicoService _ordemServicoService;
    private readonly IPagamentoOrdemServicoService _pagamentoOrdemServicoService;
    private readonly IRequisicaoPecaService _requisicaoPecaService;
    private readonly IAlertaOSService _alertaOSService;
    private readonly ITestDriveService _testDriveService;
    private readonly IBalancoMensalDespesaService _balancoDespesaService;
    private readonly IBackdateService _backdate;
    private readonly ILogger<ImportacaoDadosService> _logger;

    public ImportacaoDadosService(
        IAuthService authService,
        IClienteService clienteService,
        IFornecedorService fornecedorService,
        IComponenteService componenteService,
        IEstoqueService estoqueService,
        IChecklistPresetService checklistPresetService,
        ITemplateDocumentoService templateDocumentoService,
        IDespesaService despesaService,
        IVeiculoVendaService veiculoVendaService,
        IVeiculoConsignacaoService veiculoConsignacaoService,
        IVeiculoClienteService veiculoClienteService,
        IPropostaVendaService propostaService,
        IPagamentoPropostaService pagamentoPropostaService,
        IOrdemServicoService ordemServicoService,
        IPagamentoOrdemServicoService pagamentoOrdemServicoService,
        IRequisicaoPecaService requisicaoPecaService,
        IAlertaOSService alertaOSService,
        ITestDriveService testDriveService,
        IBalancoMensalDespesaService balancoDespesaService,
        IBackdateService backdate,
        ILogger<ImportacaoDadosService> logger)
    {
        _authService = authService;
        _clienteService = clienteService;
        _fornecedorService = fornecedorService;
        _componenteService = componenteService;
        _estoqueService = estoqueService;
        _checklistPresetService = checklistPresetService;
        _templateDocumentoService = templateDocumentoService;
        _despesaService = despesaService;
        _veiculoVendaService = veiculoVendaService;
        _veiculoConsignacaoService = veiculoConsignacaoService;
        _veiculoClienteService = veiculoClienteService;
        _propostaService = propostaService;
        _pagamentoPropostaService = pagamentoPropostaService;
        _ordemServicoService = ordemServicoService;
        _pagamentoOrdemServicoService = pagamentoOrdemServicoService;
        _requisicaoPecaService = requisicaoPecaService;
        _alertaOSService = alertaOSService;
        _testDriveService = testDriveService;
        _balancoDespesaService = balancoDespesaService;
        _backdate = backdate;
        _logger = logger;
    }

    public async Task<Result<ImportacaoResultadoDTO>> ImportarAsync(ImportacaoDadosDTO dados)
    {
        var resultado = new ImportacaoResultadoDTO();
        // Mapeia "chave" de texto do arquivo -> Guid real criado nesta importação.
        var chaves = new Dictionary<string, Guid>();

        try
        {
            await ImportarUsuariosAsync(dados.Usuarios, chaves, resultado);
            await ImportarClientesAsync(dados.Clientes, chaves, resultado);
            await ImportarFornecedoresAsync(dados.Fornecedores, chaves, resultado);
            await ImportarComponentesAsync(dados.Componentes, chaves, resultado);
            await ImportarChecklistPresetsAsync(dados.ChecklistPresets, chaves, resultado);
            await ImportarTemplatesDocumentoAsync(dados.TemplatesDocumento, chaves, resultado);
            await ImportarDespesasAsync(dados.Despesas, resultado);
            await ImportarVeiculosVendaAsync(dados.VeiculosVenda, chaves, resultado);
            await ImportarVeiculosConsignadosAsync(dados.VeiculosConsignados, chaves, resultado);
            await ImportarVeiculosClienteAsync(dados.VeiculosCliente, chaves, resultado);
            // TestDrives ANTES de PropostasVenda: propostas concluídas marcam o
            // veículo como Vendido, e agendar test drive de um veículo vendido é
            // bloqueado — então test drive precisa "acontecer" enquanto o carro
            // ainda está disponível, na ordem natural do funil real também.
            await ImportarTestDrivesAsync(dados.TestDrives, chaves, resultado);
            await ImportarPropostasAsync(dados.PropostasVenda, chaves, resultado);
            await ImportarOrdensServicoAsync(dados.OrdensServico, chaves, resultado);
            await ImportarDespesasExtrasAsync(dados.DespesasExtras, resultado);

            return Result<ImportacaoResultadoDTO>.Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha inesperada durante a importação de dados");
            return Result<ImportacaoResultadoDTO>.Fail(
                $"A importação parou no meio do processo por um erro inesperado. Já foram criados {resultado.TotalCriado} registro(s) antes da falha — confira os avisos e o restante do arquivo separadamente.");
        }
    }

    // ============================================================
    // USUÁRIOS
    // ============================================================
    private async Task ImportarUsuariosAsync(
        List<UsuarioImportDTO> itens, Dictionary<string, Guid> chaves, ImportacaoResultadoDTO resultado)
    {
        foreach (var item in itens)
        {
            try
            {
                var dto = new CriarUsuarioDTO
                {
                    Nome = item.Nome,
                    Email = item.Email,
                    Telefone = item.Telefone,
                    Senha = string.IsNullOrWhiteSpace(item.Senha) ? "Teste@123" : item.Senha,
                    Role = item.Tipo,
                    Nivel = item.Nivel,
                    Especialidade = item.Especialidade,
                    // DadosFuncionario exige estritamente DataContratacao > DateTime.Now
                    // (não só a data — a hora exata) — meia-noite de hoje já falharia.
                    // Usa a data real "daqui a pouco" aqui; a histórica (se pedida) é
                    // aplicada depois via backdate.
                    DataContratacao = item.Tipo == "Admin" ? null : DateTime.Now.AddMinutes(5)
                };

                var r = await _authService.CriarUsuarioAsync(dto);
                if (!r.IsSuccess)
                {
                    resultado.Avisos.Add($"Usuário \"{item.Chave}\" ({item.Nome}): {r.Error}");
                    continue;
                }

                RegistrarChave(chaves, item.Chave, r.Value, resultado.Avisos, "usuário");
                if (item.DataContratacao.HasValue)
                    await _backdate.AplicarAsync<Domain.Entities.Usuario>(r.Value, ("DataCriacao", item.DataContratacao.Value));
                resultado.UsuariosCriados++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Usuário \"{item.Chave}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar usuário {Chave}", item.Chave);
            }
        }
    }

    // ============================================================
    // CLIENTES
    // ============================================================
    private async Task ImportarClientesAsync(
        List<ClienteImportDTO> itens, Dictionary<string, Guid> chaves, ImportacaoResultadoDTO resultado)
    {
        foreach (var item in itens)
        {
            try
            {
                var dto = new CriarClienteDTO
                {
                    Nome = item.Nome,
                    Cpf = item.Cpf,
                    Telefone = item.Telefone,
                    Email = item.Email,
                    Endereco = item.Endereco
                };

                var r = await _clienteService.AddAsync(dto);
                if (!r.IsSuccess)
                {
                    resultado.Avisos.Add($"Cliente \"{item.Chave}\" ({item.Nome}): {r.Error}");
                    continue;
                }

                RegistrarChave(chaves, item.Chave, r.Value, resultado.Avisos, "cliente");
                if (item.DataCriacao.HasValue)
                    await _backdate.AplicarAsync<Domain.Entities.Cliente>(r.Value, ("DataCriacao", item.DataCriacao.Value));
                resultado.ClientesCriados++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Cliente \"{item.Chave}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar cliente {Chave}", item.Chave);
            }
        }
    }

    // ============================================================
    // FORNECEDORES
    // ============================================================
    private async Task ImportarFornecedoresAsync(
        List<FornecedorImportDTO> itens, Dictionary<string, Guid> chaves, ImportacaoResultadoDTO resultado)
    {
        foreach (var item in itens)
        {
            try
            {
                var dto = new DTOs.Oficina.Fornecedor.CriarFornecedorDTO
                {
                    Nome = item.Nome,
                    Cnpj = item.Cnpj,
                    Email = item.Email,
                    Telefone = item.Telefone,
                    Endereco = item.Endereco
                };

                var r = await _fornecedorService.AddAsync(dto);
                if (!r.IsSuccess)
                {
                    resultado.Avisos.Add($"Fornecedor \"{item.Chave}\" ({item.Nome}): {r.Error}");
                    continue;
                }

                RegistrarChave(chaves, item.Chave, r.Value, resultado.Avisos, "fornecedor");
                resultado.FornecedoresCriados++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Fornecedor \"{item.Chave}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar fornecedor {Chave}", item.Chave);
            }
        }
    }

    // ============================================================
    // COMPONENTES (ESTOQUE DA OFICINA)
    // ============================================================
    private async Task ImportarComponentesAsync(
        List<ComponenteImportDTO> itens, Dictionary<string, Guid> chaves, ImportacaoResultadoDTO resultado)
    {
        foreach (var item in itens)
        {
            try
            {
                if (!TentarResolver(chaves, item.FornecedorChave, out var fornecedorId, resultado.Avisos, "componente", item.Chave, "fornecedor")) continue;

                var dto = new DTOs.Oficina.Componente.CriarComponenteDTO
                {
                    FornecedorId = fornecedorId,
                    SKUInterno = item.SKUInterno,
                    Nome = item.Nome,
                    Descricao = item.Descricao,
                    MarcaFabricante = item.MarcaFabricante,
                    PartNumber = item.PartNumber,
                    CodigoOEM = item.CodigoOEM ?? "",
                    CodigoBarras = item.CodigoBarras ?? "",
                    NCM = item.NCM,
                    CEST = item.CEST ?? "",
                    Categoria = item.Categoria,
                    Unidade = item.Unidade,
                    Sistema = item.Sistema ?? "",
                    Peso = item.Peso,
                    GarantiaDias = item.GarantiaDias,
                    CustoUnitario = item.CustoUnitario,
                    MargemLucroPct = item.MargemLucroPct
                };

                var r = await _componenteService.AddAsync(dto);
                if (!r.IsSuccess)
                {
                    resultado.Avisos.Add($"Componente \"{item.Chave}\" ({item.Nome}): {r.Error}");
                    continue;
                }

                RegistrarChave(chaves, item.Chave, r.Value, resultado.Avisos, "componente");

                if (item.QuantidadeMinima > 0)
                    await _estoqueService.CriarOuAtualizarMinimoAsync(r.Value, item.QuantidadeMinima);

                if (item.ReposicoesEstoque is { Count: > 0 })
                {
                    // Cada reposição vira sua própria entrada (e sua própria
                    // despesa "Compra de componentes") — move pra competência
                    // histórica ANTES da próxima, senão duas reposições com a
                    // mesma quantidade ficariam ambíguas dentro do balanço de
                    // "hoje" (mesmo nome de despesa).
                    foreach (var rep in item.ReposicoesEstoque)
                    {
                        var rEntrada = await _estoqueService.EntradaAsync(r.Value, rep.Quantidade);
                        if (!rEntrada.IsSuccess)
                        {
                            resultado.Avisos.Add($"Componente \"{item.Chave}\": reposição de {rep.Quantidade} un. falhou — {rEntrada.Error}");
                            continue;
                        }
                        await MoverDespesaParaCompetenciaHistoricaAsync(
                            $"Compra de componente: {item.Nome} — {item.SKUInterno} (x{rep.Quantidade})", rep.Data);
                    }
                }
                else if (item.QuantidadeEstoque > 0)
                {
                    var rEntrada = await _estoqueService.EntradaAsync(r.Value, item.QuantidadeEstoque);
                    if (!rEntrada.IsSuccess)
                        resultado.Avisos.Add($"Componente \"{item.Chave}\": criado, mas a entrada de estoque falhou — {rEntrada.Error}");
                }

                resultado.ComponentesCriados++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Componente \"{item.Chave}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar componente {Chave}", item.Chave);
            }
        }
    }

    // ============================================================
    // CHECKLIST PRESETS (OFICINA)
    // ============================================================
    private async Task ImportarChecklistPresetsAsync(
        List<ChecklistPresetImportDTO> itens, Dictionary<string, Guid> chaves, ImportacaoResultadoDTO resultado)
    {
        foreach (var item in itens)
        {
            try
            {
                var dto = new SalvarChecklistPresetDTO
                {
                    Nome = item.Nome,
                    Ativo = true,
                    Itens = item.Itens
                };

                var r = await _checklistPresetService.AddAsync(dto);
                if (!r.IsSuccess)
                {
                    resultado.Avisos.Add($"Checklist preset \"{item.Chave}\" ({item.Nome}): {r.Error}");
                    continue;
                }

                RegistrarChave(chaves, item.Chave, r.Value, resultado.Avisos, "checklist preset");
                resultado.ChecklistPresetsCriados++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Checklist preset \"{item.Chave}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar checklist preset {Chave}", item.Chave);
            }
        }
    }

    // ============================================================
    // TEMPLATES DE DOCUMENTO (SISTEMA)
    // ============================================================
    private async Task ImportarTemplatesDocumentoAsync(
        List<TemplateDocumentoImportDTO> itens, Dictionary<string, Guid> chaves, ImportacaoResultadoDTO resultado)
    {
        foreach (var item in itens)
        {
            try
            {
                var dto = new SalvarTemplateDocumentoDTO
                {
                    Nome = item.Nome,
                    Conteudo = item.Conteudo,
                    Ativo = true
                };

                var r = await _templateDocumentoService.AddAsync(dto);
                if (!r.IsSuccess)
                {
                    resultado.Avisos.Add($"Template de documento \"{item.Chave}\" ({item.Nome}): {r.Error}");
                    continue;
                }

                RegistrarChave(chaves, item.Chave, r.Value, resultado.Avisos, "template de documento");
                resultado.TemplatesDocumentoCriados++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Template de documento \"{item.Chave}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar template de documento {Chave}", item.Chave);
            }
        }
    }

    // ============================================================
    // DESPESAS (FIXAS MENSAIS — SEM DATA, SEM "CHAVE")
    // ============================================================
    private async Task ImportarDespesasAsync(List<DespesaImportDTO> itens, ImportacaoResultadoDTO resultado)
    {
        foreach (var item in itens)
        {
            try
            {
                var dto = new CriarDespesaDTO
                {
                    Nome = item.Nome,
                    Valor = item.Valor,
                    Setor = item.Setor,
                    Tipo = item.Tipo
                };

                var r = await _despesaService.AddAsync(dto);
                if (!r.IsSuccess)
                {
                    resultado.Avisos.Add($"Despesa \"{item.Nome}\": {r.Error}");
                    continue;
                }

                resultado.DespesasCriadas++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Despesa \"{item.Nome}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar despesa {Nome}", item.Nome);
            }
        }
    }

    // ============================================================
    // VEÍCULOS (CONCESSIONÁRIA)
    // ============================================================
    private async Task ImportarVeiculosVendaAsync(
        List<VeiculoVendaImportDTO> itens, Dictionary<string, Guid> chaves, ImportacaoResultadoDTO resultado)
    {
        foreach (var item in itens)
        {
            try
            {
                var dto = new CriarVeiculoVendaDTO
                {
                    Marca = item.Marca,
                    Modelo = item.Modelo,
                    Cor = item.Cor,
                    Motorizacao = item.Motorizacao,
                    Ano = item.Ano,
                    Quilometragem = item.Quilometragem,
                    Placa = item.Placa,
                    Renavam = item.Renavam,
                    Cambio = item.Cambio,
                    Combustivel = item.Combustivel,
                    Valor = item.Valor,
                    ValorAquisicao = item.ValorAquisicao,
                    Acessorios = item.Acessorios,
                    AnoUltimoIpvaPago = item.AnoUltimoIpvaPago,
                    TextoTermoPreliminar = item.TextoTermoPreliminar
                };

                var r = await _veiculoVendaService.AddAsync(dto);
                if (!r.IsSuccess)
                {
                    resultado.Avisos.Add($"Veículo (concessionária) \"{item.Chave}\" ({item.Marca} {item.Modelo}): {r.Error}");
                    continue;
                }

                if (item.Cenario == "disponivel")
                    await _veiculoVendaService.LiberarParaVendaAsync(r.Value);

                RegistrarChave(chaves, item.Chave, r.Value, resultado.Avisos, "veículo (concessionária)");
                if (item.DataCriacao.HasValue)
                {
                    await _backdate.AplicarAsync<Domain.Entities.Concessionaria.VeiculoVenda>(r.Value, ("DataCriacao", item.DataCriacao.Value));

                    // VeiculoVendaService.AddAsync já lançou a despesa de
                    // "compra de veículo" na competência de HOJE (é o que
                    // acontece de verdade no uso real do sistema) — pra uma
                    // importação de dado histórico, essa despesa precisa
                    // migrar pra competência real da compra, senão o mês
                    // corrente acumula uma "compra de veículo" por veículo
                    // importado e nenhum mês histórico reflete o gasto.
                    var nomeDespesa = $"Compra de veículo para concessionária: {item.Marca} {item.Modelo} — {item.Placa}";
                    await MoverDespesaParaCompetenciaHistoricaAsync(nomeDespesa, item.DataCriacao.Value);
                }
                resultado.VeiculosVendaCriados++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Veículo (concessionária) \"{item.Chave}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar veículo de venda {Chave}", item.Chave);
            }
        }
    }

    /// <summary>
    /// Move um item de despesa (achado pelo nome exato) da competência de
    /// hoje pra uma competência histórica — usado só na importação, pra
    /// corrigir a despesa de "compra de veículo" que
    /// <see cref="VeiculoVendaService.AddAsync"/> lança sempre em hoje (certo
    /// pro uso real do sistema, errado pra um veículo com data histórica).
    /// Silencioso se o item não for encontrado (não deveria acontecer, mas
    /// não é motivo pra abortar a importação).
    /// </summary>
    private async Task MoverDespesaParaCompetenciaHistoricaAsync(string nomeItem, DateTime dataHistorica)
    {
        var hoje = DateTime.Today;
        var rHoje = await _balancoDespesaService.ObterAsync(hoje.Year, hoje.Month);
        if (!rHoje.IsSuccess || rHoje.Value is null) return;

        var item = rHoje.Value.Itens.FirstOrDefault(i => i.Nome == nomeItem);
        if (item is null) return;

        await _balancoDespesaService.RemoverItemAsync(hoje.Year, hoje.Month, item.Id);

        // Garante as despesas recorrentes do modelo na competência histórica
        // antes de somar o item — mesma lógica de VeiculoVendaService
        // .RegistrarDespesaCompraAsync; "já existe" é esperado e ignorado.
        await _balancoDespesaService.GerarDoModeloAsync(dataHistorica.Year, dataHistorica.Month);
        await _balancoDespesaService.SalvarItemAsync(new SalvarItemBalancoDTO
        {
            Ano = dataHistorica.Year,
            Mes = dataHistorica.Month,
            Nome = item.Nome,
            Setor = item.Setor,
            Categoria = item.Categoria,
            Valor = item.Valor
        });
    }

    // ============================================================
    // DESPESAS EXTRAS — variação histórica (compra de equipamento,
    // manutenção fora do comum, perda de veículo etc.), pra meses de
    // gasto alto se destacarem dos meses "só o básico" no histórico.
    // ============================================================
    private async Task ImportarDespesasExtrasAsync(
        List<DespesaExtraImportDTO> itens, ImportacaoResultadoDTO resultado)
    {
        foreach (var item in itens)
        {
            try
            {
                // Garante as despesas recorrentes do modelo nessa competência
                // antes de somar o item extra — "já existe" é esperado
                // (mês pode já ter balanço por causa de uma compra de
                // veículo caindo no mesmo mês) e ignorado.
                await _balancoDespesaService.GerarDoModeloAsync(item.Data.Year, item.Data.Month);

                var r = await _balancoDespesaService.SalvarItemAsync(new SalvarItemBalancoDTO
                {
                    Ano = item.Data.Year,
                    Mes = item.Data.Month,
                    Nome = item.Nome,
                    Setor = item.Setor,
                    Categoria = item.Categoria,
                    Valor = item.Valor
                });
                if (!r.IsSuccess)
                    resultado.Avisos.Add($"Despesa extra \"{item.Nome}\" ({item.Data:MM/yyyy}): {r.Error}");
                else
                    resultado.DespesasExtrasCriadas++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Despesa extra \"{item.Nome}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar despesa extra {Nome}", item.Nome);
            }
        }
    }

    private async Task ImportarVeiculosConsignadosAsync(
        List<VeiculoConsignacaoImportDTO> itens, Dictionary<string, Guid> chaves, ImportacaoResultadoDTO resultado)
    {
        foreach (var item in itens)
        {
            try
            {
                if (!TentarResolver(chaves, item.ClienteProprietarioChave, out var clienteId, resultado.Avisos, "veículo consignado", item.Chave, "cliente proprietário")) continue;
                if (!TentarResolver(chaves, item.VendedorResponsavelChave, out var vendedorId, resultado.Avisos, "veículo consignado", item.Chave, "vendedor responsável")) continue;

                var dto = new CriarVeiculoConsignacaoDTO
                {
                    Marca = item.Marca,
                    Modelo = item.Modelo,
                    Cor = item.Cor,
                    Motorizacao = item.Motorizacao,
                    Ano = item.Ano,
                    Quilometragem = item.Quilometragem,
                    Placa = item.Placa,
                    Renavam = item.Renavam,
                    Cambio = item.Cambio,
                    Combustivel = item.Combustivel,
                    Acessorios = item.Acessorios,
                    ClienteProprietarioId = clienteId,
                    VendedorResponsavelId = vendedorId,
                    TipoComissao = item.TipoComissao,
                    ValorVendaEsperado = item.ValorVendaEsperado,
                    ValorFixoProprietario = item.ValorFixoProprietario,
                    PorcentagemProprietario = item.PorcentagemProprietario,
                    TextoContrato = item.TextoContrato,
                    PrazoDias = item.PrazoDias
                };

                var r = await _veiculoConsignacaoService.AddAsync(dto);
                if (!r.IsSuccess)
                {
                    resultado.Avisos.Add($"Veículo consignado \"{item.Chave}\" ({item.Marca} {item.Modelo}): {r.Error}");
                    continue;
                }

                switch (item.Cenario)
                {
                    case "vendidaAguardando":
                        await _veiculoConsignacaoService.MarcarComoVendidaAsync(r.Value);
                        break;
                    case "concluida":
                        await _veiculoConsignacaoService.MarcarComoVendidaAsync(r.Value);
                        await _veiculoConsignacaoService.ConcluirVendaAsync(r.Value);
                        break;
                    case "devolvida":
                        await _veiculoConsignacaoService.DevolverAsync(r.Value);
                        break;
                    case "cancelada":
                        await _veiculoConsignacaoService.CancelarAsync(r.Value,
                            string.IsNullOrWhiteSpace(item.MotivoCancelamento) ? "Cancelado a pedido do proprietário." : item.MotivoCancelamento);
                        break;
                }

                RegistrarChave(chaves, item.Chave, r.Value, resultado.Avisos, "veículo consignado");
                if (item.DataCriacao.HasValue)
                {
                    var dataVencimento = item.DataCriacao.Value.AddDays(item.PrazoDias);
                    await _backdate.AplicarAsync<Domain.Entities.Concessionaria.VeiculoConsignacao>(
                        r.Value,
                        ("DataCriacao", item.DataCriacao.Value),
                        ("DataInicio", item.DataCriacao.Value),
                        ("DataVencimento", dataVencimento));
                }
                resultado.VeiculosConsignadosCriados++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Veículo consignado \"{item.Chave}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar veículo consignado {Chave}", item.Chave);
            }
        }
    }

    // ============================================================
    // VEÍCULO DE CLIENTE (OFICINA)
    // ============================================================
    private async Task ImportarVeiculosClienteAsync(
        List<VeiculoClienteImportDTO> itens, Dictionary<string, Guid> chaves, ImportacaoResultadoDTO resultado)
    {
        foreach (var item in itens)
        {
            try
            {
                if (!TentarResolver(chaves, item.ClienteChave, out var clienteId, resultado.Avisos, "veículo de cliente", item.Chave, "cliente")) continue;

                var dto = new CriarVeiculoClienteDTO
                {
                    ClienteId = clienteId,
                    Marca = item.Marca,
                    Modelo = item.Modelo,
                    Cor = item.Cor,
                    Ano = item.Ano,
                    Placa = item.Placa
                };

                var r = await _veiculoClienteService.AddAsync(dto);
                if (!r.IsSuccess)
                {
                    resultado.Avisos.Add($"Veículo de cliente \"{item.Chave}\" ({item.Marca} {item.Modelo}): {r.Error}");
                    continue;
                }

                RegistrarChave(chaves, item.Chave, r.Value, resultado.Avisos, "veículo de cliente");
                resultado.VeiculosClienteCriados++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Veículo de cliente \"{item.Chave}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar veículo de cliente {Chave}", item.Chave);
            }
        }
    }

    // ============================================================
    // PROPOSTAS DE VENDA — funil completo, igual ao fluxo real da UI
    // ============================================================
    private async Task ImportarPropostasAsync(
        List<PropostaVendaImportDTO> itens, Dictionary<string, Guid> chaves, ImportacaoResultadoDTO resultado)
    {
        var adminId = await ObterAdminOuVendedorIdAsync(chaves);

        foreach (var item in itens)
        {
            try
            {
                if (!TentarResolver(chaves, item.VeiculoVendaChave, out var veiculoId, resultado.Avisos, "proposta de venda", item.Chave, "veículo")) continue;
                if (!TentarResolver(chaves, item.ClienteChave, out var clienteId, resultado.Avisos, "proposta de venda", item.Chave, "cliente")) continue;
                if (!TentarResolver(chaves, item.VendedorChave, out var vendedorId, resultado.Avisos, "proposta de venda", item.Chave, "vendedor")) continue;

                var dto = new CriarPropostaVendaDTO
                {
                    VendedorId = vendedorId,
                    VeiculoVendaId = veiculoId,
                    ClienteId = clienteId,
                    ValorBase = item.ValorBase,
                    DescontoPercentual = item.DescontoPercentual
                };

                var r = await _propostaService.AddAsync(dto);
                if (!r.IsSuccess)
                {
                    resultado.Avisos.Add($"Proposta \"{item.Chave}\": {r.Error}");
                    continue;
                }

                var propostaId = r.Value;
                await AvancarFunilPropostaAsync(propostaId, item, vendedorId, adminId, resultado.Avisos);

                RegistrarChave(chaves, item.Chave, propostaId, resultado.Avisos, "proposta de venda");
                if (item.DataCriacao.HasValue)
                {
                    var valores = new List<(string, object?)> { ("DataCriacao", item.DataCriacao.Value) };
                    if (item.DataAprovacao.HasValue)
                        valores.Add(("DataAprovacao", item.DataAprovacao.Value));
                    await _backdate.AplicarAsync<Domain.Entities.Concessionaria.PropostaVenda>(propostaId, valores.ToArray());
                }

                resultado.PropostasVendaCriadas++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Proposta \"{item.Chave}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar proposta {Chave}", item.Chave);
            }
        }
    }

    /// <summary>
    /// Roda o funil real da proposta até o cenário pedido, sempre com datas
    /// reais (a proposta expira 7 dias após DataCriacao e todo método de
    /// transição bloqueia proposta expirada) — o redate pra data histórica
    /// acontece só depois, em ImportarPropostasAsync. Retorna até onde
    /// conseguiu chegar de verdade (pode ser antes do pedido, se alguma etapa
    /// falhar).
    /// </summary>
    private async Task<string> AvancarFunilPropostaAsync(
        Guid propostaId, PropostaVendaImportDTO item, Guid vendedorId, Guid adminId, List<string> avisos)
    {
        string Parar(string estagioAlcancado, string etapaQueFalhou, string? erro)
        {
            avisos.Add($"Proposta \"{item.Chave}\": não foi possível {etapaQueFalhou} — {erro ?? "motivo desconhecido"}.");
            return estagioAlcancado;
        }

        if (item.Cenario == "criada") return "criada";

        if (item.Cenario == "rejeitada")
        {
            var rr = await _propostaService.RejeitarAsync(propostaId, string.IsNullOrWhiteSpace(item.MotivoRejeicao) ? "Cliente desistiu da compra." : item.MotivoRejeicao);
            return rr.IsSuccess ? "rejeitada" : Parar("criada", "rejeitar a proposta", rr.Error);
        }

        if (item.Cenario == "financiamentoNegado")
        {
            var rmp = await _propostaService.DefinirModoPagamentoAsync(propostaId, "Financiamento");
            if (!rmp.IsSuccess) return Parar("criada", "definir modo de pagamento Financiamento", rmp.Error);

            var rsfNeg = await _propostaService.SolicitarFinanciamentoAsync(propostaId);
            if (!rsfNeg.IsSuccess) return Parar("criada", "solicitar financiamento", rsfNeg.Error);

            var rn = await _propostaService.NegarFinanciamentoAsync(propostaId,
                string.IsNullOrWhiteSpace(item.MotivoRejeicao) ? "Cliente não atende aos critérios de crédito da financeira." : item.MotivoRejeicao);
            return rn.IsSuccess ? "financiamentoNegado" : Parar("criada", "registrar negativa da financiadora", rn.Error);
        }

        var usaFinanciamento = item.Cenario == "concluidaFinanciada";
        var modo = usaFinanciamento ? "Financiamento" : (string.IsNullOrWhiteSpace(item.ModoPagamento) ? ModosAVista[0] : item.ModoPagamento);

        var rm = await _propostaService.DefinirModoPagamentoAsync(propostaId, modo);
        if (!rm.IsSuccess) return Parar("criada", "definir o modo de pagamento", rm.Error);

        if (usaFinanciamento)
        {
            var rsf = await _propostaService.SolicitarFinanciamentoAsync(propostaId);
            if (!rsf.IsSuccess) return Parar("criada", "solicitar financiamento", rsf.Error);

            var textoProposta = string.IsNullOrWhiteSpace(item.TextoPropostaFinanciadora)
                ? "Financeira parceira pré-aprovou o financiamento em 36x, sujeito a análise cadastral final na assinatura."
                : item.TextoPropostaFinanciadora;

            var rrf = await _propostaService.RegistrarRespostaFinanciadoraAsync(propostaId, new RegistrarRespostaFinanciadoraDTO
            {
                TextoProposta = textoProposta
            });
            if (!rrf.IsSuccess) return Parar("criada", "registrar resposta da financiadora", rrf.Error);
        }

        var ra = await _propostaService.AprovarAsync(propostaId);
        if (!ra.IsSuccess) return Parar("criada", "aprovar a proposta", ra.Error);
        if (item.Cenario == "aprovada") return "aprovada";

        var riv = await _propostaService.IniciarVistoriaAsync(propostaId, vendedorId);
        if (!riv.IsSuccess) return Parar("aprovada", "iniciar a vistoria", riv.Error);
        var rv = await _propostaService.RegistrarVistoriaAsync(propostaId, vendedorId, new RegistrarVistoriaDTO
        {
            Observacoes = "Veículo vistoriado, sem avarias relevantes.",
            Aprovado = true
        });
        if (!rv.IsSuccess) return Parar("aprovada", "registrar a vistoria", rv.Error);
        if (item.Cenario == "vistoriada") return "vistoriada";

        var detalhe = await _propostaService.GetByIdAsync(propostaId);
        if (!detalhe.IsSuccess || detalhe.Value is null) return Parar("vistoriada", "reler a proposta antes do pagamento", detalhe.Error);

        var rp = await _pagamentoPropostaService.RegistrarPagamentoAsync(propostaId, vendedorId, new RegistrarPagamentoPropostaDTO
        {
            ModoPagamento = modo,
            Valor = detalhe.Value.ValorFinal,
            Observacoes = "Pagamento integral registrado."
        });
        if (!rp.IsSuccess) return Parar("vistoriada", "registrar o pagamento", rp.Error);

        var rt = await _propostaService.CriarOuEditarTermoAsync(propostaId, adminId, new CriarOuEditarTermoDTO
        {
            TextoTermo = "Termo de entrega gerado a partir do arquivo de importação de dados."
        });
        if (!rt.IsSuccess) return Parar("vistoriada", "criar o termo de entrega", rt.Error);
        if (item.Cenario == "termoRedigido") return "termoRedigido";

        var re = await _propostaService.EnviarTermoParaAssinaturaAsync(propostaId);
        if (!re.IsSuccess) return Parar("termoRedigido", "enviar o termo para assinatura", re.Error);
        if (item.Cenario == "termoEnviado") return "termoEnviado";

        var termoAtual = await _propostaService.ObterTermoAsync(propostaId);
        if (!termoAtual.IsSuccess || string.IsNullOrEmpty(termoAtual.Value?.TokenAssinatura))
            return Parar("termoEnviado", "reler o termo para assinar", termoAtual.Error);

        var rassin = await _propostaService.AssinarTermoAsync(
            termoAtual.Value!.TokenAssinatura!,
            new AssinarTermoDTO
            {
                NomeCliente = "Cliente Demonstração",
                CpfCliente = "00000000000",
                Aceite = true
            },
            "127.0.0.1");

        return rassin.IsSuccess ? item.Cenario : "termoEnviado";
    }

    // ============================================================
    // ORDENS DE SERVIÇO — funil completo
    // ============================================================
    private async Task ImportarOrdensServicoAsync(
        List<OrdemServicoImportDTO> itens, Dictionary<string, Guid> chaves, ImportacaoResultadoDTO resultado)
    {
        foreach (var item in itens)
        {
            try
            {
                if (!TentarResolver(chaves, item.VeiculoClienteChave, out var veiculoId, resultado.Avisos, "ordem de serviço", item.Chave, "veículo")) continue;
                if (!TentarResolver(chaves, item.ClienteChave, out var clienteId, resultado.Avisos, "ordem de serviço", item.Chave, "cliente")) continue;
                if (!TentarResolver(chaves, item.MecanicoChave, out var mecanicoId, resultado.Avisos, "ordem de serviço", item.Chave, "mecânico")) continue;

                // Itens de Estoque/Cliente entram direto na criação da OS. Itens de
                // Encomenda só nascem via fluxo de requisição de peça atendida (ver
                // abaixo) — a criação da OS rejeitaria Origem=Encomenda diretamente.
                var itensDiretos = new List<ItemOrdemServicoDTO>();
                var itensEncomenda = new List<(string ComponenteChave, Guid ComponenteId, int Quantidade)>();
                foreach (var itemComponente in item.Itens)
                {
                    if (!TentarResolver(chaves, itemComponente.ComponenteChave, out var componenteId, resultado.Avisos, "ordem de serviço", item.Chave, "componente")) continue;

                    var quantidade = Math.Max(1, itemComponente.Quantidade);
                    var origem = string.IsNullOrWhiteSpace(itemComponente.Origem) ? "Estoque" : itemComponente.Origem;

                    if (string.Equals(origem, "Encomenda", StringComparison.OrdinalIgnoreCase))
                    {
                        itensEncomenda.Add((itemComponente.ComponenteChave, componenteId, quantidade));
                        continue;
                    }

                    var compDetalhe = await _componenteService.GetByIdAsync(componenteId);
                    if (!compDetalhe.IsSuccess || compDetalhe.Value is null)
                    {
                        resultado.Avisos.Add($"OS \"{item.Chave}\": componente \"{itemComponente.ComponenteChave}\" não encontrado — item pulado.");
                        continue;
                    }

                    itensDiretos.Add(new ItemOrdemServicoDTO
                    {
                        ComponenteId = componenteId,
                        Quantidade = quantidade,
                        ValorUnitario = compDetalhe.Value.ValorVenda,
                        Origem = origem
                    });
                }

                Guid? checklistPresetId = null;
                if (!string.IsNullOrWhiteSpace(item.ChecklistPresetChave) &&
                    TentarResolver(chaves, item.ChecklistPresetChave, out var presetId, resultado.Avisos, "ordem de serviço", item.Chave, "checklist preset"))
                {
                    checklistPresetId = presetId;
                }

                var dto = new CriarOrdemServicoDTO
                {
                    VeiculoClienteId = veiculoId,
                    ClienteId = clienteId,
                    MecanicoId = mecanicoId,
                    Tipo = item.Tipo,
                    Descricao = item.Descricao,
                    // Precisa nascer estritamente no futuro — a data histórica é aplicada depois.
                    PrazoEstimado = DateTime.UtcNow.AddDays(Math.Max(1, item.PrazoDiasAPartirDaCriacao)),
                    CustoServico = item.CustoServico,
                    ChecklistPresetId = checklistPresetId,
                    Itens = itensDiretos
                };

                var r = await _ordemServicoService.AddAsync(dto);
                if (!r.IsSuccess)
                {
                    resultado.Avisos.Add($"OS \"{item.Chave}\": {r.Error}");
                    continue;
                }

                var ordemId = r.Value;

                // Itens de Encomenda: nascem do mesmo fluxo real de requisição de
                // peça que o mecânico usaria (abre -> admin atende -> libera a OS).
                if (itensEncomenda.Count > 0)
                {
                    foreach (var (componenteChave, componenteId, quantidade) in itensEncomenda)
                    {
                        var rAbrir = await _requisicaoPecaService.AbrirAsync(ordemId, mecanicoId, new CriarRequisicaoPecaDTO
                        {
                            DescricaoPeca = "Peça sob encomenda (importação de dados)",
                            Justificativa = "Peça não disponível em estoque no momento da criação da OS.",
                            Quantidade = quantidade
                        });
                        if (!rAbrir.IsSuccess || rAbrir.Value is null)
                        {
                            resultado.Avisos.Add($"OS \"{item.Chave}\": não foi possível abrir requisição para o componente \"{componenteChave}\" — {rAbrir.Error}");
                            continue;
                        }

                        var rAtender = await _requisicaoPecaService.AtenderAsync(rAbrir.Value.Id, mecanicoId, new AtenderRequisicaoDTO
                        {
                            ComponenteId = componenteId,
                            Quantidade = quantidade
                        });
                        if (!rAtender.IsSuccess)
                            resultado.Avisos.Add($"OS \"{item.Chave}\": requisição aberta mas não atendida para o componente \"{componenteChave}\" — {rAtender.Error}");
                    }

                    var rLiberar = await _requisicaoPecaService.LiberarOrdemAsync(ordemId);
                    if (!rLiberar.IsSuccess)
                        resultado.Avisos.Add($"OS \"{item.Chave}\": não foi possível liberar a OS após atender as requisições — {rLiberar.Error}");
                }

                await AvancarFunilOrdemServicoAsync(ordemId, item, mecanicoId, resultado.Avisos);

                RegistrarChave(chaves, item.Chave, ordemId, resultado.Avisos, "ordem de serviço");
                if (item.DataCriacao.HasValue)
                {
                    var prazo = item.DataCriacao.Value.AddDays(Math.Max(1, item.PrazoDiasAPartirDaCriacao));
                    await _backdate.AplicarAsync<Domain.Entities.Oficina.OrdemServico>(
                        ordemId, ("DataCriacao", item.DataCriacao.Value), ("PrazoEstimado", prazo));
                }

                resultado.OrdensServicoCriadas++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"OS \"{item.Chave}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar ordem de serviço {Chave}", item.Chave);
            }
        }
    }

    private async Task<string> AvancarFunilOrdemServicoAsync(
        Guid ordemId, OrdemServicoImportDTO item, Guid mecanicoId, List<string> avisos)
    {
        string Parar(string estagioAlcancado, string etapaQueFalhou, string? erro)
        {
            avisos.Add($"OS \"{item.Chave}\": não foi possível {etapaQueFalhou} — {erro ?? "motivo desconhecido"}.");
            return estagioAlcancado;
        }

        if (item.Cenario == "pendente") return "pendente";

        if (item.Cenario == "cancelada")
        {
            var rc = await _ordemServicoService.CancelarAsync(ordemId);
            return rc.IsSuccess ? "cancelada" : Parar("pendente", "cancelar a OS", rc.Error);
        }

        var ri = await _ordemServicoService.IniciarAsync(ordemId);
        if (!ri.IsSuccess) return Parar("pendente", "iniciar a OS", ri.Error);

        // Desistência no meio do serviço — diferente de "cancelada" (que
        // cancela ainda pendente, antes de qualquer trabalho começar).
        if (item.Cenario == "canceladaEmAndamento")
        {
            var rcEm = await _ordemServicoService.CancelarAsync(ordemId);
            return rcEm.IsSuccess ? "canceladaEmAndamento" : Parar("emAndamento", "cancelar a OS em andamento", rcEm.Error);
        }

        // Novo problema encontrado durante o serviço: mecânico emite alerta
        // (pausa a OS) e a decisão do cliente (aprovado) já é registrada na
        // mesma importação — a OS volta pra EmAndamento e o funil continua.
        if (item.ComAlerta)
        {
            var ra = await _alertaOSService.EmitirAsync(ordemId, mecanicoId, new CriarAlertaOSDTO
            {
                Descricao = "Mecânico identificou um problema adicional durante o serviço."
            });
            if (!ra.IsSuccess || ra.Value is null)
                avisos.Add($"OS \"{item.Chave}\": não foi possível emitir alerta de problema adicional — {ra.Error}.");
            else
            {
                var rr = await _alertaOSService.ResolverAsync(ra.Value.Id, mecanicoId, new ResolverAlertaDTO
                {
                    Aprovou = true,
                    ObservacaoCliente = "Cliente aprovou o serviço adicional."
                });
                if (!rr.IsSuccess)
                    avisos.Add($"OS \"{item.Chave}\": alerta emitido mas não resolvido — {rr.Error}.");
            }
        }

        if (item.Cenario == "emAndamento") return "emAndamento";

        // Finalizar exige a checklist inteira concluída (OrdemServico.Finalizar).
        var rConcluirChecklist = await ConcluirChecklistAsync(ordemId);
        if (!rConcluirChecklist.IsSuccess) return Parar("emAndamento", "concluir a checklist da OS", rConcluirChecklist.Error);

        // "finalizadaPendente" pára AQUI, sem pagar — checklist concluída,
        // pronta pra finalizar, mas ainda em EmAndamento. Desde que
        // OrdemServicoService.FinalizarAsync passou a exigir pagamento total
        // ANTES de finalizar, o antigo estágio "Pagamento Pendente" deixou de
        // ser alcançável por este caminho — o que esse cenário representa
        // ("serviço pronto, falta cobrar") continua igual, só que sem mudar
        // o Status pra além de EmAndamento.
        if (item.Cenario == "finalizadaPendente") return "finalizadaPendente";

        var detalhe = await _ordemServicoService.GetByIdAsync(ordemId);
        if (!detalhe.IsSuccess || detalhe.Value is null) return Parar("emAndamento", "reler a OS antes do pagamento", detalhe.Error);

        var modo = string.IsNullOrWhiteSpace(item.ModoPagamento) ? "Pix" : item.ModoPagamento;
        var rp = await _pagamentoOrdemServicoService.RegistrarPagamentoAsync(ordemId, mecanicoId, new RegistrarPagamentoDTO
        {
            ModoPagamento = modo,
            Valor = detalhe.Value.ValorTotal,
            Observacoes = "Pagamento integral registrado na recepção."
        });
        if (!rp.IsSuccess) return Parar("emAndamento", "registrar o pagamento", rp.Error);

        var rf = await _ordemServicoService.FinalizarAsync(ordemId);
        if (!rf.IsSuccess) return Parar("emAndamento", "finalizar a OS", rf.Error);
        if (item.Cenario == "finalizadaPaga") return "finalizadaPaga";

        var re = await _ordemServicoService.EntregarAsync(ordemId);
        return re.IsSuccess ? "entregue" : Parar("finalizadaPaga", "marcar a OS como entregue", re.Error);
    }

    private async Task<Result> ConcluirChecklistAsync(Guid ordemId)
    {
        var detalhe = await _ordemServicoService.GetByIdAsync(ordemId);
        if (!detalhe.IsSuccess || detalhe.Value is null) return Result.Fail(detalhe.Error ?? "OS não encontrada.");

        foreach (var itemChecklist in detalhe.Value.Checklist.Where(c => c.Status != "Concluido"))
        {
            var r = await _ordemServicoService.AtualizarStatusChecklistAsync(new AtualizarStatusChecklistDTO
            {
                OrdemServicoId = ordemId,
                ItemId = itemChecklist.Id,
                NovoStatus = "Concluido"
            });
            if (!r.IsSuccess) return r;
        }
        return Result.Ok();
    }

    // ============================================================
    // TEST DRIVES (CONCESSIONÁRIA)
    // ============================================================
    private async Task ImportarTestDrivesAsync(
        List<TestDriveImportDTO> itens, Dictionary<string, Guid> chaves, ImportacaoResultadoDTO resultado)
    {
        foreach (var item in itens)
        {
            try
            {
                if (!TentarResolver(chaves, item.VeiculoVendaChave, out var veiculoId, resultado.Avisos, "test drive", item.Chave, "veículo")) continue;
                if (!TentarResolver(chaves, item.ClienteChave, out var clienteId, resultado.Avisos, "test drive", item.Chave, "cliente")) continue;
                if (!TentarResolver(chaves, item.VendedorChave, out var vendedorId, resultado.Avisos, "test drive", item.Chave, "vendedor")) continue;

                var r = await _testDriveService.AgendarAsync(new CriarTestDriveDTO
                {
                    VeiculoVendaId = veiculoId,
                    ClienteId = clienteId,
                    VendedorId = vendedorId,
                    DataHora = item.DataHora,
                    Observacao = item.Observacao
                });
                if (!r.IsSuccess)
                {
                    resultado.Avisos.Add($"Test drive \"{item.Chave}\": {r.Error}");
                    continue;
                }

                var testDriveId = r.Value;
                var statusFinal = item.Cenario switch
                {
                    "realizado" => "Realizado",
                    "cancelado" => "Cancelado",
                    "naoCompareceu" => "NaoCompareceu",
                    _ => null
                };
                if (statusFinal is not null)
                {
                    var rs = await _testDriveService.AtualizarStatusAsync(new AtualizarStatusTestDriveDTO
                    {
                        Id = testDriveId,
                        Status = statusFinal
                    });
                    if (!rs.IsSuccess)
                        resultado.Avisos.Add($"Test drive \"{item.Chave}\": não foi possível marcar como \"{item.Cenario}\" — {rs.Error}");
                }

                RegistrarChave(chaves, item.Chave, testDriveId, resultado.Avisos, "test drive");
                resultado.TestDrivesCriados++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Test drive \"{item.Chave}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar test drive {Chave}", item.Chave);
            }
        }
    }

    // ============================================================
    // HELPERS
    // ============================================================
    private async Task<Guid> ObterAdminOuVendedorIdAsync(Dictionary<string, Guid> chaves)
    {
        var r = await _authService.ListarUsuariosAsync("Admin");
        if (r.IsSuccess && r.Value is not null)
        {
            var admin = r.Value.FirstOrDefault();
            if (admin is not null) return admin.Id;
        }

        // Sem admin cadastrado (nem nesta importação, nem já no banco) — usa
        // qualquer vendedor já resolvido como responsável pelo termo.
        return chaves.Values.FirstOrDefault();
    }

    private static void RegistrarChave(Dictionary<string, Guid> chaves, string chave, Guid id, List<string> avisos, string tipoEntidade)
    {
        if (string.IsNullOrWhiteSpace(chave)) return;

        if (!chaves.TryAdd(chave, id))
            avisos.Add($"Chave \"{chave}\" duplicada ({tipoEntidade}) — o registro foi criado, mas outras seções vão referenciar só o primeiro com essa chave.");
    }

    private static bool TentarResolver(
        Dictionary<string, Guid> chaves, string chave, out Guid id, List<string> avisos, string tipoEntidade, string chaveDoRegistro, string papel)
    {
        if (!string.IsNullOrWhiteSpace(chave) && chaves.TryGetValue(chave, out id))
            return true;

        avisos.Add($"{tipoEntidade} \"{chaveDoRegistro}\": {papel} \"{chave}\" não encontrado(a) — registro pulado.");
        id = Guid.Empty;
        return false;
    }
}
