using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Oficina;

public class VeiculoCliente : Entity
{
    public Guid ClienteId { get; private set; }

    public string Marca { get; private set; } = null!;
    public string Modelo { get; private set; } = null!;
    public string Cor { get; private set; } = null!;
    public Ano Ano { get; private set; } = null!;

    public List<OrdemServico> HistoricoServicos { get; private set; } = new();

    protected VeiculoCliente() { }

    public VeiculoCliente(
        Guid clienteId,
        string marca,
        string modelo,
        string cor,
        Ano ano)
    {
        if (clienteId == Guid.Empty)
            throw new ArgumentException("Cliente inválido");

        ClienteId = clienteId;

        AlterarMarca(marca);
        AlterarModelo(modelo);
        AlterarCor(cor);

        Ano = ano;
    }

    // =========================
    // GETTERS
    // =========================

    public int GetAno() => Ano.Valor;

    public string GetDescricao() => $"{Marca} {Modelo} {Ano.Valor}";

    // =========================
    // MÉTODOS DE NEGÓCIO
    // =========================

    public void AlterarMarca(string marca)
    {
        if (string.IsNullOrWhiteSpace(marca))
            throw new ArgumentException("Marca inválida");
        Marca = marca.Trim();
    }

    public void AlterarModelo(string modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo))
            throw new ArgumentException("Modelo inválido");
        Modelo = modelo.Trim();
    }

    public void AlterarCor(string cor)
    {
        if (string.IsNullOrWhiteSpace(cor))
            throw new ArgumentException("Cor inválida");
        Cor = cor.Trim();
    }

    public void AtualizarDados(string marca, string modelo, string cor, Ano ano)
    {
        AlterarMarca(marca);
        AlterarModelo(modelo);
        AlterarCor(cor);
        Ano = ano;
    }
}