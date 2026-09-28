namespace CarStoreManager.Web.Tours;

public static partial class RegistroDeTours
{
    private static List<TourDaPagina> TodosGeral() => new()
    {
        new TourDaPagina
        {
            RotaTemplate = "/",
            Titulo = "Início",
            Descricao = "Tela de entrada: o que precisa de atenção hoje e atalho para cada área.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Ponto de partida do dia",
                    Texto = "Esta é a primeira tela depois do login. Ela mostra o que é mais importante agora " +
                        "e leva para todas as partes do sistema. O conteúdo se adapta ao seu perfil e também " +
                        "aos módulos que a administração deixou ativos — se Oficina ou Concessionária for " +
                        "desligada para a empresa toda, a área correspondente some da tela pra todo mundo."
                },
                new()
                {
                    Seletor = "inicio-hub-atalhos",
                    Titulo = "Barra de atalhos",
                    Texto = "Mesmo painel lateral dos hubs de Oficina e Concessionária. Reúne links diretos " +
                        "pras áreas que você acessa (Concessionária e/ou Oficina, Clientes) e, pra quem " +
                        "administra ou gerencia, também Financeiro, Análises, Equipe e Configurações. O link " +
                        "\"Consulta do cliente\" abre a página pública de acompanhamento de OS — útil pra " +
                        "copiar o link e passar pro cliente."
                },
                new()
                {
                    Seletor = "inicio-alertas",
                    Titulo = "Alertas",
                    Texto = "Só aparecem quando há algo pendente — ordens aguardando cobrança, consignações " +
                        "vencendo, peças abaixo do mínimo, fechamento de despesas do mês chegando. Cada alerta " +
                        "é um atalho direto pra resolver o que está pendente."
                },
                new()
                {
                    Seletor = "inicio-periodo",
                    Titulo = "Período",
                    Texto = "Escolhe o intervalo (7, 15, 30, 60 dias ou datas customizadas) usado pelos " +
                        "números do negócio e pelo gráfico logo abaixo — mesmo seletor do Financeiro e de " +
                        "Análises. Só aparece pra quem tem acesso ao Financeiro."
                },
                new()
                {
                    Seletor = "inicio-kpis",
                    Titulo = "Números do negócio",
                    Texto = "Receita, despesa, lucro líquido e capital imobilizado em estoque no período " +
                        "escolhido acima. Clique em qualquer cartão pra abrir o Financeiro com mais detalhe."
                },
                new()
                {
                    Seletor = "inicio-faturamento",
                    Titulo = "Receitas e despesas por setor",
                    Texto = "Um gráfico por setor habilitado — se Oficina e Concessionária estiverem ativas, " +
                        "aparecem lado a lado, cada uma com sua própria receita e despesa mês a mês. Nunca " +
                        "somadas num gráfico só, pra não misturar dinheiro de setores diferentes (despesa sem " +
                        "setor específico, como administrativa, entra dividida meio a meio entre os dois)."
                },
                new()
                {
                    Seletor = "inicio-areas",
                    Titulo = "Cartões de área",
                    Texto = "Um mini-painel por área com os números do dia e um botão para abrir. Cada cartão " +
                        "só aparece se você tiver acesso àquela área e se o módulo dela estiver ativo — " +
                        "Oficina e Concessionária desligadas pela administração somem daqui, mesmo pra quem " +
                        "teria permissão de papel."
                }
            }
        }
    };
}
