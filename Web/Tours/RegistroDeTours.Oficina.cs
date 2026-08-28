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
            RotaTemplate = "/recepcao",
            Titulo = "Recepção",
            Descricao = "Painel de atendimento — visão geral das ordens de serviço em andamento.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Painel da recepção",
                    Texto = "Esta é a tela de trabalho de quem atende o cliente no balcão — abre novas ordens de " +
                        "serviço, acompanha o andamento e recebe o pagamento na entrega do veículo."
                },
                new()
                {
                    Seletor = "recepcao-btn-nova",
                    Titulo = "Abrir nova ordem",
                    Texto = "Atalho direto para o formulário de abertura de OS — o mesmo formulário acessível " +
                        "pela tela Oficina."
                },
                new()
                {
                    Seletor = "recepcao-metricas",
                    Titulo = "Métricas do dia",
                    Texto = "Contagem rápida de ordens pendentes de revisão, em andamento na oficina, aguardando " +
                        "cobrança e entregues no mês. Some rápido — não são números fechados de caixa, só um panorama."
                },
                new()
                {
                    Seletor = "recepcao-cobranca",
                    Titulo = "Aguardando cobrança e entrega",
                    Texto = "Quando o mecânico termina o serviço, a ordem cai aqui com status \"Pagamento " +
                        "pendente\" — falta receber do cliente antes de liberar o veículo. Clique na ordem para " +
                        "abrir o painel de pagamento e registrar o recebimento."
                },
                new()
                {
                    Seletor = "recepcao-ordens-recentes",
                    Titulo = "Ordens recentes",
                    Texto = "As demais ordens em andamento, das mais recentes para as mais antigas pelo prazo " +
                        "estimado. Clique em qualquer uma para abrir o detalhe completo."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Fluxo completo",
                    Texto = "Para acompanhar uma ordem específica do início ao fim — orçamento, aprovação do " +
                        "cliente, execução e entrega — abra o tour da tela de detalhe da OS."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/oficina",
            Titulo = "Ordens de Serviço",
            Descricao = "Lista de todas as ordens de serviço, com filtro por status.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Todas as ordens de serviço",
                    Texto = "Esta tela lista todas as ordens de serviço do sistema. Um mecânico vê só as ordens " +
                        "atribuídas a ele — as demais funções veem a lista completa."
                },
                new()
                {
                    Seletor = "oficina-btn-nova",
                    Titulo = "Nova ordem",
                    Texto = "Abre o formulário de criação de uma nova OS. Este botão não aparece para o " +
                        "mecânico, que só acompanha as ordens já atribuídas a ele."
                },
                new()
                {
                    Seletor = "oficina-filtros",
                    Titulo = "Filtro por status",
                    Texto = "Filtre a lista por uma etapa do fluxo — pendente, em andamento, pagamento " +
                        "pendente, finalizada ou cancelada. \"Todas\" limpa o filtro."
                },
                new()
                {
                    Seletor = "oficina-card-exemplo",
                    Titulo = "Card de uma ordem",
                    Texto = "Cada card resume número, tipo de serviço, prazo, valor total e status atual. Clique " +
                        "em qualquer card para abrir o detalhe completo da ordem."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Abrindo uma ordem",
                    Texto = "Ao clicar em uma ordem, você chega à tela de detalhe — é lá que o fluxo avança, " +
                        "etapa por etapa, até a entrega ao cliente."
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
        }
    };
}
