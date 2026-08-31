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
}
