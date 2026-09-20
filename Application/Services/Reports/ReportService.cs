using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Admin;
using CarStoreManager.Application.DTOs.Reports;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services.Reports;

/// <summary>
/// Catálogo único de relatórios do sistema — ver docs/redesign/15-relatorios.md.
/// Cada Id do catálogo (<see cref="ObterCatalogo"/>) tem um builder próprio
/// aqui dentro que monta um <see cref="ReportData"/> já enriquecido (nomes
/// resolvidos, enums traduzidos via <see cref="RotulosRelatorio"/>) e
/// <see cref="ExportAsync"/> formata (CSV/XML) via Strategy.
///
/// Acesso a dado é direto via repositório (mesmo padrão do
/// DashboardService) em vez de passar pelos serviços de Application —
/// evita N+1 pra resolver nome de cliente/veículo/mecânico em cada linha.
/// </summary>
public class ReportService : IReportService
{
    private readonly IDashboardService _dashboard;
    private readonly IClienteRepository _clientes;
    private readonly IMecanicoRepository _mecanicos;
    private readonly IVendedorRepository _vendedores;
    private readonly IVeiculoVendaRepository _veiculosVenda;
    private readonly IVeiculoConsignacaoRepository _consignacoes;
    private readonly IVeiculoClienteRepository _veiculosCliente;
    private readonly IComponenteRepository _componentes;
    private readonly IEstoqueRepository _estoque;
    private readonly IFornecedorRepository _fornecedores;
    private readonly IOrdemServicoRepository _ordens;
    private readonly IPropostaVendaRepository _propostas;
    private readonly ITestDriveRepository _testDrives;
    private readonly IBalancoMensalDespesaService _balancos;
    private readonly CsvReportFormatter _csvFormatter;
    private readonly XmlReportFormatter _xmlFormatter;

    public ReportService(
        IDashboardService dashboard,
        IClienteRepository clientes,
        IMecanicoRepository mecanicos,
        IVendedorRepository vendedores,
        IVeiculoVendaRepository veiculosVenda,
        IVeiculoConsignacaoRepository consignacoes,
        IVeiculoClienteRepository veiculosCliente,
        IComponenteRepository componentes,
        IEstoqueRepository estoque,
        IFornecedorRepository fornecedores,
        IOrdemServicoRepository ordens,
        IPropostaVendaRepository propostas,
        ITestDriveRepository testDrives,
        IBalancoMensalDespesaService balancos,
        CsvReportFormatter csvFormatter,
        XmlReportFormatter xmlFormatter)
    {
        _dashboard = dashboard;
        _clientes = clientes;
        _mecanicos = mecanicos;
        _vendedores = vendedores;
        _veiculosVenda = veiculosVenda;
        _consignacoes = consignacoes;
        _veiculosCliente = veiculosCliente;
        _componentes = componentes;
        _estoque = estoque;
        _fornecedores = fornecedores;
        _ordens = ordens;
        _propostas = propostas;
        _testDrives = testDrives;
        _balancos = balancos;
        _csvFormatter = csvFormatter;
        _xmlFormatter = xmlFormatter;
    }

    // =========================================================
    // CATÁLOGO
    // =========================================================

