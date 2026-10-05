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
using CarStoreManager.Domain.Entities.Sistema;
using Microsoft.Extensions.Logging;
using System.Globalization;

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

    // Dados de apoio pra textos reais (termos, contrato da vistoria) e
    // assinaturas com nome/CPF do próprio cliente — preenchidos à medida que
    // cada seção é importada (escopo = uma chamada de ImportarAsync).
    private readonly Dictionary<Guid, (string Nome, string Cpf)> _clientes = new();
    private readonly Dictionary<Guid, string> _usuarios = new();
    private readonly Dictionary<Guid, DateTime> _datasClientes = new();
    private readonly Dictionary<Guid, VeiculoInfo> _veiculos = new();
    private Guid _adminId;
    // Arquivo exportado (DespesasAutomaticasNoArquivo): despesas automáticas
    // lançadas pelas regras durante a importação são descartadas no fim (o
    // arquivo já as traz em despesasExtras), em vez de movidas de competência.
    private bool _despesasNoArquivo;
    private HashSet<Guid> _despesasMesAtualAntes = new();

    private sealed record VeiculoInfo(string Marca, string Modelo, int Ano, string Cor, string Placa, string Renavam, int Quilometragem);

    public async Task<Result<ImportacaoResultadoDTO>> ImportarAsync(ImportacaoDadosDTO dados)
    {
        var resultado = new ImportacaoResultadoDTO();
        // Mapeia "chave" de texto do arquivo -> Guid real criado nesta importação.
        var chaves = new Dictionary<string, Guid>();
        _clientes.Clear();
        _datasClientes.Clear();
        _usuarios.Clear();
        _veiculos.Clear();

        // Seções novas (contrato v2) podem vir null num JSON antigo escrito à mão.
        dados.ComponentesEquivalentes ??= new();
        dados.FechamentosMensais ??= new();
        _despesasNoArquivo = dados.DespesasAutomaticasNoArquivo;

        try
        {
            _despesasMesAtualAntes = await IdsItensDespesaMesAtualAsync();
            await ImportarUsuariosAsync(dados.Usuarios, chaves, resultado);
            _adminId = await ObterAdminOuVendedorIdAsync(chaves);
            await ImportarClientesAsync(dados.Clientes, chaves, resultado);
            await ImportarFornecedoresAsync(dados.Fornecedores, chaves, resultado);
            // Despesas-modelo ANTES de qualquer coisa que lance despesa (reposição
            // de estoque, compra de veículo...): o 1º lançamento numa competência
            // gera o balanço "do modelo" — com o modelo ainda vazio, aquele mês
            // ficava para sempre sem salário/aluguel/contas ("já existe").
            await ImportarDespesasAsync(dados.Despesas, resultado);
            await ImportarComponentesAsync(dados.Componentes, chaves, resultado);
            await ImportarComponentesEquivalentesAsync(dados.ComponentesEquivalentes, chaves, resultado);
            await ImportarChecklistPresetsAsync(dados.ChecklistPresets, chaves, resultado);
            await ImportarTemplatesDocumentoAsync(dados.TemplatesDocumento, chaves, resultado);
            await ImportarVeiculosVendaAsync(dados.VeiculosVenda, chaves, resultado);
            // Consignações em 2 passos: cria (Ativa) agora; status final +
            // datas históricas só DEPOIS dos test drives — test drive de
            // consignado (doc 31) exige a consignação Ativa no agendamento.
            await ImportarVeiculosConsignadosAsync(dados.VeiculosConsignados, chaves, resultado);
            await ImportarVeiculosClienteAsync(dados.VeiculosCliente, chaves, resultado);
            // TestDrives ANTES de PropostasVenda: propostas concluídas marcam o
            // veículo como Vendido, e agendar test drive de um veículo vendido é
            // bloqueado — então test drive precisa "acontecer" enquanto o carro
            // ainda está disponível, na ordem natural do funil real também.
            await ImportarTestDrivesAsync(dados.TestDrives, chaves, resultado);
            await FinalizarVeiculosConsignadosAsync(dados.VeiculosConsignados, chaves, resultado);
            await ImportarPropostasAsync(dados.PropostasVenda, chaves, resultado);
            // Datas da consignação (e do histórico) só depois das propostas:
            // proposta de consignado também gera evento no histórico.
            await AplicarDatasVeiculosConsignadosAsync(dados.VeiculosConsignados, chaves);
            await ImportarOrdensServicoAsync(dados.OrdensServico, chaves, resultado);
            if (_despesasNoArquivo)
                await DescartarDespesasAutomaticasDaImportacaoAsync();
            await ImportarDespesasExtrasAsync(dados.DespesasExtras, resultado);
            // Sempre por último: fechar um mês bloqueia qualquer item novo nele.
            await ImportarFechamentosMensaisAsync(dados.FechamentosMensais, resultado);
            await AjustarDatasBalancosAsync();

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
        // Usuário com o mesmo e-mail já no banco (ex.: admin@teste.com do seed,
        // que todo arquivo exportado também traz) é reaproveitado — a chave
        // aponta pra ele, sem criar duplicado nem gerar aviso.
        var existentes = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var rLista = await _authService.ListarUsuariosAsync();
        if (rLista.IsSuccess && rLista.Value is not null)
            foreach (var u in rLista.Value) existentes.TryAdd(u.Email, u.Id);

        foreach (var item in itens)
        {
            if (!string.IsNullOrWhiteSpace(item.Email) && existentes.TryGetValue(item.Email.Trim(), out var idExistente))
            {
                RegistrarChave(chaves, item.Chave, idExistente, resultado.Avisos, "usuário");
                _usuarios[idExistente] = item.Nome;
                continue;
            }
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
                _usuarios[r.Value] = item.Nome;
                if (item.DataContratacao.HasValue)
                {
                    await _backdate.AplicarAsync<Domain.Entities.Usuario>(r.Value, ("DataCriacao", item.DataContratacao.Value));
                    // Data de contratação de verdade (DadosFuncionario, owned) —
                    // nasceu "daqui a 5 min" por causa da validação acima.
                    if (item.Tipo != "Admin")
                        await _backdate.AplicarAsync<Domain.Entities.Usuario>(r.Value, ("DadosFuncionario.DataContratacao", item.DataContratacao.Value));
                }
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
                _clientes[r.Value] = (item.Nome, item.Cpf);
                if (item.DataCriacao.HasValue)
                {
                    _datasClientes[r.Value] = item.DataCriacao.Value;
                    await _backdate.AplicarAsync<Domain.Entities.Cliente>(r.Value, ("DataCriacao", item.DataCriacao.Value));
                    var enderecoId = (await _backdate.ConsultarAsync<Domain.Entities.Cliente, Guid>(c => c.Id == r.Value, c => c.EnderecoId)).FirstOrDefault();
                    if (enderecoId != Guid.Empty)
                        await _backdate.AplicarAsync<Domain.Entities.Endereco>(enderecoId, ("DataCriacao", item.DataCriacao.Value));
                }
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
                        var nomeDespesaRep = $"Compra de componente: {item.Nome} — {item.SKUInterno} (x{rep.Quantidade})";
                        if (item.EstoqueInicial)
                            await RemoverDespesaDoMesAtualAsync(nomeDespesaRep);
                        else if (!_despesasNoArquivo)
                            await MoverDespesaParaCompetenciaHistoricaAsync(nomeDespesaRep, rep.Data);
                    }
                }
                else if (item.QuantidadeEstoque > 0)
                {
                    var rEntrada = await _estoqueService.EntradaAsync(r.Value, item.QuantidadeEstoque);
                    if (!rEntrada.IsSuccess)
                        resultado.Avisos.Add($"Componente \"{item.Chave}\": criado, mas a entrada de estoque falhou — {rEntrada.Error}");
                    else if (item.EstoqueInicial)
                        await RemoverDespesaDoMesAtualAsync($"Compra de componente: {item.Nome} — {item.SKUInterno} (x{item.QuantidadeEstoque})");
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
    // COMPONENTES COMPATÍVEIS (doc 35) — curadoria manual de equivalência
    // ============================================================
    private async Task ImportarComponentesEquivalentesAsync(
        List<ComponenteEquivalenteImportDTO> itens, Dictionary<string, Guid> chaves, ImportacaoResultadoDTO resultado)
    {
        foreach (var item in itens)
        {
            var rotulo = $"{item.ComponenteChave} ↔ {item.EquivalenteChave}";
            try
            {
                if (!TentarResolver(chaves, item.ComponenteChave, out var originalId, resultado.Avisos, "componente compatível", rotulo, "componente")) continue;
                if (!TentarResolver(chaves, item.EquivalenteChave, out var equivalenteId, resultado.Avisos, "componente compatível", rotulo, "componente equivalente")) continue;

                var tipo = Enum.TryParse<Domain.Enums.TipoEquivalencia>(item.TipoEquivalencia, true, out var t)
                    ? t : Domain.Enums.TipoEquivalencia.Similar;

                var r = await _componenteService.CriarLigacaoEquivalenciaAsync(new DTOs.Oficina.Componente.CriarComponenteEquivalenteDTO
                {
                    ComponenteOriginalId = originalId,
                    ComponenteEquivalenteId = equivalenteId,
                    TipoEquivalencia = tipo
                });
                if (!r.IsSuccess)
                {
                    resultado.Avisos.Add($"Componente compatível \"{rotulo}\": {r.Error}");
                    continue;
                }
                resultado.ComponentesEquivalentesCriados++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Componente compatível \"{rotulo}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar componente compatível {Rotulo}", rotulo);
            }
        }
    }

    // ============================================================
    // CHECKLIST PRESETS (OFICINA)
    // ============================================================
    private async Task ImportarChecklistPresetsAsync(
        List<ChecklistPresetImportDTO> itens, Dictionary<string, Guid> chaves, ImportacaoResultadoDTO resultado)
    {
        // Preset com o mesmo nome já no banco (os 3 do seed) é reaproveitado.
        var existentes = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var rLista = await _checklistPresetService.GetAllAsync();
        if (rLista.IsSuccess && rLista.Value is not null)
            foreach (var p in rLista.Value) existentes.TryAdd(p.Nome, p.Id);

        foreach (var item in itens)
        {
            if (existentes.TryGetValue(item.Nome?.Trim() ?? "", out var idExistente))
            {
                RegistrarChave(chaves, item.Chave, idExistente, resultado.Avisos, "checklist preset");
                continue;
            }
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
        // Template com o mesmo nome já no banco (os padrão do seed) é reaproveitado.
        var existentes = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var rLista = await _templateDocumentoService.GetAllAsync();
        if (rLista.IsSuccess && rLista.Value is not null)
            foreach (var t in rLista.Value) existentes.TryAdd(t.Nome, t.Id);

        foreach (var item in itens)
        {
            if (existentes.TryGetValue(item.Nome?.Trim() ?? "", out var idExistente))
            {
                RegistrarChave(chaves, item.Chave, idExistente, resultado.Avisos, "template de documento");
                continue;
            }
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
                    Tipo = item.Tipo,
                    Categoria = item.Categoria
                };

                var r = await _despesaService.AddAsync(dto);
                if (!r.IsSuccess)
                {
                    resultado.Avisos.Add($"Despesa \"{item.Nome}\": {r.Error}");
                    continue;
                }

                if (!item.Ativa)
                {
                    var ru = await _despesaService.UpdateAsync(new AtualizarDespesaDTO
                    {
                        Id = r.Value, Nome = item.Nome, Valor = item.Valor, Ativa = false, Setor = item.Setor, Tipo = item.Tipo, Categoria = item.Categoria
                    });
                    if (!ru.IsSuccess) resultado.Avisos.Add($"Despesa \"{item.Nome}\": criada, mas não foi possível desativar — {ru.Error}");
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
                var nomeDespesaVeiculo = $"Compra de veículo para concessionária: {item.Marca} {item.Modelo} — {item.Placa}";
                if (item.EstoqueInicial)
                    await RemoverDespesaDoMesAtualAsync(nomeDespesaVeiculo);
                _veiculos[r.Value] = new VeiculoInfo(item.Marca, item.Modelo, item.Ano, item.Cor, item.Placa, item.Renavam, item.Quilometragem);
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
                    if (!_despesasNoArquivo && !item.EstoqueInicial)
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
    /// Estoque inicial (já existia antes do sistema): a despesa automática de
    /// compra lançada "hoje" pela regra de negócio é só removida — não vai pra
    /// competência histórica nenhuma (e não gera balanço histórico).
    /// </summary>
    private async Task RemoverDespesaDoMesAtualAsync(string nomeItem)
    {
        var hoje = DateTime.Today;
        var rHoje = await _balancoDespesaService.ObterAsync(hoje.Year, hoje.Month);
        if (!rHoje.IsSuccess || rHoje.Value is null) return;
        var item = rHoje.Value.Itens.LastOrDefault(i => !i.DoModelo && i.Nome == nomeItem && !_despesasMesAtualAntes.Contains(i.Id));
        if (item is not null)
            await _balancoDespesaService.RemoverItemAsync(hoje.Year, hoje.Month, item.Id);
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

    // ============================================================
    // VEÍCULOS CONSIGNADOS — passo 1: cria (fica Ativa)
    // ============================================================
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

                RegistrarChave(chaves, item.Chave, r.Value, resultado.Avisos, "veículo consignado");
                _veiculos[r.Value] = new VeiculoInfo(item.Marca, item.Modelo, item.Ano, item.Cor, item.Placa, item.Renavam, item.Quilometragem);
                resultado.VeiculosConsignadosCriados++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Veículo consignado \"{item.Chave}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar veículo consignado {Chave}", item.Chave);
            }
            finally { _backdate.LimparRastreamento(); }
        }
    }

    // ============================================================
    // VEÍCULOS CONSIGNADOS — passo 2 (depois dos test drives): status
    // final do cenário + datas históricas (inclusive do histórico)
    // ============================================================
    private async Task FinalizarVeiculosConsignadosAsync(
        List<VeiculoConsignacaoImportDTO> itens, Dictionary<string, Guid> chaves, ImportacaoResultadoDTO resultado)
    {
        foreach (var item in itens)
        {
            if (string.IsNullOrWhiteSpace(item.Chave) || !chaves.TryGetValue(item.Chave, out var id)) continue;
            try
            {
                Result? rs = item.Cenario switch
                {
                    "vendidaAguardando" => await _veiculoConsignacaoService.MarcarComoVendidaAsync(id),
                    "concluida" => await MarcarVendidaEConcluirAsync(id),
                    "devolvida" => await _veiculoConsignacaoService.DevolverAsync(id),
                    "cancelada" => await _veiculoConsignacaoService.CancelarAsync(id,
                        string.IsNullOrWhiteSpace(item.MotivoCancelamento) ? "Cancelado a pedido do proprietário." : item.MotivoCancelamento),
                    _ => null
                };
                if (rs is { IsSuccess: false })
                    resultado.Avisos.Add($"Veículo consignado \"{item.Chave}\": não foi possível levar ao cenário \"{item.Cenario}\" — {rs.Error}");

            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Veículo consignado \"{item.Chave}\": erro inesperado ao aplicar o cenário ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao finalizar veículo consignado {Chave}", item.Chave);
            }
            finally { _backdate.LimparRastreamento(); }
        }
    }

    /// <summary>Datas históricas da consignação e de cada evento do histórico (depois das propostas).</summary>
    private async Task AplicarDatasVeiculosConsignadosAsync(List<VeiculoConsignacaoImportDTO> itens, Dictionary<string, Guid> chaves)
    {
        foreach (var item in itens)
        {
            if (string.IsNullOrWhiteSpace(item.Chave) || !chaves.TryGetValue(item.Chave, out var id)) continue;
            try
            {
                if (item.DataCriacao.HasValue)
                {
                    var criacao = item.DataCriacao.Value;
                    var dataVencimento = criacao.AddDays(item.PrazoDias);
                    await _backdate.AplicarAsync<Domain.Entities.Concessionaria.VeiculoConsignacao>(
                        id,
                        ("DataCriacao", criacao),
                        ("DataInicio", criacao),
                        ("DataVencimento", dataVencimento));

                    // Histórico: cada evento vai pra data do próprio evento.
                    var meio = criacao.AddDays(Math.Max(1, item.PrazoDias / 2));
                    var dataVenda = Lim(item.DataVenda ?? (item.DataConclusao?.AddDays(-7)) ?? LimEntre(criacao, meio));
                    var dataConclusao = Lim(item.DataConclusao ?? LimEntre(dataVenda, dataVenda.AddDays(7)));
                    var dataDevolucao = Lim(item.DataDevolucao ?? LimEntre(criacao, dataVencimento));
                    var dataCancelamento = Lim(item.DataCancelamento ?? LimEntre(criacao, criacao.AddDays(Math.Min(15, Math.Max(1, item.PrazoDias / 3)))));

                    var eventos = await _backdate.ConsultarAsync<Domain.Entities.Concessionaria.HistoricoConsignacao, KeyValuePair<Guid, Domain.Enums.TipoEventoConsignacao>>(
                        h => h.VeiculoConsignacaoId == id,
                        h => new KeyValuePair<Guid, Domain.Enums.TipoEventoConsignacao>(h.Id, h.TipoEvento));
                    foreach (var (eventoId, tipo) in eventos)
                    {
                        var data = tipo switch
                        {
                            Domain.Enums.TipoEventoConsignacao.Criacao => criacao,
                            Domain.Enums.TipoEventoConsignacao.Venda => dataVenda,
                            Domain.Enums.TipoEventoConsignacao.MudancaStatus => dataConclusao,
                            Domain.Enums.TipoEventoConsignacao.Devolucao => dataDevolucao,
                            Domain.Enums.TipoEventoConsignacao.Cancelamento => dataCancelamento,
                            _ => criacao
                        };
                        await _backdate.AplicarAsync<Domain.Entities.Concessionaria.HistoricoConsignacao>(eventoId, ("DataCriacao", data));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao aplicar datas do veículo consignado {Chave}", item.Chave);
            }
            finally { _backdate.LimparRastreamento(); }
        }
    }

    private async Task<Result> MarcarVendidaEConcluirAsync(Guid id)
    {
        var rv = await _veiculoConsignacaoService.MarcarComoVendidaAsync(id);
        if (!rv.IsSuccess) return rv;
        return await _veiculoConsignacaoService.ConcluirVendaAsync(id);
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
                _veiculos[r.Value] = new VeiculoInfo(item.Marca, item.Modelo, item.Ano, item.Cor, item.Placa, "", 0);
                // Sem data própria no arquivo: o carro "chega" junto com o cadastro do dono.
                if (_datasClientes.TryGetValue(clienteId, out var dataCliente))
                    await _backdate.AplicarAsync<Domain.Entities.Oficina.VeiculoCliente>(r.Value, ("DataCriacao", dataCliente));
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

    /// <summary>O que o funil da proposta realmente criou — base pro backdate das datas secundárias.</summary>
    private sealed class RastroProposta
    {
        public bool Aprovada, SolicitouFinanciamento, RespostaFinanciadora;
        public Guid? VistoriaId;
        public bool VistoriaConcluida;
        public List<(Guid Id, DateTime? Data)> Pagamentos { get; } = new();
        public Guid? TermoId;
        public bool TermoAssinado;
    }

    private async Task ImportarPropostasAsync(
        List<PropostaVendaImportDTO> itens, Dictionary<string, Guid> chaves, ImportacaoResultadoDTO resultado)
    {
        foreach (var item in itens)
        {
            try
            {
                var consignado = !string.IsNullOrWhiteSpace(item.VeiculoConsignadoChave);
                Guid veiculoId;
                if (consignado)
                {
                    if (!TentarResolver(chaves, item.VeiculoConsignadoChave!, out veiculoId, resultado.Avisos, "proposta de venda", item.Chave, "veículo consignado")) continue;
                }
                else if (!TentarResolver(chaves, item.VeiculoVendaChave, out veiculoId, resultado.Avisos, "proposta de venda", item.Chave, "veículo")) continue;
                if (!TentarResolver(chaves, item.ClienteChave, out var clienteId, resultado.Avisos, "proposta de venda", item.Chave, "cliente")) continue;
                if (!TentarResolver(chaves, item.VendedorChave, out var vendedorId, resultado.Avisos, "proposta de venda", item.Chave, "vendedor")) continue;

                var dto = new CriarPropostaVendaDTO
                {
                    VendedorId = vendedorId,
                    VeiculoVendaId = veiculoId,
                    VeiculoEntidadeTipo = consignado ? "VeiculoConsignacao" : "VeiculoVenda",
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
                var rastro = new RastroProposta();
                await AvancarFunilPropostaAsync(propostaId, item, veiculoId, clienteId, vendedorId, rastro, resultado.Avisos);

                RegistrarChave(chaves, item.Chave, propostaId, resultado.Avisos, "proposta de venda");
                if (item.DataCriacao.HasValue)
                    await AplicarDatasPropostaAsync(propostaId, item, rastro);

                resultado.PropostasVendaCriadas++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Proposta \"{item.Chave}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar proposta {Chave}", item.Chave);
            }
            finally { _backdate.LimparRastreamento(); }
        }
    }

    /// <summary>
    /// Roda o funil real da proposta até o cenário pedido, sempre com datas
    /// reais (a proposta expira 7 dias após DataCriacao e todo método de
    /// transição bloqueia proposta expirada) — o redate pra data histórica
    /// acontece só depois (<see cref="AplicarDatasPropostaAsync"/>), a partir
    /// do que ficou registrado em <paramref name="rastro"/>.
    /// </summary>
    private async Task AvancarFunilPropostaAsync(
        Guid propostaId, PropostaVendaImportDTO item, Guid veiculoId, Guid clienteId, Guid vendedorId,
        RastroProposta rastro, List<string> avisos)
    {
        void Parar(string etapaQueFalhou, string? erro)
            => avisos.Add($"Proposta \"{item.Chave}\": não foi possível {etapaQueFalhou} — {erro ?? "motivo desconhecido"}.");

        if (item.Cenario == "criada") return;

        if (item.Cenario == "rejeitada")
        {
            var rr = await _propostaService.RejeitarAsync(propostaId, string.IsNullOrWhiteSpace(item.MotivoRejeicao) ? "Cliente desistiu da compra." : item.MotivoRejeicao);
            if (!rr.IsSuccess) Parar("rejeitar a proposta", rr.Error);
            return;
        }

        var usaFinanciamento = item.Cenario is "concluidaFinanciada" or "financiamentoNegado" or "aguardandoFinanciadora" or "respostaFinanciadora"
            || string.Equals(item.ModoPagamento, "Financiamento", StringComparison.OrdinalIgnoreCase);

        // Modo "da proposta" (à vista): o informado, senão o do 1º pagamento
        // listado, senão Pix — sempre dentro dos aceitos pra veículo.
        string modo;
        if (usaFinanciamento) modo = "Financiamento";
        else
        {
            var candidato = NormalizarModo(item.ModoPagamento) ?? NormalizarModo(item.Pagamentos?.FirstOrDefault()?.Modo);
            modo = candidato is not null && ModosAVista.Contains(candidato) ? candidato : ModosAVista[0];
        }

        if (item.ValorEntrada is > 0m)
        {
            var rEnt = await _propostaService.DefinirEntradaAsync(new DefinirEntradaDTO { PropostaId = propostaId, ValorEntrada = item.ValorEntrada.Value });
            if (!rEnt.IsSuccess) Parar("definir o valor de entrada", rEnt.Error);
        }

        var rm = await _propostaService.DefinirModoPagamentoAsync(propostaId, modo);
        if (!rm.IsSuccess) { Parar("definir o modo de pagamento", rm.Error); return; }

        if (usaFinanciamento)
        {
            var rsf = await _propostaService.SolicitarFinanciamentoAsync(propostaId);
            if (!rsf.IsSuccess) { Parar("solicitar financiamento", rsf.Error); return; }
            rastro.SolicitouFinanciamento = true;
            if (item.Cenario == "aguardandoFinanciadora") return;

            if (item.Cenario == "financiamentoNegado")
            {
                var rn = await _propostaService.NegarFinanciamentoAsync(propostaId,
                    string.IsNullOrWhiteSpace(item.MotivoRejeicao) ? "Cliente não atende aos critérios de crédito da financeira." : item.MotivoRejeicao);
                if (!rn.IsSuccess) Parar("registrar negativa da financiadora", rn.Error);
                return;
            }

            var textoProposta = string.IsNullOrWhiteSpace(item.TextoPropostaFinanciadora)
                ? "Financeira parceira pré-aprovou o financiamento em 36x, sujeito a análise cadastral final na assinatura."
                : item.TextoPropostaFinanciadora;

            var rrf = await _propostaService.RegistrarRespostaFinanciadoraAsync(propostaId, new RegistrarRespostaFinanciadoraDTO
            {
                TextoProposta = textoProposta
            });
            if (!rrf.IsSuccess) { Parar("registrar resposta da financiadora", rrf.Error); return; }
            rastro.RespostaFinanciadora = true;
            if (item.Cenario == "respostaFinanciadora") return;
        }

        var ra = await _propostaService.AprovarAsync(propostaId);
        if (!ra.IsSuccess) { Parar("aprovar a proposta", ra.Error); return; }
        rastro.Aprovada = true;
        if (item.Cenario == "aprovada") return;

        if (item.Cenario == "cancelada")
        {
            var motivo = !string.IsNullOrWhiteSpace(item.MotivoCancelamento) ? item.MotivoCancelamento
                : !string.IsNullOrWhiteSpace(item.MotivoRejeicao) ? item.MotivoRejeicao
                : "Cliente desistiu da compra após a aprovação.";
            var rc = await _propostaService.CancelarAsync(propostaId, motivo);
            if (!rc.IsSuccess) { Parar("cancelar a proposta", rc.Error); return; }

            // Aprovar já reservou o carro como Vendido; com a venda cancelada o
            // vendedor devolve o veículo ao estoque (mesma ação da tela do veículo).
            if (string.IsNullOrWhiteSpace(item.VeiculoConsignadoChave))
            {
                var rd = await _veiculoVendaService.MarcarComoDisponivelAsync(veiculoId);
                if (!rd.IsSuccess) Parar("devolver o veículo ao estoque após o cancelamento", rd.Error);
            }
            return;
        }

        var riv = await _propostaService.IniciarVistoriaAsync(propostaId, vendedorId);
        if (!riv.IsSuccess) { Parar("iniciar a vistoria", riv.Error); return; }
        rastro.VistoriaId = riv.Value;
        if (item.Cenario == "aguardandoVistoria") return;
        var rv = await _propostaService.RegistrarVistoriaAsync(propostaId, vendedorId, new RegistrarVistoriaDTO
        {
            Observacoes = string.IsNullOrWhiteSpace(item.ObservacoesVistoria) ? "Veículo vistoriado, sem avarias relevantes." : item.ObservacoesVistoria,
            Aprovado = true
        });
        if (!rv.IsSuccess) { Parar("registrar a vistoria", rv.Error); return; }
        rastro.VistoriaId = rv.Value?.Id ?? rastro.VistoriaId;
        rastro.VistoriaConcluida = true;
        if (item.Cenario == "vistoriada") return;

        var detalhe = await _propostaService.GetByIdAsync(propostaId);
        if (!detalhe.IsSuccess || detalhe.Value is null) { Parar("reler a proposta antes do pagamento", detalhe.Error); return; }
        var valorFinal = detalhe.Value.ValorFinal;

        // Pagamentos: lista explícita (percentuais) ou o padrão antigo.
        // termoRedigido com lista explícita (arquivo exportado) respeita o que
        // foi pago de fato; sem lista, quita (comportamento antigo).
        var quitar = item.Cenario != "pagamentoParcial"
            && !(item.Cenario == "termoRedigido" && item.Pagamentos is not null);
        List<(string Modo, decimal Valor, DateTime? Data)> pagamentos;
        if (item.Pagamentos is { Count: > 0 })
            pagamentos = CalcularPagamentos(valorFinal, item.Pagamentos, modo, quitar);
        else if (item.Pagamentos is not null && !quitar)
            pagamentos = new();
        else if (!quitar)
            pagamentos = CalcularPagamentos(valorFinal, new List<PagamentoImportDTO> { new() { Modo = modo == "Financiamento" ? "Pix" : modo, Percentual = 20m } }, modo, false);
        else if (usaFinanciamento && item.ValorEntrada is > 0m && item.ValorEntrada < valorFinal)
            pagamentos = new() { ("Pix", item.ValorEntrada.Value, null), ("Financiamento", valorFinal - item.ValorEntrada.Value, null) };
        else
            pagamentos = new() { (modo, valorFinal, null) };

        for (var i = 0; i < pagamentos.Count; i++)
        {
            var (modoPag, valor, data) = pagamentos[i];
            var obs = pagamentos.Count == 1 && quitar ? "Pagamento integral registrado."
                : i == pagamentos.Count - 1 && quitar ? "Pagamento do saldo restante."
                : i == 0 ? "Sinal / entrada do veículo." : "Pagamento parcial.";
            var rp = await _pagamentoPropostaService.RegistrarPagamentoAsync(propostaId, vendedorId, new RegistrarPagamentoPropostaDTO
            {
                ModoPagamento = modoPag,
                Valor = valor,
                Observacoes = obs
            });
            if (!rp.IsSuccess || rp.Value is null) { Parar($"registrar o pagamento {i + 1} ({modoPag}, R$ {valor:N2})", rp.Error); return; }
            rastro.Pagamentos.Add((rp.Value.Id, data));
        }
        if (item.Cenario == "pagamentoParcial") return;

        // Termo de entrega: texto do rascunho do veículo (o mesmo que a tela
        // carrega), com os campos entre colchetes preenchidos com dados reais.
        var rascunho = await _propostaService.ObterRascunhoInicialTermoAsync(propostaId);
        var textoBase = rascunho.IsSuccess && !string.IsNullOrWhiteSpace(rascunho.Value)
            ? rascunho.Value!
            : TemplatesDocumentosPadrao.TermoEntrega;
        var cliente = await ObterClienteAsync(clienteId);
        _veiculos.TryGetValue(veiculoId, out var veic);
        var datasPagamento = item.Pagamentos?.Where(p => p.Data.HasValue).Select(p => p.Data!.Value).ToList() ?? new();
        var dataTermo = item.DataTermo
            ?? (datasPagamento.Count > 0 ? datasPagamento.Max() : (DateTime?)null)
            ?? item.DataVistoria ?? item.DataAprovacao?.AddDays(1) ?? item.DataCriacao ?? DateTime.Now;
        var textoTermo = Preencher(textoBase, new Dictionary<string, string?>
        {
            ["NOME DO CLIENTE"] = cliente.Nome,
            ["CPF DO CLIENTE"] = FormatarCpf(cliente.Cpf),
            ["MARCA E MODELO"] = veic is null ? null : $"{veic.Marca} {veic.Modelo}",
            ["ANO DE FABRICAÇÃO/MODELO"] = veic?.Ano.ToString(),
            ["ANO"] = veic?.Ano.ToString(),
            ["COR"] = veic?.Cor,
            ["PLACA"] = veic?.Placa,
            ["RENAVAM"] = veic?.Renavam,
            ["QUILOMETRAGEM"] = veic?.Quilometragem.ToString("N0", PtBr),
            ["VALOR"] = valorFinal.ToString("N2", PtBr),
            ["DATA"] = dataTermo.ToString("dd/MM/yyyy"),
            ["NÚMERO"] = "2",
            ["NOME DO VENDEDOR/REPRESENTANTE"] = _usuarios.GetValueOrDefault(vendedorId),
        });

        if (!string.IsNullOrWhiteSpace(item.TextoTermo)) textoTermo = item.TextoTermo;
        var rt = await _propostaService.CriarOuEditarTermoAsync(propostaId, _adminId, new CriarOuEditarTermoDTO { TextoTermo = textoTermo });
        if (!rt.IsSuccess || rt.Value is null) { Parar("criar o termo de entrega", rt.Error); return; }
        rastro.TermoId = rt.Value.Id;
        if (item.Cenario == "termoRedigido") return;

        var re = await _propostaService.EnviarTermoParaAssinaturaAsync(propostaId);
        if (!re.IsSuccess) { Parar("enviar o termo para assinatura", re.Error); return; }
        if (item.Cenario == "termoEnviado") return;

        var termoAtual = await _propostaService.ObterTermoAsync(propostaId);
        if (!termoAtual.IsSuccess || string.IsNullOrEmpty(termoAtual.Value?.TokenAssinatura))
        { Parar("reler o termo para assinar", termoAtual.Error); return; }

        // Assinado pelo PRÓPRIO cliente da proposta (nome e CPF reais).
        var rassin = await _propostaService.AssinarTermoAsync(
            termoAtual.Value!.TokenAssinatura!,
            new AssinarTermoDTO
            {
                NomeCliente = string.IsNullOrWhiteSpace(item.AssinaturaNome) ? cliente.Nome : item.AssinaturaNome,
                CpfCliente = string.IsNullOrWhiteSpace(item.AssinaturaCpf) ? cliente.Cpf : item.AssinaturaCpf,
                Aceite = true
            },
            "127.0.0.1");
        if (!rassin.IsSuccess) { Parar("assinar o termo de entrega", rassin.Error); return; }
        rastro.TermoAssinado = true;
    }

    /// <summary>
    /// Linha do tempo da proposta: criação → (financiadora) → aprovação →
    /// vistoria → pagamentos → termo → assinatura. Campos do JSON têm
    /// prioridade; o que faltar é derivado da etapa anterior, nunca "hoje".
    /// </summary>
    private async Task AplicarDatasPropostaAsync(Guid propostaId, PropostaVendaImportDTO item, RastroProposta rastro)
    {
        var criacao = item.DataCriacao!.Value;
        var aprovacao = Lim(item.DataAprovacao ?? LimEntre(criacao, criacao.AddDays(2)));

        var valores = new List<(string, object?)> { ("DataCriacao", criacao) };
        if (rastro.Aprovada) valores.Add(("DataAprovacao", aprovacao));
        if (rastro.SolicitouFinanciamento)
            valores.Add(("DataSolicitacaoFinanciamento", rastro.Aprovada ? Entre(criacao, aprovacao, 0.25) : Lim(criacao.AddHours(3))));
        if (rastro.RespostaFinanciadora)
            valores.Add(("DataRespostaFinanciadora", Entre(criacao, aprovacao, 0.75)));
        await _backdate.AplicarAsync<Domain.Entities.Concessionaria.PropostaVenda>(propostaId, valores.ToArray());

        var vistoria = Lim(item.DataVistoria ?? LimEntre(aprovacao, aprovacao.AddDays(1)));
        if (rastro.VistoriaId is Guid vistoriaId)
        {
            var v = new List<(string, object?)> { ("DataCriacao", vistoria), ("DataRealizada", vistoria) };
            if (rastro.VistoriaConcluida) v.Add(("DataConclusao", LimEntre(vistoria, vistoria.AddHours(1))));
            await _backdate.AplicarAsync<Domain.Entities.Concessionaria.Vistoria>(vistoriaId, v.ToArray());
        }

        var ultimoPagamento = vistoria;
        foreach (var (pagamentoId, data) in rastro.Pagamentos)
        {
            var d = Lim(data ?? LimEntre(vistoria, vistoria.AddHours(3)));
            if (d > ultimoPagamento) ultimoPagamento = d;
            await _backdate.AplicarAsync<Domain.Entities.Concessionaria.PagamentoProposta>(pagamentoId,
                ("DataCriacao", d), ("DataPagamento", d));
        }

        if (rastro.TermoId is Guid termoId)
        {
            var termo = Lim(item.DataTermo ?? LimEntre(ultimoPagamento, ultimoPagamento.AddHours(2)));
            var t = new List<(string, object?)> { ("DataCriacao", termo), ("DataRedacao", termo) };
            if (rastro.TermoAssinado) t.Add(("DataAssinatura", Lim(item.DataAssinatura ?? LimEntre(termo, termo.AddHours(4)))));
            await _backdate.AplicarAsync<Domain.Entities.Concessionaria.TermoEntrega>(termoId, t.ToArray());
        }
    }

    // ============================================================
    // ORDENS DE SERVIÇO — funil completo
    // ============================================================

    /// <summary>Cenários que só existem no fluxo novo com vistoria de entrada (doc 32).</summary>
    private static readonly HashSet<string> CenariosOSComVistoria = new() { "emVistoria", "aguardandoCliente", "orcamentoRecusado", "aprovada" };

    /// <summary>O que o funil da OS realmente criou — base pro backdate das datas secundárias.</summary>
    private sealed class RastroOS
    {
        public bool UsaVistoria;
        public Guid? VistoriaId;
        public bool VistoriaConcluida;
        public List<Guid> RequisicoesAtendidas { get; } = new();
        public List<Guid> RequisicoesPendentes { get; } = new();
        public Guid? RequisicaoRejeitadaId;
        public bool TeveEntradaEstoque;
        public Guid? AlertaId;
        public bool AlertaResolvido;
        public List<(Guid Id, DateTime? Data, bool Quitacao)> Pagamentos { get; } = new();
    }

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

                Guid? recepcionistaId = null;
                if (!string.IsNullOrWhiteSpace(item.RecepcionistaChave)
                    && TentarResolver(chaves, item.RecepcionistaChave, out var recId, resultado.Avisos, "ordem de serviço", item.Chave, "recepcionista"))
                    recepcionistaId = recId;

                // Itens de Estoque/Cliente entram direto na criação da OS. Itens de
                // Encomenda só nascem via fluxo de requisição de peça atendida (ver
                // abaixo) — a criação da OS rejeitaria Origem=Encomenda diretamente.
                var itensDiretos = new List<ItemOrdemServicoDTO>();
                var pecasDescricao = new List<string>();
                var itensEncomenda = new List<(string ComponenteChave, Guid ComponenteId, string Nome, int Quantidade, decimal? ValorUnitario)>();
                foreach (var itemComponente in item.Itens)
                {
                    var quantidade = Math.Max(1, itemComponente.Quantidade);
                    var origem = string.IsNullOrWhiteSpace(itemComponente.Origem) ? "Estoque" : itemComponente.Origem;

                    // Peça trazida pelo cliente sem cadastro no catálogo: só o nome.
                    if (string.IsNullOrWhiteSpace(itemComponente.ComponenteChave)
                        && string.Equals(origem, "Cliente", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(itemComponente.DescricaoLivre))
                    {
                        itensDiretos.Add(new ItemOrdemServicoDTO { Quantidade = quantidade, Origem = "Cliente", DescricaoLivre = itemComponente.DescricaoLivre });
                        pecasDescricao.Add($"{quantidade}x {itemComponente.DescricaoLivre} (cliente)");
                        continue;
                    }

                    if (!TentarResolver(chaves, itemComponente.ComponenteChave, out var componenteId, resultado.Avisos, "ordem de serviço", item.Chave, "componente")) continue;

                    var compDetalhe = await _componenteService.GetByIdAsync(componenteId);
                    if (!compDetalhe.IsSuccess || compDetalhe.Value is null)
                    {
                        resultado.Avisos.Add($"OS \"{item.Chave}\": componente \"{itemComponente.ComponenteChave}\" não encontrado — item pulado.");
                        continue;
                    }

                    pecasDescricao.Add($"{quantidade}x {compDetalhe.Value.Nome}" +
                        (string.Equals(origem, "Estoque", StringComparison.OrdinalIgnoreCase) ? "" : $" ({origem.ToLowerInvariant()})"));

                    if (string.Equals(origem, "Encomenda", StringComparison.OrdinalIgnoreCase))
                    {
                        itensEncomenda.Add((itemComponente.ComponenteChave, componenteId, compDetalhe.Value.Nome, quantidade, itemComponente.ValorUnitario));
                        continue;
                    }

                    itensDiretos.Add(new ItemOrdemServicoDTO
                    {
                        ComponenteId = componenteId,
                        Quantidade = quantidade,
                        ValorUnitario = itemComponente.ValorUnitario ?? compDetalhe.Value.ValorVenda,
                        Origem = origem,
                        // Peça trazida pelo cliente não referencia o catálogo — a
                        // OS só registra o nome (sem isso o item era descartado).
                        DescricaoLivre = string.Equals(origem, "Cliente", StringComparison.OrdinalIgnoreCase) ? compDetalhe.Value.Nome : null
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
                foreach (var ck in item.Checklist ?? new())
                {
                    if (string.IsNullOrWhiteSpace(ck.Descricao)) continue;
                    var rck = await _ordemServicoService.AdicionarItemChecklistAsync(new AdicionarChecklistItemDTO
                    {
                        OrdemServicoId = ordemId, Titulo = ck.Descricao, Descricao = ck.Descricao
                    });
                    if (!rck.IsSuccess) resultado.Avisos.Add($"OS \"{item.Chave}\": item de checklist \"{ck.Descricao}\" não adicionado — {rck.Error}");
                }
                var rastro = new RastroOS();
                var despesasAntes = await IdsItensDespesaMesAtualAsync();

                await AvancarFunilOrdemServicoAsync(ordemId, item, clienteId, veiculoId, mecanicoId, recepcionistaId,
                    itensEncomenda, pecasDescricao, rastro, resultado.Avisos);

                RegistrarChave(chaves, item.Chave, ordemId, resultado.Avisos, "ordem de serviço");
                if (item.DataCriacao.HasValue)
                    await AplicarDatasOrdemServicoAsync(ordemId, item, rastro, despesasAntes);

                resultado.OrdensServicoCriadas++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"OS \"{item.Chave}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar ordem de serviço {Chave}", item.Chave);
            }
            finally { _backdate.LimparRastreamento(); }
        }
    }

    private async Task AvancarFunilOrdemServicoAsync(
        Guid ordemId, OrdemServicoImportDTO item, Guid clienteId, Guid veiculoId, Guid mecanicoId, Guid? recepcionistaId,
        List<(string ComponenteChave, Guid ComponenteId, string Nome, int Quantidade, decimal? ValorUnitario)> itensEncomenda,
        List<string> pecasDescricao, RastroOS rastro, List<string> avisos)
    {
        void Parar(string etapaQueFalhou, string? erro)
            => avisos.Add($"OS \"{item.Chave}\": não foi possível {etapaQueFalhou} — {erro ?? "motivo desconhecido"}.");

        var cenario = item.Cenario;
        var recebidoPor = recepcionistaId ?? mecanicoId;

        if (CenariosOSComVistoria.Contains(cenario) && recepcionistaId is null)
        {
            Parar($"levar ao cenário \"{cenario}\"", "esse cenário passa pela vistoria de entrada e exige \"recepcionistaChave\"");
            return;
        }

        // 1) Vistoria de entrada (doc 32) — recepcionista abre a vistoria e já
        // deixa o contrato rascunhado (template "Contrato de OS" preenchido).
        rastro.UsaVistoria = recepcionistaId is not null && cenario is not ("pendente" or "cancelada");
        string? textoVistoria = null;
        if (rastro.UsaVistoria)
        {
            var riv = await _ordemServicoService.IniciarVistoriaAsync(ordemId, recepcionistaId!.Value);
            if (!riv.IsSuccess || riv.Value is null) { Parar("iniciar a vistoria de entrada", riv.Error); return; }
            rastro.VistoriaId = riv.Value.Id;

            textoVistoria = !string.IsNullOrWhiteSpace(item.TextoVistoria)
                ? item.TextoVistoria
                : await MontarContratoOSAsync(ordemId, item, clienteId, veiculoId, recepcionistaId.Value, pecasDescricao);
            var red = await _ordemServicoService.EditarVistoriaAsync(ordemId, textoVistoria);
            if (!red.IsSuccess) Parar("redigir o contrato da vistoria", red.Error);
        }

        // 2) Peças durante a montagem do orçamento (Pendente/EmVistoria —
        // único momento em que o domínio aceita requisição): rejeitada,
        // encomendas atendidas (+ chegada no estoque) ou deixadas pendentes.
        var deixarPendente = cenario == "aguardandoPeca";
        var abriuRequisicao = false;

        if (item.RequisicaoRejeitada is { } rej && !string.IsNullOrWhiteSpace(rej.DescricaoPeca))
        {
            var rAbrir = await _requisicaoPecaService.AbrirAsync(ordemId, mecanicoId, new CriarRequisicaoPecaDTO
            {
                DescricaoPeca = rej.DescricaoPeca,
                Justificativa = "Peça necessária para o orçamento, sem estoque no momento.",
                Quantidade = 1
            });
            if (!rAbrir.IsSuccess || rAbrir.Value is null) Parar($"abrir a requisição de \"{rej.DescricaoPeca}\"", rAbrir.Error);
            else
            {
                abriuRequisicao = true;
                rastro.RequisicaoRejeitadaId = rAbrir.Value.Id;
                var rRej = await _requisicaoPecaService.RejeitarAsync(rAbrir.Value.Id, _adminId, new RejeitarRequisicaoDTO
                {
                    Motivo = string.IsNullOrWhiteSpace(rej.Motivo) ? "Peça substituída por um componente compatível já em estoque." : rej.Motivo
                });
                if (!rRej.IsSuccess) Parar($"rejeitar a requisição de \"{rej.DescricaoPeca}\"", rRej.Error);
            }
        }

        foreach (var (componenteChave, componenteId, nome, quantidade, valorUnitario) in itensEncomenda)
        {
            var rAbrir = await _requisicaoPecaService.AbrirAsync(ordemId, mecanicoId, new CriarRequisicaoPecaDTO
            {
                DescricaoPeca = nome,
                Justificativa = "Peça não disponível em estoque no momento do orçamento.",
                Quantidade = quantidade
            });
            if (!rAbrir.IsSuccess || rAbrir.Value is null)
            {
                Parar($"abrir requisição para o componente \"{componenteChave}\"", rAbrir.Error);
                continue;
            }
            abriuRequisicao = true;

            if (deixarPendente)
            {
                rastro.RequisicoesPendentes.Add(rAbrir.Value.Id);
                continue;
            }

            var rAtender = await _requisicaoPecaService.AtenderAsync(rAbrir.Value.Id, _adminId, new AtenderRequisicaoDTO
            {
                ComponenteId = componenteId,
                Quantidade = quantidade,
                ValorUnitario = valorUnitario
            });
            if (!rAtender.IsSuccess)
            {
                Parar($"atender a requisição do componente \"{componenteChave}\"", rAtender.Error);
                continue;
            }
            rastro.RequisicoesAtendidas.Add(rAbrir.Value.Id);

            // A peça encomendada chega: entrada no estoque (lança a despesa de
            // compra e marca o item da OS como Recebido — conciliação do
            // EstoqueService) e sai na mesma hora pro carro do cliente, sem
            // inflar o saldo de estoque.
            var rEntrada = await _estoqueService.EntradaAsync(componenteId, quantidade);
            if (!rEntrada.IsSuccess) Parar($"registrar a chegada do componente \"{componenteChave}\"", rEntrada.Error);
            else
            {
                rastro.TeveEntradaEstoque = true;
                var rSaida = await _estoqueService.SaidaAsync(componenteId, quantidade);
                if (!rSaida.IsSuccess) Parar($"dar baixa do componente \"{componenteChave}\" aplicado na OS", rSaida.Error);
            }
        }

        if (deixarPendente && rastro.RequisicoesPendentes.Count == 0)
        {
            // Cenário pede peça pendente mas a OS não tem item de encomenda —
            // abre uma requisição genérica pro orçamento.
            var rAbrir = await _requisicaoPecaService.AbrirAsync(ordemId, mecanicoId, new CriarRequisicaoPecaDTO
            {
                DescricaoPeca = $"Peça para orçamento — {item.Descricao}",
                Justificativa = "Peça não cadastrada/sem estoque; aguardando cotação com fornecedor.",
                Quantidade = 1
            });
            if (!rAbrir.IsSuccess || rAbrir.Value is null) Parar("abrir a requisição de peça pendente", rAbrir.Error);
            else rastro.RequisicoesPendentes.Add(rAbrir.Value.Id);
        }
        if (deixarPendente) return;

        if (abriuRequisicao)
        {
            var rLiberar = await _requisicaoPecaService.LiberarOrdemAsync(ordemId);
            if (!rLiberar.IsSuccess) Parar("liberar a OS após resolver as requisições", rLiberar.Error);
        }

        if (cenario == "pendente") return;
        if (cenario == "cancelada")
        {
            var rc = await _ordemServicoService.CancelarAsync(ordemId);
            if (!rc.IsSuccess) Parar("cancelar a OS", rc.Error);
            return;
        }
        if (cenario == "emVistoria") return;

        // 3) Conclui a vistoria → AguardandoCliente → cliente decide.
        if (rastro.UsaVistoria)
        {
            var rcv = await _ordemServicoService.ConcluirVistoriaAsync(ordemId, textoVistoria!);
            if (!rcv.IsSuccess) { Parar("concluir a vistoria de entrada", rcv.Error); return; }
            rastro.VistoriaConcluida = true;
            if (cenario == "aguardandoCliente") return;

            if (cenario == "orcamentoRecusado")
            {
                var rc = await _ordemServicoService.CancelarAsync(ordemId);
                if (!rc.IsSuccess) Parar("cancelar a OS após o cliente recusar o orçamento", rc.Error);
                return;
            }

            var rap = await _ordemServicoService.RegistrarAprovacaoDoClienteAsync(ordemId);
            if (!rap.IsSuccess) { Parar("registrar a aprovação do cliente", rap.Error); return; }
            if (cenario == "aprovada") return;
        }

        var ri = await _ordemServicoService.IniciarAsync(ordemId);
        if (!ri.IsSuccess) { Parar("iniciar a OS", ri.Error); return; }

        // Checklist explícito: itens já feitos ficam marcados (só dá pra marcar
        // em andamento — antes de um eventual alerta pausar a OS).
        var feitos = item.Checklist?.Where(c => c.Concluido).Select(c => c.Descricao).ToList();
        if (feitos is { Count: > 0 })
        {
            var rck = await ConcluirChecklistAsync(ordemId, feitos);
            if (!rck.IsSuccess) Parar("marcar os itens de checklist já concluídos", rck.Error);
        }

        // Desistência no meio do serviço — diferente de "cancelada" (que
        // cancela ainda pendente, antes de qualquer trabalho começar).
        if (cenario == "canceladaEmAndamento")
        {
            await RegistrarPagamentosOSAsync(ordemId, item, recebidoPor, quitar: false, rastro, avisos);
            var rcEm = await _ordemServicoService.CancelarAsync(ordemId);
            if (!rcEm.IsSuccess) Parar("cancelar a OS em andamento", rcEm.Error);
            return;
        }

        // 4) Alerta de problema adicional: mecânico emite (OS pausa) e o
        // cliente decide — aprovado/recusado retomam; pendente deixa Pausada.
        var alerta = item.Alerta
            ?? (cenario == "pausada" ? new AlertaImportDTO { Decisao = "pendente" }
                : item.ComAlerta ? new AlertaImportDTO { Decisao = "aprovado" } : null);
        if (alerta is not null)
        {
            var decisao = cenario == "pausada" ? "pendente" : (alerta.Decisao ?? "aprovado").Trim().ToLowerInvariant();
            var ra = await _alertaOSService.EmitirAsync(ordemId, mecanicoId, new CriarAlertaOSDTO
            {
                Descricao = string.IsNullOrWhiteSpace(alerta.Descricao) ? "Mecânico identificou um problema adicional durante o serviço." : alerta.Descricao
            });
            if (!ra.IsSuccess || ra.Value is null) Parar("emitir alerta de problema adicional", ra.Error);
            else
            {
                rastro.AlertaId = ra.Value.Id;
                if (decisao == "pendente")
                {
                    await RegistrarPagamentosOSAsync(ordemId, item, recebidoPor, quitar: false, rastro, avisos);
                    return;
                }

                var aprovou = decisao != "recusado";
                var rr = await _alertaOSService.ResolverAsync(ra.Value.Id, recebidoPor, new ResolverAlertaDTO
                {
                    Aprovou = aprovou,
                    ObservacaoCliente = !string.IsNullOrWhiteSpace(alerta.ObservacaoCliente) ? alerta.ObservacaoCliente
                        : aprovou ? "Cliente aprovou o serviço adicional." : "Cliente recusou o serviço adicional; seguir só com o orçamento original."
                });
                if (!rr.IsSuccess) Parar("registrar a decisão do cliente sobre o alerta", rr.Error);
                else rastro.AlertaResolvido = true;
            }
        }

        if (cenario is "emAndamento" or "pausada")
        {
            await RegistrarPagamentosOSAsync(ordemId, item, recebidoPor, quitar: false, rastro, avisos);
            return;
        }

        // Finalizar exige a checklist inteira concluída (OrdemServico.Finalizar).
        var rConcluirChecklist = await ConcluirChecklistAsync(ordemId);
        if (!rConcluirChecklist.IsSuccess) { Parar("concluir a checklist da OS", rConcluirChecklist.Error); return; }

        // "finalizadaPendente": serviço pronto, falta cobrar (FinalizarAsync
        // exige pagamento total antes) — fica EmAndamento, com sinal se houver.
        if (cenario == "finalizadaPendente")
        {
            await RegistrarPagamentosOSAsync(ordemId, item, recebidoPor, quitar: false, rastro, avisos);
            return;
        }

        if (!await RegistrarPagamentosOSAsync(ordemId, item, recebidoPor, quitar: true, rastro, avisos)) return;

        var rf = await _ordemServicoService.FinalizarAsync(ordemId);
        if (!rf.IsSuccess) { Parar("finalizar a OS", rf.Error); return; }
        if (cenario == "finalizadaPaga") return;

        var re = await _ordemServicoService.EntregarAsync(ordemId);
        if (!re.IsSuccess) Parar("marcar a OS como entregue", re.Error);
    }

    /// <summary>
    /// Registra os pagamentos da OS. Quitação: lista (último = saldo exato)
    /// ou 1 pagamento integral (padrão antigo). Parcial: só a lista, sem
    /// ajuste (sinal de OS em andamento) — sem lista, nada é cobrado.
    /// </summary>
    private async Task<bool> RegistrarPagamentosOSAsync(
        Guid ordemId, OrdemServicoImportDTO item, Guid recebidoPor, bool quitar, RastroOS rastro, List<string> avisos)
    {
        if (!quitar && item.Pagamentos is not { Count: > 0 }) return true;

        var detalhe = await _ordemServicoService.GetByIdAsync(ordemId);
        if (!detalhe.IsSuccess || detalhe.Value is null)
        {
            avisos.Add($"OS \"{item.Chave}\": não foi possível reler a OS antes do pagamento — {detalhe.Error}.");
            return false;
        }

        var modoPadrao = NormalizarModo(item.ModoPagamento) ?? "Pix";
        var pagamentos = item.Pagamentos is { Count: > 0 }
            ? CalcularPagamentos(detalhe.Value.ValorTotal, item.Pagamentos, modoPadrao, quitar)
            : new List<(string, decimal, DateTime?)> { (modoPadrao, detalhe.Value.ValorTotal, null) };

        for (var i = 0; i < pagamentos.Count; i++)
        {
            var (modo, valor, data) = pagamentos[i];
            var obs = !quitar ? "Sinal recebido na recepção."
                : pagamentos.Count == 1 ? "Pagamento integral registrado na recepção."
                : $"Pagamento dividido ({i + 1}/{pagamentos.Count}) registrado na recepção.";
            var rp = await _pagamentoOrdemServicoService.RegistrarPagamentoAsync(ordemId, recebidoPor, new RegistrarPagamentoDTO
            {
                ModoPagamento = modo,
                Valor = valor,
                Observacoes = obs
            });
            if (!rp.IsSuccess || rp.Value is null)
            {
                avisos.Add($"OS \"{item.Chave}\": não foi possível registrar o pagamento {i + 1} ({modo}, R$ {valor:N2}) — {rp.Error}.");
                return false;
            }
            rastro.Pagamentos.Add((rp.Value.Id, data, quitar && i == pagamentos.Count - 1));
        }
        return true;
    }

    /// <summary>Contrato da vistoria de entrada: template padrão "Contrato de OS" com os dados reais da OS.</summary>
    private async Task<string> MontarContratoOSAsync(
        Guid ordemId, OrdemServicoImportDTO item, Guid clienteId, Guid veiculoId, Guid recepcionistaId,
        List<string> pecas)
    {
        var detalhe = await _ordemServicoService.GetByIdAsync(ordemId);
        var cliente = await ObterClienteAsync(clienteId);
        _veiculos.TryGetValue(veiculoId, out var veic);
        var dataVistoria = item.DataVistoria ?? item.DataCriacao?.AddMinutes(30) ?? DateTime.Now;
        var prazo = (item.DataCriacao ?? DateTime.Now).AddDays(Math.Max(1, item.PrazoDiasAPartirDaCriacao));


        return Preencher(TemplatesDocumentosPadrao.ContratoOS, new Dictionary<string, string?>
        {
            ["NÚMERO DA OS"] = detalhe.Value?.NumeroPublico,
            ["NOME DO CLIENTE"] = cliente.Nome,
            ["MARCA E MODELO"] = veic is null ? null : $"{veic.Marca} {veic.Modelo}",
            ["PLACA"] = veic?.Placa,
            ["DATA E HORA"] = dataVistoria.ToString("dd/MM/yyyy HH:mm"),
            ["NOME DO RECEPCIONISTA"] = _usuarios.GetValueOrDefault(recepcionistaId),
            ["NÍVEL"] = "1/2 tanque",
            ["DESCRIÇÃO DO SERVIÇO"] = item.Descricao,
            ["LISTAR PEÇAS, SE JÁ IDENTIFICADAS"] = pecas.Count > 0 ? string.Join("; ", pecas) : "a definir no diagnóstico",
            ["OBSERVAÇÕES"] = "nenhuma observação adicional",
            ["DATA PREVISTA"] = prazo.ToString("dd/MM/yyyy"),
        })
        .Replace("[DESCREVER — LATARIA, PNEUS, VIDROS,\nBANCOS, PAINEL]", "lataria, pneus, vidros, bancos e painel em bom estado de conservação")
        .Replace("[DESCREVER OU\n\"NENHUMA AVARIA VISÍVEL\"]", "nenhuma avaria visível")
        .Replace("[DESCREVER OU \"NENHUM\"]", "nenhum");
    }

    /// <summary>
    /// Linha do tempo da OS: abertura → vistoria → (peças) → aprovação do
    /// cliente → início → alerta → pagamentos/finalização. Campos do JSON
    /// têm prioridade; o resto é derivado da etapa anterior, nunca "hoje".
    /// Despesa de compra de peça encomendada vai pra competência da chegada.
    /// </summary>
    private async Task AplicarDatasOrdemServicoAsync(Guid ordemId, OrdemServicoImportDTO item, RastroOS rastro, HashSet<Guid> despesasAntes)
    {
        var criacao = item.DataCriacao!.Value;
        var prazoDias = Math.Max(1, item.PrazoDiasAPartirDaCriacao);
        await _backdate.AplicarAsync<Domain.Entities.Oficina.OrdemServico>(
            ordemId, ("DataCriacao", criacao), ("PrazoEstimado", criacao.AddDays(prazoDias)));

        await _backdate.AplicarOndeAsync<Domain.Entities.Oficina.ChecklistOrdemServico>(c => c.OrdemServicoId == ordemId, ("DataCriacao", criacao));
        await _backdate.AplicarOndeAsync<Domain.Entities.Oficina.ItemOrdemServico>(i => i.OrdemServicoId == ordemId, ("DataCriacao", criacao));

        var vistoria = Lim(item.DataVistoria ?? LimEntre(criacao, criacao.AddMinutes(30)));
        var aprovacaoCliente = Lim(item.DataAprovacaoCliente ?? LimEntre(vistoria, vistoria.AddHours(2)));
        var inicio = Lim(item.DataInicio ?? (rastro.UsaVistoria ? LimEntre(aprovacaoCliente, aprovacaoCliente.AddHours(1)) : LimEntre(criacao, criacao.AddHours(2))));
        var finalizacao = Lim(item.DataFinalizacao ?? LimEntre(inicio, Max(inicio.AddHours(4), criacao.AddDays(prazoDias - 1))));

        if (rastro.VistoriaId is Guid vistoriaId)
        {
            var v = new List<(string, object?)> { ("DataCriacao", vistoria), ("DataInicio", vistoria) };
            if (rastro.VistoriaConcluida)
                v.Add(("DataConclusao", LimEntre(vistoria, Min(vistoria.AddMinutes(45), Entre(vistoria, aprovacaoCliente, 0.5)))));
            await _backdate.AplicarAsync<Domain.Entities.Oficina.VistoriaOrdemServico>(vistoriaId, v.ToArray());
        }

        // Peças: abertas logo após a vistoria (ou a abertura) e resolvidas
        // antes do início do serviço.
        var baseReq = rastro.UsaVistoria ? vistoria : criacao;
        var aberturaReq = LimEntre(baseReq, baseReq.AddHours(1));
        if (rastro.RequisicaoRejeitadaId is Guid rejId)
        {
            var dataRej = Lim(item.RequisicaoRejeitada?.Data ?? aberturaReq);
            await _backdate.AplicarAsync<Domain.Entities.Oficina.RequisicaoPecaOS>(rejId,
                ("DataCriacao", dataRej), ("DataResolucao", LimEntre(dataRej, dataRej.AddHours(3))));
        }
        foreach (var reqId in rastro.RequisicoesPendentes)
            await _backdate.AplicarAsync<Domain.Entities.Oficina.RequisicaoPecaOS>(reqId, ("DataCriacao", aberturaReq));

        var chegadaPecas = aberturaReq;
        if (rastro.RequisicoesAtendidas.Count > 0)
        {
            var limite = inicio > aberturaReq ? inicio : LimEntre(aberturaReq, aberturaReq.AddDays(2));
            var resolucao = Lim(Entre(aberturaReq, limite, 0.4));
            chegadaPecas = Lim(Entre(aberturaReq, limite, 0.8));
            foreach (var reqId in rastro.RequisicoesAtendidas)
                await _backdate.AplicarAsync<Domain.Entities.Oficina.RequisicaoPecaOS>(reqId,
                    ("DataCriacao", aberturaReq), ("DataResolucao", resolucao));
            await _backdate.AplicarOndeAsync<Domain.Entities.Oficina.ItemOrdemServico>(
                i => i.OrdemServicoId == ordemId && i.DataRecebimento != null, ("DataRecebimento", chegadaPecas));
        }
        if (rastro.TeveEntradaEstoque && !_despesasNoArquivo)
            await MoverDespesasNovasParaCompetenciaAsync(despesasAntes, chegadaPecas);

        if (rastro.AlertaId is Guid alertaId)
        {
            var dataAlerta = Lim(item.Alerta?.Data ?? Entre(inicio, finalizacao, 0.4));
            var a = new List<(string, object?)> { ("DataCriacao", dataAlerta) };
            if (rastro.AlertaResolvido) a.Add(("DataResolucao", Lim(item.Alerta?.DataDecisao ?? LimEntre(dataAlerta, dataAlerta.AddHours(4)))));
            await _backdate.AplicarAsync<Domain.Entities.Oficina.AlertaOS>(alertaId, a.ToArray());
        }

        foreach (var (pagamentoId, data, quitacao) in rastro.Pagamentos)
        {
            var d = Lim(data ?? (quitacao ? finalizacao : inicio));
            await _backdate.AplicarAsync<Domain.Entities.Oficina.PagamentoOrdemServico>(pagamentoId,
                ("DataCriacao", d), ("DataPagamento", d));
        }
    }

    private async Task<Result> ConcluirChecklistAsync(Guid ordemId, List<string>? somenteDescricoes = null)
    {
        var detalhe = await _ordemServicoService.GetByIdAsync(ordemId);
        if (!detalhe.IsSuccess || detalhe.Value is null) return Result.Fail(detalhe.Error ?? "OS não encontrada.");

        var restantes = somenteDescricoes is null ? null : new List<string>(somenteDescricoes);
        foreach (var itemChecklist in detalhe.Value.Checklist.Where(c => c.Status != "Concluido"))
        {
            if (restantes is not null)
            {
                // Só os itens pedidos (casando pela descrição, 1 a 1).
                if (!restantes.Remove(itemChecklist.Descricao)) continue;
            }
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
                // Veículo próprio (VeiculoVendaChave) ou consignado (doc 31).
                var consignado = !string.IsNullOrWhiteSpace(item.VeiculoConsignadoChave);
                Guid veiculoId;
                if (consignado)
                {
                    if (!TentarResolver(chaves, item.VeiculoConsignadoChave!, out veiculoId, resultado.Avisos, "test drive", item.Chave, "veículo consignado")) continue;
                }
                else if (!TentarResolver(chaves, item.VeiculoVendaChave, out veiculoId, resultado.Avisos, "test drive", item.Chave, "veículo")) continue;
                if (!TentarResolver(chaves, item.ClienteChave, out var clienteId, resultado.Avisos, "test drive", item.Chave, "cliente")) continue;
                if (!TentarResolver(chaves, item.VendedorChave, out var vendedorId, resultado.Avisos, "test drive", item.Chave, "vendedor")) continue;

                var reagendado = item.Cenario == "reagendado";
                var dataOriginal = reagendado ? item.DataReagendamentoOriginal ?? item.DataHora.AddDays(-2) : item.DataHora;

                // Termo de responsabilidade: template padrão preenchido com os
                // dados reais (o service exige texto no agendamento).
                var cliente = await ObterClienteAsync(clienteId);
                _veiculos.TryGetValue(veiculoId, out var veic);
                var textoTermo = Preencher(TemplatesDocumentosPadrao.TermoTestDrive, new Dictionary<string, string?>
                {
                    ["NOME DO CLIENTE"] = cliente.Nome,
                    ["CPF DO CLIENTE"] = FormatarCpf(cliente.Cpf),
                    ["CATEGORIA DA\nCNH"] = "B",
                    ["MARCA E MODELO"] = veic is null ? null : $"{veic.Marca} {veic.Modelo}",
                    ["ANO"] = veic?.Ano.ToString(),
                    ["PLACA"] = veic?.Placa,
                    ["DATA E HORA"] = item.DataHora.ToString("dd/MM/yyyy HH:mm"),
                    ["COM/SEM ACOMPANHAMENTO DE UM VENDEDOR"] = $"com acompanhamento do vendedor {_usuarios.GetValueOrDefault(vendedorId) ?? ""}".TrimEnd(),
                    ["DATA"] = item.DataHora.ToString("dd/MM/yyyy"),
                });

                // AgendarAsync aceita data passada — o agendamento já nasce com a
                // data/hora real do passeio (ou a original, se foi reagendado).
                var r = await _testDriveService.AgendarAsync(new CriarTestDriveDTO
                {
                    VeiculoVendaId = veiculoId,
                    VeiculoEntidadeTipo = consignado ? "VeiculoConsignacao" : "VeiculoVenda",
                    ClienteId = clienteId,
                    VendedorId = vendedorId,
                    DataHora = dataOriginal,
                    Observacao = item.Observacao,
                    TextoTermo = textoTermo
                });
                if (!r.IsSuccess)
                {
                    resultado.Avisos.Add($"Test drive \"{item.Chave}\": {r.Error}");
                    continue;
                }

                var testDriveId = r.Value;
                if (reagendado)
                {
                    var rr = await _testDriveService.ReagendarAsync(testDriveId, item.DataHora);
                    if (!rr.IsSuccess) resultado.Avisos.Add($"Test drive \"{item.Chave}\": não foi possível reagendar — {rr.Error}");
                }

                // Termo: assinado antes de sair (padrão do realizado), enviado
                // (link pendente) ou só rascunho.
                var termo = (item.Termo ?? (item.Cenario == "realizado" ? "assinado" : "rascunho")).Trim().ToLowerInvariant();
                var assinou = false;
                if (termo is "enviado" or "assinado")
                {
                    var re = await _testDriveService.EnviarTermoParaAssinaturaAsync(testDriveId);
                    if (!re.IsSuccess) resultado.Avisos.Add($"Test drive \"{item.Chave}\": não foi possível enviar o termo — {re.Error}");
                    else if (termo == "assinado")
                    {
                        var t = await _testDriveService.ObterTermoAsync(testDriveId);
                        if (!t.IsSuccess || string.IsNullOrEmpty(t.Value?.TokenAssinatura))
                            resultado.Avisos.Add($"Test drive \"{item.Chave}\": não foi possível reler o termo para assinar — {t.Error}");
                        else
                        {
                            var ra = await _testDriveService.AssinarTermoAsync(t.Value!.TokenAssinatura!,
                                new AssinarTermoTestDriveDTO { NomeCliente = cliente.Nome, CpfCliente = cliente.Cpf, Aceite = true }, "127.0.0.1");
                            if (!ra.IsSuccess) resultado.Avisos.Add($"Test drive \"{item.Chave}\": não foi possível assinar o termo — {ra.Error}");
                            else assinou = true;
                        }
                    }
                }

                var statusFinal = item.Cenario switch
                {
                    "realizado" => "Realizado",
                    "cancelado" => "Cancelado",
                    "naoCompareceu" => "NaoCompareceu",
                    _ => null
                };
                if (statusFinal is not null)
                {
                    // Realizado lança R$ 50 de combustível "hoje" (doc 30) —
                    // captura o que já existia pra mover só o lançamento novo.
                    var despesasAntes = statusFinal == "Realizado" ? await IdsItensDespesaMesAtualAsync() : null;
                    var rs = await _testDriveService.AtualizarStatusAsync(new AtualizarStatusTestDriveDTO
                    {
                        Id = testDriveId,
                        Status = statusFinal
                    });
                    if (!rs.IsSuccess)
                        resultado.Avisos.Add($"Test drive \"{item.Chave}\": não foi possível marcar como \"{item.Cenario}\" — {rs.Error}");
                    else if (despesasAntes is not null && !_despesasNoArquivo)
                        await MoverDespesasNovasParaCompetenciaAsync(despesasAntes, Lim(item.DataHora));
                }

                // Linha do tempo: agendado alguns dias antes do passeio; termo
                // redigido no agendamento e assinado pouco antes de sair.
                var agendamento = Lim(dataOriginal.AddDays(-3));
                await _backdate.AplicarAsync<Domain.Entities.Concessionaria.TestDrive>(testDriveId, ("DataCriacao", agendamento));
                var termoValores = new List<(string, object?)> { ("DataCriacao", agendamento), ("DataRedacao", agendamento) };
                if (assinou) termoValores.Add(("DataAssinatura", Lim(Max(agendamento, item.DataHora.AddMinutes(-15)))));
                await _backdate.AplicarOndeAsync<Domain.Entities.Concessionaria.TermoTestDrive>(
                    t => t.TestDriveId == testDriveId, termoValores.ToArray());

                RegistrarChave(chaves, item.Chave, testDriveId, resultado.Avisos, "test drive");
                resultado.TestDrivesCriados++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Test drive \"{item.Chave}\": erro inesperado ao criar ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar test drive {Chave}", item.Chave);
            }
            finally { _backdate.LimparRastreamento(); }
        }
    }

    /// <summary>
    /// Modo "arquivo exportado": remove do mês atual toda despesa não-modelo
    /// lançada pelas regras de negócio durante esta importação — o arquivo já
    /// traz cada uma (em despesasExtras) na competência original.
    /// </summary>
    private async Task DescartarDespesasAutomaticasDaImportacaoAsync()
    {
        var hoje = DateTime.Today;
        var r = await _balancoDespesaService.ObterAsync(hoje.Year, hoje.Month);
        if (!r.IsSuccess || r.Value is null) return;
        foreach (var item in r.Value.Itens.Where(i => !i.DoModelo && !_despesasMesAtualAntes.Contains(i.Id)))
            await _balancoDespesaService.RemoverItemAsync(hoje.Year, hoje.Month, item.Id);
        _backdate.LimparRastreamento();
    }

    /// <summary>
    /// Balanços de meses passados gerados durante a importação nasceram
    /// "hoje" (DataCriacao) — passa a ser o 1º dia da própria competência.
    /// </summary>
    private async Task AjustarDatasBalancosAsync()
    {
        try
        {
            var mesAtual = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
            var balancos = await _backdate.ConsultarAsync<BalancoMensalDespesa, KeyValuePair<Guid, DateOnly>>(
                b => b.Competencia < mesAtual, b => new KeyValuePair<Guid, DateOnly>(b.Id, b.Competencia));
            foreach (var (id, competencia) in balancos)
            {
                var data = competencia.ToDateTime(new TimeOnly(8, 0));
                await _backdate.AplicarAsync<BalancoMensalDespesa>(id, ("DataCriacao", data));
                await _backdate.AplicarOndeAsync<ItemBalancoDespesa>(i => i.BalancoId == id, ("DataCriacao", data));
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "Erro ao ajustar datas dos balanços mensais"); }
        finally { _backdate.LimparRastreamento(); }
    }

    // ============================================================
    // FECHAMENTOS MENSAIS — variação dos itens do modelo + fechamento
    // (sempre a última etapa: balanço fechado rejeita item novo)
    // ============================================================
    private async Task ImportarFechamentosMensaisAsync(List<FechamentoMensalImportDTO> itens, ImportacaoResultadoDTO resultado)
    {
        foreach (var item in itens)
        {
            var rotulo = $"{item.Mes:00}/{item.Ano}";
            try
            {
                if (item.Mes is < 1 or > 12 || item.Ano < 2000)
                {
                    resultado.Avisos.Add($"Fechamento mensal \"{rotulo}\": competência inválida — ignorado.");
                    continue;
                }

                // "Já existe um balanço" é esperado (compra/despesa automática caiu no mês).
                await _balancoDespesaService.GerarDoModeloAsync(item.Ano, item.Mes);

                var balanco = await _balancoDespesaService.ObterAsync(item.Ano, item.Mes);
                if (!balanco.IsSuccess || balanco.Value is null || !balanco.Value.ExisteNoBanco)
                {
                    resultado.Avisos.Add($"Fechamento mensal \"{rotulo}\": balanço não encontrado — {balanco.Error}");
                    continue;
                }
                if (balanco.Value.Fechado && (item.Variacoes?.Count ?? 0) > 0)
                {
                    resultado.Avisos.Add($"Fechamento mensal \"{rotulo}\": balanço já estava fechado — variações ignoradas.");
                    continue;
                }

                foreach (var variacao in item.Variacoes ?? new())
                {
                    var alvo = balanco.Value.Itens.FirstOrDefault(i => i.DoModelo && i.Nome == variacao.Nome)
                        ?? balanco.Value.Itens.FirstOrDefault(i => i.Nome == variacao.Nome);
                    if (alvo is null)
                    {
                        resultado.Avisos.Add($"Fechamento mensal \"{rotulo}\": item \"{variacao.Nome}\" não existe no balanço do mês — variação ignorada.");
                        continue;
                    }
                    // Atualiza o próprio item (mesmo Id): mantém nome/setor/categoria
                    // e a marca de "veio do modelo", trocando só o valor.
                    var rv = await _balancoDespesaService.SalvarItemAsync(new SalvarItemBalancoDTO
                    {
                        Ano = item.Ano,
                        Mes = item.Mes,
                        ItemId = alvo.Id,
                        Nome = alvo.Nome,
                        Setor = alvo.Setor,
                        Categoria = alvo.Categoria,
                        Valor = variacao.Valor
                    });
                    if (!rv.IsSuccess)
                        resultado.Avisos.Add($"Fechamento mensal \"{rotulo}\": não foi possível ajustar \"{variacao.Nome}\" — {rv.Error}");
                }

                foreach (var nome in item.RemoverItensModelo ?? new())
                {
                    var alvo = balanco.Value.Itens.FirstOrDefault(i => i.DoModelo && i.Nome == nome);
                    if (alvo is null) continue; // já não existe — nada a remover
                    var rr = await _balancoDespesaService.RemoverItemAsync(item.Ano, item.Mes, alvo.Id);
                    if (!rr.IsSuccess)
                        resultado.Avisos.Add($"Fechamento mensal \"{rotulo}\": não foi possível remover \"{nome}\" — {rr.Error}");
                }

                if (item.Fechar && !balanco.Value.Fechado)
                {
                    var rf = await _balancoDespesaService.FecharAsync(item.Ano, item.Mes);
                    if (!rf.IsSuccess)
                    {
                        resultado.Avisos.Add($"Fechamento mensal \"{rotulo}\": não foi possível fechar — {rf.Error}");
                        continue;
                    }
                    var dataFechamento = Lim(item.DataFechamento
                        ?? new DateTime(item.Ano, item.Mes, 1).AddMonths(1).AddDays(4).AddHours(18));
                    await _backdate.AplicarAsync<BalancoMensalDespesa>(balanco.Value.Id, ("DataFechamento", dataFechamento));
                }

                resultado.FechamentosMensaisCriados++;
            }
            catch (Exception ex)
            {
                resultado.Avisos.Add($"Fechamento mensal \"{rotulo}\": erro inesperado ({ex.GetType().Name}).");
                _logger.LogError(ex, "Erro ao importar fechamento mensal {Rotulo}", rotulo);
            }
            finally { _backdate.LimparRastreamento(); }
        }
    }

    // ============================================================
    // HELPERS
    // ============================================================
    private static readonly CultureInfo PtBr = new("pt-BR");

    /// <summary>Data derivada nunca passa de "agora" (registro histórico não pode nascer no futuro).</summary>
    private static DateTime Lim(DateTime d) => d > DateTime.Now ? DateTime.Now : d;
    /// <summary>
    /// Como <see cref="Lim"/>, mas uma data derivada que cairia no futuro vira
    /// um ponto entre a etapa anterior e agora (em vez de "agora" cravado).
    /// </summary>
    private static DateTime LimEntre(DateTime anterior, DateTime d)
        => d <= DateTime.Now ? d : Entre(Min(anterior, DateTime.Now), DateTime.Now, 0.5);
    private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;
    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
    /// <summary>Ponto proporcional entre duas datas (fração 0..1); se fim ≤ início, devolve o início.</summary>
    private static DateTime Entre(DateTime inicio, DateTime fim, double fracao)
        => fim <= inicio ? inicio : inicio.AddTicks((long)((fim - inicio).Ticks * fracao));

    private static string FormatarCpf(string cpf)
    {
        var d = new string((cpf ?? "").Where(char.IsDigit).ToArray());
        return d.Length == 11 ? $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}" : cpf ?? "";
    }

    /// <summary>Troca os campos "[CAMPO]" do template pelos valores conhecidos (os sem valor ficam como estão).</summary>
    private static string Preencher(string texto, IDictionary<string, string?> valores)
    {
        foreach (var (campo, valor) in valores)
            if (!string.IsNullOrWhiteSpace(valor))
                texto = texto.Replace($"[{campo}]", valor);
        return texto;
    }

    /// <summary>Aceita "Debito"/"Débito"/"Credito"/"Crédito"/"Transferência" além dos nomes do enum ModoPagamento.</summary>
    private static string? NormalizarModo(string? modo)
    {
        if (string.IsNullOrWhiteSpace(modo)) return null;
        var m = modo.Trim();
        return m.ToLowerInvariant() switch
        {
            "debito" or "débito" or "cartaodebito" or "cartão de débito" => "CartaoDebito",
            "credito" or "crédito" or "cartaocredito" or "cartão de crédito" => "CartaoCredito",
            "transferência" or "transferencia" => "Transferencia",
            "pix" => "Pix",
            "boleto" => "Boleto",
            "dinheiro" => "Dinheiro",
            "financiamento" => "Financiamento",
            _ => m
        };
    }

    /// <summary>
    /// Valor de cada pagamento = total × percentual/100 (2 casas). Se
    /// <paramref name="quitar"/>, o ÚLTIMO pagamento recebe o saldo restante
    /// exato (fecha centavos de arredondamento e percentuais que não somam 100).
    /// </summary>
    private static List<(string Modo, decimal Valor, DateTime? Data)> CalcularPagamentos(
        decimal total, List<PagamentoImportDTO> lista, string modoPadrao, bool quitar)
    {
        var resultado = new List<(string, decimal, DateTime?)>();
        decimal acumulado = 0m;
        for (var i = 0; i < lista.Count; i++)
        {
            var p = lista[i];
            var valor = quitar && i == lista.Count - 1
                ? total - acumulado
                : Math.Round(total * p.Percentual / 100m, 2, MidpointRounding.AwayFromZero);
            if (valor <= 0m) continue;
            acumulado += valor;
            resultado.Add((NormalizarModo(p.Modo) ?? modoPadrao, valor, p.Data));
        }
        return resultado;
    }

    private async Task<(string Nome, string Cpf)> ObterClienteAsync(Guid clienteId)
    {
        if (_clientes.TryGetValue(clienteId, out var c)) return c;
        var r = await _clienteService.GetByIdAsync(clienteId);
        var info = r.IsSuccess && r.Value is not null ? (r.Value.Nome, r.Value.Cpf) : ("Cliente", "00000000000");
        _clientes[clienteId] = info;
        return info;
    }

    /// <summary>Ids dos itens de despesa (não-modelo) já lançados na competência atual.</summary>
    private async Task<HashSet<Guid>> IdsItensDespesaMesAtualAsync()
    {
        var hoje = DateTime.Today;
        var r = await _balancoDespesaService.ObterAsync(hoje.Year, hoje.Month);
        return r.IsSuccess && r.Value is not null ? r.Value.Itens.Select(i => i.Id).ToHashSet() : new HashSet<Guid>();
    }

    /// <summary>
    /// Despesa automática lançada "hoje" por uma regra de negócio (R$ 50 de
    /// combustível do test drive, compra de peça encomendada na entrada do
    /// estoque) durante a importação — move os itens NOVOS (não estavam em
    /// <paramref name="antes"/>, e não são linhas do modelo) pra competência
    /// histórica do evento. Mesmo padrão de MoverDespesaParaCompetenciaHistoricaAsync.
    /// </summary>
    private async Task MoverDespesasNovasParaCompetenciaAsync(HashSet<Guid> antes, DateTime dataHistorica)
    {
        var hoje = DateTime.Today;
        if (dataHistorica.Year == hoje.Year && dataHistorica.Month == hoje.Month) return;

        var rHoje = await _balancoDespesaService.ObterAsync(hoje.Year, hoje.Month);
        if (!rHoje.IsSuccess || rHoje.Value is null) return;

        var novos = rHoje.Value.Itens.Where(i => !i.DoModelo && !antes.Contains(i.Id)).ToList();
        if (novos.Count == 0) return;

        await _balancoDespesaService.GerarDoModeloAsync(dataHistorica.Year, dataHistorica.Month);
        foreach (var item in novos)
        {
            await _balancoDespesaService.RemoverItemAsync(hoje.Year, hoje.Month, item.Id);
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
    }

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
