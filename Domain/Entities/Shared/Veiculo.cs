using CarStoreManager.Domain.ValueObjects;
using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities
{
    public class Veiculo : Entity
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

        // EF
        protected Veiculo() { }

        public Veiculo(
            Guid id,
            string marca,
            string modelo,
            string cor,
            Ano ano,
            Quilometragem quilometragem,
            PlacaVeiculo placa,
            EstadoConservacao estado,
            DisponibilidadeVeiculo disponibilidade,
            Dinheiro valor)
        {
            Id = id;

            AlterarMarca(marca);
            AlterarModelo(modelo);
            AlterarCor(cor);

            Ano = ano;
            Quilometragem = quilometragem;
            Placa = placa;

            Estado = estado;
            Disponibilidade = disponibilidade;
            Valor = valor;
        }

        // =========================
        // GETERS
        // =========================

        public int GetAno()
        {
            return Ano.Valor;
        }

        public int GetQuilometragem()
        {
            return Quilometragem.Valor;
        }

        public string GetPlaca()
        {
            return Placa.ToString();
        }

        public decimal GetValor()
        {
            return Valor.Valor;
        }


        // =========================
        // MÉTODOS DE NEGÓCIO
        // =========================

        public void AlterarMarca(string marca)
        {
            if (string.IsNullOrWhiteSpace(marca))
                throw new ArgumentException("Marca inválida");

            Marca = marca;
        }

        public void AlterarModelo(string modelo)
        {
            if (string.IsNullOrWhiteSpace(modelo))
                throw new ArgumentException("Modelo inválido");

            Modelo = modelo;
        }

        public void AlterarCor(string cor)
        {
            if (string.IsNullOrWhiteSpace(cor))
                throw new ArgumentException("Cor inválida");

            Cor = cor;
        }

        public void AtualizarValor(Dinheiro novoValor)
        {
            if (novoValor.Valor <= 0)
                throw new ArgumentException("Valor inválido");

            Valor = novoValor;
        }

        public void AlterarDisponibilidade(DisponibilidadeVeiculo disponibilidade)
        {
            Disponibilidade = disponibilidade;
        }

        public void AtualizarQuilometragem(int  novaKm)
        {
            Quilometragem.Atualizar(novaKm);
        }

        public void AlterarEstado(EstadoConservacao estado)
        {
            Estado = estado;
        }

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

        public void MarcarComoVendido() => Disponibilidade = DisponibilidadeVeiculo.Vendido;

        public void MarcarComoDisponivel() => Disponibilidade = DisponibilidadeVeiculo.Disponivel;
    }
}