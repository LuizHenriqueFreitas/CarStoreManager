using System.Text.RegularExpressions;
using CarStoreManager.Domain.Exceptions;

namespace CarStoreManager.Domain.ValueObjects;

/*
    Neste arquivo temos as implementações de verificações 
    e regras de negócios do VO de Placa dos Veiculos, 
    alem do metodo get e set, temos a validação e normalização.

    Testes automaticos implementados para:
        Criar placas validas nos padroes permitidos,
        Bloquear placas fora dos padroes permitidos,
        Verificar o metodo de normalização
*/

public class PlacaVeiculo
{
    public string Valor { get; private set; } = null!;

    protected PlacaVeiculo () {}

    //metodo construtor roda o metodo de validação
    public PlacaVeiculo(string placa)
    {
        if (!ValidaPlaca(placa))
            throw new PlacaVeiculoInvalidaException(placa);

        Valor = Normalizar(placa);
    }

    //get valor placa
    public string GetPlaca()
    {
        return Valor;
    }

    /*
        metodo de atualização da placa
        embora seja dificil usar, ele ja existe.
        serve como setter
    */
    public void AtualizaPlaca(string novaPlaca)
    {
        if(!ValidaPlaca(novaPlaca))
            throw new PlacaVeiculoInvalidaException(novaPlaca);

        Valor = Normalizar(novaPlaca);
    }

    //metodo que normaliza a entrada do usuario
    protected string Normalizar(string placa)
    {
        return placa.Replace("-", "")
                    .Replace(" ", "")
                    .ToUpper()
                    .Trim();
    }

    /*
        metodo de validação da placa
        verifica que a entrada esteja de acordo
        com o padrao antigo brasileiro ou 
        padrao mercosul
    */
    public bool ValidaPlaca(string placa)
    {
        if (string.IsNullOrWhiteSpace(placa))
            return false;

        // Normaliza antes de validar — usuário pode digitar com ou sem hífen/espaço
        placa = Normalizar(placa);

        // formato antigo: ABC1234
        var antiga = new Regex(@"^[A-Z]{3}\d{4}$");

        // formato mercosul: ABC1D23
        var mercosul = new Regex(@"^[A-Z]{3}\d[A-Z]\d{2}$");

        return antiga.IsMatch(placa) || mercosul.IsMatch(placa);
    }

    //formatação de saida com ToString()
    public override string ToString()
    {
        return $"{Valor.Substring(0, 3)}-{Valor.Substring(3)}" ?? string.Empty;
    }
}