using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Geradores.Nucleo;

namespace CarStoreManager.Geradores.Entidades;

/// <summary>Gera Despesas mensais (Sistema/Configurações) via IDespesaService.AddAsync.</summary>
public static class DespesaGerador
{
    private static readonly (string Nome, TipoDespesa Tipo)[] Modelos =
    {
        ("Salário recepcionista", TipoDespesa.Salario),
        ("Salário mecânico", TipoDespesa.Salario),
        ("Salário vendedor", TipoDespesa.Salario),
        ("Pró-labore administrador", TipoDespesa.Salario),
        ("Aluguel do galpão", TipoDespesa.Aluguel),
        ("Aluguel do showroom", TipoDespesa.Aluguel),
        ("Conta de energia elétrica", TipoDespesa.Utilidades),
        ("Conta de água", TipoDespesa.Utilidades),
        ("Internet e telefonia", TipoDespesa.Utilidades),
        ("Manutenção de ferramentas", TipoDespesa.Manutencao),
        ("Manutenção da elevatória", TipoDespesa.Manutencao),
        ("Marketing digital", TipoDespesa.Marketing),
        ("Anúncios em portais de veículos", TipoDespesa.Marketing),
        ("Imposto municipal (ISS)", TipoDespesa.Impostos),
        ("Imposto sobre serviços", TipoDespesa.Impostos),
        ("Seguro do galpão", TipoDespesa.Seguro),
        ("Seguro da frota de test-drive", TipoDespesa.Seguro),
        ("Reforma do showroom", TipoDespesa.Investimento),
        ("Compra de elevador novo", TipoDespesa.Investimento),
        ("Contabilidade terceirizada", TipoDespesa.Servicos),
        ("Serviço de limpeza", TipoDespesa.Servicos),
        ("Material de escritório", TipoDespesa.Outros),
        ("Despesas diversas", TipoDespesa.Outros)
    };

    public static Task<List<Guid>> GerarAsync(IServiceProvider provider, int quantidade, Random rng)
    {
        return ExecutorLote.ExecutarAsync<IDespesaService>(
            provider,
            quantidade,
            (servico, i) => servico.AddAsync(MontarDto(rng, i)),
            "Despesas");
    }

    private static CriarDespesaDTO MontarDto(Random rng, int indice)
    {
        var modelo = Modelos[rng.Next(Modelos.Length)];
        var setores = Enum.GetValues<SetorDespesa>();

        return new CriarDespesaDTO
        {
            Nome = $"{modelo.Nome} #{indice + 1}",
            Valor = DocumentoUtils.ValorRedondo(rng, 150, 5000, 50),
            Setor = setores[rng.Next(setores.Length)].ToString(),
            Tipo = modelo.Tipo.ToString()
        };
    }
}
