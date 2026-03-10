using SistemaEmpresa.Domain.Base;

namespace SistemaEmpresa.Domain.Entities.Concessionaria
{
    public class PropostaVenda : Entity
    {
        public Guid VendedorId { get; private set; }

        public Guid CarroId { get; private set; }

        public decimal ValorBase { get; private set; }

        public decimal Desconto { get; private set; }

        public decimal ValorFinal { get; private set; }

        public decimal Entrada { get; private set; }

        public decimal ValorFinanciado { get; private set; }

        public int Parcelas { get; private set; }

        public DateTime DataCriacao { get; private set; } = DateTime.Now;

        public PropostaVenda() { }

        public PropostaVenda(Guid vendedorId, Guid carroId, decimal valorBase, decimal desconto, decimal entrada, int parcelas)
        {
            this.VendedorId = vendedorId;
            this.CarroId = carroId;
            this.ValorBase = valorBase;
            this.Desconto = desconto;
            this.Entrada = entrada;
            this.Parcelas = parcelas;

            this.ValorFinal = valorBase - desconto;
            this.ValorFinanciado = ValorFinal - entrada;
        }
    }
}