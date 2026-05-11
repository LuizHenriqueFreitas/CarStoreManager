using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.ValueObjects;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities.Concessionaria;

public class PropostaVenda : Entity
{
    /// <summary>Prazo de validade automática da proposta (lazy expire).</summary>
    public const int PrazoValidadeDias = 7;

    public Guid VendedorId { get; private set; }
    public Guid VeiculoVendaId { get; private set; }
    public Guid ClienteId { get; private set; }

    public Dinheiro ValorBase { get; private set; } = null!;
    public Percentual Desconto { get; private set; } = null!;
    public Dinheiro ValorFinal { get; private set; } = null!;
    public Dinheiro Entrada { get; private set; } = null!;

    public DateTime DataCriacao { get; private set; }
    public StatusPropostaVenda Status { get; private set; }

    /// <summary>Modo de pagamento escolhido pelo cliente.</summary>
    public ModoPagamento ModoPagamento { get; private set; } = ModoPagamento.NaoDefinido;

    // === Financiamento (preenchidos quando ModoPagamento = Financiamento) ===

    public DateTime? DataSolicitacaoFinanciamento { get; private set; }
    public DateTime? DataRespostaFinanciadora { get; private set; }
    public int? ParcelasFinanciamento { get; private set; }
    public Dinheiro? ValorParcela { get; private set; }
    public decimal? TaxaJurosMensal { get; private set; }
    public string? ObservacoesFinanciamento { get; private set; }

    // === Auditoria de transições terminais ===
    public string? MotivoRejeicao { get; private set; }
    public string? MotivoCancelamento { get; private set; }
    public DateTime? DataAprovacao { get; private set; }

    protected PropostaVenda() { }

    public PropostaVenda(
        Guid vendedorId,
        Guid veiculoVendaId,
        Guid clienteId,
        decimal valorBase,
        decimal desconto)
    {
        VendedorId = vendedorId;
        VeiculoVendaId = veiculoVendaId;
        ClienteId = clienteId;
        ValorBase = new Dinheiro(valorBase);
        Desconto = new Percentual(desconto);
        Entrada = new Dinheiro(0);
        AplicarDesconto(desconto);

        DataCriacao = DateTime.UtcNow;
        Status = StatusPropostaVenda.Criada;
    }

    /* ================================
        getters legados
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

    /// <summary>Valor que efetivamente será financiado (valor final - entrada).</summary>
    public decimal ValorLiquidoFinanciamento()
        => Math.Max(0m, ValorFinal.GetValorDinheiro() - Entrada.GetValorDinheiro());

    /* =====================================
        ajustes financeiros
     =====================================*/

    public void AplicarDesconto(decimal desconto)
    {
        BloquearSeTerminal();
        Desconto = new Percentual(desconto);
        ValorFinal = CalcularValorFinal();
    }

    private Dinheiro CalcularValorFinal()
    {
        var valorDesconto = Desconto.CalcularDescontoValor(ValorBase);
        return ValorBase.Subtrair(valorDesconto);
    }

    public void AtualizarEntrada(decimal entrada)
    {
        BloquearSeTerminal();
        if (entrada < 0)
            throw new ArgumentException("Entrada não pode ser negativa.", nameof(entrada));
        if (entrada > ValorFinal.GetValorDinheiro())
            throw new ArgumentException("Entrada não pode ser maior que o valor final.");

        Entrada = new Dinheiro(entrada);
    }

    public void DefinirModoPagamento(ModoPagamento modo)
    {
        BloquearSeTerminal();
        ModoPagamento = modo;
    }

    /* =======================================
        Fluxo de financiamento
     ========================================*/

