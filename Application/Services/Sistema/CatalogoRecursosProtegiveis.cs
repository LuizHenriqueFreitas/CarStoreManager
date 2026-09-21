using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Domain.Enums;
using static CarStoreManager.Domain.Enums.RoleUsuario;

namespace CarStoreManager.Application.Services.Sistema;

/// <summary>
/// Catálogo de todo recurso (página ou ação) que pode ter acesso restrito
/// por papel — é CÓDIGO, não fica no banco (mesmo raciocínio de
/// <c>RegistroDeTours</c>: se um recurso for removido/renomeado no código e
/// o catálogo morasse só no banco, a tela de permissões ficaria com linhas
/// fantasma). Cada entrada já carrega o padrão ATUAL do sistema
/// (<see cref="RecursoProtegivel.RolesPermitidos"/>) — é também o TETO: o
/// admin só pode tirar acesso de alguém dentro desse conjunto, nunca dar
/// acesso a um papel que o próprio código (`[Authorize(Roles=)]`/
/// `AuthorizeView`) já bloqueia de saída.
///
/// Cobertura desta rodada (ver docs/redesign/23-permissoes-dinamicas-por-
/// papel.md): todas as 22 telas roteáveis relevantes (nível página — as
/// sub-rotas de detalhe/edição herdam a visibilidade da tela-mãe, não
/// viram entrada própria) + as ações concretas já citadas no pedido do
/// dono (cadastrar veículo/veículo consignado, cadastrar fornecedor). O
/// levantamento completo de TODO ponto de ação (~20, ver doc 23) fica pra
/// uma rodada futura — cobrir todos de uma vez arrisca inconsistência.
/// </summary>
public static class CatalogoRecursosProtegiveis
{
    private static RecursoProtegivel Pagina(string rota, string rotulo, string area, params RoleUsuario[] roles) => new()
    {
        Chave = $"pagina:{rota}",
        Tipo = TipoRecursoProtegivel.Pagina,
        Area = area,
        Rotulo = rotulo,
        RotaPagina = rota,
        RolesPermitidos = roles.ToHashSet()
    };

    private static RecursoProtegivel Acao(string chave, string rotulo, string area, params RoleUsuario[] roles) => new()
    {
        Chave = $"acao:{chave}",
        Tipo = TipoRecursoProtegivel.Acao,
        Area = area,
        Rotulo = rotulo,
        RolesPermitidos = roles.ToHashSet()
    };

    private static readonly RoleUsuario[] Todos = { Admin, Vendedor, Mecanico, Recepcionista, ChefeOficina, GerenteVendas };

    public static readonly IReadOnlyList<RecursoProtegivel> Itens = new List<RecursoProtegivel>
    {
        // ===== GERAL =====
        Pagina("/", "Início", "Geral", Todos),
        Pagina("/conta", "Minha conta", "Geral", Todos),
        Pagina("/clientes", "Clientes", "Geral", Admin, Vendedor, Recepcionista, ChefeOficina, GerenteVendas),

        // ===== ADMINISTRAÇÃO =====
        Pagina("/dashboard", "Análises", "Administração", Admin, ChefeOficina, GerenteVendas),
        Pagina("/equipe", "Equipe", "Administração", Admin),
        Pagina("/configuracoes", "Configurações do sistema", "Administração", Admin),
        Pagina("/integracoes/mercadolivre", "Integrações Mercado Livre", "Administração", Admin),

        // ===== CONCESSIONÁRIA =====
        Pagina("/concessionaria", "Concessionária — visão geral", "Concessionária", Admin, Vendedor, GerenteVendas),
        Pagina("/concessionaria/salao", "Salão", "Concessionária", Admin, Vendedor, GerenteVendas),
        Pagina("/concessionaria/propostas", "Propostas de venda", "Concessionária", Admin, Vendedor, GerenteVendas),
        Pagina("/concessionaria/consignacoes", "Consignações", "Concessionária", Admin, Vendedor, GerenteVendas),
        Pagina("/concessionaria/test-drives", "Test drives", "Concessionária", Admin, Vendedor, GerenteVendas),
        Pagina("/concessionaria/veiculo/novo", "Cadastro de veículo (tela)", "Concessionária", Admin, GerenteVendas),
        Acao("concessionaria.cadastrar-veiculo", "Botão \"Cadastrar veículo\"", "Concessionária", Admin, GerenteVendas),
        Acao("concessionaria.cadastrar-veiculo-consignado", "Botão \"Cadastrar veículo consignado\"", "Concessionária", Admin, GerenteVendas),

        // ===== FINANCEIRO =====
        Pagina("/financeiro", "Financeiro", "Financeiro", Admin, ChefeOficina, GerenteVendas),
        Pagina("/financeiro/despesas", "Despesas", "Financeiro", Admin, ChefeOficina, GerenteVendas),
        Pagina("/financeiro/a-receber", "A receber", "Financeiro", Admin, ChefeOficina, GerenteVendas),
        Pagina("/financeiro/relatorios", "Relatórios", "Financeiro", Admin, ChefeOficina, GerenteVendas),

        // ===== OFICINA =====
        Pagina("/oficina", "Oficina — visão geral", "Oficina", Admin, Mecanico, Recepcionista, ChefeOficina),
        Pagina("/oficina/recepcao", "Recepção", "Oficina", Admin, Recepcionista, ChefeOficina),
        Pagina("/oficina/ordens", "Ordens de serviço", "Oficina", Admin, Mecanico, Recepcionista, ChefeOficina),
        Pagina("/oficina/componentes", "Estoque de peças", "Oficina", Admin, Mecanico, ChefeOficina),
        Pagina("/oficina/fornecedores", "Fornecedores", "Oficina", Admin, Mecanico, ChefeOficina),
        Acao("oficina.novo-fornecedor", "Botão \"Novo fornecedor\"", "Oficina", Admin),
    };

    public static RecursoProtegivel? ObterPorChave(string chave) => Itens.FirstOrDefault(r => r.Chave == chave);
}