    public IReadOnlyList<RelatorioDefinicao> ObterCatalogo(bool podeOficina, bool podeConcessionaria)
    {
        var lista = new List<RelatorioDefinicao>();

        // --- Operacional ---
        if (podeOficina)
            lista.Add(new() { Id = "equipe-oficina", Titulo = "Equipe da oficina", Grupo = GrupoRelatorio.Operacional,
                Descricao = "Mecânicos: nome, especialidade, nível, data de admissão." });
        if (podeConcessionaria)
            lista.Add(new() { Id = "equipe-vendas", Titulo = "Equipe de vendas", Grupo = GrupoRelatorio.Operacional,
                Descricao = "Vendedores: nome, telefone, nível, data de admissão." });
        if (podeOficina || podeConcessionaria)
            lista.Add(new() { Id = "clientes", Titulo = "Clientes cadastrados", Grupo = GrupoRelatorio.Operacional,
                Descricao = "Nome, CPF, telefone, e-mail, data de cadastro." });
        if (podeConcessionaria)
        {
            lista.Add(new() { Id = "estoque-veiculos-loja", Titulo = "Estoque de veículos — loja própria", Grupo = GrupoRelatorio.Operacional,
                Descricao = "Veículos próprios: marca, modelo, ano, combustível, placa, disponibilidade, valor." });
            lista.Add(new() { Id = "veiculos-consignados", Titulo = "Veículos em consignação", Grupo = GrupoRelatorio.Operacional,
                Descricao = "Veículo, proprietário, vendedor responsável, status, valor esperado, vencimento do contrato." });
            lista.Add(new() { Id = "propostas-venda", Titulo = "Propostas de venda", Grupo = GrupoRelatorio.Operacional,
                Descricao = "Cliente, veículo, vendedor, valor, forma de pagamento, status." });
            lista.Add(new() { Id = "test-drives", Titulo = "Test drives", Grupo = GrupoRelatorio.Operacional,
                Descricao = "Cliente, veículo, vendedor, data/hora, status." });
        }
        if (podeOficina)
        {
            lista.Add(new() { Id = "estoque-pecas", Titulo = "Estoque de peças", Grupo = GrupoRelatorio.Operacional,
                Descricao = "Componentes: SKU, nome, categoria, sistema, fornecedor, quantidade em estoque." });
            lista.Add(new() { Id = "fornecedores", Titulo = "Fornecedores cadastrados", Grupo = GrupoRelatorio.Operacional,
                Descricao = "Nome, CNPJ, contato, situação." });
            lista.Add(new() { Id = "ordens-servico", Titulo = "Ordens de serviço", Grupo = GrupoRelatorio.Operacional,
                Descricao = "Número, tipo, status, cliente, veículo, mecânico, prazo, valor." });
        }

        // --- Financeiro ---
        if (podeOficina && podeConcessionaria)
            lista.Add(new() { Id = "financeiro-geral", Titulo = "Financeiro consolidado — operação completa", Grupo = GrupoRelatorio.Financeiro,
                Descricao = "Receitas, despesas e resultado de toda a operação no período." });
        if (podeOficina)
            lista.Add(new() { Id = "financeiro-oficina", Titulo = "Financeiro consolidado — oficina", Grupo = GrupoRelatorio.Financeiro,
                Descricao = "Receita de serviços, despesas, lucro e OS por status no período." });
        if (podeConcessionaria)
            lista.Add(new() { Id = "financeiro-concessionaria", Titulo = "Financeiro consolidado — concessionária", Grupo = GrupoRelatorio.Financeiro,
                Descricao = "Receita de vendas, despesas, lucro e veículos por status no período." });
        if (podeOficina || podeConcessionaria)
            lista.Add(new() { Id = "despesas-detalhadas", Titulo = "Despesas detalhadas por categoria", Grupo = GrupoRelatorio.Financeiro,
                Descricao = "Uma linha por item de despesa lançado: mês de competência, setor, categoria, nome, valor." });
        if (podeOficina)
            lista.Add(new() { Id = "compras-material", Titulo = "Compras de material (peças)", Grupo = GrupoRelatorio.Financeiro,
                Descricao = "Peça, fornecedor, custo unitário, quantidade em estoque, valor total investido." });
        if (podeConcessionaria)
            lista.Add(new() { Id = "vendas-margem", Titulo = "Vendas de veículos com margem", Grupo = GrupoRelatorio.Financeiro,
                Descricao = "Venda concluída: valor de venda, valor de aquisição do veículo, margem, vendedor." });

        // --- Comparativo (mesmo catálogo de gráficos da Análises, exportado em tabela) ---
        if (podeOficina || podeConcessionaria)
            lista.Add(new() { Id = "comparativo-geral", Titulo = "Comparativos — geral", Grupo = GrupoRelatorio.Comparativo,
                Descricao = "Composição financeira, valores médios (ticket) e funcionários por tipo." });
        if (podeConcessionaria)
            lista.Add(new() { Id = "comparativo-concessionaria", Titulo = "Comparativos — concessionária", Grupo = GrupoRelatorio.Comparativo,
                Descricao = "Marcas/modelos/cores mais vendidos, combustível, câmbio, pagamento, status de veículos, top clientes." });
        if (podeOficina)
        {
            lista.Add(new() { Id = "comparativo-oficina", Titulo = "Comparativos — oficina", Grupo = GrupoRelatorio.Comparativo,
                Descricao = "Mecânicos por especialização, tipos de serviço, OS por status, prazo médio, top clientes." });
            lista.Add(new() { Id = "comparativo-estoque", Titulo = "Comparativos — estoque de peças", Grupo = GrupoRelatorio.Comparativo,
                Descricao = "Componentes em estoque por sistema do veículo." });
        }

        return lista;
    }

    // =========================================================
    // EXPORTAÇÃO
    // =========================================================

