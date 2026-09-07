using CarStoreManager.Application.DTOs.Oficina.Componente;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Geradores.Dados;
using CarStoreManager.Geradores.Nucleo;
using Microsoft.Extensions.DependencyInjection;

namespace CarStoreManager.Geradores.Entidades;

/// <summary>
/// Gera Componentes (peças de estoque da oficina) via IComponenteService.AddAsync.
/// Categoria não é mais um campo do formulário (ver Configuracoes.razor) — aqui
/// replicamos o mesmo comportamento da UI: Categoria = Sistema escolhido.
/// Depois de cada componente criado, dá entrada de estoque via IEstoqueService
/// para os dados aparecerem populados na tela de Estoque.
/// </summary>
public static class ComponenteGerador
{
    public static async Task<List<Guid>> GerarAsync(
        IServiceProvider provider,
        int quantidade,
        Random rng,
        UniquePool skuPool,
        IReadOnlyList<Guid> fornecedorIds)
    {
        if (fornecedorIds.Count == 0)
        {
            Console.WriteLine("  Componentes: pulado — nenhum fornecedor cadastrado (pré-requisito do formulário de peça).");
            return new List<Guid>();
        }

        var ids = await ExecutorLote.ExecutarAsync<IComponenteService>(
            provider,
            quantidade,
            (servico, _) => servico.AddAsync(MontarDto(rng, skuPool, fornecedorIds)),
            "Componentes");

        var comEstoque = 0;
        foreach (var id in ids)
        {
            using var scope = provider.CreateScope();
            var estoqueService = scope.ServiceProvider.GetRequiredService<IEstoqueService>();
            var entrada = await estoqueService.EntradaAsync(id, rng.Next(5, 200));
            if (entrada.IsSuccess) comEstoque++;
        }
        Console.WriteLine($"  Estoque: entrada de quantidade lançada para {comEstoque}/{ids.Count} componentes");

        return ids;
    }

    private static CriarComponenteDTO MontarDto(Random rng, UniquePool skuPool, IReadOnlyList<Guid> fornecedorIds)
    {
        var sistemas = Enum.GetValues<SistemaComponente>();
        var sistema = sistemas[rng.Next(sistemas.Length)];
        var nomePeca = NomesPt.NomesPecaGenericos[rng.Next(NomesPt.NomesPecaGenericos.Length)];
        var marca = NomesPt.MarcasComponentes[rng.Next(NomesPt.MarcasComponentes.Length)];
        var sku = skuPool.Reservar(() => $"SKU-{DocumentoUtils.GerarDigitos(rng, 8)}");

        return new CriarComponenteDTO
        {
            FornecedorId = fornecedorIds[rng.Next(fornecedorIds.Count)],
            SKUInterno = sku,
            Nome = $"{nomePeca} {marca}",
            Descricao = $"{nomePeca} — sistema {sistema}, compatível com diversos modelos.",
            MarcaFabricante = marca,
            PartNumber = $"PN{DocumentoUtils.GerarDigitos(rng, 6)}",
            CodigoOEM = rng.Next(3) == 0 ? "" : $"OEM{DocumentoUtils.GerarDigitos(rng, 5)}",
            CodigoBarras = rng.Next(3) == 0 ? "" : DocumentoUtils.GerarDigitos(rng, 13),
            NCM = DocumentoUtils.GerarDigitos(rng, 8),
            CEST = rng.Next(2) == 0 ? "" : DocumentoUtils.GerarDigitos(rng, 7),
            // Categoria não existe mais como campo do form — espelha o Sistema (ver EstoqueComponentes.razor).
            Categoria = sistema.ToString(),
            Sistema = sistema.ToString(),
            Unidade = "UN",
            Peso = Math.Round((decimal)(rng.NextDouble() * 20), 3),
            GarantiaDias = new[] { 30, 60, 90, 180, 365 }[rng.Next(5)],
            CustoUnitario = DocumentoUtils.ValorRedondo(rng, 10, 500, 5),
            MargemLucroPct = rng.Next(15, 61)
        };
    }
}
