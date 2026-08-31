using CarStoreManager.Application.DTOs.Shared.Cliente;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Geradores.Dados;
using CarStoreManager.Geradores.Nucleo;

namespace CarStoreManager.Geradores.Entidades;

/// <summary>Gera Clientes via IClienteService.AddAsync (respeita a checagem de CPF duplicado).</summary>
public static class ClienteGerador
{
    public static Task<List<Guid>> GerarAsync(
        IServiceProvider provider,
        int quantidade,
        Random rng,
        UniquePool cpfPool,
        UniquePool emailPool,
        UniquePool telefonePool)
    {
        return ExecutorLote.ExecutarAsync<IClienteService>(
            provider,
            quantidade,
            (servico, _) => servico.AddAsync(MontarDto(rng, cpfPool, emailPool, telefonePool)),
            "Clientes");
    }

    private static CriarClienteDTO MontarDto(Random rng, UniquePool cpfPool, UniquePool emailPool, UniquePool telefonePool)
    {
        var nome = NomesPt.NomeCompletoAleatorio(rng);
        var cidadeIdx = rng.Next(NomesPt.Cidades.Length);

        return new CriarClienteDTO
        {
            Nome = nome,
            Cpf = cpfPool.Reservar(() => DocumentoUtils.GerarCpf(rng)),
            Email = emailPool.Reservar(() => NomesPt.EmailPara(nome, "gmail.com", rng)),
            Telefone = telefonePool.Reservar(() => DocumentoUtils.GerarTelefoneCelular(rng)),
            Endereco = new EnderecoDTO
            {
                Logradouro = NomesPt.Logradouros[rng.Next(NomesPt.Logradouros.Length)],
                Numero = rng.Next(1, 3000).ToString(),
                Complemento = rng.Next(4) == 0 ? $"Apto {rng.Next(1, 200)}" : null,
                Bairro = NomesPt.Bairros[rng.Next(NomesPt.Bairros.Length)],
                Cidade = NomesPt.Cidades[cidadeIdx],
                Uf = NomesPt.UfsPorCidade[cidadeIdx],
                Cep = DocumentoUtils.GerarCep(rng)
            }
        };
    }
}