    /// <summary>
    /// Marca a proposta como aguardando retorno da financiadora. O envio
    /// efetivo do e-mail é responsabilidade do service (a entidade não conhece
    /// I/O). Pré-requisito: ModoPagamento = Financiamento.
    /// </summary>
    public void SolicitarFinanciamento()
    {
        BloquearSeTerminal();
        if (ModoPagamento != ModoPagamento.Financiamento)
            throw new InvalidOperationException(
                "Só é possível solicitar financiamento quando o modo de pagamento é Financiamento.");
        if (Entrada.GetValorDinheiro() < 0)
            throw new InvalidOperationException("Defina o valor da entrada antes de solicitar financiamento.");

        Status = StatusPropostaVenda.AguardandoFinanciadora;
        DataSolicitacaoFinanciamento = DateTime.UtcNow;
    }

    /// <summary>
    /// Admin recebe retorno da financiadora (e-mail/telefone) e digita os
    /// termos propostos. Avança o status para o cliente decidir.
    /// </summary>
    public void RegistrarRespostaFinanciadora(
        int parcelas,
        decimal valorParcela,
        decimal taxaJurosMensal,
        string? observacoes = null)
    {
        BloquearSeTerminal();
        if (Status != StatusPropostaVenda.AguardandoFinanciadora)
            throw new InvalidOperationException(
                $"Só é possível registrar resposta da financiadora no status AguardandoFinanciadora (atual: {Status}).");
        if (parcelas <= 0)
            throw new ArgumentException("Número de parcelas deve ser positivo.", nameof(parcelas));
        if (valorParcela <= 0)
            throw new ArgumentException("Valor da parcela deve ser positivo.", nameof(valorParcela));
        if (taxaJurosMensal < 0)
            throw new ArgumentException("Taxa de juros não pode ser negativa.", nameof(taxaJurosMensal));

        ParcelasFinanciamento = parcelas;
        ValorParcela = new Dinheiro(valorParcela);
        TaxaJurosMensal = taxaJurosMensal;
        ObservacoesFinanciamento = observacoes?.Trim();
        DataRespostaFinanciadora = DateTime.UtcNow;
        Status = StatusPropostaVenda.PropostaFinanciadoraRecebida;
    }

    /* =======================================
        Transições principais
     ========================================*/

    /// <summary>
    /// Cliente aprova a proposta. Pré-requisitos:
    /// - À vista: status Criada + ModoPagamento ≠ NaoDefinido
    /// - Financiamento: status PropostaFinanciadoraRecebida
    /// </summary>
    public void Aprovar()
    {
        BloquearSeTerminal();

        if (ModoPagamento == ModoPagamento.NaoDefinido)
            throw new InvalidOperationException("Defina o modo de pagamento antes de aprovar.");

        if (ModoPagamento == ModoPagamento.Financiamento)
        {
            if (Status != StatusPropostaVenda.PropostaFinanciadoraRecebida)
                throw new InvalidOperationException(
                    "Para financiamento, é preciso ter resposta da financiadora antes da aprovação.");
        }
        else
        {
            if (Status != StatusPropostaVenda.Criada)
                throw new InvalidOperationException(
                    $"Proposta só pode ser aprovada no status Criada (atual: {Status}).");
        }

        Status = StatusPropostaVenda.Aprovada;
        DataAprovacao = DateTime.UtcNow;
    }

    public void Rejeitar(string motivo)
    {
        BloquearSeTerminal();
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Motivo da rejeição é obrigatório.", nameof(motivo));
        Status = StatusPropostaVenda.Rejeitada;
        MotivoRejeicao = motivo.Trim();
    }

    public void Cancelar(string motivo)
    {
        BloquearSeTerminal();
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Motivo do cancelamento é obrigatório.", nameof(motivo));
        Status = StatusPropostaVenda.Cancelada;
        MotivoCancelamento = motivo.Trim();
    }

    /// <summary>Avança para AguardandoVistoria (apenas a partir de Aprovada).</summary>
    public void IniciarVistoria()
    {
        BloquearSeTerminal();
        if (Status != StatusPropostaVenda.Aprovada)
            throw new InvalidOperationException(
                $"Vistoria só pode iniciar a partir do status Aprovada (atual: {Status}).");
        Status = StatusPropostaVenda.AguardandoVistoria;
    }

