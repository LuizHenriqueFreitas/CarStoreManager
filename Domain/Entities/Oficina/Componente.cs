//classe base para todos os componentes armazenados no estoque da loja, ou que ja passaram pela loja

using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.ValueObjects;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities.Oficina;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de Componente.cs

    Esta classe tem testes automaticos implementados para:
        Nada ainda
*/

public class Componente : Entity
{
    public string Nome { get; private set; } = null!;
    public string Modelo { get; private set; } = null!;
    public SistemaComponente Sistema { get; private set; }

    public Dinheiro Valor { get; private set; } = null!;

    public int QuantidadeEstoque { get; private set; }
    public int EstoqueMinimo { get; private set; }

    protected Componente() { }

    /*
        construtor valida que
        quantidadeInicial e estoqueMinimo
        sejam maiores ou iguais a 0
    */
    public Componente(
        string nome,
        string modelo,
        SistemaComponente sistema,
        decimal valor,
        int quantidadeInicial,
        int estoqueMinimo)
    {
        DefinirNome(nome);
        DefinirModelo(modelo);

        Sistema = sistema;
        Valor = new Dinheiro(valor);

        if (quantidadeInicial < 0)
            throw new ArgumentException("Estoque inicial inválido");

        if (estoqueMinimo < 0)
            throw new ArgumentException("Estoque mínimo inválido");

        QuantidadeEstoque = quantidadeInicial;
        EstoqueMinimo = estoqueMinimo;
    }

    /* ================================
        metodos GETTERS dos atributos
     ================================*/
    public string GetNome() => Nome;
    public string GetModelo() => Modelo;
    public string GetSistema() => Sistema.ToString();
    public decimal GetValor() => Valor.GetValorDinheiro();
    public int GetQuantidade() => QuantidadeEstoque;
    public int GetEstoqueMinimo() => EstoqueMinimo;

    /* =====================================
        metodos SETTERS de cada atributo
        com regras de negocio aplicadas
     =====================================*/

    //verifica que o novo Nome nao seja vazio
    public void DefinirNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome inválido");

        Nome = nome.Trim();
    }

    //verifica que o novo Modelo nao seja vazio
    public void DefinirModelo(string modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo))
            throw new ArgumentException("Modelo inválido");

        Modelo = modelo.Trim();
    }

    //atualiza o valor da peça pelo metodo setter do dinheiro
    public void AtualizarValor(Dinheiro valor)
    {
        Valor.SetValorDinheiro(valor);
    }

    //valida que o novo estoque minimo seja maior ou igual a 0
    public void DefinirEstoqueMinimo(int minimo)
    {
        if (minimo < 0)
            throw new ArgumentException("Estoque mínimo inválido");

        EstoqueMinimo = minimo;
    }

    //Atualiza valor, quantidadeEstoque e quantidadeMinima juntos
    public void AtualizarDadosComponente
    (
        decimal valor,
        int quantidadeEstoque,
        int quantidadeMinima
    )
    {
        AtualizarValor(new Dinheiro(valor));
        QuantidadeEstoque = quantidadeEstoque;
        EstoqueMinimo = quantidadeMinima;
    }

    /* ===========================
        gerenciamento de ESTOQUE
     ============================*/

    /*
        metodo para adicionar estoque valida que
        a quantidade seja maior que 0 
    */
    public void AdicionarEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade inválida");

        QuantidadeEstoque += quantidade;
    }

    /*
        metodo para remover estoque valida que
        a quantidade seja maior que 0 e
        tambem seja menor que a quantidade em estoque
    */
    public void RemoverEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade inválida");

        if (quantidade > QuantidadeEstoque)
            throw new InvalidOperationException("Estoque insuficiente");

        QuantidadeEstoque -= quantidade;
    }

    /*
        metodo bool que retorna 
        se quantidade no estoque é 
        menor ou igual a quantidade minima
    */
    public bool EstoqueBaixo()
    {
        return QuantidadeEstoque <= EstoqueMinimo;
    }

    /*
        metodo bool que verifica se tem estoque
        suficiente para uma OS que retorna 
        se quantidade no estoque é maior 
        ou igual a que a quantidade necessaria
    */
    public bool TemEstoque(int quantidade)
    {
        return QuantidadeEstoque >= quantidade;
    }
}