    public async Task<Result<byte[]>> ExportAsync(
        string relatorioId, string formato, DateTime dataInicio, DateTime dataFim,
        bool podeOficina, bool podeConcessionaria)
    {
        formato = formato.ToLowerInvariant();
        if (formato is not ("csv" or "xml"))
            return Result<byte[]>.Fail($"Formato '{formato}' não suportado. Use 'csv' ou 'xml'.");
        if (dataFim.Date < dataInicio.Date)
            return Result<byte[]>.Fail("Data final não pode ser anterior à data inicial.");

        var catalogo = ObterCatalogo(podeOficina, podeConcessionaria);
        if (!catalogo.Any(r => r.Id == relatorioId))
            return Result<byte[]>.Fail("Relatório inválido ou você não tem permissão para acessá-lo.");

        ReportData data;
        try
        {
            data = relatorioId switch
            {
                "equipe-oficina" => await ConstruirEquipeOficinaAsync(dataInicio, dataFim),
                "equipe-vendas" => await ConstruirEquipeVendasAsync(dataInicio, dataFim),
                "clientes" => await ConstruirClientesAsync(dataInicio, dataFim),
                "estoque-veiculos-loja" => await ConstruirEstoqueVeiculosLojaAsync(dataInicio, dataFim),
                "veiculos-consignados" => await ConstruirVeiculosConsignadosAsync(dataInicio, dataFim),
                "propostas-venda" => await ConstruirPropostasVendaAsync(dataInicio, dataFim),
                "test-drives" => await ConstruirTestDrivesAsync(dataInicio, dataFim),
                "estoque-pecas" => await ConstruirEstoquePecasAsync(dataInicio, dataFim),
                "fornecedores" => await ConstruirFornecedoresAsync(dataInicio, dataFim),
                "ordens-servico" => await ConstruirOrdensServicoAsync(dataInicio, dataFim),
                "financeiro-geral" => await ConstruirFinanceiroGeralAsync(dataInicio, dataFim),
                "financeiro-oficina" => await ConstruirFinanceiroOficinaAsync(dataInicio, dataFim),
                "financeiro-concessionaria" => await ConstruirFinanceiroConcessionariaAsync(dataInicio, dataFim),
                "despesas-detalhadas" => await ConstruirDespesasDetalhadasAsync(dataInicio, dataFim),
                "compras-material" => await ConstruirComprasMaterialAsync(dataInicio, dataFim),
                "vendas-margem" => await ConstruirVendasMargemAsync(dataInicio, dataFim),
                "comparativo-geral" => await ConstruirComparativoAsync("Comparativos — geral", new[] { "Financeiro", "Pessoas" }, dataInicio, dataFim),
                "comparativo-concessionaria" => await ConstruirComparativoAsync("Comparativos — concessionária", new[] { "Concessionária" }, dataInicio, dataFim),
                "comparativo-oficina" => await ConstruirComparativoAsync("Comparativos — oficina", new[] { "Oficina" }, dataInicio, dataFim),
                "comparativo-estoque" => await ConstruirComparativoAsync("Comparativos — estoque de peças", new[] { "Estoque" }, dataInicio, dataFim),
                _ => throw new InvalidOperationException($"Relatório sem builder registrado: {relatorioId}")
            };
        }
        catch (Exception)
        {
            return Result<byte[]>.Fail("Não foi possível gerar o relatório. Tente novamente em instantes.");
        }

        data.Periodo = $"{dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}";
        var bytes = formato == "xml" ? await _xmlFormatter.FormatAsync(data) : await _csvFormatter.FormatAsync(data);
        return Result<byte[]>.Ok(bytes);
    }

    // =========================================================
    // OPERACIONAL
    // =========================================================

    private async Task<ReportData> ConstruirEquipeOficinaAsync(DateTime inicio, DateTime fim)
    {
        var itens = FiltrarPorPeriodo((await _mecanicos.GetAllAsync()).ToList(), m => m.DataCriacao, inicio, fim);
        return SecaoUnica("Equipe da oficina", "Mecânicos", itens, m => new Dictionary<string, object?>
        {
            ["Nome"] = m.Nome,
            ["Especialidade"] = m.GetEspecialidade(),
            ["Nivel"] = m.DadosFuncionario.GetNivel().ToString(),
            ["DataAdmissao"] = m.DadosFuncionario.GetDataContratacao()
        });
    }

