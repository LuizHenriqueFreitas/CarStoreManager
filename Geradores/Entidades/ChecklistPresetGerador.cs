using CarStoreManager.Application.DTOs.Oficina.ChecklistPreset;
using CarStoreManager.Application.Interfaces.Oficina;
using CarStoreManager.Geradores.Nucleo;

namespace CarStoreManager.Geradores.Entidades;

/// <summary>Gera presets de checklist (Configurações → Checklists) via IChecklistPresetService.AddAsync.</summary>
public static class ChecklistPresetGerador
{
    private static readonly (string Nome, string[] Itens)[] Modelos =
    {
        ("Revisão básica", new[] { "Verificar nível de óleo", "Verificar freios", "Verificar pneus", "Verificar luzes" }),
        ("Troca de óleo", new[] { "Drenar óleo antigo", "Trocar filtro de óleo", "Abastecer óleo novo", "Verificar vazamentos" }),
        ("Diagnóstico elétrico", new[] { "Testar bateria", "Testar alternador", "Verificar fusíveis", "Verificar chicote" }),
        ("Freios completo", new[] { "Verificar pastilhas", "Verificar discos", "Verificar fluido de freio", "Sangria do sistema" }),
        ("Ar-condicionado", new[] { "Verificar gás", "Verificar compressor", "Limpar filtro de cabine", "Testar termostato" }),
        ("Suspensão", new[] { "Verificar amortecedores", "Verificar molas", "Verificar bandejas", "Verificar bieletas" }),
        ("Pré-venda", new[] { "Lavagem completa", "Verificar documentação", "Checklist de itens de série", "Teste de estrada" }),
        ("Funilaria e pintura", new[] { "Avaliar amassados", "Preparar superfície", "Aplicar primer", "Pintura e polimento" })
    };

    public static Task<List<Guid>> GerarAsync(IServiceProvider provider, int quantidade, Random rng)
    {
        return ExecutorLote.ExecutarAsync<IChecklistPresetService>(
            provider,
            quantidade,
            (servico, i) => servico.AddAsync(MontarDto(rng, i)),
            "Checklist presets");
    }

    private static SalvarChecklistPresetDTO MontarDto(Random rng, int indice)
    {
        var modelo = Modelos[rng.Next(Modelos.Length)];

        return new SalvarChecklistPresetDTO
        {
            Nome = $"{modelo.Nome} #{indice + 1}",
            Ativo = rng.Next(10) != 0, // ~90% ativos
            Itens = modelo.Itens.ToList()
        };
    }
}
