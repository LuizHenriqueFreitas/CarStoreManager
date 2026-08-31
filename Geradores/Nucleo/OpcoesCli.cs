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
        ["Vendedor"] = 20,
        ["Mecanico"] = 20,
        ["Recepcionista"] = 15,
        ["ChefeOficina"] = 8,
        ["GerenteVendas"] = 8,
        ["Admin"] = 5
    };

    public int Clientes { get; private set; } = 120;
    public int Componentes { get; private set; } = 80;
    public int Despesas { get; private set; } = 30;
    public int ChecklistPresets { get; private set; } = 15;
    public int VeiculosVenda { get; private set; } = 150;
    public int VeiculosCliente { get; private set; } = 100;
    public int VeiculosConsignacao { get; private set; } = 60;
    public int PropostasVenda { get; private set; } = 150;
    public int OrdensServico { get; private set; } = 150;

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
                case "--vendedores": AtribuirSeValido(valorTexto, v => opcoes.Usuarios["Vendedor"] = v); i++; break;
                case "--mecanicos": AtribuirSeValido(valorTexto, v => opcoes.Usuarios["Mecanico"] = v); i++; break;
                case "--recepcionistas": AtribuirSeValido(valorTexto, v => opcoes.Usuarios["Recepcionista"] = v); i++; break;
                case "--chefes-oficina": AtribuirSeValido(valorTexto, v => opcoes.Usuarios["ChefeOficina"] = v); i++; break;
                case "--gerentes-vendas": AtribuirSeValido(valorTexto, v => opcoes.Usuarios["GerenteVendas"] = v); i++; break;
                case "--admins": AtribuirSeValido(valorTexto, v => opcoes.Usuarios["Admin"] = v); i++; break;
                case "--clientes": AtribuirSeValido(valorTexto, v => opcoes.Clientes = v); i++; break;
                case "--componentes": AtribuirSeValido(valorTexto, v => opcoes.Componentes = v); i++; break;
                case "--despesas": AtribuirSeValido(valorTexto, v => opcoes.Despesas = v); i++; break;
                case "--checklists": AtribuirSeValido(valorTexto, v => opcoes.ChecklistPresets = v); i++; break;
                case "--veiculos-venda": AtribuirSeValido(valorTexto, v => opcoes.VeiculosVenda = v); i++; break;
                case "--veiculos-cliente": AtribuirSeValido(valorTexto, v => opcoes.VeiculosCliente = v); i++; break;
                case "--veiculos-consignados": AtribuirSeValido(valorTexto, v => opcoes.VeiculosConsignacao = v); i++; break;
                case "--propostas": AtribuirSeValido(valorTexto, v => opcoes.PropostasVenda = v); i++; break;
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

            Opções (todas opcionais — sem nenhuma, gera a distribuição padrão, ~900 registros):
              --seed N                    semente do gerador aleatório (reprodutibilidade)
              --vendedores N               (padrão 20)
              --mecanicos N                (padrão 20)
              --recepcionistas N           (padrão 15)
              --chefes-oficina N           (padrão 8)
              --gerentes-vendas N          (padrão 8)
              --admins N                   (padrão 5)
              --clientes N                 (padrão 120)
              --componentes N              (padrão 80)
              --despesas N                 (padrão 30)
              --checklists N               (padrão 15)
              --veiculos-venda N           (padrão 150)
              --veiculos-cliente N         (padrão 100)
              --veiculos-consignados N     (padrão 60)
              --propostas N                (padrão 150)
              --ordens-servico N           (padrão 150)

            Exemplos:
              dotnet run --project Geradores
              dotnet run --project Geradores -- --clientes 300 --ordens-servico 200
              dotnet run --project Geradores -- --seed 42
            """);
    }
}
