using CarStoreManager.Domain.Base;

namespace Oficina.Domain.Entities;

public class LoteComponente : Entity
{
    public Guid ComponenteId { get; private set; }

    public Componente Componente { get; private set; } = null!;

    public string NumeroLote { get; private set; } = null!;

    public DateTime DataFabricacao { get; private set; }

    public DateTime? DataValidade { get; private set; }

    public Guid FornecedorId { get; private set; }

    public Fornecedor Fornecedor { get; private set; } = null!;

    public Guid NotaFiscalId { get; private set; }

    public NotaFiscal NotaFiscal { get; private set; } = null!;

    public int QuantidadeRecebida { get; private set; }

    public int QuantidadeDisponivel { get; private set; }

    protected LoteComponente() {}

    public LoteComponente(
        Guid componenteId,
        string numeroLote,
        DateTime dataFabricacao,
        DateTime? dataValidade,
        Guid fornecedorId,
        Guid notaFiscalId,
        int quantidadeRecebida)
    {
        ComponenteId = componenteId;
        NumeroLote = numeroLote;
        DataFabricacao = dataFabricacao;
        DataValidade = dataValidade;
        FornecedorId = fornecedorId;
        NotaFiscalId = notaFiscalId;
        QuantidadeRecebida = quantidadeRecebida;
        QuantidadeDisponivel = quantidadeRecebida;
    }
}