namespace CarStoreManager.Geradores.Nucleo;

/// <summary>
/// Quantidades a gerar por entidade — com defaults que somam bem acima de
/// 500 registros. Cada quantidade pode ser sobrescrita via linha de comando,
/// o que é o que torna o gerador reaproveitável para ir aumentando a base
/// aos poucos (ex.: rodar só mais Clientes e OrdensServico depois).
/// </summary>
public sealed class OpcoesCli
{
    public int Seed { get; private set; } = Environment.TickCount;
    public bool MostrarAjuda { get; private set; }

    public Dictionary<string, int> Usuarios { get; } = new()
    {
        ["Vendedor"] = 30,
        ["Mecanico"] = 30,
        ["Recepcionista"] = 18,
        ["ChefeOficina"] = 6,
        ["GerenteVendas"] = 6,
        ["Admin"] = 4
    };

    /// <summary>
    /// Há quantos anos a loja "está em operação" — define o tamanho da janela
    /// histórica em que veículos/propostas/OS/consignações/balanços são
    /// espalhados. Padrão 4 (dentro da faixa 3–5 anos).
    /// </summary>
    public double AnosOperacao { get; private set; } = 4.0;

    public int Clientes { get; private set; } = 800;
    public int Fornecedores { get; private set; } = 60;
    public int Componentes { get; private set; } = 300;
    public int Despesas { get; private set; } = 45;
    public int ChecklistPresets { get; private set; } = 24;
    public int VeiculosVenda { get; private set; } = 900;
    public int VeiculosCliente { get; private set; } = 900;
    public int VeiculosConsignacao { get; private set; } = 350;
    public int PropostasVenda { get; private set; } = 550;
    public int TestDrives { get; private set; } = 700;
    public int OrdensServico { get; private set; } = 900;

    public static OpcoesCli Analisar(string[] args)
    {
        var opcoes = new OpcoesCli();

        for (var i = 0; i < args.Length; i++)
        {
            var chave = args[i];

            if (chave is "--ajuda" or "--help" or "-h")
            {
                opcoes.MostrarAjuda = true;
                continue;
            }

            if (i + 1 >= args.Length) continue;
            var valorTexto = args[i + 1];

            switch (chave)
            {
                case "--seed":
                    if (int.TryParse(valorTexto, out var seed)) opcoes.Seed = seed;
                    i++;
                    break;
                case "--anos-operacao":
                    if (double.TryParse(valorTexto, System.Globalization.CultureInfo.InvariantCulture, out var anos) && anos > 0)
                        opcoes.AnosOperacao = anos;
                    i++;
                    break;
                case "--vendedores": AtribuirSeValido(valorTexto, v => opcoes.Usuarios["Vendedor"] = v); i++; break;
                case "--mecanicos": AtribuirSeValido(valorTexto, v => opcoes.Usuarios["Mecanico"] = v); i++; break;
                case "--recepcionistas": AtribuirSeValido(valorTexto, v => opcoes.Usuarios["Recepcionista"] = v); i++; break;
                case "--chefes-oficina": AtribuirSeValido(valorTexto, v => opcoes.Usuarios["ChefeOficina"] = v); i++; break;
                case "--gerentes-vendas": AtribuirSeValido(valorTexto, v => opcoes.Usuarios["GerenteVendas"] = v); i++; break;
                case "--admins": AtribuirSeValido(valorTexto, v => opcoes.Usuarios["Admin"] = v); i++; break;
                case "--clientes": AtribuirSeValido(valorTexto, v => opcoes.Clientes = v); i++; break;
                case "--fornecedores": AtribuirSeValido(valorTexto, v => opcoes.Fornecedores = v); i++; break;
                case "--componentes": AtribuirSeValido(valorTexto, v => opcoes.Componentes = v); i++; break;
                case "--despesas": AtribuirSeValido(valorTexto, v => opcoes.Despesas = v); i++; break;
                case "--checklists": AtribuirSeValido(valorTexto, v => opcoes.ChecklistPresets = v); i++; break;
                case "--veiculos-venda": AtribuirSeValido(valorTexto, v => opcoes.VeiculosVenda = v); i++; break;
                case "--veiculos-cliente": AtribuirSeValido(valorTexto, v => opcoes.VeiculosCliente = v); i++; break;
                case "--veiculos-consignados": AtribuirSeValido(valorTexto, v => opcoes.VeiculosConsignacao = v); i++; break;
                case "--propostas": AtribuirSeValido(valorTexto, v => opcoes.PropostasVenda = v); i++; break;
                case "--test-drives": AtribuirSeValido(valorTexto, v => opcoes.TestDrives = v); i++; break;
                case "--ordens-servico": AtribuirSeValido(valorTexto, v => opcoes.OrdensServico = v); i++; break;
            }
        }

        return opcoes;

        static void AtribuirSeValido(string texto, Action<int> atribuir)
        {
            if (int.TryParse(texto, out var valor) && valor >= 0) atribuir(valor);
        }
    }

    public static void ImprimirAjuda()
    {
        Console.WriteLine("""
            Geradores — popula o banco SQLite local com dados fake reais (via Application Services).

            Por padrão, veículos/propostas/consignações/OS são espalhados pelos
            últimos ~2 anos e avançados por vários estágios do fluxo de negócio
            (concluídas, em andamento, canceladas/rejeitadas, recém-criadas) —
            simulando uma loja em operação há tempo, não uma base zerada.

            Uso:
              dotnet run --project Geradores -- [opções]

            Opções (todas opcionais — sem nenhuma, gera a base de demonstração
            completa: loja "em operação há ~4 anos", ~12 mil registros no banco,
            processos em todos os estágios do fluxo, dados em todas as tabelas):
              --seed N                    semente do gerador aleatório (reprodutibilidade)
              --anos-operacao N            janela histórica em anos (padrão 4; faixa útil 3–5)
              --vendedores N               (padrão 30)
              --mecanicos N                (padrão 30)
              --recepcionistas N           (padrão 18)
              --chefes-oficina N           (padrão 6)
              --gerentes-vendas N          (padrão 6)
              --admins N                   (padrão 4)
              --clientes N                 (padrão 800)
              --fornecedores N             (padrão 60)
              --componentes N              (padrão 300)
              --despesas N                 (padrão 45)
              --checklists N               (padrão 24)
              --veiculos-venda N           (padrão 900)
              --veiculos-cliente N         (padrão 900)
              --veiculos-consignados N     (padrão 350)
              --propostas N                (padrão 550)
              --test-drives N              (padrão 700)
              --ordens-servico N           (padrão 900)

            Exemplos:
              dotnet run --project Geradores
              dotnet run --project Geradores -- --anos-operacao 5 --ordens-servico 1200
              dotnet run --project Geradores -- --seed 42
            """);
    }
}
