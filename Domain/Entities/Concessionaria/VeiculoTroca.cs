using CarStoreManager.Domain.ValueObjects;
using SistemaEmpresa.Domain.Base;

namespace SistemaEmpresa.Domain.Entities.Concessionaria
{
    public class VeiculoTroca : Veiculo
    {
        public Guid PropostaVendaId { get; private set; }

        public VeiculoTroca() { }

        public VeiculoTroca(Guid propostaId)
        {
            this.PropostaVendaId = propostaId;
        }
    }
}