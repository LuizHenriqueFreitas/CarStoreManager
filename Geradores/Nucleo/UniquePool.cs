namespace CarStoreManager.Geradores.Nucleo;

/// <summary>
/// Garante unicidade de campos como email/CPF/placa/SKU tanto dentro de uma
/// mesma execução quanto contra o que já existe no banco — carregado uma vez
/// no início via <see cref="CarregarExistentes"/>. Isso é o que permite rodar
/// o gerador várias vezes seguidas para ir aumentando a base sem colisão.
/// </summary>
public sealed class UniquePool
{
    private readonly HashSet<string> _valores;

    public UniquePool(IEnumerable<string>? existentes = null)
    {
        _valores = new HashSet<string>(
            (existentes ?? Enumerable.Empty<string>()).Select(Normalizar),
            StringComparer.OrdinalIgnoreCase);
    }

    private static string Normalizar(string valor) => valor.Trim().ToUpperInvariant();

    /// <summary>Chama <paramref name="gerador"/> até obter um valor ainda não usado, reserva e devolve.</summary>
    public string Reservar(Func<string> gerador, int maxTentativas = 200)
    {
        for (var i = 0; i < maxTentativas; i++)
        {
            var candidato = gerador();
            if (_valores.Add(Normalizar(candidato)))
                return candidato;
        }

        throw new InvalidOperationException(
            $"Não foi possível gerar um valor único após {maxTentativas} tentativas — pool de valores possivelmente saturado.");
    }
}
