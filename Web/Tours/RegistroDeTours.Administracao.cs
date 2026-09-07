namespace CarStoreManager.Web.Tours;

public static partial class RegistroDeTours
{
    private static List<TourDaPagina> TodosAdministracao() => new()
    {
        // ============================================================
        // ANÁLISES (antiga Dashboard) — só gráficos e comparativos.
        // Os números financeiros migraram para o Financeiro.
        // ============================================================
        new TourDaPagina
        {
            RotaTemplate = "/dashboard",
            Titulo = "Análises",
            Descricao = "Indicadores e gráficos comparativos da oficina e da concessionária.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Painel de análises",
                    Texto = "Esta tela concentra os gráficos e comparativos do negócio. Um administrador vê " +
                        "os dois setores; o chefe de oficina e o gerente de vendas veem só o próprio."
                },
                new()
                {
                    Seletor = "dashboard-periodo",
                    Titulo = "Período dos gráficos",
                    Texto = "Escolha a janela de tempo (3, 6 ou 12 meses) usada pelas séries temporais " +
                        "de todos os gráficos da tela."
                },
                new()
                {
                    Seletor = "dashboard-relatorio",
                    Titulo = "Exportar relatório",
                    Texto = "Um botão só: escolha o tipo de relatório e o período, e baixe em CSV ou XML. " +
                        "Os relatórios \"consolidados\" recalculam as métricas inteiras para o período pedido."
                },
                new()
                {
                    Seletor = "dashboard-abas",
                    Titulo = "Abas Geral, Oficina e Concessionária",
                    Texto = "\"Geral\" traz o gráfico de faturamento e o catálogo de comparativos. " +
                        "\"Oficina\" e \"Concessionária\" detalham cada área — só aparecem para quem tem acesso."
                },
                new()
                {
                    Seletor = "dashboard-analises",
                    Titulo = "Catálogo de comparativos",
                    Texto = "Troque o gráfico exibido pelo menu — opções agrupadas por Financeiro, Oficina, " +
                        "Concessionária, Pessoas e Estoque, sem sair da aba Geral."
                },
                new()
                {
                    Seletor = "dashboard-oficina-metricas",
                    Titulo = "Operação da oficina",
                    Texto = "Contagem rápida de ordens de serviço por situação e de peças com estoque baixo. " +
                        "Visível na aba Oficina."
                },
                new()
                {
                    Seletor = "dashboard-concessionaria-metricas",
                    Titulo = "Operação da concessionária",
                    Texto = "Veículos disponíveis, vendidos e propostas em aberto. Visível na aba Concessionária."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Onde ver os números de dinheiro",
                    Texto = "Receita, despesa, lucro, contas a receber e relatórios exportáveis ficam agora " +
                        "na tela Financeiro, no menu principal."
                }
            }
        },

        // ============================================================
        // CONFIGURAÇÕES — índice
        // ============================================================
        new TourDaPagina
        {
            RotaTemplate = "/configuracoes",
            Titulo = "Configurações",
            Descricao = "Parâmetros de funcionamento, presets e dados do sistema.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Central de configurações",
                    Texto = "Aqui ficam só coisas de funcionamento do sistema: o formulário mensal de despesas, " +
                        "os parâmetros de operação, e a importação/exportação de dados. Clientes e Equipe têm " +
                        "telas próprias no menu de cima."
                },
                new()
                {
                    Seletor = "config-cards",
                    Titulo = "Cada card é um atalho",
                    Texto = "Clique em um card para abrir a área correspondente. Os mesmos destinos também " +
                        "estão no menu \"Configurações\" da barra superior."
                }
            }
        },

        // ============================================================
        // CLIENTES — lista
        // ============================================================
        new TourDaPagina
        {
            RotaTemplate = "/clientes",
            Titulo = "Clientes",
            Descricao = "Cadastro único de clientes do sistema — oficina e concessionária.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Um cadastro por CPF",
                    Texto = "Cada cliente tem um único cadastro no sistema, identificado pelo CPF. Se o mesmo " +
                        "cliente é atendido na oficina e na concessionária, é o mesmo registro — com o histórico " +
                        "dos dois lados."
                },
                new()
                {
                    Seletor = "clientes-busca",
                    Titulo = "Busca",
                    Texto = "Filtra a lista por nome, CPF ou telefone conforme você digita."
                },
                new()
                {
                    Seletor = "clientes-card-exemplo",
                    Titulo = "Abrir a ficha",
                    Texto = "Clique no cartão do cliente para abrir a ficha completa (histórico, veículos, " +
                        "valores). O botão \"Editar\" abre só o formulário de dados cadastrais."
                }
            }
        },

        // ============================================================
        // CLIENTES — ficha 360°
        // ============================================================
        new TourDaPagina
        {
            RotaTemplate = "/clientes/{id:guid}",
            Titulo = "Ficha do cliente",
            Descricao = "Visão 360° de um cliente: dados, veículos e histórico de atendimentos.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Tudo sobre o cliente numa tela",
                    Texto = "Reúne os dados de contato, os veículos, o histórico de ordens de serviço e de " +
                        "propostas de venda, e um resumo de quanto o cliente já gastou em cada setor."
                },
                new()
                {
                    Seletor = "ficha-resumo",
                    Titulo = "Resumo",
                    Texto = "Quantas OS e propostas o cliente tem, quanto já gastou na oficina e na " +
                        "concessionária, e a data do último atendimento (em qualquer setor)."
                },
                new()
                {
                    Seletor = "ficha-veiculos",
                    Titulo = "Veículos do cliente",
                    Texto = "Os veículos que o cliente já trouxe à oficina, com a contagem de atendimentos de cada um."
                },
                new()
                {
                    Seletor = "ficha-historico",
                    Titulo = "Histórico",
                    Texto = "Linha do tempo de ordens de serviço e propostas. Clique em qualquer linha para " +
                        "abrir o detalhe daquele atendimento."
                }
            }
        },

        // ============================================================
        // EQUIPE / USUÁRIOS
        // ============================================================
        new TourDaPagina
        {
            RotaTemplate = "/equipe",
            Titulo = "Equipe",
            Descricao = "Cadastro e desativação dos funcionários com acesso ao sistema.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Gestão de funcionários",
                    Texto = "Cadastra e desativa os funcionários que têm login no sistema. Só o administrador acessa."
                },
                new()
                {
                    Seletor = "usuarios-btn-novo",
                    Titulo = "Novo usuário",
                    Texto = "Cadastra um funcionário com tipo, nome, e-mail, telefone e senha inicial. Os perfis " +
                        "operacionais exigem data de contratação, e o mecânico exige uma especialidade."
                },
                new()
                {
                    Seletor = "usuarios-busca",
                    Titulo = "Busca por nome ou telefone",
                    Texto = "Filtra a lista conforme você digita, sem precisar apertar Enter."
                },
                new()
                {
                    Seletor = "usuarios-filtro-tipo",
                    Titulo = "Filtro por tipo",
                    Texto = "Restringe a lista a um único perfil — útil para conferir, por exemplo, só os mecânicos."
                },
                new()
                {
                    Seletor = "usuarios-card-exemplo",
                    Titulo = "Card de um funcionário",
                    Texto = "A cor do avatar e do selo identificam o tipo do funcionário. Seu próprio usuário " +
                        "aparece marcado como \"você\", sem o botão de desativar."
                },
                new()
                {
                    Seletor = "usuarios-btn-desativar",
                    Titulo = "Desativar um usuário",
                    Texto = "Revoga o acesso do funcionário. A confirmação pede a sua própria senha de " +
                        "administrador — não a senha do usuário sendo desativado."
                }
            }
        },

        // ============================================================
        // CONFIGURAÇÕES DO SISTEMA — menu de botões
        // ============================================================
        new TourDaPagina
        {
            RotaTemplate = "/configuracoes/sistema",
            Titulo = "Configurações do sistema",
            Descricao = "Integrações, checklists, modo operante, documentos e importação.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Configurações administrativas",
                    Texto = "Parâmetros que afetam o sistema inteiro. Só o administrador acessa. Escolha uma " +
                        "das áreas nos cartões — cada uma abre sua própria tela."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Integrações",
                    Texto = "Conexão com o Mercado Livre e gerenciamento dos anúncios publicados."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Checklists",
                    Texto = "Modelos de checklist reaproveitáveis na abertura de uma OS. Ao escolher um preset " +
                        "na OS, os itens são copiados — editar o preset depois não muda as OS já criadas."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Modo operante",
                    Texto = "Ajustes globais de operação, como exigir entrada mínima para iniciar um serviço."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Documentos",
                    Texto = "Templates dos documentos gerados pelo sistema (termos, propostas)."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Importar e Exportar dados",
                    Texto = "Importar carrega uma base a partir de um arquivo JSON. Exportar baixa todo o banco " +
                        "de dados da aplicação num único arquivo JSON — use para backup. A planilha de despesas " +
                        "mudou de lugar: agora fica no Financeiro."
                }
            }
        },

        // ============================================================
        // MINHA CONTA
        // ============================================================
        new TourDaPagina
        {
            RotaTemplate = "/conta",
            Titulo = "Minha conta",
            Descricao = "Dados pessoais e senha do próprio usuário logado.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Sua conta",
                    Texto = "Autoatendimento — qualquer funcionário logado edita aqui os próprios dados de " +
                        "contato e a própria senha, independente do perfil."
                },
                new()
                {
                    Seletor = "conta-dados-pessoais",
                    Titulo = "Dados pessoais",
                    Texto = "O nome não é editável aqui. E-mail e telefone você mesmo mantém atualizados."
                },
                new()
                {
                    Seletor = "conta-btn-salvar-dados",
                    Titulo = "Salvar dados de contato",
                    Texto = "Grava e-mail e telefone imediatamente, sem exigir confirmação de senha."
                },
                new()
                {
                    Seletor = "conta-alterar-senha",
                    Titulo = "Alterar senha",
                    Texto = "Exige a senha atual antes de aceitar a nova. A nova senha e a confirmação " +
                        "precisam ser idênticas."
                },
                new()
                {
                    Seletor = "conta-btn-salvar-senha",
                    Titulo = "Confirmar a nova senha",
                    Texto = "Depois de salva, use a nova senha no próximo login."
                }
            }
        }
    };
}
