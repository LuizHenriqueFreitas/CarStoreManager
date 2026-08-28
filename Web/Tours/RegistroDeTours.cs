namespace CarStoreManager.Web.Tours;

/*
    Por que os textos dos tours ficam aqui em código, e não no banco:

    Cada passo aponta pra um elemento específico do markup via data-tour. Se o
    markup mudar (um campo é removido, uma seção é reorganizada) e o texto do
    passo estivesse no banco, o tour quebraria silenciosamente — ninguém
    percebe até abrir o tour na apresentação e ver um passo travado ou um
    destaque apontando pro lugar errado. Com os dois no mesmo repositório
    (data-tour no .razor, texto do passo aqui), eles aparecem juntos no mesmo
    diff quando alguém mexe na tela, e não é preciso migration nem seed pra
    popular nada. Também elimina uma consulta ao banco toda vez que o usuário
    abre um tour.

    O conteúdo é dividido em um arquivo por módulo (RegistroDeTours.*.cs, todos
    "partial class" da mesma classe) em vez de um #region só neste arquivo —
    com ~23 telas de até 12 passos cada, um arquivo único ficaria com milhares
    de linhas mesmo com #region, exatamente a "parede ilegível" que se queria
    evitar. Continua sendo a mesma classe RegistroDeTours; só o texto de cada
    módulo mora em seu próprio arquivo.
*/

/// <summary>
/// Registro estático de todos os tours guiados do sistema — uma entrada por
/// tela roteável. <see cref="ObterPorRota"/> localiza o tour da rota atual
/// comparando a ESTRUTURA da URL contra o template de cada tour registrado.
/// </summary>
public static partial class RegistroDeTours
{
    private static readonly List<TourDaPagina> Tours = new();

    static RegistroDeTours()
    {
        Tours.AddRange(TodosAutenticacaoEAcessoPublico());
        Tours.AddRange(TodosConcessionaria());
        Tours.AddRange(TodosOficina());
        Tours.AddRange(TodosAdministracao());
        Tours.AddRange(TodosIntegracoes());
    }

    /// <summary>
    /// Localiza o tour da rota atual. Compara a URL informada (ex.:
    /// <c>/oficina/os/3a51433b-...</c>) segmento a segmento contra o
    /// RotaTemplate de cada tour registrado (ex.: <c>/oficina/os/{id}</c>) —
    /// um segmento do template entre chaves casa com qualquer valor na mesma
    /// posição, o resto precisa ser idêntico (sem diferenciar maiúsculas).
    ///
    /// Essa comparação estrutural foi escolhida em vez de pré-normalizar a
    /// URL recebida (trocar cada segmento que "parece" GUID/número por um
    /// coringa antes de procurar num dicionário) porque nem todo parâmetro
    /// tem uma forma reconhecível por regex — o {Token} de assinatura de
    /// termo e o {Codigo} de consulta pública são strings opacas, indistintas
    /// de um segmento literal como "novo" só pela forma. Comparar contra os
    /// templates já registrados resolve os dois casos com uma lógica só, sem
    /// precisar adivinhar o tipo de cada segmento.
    /// </summary>
    public static TourDaPagina? ObterPorRota(string caminhoAtual)
    {
        var segmentosAtuais = Segmentos(caminhoAtual);

        foreach (var tour in Tours)
        {
            var segmentosTemplate = Segmentos(tour.RotaTemplate);
            if (segmentosTemplate.Length != segmentosAtuais.Length) continue;

            var bateu = true;
            for (var i = 0; i < segmentosTemplate.Length; i++)
            {
                var seg = segmentosTemplate[i];
                var ehParametro = seg.StartsWith('{') && seg.EndsWith('}');
                if (ehParametro) continue;

                if (!string.Equals(seg, segmentosAtuais[i], StringComparison.OrdinalIgnoreCase))
                {
                    bateu = false;
                    break;
                }
            }

            if (bateu) return tour;
        }

        return null;
    }

    private static string[] Segmentos(string rota) =>
        rota.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
}