    private async Task<ReportData> ConstruirEquipeVendasAsync(DateTime inicio, DateTime fim)
    {
        var itens = FiltrarPorPeriodo((await _vendedores.GetAllAsync()).ToList(), v => v.DataCriacao, inicio, fim);
        return SecaoUnica("Equipe de vendas", "Vendedores", itens, v => new Dictionary<string, object?>
        {
            ["Nome"] = v.Nome,
            ["Telefone"] = v.GetTelefone(),
            ["Nivel"] = v.DadosFuncionario.GetNivel().ToString(),
            ["DataAdmissao"] = v.DadosFuncionario.GetDataContratacao()
        });
    }

    private async Task<ReportData> ConstruirClientesAsync(DateTime inicio, DateTime fim)
    {
        var itens = FiltrarPorPeriodo((await _clientes.GetAllAsync()).ToList(), c => c.DataCriacao, inicio, fim);
        return SecaoUnica("Clientes cadastrados", "Clientes", itens, c => new Dictionary<string, object?>
        {
            ["Nome"] = c.GetNome(),
            ["CPF"] = c.GetCpf(),
            ["Telefone"] = c.GetTelefone(),
            ["Email"] = c.GetEmail(),
            ["DataCadastro"] = c.DataCriacao
        });
    }

    private async Task<ReportData> ConstruirEstoqueVeiculosLojaAsync(DateTime inicio, DateTime fim)
    {
        var itens = FiltrarPorPeriodo((await _veiculosVenda.GetAllAsync()).ToList(), v => v.DataCriacao, inicio, fim);
        return SecaoUnica("Estoque de veículos — loja própria", "Veículos próprios", itens, v => new Dictionary<string, object?>
        {
            ["Marca"] = v.GetMarca(),
            ["Modelo"] = v.GetModelo(),
            ["Ano"] = v.GetAno(),
            ["Placa"] = v.GetPlacaCarro(),
            ["Combustivel"] = v.GetCombustivel(),
            ["Disponibilidade"] = RotulosRelatorio.Disponibilidade(v.Disponibilidade.ToString()),
            ["Valor"] = v.GetValor(),
            ["ValorAquisicao"] = v.GetValorAquisicao(),
            ["DataCadastro"] = v.DataCriacao
        });
    }

    private async Task<ReportData> ConstruirVeiculosConsignadosAsync(DateTime inicio, DateTime fim)
    {
        var todos = (await _consignacoes.GetAllAsync()).ToList();
        var clientesPorId = await ObterClientesPorIdAsync();
        var vendedoresPorId = await ObterVendedoresPorIdAsync();
        var itens = FiltrarPorPeriodo(todos, c => c.DataCriacao, inicio, fim);

        return SecaoUnica("Veículos em consignação", "Consignações", itens, c => new Dictionary<string, object?>
        {
            ["Marca"] = c.GetMarca(),
            ["Modelo"] = c.GetModelo(),
            ["Ano"] = c.GetAno(),
            ["Proprietario"] = NomeOuRemovido(clientesPorId, c.ClienteProprietarioId, "Cliente removido"),
            ["VendedorResponsavel"] = NomeOuRemovido(vendedoresPorId, c.VendedorResponsavelId, "Vendedor removido"),
            ["Status"] = RotulosRelatorio.StatusConsignacao(c.Status.ToString()),
            ["ValorVendaEsperado"] = c.Comissao.ValorVendaEsperado.GetValorDinheiro(),
            ["DataInicio"] = c.DataInicio,
            ["DataVencimento"] = c.DataVencimento
        });
    }

    private async Task<ReportData> ConstruirPropostasVendaAsync(DateTime inicio, DateTime fim)
    {
        var todas = (await _propostas.GetAllAsync()).ToList();
        var clientesPorId = await ObterClientesPorIdAsync();
        var vendedoresPorId = await ObterVendedoresPorIdAsync();
        var veiculosPorId = (await _veiculosVenda.GetAllAsync()).ToDictionary(v => v.Id);
        var itens = FiltrarPorPeriodo(todas, p => p.DataCriacao, inicio, fim);

        return SecaoUnica("Propostas de venda", "Propostas", itens, p => new Dictionary<string, object?>
        {
            ["Cliente"] = NomeOuRemovido(clientesPorId, p.GetClienteId(), "Cliente removido"),
            ["Veiculo"] = DescricaoVeiculo(veiculosPorId, p.GetVeiculoId()),
            ["Vendedor"] = NomeOuRemovido(vendedoresPorId, p.GetVendedorId(), "Vendedor removido"),
            ["ValorFinal"] = p.GetValorFinal(),
            ["FormaPagamento"] = RotulosRelatorio.ModoPagamento(p.ModoPagamento.ToString()),
            ["Status"] = RotulosRelatorio.StatusProposta(p.GetStatus()),
            ["DataCriacao"] = p.GetDataCriacao()
        });
    }

