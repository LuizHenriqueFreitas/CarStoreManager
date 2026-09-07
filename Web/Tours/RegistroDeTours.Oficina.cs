namespace CarStoreManager.Web.Tours;

public static partial class RegistroDeTours
{
    private static List<TourDaPagina> TodosOficina() => new()
    {
        // /oficina/os/nova — tour PILOTO: escrito primeiro e usado pra validar
        // o componente TourGuiado no pior caso (tela mais densa do sistema,
        // com 3 componentes filhos reaproveitados de outras telas).
        new TourDaPagina
        {
            RotaTemplate = "/oficina/os/nova",
            Titulo = "Nova Ordem de Serviço",
            Descricao = "Abertura de uma ordem de serviço pela recepção.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Abrindo uma nova OS",
                    Texto = "Esta tela registra uma ordem de serviço — o pedido formal de manutenção de um " +
                        "veículo. Ela nasce com status \"Pendente\" e segue um fluxo de aprovação até ser " +
                        "finalizada e entregue. Vamos percorrer cada bloco do formulário."
                },
                new()
                {
                    Seletor = "os-cliente-secao",
                    Titulo = "Dados do cliente",
                    Texto = "Busque um cliente já cadastrado pelo nome ou CPF, ou cadastre um novo aqui mesmo. " +
                        "O CPF é validado pelo dígito verificador oficial — um número com dígitos inconsistentes " +
                        "é rejeitado antes de chegar ao banco."
                },
                new()
                {
                    Seletor = "os-veiculo-secao",
                    Titulo = "Veículo do cliente",
                    Texto = "Informe o veículo que vai passar pela manutenção, ou busque um já cadastrado para " +
                        "esse cliente. A placa aceita tanto o padrão antigo (ABC1234) quanto o padrão Mercosul " +
                        "(ABC1D23) e é validada no formato exato."
                },
                new()
                {
                    Seletor = "os-mecanico-select",
                    Titulo = "Mecânico responsável",
                    Texto = "Escolha quem vai executar o serviço. A especialidade aparece ao lado do nome para " +
                        "ajudar na escolha — um mecânico com muitas ordens ativas simultâneas aparece como " +
                        "\"Ocupado\" ou \"Indisponível\" nas telas de equipe, mas nada aqui impede a atribuição."
                },
                new()
                {
                    Seletor = "os-descricao",
                    Titulo = "Descrição do problema",
                    Texto = "Descreva em texto livre o que o cliente relatou ou o que foi identificado na " +
                        "recepção. É esse texto que o mecânico vê primeiro ao montar o orçamento."
                },
                new()
                {
                    Seletor = "os-tipo",
                    Titulo = "Tipo de serviço",
                    Texto = "Classifica a ordem para fins de relatório e dos gráficos do dashboard — " +
                        "manutenção, revisão, troca de peças, diagnóstico ou outro."
                },
                new()
                {
                    Seletor = "os-valor-estimado",
                    Titulo = "Valor estimado da mão de obra",
                    Texto = "Este valor cobre só a mão de obra prevista para o serviço. O valor final da ordem " +
                        "soma esse número ao preço de cada peça adicionada depois, na tela de detalhe."
                },
                new()
                {
                    Seletor = "os-prazo",
                    Titulo = "Data prevista de conclusão",
                    Texto = "Prazo estimado para entregar o veículo. Ele aparece em destaque na tela de detalhe " +
                        "se a data passar antes da ordem ser finalizada."
                },
                new()
                {
                    Seletor = "os-checklist-preset",
                    Titulo = "Preset de checklist",
                    Texto = "Presets são modelos de checklist cadastrados em Configurações — escolher um aqui " +
                        "copia a lista de itens padrão para esta ordem, e o mecânico ainda pode adicionar ou " +
                        "remover itens depois. Deixe em branco para montar o checklist do zero."
                },
                new()
                {
                    Seletor = "os-itens-secao",
                    Titulo = "Peças utilizadas",
                    Texto = "Adicione peças buscando pelo nome, código OEM, part number ou SKU. Cada peça pode " +
                        "vir do estoque da oficina (o que baixa a quantidade disponível automaticamente) ou ser " +
                        "fornecida pelo próprio cliente — nesse caso o estoque não é afetado."
                },
                new()
                {
                    Seletor = "os-criar-btn",
                    Titulo = "Criar a ordem",
                    Texto = "Cliente, veículo, mecânico e descrição são obrigatórios. Depois de criada, a ordem " +
                        "abre na tela de detalhe, de onde ela avança pelo fluxo de aprovação."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Próximo passo",
                    Texto = "Depois de criada, a OS aparece na fila de \"Pendentes\" da tela Oficina. O tour da " +
                        "tela de detalhe explica o restante do fluxo, do envio para revisão até a entrega ao cliente."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/oficina/recepcao",
            Titulo = "Recepção",
            Descricao = "Tela de atendimento — abrir OS, cobrar e entregar.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Painel da recepção",
                    Texto = "Esta é a tela de trabalho de quem atende o cliente no balcão — abre novas ordens de " +
                        "serviço e recebe o pagamento na entrega do veículo. O painel geral da oficina (KPIs, " +
                        "board de etapas) fica em \"Visão geral\"."
                },
                new()
                {
                    Seletor = "recepcao-btn-nova",
                    Titulo = "Abrir nova ordem",
                    Texto = "Atalho direto para o formulário de abertura de OS."
                },
                new()
                {
                    Seletor = "recepcao-metricas",
                    Titulo = "Métricas do dia",
                    Texto = "Contagem rápida — em destaque, quantas ordens estão aguardando cobrança."
                },
                new()
                {
                    Seletor = "recepcao-cobranca",
                    Titulo = "Aguardando cobrança e entrega",
                    Texto = "Quando o mecânico termina o serviço, a ordem cai aqui com status \"Aguardando " +
                        "cobrança\" — falta receber do cliente antes de liberar o veículo. Clique na ordem para " +
                        "abrir o painel de pagamento e registrar o recebimento."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/oficina",
            Titulo = "Gestão da Oficina",
            Descricao = "Painel geral: KPIs, board de OS por etapa e pendências.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Visão geral da oficina",
                    Texto = "Em uma tela: como a oficina está agora, o que precisa de atenção e para onde ir. " +
                        "Um mecânico vê só as ordens atribuídas a ele."
                },
                new()
                {
                    Seletor = "oficina-hub-kpis",
                    Titulo = "Indicadores",
                    Texto = "OS pendentes, em andamento, aguardando cobrança, entregues no mês e prazo médio " +
                        "estimado."
                },
                new()
                {
                    Seletor = "oficina-hub-board",
                    Titulo = "Ordens por etapa",
                    Texto = "Cada coluna é uma etapa do fluxo da OS. Clique num card para abrir a ordem. Para a " +
                        "lista completa com filtros e busca, use \"Ordens de serviço\"."
                },
                new()
                {
                    Seletor = "oficina-hub-atencao",
                    Titulo = "Precisa de atenção",
                    Texto = "Cobranças pendentes, prazos estourados e peças abaixo do mínimo, já com o link " +
                        "para resolver."
                },
                new()
                {
                    Seletor = "oficina-hub-graficos",
                    Titulo = "Gráficos",
                    Texto = "Receita de serviços nos últimos meses (linha) e ordens por situação (barra). " +
                        "Visível para administração e chefia."
                },
                new()
                {
                    Seletor = "oficina-hub-atalhos",
                    Titulo = "Ir para",
                    Texto = "Atalhos para estoque, fornecedores, equipe, financeiro da oficina, despesas, " +
                        "relatórios e análises."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/oficina/ordens",
            Titulo = "Ordens de serviço",
            Descricao = "Lista completa com filtros por status e busca.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Todas as ordens",
                    Texto = "Lista completa das OS. Filtre por status (incluindo \"Atrasadas\") ou busque por " +
                        "número, cliente ou placa. Clique em qualquer card para abrir o detalhe."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/oficina/os/{id:guid}",
            Titulo = "Detalhe da Ordem de Serviço",
            Descricao = "Acompanhamento e execução do fluxo completo de uma ordem de serviço.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Detalhe da ordem",
                    Texto = "Esta tela concentra todo o ciclo de vida de uma ordem de serviço: orçamento, " +
                        "revisão técnica, aprovação do cliente, execução, checklist, peças e entrega. Os " +
                        "botões disponíveis mudam de acordo com a etapa atual e com o seu papel no sistema."
                },
                new()
                {
                    Seletor = "os-detalhe-header",
                    Titulo = "Número e status",
                    Texto = "O número público identifica a ordem em conversas com o cliente e em consultas " +
                        "externas. O selo ao lado mostra a etapa atual do fluxo."
                },
                new()
                {
                    Seletor = "os-detalhe-acoes",
                    Titulo = "Ações da etapa atual",
                    Texto = "Só aparecem os botões válidos para o status e para o seu papel — por exemplo, " +
                        "\"Enviar para revisão\" some depois que a ordem já foi enviada, e \"Cobrar e entregar\" " +
                        "só existe para Administração e Recepção."
                },
                new()
                {
                    Seletor = "os-detalhe-trilha",
                    Titulo = "Trilha do fluxo",
                    Texto = "Visão das sete etapas da ordem, da abertura do orçamento até a finalização. Uma " +
                        "ordem cancelada sai da trilha normal e aparece marcada à parte."
                },
                new()
                {
                    Seletor = "os-detalhe-info-gerais",
                    Titulo = "Informações gerais",
                    Texto = "Tipo de serviço, descrição relatada, datas e valores. O \"Custo do serviço\" é só " +
                        "a mão de obra — o \"Valor total\" já soma as peças adicionadas."
                },
                new()
                {
                    Seletor = "os-detalhe-checklist",
                    Titulo = "Checklist de execução",
                    Texto = "Lista de verificações do serviço, vinda de um preset ou montada manualmente. " +
                        "Clique no círculo de um item para avançar seu status — pendente, em andamento, concluído."
                },
                new()
                {
                    Seletor = "os-detalhe-pecas",
                    Titulo = "Peças utilizadas",
                    Texto = "Peças já adicionadas à ordem, com origem (estoque, cliente ou encomenda) e valor. " +
                        "Adicionar uma peça vinda do estoque com a ordem já em andamento pausa o serviço até o " +
                        "cliente reaprovar o novo valor."
                },
                new()
                {
                    Seletor = "os-detalhe-progresso",
                    Titulo = "Progresso do checklist",
                    Texto = "Percentual de itens do checklist já concluídos — um resumo visual rápido do quanto " +
                        "falta para o serviço terminar."
                },
                new()
                {
                    Seletor = "os-detalhe-paineis",
                    Titulo = "Requisições, alertas e pagamento",
                    Texto = "Abaixo do conteúdo principal ficam os painéis de requisições de peças em falta, " +
                        "alertas da ordem (como reaprovações pendentes) e o registro de pagamentos recebidos."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Do orçamento à entrega",
                    Texto = "O mecânico finaliza o serviço sem precisar do pagamento — a ordem vai para " +
                        "\"Pagamento pendente\" e só é liberada para entrega depois que a recepção recebe o " +
                        "valor total pelo painel de pagamento."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/oficina/componentes",
            Titulo = "Componentes",
            Descricao = "Cadastro de peças e controle de estoque da oficina.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Estoque de componentes",
                    Texto = "Aqui ficam cadastradas todas as peças usadas nas ordens de serviço, com controle " +
                        "de quantidade em estoque. É essa lista que abastece a busca de peças na abertura e no " +
                        "detalhe de uma OS."
                },
                new()
                {
                    Seletor = "comp-btn-novo",
                    Titulo = "Novo componente",
                    Texto = "Cadastra uma peça nova, com SKU interno, part number, código OEM, sistema do " +
                        "veículo e dados fiscais (NCM, CEST). Só a Administração cadastra componentes novos."
                },
                new()
                {
                    Seletor = "comp-alerta-estoque",
                    Titulo = "Estoque abaixo do mínimo",
                    Texto = "Este aviso aparece quando alguma peça está com quantidade abaixo do mínimo " +
                        "configurado para ela. Clique para filtrar só as peças nessa situação."
                },
                new()
                {
                    Seletor = "comp-filtros",
                    Titulo = "Filtros e busca",
                    Texto = "Filtre por sistema do veículo (motor, freios, elétrica...) ou busque por nome, SKU, " +
                        "part number, código OEM, marca, categoria ou código de barras."
                },
                new()
                {
                    Seletor = "comp-tabela",
                    Titulo = "Lista de componentes",
                    Texto = "Cada linha mostra nome, part number, sistema e quantidade em estoque. Clique no " +
                        "nome de uma peça para ver o cadastro completo e buscar peças equivalentes pelo código OEM."
                },
                new()
                {
                    Seletor = "comp-row-acoes",
                    Titulo = "Entrada e estoque mínimo",
                    Texto = "\"+ Entrada\" registra a chegada de novas unidades no estoque. \"Mínimo\" define a " +
                        "partir de qual quantidade essa peça deve aparecer no alerta de estoque baixo. Só a " +
                        "Administração vê essas ações."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "De volta às ordens",
                    Texto = "As peças cadastradas aqui ficam disponíveis para adicionar em qualquer ordem de " +
                        "serviço, tanto na abertura quanto no detalhe."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/oficina/fornecedores",
            Titulo = "Fornecedores",
            Descricao = "Cadastro dos fornecedores de peças da oficina.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Fornecedores de peças",
                    Texto = "Cadastro dos fornecedores usados no cadastro de componentes. Nome e CNPJ são " +
                        "obrigatórios; endereço, e-mail e telefone são opcionais."
                },
                new()
                {
                    Seletor = "forn-btn-novo",
                    Titulo = "Novo fornecedor",
                    Texto = "Abre o formulário de cadastro. O CNPJ precisa ser único — o sistema recusa duplicados."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Ativar e desativar",
                    Texto = "Um fornecedor desativado deixa de aparecer no autocomplete de cadastro de peças, " +
                        "mas o histórico é preservado."
                }
            }
        }
    };
}