    /// <summary>
    /// Registra resultado da vistoria. Se aprovada, avança para VistoriaConcluida;
    /// se reprovada, volta para Aprovada (admin precisa lançar nova vistoria).
    /// </summary>
    public void RegistrarResultadoVistoria(bool vistoriaAprovada)
    {
        BloquearSeTerminal();
        if (Status != StatusPropostaVenda.AguardandoVistoria)
            throw new InvalidOperationException(
                $"Resultado de vistoria só vale no status AguardandoVistoria (atual: {Status}).");

        Status = vistoriaAprovada
            ? StatusPropostaVenda.VistoriaConcluida
            : StatusPropostaVenda.Aprovada;
    }

    /// <summary>Avança para AguardandoAssinaturaTermo (a partir de VistoriaConcluida).</summary>
    public void EnviarTermoParaAssinatura()
    {
        BloquearSeTerminal();
        if (Status != StatusPropostaVenda.VistoriaConcluida)
            throw new InvalidOperationException(
                $"Termo só pode ser enviado para assinatura após vistoria concluída (atual: {Status}).");
        Status = StatusPropostaVenda.AguardandoAssinaturaTermo;
    }

    /// <summary>Marca proposta como concluída (terminal feliz) após termo assinado.</summary>
    public void Concluir()
    {
        if (Status != StatusPropostaVenda.AguardandoAssinaturaTermo)
            throw new InvalidOperationException(
                $"Proposta só pode ser concluída no status AguardandoAssinaturaTermo (atual: {Status}).");
        Status = StatusPropostaVenda.Concluida;
    }

    /// <summary>
    /// Lazy expiration: chamado pelo service na leitura. Marca como expirada
    /// se passou do prazo e não está em estado terminal nem já avançou para
    /// Aprovada (que não deve mais expirar — virou compromisso).
    /// </summary>
    public bool TentarExpirar()
    {
        if (EhTerminal()) return false;
        // Após aprovação do cliente vira compromisso — não expira mais.
        if (Status is StatusPropostaVenda.Aprovada
            or StatusPropostaVenda.AguardandoVistoria
            or StatusPropostaVenda.VistoriaConcluida
            or StatusPropostaVenda.AguardandoAssinaturaTermo) return false;
        if (DateTime.UtcNow - DataCriacao < TimeSpan.FromDays(PrazoValidadeDias)) return false;

        Status = StatusPropostaVenda.Expirada;
        return true;
    }

    public bool EhTerminal()
        => Status is StatusPropostaVenda.Concluida
            or StatusPropostaVenda.Rejeitada
            or StatusPropostaVenda.Cancelada
            or StatusPropostaVenda.Expirada;

    private void BloquearSeTerminal()
    {
        if (EhTerminal())
            throw new InvalidOperationException(
                $"Operação não permitida: proposta está em estado terminal ({Status}).");
    }

    // === stubs legados — métodos antigos do código de financiamento "todo" ===

    public void GerarFinanciamento(decimal valorBase, int parcelas, decimal entrada)
    {
        // Mantido por retrocompatibilidade — não use; prefira
        // SolicitarFinanciamento + RegistrarRespostaFinanciadora.
        throw new NotSupportedException(
            "Use DefinirModoPagamento(Financiamento) + SolicitarFinanciamento + RegistrarRespostaFinanciadora.");
    }

    public void RemoverFinanciamento()
    {
        BloquearSeTerminal();
        ModoPagamento = ModoPagamento.NaoDefinido;
        ParcelasFinanciamento = null;
        ValorParcela = null;
        TaxaJurosMensal = null;
        ObservacoesFinanciamento = null;
        DataSolicitacaoFinanciamento = null;
        DataRespostaFinanciadora = null;
        if (Status is StatusPropostaVenda.AguardandoFinanciadora
            or StatusPropostaVenda.PropostaFinanciadoraRecebida)
        {
            Status = StatusPropostaVenda.Criada;
        }
    }
}
