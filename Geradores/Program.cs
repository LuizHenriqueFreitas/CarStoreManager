using CarStoreManager.Geradores.Entidades;
using CarStoreManager.Geradores.Nucleo;

// =====================================================================
// Geradores — popula o banco SQLite local (carstore.db) com dados fake
// passando pelas MESMAS regras de negócio da aplicação real (Application
// Services), nunca via INSERT direto. Pode ser rodado várias vezes seguidas
// para ir aumentando a base — ver Geradores/README.md para detalhes de uso
// e reaproveitamento.
// =====================================================================

var opcoes = OpcoesCli.Analisar(args);

if (opcoes.MostrarAjuda)
{
    OpcoesCli.ImprimirAjuda();
    return;
}

Console.WriteLine("Banco alvo: o mesmo configurado para o Web (appsettings.json — carstore.db).");
Console.WriteLine($"Seed aleatória: {opcoes.Seed}");
Console.WriteLine();

var rng = new Random(opcoes.Seed);
var provider = ComposicaoServicos.Construir();

// Período histórico simulado (~2 anos) — veículos, propostas, consignações e
// OS são espalhados por essa janela em vez de nascerem todos "agora", pra
// dar volume e variação real aos gráficos mensais da dashboard.
var periodoFim = DateTime.UtcNow;
var periodoInicio = periodoFim.AddYears(-2);

Console.WriteLine("Carregando valores já existentes no banco (evita colisão de e-mail/CPF/placa/SKU)...");
var pools = await PoolsIniciais.CarregarAsync(provider);
Console.WriteLine("Pronto.");
Console.WriteLine();

var totalCriado = 0;

Console.WriteLine("== Usuários ==");
var usuarios = await UsuarioGerador.GerarAsync(provider, rng, pools.Email, pools.Telefone, opcoes.Usuarios);
totalCriado += usuarios.Values.Sum(l => l.Count);
var vendedorIds = usuarios.GetValueOrDefault("Vendedor", new List<Guid>());
var mecanicoIds = usuarios.GetValueOrDefault("Mecanico", new List<Guid>());
var adminIds = usuarios.GetValueOrDefault("Admin", new List<Guid>());

Console.WriteLine();
Console.WriteLine("== Clientes ==");
var clienteIds = await ClienteGerador.GerarAsync(provider, opcoes.Clientes, rng, pools.Cpf, pools.Email, pools.Telefone);
totalCriado += clienteIds.Count;

Console.WriteLine();
Console.WriteLine("== Componentes (estoque da oficina) ==");
var componenteIds = await ComponenteGerador.GerarAsync(provider, opcoes.Componentes, rng, pools.Sku);
totalCriado += componenteIds.Count;

Console.WriteLine();
Console.WriteLine("== Despesas ==");
var despesaIds = await DespesaGerador.GerarAsync(provider, opcoes.Despesas, rng);
totalCriado += despesaIds.Count;

Console.WriteLine();
Console.WriteLine("== Checklist presets ==");
var presetIds = await ChecklistPresetGerador.GerarAsync(provider, opcoes.ChecklistPresets, rng);
totalCriado += presetIds.Count;

Console.WriteLine();
Console.WriteLine("== Templates de documento ==");
var templateDocIds = await TemplateDocumentoGerador.GerarAsync(provider);
totalCriado += templateDocIds.Count;

Console.WriteLine();
Console.WriteLine("== Veículos (concessionária) ==");
var veiculoVendaIds = await VeiculoVendaGerador.GerarAsync(
    provider, opcoes.VeiculosVenda, rng, pools.Placa, pools.Renavam, periodoInicio, periodoFim);
totalCriado += veiculoVendaIds.Count;

Console.WriteLine();
Console.WriteLine("== Veículos (cliente / oficina) ==");
var veiculosCliente = await VeiculoClienteGerador.GerarAsync(provider, opcoes.VeiculosCliente, rng, pools.Placa, clienteIds);
totalCriado += veiculosCliente.Count;

Console.WriteLine();
Console.WriteLine("== Veículos consignados ==");
var consignacaoIds = await VeiculoConsignacaoGerador.GerarAsync(
    provider, opcoes.VeiculosConsignacao, rng, pools.Placa, pools.Renavam, clienteIds, vendedorIds,
    periodoInicio, periodoFim);
totalCriado += consignacaoIds.Count;

Console.WriteLine();
Console.WriteLine("== Propostas de venda ==");
var disponiveis = await VeiculoVendaGerador.ObterDisponiveisAsync(provider);
var propostaIds = await PropostaVendaGerador.GerarAsync(provider, opcoes.PropostasVenda, rng, disponiveis, vendedorIds, clienteIds);
totalCriado += propostaIds.Count;

if (propostaIds.Count > 0 && vendedorIds.Count > 0)
{
    var adminOuVendedorId = adminIds.Count > 0 ? adminIds[0] : vendedorIds[0];
    await PropostaVendaGerador.AvancarStatusEDatasAsync(
        provider, propostaIds, rng, periodoInicio, periodoFim, adminOuVendedorId, vendedorIds);
}

Console.WriteLine();
Console.WriteLine("== Ordens de serviço ==");
var ordemIds = await OrdemServicoGerador.GerarAsync(
    provider, opcoes.OrdensServico, rng, veiculosCliente, mecanicoIds, periodoInicio, periodoFim);
totalCriado += ordemIds.Count;

Console.WriteLine();
Console.WriteLine("=====================================================");
Console.WriteLine($"Total de registros criados nesta execução: {totalCriado}");
Console.WriteLine("=====================================================");
Console.WriteLine();
Console.WriteLine($"Todos os usuários gerados têm a senha: \"{UsuarioGerador.SenhaPadrao}\"");
