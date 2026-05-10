using CarStoreManager.Domain.Base;
using Oficina.Domain.Entities;
using Oficina.Domain.Enums;

public class NotaFiscal : Entity
{
    public TipoNotaFiscal Tipo { get; private set; }

    public string Numero { get; private set; } = null!;

    public string Serie { get; private set; } = null!;

    public string ChaveAcesso { get; private set; } = null!;

    public DateTime DataEmissao { get; private set; }

    // XML ORIGINAL
    public string XmlConteudo { get; private set; } = null!;

    // VALORES
    public decimal ValorProdutos { get; private set; }

    public decimal ValorImpostos { get; private set; }

    public decimal ValorTotal { get; private set; }

    public StatusNotaFiscal Status { get; private set; }

    // FORNECEDOR/CLIENTE
    public Guid PessoaId { get; private set; }

    // RELACIONAMENTOS
    public ICollection<ItemNotaFiscal> Itens { get; private set; }
        = new List<ItemNotaFiscal>();

    protected NotaFiscal() {}

    public NotaFiscal(
        TipoNotaFiscal tipo,
        string numero,
        string serie,
        string chaveAcesso,
        DateTime dataEmissao,
        string xmlConteudo,
        decimal valorProdutos,
        decimal valorImpostos,
        decimal valorTotal,
        Guid pessoaId)
    {
        Tipo = tipo;
        Numero = numero;
        Serie = serie;
        ChaveAcesso = chaveAcesso;
        DataEmissao = dataEmissao;
        XmlConteudo = xmlConteudo;
        ValorProdutos = valorProdutos;
        ValorImpostos = valorImpostos;
        ValorTotal = valorTotal;
        PessoaId = pessoaId;

        Status = StatusNotaFiscal.Pendente;
    }
}