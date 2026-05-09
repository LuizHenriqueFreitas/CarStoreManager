namespace CarStoreManager.Domain.ValueObjects;

/*
    Neste arquivo temos as implementações de verificações 
    e regras de negócios do VO de Parcelas, como o metodo 
    construtor e GetParcela().

    Testes automaticos implementados para:
        Criar quantidade de parcelas valida,
        Bloquear quantidade de parcelas invalida
*/

public class Parcelas
{
    private int Quantidade { get; set; }

    protected Parcelas () {}

    //metodo construtor
    public Parcelas(int quantidade)
    {   
        //verifica se as parcela estão num espaço entre 0 e 72 meses (6 anos)
        if (quantidade < 0 || quantidade > 72)
            throw new ArgumentException("Parcelas inválidas");

        Quantidade = quantidade;
    }

    //metodo getter que pega o valor da quantidade de parcelas
    public int GetParcelasQuantidade()
    {
        return Quantidade;
    }

    //metodo setter - mesmo que nao tenha uso, esta implementado com validação
    public void SetParcelas(int novaQuant)
    {
        if (novaQuant <= 0 || novaQuant > 72)
            throw new ArgumentException("Parcelas inválidas");

        Quantidade = novaQuant;
    }
}