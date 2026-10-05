using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Entities.Sistema;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Data;

/// <summary>
/// Seed do banco no 1º run — aplica as migrations e cria o mínimo pra o
/// sistema funcionar (admin inicial, presets de checklist, templates padrão
/// de termo de test drive e contrato de OS). Antes vivia inline no
/// Web/Program.cs; foi extraído pra cá pra que o CLI de importação
/// (Geradores --importar) monte o banco EXATAMENTE igual ao Web, sem
/// precisar subir o site uma vez antes. Idempotente: cada etapa tem guarda.
/// ConfiguracaoSistema e permissões por papel não precisam de seed — são
/// criadas sob demanda (ConfiguracaoSistemaRepository.ObterAsync /
/// CatalogoRecursosProtegiveis como padrão).
/// </summary>
public static class SeedInicial
{
    public static async Task AplicarAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();
        await SeedAdminAsync(context);
        await SeedChecklistPresetsAsync(context);
        await SeedTemplateTermoTestDriveAsync(context);
        await SeedTemplateContratoOSAsync(context);
    }

    private static async Task SeedAdminAsync(AppDbContext context)
    {
        var adminExiste = context.Usuarios.OfType<Admin>().Any();
        if (adminExiste) return;

        var admin = new Admin(
            "Administrador",
            "admin@teste.com",
            "11215126548",
            "12345A"
        );

        context.Usuarios.Add(admin);
        await context.SaveChangesAsync();

        Console.WriteLine("Admin criado — email: admin@teste.com / senha: 12345A");
    }

    // Popula presets iniciais somente quando o BD ainda não tem nenhum — admin
    // pode editar/renomear/excluir tudo via tela de Configurações depois.
    private static async Task SeedChecklistPresetsAsync(AppDbContext context)
    {
        if (context.ChecklistPresets.Any()) return;

        var presets = new[]
        {
            ("Manutenção padrão", new[]
            {
                "Verificar nível de óleo do motor",
                "Verificar fluido de freio",
                "Verificar fluido de arrefecimento",
                "Inspecionar filtro de ar",
                "Verificar correia dentada",
                "Inspecionar sistema de freios",
                "Testar bateria"
            }),
            ("Revisão completa", new[]
            {
                "Trocar óleo e filtro",
                "Verificar pressão dos pneus",
                "Inspecionar sistema de suspensão",
                "Verificar alinhamento e balanceamento",
                "Testar todos os itens elétricos",
                "Verificar lâmpadas e faróis"
            }),
            ("Carro elétrico", new[]
            {
                "Verificar estado e fixação da bateria de alta tensão",
                "Inspecionar cabeamento de alta voltagem",
                "Testar sistema de carregamento",
                "Verificar sistema de arrefecimento da bateria",
                "Atualizar firmware se aplicável",
                "Inspecionar sistema regenerativo de freios"
            })
        };

        foreach (var (nome, itens) in presets)
        {
            var preset = new ChecklistPreset(nome);
            preset.SubstituirItens(itens);
            context.ChecklistPresets.Add(preset);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"{presets.Length} preset(s) de checklist iniciais criados.");
    }

    // Semeia o template padrão de termo de responsabilidade de test drive — guard
    // por Nome (não "Any()" geral, já que a tabela de templates pode já ter
    // outras linhas cadastradas manualmente pelo admin) — admin pode editar ou
    // excluir depois em Configurações → Documentos.
    private static async Task SeedTemplateTermoTestDriveAsync(AppDbContext context)
    {
        const string nome = "Termo de responsabilidade — Test drive (padrão)";
        if (context.TemplatesDocumento.Any(t => t.Nome == nome)) return;

        context.TemplatesDocumento.Add(new TemplateDocumento(nome, TemplatesDocumentosPadrao.TermoTestDrive));
        await context.SaveChangesAsync();
        Console.WriteLine("Template de termo de test drive criado.");
    }

    // Semeia o template padrão do contrato de OS (vistoria de entrada, feita
    // pelo recepcionista) — mesmo padrão guard-por-Nome acima.
    private static async Task SeedTemplateContratoOSAsync(AppDbContext context)
    {
        const string nome = "Contrato de OS — vistoria de entrada (padrão)";
        if (context.TemplatesDocumento.Any(t => t.Nome == nome)) return;

        context.TemplatesDocumento.Add(new TemplateDocumento(nome, TemplatesDocumentosPadrao.ContratoOS));
        await context.SaveChangesAsync();
        Console.WriteLine("Template de contrato de OS criado.");
    }
}
