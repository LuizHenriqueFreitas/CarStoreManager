namespace CarStoreManager.Web.Tours;

public static partial class RegistroDeTours
{
    private static List<TourDaPagina> TodosIntegracoes() => new()
    {
        new TourDaPagina
        {
            RotaTemplate = "/integracoes/mercadolivre",
            Titulo = "Integração — Mercado Livre",
            Descricao = "Conexão da loja com a conta do Mercado Livre.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Publicando no Mercado Livre",
                    Texto = "Esta tela conecta a loja a uma conta do Mercado Livre. Com a conexão ativa, " +
                        "veículos, consignações e componentes podem ser publicados como anúncios a partir " +
                        "das próprias telas de detalhe, e o sistema sincroniza preço e status com o anúncio. " +
                        "Só o administrador acessa esta tela."
                },
                new()
                {
                    Seletor = "ml-link-anuncios",
                    Titulo = "Anúncios publicados",
                    Texto = "Atalho para a tela que lista tudo o que já foi publicado e permite pausar ou " +
                        "encerrar cada anúncio."
                },
                new()
                {
                    Seletor = "ml-status-badge",
                    Titulo = "Status da conexão",
                    Texto = "Mostra se a loja está conectada, qual conta do Mercado Livre está associada e " +
                        "quando o token de acesso expira. O sistema renova esse token automaticamente " +
                        "enquanto a conexão estiver ativa."
                },
                new()
                {
                    Seletor = "ml-modo-operacao",
                    Titulo = "Ambiente da integração",
                    Texto = "Indica se a integração está apontando para o ambiente de testes (sandbox) ou " +
                        "para produção. Essa configuração vem do appsettings.json — trocar de ambiente exige " +
                        "reiniciar a aplicação, não é ajustável por aqui."
                },
                new()
                {
                    Seletor = "ml-btn-conectar",
                    Titulo = "Conectar a loja",
                    Texto = "Redireciona para a página de autorização do Mercado Livre. Depois de aprovar o " +
                        "acesso lá, você volta para esta tela já conectado."
                },
                new()
                {
                    Seletor = "ml-btn-desconectar",
                    Titulo = "Desconectar",
                    Texto = "Remove a autorização armazenada. Os anúncios já publicados continuam ativos no " +
                        "Mercado Livre, mas o sistema deixa de conseguir sincronizar preço, pausar ou encerrar " +
                        "esses anúncios até que a conexão seja refeita."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Depois de conectar",
                    Texto = "Com a loja conectada, publique um veículo, uma consignação ou um componente pela " +
                        "própria tela de detalhe de cada um — o botão \"Publicar no Mercado Livre\" fica " +
                        "disponível ali, e o anúncio aparece na tela de gerenciamento de anúncios."
                }
            }
        },

        new TourDaPagina
        {
            RotaTemplate = "/integracoes/mercadolivre/anuncios",
            Titulo = "Anúncios no Mercado Livre",
            Descricao = "Acompanhamento dos anúncios já publicados na conta conectada.",
            Passos = new List<PassoTour>
            {
                new()
                {
                    Seletor = null,
                    Titulo = "Anúncios publicados",
                    Texto = "Esta tela lista tudo o que já foi publicado no Mercado Livre — veículos, " +
                        "consignações e componentes. Ela não cria anúncios novos: a publicação parte da tela " +
                        "de detalhe de cada item."
                },
                new()
                {
                    Seletor = "ml-anuncios-btn-status",
                    Titulo = "Voltar ao status da conexão",
                    Texto = "Atalho para a tela de status, caso a conexão precise ser verificada ou refeita."
                },
                new()
                {
                    Seletor = "ml-anuncios-lista",
                    Titulo = "Lista de anúncios",
                    Texto = "Um item por anúncio, com o selo de status: Publicado, Pausado, Encerrado ou " +
                        "Erro de publicação."
                },
                new()
                {
                    Seletor = "ml-anuncios-card-exemplo",
                    Titulo = "Card de um anúncio",
                    Texto = "Mostra o identificador do item no Mercado Livre, o último preço sincronizado e a " +
                        "data da última sincronização. Se a última tentativa de sincronizar falhou, o motivo " +
                        "do erro aparece destacado no próprio card."
                },
                new()
                {
                    Seletor = "ml-anuncios-btn-pausar",
                    Titulo = "Pausar um anúncio",
                    Texto = "Oculta o anúncio temporariamente da vitrine do Mercado Livre, sem apagar o " +
                        "registro. Para reativar, publique o mesmo item novamente pela tela de detalhe dele."
                },
                new()
                {
                    Seletor = "ml-anuncios-btn-encerrar",
                    Titulo = "Encerrar um anúncio",
                    Texto = "Remove o anúncio de vez do Mercado Livre. Assim como o pausado, um anúncio " +
                        "encerrado pode ser publicado outra vez do zero pela tela de detalhe do item, se " +
                        "necessário."
                },
                new()
                {
                    Seletor = null,
                    Titulo = "Fluxo completo",
                    Texto = "Publicar, pausar e encerrar sempre partem da tela de detalhe do veículo, " +
                        "consignação ou componente — esta tela é só o painel de acompanhamento de tudo o que " +
                        "já foi publicado."
                }
            }
        }
    };
}
