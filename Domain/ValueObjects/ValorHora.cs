namespace CarStoreManager.Domain.ValueObjects;

/*
    Neste arquivo temos as implementações de verificações 
    e regras de negócios do VO de ValorHora, alem dos metodos 
    get e set.

    Testes automaticos implementados para:
        Criar ValorHora valido,
        Bloquear ValorHora invalido,
        Atualizar ValorHora valido,
        Bloquear atualização com valor invalido,
        Veficar o calculo de horas trabalhadas * ValorHora,
        Verifivar a formatação de saida com o metodo ToString()
*/

public class ValorHora
{
    private Dinheiro Valor { get; set; } = null!;

    protected ValorHora() { }

    //construtor passa pela validação
    public ValorHora(decimal valor)
    {
        if (!ValidaValorHora(valor))
            throw new ArgumentException("Valor por hora deve ser maior que zero");

        Valor = new Dinheiro(decimal.Round(valor, 2));
    }

    //metodo que pega o valor hora como tipo de dado Dinheiro
    public Dinheiro GetValorHora()
    {
        return Valor;
    }

    //atualiza o valor hora - passa pela validação
    public void Atualizar(decimal novoValor)
    {
        if (!ValidaValorHora(novoValor))
            throw new ArgumentException("Valor por hora deve ser maior que zero");

        Valor = new Dinheiro(novoValor);
    }

    //verifica que o valor hora é maior que 0
    public bool ValidaValorHora(decimal valor)
    {
        if(valor <= 0)
            return false;

        return true;
    }

    //metodo que calcula o valor de um servico apartir do valor hora
    public Dinheiro CalcularServicoValorHora(int horas)
    {
        if (horas <= 0)
            throw new ArgumentException("Horas inválidas");

        return new Dinheiro(Valor.GetValorDinheiro() * horas);
    }

    //formata saida do valor hora em string
    public override string ToString()
        => $"{Valor:F2}";
}