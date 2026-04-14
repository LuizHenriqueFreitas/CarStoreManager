using System.Text.RegularExpressions;

namespace CarStoreManager.Domain.ValueObjects;

public class PlacaVeiculo
{
    public string Valor { get; }

    protected PlacaVeiculo () {}

    public PlacaVeiculo(string placa)
    {
        if (!Validar(placa))
            throw new ArgumentException("Placa inválida");

        Valor = Normalizar(placa);
    }

    private static string Normalizar(string placa)
    {
        return placa.Replace("-", "")
                    .ToUpper()
                    .Trim();
    }

    public static bool Validar(string placa)
    {
        if (string.IsNullOrWhiteSpace(placa))
            return false;

        placa = placa.ToUpper().Trim();

        // 🚗 formato antigo: ABC-1234 ou ABC1234
        var antiga = new Regex(@"^[A-Z]{3}-?\d{4}$");

        // 🚗 mercosul: ABC1D23
        var mercosul = new Regex(@"^[A-Z]{3}\d[A-Z]\d{2}$");

        return antiga.IsMatch(placa) || mercosul.IsMatch(placa);
    }

    public override string ToString()
    {
        return Valor;
    }
}