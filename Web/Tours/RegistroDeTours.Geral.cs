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
                    Seletor = "inicio-alertas",
                    Titulo = "Alertas",
                    Texto = "Só aparecem quando há algo pendente — ordens aguardando cobrança, consignações " +
                        "vencendo, peças abaixo do mínimo. Cada alerta é um atalho para resolver."
                },
                new()
                {
                    Seletor = "inicio-kpis",
                    Titulo = "Números do negócio",
                    Texto = "Receita, despesa, lucro e capital em estoque do mês — visível para administração " +
                        "e gestão. Clique para abrir o Financeiro."
                },
                new()
                {
                    Seletor = "inicio-faturamento",
                    Titulo = "Faturamento dos últimos meses",
                    Texto = "Linha com a receita total (serviços + vendas) mês a mês dos últimos 6 meses — " +
                        "uma leitura rápida da tendência. Aparece para administração e gestão."
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
                    Seletor = "inicio-atalhos",
                    Titulo = "Atalhos",
                    Texto = "Acesso rápido a clientes, relatórios, análises, despesas do mês e configurações — " +
                        "sem passar pelo menu."
                }
            }
        }
    };
}
