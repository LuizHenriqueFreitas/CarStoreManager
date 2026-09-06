namespace CarStoreManager.Web.Tours;

public static partial class RegistroDeTours
{
    private static List<TourDaPagina> TodosAdministracao() => new()
    {
        new TourDaPagina
        {
            RotaTemplate = "/dashboard",
            Titulo = "Dashboard",
            Descricao = "Visão geral do negócio — financeiro, oficina, concessionária e pessoas.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Painel gerencial",
                    Texto = "Este é o painel de acompanhamento do negócio. Um administrador vê tudo — " +
                        "oficina, concessionária e financeiro geral — enquanto o chefe de oficina e o gerente " +
                        "de vendas veem só os números e relatórios do próprio setor."
                },
                new()
                {
                    Seletor = "dashboard-abas",
                    Titulo = "Abas Geral, Oficina e Concessionária",
                    Texto = "\"Geral\" cruza os dois setores num só painel financeiro. \"Oficina\" e " +
                        "\"Concessionária\" trazem o detalhe operacional de cada área — só aparecem para quem " +
                        "tem acesso a elas."
                },
                new()
                {
                    Seletor = "dashboard-metricas-geral",
                    Titulo = "Receitas, despesas e lucro do mês",
                    Texto = "Receitas soma serviços da oficina e vendas da concessionária; despesas soma as " +
                        "despesas fixas cadastradas em Configurações. O lucro líquido é simplesmente a " +
                        "diferença entre os dois."
                },
                new()
                {
                    Seletor = "dashboard-capital-imobilizado",
                    Titulo = "Capital imobilizado",
                    Texto = "Valor somado dos veículos disponíveis no pátio, aguardando venda. Esse número " +
                        "não entra no lucro do mês — é dinheiro investido em estoque, só vira receita quando " +
                        "o veículo é vendido."
                },
                new()
                {
                    Seletor = "dashboard-analises",
                    Titulo = "Catálogo de análises",
                    Texto = "Troque o gráfico exibido aqui pelo menu — há opções agrupadas por Financeiro, " +
                        "Oficina, Concessionária, Pessoas e Estoque, sem precisar sair da aba Geral para " +
                        "comparar métricas de setores diferentes."
                },
                new()
                {
                    Seletor = "dashboard-oficina-metricas",
                    Titulo = "Operação da oficina",
                    Texto = "Contagem rápida de ordens de serviço pendentes, em andamento, finalizadas no " +
                        "mês e peças com estoque abaixo do mínimo. Visível na aba Oficina."
                },
                new()
                {
                    Seletor = "dashboard-concessionaria-metricas",
                    Titulo = "Operação da concessionária",
                    Texto = "Veículos disponíveis para venda, já vendidos e propostas ainda em aberto " +
                        "(nem aprovadas, nem rejeitadas, nem canceladas). Visível na aba Concessionária."
                },
                new()
                {
                    Seletor = "dashboard-relatorios",
                    Titulo = "Relatórios",
                    Texto = "Cada botão abre um modal para escolher o período e o formato do arquivo. Os " +
                        "três relatórios \"completo\" não exportam o que já está na tela — eles recalculam " +
                        "métricas e gráficos inteiramente para o período escolhido."
                },
                new()
                {
                    Seletor = "dashboard-pessoas",
                    Titulo = "Pessoas cadastradas",
                    Texto = "Total de clientes, mecânicos e vendedores ativos no sistema — um panorama do " +
                        "tamanho da operação, independente do mês."
                },
                new()
                {
                    Seletor = "dashboard-atalhos",
                    Titulo = "Atalhos para os módulos",
                    Texto = "Acesso rápido às telas de Concessionária, Oficina, Estoque de componentes e " +
                        "Usuários, sem precisar navegar pelo menu principal."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Ponto de partida do dia a dia",
                    Texto = "O dashboard é o primeiro lugar a olhar antes de decidir onde agir — um alerta " +
                        "de estoque baixo ou uma consignação vencendo, por exemplo, levam direto para a tela " +
                        "certa através dos atalhos e avisos exibidos aqui."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/admin/usuarios",
            Titulo = "Usuários",
            Descricao = "Cadastro e desativação dos funcionários com acesso ao sistema.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Gestão de funcionários",
                    Texto = "Esta tela cadastra e desativa os funcionários que têm login no sistema. Só o " +
                        "administrador acessa esta tela."
                },
                new()
                {
                    Seletor = "usuarios-btn-novo",
                    Titulo = "Novo usuário",
                    Texto = "Cadastra um funcionário com tipo, nome, e-mail, telefone e senha inicial. " +
                        "Vendedor, mecânico, recepcionista, chefe de oficina e gerente de vendas também " +
                        "exigem data de contratação, e o mecânico exige uma especialidade."
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
                    Texto = "Restringe a lista a um único perfil — útil para conferir rapidamente, por " +
                        "exemplo, só os mecânicos ou só os administradores cadastrados."
                },
                new()
                {
                    Seletor = "usuarios-card-exemplo",
                    Titulo = "Card de um funcionário",
                    Texto = "A cor do avatar e do selo identificam o tipo do funcionário à primeira vista. " +
                        "Seu próprio usuário aparece marcado como \"você\", sem o botão de desativar."
                },
                new()
                {
                    Seletor = "usuarios-btn-desativar",
                    Titulo = "Desativar um usuário",
                    Texto = "Revoga o acesso do funcionário ao sistema. Por segurança, a confirmação pede a " +
                        "sua própria senha de administrador antes de aplicar a desativação — não a senha do " +
                        "usuário sendo desativado."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Acesso concedido, não configuração",
                    Texto = "Esta tela só controla quem consegue entrar no sistema e com qual perfil. Ajustes " +
                        "de comportamento do sistema em si ficam em Configurações do sistema."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/admin/configuracoes",
            Titulo = "Configurações do Sistema",
            Descricao = "Parâmetros globais de funcionamento — geral, despesas, checklists, operação e documentos.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Configurações administrativas",
                    Texto = "Esta tela reúne os parâmetros que afetam o sistema inteiro, organizados em " +
                        "cinco abas. Só o administrador acessa esta tela."
                },
                new()
                {
                    Seletor = "cfg-abas",
                    Titulo = "Cinco áreas de configuração",
                    Texto = "Geral, Despesas mensais, Checklists, Modo operante e Documentos — cada aba " +
                        "guarda um grupo de parâmetros independente dos demais."
                },
                new()
                {
                    Seletor = "cfg-integracao-ml",
                    Titulo = "Integração com Mercado Livre",
                    Texto = "Ponto de entrada para conectar a loja ao Mercado Livre e gerenciar os anúncios " +
                        "publicados. Os tours dessas duas telas explicam o fluxo completo."
                },
                new()
                {
                    Seletor = "cfg-despesas-tabela",
                    Titulo = "Despesas mensais fixas",
                    Texto = "Cadastre aqui aluguel, contas e salários recorrentes. A soma das despesas ativas " +
                        "de cada setor (Geral, Oficina, Concessionária) é o que aparece nos números de " +
                        "despesa e lucro operacional do dashboard."
                },
                new()
                {
                    Seletor = "cfg-checklists-lista",
                    Titulo = "Presets de checklist",
                    Texto = "Modelos de checklist reaproveitáveis na abertura de uma ordem de serviço. Ao " +
                        "escolher um preset na OS, os itens são copiados como uma cópia independente — editar " +
                        "o preset depois não altera as ordens já criadas."
                },
                new()
                {
                    Seletor = "cfg-operacao-entrada",
                    Titulo = "Entrada mínima obrigatória",
                    Texto = "Se ativado, uma ordem de serviço só sai de \"Aprovada\" para \"Em andamento\" " +
                        "depois que o cliente pagar o percentual mínimo configurado do valor total. Desativado, " +
                        "o mecânico pode iniciar o serviço assim que o cliente aprova o orçamento."
                },
                new()
                {
                    Seletor = "cfg-documentos-templates",
                    Titulo = "Templates de documentos",
                    Texto = "Texto-base do termo de entrega de veículo, do contrato de consignação e do " +
                        "registro de resposta da financiadora. Serve só para pré-preencher — cada proposta, " +
                        "veículo ou consignação ainda pode ajustar o texto individualmente."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Alcance das mudanças",
                    Texto = "Alterações feitas aqui valem para o sistema inteiro, não só para quem está " +
                        "configurando — revise com atenção antes de salvar, principalmente na aba Modo operante."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/configuracoes",
            Titulo = "Configurações da Conta",
            Descricao = "Dados pessoais e senha do próprio usuário logado.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Sua conta",
                    Texto = "Esta tela é de autoatendimento — qualquer funcionário logado edita aqui os " +
                        "próprios dados de contato e a própria senha, independente do seu perfil."
                },
                new()
                {
                    Seletor = "conta-dados-pessoais",
                    Titulo = "Dados pessoais",
                    Texto = "O nome não é editável nesta tela de autoatendimento. E-mail e telefone você " +
                        "mesmo mantém atualizados."
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
                    Texto = "Exige a senha atual antes de aceitar a nova — protege a conta caso alguém " +
                        "encontre a sessão aberta. A nova senha e a confirmação precisam ser idênticas."
                },
                new()
                {
                    Seletor = "conta-btn-salvar-senha",
                    Titulo = "Confirmar a nova senha",
                    Texto = "Depois de salva, use a nova senha no próximo login — a atual continua valendo " +
                        "para a sessão já aberta."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Sua conta, não a dos outros",
                    Texto = "Para gerenciar outros funcionários — cadastrar ou desativar — é preciso ser " +
                        "administrador e usar a tela de Usuários, não esta aqui."
                }
            }
        }
    };
}
