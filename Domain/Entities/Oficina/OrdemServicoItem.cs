using SistemaEmpresa.Domain.Base;

namespace SistemaEmpresa.Domain.Entities.Oficina
{
    public class OrdemServicoItem : Entity
    {
        public Guid OrdemServicoId { get; private set; }

        public Guid PecaId { get; private set; }

        public int Quantidade { get; private set; }

        public decimal ValorUnitario { get; private set; }

        public decimal ValorTotal { get; private set; }

        public OrdemServicoItem() { }

        public OrdemServicoItem(Guid ordemId, Guid pecaId, int quantidade, decimal valorUnitario)
        {
            this.OrdemServicoId = ordemId;
            this.PecaId = pecaId;
            this.Quantidade = quantidade;
            this.ValorUnitario = valorUnitario;

            this.ValorTotal = quantidade * valorUnitario;
        }
    }
}