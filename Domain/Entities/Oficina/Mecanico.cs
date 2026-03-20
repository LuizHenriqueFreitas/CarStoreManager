//adicionar campo de especialidade do mecanico

using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Oficina
{
    public class Mecanico : Entity
    {
        public string Nome { get; private set; } = null!;
        public FuncionarioData Cpf { get; private set; } = null!;

        public string Especialidade { get; private set; } = null!;

        public decimal ValorHora { get; private set; }

        public bool Ativo { get; private set; } = true;

        public Mecanico() { }

        public Mecanico(string nome, FuncionarioData cpf, string especialidade, decimal valorHora)
        {
            this.Nome = nome;
            this.Cpf = cpf;
            this.Especialidade = especialidade;
            this.ValorHora = valorHora;
        }
    }
}