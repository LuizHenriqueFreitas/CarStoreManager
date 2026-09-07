using CarStoreManager.Application.DTOs.Oficina.Fornecedor;
using CarStoreManager.Application.DTOs.Shared.Cliente;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Geradores.Dados;
using CarStoreManager.Geradores.Nucleo;
using Microsoft.Extensions.DependencyInjection;

namespace CarStoreManager.Geradores.Entidades;

/// <summary>
/// Gera fornecedores de peças (oficina → Fornecedores) via IFornecedorService.
/// É pré-requisito de Componente — a partir do retorno do formulário de peça,
/// IComponenteService exige um FornecedorId já cadastrado. Uma fração é
/// desativada depois de criada (fornecedor que a oficina parou de usar).
/// </summary>
public static class FornecedorGerador
{
    public static async Task<List<Guid>> GerarAsync(
        IServiceProvider provider,
        int quantidade,
        Random rng,
        UniquePool cnpjPool,
        UniquePool emailPool,
        double fracaoDesativar = 0.15)
    {
        var ids = await ExecutorLote.ExecutarAsync<IFornecedorService>(
            provider,
            quantidade,
            (servico, _) => servico.AddAsync(MontarDto(rng, cnpjPool, emailPool)),
            "Fornecedores");

        var desativados = 0;
        foreach (var id in ids)
        {
            if (rng.NextDouble() > fracaoDesativar) continue;
            using var scope = provider.CreateScope();
            var servico = scope.ServiceProvider.GetRequiredService<IFornecedorService>();
            var r = await servico.DesativarAsync(id);
            if (r.IsSuccess) desativados++;
        }
        Console.WriteLine($"  Fornecedores: {desativados}/{ids.Count} desativados (parceria encerrada)");

        return ids;
    }

    private static CriarFornecedorDTO MontarDto(Random rng, UniquePool cnpjPool, UniquePool emailPool)
    {
        var nome = NomesPt.RazaoSocialFornecedor(rng);
        var cidadeIdx = rng.Next(NomesPt.Cidades.Length);
        var temEndereco = rng.Next(3) != 0; // ~2/3 têm endereço

        return new CriarFornecedorDTO
        {
            Nome = nome,
            Cnpj = cnpjPool.Reservar(() => DocumentoUtils.GerarCnpj(rng)),
            Email = emailPool.Reservar(() => NomesPt.EmailPara(nome, "fornecedores.com.br", rng)),
            Telefone = DocumentoUtils.GerarTelefoneCelular(rng),
            Endereco = temEndereco
                ? new EnderecoDTO
                {
                    Logradouro = NomesPt.Logradouros[rng.Next(NomesPt.Logradouros.Length)],
                    Numero = rng.Next(1, 4000).ToString(),
                    Complemento = rng.Next(3) == 0 ? $"Galpão {rng.Next(1, 40)}" : null,
                    Bairro = NomesPt.Bairros[rng.Next(NomesPt.Bairros.Length)],
                    Cidade = NomesPt.Cidades[cidadeIdx],
                    Uf = NomesPt.UfsPorCidade[cidadeIdx],
                    Cep = DocumentoUtils.GerarCep(rng)
                }
                : null
        };
    }
}
