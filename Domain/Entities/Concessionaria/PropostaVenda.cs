// Classe base para propostas de venda

using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.ValueObjects;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities.Concessionaria;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de PropostaVenda.cs

    Esta classe tem testes automaticos implementados para:
        Nada ainda
*/

public class PropostaVenda : Entity
{
    public Guid VendedorId { get; private set; }
    public Guid VeiculoVendaId { get; private set; }
    public Guid ClienteId { get; private set; }

    public Dinheiro ValorBase { get; private set; } = null!;
    public Percentual Desconto { get; private set; } = null!;
    public Dinheiro ValorFinal { get; private set; } = null!;
    public Dinheiro Entrada { get; private set; } = null!;

    public DateTime DataCriacao { get; private set; }
    public StatusPropostaVenda Status { get; private set; }

    protected PropostaVenda() { }

    public PropostaVenda(
        Guid vendedorId,
        Guid veiculoVendaId,
        Guid clienteId,
        decimal valorBase,
        decimal desconto )
    {
        VendedorId = vendedorId;
        VeiculoVendaId = veiculoVendaId;
        ClienteId = clienteId;
        ValorBase = new Dinheiro(valorBase);
        Desconto = new Percentual(desconto);
        AplicarDesconto(desconto);

        DataCriacao = DateTime.UtcNow;
        Status = StatusPropostaVenda.Rascunho;
    }

    /* ================================
        metodos GETTERS dos atributos
     ================================*/
    public Guid GetVendedorId() => VendedorId;
    public Guid GetVeiculoId() => VeiculoVendaId;
    public Guid GetClienteId() => ClienteId;
    public decimal GetValorBase() => ValorBase.GetValorDinheiro();
    public decimal GetDesconto() => Desconto.GetDescontoValor();
    public decimal GetValorFinal() => ValorFinal.GetValorDinheiro();
    public decimal GetEntrada() => Entrada!.GetValorDinheiro();
    public DateTime GetDataCriacao() => DataCriacao;
    public string GetStatus() => Status.ToString();

    
    /* =====================================
        metodos SETTERS de cada atributo
        com regras de negocio aplicadas
     =====================================*/
    
    //calcula o valor final ja com desconto aplicado
    public void AplicarDesconto(decimal desconto)
    {
        Desconto = new Percentual(desconto);
        ValorFinal = CalcularValorFinal();
    }

    //metodo que de fato aplica o desconto no valor base
    private Dinheiro CalcularValorFinal()
    {
        var valorDesconto = Desconto.CalcularDescontoValor(ValorBase);
        return ValorBase.Subtrair(valorDesconto);
    }

    /*
        metodo que atualiza o valor da entrada
        valida que a nova entrada seja menor que o valor final
    */
    public void AtualizarEntrada(decimal entrada)
    {
        if (entrada > ValorFinal.GetValorDinheiro())
            throw new ArgumentException("Entrada não pode ser maior que o valor final");

        Entrada = new Dinheiro(entrada);
    }

    //metodo que cria um novo financiamento para a proposta
    public void GerarFinanciamento(decimal valorBase, int parcelas, decimal entrada)
    {
        //implementar
    }

    /*
        metodo para remover o financiamento da proposta
        retornar a ser a vista
    */
    public void RemoverFinanciamento()
    {
        //implementar
    }

    /* =======================================
        Gerenciamento de Status
        rascunho > aprovado OU rejeitado
        -> para ser aprovado ou rejeitado
        o estado anterior deve ser "rascunho"
        pode ser cancelado a qualquer momento
     ========================================*/
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

    //validação do estado da proposta
    private void ValidarEstado(StatusPropostaVenda esperado)
    {
        if (Status != esperado)
            throw new InvalidOperationException($"Estado inválido: {Status}");
    }
}