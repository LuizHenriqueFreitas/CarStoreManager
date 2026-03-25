using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Services.Oficina;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Oficina;

public class OrdemServico : Entity
{
    public Guid VeiculoId { get; private set; }
    public Guid MecanicoId { get; private set; }
    public Guid ClienteId { get; private set; }

    public TipoServico Tipo { get; private set; }

    public string Descricao { get; private set; } = null!;
    public string NumeroPublico { get; private set; } = null!;

    public DateTime DataCriacao { get; private set; }
    public DateTime PrazoEstimado { get; private set; }

    public Dinheiro CustoServico { get; private set; } = null!;
    public Dinheiro ValorTotal { get; private set; } = null!;

    public StatusOrdemServico Status { get; private set; }

    public List<ItemOrdemServico> Itens { get; private set; } = new();
    public List<ChecklistItem> Checklist { get; private set; } = new();

    protected OrdemServico() { }

    public OrdemServico(
        Guid veiculoId,
        Guid mecanicoId,
        Guid clienteId,
        TipoServico tipo,
        string descricao,
        DateTime prazoEstimado,
        Dinheiro custoServico)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição inválida");

        if (prazoEstimado < DateTime.UtcNow)
            throw new ArgumentException("Prazo inválido");

        VeiculoId = veiculoId;
        MecanicoId = mecanicoId;
        ClienteId = clienteId;

        Tipo = tipo;
        Descricao = descricao.Trim();
        PrazoEstimado = prazoEstimado;

        CustoServico = custoServico;

        DataCriacao = DateTime.UtcNow;
        Status = StatusOrdemServico.Pendente;
        NumeroPublico = GerarNumeroPublico();

        RecalcularTotal();
    }

    // ============================
    // GETERS
    // ============================

    public decimal GetCustoServico()
    {
        return CustoServico.Valor;
    }

    public decimal GetValorTotal()
    {
        return ValorTotal.Valor;
    }

    // =========================
    // SETERS
    // =========================

        public void DefinirCliente(Guid clienteId)
    {
        if (clienteId == Guid.Empty)
            throw new ArgumentException("Cliente inválido");

        ClienteId = clienteId;
    }

    public void DefinirVeiculo(Guid veiculoId)
    {
        if (veiculoId == Guid.Empty)
            throw new ArgumentException("Veículo inválido");

        VeiculoId = veiculoId;
    }

    public void SetTipo(TipoServico tipo)
    {
        Tipo = tipo;
    }

    private static string GerarNumeroPublico()
    {
        var bytes = Guid.NewGuid().ToByteArray();
        return Convert.ToHexString(bytes)[..8].ToUpper(); // ex: "A3F2BC91"
    }

    public void GerarChecklistAutomatico()
    {
        if (Checklist.Any(c => c.Origem == OrigemChecklistItem.Automatico))
            return; // já gerado, não duplica

        var itens = ChecklistTemplates.ObterPorTipo(Tipo);
        for (int i = 0; i < itens.Count; i++)
        {
            var item = new ChecklistItem(Id, itens[i], OrigemChecklistItem.Automatico, i + 1);
            Checklist.Add(item);
        }
    }

    public void AdicionarItemChecklist(string descricao)
    {
        if (Status == StatusOrdemServico.Finalizada || Status == StatusOrdemServico.Cancelada)
            throw new InvalidOperationException("Não é possível alterar o checklist desta OS");

        var ordem = Checklist.Count + 1;
        var item = new ChecklistItem(Id, descricao, OrigemChecklistItem.Manual, ordem);
        Checklist.Add(item);
    }

    // =========================
    // ITENS
    // =========================

    public void AdicionarItem(ItemOrdemServico item)
    {
        if (Status == StatusOrdemServico.Finalizada)
            throw new InvalidOperationException("Não é possível alterar itens");

        Itens.Add(item);
        RecalcularTotal();
    }

    public void RemoverItem(Guid itemId)
    {
        var item = Itens.FirstOrDefault(i => i.Id == itemId);

        if (item == null)
            throw new InvalidOperationException("Item não encontrado");

        Itens.Remove(item);
        RecalcularTotal();
    }

            public void AtualizarItem(Guid itemId, int novaQuantidade)
        {
            var item = Itens.FirstOrDefault(i => i.Id == itemId);

            if (item is null)
                throw new InvalidOperationException("Item não encontrado");

            item.AlterarQuantidade(novaQuantidade);

            RecalcularTotal();
        }

    // =========================
    // CÁLCULO
    // =========================

    public void RecalcularTotal()
    {
        var totalItens = Itens
            .Select(i => i.ValorTotal)
            .Aggregate(new Dinheiro(0), (acc, v) => acc.Somar(v));

        ValorTotal = totalItens.Somar(CustoServico);
    }

    // =========================
    // STATUS (FLUXO CONTROLADO)
    // =========================

    public void Iniciar()
    {
        ValidarStatus(StatusOrdemServico.Pendente);
        Status = StatusOrdemServico.EmAndamento;
    }

    public void Finalizar()
    {
        ValidarStatus(StatusOrdemServico.EmAndamento);
        Status = StatusOrdemServico.Finalizada;
    }

    public void Cancelar()
    {
        Status = StatusOrdemServico.Cancelada;
    }

    public void AtualizarStatus(StatusOrdemServico novoStatus)
    {
        if (Status == StatusOrdemServico.Finalizada)
            throw new InvalidOperationException("OS já finalizada");

        if (Status == StatusOrdemServico.Cancelada)
            throw new InvalidOperationException("OS cancelada não pode ser alterada");

        Status = novoStatus;
    }

    private void ValidarStatus(StatusOrdemServico esperado)
    {
        if (Status != esperado)
            throw new InvalidOperationException($"Status inválido: {Status}");
    }
}