namespace CarStoreManager.Geradores.Dados;

/// <summary>
/// Pools de dados brasileiros reaproveitáveis por qualquer gerador de
/// entidade: nomes, cidades, endereços, marcas/modelos de veículo, peças de
/// oficina. Mantidos aqui (e não dentro de cada gerador) para que futuras
/// entidades possam reusar as mesmas listas sem duplicar dados.
/// </summary>
public static class NomesPt
{
    public static readonly string[] PrimeirosNomesMasculinos =
    {
        "João", "Pedro", "Lucas", "Gabriel", "Matheus", "Rafael", "Gustavo", "Felipe", "Bruno", "Rodrigo",
        "Carlos", "Marcos", "Paulo", "Ricardo", "Eduardo", "Fernando", "Diego", "Thiago", "André", "Leonardo",
        "Vinícius", "Henrique", "Daniel", "Alexandre", "Fábio", "Márcio", "Roberto", "Sérgio", "Antônio", "José",
        "Caio", "Igor", "Renato", "Vitor", "Guilherme", "Rogério", "Emerson", "Fabrício", "Wesley", "Anderson"
    };

    public static readonly string[] PrimeirosNomesFemininos =
    {
        "Maria", "Ana", "Juliana", "Fernanda", "Camila", "Beatriz", "Larissa", "Amanda", "Patrícia", "Bruna",
        "Carla", "Renata", "Vanessa", "Aline", "Priscila", "Débora", "Letícia", "Gabriela", "Mariana", "Tatiane",
        "Sandra", "Simone", "Cristina", "Adriana", "Luciana", "Daniela", "Raquel", "Natália", "Isabela", "Vitória",
        "Bianca", "Jéssica", "Michele", "Rosana", "Elaine", "Viviane", "Karina", "Cíntia", "Regiane", "Sabrina"
    };

    public static readonly string[] Sobrenomes =
    {
        "Silva", "Santos", "Oliveira", "Souza", "Rodrigues", "Ferreira", "Alves", "Pereira", "Lima", "Gomes",
        "Costa", "Ribeiro", "Martins", "Carvalho", "Almeida", "Lopes", "Soares", "Fernandes", "Vieira", "Barbosa",
        "Rocha", "Dias", "Nascimento", "Andrade", "Moreira", "Nunes", "Marques", "Machado", "Mendes", "Freitas",
        "Cardoso", "Teixeira", "Correia", "Cavalcanti", "Monteiro", "Pinto", "Ramos", "Araújo", "Castro", "Campos"
    };

    public static readonly string[] Cidades =
    {
        "São Paulo", "Rio de Janeiro", "Belo Horizonte", "Curitiba", "Porto Alegre", "Salvador", "Recife",
        "Fortaleza", "Brasília", "Campinas", "Santos", "Uberlândia", "Ribeirão Preto", "Sorocaba", "Joinville",
        "Londrina", "Niterói", "Florianópolis", "Goiânia", "Vitória"
    };

    public static readonly string[] UfsPorCidade =
    {
        "SP", "RJ", "MG", "PR", "RS", "BA", "PE",
        "CE", "DF", "SP", "SP", "MG", "SP", "SP", "SC",
        "PR", "RJ", "SC", "GO", "ES"
    };

    public static readonly string[] Bairros =
    {
        "Centro", "Jardim América", "Vila Nova", "Boa Vista", "Santa Mônica", "Cidade Alta", "Bela Vista",
        "São José", "Vila Industrial", "Jardim Europa", "Parque das Flores", "Alto da Boa Vista", "Cascatinha",
        "Vila Esperança", "Jardim Primavera"
    };

    public static readonly string[] Logradouros =
    {
        "Rua das Flores", "Avenida Brasil", "Rua Sete de Setembro", "Rua XV de Novembro", "Avenida Paulista",
        "Rua São João", "Rua Rio Branco", "Avenida Independência", "Rua Marechal Deodoro", "Rua Tiradentes",
        "Avenida Getúlio Vargas", "Rua Dom Pedro II", "Rua das Palmeiras", "Rua dos Andradas", "Avenida Central"
    };

    // marca -> modelos plausíveis, usado tanto em VeiculoVenda/Consignacao quanto VeiculoCliente
    public static readonly (string Marca, string[] Modelos)[] MarcasEModelos =
    {
        ("Chevrolet", new[] { "Onix", "Onix Plus", "Tracker", "Spin", "S10", "Cruze" }),
        ("Volkswagen", new[] { "Gol", "Polo", "Virtus", "T-Cross", "Nivus", "Saveiro" }),
        ("Fiat", new[] { "Argo", "Mobi", "Pulse", "Strada", "Cronos", "Toro" }),
        ("Hyundai", new[] { "HB20", "Creta", "Tucson", "HB20S" }),
        ("Toyota", new[] { "Corolla", "Yaris", "Hilux", "Corolla Cross" }),
        ("Honda", new[] { "Civic", "HR-V", "City", "Fit" }),
        ("Renault", new[] { "Kwid", "Sandero", "Duster", "Logan" }),
        ("Jeep", new[] { "Renegade", "Compass", "Commander" }),
        ("Nissan", new[] { "Kicks", "Versa", "Frontier" }),
        ("Ford", new[] { "Ka", "EcoSport", "Ranger" })
    };

    public static readonly string[] Cores =
    {
        "Branco", "Prata", "Preto", "Cinza", "Vermelho", "Azul", "Verde"
    };

    public static readonly string[] Motorizacoes = { "1.0", "1.3", "1.4", "1.6", "1.8", "2.0" };

    // MarcaFabricante de peças de oficina (fornecedores/fabricantes reais do setor automotivo)
    public static readonly string[] MarcasComponentes =
    {
        "Bosch", "NGK", "Cofap", "Nakata", "Fras-le", "TRW", "Mahle", "Wega", "Sabo", "Continental",
        "Delphi", "Varga", "Monroe", "Magneti Marelli"
    };

    // nome base de peça por sistema — combinado com SistemaComponente para variar o "Nome"
    public static readonly string[] NomesPecaGenericos =
    {
        "Filtro de óleo", "Filtro de ar", "Filtro de combustível", "Pastilha de freio", "Disco de freio",
        "Amortecedor", "Vela de ignição", "Correia dentada", "Bomba de combustível", "Radiador",
        "Bateria", "Alternador", "Sensor de oxigênio", "Kit embreagem", "Rolamento de roda",
        "Terminal de direção", "Bieleta", "Mangueira do radiador", "Compressor de ar-condicionado", "Farol"
    };

    public static string NomeCompletoAleatorio(Random rng)
    {
        var masculino = rng.Next(2) == 0;
        var primeiro = masculino
            ? PrimeirosNomesMasculinos[rng.Next(PrimeirosNomesMasculinos.Length)]
            : PrimeirosNomesFemininos[rng.Next(PrimeirosNomesFemininos.Length)];
        var sobrenome1 = Sobrenomes[rng.Next(Sobrenomes.Length)];
        var sobrenome2 = Sobrenomes[rng.Next(Sobrenomes.Length)];
        return sobrenome1 == sobrenome2 ? $"{primeiro} {sobrenome1}" : $"{primeiro} {sobrenome1} {sobrenome2}";
    }

    public static string EmailPara(string nomeCompleto, string dominio, Random rng)
    {
        var slug = nomeCompleto
            .ToLowerInvariant()
            .Replace(" ", ".")
            .Normalize(System.Text.NormalizationForm.FormD);

        var semAcento = new string(slug.Where(c =>
            System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray());

        return $"{semAcento}{rng.Next(1, 9999)}@{dominio}";
    }
}
