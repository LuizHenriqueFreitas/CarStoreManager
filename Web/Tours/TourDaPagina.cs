namespace CarStoreManager.Web.Tours;

/// <summary>Tour guiado de uma tela: o roteiro completo de passos, mais os metadados usados para localizá-lo e apresentá-lo.</summary>
public sealed class TourDaPagina
{
    /// <summary>
    /// Template da rota exatamente como declarado no <c>@page</c> da tela
    /// (ex.: <c>"/concessionaria/veiculo/{id}"</c>), em minúsculas, com o nome
    /// do parâmetro genérico — ver <see cref="RegistroDeTours.NormalizarRota"/>.
    /// </summary>
    public required string RotaTemplate { get; init; }

    /// <summary>Nome da tela, mostrado no cabeçalho do balão de introdução.</summary>
    public required string Titulo { get; init; }

    /// <summary>Resumo de uma linha do que a tela faz.</summary>
    public required string Descricao { get; init; }

    public required IReadOnlyList<PassoTour> Passos { get; init; }
}
