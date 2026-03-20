using CarStoreManager.Domain.Base;

namespace CarStoreManager.Domain.Entities.Concessionaria
{
    public class PropostaVenda : Entity
    {
        public Guid VendedorId { get; private set; }

        public Guid VeiculoId { get; private set; }

        public Guid ClienteId { get; private set; }

        public decimal ValorBase { get; private set; }

        public decimal Desconto { get; private set; }

        public decimal ValorFinal { get; private set; }

        public decimal Entrada { get; private set; }

        public decimal ValorFinanciado { get; private set; }

        public int Parcelas { get; private set; }

        public DateTime DataCriacao { get; private set; } = DateTime.Now;

        public string Status { get; private set; } = null!;

        public PropostaVenda() { }

        public PropostaVenda(Guid vendedorId, Guid veiculoId, Guid clienteId, decimal valorBase, decimal desconto, decimal valorFinal, decimal entrada, decimal valorFinanciado, int parcelas, DateTime dataCriacao, string status)
        {
            this.VendedorId = vendedorId;
            this.VeiculoId = veiculoId;
            this.ClienteId = clienteId;
            this.ValorBase = valorBase;
            this.Desconto = desconto;
            this.ValorFinal = valorFinal;
            this.Entrada = entrada;
            this.ValorFinanciado = valorFinanciado;
            this.Parcelas = parcelas;
            this.DataCriacao = dataCriacao;
            this.Status = status;

            this.ValorFinal = valorBase - desconto;
            this.ValorFinanciado = ValorFinal - entrada;
        }

        public void AtualizarStatus(string status)
        {
            this.Status = status;
        }
    }
}