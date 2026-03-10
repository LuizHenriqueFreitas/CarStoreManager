using Domain.ValueObjects;
using SistemaEmpresa.Domain.Base;

namespace SistemaEmpresa.Domain.Entities.Concessionaria
{
    public class Veiculo : Entity
    {
        public string Marca { get; private set; } = null!;

        public string Modelo { get; private set; } = null!;

        public int Ano { get; private set; }

        public string Cor { get; private set; } = null!;

        public int Quilometragem { get; private set; }

        public string Estado { get; private set; } = null!;

        public PlacaVeiculo Placa { get; private set; } = null!;

        public decimal Valor { get; private set; }

        public bool Disponivel { get; private set; } = true;

        public Veiculo() { }

        public Veiculo(string marca, string modelo, int ano, string cor, int km, string estado, PlacaVeiculo placa, decimal valor)
        {
            this.Marca = marca;
            this.Modelo = modelo;
            this.Ano = ano;
            this.Cor = cor;
            this.Quilometragem = km;
            this.Estado = estado;
            this.Placa = placa;
            this.Valor = valor;
        }
    }
}