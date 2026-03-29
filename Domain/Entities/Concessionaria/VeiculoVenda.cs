//classe base dos veiculos da concessionaria

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
        Disponibilidade = DisponibilidadeVeiculo.Disponivel;
        Valor = valor ?? throw new ArgumentNullException(nameof(valor));
        Acessorios = acessorios;
    }

    // =========================
    // GETTERS
    // =========================
    public string GetMarca() => Marca;
    public string GetModelo() => Modelo;
    public string GetCor() => Cor;
    public string GetMotorizacao() => Motorizacao; 
    public int GetAno() => Ano.Valor;
    public int GetQuilometragem() => Quilometragem.Valor;
    public string GetPlaca() => Placa.ToString();
    public string GetCambio() => Cambio.ToString();
    public string getCombustivel() => Combustivel.ToString();
    public decimal GetValor() => Valor.Valor;

    public AcessoriosVeiculo GetAcessoriosVeiculo() => Acessorios;

    public List<string> GetAcessoriosLista()
    {
        return Enum.GetValues<AcessoriosVeiculo>()
            .Where(a => a != AcessoriosVeiculo.Nenhum && Acessorios.HasFlag(a))
            .Select(a => a.ToString())
            .ToList();
    }

    // =========================
    // REGRAS DE NEGOCIOS - SETERS
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

    public void AlterarQuilometragem(int novaKm)
        => Quilometragem.Atualizar(novaKm);


    public void AtualizarValor(Dinheiro novoValor)
    {
        if (novoValor.Valor <= 0)
            throw new ArgumentException("Valor inválido");
        Valor = novoValor;
    }

    // acessorios

    public void AdicionarAcessorio(AcessoriosVeiculo acessorio)
        => Acessorios |= acessorio;

    public void RemoverAcessorio(AcessoriosVeiculo acessorio)
        => Acessorios &= ~acessorio;

    public void DefinirAcessorios(AcessoriosVeiculo acessorios)
        => Acessorios = acessorios;

    // status
    public void AlterarDisponibilidade(DisponibilidadeVeiculo disponibilidade)
        => Disponibilidade = disponibilidade;

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
        Dinheiro novoValor,
        DisponibilidadeVeiculo disponibilidade)
    {
        AtualizarValor(novoValor);
        AlterarDisponibilidade(disponibilidade);
    }

    private void ReordenarFotos()
    {
        for (int i = 0; i < Fotos.Count; i++)
            Fotos[i].AlterarOrdem(i);
    }
}