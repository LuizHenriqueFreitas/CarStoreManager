namespace CarStoreManager.Web.Tours;

public static partial class RegistroDeTours
{
    private static List<TourDaPagina> TodosConcessionaria() => new()
    {
        new TourDaPagina
        {
            RotaTemplate = "/concessionaria",
            Titulo = "Concessionária — visão geral",
            Descricao = "Painel gerencial da concessionária: indicadores, propostas e consignações.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Visão geral da concessionária",
                    Texto = "Ponto de partida da área comercial — reúne num só painel o estado do salão, das " +
                        "propostas em andamento e das consignações. Para navegar o estoque em si, abra \"Salão\" " +
                        "na sub-navegação; o menu fixo à esquerda leva ao resto do módulo."
                },
                new()
                {
                    Seletor = "conc-hub-atalhos",
                    Titulo = "Ir para",
                    Texto = "Atalhos para salão, propostas, test drives, consignações e clientes. Financeiro, " +
                        "relatórios e análises só aparecem aqui para quem pode ver o financeiro da concessionária " +
                        "(Admin e Gerente de Vendas) — os demais perfis veem só os atalhos operacionais."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Indicadores do topo",
                    Texto = "\"Em preparação\" não entra na contagem de \"Disponíveis\" — é um veículo que ainda " +
                        "não pode ser oferecido a cliente. \"Vendas no mês\" e \"Ticket médio\" somam só propostas " +
                        "concluídas dentro do mês corrente, e zeram de novo na virada do mês."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Três avisos que pedem ação",
                    Texto = "\"Propostas que precisam de atenção\" lista quem já teve resposta da financiadora, " +
                        "foi aprovada, teve vistoria concluída ou está aguardando assinatura do termo. \"Veículos " +
                        "parados\" aponta quem está há mais de 60 dias disponível no estoque — um sinal pra rever " +
                        "o preço. \"Consignações vencendo\" usa a mesma janela de 7 dias do Salão. Cada lista " +
                        "mostra só 2 itens; o link \"Ver todas\" abre a tela cheia."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Resumo financeiro do mês",
                    Texto = "Card visível só para Admin e Gerente de Vendas: receita de vendas, despesas da " +
                        "concessionária, lucro operacional e quanto está imobilizado em aquisição de veículos " +
                        "ainda disponíveis. \"Ver no Financeiro\" abre o detalhe completo dessas contas."
                },
                new()
                {
                    Seletor = "conc-hub-graficos",
                    Titulo = "Gráficos",
                    Texto = "Vendas nos últimos meses (linha) e veículos por situação (barra) — mesma regra de " +
                        "acesso do card financeiro logo acima."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/concessionaria/salao",
            Titulo = "Salão",
            Descricao = "Grade de veículos próprios e consignados, com filtros.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Estoque do salão",
                    Texto = "Reúne, na mesma grade, os veículos da loja e os consignados por terceiros. Veículos " +
                        "já vendidos não aparecem aqui. Os filtros à esquerda e a busca acima ajudam a localizar " +
                        "um veículo."
                },
                new()
                {
                    Seletor = "conc-sidebar-filtros",
                    Titulo = "Filtros do estoque",
                    Texto = "Filtre por disponibilidade, marca, ano, valor, motorização, combustível, final de " +
                        "placa, IPVA do ano corrente e acessórios. Em \"Disponibilidade\", uma consignação com " +
                        "status Ativa conta como \"Disponível\", do mesmo jeito que um veículo próprio nesse status."
                },
                new()
                {
                    Seletor = "conc-toolbar",
                    Titulo = "Busca e ordenação",
                    Texto = "Busque livremente por modelo ou placa, e ordene a grade por data de cadastro, " +
                        "preço ou nome. A ordenação padrão mostra os veículos mais recentes primeiro."
                },
                new()
                {
                    Seletor = "conc-btn-cadastrar",
                    Titulo = "Cadastrar veículo da loja",
                    Texto = "Registra um veículo que pertence à concessionária. Antes de abrir o formulário, o " +
                        "sistema pede confirmação de senha — é o mesmo gatilho de segurança usado em outras " +
                        "ações sensíveis do módulo."
                },
                new()
                {
                    Seletor = "conc-btn-cadastrar-consignado",
                    Titulo = "Cadastrar consignação",
                    Texto = "Registra um veículo de terceiros que a loja vai vender em nome do proprietário, " +
                        "mediante comissão. É um fluxo de cadastro separado, com campos próprios de proprietário, " +
                        "vendedor responsável e prazo do contrato."
                },
                new()
                {
                    Seletor = "conc-banner-vencimento",
                    Titulo = "Consignações vencendo",
                    Texto = "Este aviso só aparece quando existe consignação ativa com 7 dias ou menos até o " +
                        "vencimento, ou já vencida. É um alerta rápido para renovar o prazo ou decidir o " +
                        "próximo passo com o proprietário."
                },
                new()
                {
                    Seletor = "conc-card-exemplo",
                    Titulo = "Card de um veículo",
                    Texto = "Cada card mostra a foto principal, marca, modelo, ano, valor e status. Veículos " +
                        "consignados recebem um selo \"Consignado\" adicional. Clique em qualquer card para " +
                        "abrir o detalhe completo."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Do estoque ao detalhe",
                    Texto = "Ao abrir um veículo, você chega à tela de detalhe — é lá que se gera uma proposta " +
                        "de venda, se libera um veículo em preparação, ou se administra uma consignação."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/concessionaria/consignacoes",
            Titulo = "Consignações",
            Descricao = "Lista de consignações por status, com renovação rápida.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Lista de consignações",
                    Texto = "Mostra vendedor responsável, valor esperado e quanto falta pro prazo vencer. Por " +
                        "padrão só aparecem as Ativas — troque o filtro de Situação pra ver vencendo, vendidas, " +
                        "concluídas, devolvidas ou canceladas. As colunas da tabela são clicáveis e ordenam a lista."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Renovação e detalhe",
                    Texto = "O botão \"Renovar +90d\" aparece direto na lista quando faltam 10 dias ou menos pro " +
                        "vencimento (ou já venceu) — evita abrir o detalhe só pra isso. Clique em qualquer linha " +
                        "para ver o histórico completo e trocar o vendedor, devolver ao proprietário ou cancelar " +
                        "o contrato."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/concessionaria/test-drives",
            Titulo = "Test drives",
            Descricao = "Agendamento e acompanhamento de test drives.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Agenda de test drives",
                    Texto = "\"+ Agendar test drive\" leva a uma tela própria pra escolher veículo, cliente, " +
                        "vendedor, data e redigir o termo de responsabilidade. Depois de agendado, marque o " +
                        "resultado aqui — realizado, não compareceu ou cancelado. O cliente precisa já estar " +
                        "cadastrado (Clientes)."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Filtrar por período",
                    Texto = "O filtro \"Quando\" isola os agendamentos de hoje, dos próximos 7 dias ou os que já " +
                        "passaram — útil pra achar rápido quem precisa de uma resposta sem rolar a lista inteira."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Termo de responsabilidade",
                    Texto = "O botão \"Termo\" de cada linha abre a tela de acompanhamento do termo — gerar o " +
                        "link de assinatura, ver se o cliente já assinou. Um test drive concluído (\"Realizado\") " +
                        "lança automaticamente R$ 50 de despesa de combustível, bancada pela concessionária."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/concessionaria/test-drives/novo",
            Titulo = "Agendar test drive",
            Descricao = "Formulário de agendamento com o termo de responsabilidade do cliente.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Uma tela só pro agendamento e pro termo",
                    Texto = "Antes era um modal — agora é uma tela própria, porque também é aqui que se " +
                        "redige o termo de responsabilidade que o cliente vai assinar."
                },
                new()
                {
                    Seletor = "td-form-agendamento",
                    Titulo = "Dados do passeio",
                    Texto = "Escolha primeiro se o veículo é próprio ou consignado — a busca abaixo muda de " +
                        "acordo (só consignações Ativas aparecem). Depois é cliente, vendedor e data/hora do " +
                        "test drive."
                },
                new()
                {
                    Seletor = "td-form-termo",
                    Titulo = "Termo de responsabilidade",
                    Texto = "Redija (ou aplique um template já cadastrado em Configurações → Documentos) o " +
                        "termo que o cliente assume ao dirigir o veículo no test drive. Obrigatório — nasce como " +
                        "rascunho junto com o agendamento; a assinatura do cliente acontece depois, por um link."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/concessionaria/test-drives/{id:guid}",
            Titulo = "Test drive — termo de responsabilidade",
            Descricao = "Acompanhamento do agendamento e da assinatura do termo.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Dados do passeio e do termo",
                    Texto = "Resumo do agendamento (veículo, cliente, vendedor, data, status) e o " +
                        "acompanhamento do termo de responsabilidade do cliente, no mesmo lugar. Enquanto o " +
                        "status for Agendado, Admin e Gerente de Vendas podem trocar o vendedor responsável " +
                        "pelo link \"Trocar\" ao lado do nome."
                },
                new()
                {
                    Seletor = "td-detalhe-termo",
                    Titulo = "Gerar o link de assinatura",
                    Texto = "Com o termo em rascunho, edite o texto se precisar e clique em \"Gerar link de " +
                        "assinatura\" — o cliente assina eletronicamente (nome, CPF, aceite), mesma validade " +
                        "jurídica do termo de entrega de veículo (Lei 14.063/2020). Mostre a tela do link no " +
                        "celular/tablet do cliente na hora do passeio, ou envie por mensagem."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/concessionaria/assinar-test-drive/{token}",
            Titulo = "Assinatura do termo de test drive",
            Descricao = "Página pública de assinatura eletrônica do termo de responsabilidade do test drive.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Página do cliente, sem login",
                    Texto = "Mesmo modelo da assinatura do termo de entrega: acessada pelo link gerado na tela " +
                        "do test drive (\"Gerar link de assinatura\"), sem menu e sem autenticação. Se o link já " +
                        "foi usado, a página mostra o termo como assinado em vez do formulário."
                },
                new()
                {
                    Seletor = "assinar-td-texto-termo",
                    Titulo = "Conteúdo do termo",
                    Texto = "Texto de responsabilidade que o cliente assume ao dirigir o veículo durante o " +
                        "passeio — redigido no agendamento do test drive."
                },
                new()
                {
                    Seletor = "assinar-td-form",
                    Titulo = "Assinatura eletrônica",
                    Texto = "Nome, CPF e o aceite explícito registram a assinatura eletrônica simples (Lei " +
                        "14.063/2020), com data/hora e IP capturados automaticamente na confirmação."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/concessionaria/veiculo/novo",
            Titulo = "Cadastrar Veículo da Loja",
            Descricao = "Registro de um veículo próprio no estoque de venda direta.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Novo veículo da loja",
                    Texto = "Este formulário cadastra um veículo que pertence à concessionária, disponível para " +
                        "venda direta. Ele nasce com status \"Em preparação\" e precisa ser liberado manualmente " +
                        "antes de aparecer como disponível para venda."
                },
                new()
                {
                    Seletor = "veiculo-novo-identificacao",
                    Titulo = "Identificação do veículo",
                    Texto = "A placa aceita tanto o padrão antigo (ABC1234) quanto o padrão Mercosul (ABC1D23). " +
                        "O RENAVAM é validado pelo dígito verificador oficial — um número com o dígito errado é " +
                        "rejeitado antes de chegar ao banco."
                },
                new()
                {
                    Seletor = "veiculo-novo-caracteristicas",
                    Titulo = "Câmbio e combustível",
                    Texto = "Essas características aparecem no anúncio do veículo e alimentam os filtros de " +
                        "busca do estoque — mantenha a informação fiel à ficha técnica real do veículo."
                },
                new()
                {
                    Seletor = "veiculo-novo-acessorios",
                    Titulo = "Acessórios",
                    Texto = "Marque os itens presentes no veículo. Eles aparecem no anúncio e também podem ser " +
                        "usados como filtro \"Tem\"/\"Não tem\" na busca do estoque."
                },
                new()
                {
                    Seletor = "veiculo-novo-termo",
                    Titulo = "Termo de entrega preliminar",
                    Texto = "Este texto vem pré-preenchido com o modelo definido em Configurações → Documentos. " +
                        "Ele serve de rascunho — quando uma proposta for gerada e a venda avançar, esse texto é " +
                        "a base do termo de entrega final, redigido e assinado pelo cliente naquele fluxo."
                },
                new()
                {
                    Seletor = "veiculo-novo-fotos",
                    Titulo = "Fotos do anúncio",
                    Texto = "Aceita JPEG ou PNG, até 5MB por arquivo. É possível reordenar as fotos depois, na " +
                        "tela de detalhe do veículo."
                },
                new()
                {
                    Seletor = "veiculo-novo-salvar-btn",
                    Titulo = "Cadastrar veículo",
                    Texto = "Marca, modelo, placa e RENAVAM são obrigatórios. Ao confirmar, o sistema pede sua " +
                        "senha antes de gravar o cadastro — a mesma confirmação usada nas demais ações sensíveis " +
                        "deste módulo."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Depois do cadastro",
                    Texto = "O veículo aparece no estoque com status \"Em preparação\". Use o botão \"Liberar " +
                        "para venda\" na tela de detalhe assim que ele estiver pronto para ser vendido."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/concessionaria/veiculo-consignado/novo",
            Titulo = "Cadastrar Veículo Consignado",
            Descricao = "Registro de um veículo de terceiros para venda em consignação.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Nova consignação",
                    Texto = "Este formulário registra um veículo de um cliente que a loja vai vender em seu " +
                        "nome, mediante comissão. É um cadastro diferente do veículo próprio — envolve " +
                        "proprietário, vendedor responsável, comissão e prazo de contrato."
                },
                new()
                {
                    Seletor = "consig-novo-identificacao",
                    Titulo = "Identificação do veículo",
                    Texto = "Mesma validação do cadastro de veículo próprio: a placa aceita padrão antigo ou " +
                        "Mercosul, e o RENAVAM é conferido pelo dígito verificador oficial."
                },
                new()
                {
                    Seletor = "consig-novo-caracteristicas",
                    Titulo = "Câmbio e combustível",
                    Texto = "Essas características aparecem no anúncio do veículo consignado, junto com os " +
                        "dados do veículo próprio no mesmo estoque unificado."
                },
                new()
                {
                    Seletor = "consig-novo-proprietario-vendedor",
                    Titulo = "Proprietário e vendedor",
                    Texto = "Busque o proprietário por nome ou CPF entre os clientes já cadastrados. O vendedor " +
                        "responsável é quem acompanha essa consignação — ele aparece no detalhe do veículo e no " +
                        "histórico de eventos."
                },
                new()
                {
                    Seletor = "consig-novo-comissao-prazo",
                    Titulo = "Comissão e prazo",
                    Texto = "A comissão do proprietário pode ser um valor fixo ou uma porcentagem sobre o valor " +
                        "de venda esperado. O prazo da consignação começa em 90 dias por padrão — passado esse " +
                        "prazo sem venda, a consignação pode ser renovada por mais 90 dias a partir da tela de " +
                        "detalhe do veículo."
                },
                new()
                {
                    Seletor = "consig-novo-contrato",
                    Titulo = "Contrato de consignação",
                    Texto = "O texto vem pré-preenchido com o modelo definido em Configurações → Documentos. Você " +
                        "também pode anexar o link de um PDF já assinado fora do sistema, se for o caso."
                },
                new()
                {
                    Seletor = "consig-novo-fotos",
                    Titulo = "Fotos do veículo",
                    Texto = "Aceita JPEG ou PNG, até 5MB por arquivo. Assim como no veículo próprio, dá para " +
                        "reordenar as fotos depois, na tela de detalhe."
                },
                new()
                {
                    Seletor = "consig-novo-salvar-btn",
                    Titulo = "Cadastrar consignação",
                    Texto = "Proprietário, vendedor responsável e valor de venda esperado são obrigatórios, além " +
                        "dos dados do veículo. A confirmação também exige sua senha antes de gravar."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Depois do cadastro",
                    Texto = "A consignação entra no estoque com status Ativa, disponível para venda imediatamente " +
                        "— ao contrário do veículo próprio, não passa por etapa de preparação. O acompanhamento " +
                        "completo (renovação, devolução, geração de proposta) fica na tela de detalhe do veículo."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/concessionaria/veiculo/{id:guid}",
            Titulo = "Detalhe do Veículo",
            Descricao = "Ficha completa de um veículo próprio ou consignado, com as ações do seu ciclo de vida.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Ficha do veículo",
                    Texto = "Esta mesma tela atende tanto veículos próprios quanto consignados — o conteúdo e " +
                        "as ações disponíveis mudam de acordo com o tipo e o status atual do veículo."
                },
                new()
                {
                    Seletor = "veiculo-detalhe-galeria",
                    Titulo = "Galeria de fotos",
                    Texto = "Navegue pelas fotos com as setas ou os pontos abaixo, e clique na foto principal " +
                        "para abrir em tela cheia."
                },
                new()
                {
                    Seletor = "veiculo-detalhe-header",
                    Titulo = "Identificação e status",
                    Texto = "O selo ao lado do nome mostra o status atual — Disponível, Em preparação, Vendido, " +
                        "ou o status equivalente de uma consignação (Ativa, Prazo expirado, Vendido, Devolvida, " +
                        "Cancelada)."
                },
                new()
                {
                    Seletor = "veiculo-detalhe-especificacoes",
                    Titulo = "Especificações técnicas",
                    Texto = "Ficha técnica completa do veículo — ano, quilometragem, cor, combustível, câmbio, " +
                        "motorização e placa."
                },
                new()
                {
                    Seletor = "veiculo-detalhe-acao-principal",
                    Titulo = "Ação principal (veículo próprio)",
                    Texto = "Com o veículo Disponível, este botão abre o formulário de nova proposta de venda. " +
                        "Enquanto o veículo está Em preparação, o mesmo espaço mostra \"Liberar para venda\" — " +
                        "ação restrita a Administração e Gerência de Vendas."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Outras ações (veículo próprio)",
                    Texto = "Logo abaixo da ação principal: agendar test drive, editar dados/fotos, e — só pra " +
                        "Admin e Gerente de Vendas — voltar um veículo Disponível pra preparação ou publicá-lo no " +
                        "Mercado Livre. \"Excluir veículo\" é destrutivo e não pode ser desfeito."
                },
                new()
                {
                    Seletor = "consig-detalhe-info",
                    Titulo = "Consignação — proprietário e prazo",
                    Texto = "Mostra o proprietário do veículo e o vendedor responsável — trocável a qualquer " +
                        "momento enquanto a consignação está Ativa ou Expirada, pelo link \"Trocar\" ao lado do " +
                        "nome — além das datas de início e vencimento. Um aviso no topo da página avisa quando " +
                        "faltam poucos dias ou o prazo já venceu."
                },
                new()
                {
                    Seletor = "consig-detalhe-comissao",
                    Titulo = "Consignação — comissão",
                    Texto = "Mostra o valor de venda esperado combinado com o proprietário e como a comissão foi " +
                        "definida — um valor fixo a repassar, ou uma porcentagem do valor de venda."
                },
                new()
                {
                    Seletor = "consig-detalhe-renovar-btn",
                    Titulo = "Renovar consignação",
                    Texto = "Estende o prazo da consignação por mais 90 dias a partir de hoje. Fica visível " +
                        "apenas para Administração e Gerência de Vendas, e pede confirmação de senha antes de " +
                        "aplicar."
                },
                new()
                {
                    Seletor = "consig-detalhe-gerar-proposta-btn",
                    Titulo = "Gerar proposta (consignado)",
                    Texto = "Abre o mesmo formulário de proposta de venda usado para veículos próprios. Quando " +
                        "essa proposta for aprovada, a consignação passa a ficar marcada como vendida; quando o " +
                        "termo de entrega for assinado, ela é concluída."
                },
                new()
                {
                    Seletor = "consig-detalhe-devolver-btn",
                    Titulo = "Devolver ao proprietário",
                    Texto = "Encerra a consignação sem venda e devolve o veículo ao proprietário. Use quando o " +
                        "prazo se esgotou e não há mais interesse em manter o veículo na loja."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Depois da venda, e cancelamento",
                    Texto = "Quando a proposta gerada aqui é aprovada, a consignação vira \"Vendido (aguard. " +
                        "pagamento)\" — \"Confirmar pagamento e concluir\" fecha o ciclo só depois que o valor " +
                        "entrou. \"Cancelar consignação\" (Admin/Gerência) pede um motivo obrigatório e fica " +
                        "disponível enquanto o contrato está Ativo ou Expirado."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Próximo passo",
                    Texto = "Gerar uma proposta a partir daqui leva ao formulário de nova proposta, já com o " +
                        "veículo pré-selecionado. Dali em diante, o acompanhamento continua na tela de detalhe " +
                        "da proposta."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/concessionaria/propostas",
            Titulo = "Propostas de Venda",
            Descricao = "Lista de todas as propostas de venda, com filtro por etapa do fluxo.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Todas as propostas",
                    Texto = "Esta tela lista as propostas de venda de veículos próprios e consignados. Uma " +
                        "proposta nasce quando alguém gera uma proposta a partir da tela de detalhe de um " +
                        "veículo, e passa por várias etapas até ser concluída ou encerrada."
                },
                new()
                {
                    Seletor = "prop-lista-header",
                    Titulo = "Total e pendências",
                    Texto = "O resumo no topo mostra quantas propostas existem ao todo e quantas precisam de " +
                        "alguma ação sua neste momento."
                },
                new()
                {
                    Seletor = "prop-lista-busca",
                    Titulo = "Busca",
                    Texto = "Busque por nome do cliente, marca ou modelo do veículo, ou pelo início do número da " +
                        "proposta."
                },
                new()
                {
                    Seletor = "prop-lista-atencao-banner",
                    Titulo = "Aguardando ação",
                    Texto = "Aparece quando existe proposta com resposta da financiadora recebida, aprovada " +
                        "aguardando início de vistoria, ou com vistoria concluída aguardando o termo. Clique em " +
                        "um item para ir direto à proposta."
                },
                new()
                {
                    Seletor = "prop-lista-filtros",
                    Titulo = "Filtro por etapa",
                    Texto = "Filtre pela etapa exata do fluxo — desde \"Ativas\" (recém-criadas) até " +
                        "\"Concluídas\", passando por financiamento, vistoria e assinatura do termo."
                },
                new()
                {
                    Seletor = "prop-lista-row-exemplo",
                    Titulo = "Linha de uma proposta",
                    Texto = "Cada linha mostra cliente, veículo, modo de pagamento, valor final e status. Uma " +
                        "proposta ainda não aprovada expira automaticamente 7 dias após a criação se ninguém " +
                        "agir — o prazo restante aparece abaixo da data de criação."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Abrindo uma proposta",
                    Texto = "Clique em qualquer linha para abrir o detalhe completo — é lá que o fluxo avança, " +
                        "da definição do modo de pagamento até a entrega do veículo."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/concessionaria/proposta/nova/{veiculoid:guid}",
            Titulo = "Nova Proposta (Veículo Próprio)",
            Descricao = "Criação de uma proposta de venda para um veículo do estoque da loja.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Nova proposta de venda",
                    Texto = "Este formulário registra a proposta de compra de um veículo próprio da loja. Ele " +
                        "abre com o veículo já selecionado, vindo da tela de detalhe de onde você clicou."
                },
                new()
                {
                    Seletor = "os-cliente-secao",
                    Titulo = "Dados do cliente",
                    Texto = "Busque um cliente já cadastrado pelo nome ou CPF, ou cadastre um novo aqui mesmo — " +
                        "é o comprador do veículo. O CPF é validado pelo dígito verificador oficial."
                },
                new()
                {
                    Seletor = "proposta-nova-veiculo-resumo",
                    Titulo = "Veículo em negociação",
                    Texto = "Resumo somente leitura do veículo escolhido, para conferência antes de prosseguir."
                },
                new()
                {
                    Seletor = "proposta-nova-valores",
                    Titulo = "Valores e desconto",
                    Texto = "O valor base vem pré-preenchido com o preço anunciado do veículo, mas pode ser " +
                        "ajustado. O desconto é percentual (0 a 100) e o valor final é recalculado " +
                        "automaticamente conforme você digita."
                },
                new()
                {
                    Seletor = "proposta-nova-modo-pagamento",
                    Titulo = "Modo de pagamento",
                    Texto = "A compra de um veículo não aceita pagamento em dinheiro — apenas Pix, " +
                        "financiamento, boleto ou transferência. Essa escolha fica registrada na proposta e não " +
                        "pode ser alterada depois de criada; mudar de ideia exige cancelar e criar uma nova."
                },
                new()
                {
                    Seletor = "proposta-nova-financ-bloco",
                    Titulo = "Detalhes do financiamento",
                    Texto = "Aparece só quando o modo é Financiamento. Informe a entrada — o valor a financiar é " +
                        "calculado automaticamente — e marque a opção para já marcar como enviado à financiadora " +
                        "assim que a proposta for registrada (o contato em si é feito por fora do sistema)."
                },
                new()
                {
                    Seletor = "proposta-nova-salvar-btn",
                    Titulo = "Registrar proposta",
                    Texto = "Cliente, valor base e modo de pagamento são obrigatórios. Ao confirmar, a proposta " +
                        "nasce com status \"Criada\" e você é levado direto para o detalhe dela."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Próxima etapa",
                    Texto = "A partir daqui a proposta segue seu próprio fluxo: aprovação do cliente, vistoria " +
                        "do veículo e assinatura do termo de entrega — tudo acompanhado na tela de detalhe da " +
                        "proposta."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/concessionaria/proposta/nova-consignado/{veiculoid:guid}",
            Titulo = "Nova Proposta (Veículo Consignado)",
            Descricao = "Criação de uma proposta de venda para um veículo consignado.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Nova proposta — consignação",
                    Texto = "Mesmo formulário de proposta usado para veículos próprios, mas aplicado a um " +
                        "veículo consignado. O valor base já vem preenchido com o valor de venda esperado " +
                        "combinado com o proprietário."
                },
                new()
                {
                    Seletor = "proposta-nova-aviso-consignado",
                    Titulo = "Aviso de consignação",
                    Texto = "Identifica o proprietário do veículo e lembra que a consignação será marcada como " +
                        "vendida quando esta proposta for aprovada, e concluída quando o termo de entrega for " +
                        "assinado."
                },
                new()
                {
                    Seletor = "os-cliente-secao",
                    Titulo = "Dados do cliente",
                    Texto = "Busque um cliente já cadastrado pelo nome ou CPF, ou cadastre um novo aqui mesmo — " +
                        "é o comprador do veículo, não o proprietário consignante."
                },
                new()
                {
                    Seletor = "proposta-nova-veiculo-resumo",
                    Titulo = "Veículo em negociação",
                    Texto = "Resumo somente leitura do veículo consignado escolhido, para conferência antes de " +
                        "prosseguir."
                },
                new()
                {
                    Seletor = "proposta-nova-valores",
                    Titulo = "Valores e desconto",
                    Texto = "O valor base parte do valor de venda esperado cadastrado na consignação. Um " +
                        "desconto aplicado aqui reduz o valor final da venda — negocie com atenção, já que o " +
                        "valor combinado com o proprietário foi definido na consignação."
                },
                new()
                {
                    Seletor = "proposta-nova-modo-pagamento",
                    Titulo = "Modo de pagamento",
                    Texto = "As mesmas regras do veículo próprio se aplicam aqui: sem pagamento em dinheiro, " +
                        "apenas Pix, financiamento, boleto ou transferência, definidos uma única vez na criação."
                },
                new()
                {
                    Seletor = "proposta-nova-financ-bloco",
                    Titulo = "Detalhes do financiamento",
                    Texto = "Aparece só quando o modo é Financiamento, com entrada e valor a financiar " +
                        "calculados do mesmo jeito que na venda de um veículo próprio."
                },
                new()
                {
                    Seletor = "proposta-nova-salvar-btn",
                    Titulo = "Registrar proposta",
                    Texto = "Cliente, valor base e modo de pagamento são obrigatórios. Ao confirmar, a proposta " +
                        "nasce vinculada à consignação, e você é levado direto para o detalhe dela."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Próxima etapa",
                    Texto = "Acompanhe a aprovação, a vistoria e o termo de entrega na tela de detalhe da " +
                        "proposta. O status da consignação, na tela do veículo, muda automaticamente conforme a " +
                        "proposta avança."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/concessionaria/proposta/{id:guid}",
            Titulo = "Detalhe da Proposta",
            Descricao = "Acompanhamento do fluxo completo de uma proposta, da aprovação à entrega.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Detalhe da proposta",
                    Texto = "Esta tela concentra todo o ciclo de vida de uma proposta de venda: aprovação, " +
                        "financiamento, vistoria do veículo e termo de entrega. As seções disponíveis mudam de " +
                        "acordo com a etapa atual."
                },
                new()
                {
                    Seletor = "proposta-detalhe-status",
                    Titulo = "Status e validade",
                    Texto = "O selo mostra a etapa atual. Enquanto a proposta ainda não foi aprovada, ela expira " +
                        "automaticamente 7 dias após a criação se ninguém agir — o prazo restante aparece ao " +
                        "lado do selo."
                },
                new()
                {
                    Seletor = "proposta-detalhe-cliente-veiculo",
                    Titulo = "Cliente e veículo",
                    Texto = "Dados de contato do comprador e a ficha resumida do veículo em negociação, próprio " +
                        "ou consignado."
                },
                new()
                {
                    Seletor = "proposta-detalhe-valores",
                    Titulo = "Valores da venda",
                    Texto = "Valor base, desconto aplicado, valor final e entrada. Enquanto a proposta ainda " +
                        "está em andamento, dá para atualizar o valor da entrada diretamente aqui."
                },
                new()
                {
                    Seletor = "proposta-detalhe-modo-pagamento",
                    Titulo = "Modo de pagamento",
                    Texto = "Definido na criação da proposta e travado depois disso — trocar de modo exige " +
                        "cancelar esta proposta e criar uma nova."
                },
                new()
                {
                    Seletor = "proposta-detalhe-financiamento",
                    Titulo = "Financiamento",
                    Texto = "O sistema não simula financiamento — entre em contato com a financiadora por fora " +
                        "(telefone, e-mail, portal do parceiro) e anote aqui o que ela propôs em texto livre, ou " +
                        "registre a negativa se ela recusar. Essa seção só aparece quando o modo de pagamento é " +
                        "Financiamento."
                },
                new()
                {
                    Seletor = "proposta-detalhe-vistoria",
                    Titulo = "Vistoria do veículo",
                    Texto = "Depois que o cliente aprova a proposta, inicie a vistoria, anexe fotos do estado do " +
                        "veículo e registre o resultado. Uma vistoria reprovada permite iniciar uma nova, sem " +
                        "travar o fluxo."
                },
                new()
                {
                    Seletor = "proposta-detalhe-pagamento",
                    Titulo = "Cobrança do veículo",
                    Texto = "Painel de recebimento do valor da venda, visível a partir da aprovação. Some os " +
                        "pagamentos registrados aqui para acompanhar quanto falta receber do cliente."
                },
                new()
                {
                    Seletor = "proposta-detalhe-termo",
                    Titulo = "Termo de entrega",
                    Texto = "Redija o termo depois da vistoria concluída, envie para assinatura do cliente e " +
                        "compartilhe o link gerado — a assinatura acontece numa página pública específica, sem " +
                        "necessidade de login do cliente."
                },
                new()
                {
                    Seletor = "proposta-detalhe-acoes",
                    Titulo = "Aprovar, rejeitar ou cancelar",
                    Texto = "Só aparece enquanto a proposta está em andamento (antes da aprovação). Rejeitar e " +
                        "cancelar exigem um motivo, que fica registrado na proposta."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Fim do fluxo",
                    Texto = "Quando o termo é assinado, a proposta é concluída — e, se o veículo era " +
                        "consignado, a consignação correspondente também é concluída automaticamente. A " +
                        "proposta permanece no histórico da tela de Propostas."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/concessionaria/assinar/{token}",
            Titulo = "Assinatura do termo de entrega",
            Descricao = "Página pública de assinatura eletrônica do termo de entrega do veículo.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Página do cliente, sem login",
                    Texto = "Acessada pelo link gerado na aba \"Termo de entrega\" da proposta — sem menu, sem " +
                        "autenticação, feita só pra este formulário. Se o link já foi usado, a página mostra o " +
                        "termo como assinado em vez do formulário."
                },
                new()
                {
                    Seletor = "assinar-texto-termo",
                    Titulo = "Conteúdo do termo",
                    Texto = "Texto redigido pelo vendedor na proposta, descrevendo as condições de entrega do " +
                        "veículo. Peça pro cliente ler com atenção — o conteúdo não pode ser editado por aqui."
                },
                new()
                {
                    Seletor = "assinar-form",
                    Titulo = "Assinatura eletrônica",
                    Texto = "Nome, CPF e o aceite explícito registram a assinatura eletrônica simples (Lei " +
                        "14.063/2020) — data/hora e IP são capturados automaticamente no momento da confirmação, " +
                        "sem precisar digitar nada além disso."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/concessionaria/relatorios",
            Titulo = "Relatórios — Concessionária",
            Descricao = "Estoque de veículos, propostas, test drives e consignações, em CSV ou XML por período.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Relatórios exportáveis",
                    Texto = "Cada cartão gera um arquivo para baixar, só com dados da concessionária. Os " +
                        "relatórios financeiros e comparativos (margem de venda, comissões) só aparecem para " +
                        "Admin e Gerente de vendas — um Vendedor vê só os relatórios operacionais."
                },
                new()
                {
                    Seletor = "rel-filtros",
                    Titulo = "Período e formato",
                    Texto = "Escolha a janela de tempo e o formato (CSV abre no Excel/LibreOffice; XML para " +
                        "integração). Valem para todos os relatórios da tela."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/concessionaria/documentos",
            Titulo = "Documentos",
            Descricao = "Busca de termos e contratos já produzidos pelo sistema, com exportação em PDF.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Central de Documentos",
                    Texto = "Reúne termo de entrega, termo de test drive e contrato de consignação num só " +
                        "lugar — visível só para Admin e Gerente de vendas, já que envolvem dados pessoais " +
                        "do cliente. Edição continua na proposta, no test drive ou no veículo consignado; " +
                        "aqui é só busca, visualização e exportação."
                },
                new()
                {
                    Seletor = "doc-filtros",
                    Titulo = "Tipo e busca",
                    Texto = "Escolha o tipo de documento e busque por placa, nome do cliente ou nome do " +
                        "vendedor — o filtro já aplica enquanto você digita."
                },
                new()
                {
                    Seletor = "doc-tabela",
                    Titulo = "Ver e exportar",
                    Texto = "Clique numa linha para abrir o texto do documento (e as fotos anexadas, quando " +
                        "houver — termo de entrega e contrato de consignação podem ter). O botão \"Gerar PDF\" " +
                        "abre o diálogo de impressão do navegador; escolha \"Salvar como PDF\"."
                }
            }
        }
    };
}
