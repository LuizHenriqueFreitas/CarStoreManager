using SistemaEmpresa.Domain.Base;

namespace SistemaEmpresa.Domain.Entities.Oficina
{
    public class OrdemServico : Entity
    {
        public Guid ClienteId { get; private set; }

        public Guid MecanicoId { get; private set; }

        public Guid VeiculoId { get; private set; }

        public string Descricao { get; private set; } = null!;

        public DateTime DataCriacao { get; private set; } = DateTime.Now;

        public DateTime PrazoEstimado { get; private set; }

        public decimal CustoPecas { get; private set; }

        public decimal CustoServico { get; private set; }

        public decimal ValorTotal { get; private set; }

        public string Status { get; private set; } = null!;

        public OrdemServico() { }

        public OrdemServico(Guid mecanicoId, string descricao, DateTime prazo, decimal custoServico)
        {
            this.MecanicoId = mecanicoId;
            this.Descricao = descricao;
            this.PrazoEstimado = prazo;
            this.CustoServico = custoServico;
        }

        public void CalcularTotal(decimal custoPecas)
        {
            this.CustoPecas = custoPecas;
            this.ValorTotal = CustoServico + CustoPecas;
        }

        public void AtualizarStatus(string status)
        {
            this.Status = status;
        }

    }   
}