    private async Task<ReportData> ConstruirTestDrivesAsync(DateTime inicio, DateTime fim)
    {
        var todos = (await _testDrives.GetAllAsync()).ToList();
        var clientesPorId = await ObterClientesPorIdAsync();
        var vendedoresPorId = await ObterVendedoresPorIdAsync();
        var veiculosPorId = (await _veiculosVenda.GetAllAsync()).ToDictionary(v => v.Id);
        // Filtra pela data agendada do test drive, não pela data de criação do
        // registro — é o que faz sentido pra "test drives no período".
        var itens = FiltrarPorPeriodo(todos, t => t.DataHora, inicio, fim);

        return SecaoUnica("Test drives", "Test drives", itens, t => new Dictionary<string, object?>
        {
            ["Cliente"] = NomeOuRemovido(clientesPorId, t.ClienteId, "Cliente removido"),
            ["Veiculo"] = DescricaoVeiculo(veiculosPorId, t.VeiculoVendaId),
            ["Vendedor"] = NomeOuRemovido(vendedoresPorId, t.VendedorId, "Vendedor removido"),
            ["DataHora"] = t.DataHora,
            ["Status"] = RotulosRelatorio.StatusTestDrive(t.Status.ToString())
        });
    }

    private async Task<ReportData> ConstruirEstoquePecasAsync(DateTime inicio, DateTime fim)
    {
        var componentes = FiltrarPorPeriodo((await _componentes.GetAllAsync()).ToList(), c => c.DataCriacao, inicio, fim);
        var fornecedoresPorId = (await _fornecedores.GetAllAsync()).ToDictionary(f => f.Id, f => f.Nome);
        var estoquePorComponente = (await _estoque.GetAllAsync()).ToDictionary(e => e.PecaId, e => e.QuantidadeAtual);

        return SecaoUnica("Estoque de peças", "Componentes", componentes, c => new Dictionary<string, object?>
        {
            ["SKU"] = c.SKUInterno,
            ["Nome"] = c.Nome,
            ["Categoria"] = c.Categoria,
            ["Sistema"] = RotulosRelatorio.Sistema(c.Sistema?.ToString()),
            ["Fornecedor"] = fornecedoresPorId.TryGetValue(c.FornecedorId, out var nomeForn) ? nomeForn : "Fornecedor removido",
            ["QuantidadeEmEstoque"] = estoquePorComponente.TryGetValue(c.Id, out var qtd) ? qtd : 0
        });
    }

    private async Task<ReportData> ConstruirFornecedoresAsync(DateTime inicio, DateTime fim)
    {
        var itens = FiltrarPorPeriodo((await _fornecedores.GetAllAsync()).ToList(), f => f.DataCriacao, inicio, fim);
        return SecaoUnica("Fornecedores cadastrados", "Fornecedores", itens, f => new Dictionary<string, object?>
        {
            ["Nome"] = f.Nome,
            ["CNPJ"] = f.Cnpj.ToString(),
            ["Email"] = f.Email ?? "",
            ["Telefone"] = f.Telefone ?? "",
            ["Ativo"] = f.Ativo ? "Sim" : "Não",
            ["DataCadastro"] = f.DataCriacao
        });
    }

    private async Task<ReportData> ConstruirOrdensServicoAsync(DateTime inicio, DateTime fim)
    {
        var todas = (await _ordens.GetAllAsync()).ToList();
        var clientesPorId = await ObterClientesPorIdAsync();
        var mecanicosPorId = (await _mecanicos.GetAllAsync()).ToDictionary(m => m.Id, m => m.Nome);
        var veiculosClientePorId = (await _veiculosCliente.GetAllAsync())
            .ToDictionary(v => v.Id, v => $"{v.Marca} {v.Modelo} — {v.Placa}");
        var itens = FiltrarPorPeriodo(todas, o => o.DataCriacao, inicio, fim);

        return SecaoUnica("Ordens de serviço", "Ordens de serviço", itens, o => new Dictionary<string, object?>
        {
            ["Numero"] = o.GetNumeroPublico(),
            ["Tipo"] = RotulosRelatorio.TipoServico(o.GetTipoServico()),
            ["Status"] = RotulosRelatorio.StatusOS(o.GetStatus()),
            ["Cliente"] = NomeOuRemovido(clientesPorId, o.ClienteId, "Cliente removido"),
            ["Veiculo"] = veiculosClientePorId.TryGetValue(o.VeiculoClienteId, out var v) ? v : "Veículo removido",
            ["Mecanico"] = mecanicosPorId.TryGetValue(o.MecanicoId, out var nomeMec) ? nomeMec : "Mecânico removido",
            ["PrazoEstimado"] = o.PrazoEstimado,
            ["ValorTotal"] = o.GetValorTotal(),
            ["DataCriacao"] = o.DataCriacao
        });
    }

