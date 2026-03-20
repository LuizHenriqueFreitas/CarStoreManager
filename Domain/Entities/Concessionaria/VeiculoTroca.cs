//tem que revisar a estrutura desta entidade

namespace CarStoreManager.Domain.Entities.Concessionaria
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