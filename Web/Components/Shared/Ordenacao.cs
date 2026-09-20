namespace CarStoreManager.Web.Components.Shared;

/// <summary>
/// Helper de ordenação pras "tabelas" (div-grid) de listagem com cabeçalho
/// clicável (ver <see cref="ColunaOrdenavel"/>) — mesma lógica pras 4 telas
/// de lista da Oficina (Recepção, Ordens de serviço, Estoque, Fornecedores):
/// 1º clique na coluna ordena crescente, 2º clique inverte. Funciona pra
/// colunas numéricas (int/decimal/DateTime) e alfabéticas (string) porque
/// <c>OrderBy</c>/<c>OrderByDescending</c> já usam o comparador natural de
/// <typeparamref name="TKey"/>.
/// </summary>
public static class Ordenacao
{
    public static List<T> Por<T, TKey>(IEnumerable<T> itens, Func<T, TKey> chave, bool ascendente) =>
        (ascendente ? itens.OrderBy(chave) : itens.OrderByDescending(chave)).ToList();
}