    // =========================================================
    // FINANCEIRO
    // =========================================================

    private async Task<ReportData> ConstruirFinanceiroGeralAsync(DateTime inicio, DateTime fim)
    {
        var m = await ObterMetricasOuFalharAsync(inicio, fim);
        var data = NovoReportData("Financeiro consolidado — operação completa");

        data.Sections.Add(new ReportSection
        {
            Name = "KPIs financeiros do período",
            Rows = new()
            {
                LinhaMetrica("Receitas do período", m.TotalReceitasMes),
                LinhaMetrica("Despesas do período (estimativa proporcional)", m.TotalDespesasMes),
                LinhaMetrica("Lucro líquido", m.LucroLiquidoMes),
                LinhaMetrica("Capital imobilizado em veículos (estoque atual)", m.CapitalEstoqueVeiculos)
            }
        });
        data.Sections.Add(new ReportSection
        {
            Name = "Receita oficina vs. concessionária, por período",
            Rows = m.SerieReceitaServicos
                .Zip(m.SerieReceitaVendas, (of, co) => new Dictionary<string, object?>
                {
                    ["Periodo"] = of.Label,
                    ["Oficina"] = of.Valor,
                    ["Concessionaria"] = co.Valor
                }).ToList()
        });
        data.Sections.Add(SecaoContagem("Ordens de serviço por status, no período", m.OrdensServicoPorStatus, RotulosRelatorio.StatusOS));
        data.Sections.Add(SecaoContagem("Veículos por status (estoque atual)", m.VeiculosPorStatus, RotulosRelatorio.Disponibilidade));
        return data;
    }

    private async Task<ReportData> ConstruirFinanceiroOficinaAsync(DateTime inicio, DateTime fim)
    {
        var m = await ObterMetricasOuFalharAsync(inicio, fim);
        var data = NovoReportData("Financeiro consolidado — oficina");

        data.Sections.Add(new ReportSection
        {
            Name = "Financeiro da oficina no período",
            Rows = new()
            {
                LinhaMetrica("Receita de serviços", m.ReceitaServicosMesAtual),
                LinhaMetrica("Despesas da oficina (estimativa proporcional ao período)", m.TotalDespesasOficinaMensal),
                LinhaMetrica("Lucro operacional", m.LucroOficinaMes)
            }
        });
        data.Sections.Add(SecaoContagem("Ordens de serviço por status", m.OrdensServicoPorStatus, RotulosRelatorio.StatusOS));
        data.Sections.Add(new ReportSection
        {
            Name = "Receita por mecânico (top 5)",
            Rows = m.ReceitaPorMecanico
                .Select(r => new Dictionary<string, object?> { ["Mecanico"] = r.MecanicoNome, ["Receita"] = r.Receita })
                .ToList()
        });
        data.Sections.Add(SecaoSerieTemporal("Receita de serviços por período", m.SerieReceitaServicos));
        data.Sections.Add(SecaoSerieTemporal("Receita de serviços acumulada no período", m.SerieReceitaServicosAcumulada12m));
        return data;
    }

