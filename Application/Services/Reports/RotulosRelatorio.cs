namespace CarStoreManager.Application.Services.Reports;

/// <summary>
/// Traduz nomes de enum (PascalCase sem espaço/acento) para rótulos legíveis
/// nos relatórios exportados — ver docs/redesign/15-relatorios.md. Mesmo
/// espírito do <c>RotuloRole</c> já usado em Dashboard.razor, só que central
/// pra não duplicar tradução de enum em cada relatório novo.
/// </summary>
public static class RotulosRelatorio
{
    public static string StatusOS(string s) => s switch
    {
        "Pendente" => "Pendente",
        "EmAnalise" => "Em análise",
        "BuscandoPecasParaOrcamento" => "Buscando peças para orçamento",
        "AguardandoCliente" => "Aguardando cliente",
        "Aprovada" => "Aprovada",
        "EmAndamento" => "Em andamento",
        "Pausada" => "Pausada",
        "PagamentoPendente" => "Pagamento pendente",
        "Finalizada" => "Finalizada",
        "Entregue" => "Entregue",
        "Cancelada" => "Cancelada",
        _ => s
    };

    public static string TipoServico(string s) => s switch
    {
        "Manutencao" => "Manutenção",
        "Revisao" => "Revisão",
        "TrocaPecas" => "Troca de peças",
        "Diagnostico" => "Diagnóstico",
        "Outro" => "Outro",
        _ => s
    };

    public static string StatusProposta(string s) => s switch
    {
        "Rascunho" => "Rascunho",
        "Criada" => "Criada",
        "Enviada" => "Enviada",
        "AguardandoFinanciadora" => "Aguardando financiadora",
        "PropostaFinanciadoraRecebida" => "Proposta da financiadora recebida",
        "Aprovada" => "Aprovada",
        "AguardandoVistoria" => "Aguardando vistoria",
        "VistoriaConcluida" => "Vistoria concluída",
        "AguardandoAssinaturaTermo" => "Aguardando assinatura do termo",
        "Concluida" => "Concluída",
        "Rejeitada" => "Rejeitada",
        "Cancelada" => "Cancelada",
        "Expirada" => "Expirada",
        _ => s
    };

    public static string ModoPagamento(string s) => s switch
    {
        "NaoDefinido" => "Não definido",
        "Dinheiro" => "Dinheiro",
        "Pix" => "Pix",
        "CartaoDebito" => "Cartão de débito",
        "CartaoCredito" => "Cartão de crédito",
        "Financiamento" => "Financiamento",
        "Boleto" => "Boleto",
        "Transferencia" => "Transferência",
        _ => s
    };

    public static string Disponibilidade(string s) => s switch
    {
        "Disponivel" => "Disponível",
        "Reservado" => "Reservado",
        "Vendido" => "Vendido",
        "EmPreparacao" => "Em preparação",
        _ => s
    };

    public static string StatusConsignacao(string s) => s switch
    {
        "Ativa" => "Ativa",
        "Expirada" => "Expirada",
        "VendidoAguardandoPagamento" => "Vendido, aguardando pagamento",
        "Concluida" => "Concluída",
        "Devolvida" => "Devolvida",
        "Cancelada" => "Cancelada",
        _ => s
    };

    public static string StatusTestDrive(string s) => s switch
    {
        "Agendado" => "Agendado",
        "Realizado" => "Realizado",
        "Cancelado" => "Cancelado",
        "NaoCompareceu" => "Não compareceu",
        _ => s
    };

    public static string SetorDespesa(string s) => s switch
    {
        "Geral" => "Geral",
        "Oficina" => "Oficina",
        "Concessionaria" => "Concessionária",
        _ => s
    };

    public static string Sistema(string? s) => s switch
    {
        null or "" => "",
        "Suspensao" => "Suspensão",
        "Direcao" => "Direção",
        "Acessorios" => "Acessórios",
        _ => s
    };
}
