using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities.Oficina;

/// <summary>
/// Pedido aberto pelo mecânico para o admin quando uma peça necessária
/// não existe no cadastro (nem como equivalente). O admin atende: cadastra
/// o componente novo (ou vincula um existente que ele encontrou), depois
/// libera a OS para continuar — opcionalmente já adicionando como item de
/// Encomenda na própria OS para que o sistema rastreie a chegada.
/// </summary>
public class RequisicaoPecaOS : Entity
{
    public Guid OrdemServicoId { get; private set; }
    public Guid MecanicoId { get; private set; }

    /// <summary>Nome/descrição livre da peça pretendida (ex: "amortecedor traseiro Civic 2018").</summary>
    public string DescricaoPeca { get; private set; } = "";

    /// <summary>
    /// Justificativa técnica/clínica do mecânico (por que essa peça é necessária,
    /// o que ele encontrou no veículo).
    /// </summary>
    public string Justificativa { get; private set; } = "";

    /// <summary>Quantidade aproximada que o mecânico precisa.</summary>
    public int Quantidade { get; private set; }

    public StatusRequisicaoPeca Status { get; private set; }

    public DateTime DataCriacao { get; private set; }
    public DateTime? DataResolucao { get; private set; }
    public Guid? ResolvidaPor { get; private set; }
    public string? ObservacaoAdmin { get; private set; }

    /// <summary>
    /// ComponenteId que o admin associou à requisição (peça cadastrada no sistema).
    /// Pode ser uma peça existente ou uma que ele criou para atender este pedido.
    /// </summary>
    public Guid? ComponenteAtendidoId { get; private set; }

    protected RequisicaoPecaOS() { }

    public RequisicaoPecaOS(
        Guid ordemServicoId,
        Guid mecanicoId,
        string descricaoPeca,
        string justificativa,
        int quantidade)
    {
        if (string.IsNullOrWhiteSpace(descricaoPeca))
            throw new ArgumentException("Descrição da peça é obrigatória.", nameof(descricaoPeca));
        if (string.IsNullOrWhiteSpace(justificativa))
            throw new ArgumentException("Justificativa é obrigatória.", nameof(justificativa));
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser positiva.", nameof(quantidade));

        OrdemServicoId = ordemServicoId;
        MecanicoId = mecanicoId;
        DescricaoPeca = descricaoPeca.Trim();
        Justificativa = justificativa.Trim();
        Quantidade = quantidade;
        Status = StatusRequisicaoPeca.Pendente;
        DataCriacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Admin vinculou um componente (existente ou recém-cadastrado) à requisição.
    /// </summary>
    public void Atender(Guid resolvidaPor, Guid componenteId, string? observacao = null)
    {
        if (Status != StatusRequisicaoPeca.Pendente)
            throw new InvalidOperationException($"Requisição já resolvida (status {Status}).");
        if (componenteId == Guid.Empty)
            throw new ArgumentException("ComponenteId inválido.", nameof(componenteId));

        Status = StatusRequisicaoPeca.Atendida;
        ResolvidaPor = resolvidaPor;
        ComponenteAtendidoId = componenteId;
        ObservacaoAdmin = observacao?.Trim();
        DataResolucao = DateTime.UtcNow;
    }

    public void Rejeitar(Guid resolvidaPor, string motivo)
    {
        if (Status != StatusRequisicaoPeca.Pendente)
            throw new InvalidOperationException($"Requisição já resolvida (status {Status}).");
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Motivo é obrigatório.", nameof(motivo));

        Status = StatusRequisicaoPeca.Rejeitada;
        ResolvidaPor = resolvidaPor;
        ObservacaoAdmin = motivo.Trim();
        DataResolucao = DateTime.UtcNow;
    }
}