    private async Task<ReportData> ConstruirFinanceiroConcessionariaAsync(DateTime inicio, DateTime fim)
    {
        var m = await ObterMetricasOuFalharAsync(inicio, fim);
        var data = NovoReportData("Financeiro consolidado — concessionária");

        data.Sections.Add(new ReportSection
        {
            Name = "Financeiro da concessionária no período",
            Rows = new()
            {
                LinhaMetrica("Receita de vendas", m.ReceitaVendasMesAtual),
                LinhaMetrica("Despesas da concessionária (estimativa proporcional ao período)", m.TotalDespesasConcessionariaMensal),
                LinhaMetrica("Lucro operacional", m.LucroConcessionariaMes),
                LinhaMetrica("Capital imobilizado em veículos (estoque atual)", m.CapitalEstoqueVeiculos)
            }
        });
        data.Sections.Add(SecaoContagem("Veículos por status (estoque atual)", m.VeiculosPorStatus, RotulosRelatorio.Disponibilidade));
        data.Sections.Add(new ReportSection
        {
            Name = "Vendas por marca no período (top 5)",
            Rows = m.VendasPorMarca
                .Select(v => new Dictionary<string, object?> { ["Marca"] = v.Marca, ["Quantidade"] = v.Quantidade })
                .ToList()
        });
        data.Sections.Add(SecaoSerieTemporal("Receita de vendas por período", m.SerieReceitaVendas));
        data.Sections.Add(new ReportSection
        {
            Name = "Propostas aprovadas vs. rejeitadas, no período",
            Rows = m.PropostasTimeline
                .Select(t => new Dictionary<string, object?>
                {
                    ["Periodo"] = t.Label,
                    ["Aprovadas"] = t.Aprovadas,
                    ["Rejeitadas"] = t.Rejeitadas
                }).ToList()
        });
        return data;
    }

    private async Task<ReportData> ConstruirDespesasDetalhadasAsync(DateTime inicio, DateTime fim)
    {
        var linhas = new List<Dictionary<string, object?>>();
        var cursor = new DateTime(inicio.Year, inicio.Month, 1);
        var cursorFim = new DateTime(fim.Year, fim.Month, 1);

        while (cursor <= cursorFim)
        {
            var r = await _balancos.ObterAsync(cursor.Year, cursor.Month);
            if (r.IsSuccess && r.Value is not null)
            {
                var competencia = cursor.ToString("MM/yyyy");
                linhas.AddRange(r.Value.Itens.Select(i => new Dictionary<string, object?>
                {
                    ["Competencia"] = competencia,
                    ["Nome"] = i.Nome,
                    ["Setor"] = RotulosRelatorio.SetorDespesa(i.Setor),
                    ["Categoria"] = i.Categoria ?? "",
                    ["Valor"] = i.Valor
                }));
            }
            cursor = cursor.AddMonths(1);
        }

        var data = NovoReportData("Despesas detalhadas por categoria");
        data.Sections.Add(new ReportSection { Name = "Despesas por competência", Rows = linhas });
        return data;
    }

    private async Task<ReportData> ConstruirComprasMaterialAsync(DateTime inicio, DateTime fim)
    {
        var componentes = FiltrarPorPeriodo((await _componentes.GetAllAsync()).ToList(), c => c.DataCriacao, inicio, fim);
        var fornecedoresPorId = (await _fornecedores.GetAllAsync()).ToDictionary(f => f.Id, f => f.Nome);
        var estoquePorComponente = (await _estoque.GetAllAsync()).ToDictionary(e => e.PecaId, e => e.QuantidadeAtual);

        var data = NovoReportData("Compras de material (peças)");
        data.Sections.Add(new ReportSection
        {
            Name = "Peças em estoque — custo e investimento",
            Rows = componentes.Select(c =>
            {
                var qtd = estoquePorComponente.TryGetValue(c.Id, out var q) ? q : 0;
                return new Dictionary<string, object?>
                {
                    ["Peca"] = c.Nome,
                    ["Fornecedor"] = fornecedoresPorId.TryGetValue(c.FornecedorId, out var nomeForn) ? nomeForn : "Fornecedor removido",
                    ["CustoUnitario"] = c.CustoUnitario,
                    ["QuantidadeEmEstoque"] = qtd,
                    ["ValorTotalInvestido"] = c.CustoUnitario * qtd
                };
            }).ToList()
        });
        return data;
    }

    private async Task<ReportData> ConstruirVendasMargemAsync(DateTime inicio, DateTime fim)
    {
        var todas = (await _propostas.GetAllAsync()).ToList();
        var concluidas = todas.Where(p => p.Status == StatusPropostaVenda.Concluida && p.DataAprovacao.HasValue
            && p.DataAprovacao.Value.Date >= inicio.Date && p.DataAprovacao.Value.Date <= fim.Date).ToList();

        var clientesPorId = await ObterClientesPorIdAsync();
        var vendedoresPorId = await ObterVendedoresPorIdAsync();
        var veiculosPorId = (await _veiculosVenda.GetAllAsync()).ToDictionary(v => v.Id);

        var data = NovoReportData("Vendas de veículos com margem");
        data.Sections.Add(new ReportSection
        {
            Name = "Vendas concluídas no período",
            Rows = concluidas.Select(p =>
            {
                veiculosPorId.TryGetValue(p.GetVeiculoId(), out var veiculo);
                var valorAquisicao = veiculo?.GetValorAquisicao() ?? 0m;
                return new Dictionary<string, object?>
                {
                    ["Cliente"] = NomeOuRemovido(clientesPorId, p.GetClienteId(), "Cliente removido"),
                    ["Veiculo"] = veiculo is null ? "Veículo removido" : $"{veiculo.GetMarca()} {veiculo.GetModelo()}",
                    ["Vendedor"] = NomeOuRemovido(vendedoresPorId, p.GetVendedorId(), "Vendedor removido"),
                    ["ValorVenda"] = p.GetValorFinal(),
                    ["ValorAquisicao"] = valorAquisicao,
                    ["Margem"] = p.GetValorFinal() - valorAquisicao,
                    ["DataVenda"] = p.DataAprovacao
                };
            }).ToList()
        });
        return data;
    }

