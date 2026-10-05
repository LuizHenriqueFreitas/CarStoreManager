using System.Linq.Expressions;

namespace CarStoreManager.Application.Interfaces.Sistema;

/// <summary>
/// Sobrescreve campos de data/hora de uma entidade já criada — DataCriacao e
/// similares (PrazoEstimado, DataAprovacao...) só têm setter protegido, sem
/// jeito de vir de fora pelo construtor. Usado exclusivamente pela importação
/// de dados de demonstração, pra simular um histórico de anos de uso em vez
/// de tudo nascer "agora". Implementado em Infrastructure via EF Core
/// (contorna o setter protegido através dos metadados do EF, não reflection
/// direta).
/// </summary>
public interface IBackdateService
{
    Task AplicarAsync<TEntity>(Guid id, params (string Propriedade, object? Valor)[] valores)
        where TEntity : class;

    /// <summary>
    /// Mesmo que <see cref="AplicarAsync{TEntity}"/>, só que em lote: aplica os
    /// valores a TODAS as entidades que casam com o filtro (ex.: todos os itens
    /// de checklist de uma OS) — pra filhos cujo Id nenhum service devolve.
    /// Retorna quantas linhas foram alteradas.
    /// </summary>
    Task<int> AplicarOndeAsync<TEntity>(Expression<Func<TEntity, bool>> filtro, params (string Propriedade, object? Valor)[] valores)
        where TEntity : class;

    /// <summary>
    /// Consulta somente-leitura (sem rastreamento) com projeção — pra achar
    /// Ids de filhos que nenhum service devolve (ex.: histórico de uma
    /// consignação, item de despesa recém-lançado).
    /// </summary>
    Task<List<TResultado>> ConsultarAsync<TEntity, TResultado>(
        Expression<Func<TEntity, bool>> filtro, Expression<Func<TEntity, TResultado>> projecao)
        where TEntity : class;

    /// <summary>
    /// Esvazia o rastreamento de entidades do contexto compartilhado (escopo
    /// da importação). Importação grande no mesmo escopo acumula milhares de
    /// entidades rastreadas e cada SaveChanges fica O(n) — chamado entre um
    /// registro e outro (nunca no meio de um), mantém a importação linear.
    /// </summary>
    void LimparRastreamento();
}
