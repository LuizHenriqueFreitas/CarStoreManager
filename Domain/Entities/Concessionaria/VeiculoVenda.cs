using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Concessionaria;

public class VeiculoVenda : Entity
{
    public string Marca { get; private set; } = null!;
    public string Modelo { get; private set; } = null!;
    public string Cor { get; private set; } = null!;
    public string Motorizacao { get; private set; } = null!;

    public Ano Ano { get; private set; } = null!;
    public Quilometragem Quilometragem { get; private set; } = null!;
    public PlacaVeiculo Placa { get; private set; } = null!;

    public TipoCambio Cambio { get; private set; }
    public TipoCombustivel Combustivel { get; private set; }
    public EstadoConservacao Estado { get; private set; }
    public DisponibilidadeVeiculo Disponibilidade { get; private set; }
    public AcessoriosVeiculo Acessorios { get; private set; }

    public Dinheiro Valor { get; private set; } = null!;

    public List<FotoVeiculo> Fotos { get; private set; } = new();

    protected VeiculoVenda() { }

    public VeiculoVenda(
        string marca,
        string modelo,
        string cor,
        string motorizacao,
        Ano ano,
        Quilometragem quilometragem,
        PlacaVeiculo placa,
        TipoCambio cambio,
        TipoCombustivel combustivel,
        EstadoConservacao estado,
        Dinheiro valor,
        AcessoriosVeiculo acessorios = AcessoriosVeiculo.Nenhum)
    {
        AlterarMarca(marca);
        AlterarModelo(modelo);
        AlterarCor(cor);
        AlterarMotorizacao(motorizacao);

        Ano = ano ?? throw new ArgumentNullException(nameof(ano));
        Quilometragem = quilometragem ?? throw new ArgumentNullException(nameof(quilometragem));
        Placa = placa ?? throw new ArgumentNullException(nameof(placa));

        Cambio = cambio;
        Combustivel = combustivel;
        Estado = estado;
        Disponibilidade = DisponibilidadeVeiculo.Disponivel;
        Valor = valor ?? throw new ArgumentNullException(nameof(valor));
        Acessorios = acessorios;
    }

    // =========================
    // GETTERS
    // =========================

    public int GetAno() => Ano.Valor;
    public int GetQuilometragem() => Quilometragem.Valor;
    public string GetPlaca() => Placa.ToString();
    public decimal GetValor() => Valor.Valor;

    public List<string> GetAcessoriosLista()
    {
        return Enum.GetValues<AcessoriosVeiculo>()
            .Where(a => a != AcessoriosVeiculo.Nenhum && Acessorios.HasFlag(a))
            .Select(a => a.ToString())
            .ToList();
    }

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

    public void AlterarMotorizacao(string motorizacao)
    {
        if (string.IsNullOrWhiteSpace(motorizacao))
            throw new ArgumentException("Motorização inválida");
        Motorizacao = motorizacao.Trim();
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

    public void AdicionarAcessorio(AcessoriosVeiculo acessorio)
        => Acessorios |= acessorio;

    public void RemoverAcessorio(AcessoriosVeiculo acessorio)
        => Acessorios &= ~acessorio;

    public void DefinirAcessorios(AcessoriosVeiculo acessorios)
        => Acessorios = acessorios;

    public void MarcarComoVendido()
        => Disponibilidade = DisponibilidadeVeiculo.Vendido;

    public void MarcarComoDisponivel()
        => Disponibilidade = DisponibilidadeVeiculo.Disponivel;

    // =========================
    // FOTOS
    // =========================

    public void AdicionarFoto(string url)
    {
        var ordem = Fotos.Count;
        Fotos.Add(new FotoVeiculo(Id, url, ordem));
    }

    public void RemoverFoto(Guid fotoId)
    {
        var foto = Fotos.FirstOrDefault(f => f.Id == fotoId)
            ?? throw new InvalidOperationException("Foto não encontrada");

        Fotos.Remove(foto);
        ReordenarFotos();
    }

    public void AtualizarDados(
        string marca,
        string modelo,
        string cor,
        string motorizacao,
        Dinheiro novoValor,
        DisponibilidadeVeiculo disponibilidade,
        int novaKm,
        EstadoConservacao estado,
        TipoCambio cambio,
        TipoCombustivel combustivel,
        AcessoriosVeiculo acessorios)
    {
        AlterarMarca(marca);
        AlterarModelo(modelo);
        AlterarCor(cor);
        AlterarMotorizacao(motorizacao);
        AtualizarValor(novoValor);
        AlterarDisponibilidade(disponibilidade);
        AtualizarQuilometragem(novaKm);
        AlterarEstado(estado);
        Cambio = cambio;
        Combustivel = combustivel;
        DefinirAcessorios(acessorios);
    }

    private void ReordenarFotos()
    {
        for (int i = 0; i < Fotos.Count; i++)
            Fotos[i].AlterarOrdem(i);
    }
}