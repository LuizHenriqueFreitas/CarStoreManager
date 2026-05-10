using CarStoreManager.Domain.Base;

namespace Oficina.Domain.Entities;

public class ItemNotaFiscal : Entity
{
    public Guid NotaFiscalId { get; private set; }

    public NotaFiscal NotaFiscal { get; private set; } = null!;

    public Guid ComponenteId { get; private set; }

    public Componente Componente { get; private set; } = null!;

    public int Quantidade { get; private set; }

    public decimal ValorUnitario { get; private set; }

    public decimal ValorTotal { get; private set; }

    // FISCAL
    public string CFOP { get; private set; } = null!;

    public string CST { get; private set; } = null!;

    public string CSOSN { get; private set; } = null!;

    public decimal AliquotaICMS { get; private set; }

    public decimal ValorICMS { get; private set; }

    protected ItemNotaFiscal() {}

    public ItemNotaFiscal(
        Guid notaFiscalId,
        Guid componenteId,
        int quantidade,
        decimal valorUnitario,
        string cfop,
        string cst,
        string csosn,
        decimal aliquotaICMS,
        decimal valorICMS)
    {
        NotaFiscalId = notaFiscalId;
        ComponenteId = componenteId;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
        ValorTotal = quantidade * valorUnitario;
        CFOP = cfop;
        CST = cst;
        CSOSN = csosn;
        AliquotaICMS = aliquotaICMS;
        ValorICMS = valorICMS;
    }
}