using CarStoreManager.Application.Common;
using Microsoft.Extensions.DependencyInjection;

namespace CarStoreManager.Geradores.Nucleo;

/// <summary>
/// Executor genérico de criação em lote: para cada item, abre um escopo de DI
/// novo (DbContext isolado por operação, igual ao ciclo de vida real de uma
/// requisição HTTP), resolve o serviço da Application e chama AddAsync.
/// Qualquer gerador de entidade nova pode reusar isso em vez de reescrever o
/// laço de criação + contagem de sucesso/falha.
/// </summary>
public static class ExecutorLote
{
    public static async Task<List<Guid>> ExecutarAsync<TServico>(
        IServiceProvider provider,
        int quantidade,
        Func<TServico, int, Task<Result<Guid>>> acao,
        string rotulo)
        where TServico : notnull
    {
        var ids = new List<Guid>(quantidade);
        var falhas = new List<string>();

        for (var i = 0; i < quantidade; i++)
        {
            using var scope = provider.CreateScope();
            var servico = scope.ServiceProvider.GetRequiredService<TServico>();

            Result<Guid> resultado;
            try
            {
                resultado = await acao(servico, i);
            }
            catch (Exception ex)
            {
                resultado = Result<Guid>.Fail(ex.Message);
            }

            if (resultado.IsSuccess && resultado.Value != Guid.Empty)
                ids.Add(resultado.Value);
            else
                falhas.Add(resultado.Error ?? "erro desconhecido");
        }

        Console.WriteLine(
            falhas.Count == 0
                ? $"  {rotulo}: {ids.Count}/{quantidade} criados"
                : $"  {rotulo}: {ids.Count}/{quantidade} criados ({falhas.Count} falharam — ex: \"{falhas[0]}\")");

        return ids;
    }
}
