// Classe base para propostas de venda

using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.ValueObjects;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities.Concessionaria;

public class PropostaVenda : Entity
{
    public Guid VendedorId { get; private set; }
    public Guid VeiculoVendaId { get; private set; }
    public Guid ClienteId { get; private set; }

    public Dinheiro ValorBase { get; private set; } = null!;
    public Percentual Desconto { get; private set; } = null!;
    public Dinheiro ValorFinal { get; private set; } = null!;

    public Dinheiro Entrada { get; private set; } = null!;
    public Financiamento Financiamento { get; private set; } = null!;

    public DateTime DataCriacao { get; private set; }
    public StatusPropostaVenda Status { get; private set; }

    protected PropostaVenda() { }

    public PropostaVenda(
        Guid vendedorId,
        Guid veiculoVendaId,
        Guid clienteId,
        Dinheiro valorBase,
        decimal Desconto)
    {
        VendedorId = vendedorId;
        VeiculoVendaId = veiculoVendaId;
        ClienteId = clienteId;

        ValorBase = valorBase;

        CalcularValorFinal();

        DataCriacao = DateTime.UtcNow;
        Status = StatusPropostaVenda.Rascunho;
    }

    // =========================
    // GETERS
    // =========================

    public Guid GetVendedorId() => VendedorId;
    public Guid GetVeiculoId() => VeiculoVendaId;
    public Guid GetClienteId() => ClienteId;
    public decimal GetValorBase() => ValorBase.Valor;
    public decimal GetDesconto() => Desconto.Valor;
    public decimal GetValorFinal() => ValorFinal.Valor;
    public decimal GetEntrada() => Entrada.Valor;
    public decimal GetValorFinanciado() => Financiamento.ValorFinanciado.Valor;
    public int GetParcelas() => Financiamento.Parcelas.Quantidade;
    public DateTime GetDataCriacao() => DataCriacao;
    public string GetStatus() => Status.ToString();

    // =========================
    // REGRAS DE NEGÓCIO - SETERS
    // =========================

    public void AplicarDesconto(Percentual desconto)
    {
        Desconto = desconto;
        CalcularValorFinal();
    }

    private void CalcularValorFinal()
    {
        var valorDesconto = Desconto.CalcularValor(ValorBase);
        ValorFinal = ValorBase.Subtrair(valorDesconto);
    }

    public void DefinirEntrada(Dinheiro entrada)
    {
        if (entrada.Valor > ValorFinal.Valor)
            throw new ArgumentException("Entrada não pode ser maior que o valor final");

        Entrada = entrada;
    }

    public void GerarFinanciamento(Parcelas parcelas)
    {
        if (Entrada is null)
            throw new InvalidOperationException("Defina a entrada antes do financiamento");

        Financiamento = new Financiamento(
            ValorFinal,
            parcelas,
            Entrada
        );
    }

    // =======================
    // STATUS
    // =======================

    public void Aprovar()
    {
        ValidarEstado(StatusPropostaVenda.Rascunho);
        Status = StatusPropostaVenda.Aprovada;
    }

    public void Rejeitar()
    {
        ValidarEstado(StatusPropostaVenda.Rascunho);
        Status = StatusPropostaVenda.Rejeitada;
    }

    public void Cancelar()
    {
        Status = StatusPropostaVenda.Cancelada;
    }

    private void ValidarEstado(StatusPropostaVenda esperado)
    {
        if (Status != esperado)
            throw new InvalidOperationException($"Estado inválido: {Status}");
    }
}