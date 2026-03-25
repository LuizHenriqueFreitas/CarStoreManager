using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Concessionaria;

public class VeiculoVenda : Entity
{
    public string Marca { get; private set; } = null!;
    public string Modelo { get; private set; } = null!;
    public string Cor { get; private set; } = null!;

    public Ano Ano { get; private set; } = null!;
    public Quilometragem Quilometragem { get; private set; } = null!;
    public PlacaVeiculo Placa { get; private set; } = null!;

    public EstadoConservacao Estado { get; private set; }
    public DisponibilidadeVeiculo Disponibilidade { get; private set; }

    public Dinheiro Valor { get; private set; } = null!;

    protected VeiculoVenda() { }

    public VeiculoVenda(
        string marca,
        string modelo,
        string cor,
        Ano ano,
        Quilometragem quilometragem,
        PlacaVeiculo placa,
        EstadoConservacao estado,
        Dinheiro valor)
    {
        AlterarMarca(marca);
        AlterarModelo(modelo);
        AlterarCor(cor);

        Ano = ano;
        Quilometragem = quilometragem;
        Placa = placa;
        Estado = estado;
        Disponibilidade = DisponibilidadeVeiculo.Disponivel;
        Valor = valor;
    }

    // =========================
    // GETTERS
    // =========================

    public int GetAno() => Ano.Valor;
    public int GetQuilometragem() => Quilometragem.Valor;
    public string GetPlaca() => Placa.ToString();
    public decimal GetValor() => Valor.Valor;

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

    public void AtualizarValor(Dinheiro novoValor)
    {
        if (novoValor.Valor <= 0)
            throw new ArgumentException("Valor inválido");
        Valor = novoValor;
    }

    public void AtualizarQuilometragem(int novaKm)
        => Quilometragem.Atualizar(novaKm);

    public void AlterarEstado(EstadoConservacao estado)
        => Estado = estado;

    public void AlterarDisponibilidade(DisponibilidadeVeiculo disponibilidade)
        => Disponibilidade = disponibilidade;

    public void MarcarComoVendido()
        => Disponibilidade = DisponibilidadeVeiculo.Vendido;

    public void MarcarComoDisponivel()
        => Disponibilidade = DisponibilidadeVeiculo.Disponivel;

    public void AtualizarDados(
        string marca,
        string modelo,
        string cor,
        Dinheiro novoValor,
        DisponibilidadeVeiculo disponibilidade,
        int novaKm,
        EstadoConservacao estado)
    {
        AlterarMarca(marca);
        AlterarModelo(modelo);
        AlterarCor(cor);
        AtualizarValor(novoValor);
        AlterarDisponibilidade(disponibilidade);
        AtualizarQuilometragem(novaKm);
        AlterarEstado(estado);
    }
}