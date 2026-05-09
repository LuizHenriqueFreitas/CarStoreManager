using CarStoreManager.Domain.Exceptions;

namespace CarStoreManager.Domain.ValueObjects;

/*
    Neste arquivo temos as implementações de verificações 
    e regras de negócios do VO de CPF, alem dos metodos de:
    GetNumeroCpf() e GetNumeroCpfRaiz() - servem como getters.
    
    Sendo que o primeiro tras o CPF Formatado.
    O segundo tras o CPF sem formatação.

    Testes automaticos implementados para:
        Verificar um CFP valido,
        Bloquear um CPF vazio,
        Bloquear um CPF com menos de 11 caracteres,
        Bloquear um CPF com mais de 11 caracteres,
        Bloquear um CPF com todos os numeros iguais,
        Bloquear um CPF com digito verificador 1 invalido,
        Bloquear um CPF com digito verificador 2 invalido,
        Verificar a remoção de formatação - importante para o processo de verificar validade do cpf,
        Verificar a re-formatação - importante para exibição.
*/


public class Cpf
{
    public string Numero { get; } = null!;

    protected Cpf () {}

    public Cpf(string numero)
    {
        //valida se o cpf existe já no construtor
        if (!Validar(numero))
            /*
                É interessante deixar a exception aqui, pois
                como pode ver na função de validação, ela 
                pode ser dada como falsa em varios momentos diferentes
                se a excessao nao estivesse aqui, ela teria 
                que ser repetida em cada lugar onde a 
                função de validação se torna falsa, estando
                aqui torna o codigo bem menos verboso.
            */
            throw new CpfInvalidoException(ToString());

        Numero = Limpar(numero);//mantem o valor do cpf como sem formatação
    }

    //metodo que formata o valor do CPF corretamente
    public override string ToString()
    {
        return Convert.ToUInt64(Numero).ToString(@"000\.000\.000\-00");
    }

    /*
        acessa o valor do CPF SEM formatado
        usado apenas pelo teste que verifica a desformatação da string
    */
    public string GetNumeroCpf()
    {
        return Numero;
    }

    //metodo que retira a formatação
    private static string Limpar(string cpf)
    {
        return cpf.Replace(".", "")
                  .Replace("-", "")
                  .Trim();
    }

    /*
        metodo de validação com toda a logica necessaria
        incluindo digito verificador
    */
    public bool Validar(string cpf)
    {
        //caso esteja em branco, torna invalido
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        cpf = Limpar(cpf);

        //caso tenha menos ou mais que 11 caracteres, torna invalido - sem formatação
        if (cpf.Length != 11)
            return false;

        //caso seja repetido por completo, torna invalido
        if (cpf.All(c => c == cpf[0]))
            return false;

        //preparando validação dos digitos
        int[] numeros = cpf.Select(c => int.Parse(c.ToString())).ToArray();

        int soma = 0;
        for (int i = 0; i < 9; i++)
            soma += numeros[i] * (10 - i);

        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;

        //caso primeiro digito esteja errado, torna invalido
        if (numeros[9] != digito1)
            return false;

        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += numeros[i] * (11 - i);

        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        /*
            caso a função chegue até aqui, quer dizer que
            1 - nao é uma string vazia,
            2 - tem 11 digitos exatos (sem formatação),
            3 - não são todos iguais,
            4 - o primiero digito esta correto

            sendo assim, caso o segundo digito esteja correto,
            então será valido. Caso contrario, torna invalido
        */
        return numeros[10] == digito2;
    }
}