    // =========================================================
    // COMPARATIVO
    // =========================================================

    private async Task<ReportData> ConstruirComparativoAsync(string titulo, string[] categorias, DateTime inicio, DateTime fim)
    {
        var m = await ObterMetricasOuFalharAsync(inicio, fim);
        var data = NovoReportData(titulo);
        data.Sections = m.Graficos
            .Where(g => categorias.Contains(g.Categoria))
            .Select(g => new ReportSection
            {
                Name = g.Titulo,
                Rows = g.Dados.Select(d => new Dictionary<string, object?> { ["Categoria"] = d.Rotulo, ["Valor"] = d.Valor }).ToList()
            })
            .ToList();
        return data;
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private async Task<DashboardMetricasDTO> ObterMetricasOuFalharAsync(DateTime inicio, DateTime fim)
    {
        var r = await _dashboard.ObterMetricasPeriodoAsync(inicio, fim);
        if (!r.IsSuccess || r.Value is null)
            throw new InvalidOperationException(r.Error ?? "Não foi possível obter as métricas.");
        return r.Value;
    }

    private async Task<Dictionary<Guid, string>> ObterClientesPorIdAsync()
        => (await _clientes.GetAllAsync()).ToDictionary(c => c.Id, c => c.GetNome());

    private async Task<Dictionary<Guid, string>> ObterVendedoresPorIdAsync()
        => (await _vendedores.GetAllAsync()).ToDictionary(v => v.Id, v => v.Nome);

    private static string NomeOuRemovido(Dictionary<Guid, string> mapa, Guid id, string fallback)
        => mapa.TryGetValue(id, out var nome) ? nome : fallback;

    private static string DescricaoVeiculo(Dictionary<Guid, VeiculoVenda> mapa, Guid id)
        => mapa.TryGetValue(id, out var v) ? $"{v.GetMarca()} {v.GetModelo()} — {v.GetPlacaCarro()}" : "Veículo removido";

    private static List<T> FiltrarPorPeriodo<T>(
        IEnumerable<T> itens, Func<T, DateTime> dataSeletor, DateTime inicio, DateTime fim)
        => itens.Where(i => dataSeletor(i) >= inicio.Date && dataSeletor(i) <= fim.Date.AddDays(1).AddTicks(-1)).ToList();

    private static ReportData NovoReportData(string titulo)
        => new() { Title = titulo, GeneratedAt = DateTime.Now };

    private static ReportData SecaoUnica<T>(
        string titulo, string nomeSecao, List<T> itens, Func<T, Dictionary<string, object?>> linha)
    {
        var data = NovoReportData(titulo);
        data.Sections.Add(new ReportSection { Name = nomeSecao, Rows = itens.Select(linha).ToList() });
        return data;
    }

    private static Dictionary<string, object?> LinhaMetrica(string nome, decimal valor)
        => new() { ["Metrica"] = nome, ["Valor"] = valor };

    private static ReportSection SecaoContagem(string nome, Dictionary<string, int> contagem, Func<string, string>? traduzir = null)
        => new()
        {
            Name = nome,
            Rows = contagem
                .Select(kvp => new Dictionary<string, object?> { ["Status"] = traduzir?.Invoke(kvp.Key) ?? kvp.Key, ["Quantidade"] = kvp.Value })
                .ToList()
        };

    private static ReportSection SecaoSerieTemporal(string nome, List<MesValorDTO> serie)
        => new()
        {
            Name = nome,
            Rows = serie
                .Select(s => new Dictionary<string, object?> { ["Periodo"] = s.Label, ["Valor"] = s.Valor })
                .ToList()
        };
}
