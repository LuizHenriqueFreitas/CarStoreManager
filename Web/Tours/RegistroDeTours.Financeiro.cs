namespace CarStoreManager.Web.Tours;

public static partial class RegistroDeTours
{
    private static List<TourDaPagina> TodosFinanceiro() => new()
    {
        new TourDaPagina
        {
            RotaTemplate = "/financeiro",
            Titulo = "Financeiro — visão geral",
            Descricao = "Resultado do período em regime de caixa: receita, despesa, lucro e capital.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "O dinheiro do negócio, num lugar só",
                    Texto = "Receita, despesa, lucro, capital investido e contas a receber. Um administrador " +
                        "vê tudo; o gerente de vendas vê o recorte da concessionária e o chefe de oficina o da oficina."
                },
                new()
                {
                    Seletor = "fin-periodo",
                    Titulo = "Período",
                    Texto = "Escolha a janela — 7, 15, 30 ou 60 dias, ou um intervalo personalizado. Vale para " +
                        "os indicadores e a comparação com o período anterior. O padrão são os últimos 15 dias."
                },
                new()
                {
                    Seletor = "fin-area",
                    Titulo = "Recorte por área",
                    Texto = "Filtra a tela inteira para Tudo, só a Oficina ou só a Concessionária."
                },
                new()
                {
                    Seletor = "fin-kpis",
                    Titulo = "Indicadores",
                    Texto = "Receita, despesa, lucro líquido e margem. A seta e o percentual comparam com o " +
                        "período anterior de mesma duração. Valores em regime de caixa — o que entrou e saiu."
                },
                new()
                {
                    Seletor = "fin-grafico",
                    Titulo = "Evolução mensal",
                    Texto = "Barras de receita e despesa com a linha de lucro por cima, mês a mês."
                },
                new()
                {
                    Seletor = "fin-areas",
                    Titulo = "Resultado por área",
                    Texto = "O lucro operacional da oficina e o da concessionária, separados. Clique em " +
                        "\"Abrir recorte\" para filtrar a tela toda naquele setor."
                },
                new()
                {
                    Seletor = "fin-capital",
                    Titulo = "Capital imobilizado",
                    Texto = "Quanto está parado em estoque de veículos e de peças. Não é despesa do mês — o " +
                        "investimento já saiu do caixa quando o item foi comprado; aqui é só o retrato do que " +
                        "ainda não foi vendido."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/financeiro/despesas",
            Titulo = "Despesas — balanço mensal",
            Descricao = "Preencha os valores reais do mês a partir do formulário-modelo.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Balanço do mês",
                    Texto = "Escolha a competência (mês/ano) no seletor do topo. Se ainda não houver balanço, " +
                        "clique em \"Gerar a partir do formulário\" — as linhas vêm do formulário mensal de " +
                        "despesas configurado pela gestão."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Ajustar e adicionar",
                    Texto = "Edite o valor real de cada linha. Linhas com barra azul à esquerda foram " +
                        "adicionadas só neste mês (não estão no modelo). Você pode adicionar quantas quiser."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Fechar o balanço",
                    Texto = "Quando terminar, clique em \"Fechar balanço\" — trava a edição. Dá para reabrir " +
                        "depois. Cada mês fica guardado para sempre, para análises futuras."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/configuracoes/formulario-despesas",
            Titulo = "Formulário mensal de despesas",
            Descricao = "Modelo das despesas do mês e dia de fechamento do balanço.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "O modelo do balanço",
                    Texto = "Aqui a gestão monta o modelo que se repete todo mês: cada linha tem nome, setor, " +
                        "categoria em texto livre (energia, aluguel, investimento, perda…) e um valor padrão."
                },
                new()
                {
                    Seletor = "form-despesas-dia",
                    Titulo = "Dia de fechamento",
                    Texto = "O dia do mês (1 a 28) em que o balanço deve ser preenchido e fechado no Financeiro. " +
                        "Um aviso aparece nas telas 5 dias antes dessa data."
                },
                new()
                {
                    Seletor = "form-despesas-tabela",
                    Titulo = "As linhas do modelo",
                    Texto = "Adicione, edite ou desative linhas. Só as linhas ativas são copiadas para o balanço " +
                        "do mês quando o gerente clica em \"Gerar a partir do formulário\"."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/financeiro/a-receber",
            Titulo = "Contas a receber",
            Descricao = "Serviços concluídos aguardando cobrança.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "O que ainda falta receber",
                    Texto = "Lista as ordens de serviço com o trabalho técnico concluído mas ainda não pagas " +
                        "(status \"Aguardando cobrança\"). Propostas de venda com pagamento pendente aparecem " +
                        "na tela de Propostas."
                },
                new()
                {
                    Seletor = "areceber-kpis",
                    Titulo = "Totais",
                    Texto = "Valor total a receber, quantidade de ordens em aberto e há quantos dias está a mais antiga."
                },
                new()
                {
                    Seletor = "areceber-lista",
                    Titulo = "Detalhamento",
                    Texto = "Ordenada da mais antiga para a mais recente. Linhas em vermelho estão abertas há " +
                        "mais de 7 dias. Clique para abrir a OS e registrar o pagamento."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/financeiro/relatorios",
            Titulo = "Relatórios",
            Descricao = "Exportação de relatórios em CSV ou XML por período.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Relatórios exportáveis",
                    Texto = "Cada cartão gera um arquivo para baixar. Os relatórios \"consolidados\" recalculam " +
                        "as métricas inteiras para o período escolhido — não exportam o que está na tela."
                },
                new()
                {
                    Seletor = "rel-filtros",
                    Titulo = "Período e formato",
                    Texto = "Escolha a janela de tempo e o formato (CSV abre no Excel/LibreOffice; XML para " +
                        "integração). Valem para todos os relatórios da tela."
                }
            }
        }
    };
}
