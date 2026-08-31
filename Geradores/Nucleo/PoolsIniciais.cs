using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarStoreManager.Geradores.Nucleo;

/// <summary>
/// Conjunto de <see cref="UniquePool"/> pré-carregados com o que já existe no
/// banco (e-mail, telefone, CPF, placa, RENAVAM, SKU) — é isso que permite
/// rodar o gerador várias vezes seguidas sem colidir com dados de execuções
/// anteriores.
/// </summary>
public sealed class PoolsIniciais
{
    public required UniquePool Email { get; init; }
    public required UniquePool Telefone { get; init; }
    public required UniquePool Cpf { get; init; }
    public required UniquePool Placa { get; init; }
    public required UniquePool Renavam { get; init; }
    public required UniquePool Sku { get; init; }

    public static async Task<PoolsIniciais> CarregarAsync(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var usuarios = await db.Usuarios.ToListAsync();
        var clientes = await db.Clientes.ToListAsync();
        var veiculosVenda = await db.VeiculosVenda.ToListAsync();
        var veiculosCliente = await db.VeiculosCliente.ToListAsync();
        var veiculosConsignacao = await db.VeiculosConsignacao.ToListAsync();
        var componentes = await db.Componentes.ToListAsync();

        var emails = usuarios.Select(u => u.GetEmail())
            .Concat(clientes.Select(c => c.GetEmail()));

        var telefones = usuarios.Select(u => u.GetTelefone())
            .Concat(clientes.Select(c => c.GetTelefone()));

        var cpfs = clientes.Select(c => c.GetCpf());

        var placas = veiculosVenda.Select(v => v.GetPlacaCarro())
            .Concat(veiculosCliente.Select(v => v.Placa.GetPlaca()))
            .Concat(veiculosConsignacao.Select(v => v.GetPlacaCarro()));

        var renavams = veiculosVenda.Select(v => v.GetRenavam())
            .Concat(veiculosConsignacao.Select(v => v.GetRenavam()));

        var skus = componentes.Select(c => c.GetSKUInterno());

        return new PoolsIniciais
        {
            Email = new UniquePool(emails),
            Telefone = new UniquePool(telefones),
            Cpf = new UniquePool(cpfs),
            Placa = new UniquePool(placas),
            Renavam = new UniquePool(renavams),
            Sku = new UniquePool(skus)
        };
    }
}
