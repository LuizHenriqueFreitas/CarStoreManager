namespace CarStoreManager.Domain.ValueObjects;

/*
    Neste arquivo temos as implementações de verificações 
    e regras de negócios do VO de Dinheiro, 
    alem do metodo de: GetValorDinheiro().

    Testes automaticos implementados para:
        Validar criação de dinheiro correto,
        Arredondar valor para 2 casas decimais,
        Bloquear criação de dinhiero negativo,
        Validar operação de soma pelo metodo Somar(),
        Validar operação de soma pelo operador "+",
        Validar operação de subtração pelo metodo Subtrair(),
        Validar operação de subtração pelo operador "-",
        Bloquear resultado negativo no meotodo Subtrair(),
        Validar operação de multiplicação pelo metodo Multiplicar(),
        Validar operação de multiplicação pelo operador "*",
        Bloquear resultado negativo no meotodo Multiplicar(),
        Validar operação de dividisão pelo metodo Dividir(),
        Validar operação de dividisão pelo operador "/",
        Bloquear divirsor zero,
        Bloquear divirsor negativo,
        Validar operação com sinal de "<",
        Validar operação com sinal de ">",
        Validar operação de igualdade / desigualdade,
        Verificar a formatação de saida.
*/

public class Dinheiro : IEquatable<Dinheiro>
{
    private decimal Valor { get; set; }

    protected Dinheiro () {}

    /*
        o construtor recebe o valor em decimal
        ele verifica se é positivo - negativos devem ser bloqueados
        e arredonda para 2 casas decimais.
    */
    public Dinheiro(decimal valor)
    {
        if(valor < 0)
            throw new ArgumentException("Valor não pode ser negativo");
        
        Valor = decimal.Round(valor, 2, MidpointRounding.AwayFromZero);
    }

    //getter do valor do dinhiero
    public decimal GetValorDinheiro()
    {
        return Valor;
    }

    /*
        O metodo setter do valor tem tambem 
        a implementação da verificação do dinheiro
        e tambem arredonda para 2 casas decimais
    */
    public void SetValorDinheiro(Dinheiro novoValor)
    {
        //verifica se o valor é positivo já no construtor
        if (!ValidaDinheiro(novoValor))
            throw new ArgumentException("Valor não pode ser negativo");

        //arredonda para 2 casas decimais
        Valor = decimal.Round(novoValor.GetValorDinheiro(), 2, MidpointRounding.AwayFromZero);
    
    }

    /*
        OPERAÇÕES ARITMETICAS BÁSICAS
        Estes metodos fazem contas entre 2, ou mais,
        objetos do tipo Dinheiro diretamente.
    */

    public Dinheiro Somar(Dinheiro outro)
    {
        ValidaDinheiro(outro);

        decimal somaValor = outro.GetValorDinheiro();

        return new Dinheiro(Valor + somaValor);
    }

    public Dinheiro Subtrair(Dinheiro outro)
    {
        ValidaDinheiro(outro);

        var resultado = Valor - outro.GetValorDinheiro();

        if (resultado < 0)
            throw new ArgumentException("Resultado não pode ser negativo");

        return new Dinheiro(resultado);
    }

    public Dinheiro Multiplicar(int quantidade)
    {
        if (quantidade < 0)
            throw new ArgumentException("Quantidade inválida");

        return new Dinheiro(Valor * quantidade);
    }

    public Dinheiro Dividir(int divisor)
    {
        if (divisor <= 0)
            throw new ArgumentException("Divisor inválido");

        return new Dinheiro(Valor / divisor);
    }

    /*
        Esses metodos de comparações conseguem operar
        diretamente entre 2 objetos do tipo Dinheiro
    */

    public bool MaiorQue(Dinheiro outro)
    {
        ValidaDinheiro(outro);
        return Valor > outro.GetValorDinheiro();
    }

    public bool MenorQue(Dinheiro outro)
    {
        ValidaDinheiro(outro);
        return Valor < outro.GetValorDinheiro();
    }

    public bool Igual(Dinheiro outro)
    {
        ValidaDinheiro(outro);
        return Valor == outro.GetValorDinheiro();
    }

    /*
        Os metodos abaixo permitem fazer operações entre 2 
        objetos do tipo dinheiro usando apenas sinais.

        A operação de multiplicação e divisão funcionam apenas
        entre Dinheiro * ou / por um inteiro.
    */

    public static Dinheiro operator +(Dinheiro a, Dinheiro b)
        => a.Somar(b);

    public static Dinheiro operator -(Dinheiro a, Dinheiro b)
        => a.Subtrair(b);

    public static Dinheiro operator *(Dinheiro a, int b)
        => a.Multiplicar(b);

    public static Dinheiro operator /(Dinheiro a, int b)
        => a.Dividir(b);

    public static bool operator >(Dinheiro a, Dinheiro b)
        => a.MaiorQue(b);

    public static bool operator <(Dinheiro a, Dinheiro b)
        => a.MenorQue(b);

    public static bool operator ==(Dinheiro a, Dinheiro b)
        => Equals(a, b);

    public static bool operator !=(Dinheiro a, Dinheiro b)
        => !Equals(a, b);


    //funcoes utilitarias de bibliotecas inclusas

    public bool Equals(Dinheiro? other)
    {
        if (other is null) return false;
        return Valor == other.Valor;
    }

    public override bool Equals(object? obj)
        => obj is Dinheiro other && Equals(other);

    public override int GetHashCode()
        => Valor.GetHashCode();

    // =========================
    // UTILIDADES
    // =========================

    //formata saida com R$
    public override string ToString()
        => $"R$ {Valor:N2}";

    /*
        verifica que o valor inseriodo não é vazio, 
        nem igual ou menor que zero
    */
    private bool ValidaDinheiro(Dinheiro dinheiro)
    {
        if (dinheiro is null || dinheiro.GetValorDinheiro() < 0)
            throw new ArgumentNullException(nameof(dinheiro));

        return true;
    }
}