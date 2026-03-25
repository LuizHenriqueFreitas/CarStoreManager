using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Services.Oficina;

public static class ChecklistTemplates
{
    private static readonly Dictionary<TipoServico, List<string>> _templates = new()
    {
        [TipoServico.Manutencao] = new()
        {
            "Verificar nível de óleo do motor",
            "Verificar fluido de freio",
            "Verificar fluido de arrefecimento",
            "Inspecionar filtro de ar",
            "Verificar correia dentada",
            "Inspecionar sistema de freios",
            "Testar bateria",
        },
        [TipoServico.Revisao] = new()
        {
            "Trocar óleo e filtro",
            "Verificar pressão dos pneus",
            "Inspecionar sistema de suspensão",
            "Verificar alinhamento e balanceamento",
            "Testar todos os itens elétricos",
            "Verificar lâmpadas e faróis",
        },
        [TipoServico.TrocaPecas] = new()
        {
            "Identificar peça a ser substituída",
            "Verificar disponibilidade em estoque",
            "Remover peça antiga",
            "Instalar peça nova",
            "Teste funcional pós-instalação",
        },
        [TipoServico.Diagnostico] = new()
        {
            "Leitura de códigos de falha (scanner)",
            "Identificar sistema com problema",
            "Realizar teste de bancada se necessário",
            "Documentar falhas encontradas",
            "Apresentar relatório ao responsável",
        },
        [TipoServico.Outro] = new()
        {
            "Avaliar condição geral do veículo",
            "Descrever serviço a ser executado",
            "Executar serviço",
            "Verificar resultado final",
        }
    };

    public static List<string> ObterPorTipo(TipoServico tipo)
    {
        return _templates.TryGetValue(tipo, out var itens) ? itens : _templates[TipoServico.Outro];
    }
}