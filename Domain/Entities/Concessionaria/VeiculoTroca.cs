using Domain.ValueObjects;
using SistemaEmpresa.Domain.Base;

namespace SistemaEmpresa.Domain.Entities.Concessionaria
{
    public class VeiculoTroca : Entity
    {
        public Guid PropostaVendaId { get; private set; }

        public string Marca { get; private set; } = null!;

        public string Modelo { get; private set; } = null!;

        public PlacaVeiculo Placa { get; private set; } = null!;

        public int Ano { get; private set; }

        public int Quilometragem { get; private set; }

        public decimal ValorAvaliado { get; private set; }

        public VeiculoTroca() { }

        public VeiculoTroca(Guid propostaId, string marca, string modelo, PlacaVeiculo placa, int ano, int km, decimal valor)
        {
            this.PropostaVendaId = propostaId;
            this.Marca = marca;
            this.Modelo = modelo;
            this.Placa = placa;
            this.Ano = ano;
            this.Quilometragem = km;
            this.ValorAvaliado = valor;
        }
    }
}