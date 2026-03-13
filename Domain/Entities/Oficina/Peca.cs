using SistemaEmpresa.Domain.Base;

namespace SistemaEmpresa.Domain.Entities.Oficina
{
    public class Peca : Entity
    {
        public string Nome { get; private set; } = null!;

        public string Modelo { get; private set; } = null!;

        public decimal Valor { get; private set; }

        public int QuantidadeEstoque { get; private set; }

        public Peca() { }

        public Peca(string nome, string modelo, decimal valor, int quantidade)
        {
            this.Nome = nome;
            this.Modelo = modelo;
            this.Valor = valor;
            this.QuantidadeEstoque = quantidade;
        }

        public void AtualizarDados(string Nome, string Modelo, decimal Valor, int Quantidade)
        {
            this.Nome = Nome;
            this.Modelo = Modelo;
            this.Valor = Valor;
            this.QuantidadeEstoque = Quantidade;
        }
    }
}