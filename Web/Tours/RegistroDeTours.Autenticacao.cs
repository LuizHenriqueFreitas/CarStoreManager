namespace CarStoreManager.Web.Tours;

public static partial class RegistroDeTours
{
    private static List<TourDaPagina> TodosAutenticacaoEAcessoPublico()
    {
        // /consulta e /consulta/{Codigo} são tratadas como um único tour — a
        // segunda rota só existe pra permitir compartilhar um link direto com
        // o código já preenchido, a tela e o fluxo são os mesmos. Como o
        // casamento de rota em ObterPorRota compara por template exato
        // (mesma quantidade de segmentos), registramos as duas rotas
        // apontando pra essa mesma lista de passos.
        var passosConsulta = new List<PassoTour>
        {
            new()
            {
                Seletor = null,
                Titulo = "Consulta pública de OS",
                Texto = "Esta tela permite que qualquer cliente acompanhe o andamento do serviço do seu " +
                    "veículo sem precisar de login — basta o código que a oficina informou na abertura da " +
                    "ordem."
            },
            new()
            {
                Seletor = "consulta-busca",
                Titulo = "Código da ordem",
                Texto = "Digite o código de 8 caracteres recebido da oficina (ex.: A1B2C3D4) e clique em " +
                    "\"Consultar\". O mesmo código também funciona direto na URL, em /consulta/SEUCODIGO."
            },
            new()
            {
                Seletor = "consulta-trilha",
                Titulo = "Andamento do serviço",
                Texto = "Uma versão simplificada da trilha usada internamente pela oficina, mostrando em qual " +
                    "das seis etapas o serviço está agora."
            },
            new()
            {
                Seletor = "consulta-info",
                Titulo = "Informações do serviço",
                Texto = "Tipo de serviço, data de abertura, descrição do problema relatado, prazo estimado e " +
                    "valor total — a mesma visão que a recepção usa, mas sem os campos internos."
            },
            new()
            {
                Seletor = "consulta-checklist",
                Titulo = "Etapas técnicas",
                Texto = "Se a oficina já definiu um checklist para o serviço, cada item aparece aqui com seu " +
                    "progresso — dá pra acompanhar exatamente o que já foi feito no veículo."
            },
            new()
            {
                Seletor = null,
                Titulo = "Sem cadastro necessário",
                Texto = "Esta consulta não exige login. Para acessar as telas internas do sistema, use o link " +
                    "\"Acesso interno\" no topo da página."
            }
        };

        return new()
        {
            new TourDaPagina
            {
                RotaTemplate = "/login",
                Titulo = "Entrar no sistema",
                Descricao = "Autenticação de funcionários no sistema interno.",
                Passos = new List<PassoTour>
                {
                    new()
                    {
                        Seletor = null,
                        Titulo = "Acesso interno",
                        Texto = "Esta tela autentica os funcionários da loja e da oficina. Cada papel " +
                            "(Administração, Vendas, Oficina, Recepção) tem acesso só às telas relevantes " +
                            "para sua função."
                    },
                    new()
                    {
                        Seletor = "login-form",
                        Titulo = "E-mail e senha",
                        Texto = "Informe o e-mail e a senha cadastrados pela Administração. Após autenticar, " +
                            "você é levado direto para a tela inicial do seu papel — por exemplo, a Oficina " +
                            "abre para mecânicos e a Concessionária abre para vendedores."
                    },
                    new()
                    {
                        Seletor = "login-rodape",
                        Titulo = "É cliente da loja?",
                        Texto = "Clientes não fazem login aqui. O link \"Consultar ordem de serviço\" leva à " +
                            "consulta pública, onde o andamento do serviço é acompanhado só com o código " +
                            "recebido da oficina."
                    },
                    new()
                    {
                        Seletor = null,
                        Titulo = "Esqueceu a senha?",
                        Texto = "Esta tela não tem recuperação de senha automática — a troca de senha exige " +
                            "informar a senha atual, na tela de Configurações, já autenticado. Se esqueceu a " +
                            "senha, procure a Administração."
                    }
                }
            },

            new TourDaPagina
            {
                RotaTemplate = "/consulta",
                Titulo = "Consultar Ordem de Serviço",
                Descricao = "Consulta pública do andamento de uma ordem de serviço, sem necessidade de login.",
                Passos = passosConsulta
            },

            new TourDaPagina
            {
                RotaTemplate = "/consulta/{codigo}",
                Titulo = "Consultar Ordem de Serviço",
                Descricao = "Consulta pública do andamento de uma ordem de serviço, sem necessidade de login.",
                Passos = passosConsulta
            },

            new TourDaPagina
            {
                RotaTemplate = "/concessionaria/assinar/{token}",
                Titulo = "Assinatura do Termo de Entrega",
                Descricao = "Assinatura eletrônica pública do termo de entrega do veículo, sem necessidade de login.",
                Passos = new List<PassoTour>
                {
                    new()
                    {
                        Seletor = null,
                        Titulo = "Termo de entrega",
                        Texto = "Este link é enviado ao cliente quando o veículo está pronto para entrega. Ele " +
                            "não exige login — o acesso é feito só pelo link, que é único para cada proposta " +
                            "de venda."
                    },
                    new()
                    {
                        Seletor = "assinar-texto-termo",
                        Titulo = "Leia o termo",
                        Texto = "O texto completo do termo de entrega, gerado a partir dos dados da proposta " +
                            "de venda aprovada. Leia com atenção antes de assinar."
                    },
                    new()
                    {
                        Seletor = "assinar-form",
                        Titulo = "Assinatura eletrônica",
                        Texto = "Informe nome completo e CPF exatamente como constam no documento, marque o " +
                            "aceite e confirme. Pela Lei 14.063/2020, essa assinatura eletrônica simples tem " +
                            "validade jurídica — a data, hora e endereço IP da assinatura ficam registrados " +
                            "junto ao termo."
                    },
                    new()
                    {
                        Seletor = "assinar-btn",
                        Titulo = "Confirmar assinatura",
                        Texto = "O botão só é liberado depois que nome, CPF e aceite estiverem preenchidos. " +
                            "Depois de assinado, o termo não pode ser assinado novamente — a tela passa a " +
                            "mostrar só a confirmação."
                    },
                    new()
                    {
                        Seletor = null,
                        Titulo = "Depois de assinar",
                        Texto = "Com o termo assinado, a entrega do veículo está formalizada. Qualquer dúvida " +
                            "sobre o processo deve ser tratada diretamente com a loja."
                    }
                }
            }
        };
    }
}
