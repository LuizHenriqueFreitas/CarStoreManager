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
                        "e leva para todas as partes do sistema. O conteúdo se adapta ao seu perfil."
                },
                new()
                {
                    Seletor = "inicio-hub-atalhos",
                    Titulo = "Atalhos rápidos",
                    Texto = "Mesma barra lateral dos painéis de Oficina e Concessionária — acesso direto às " +
                        "áreas que você tem permissão de ver."
                },
                new()
                {
                    Seletor = "inicio-alertas",
                    Titulo = "Alertas",
                    Texto = "Só aparecem quando há algo pendente — ordens aguardando cobrança, consignações " +
                        "vencendo, peças abaixo do mínimo. Cada alerta é um atalho para resolver."
                },
                new()
                {
                    Seletor = "inicio-periodo",
                    Titulo = "Período",
                    Texto = "Escolhe o intervalo (7, 15, 30, 60 dias ou datas customizadas) usado pelos " +
                        "números do negócio logo abaixo — mesmo seletor do Financeiro e de Análises."
                },
                new()
                {
                    Seletor = "inicio-kpis",
                    Titulo = "Números do negócio",
                    Texto = "Receita, despesa, lucro e capital em estoque no período escolhido — visível para " +
                        "administração e gestão. Clique para abrir o Financeiro."
                },
                new()
                {
                    Seletor = "inicio-faturamento",
                    Titulo = "Receitas e despesas",
                    Texto = "Três linhas separadas — receita da oficina, receita da concessionária e despesa " +
                        "— mês a mês, dentro do período escolhido no seletor acima. Nunca somadas numa linha só, " +
                        "pra não misturar dinheiro de setores diferentes. Aparece para administração e gestão."
                },
                new()
                {
                    Seletor = "inicio-areas",
                    Titulo = "Cartões de área",
                    Texto = "Um mini-painel por área com os números do dia e um botão para abrir. Você vê só " +
                        "as áreas a que tem acesso."
                },
                new()
                {
                    Seletor = "inicio-hub-atalhos",
                    Titulo = "Atalhos",
                    Texto = "Acesso rápido a clientes, relatórios, análises, despesas do mês e configurações — " +
                        "sem passar pelo menu."
                }
            }
        }
    };
}
