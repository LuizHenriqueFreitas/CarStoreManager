using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities.Concessionaria;

/// <summary>
/// Registro de um test drive agendado/realizado — liga um veículo do salão a um
/// cliente e a um vendedor. Escopo enxuto (sem calendário/lembretes automáticos).
/// Ver docs/redesign/01-arquitetura-informacao.md §4.2.
/// </summary>
public class TestDrive : Entity
{
    public Guid VeiculoVendaId { get; private set; }
    public Guid ClienteId { get; private set; }
    public Guid VendedorId { get; private set; }
    public DateTime DataHora { get; private set; }
    public StatusTestDrive Status { get; private set; } = StatusTestDrive.Agendado;
    public string? Observacao { get; private set; }

    protected TestDrive() { }

    public TestDrive(Guid veiculoVendaId, Guid clienteId, Guid vendedorId, DateTime dataHora, string? observacao = null)
    {
        if (veiculoVendaId == Guid.Empty) throw new ArgumentException("Veículo inválido");
        if (clienteId == Guid.Empty) throw new ArgumentException("Cliente inválido");
        if (vendedorId == Guid.Empty) throw new ArgumentException("Vendedor inválido");

        VeiculoVendaId = veiculoVendaId;
        ClienteId = clienteId;
        VendedorId = vendedorId;
        DataHora = dataHora;
        SetObservacao(observacao);
    }

    public void SetObservacao(string? obs)
        => Observacao = string.IsNullOrWhiteSpace(obs) ? null : obs.Trim();

    public void Reagendar(DateTime novaDataHora)
    {
        if (Status is StatusTestDrive.Realizado or StatusTestDrive.Cancelado)
            throw new InvalidOperationException("Test drive já finalizado não pode ser reagendado.");
        DataHora = novaDataHora;
        Status = StatusTestDrive.Agendado;
    }

    public void MarcarRealizado() => Status = StatusTestDrive.Realizado;
    public void Cancelar() => Status = StatusTestDrive.Cancelado;
    public void MarcarNaoCompareceu() => Status = StatusTestDrive.NaoCompareceu;

    public void TrocarVendedor(Guid novoVendedorId)
    {
        if (novoVendedorId == Guid.Empty) throw new ArgumentException("Vendedor inválido");
        VendedorId = novoVendedorId;
    }
}
