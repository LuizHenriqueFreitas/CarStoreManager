using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.ValueObjects;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities.Oficina;

public class Componente : Entity
{
    public string Nome { get; private set; } = null!;
    public string Modelo { get; private set; } = null!;
    public SistemaComponente Sistema { get; private set; }

    public Dinheiro Valor { get; private set; } = null!;

    public int QuantidadeEstoque { get; private set; }
    public int EstoqueMinimo { get; private set; }

    protected Componente() { }

    public Componente(
        string nome,
        string modelo,
        SistemaComponente sistema,
        Dinheiro valor,
        int quantidadeInicial,
        int estoqueMinimo)
    {
        DefinirNome(nome);
        DefinirModelo(modelo);

        Sistema = sistema;
        Valor = valor;

        if (quantidadeInicial < 0)
            throw new ArgumentException("Estoque inicial inválido");

        if (estoqueMinimo < 0)
            throw new ArgumentException("Estoque mínimo inválido");

        QuantidadeEstoque = quantidadeInicial;
        EstoqueMinimo = estoqueMinimo;
    }

    // ========================
    // GETERS
    // ========================

    public decimal GetValor()
    {
        return Valor.Valor;
    }

    // =========================
    // MÉTODOS DE NEGÓCIO
    // =========================

    public void DefinirNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome inválido");

        Nome = nome.Trim();
    }

    public void DefinirModelo(string modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo))
            throw new ArgumentException("Modelo inválido");

        Modelo = modelo.Trim();
    }

    public void AtualizarValor(Dinheiro valor)
    {
        Valor = valor;
    }

    public void DefinirEstoqueMinimo(int minimo)
    {
        if (minimo < 0)
            throw new ArgumentException("Estoque mínimo inválido");

        EstoqueMinimo = minimo;
    }

    public void AtualizarDados
    (
        Dinheiro valor,
        int quantidadeEstoque,
        int quantidadeMinima
    )
    {
        AtualizarValor(valor);
        QuantidadeEstoque = quantidadeEstoque;
        EstoqueMinimo = quantidadeMinima;
    }

    // =========================
    // CONTROLE DE ESTOQUE
    // =========================

    public void AdicionarEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade inválida");

        QuantidadeEstoque += quantidade;
    }

    public void RemoverEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade inválida");

        if (quantidade > QuantidadeEstoque)
            throw new InvalidOperationException("Estoque insuficiente");

        QuantidadeEstoque -= quantidade;
    }

    public bool EstoqueBaixo()
    {
        return QuantidadeEstoque <= EstoqueMinimo;
    }

    public bool TemEstoque(int quantidade)
    {
        return QuantidadeEstoque >= quantidade;
    }
}