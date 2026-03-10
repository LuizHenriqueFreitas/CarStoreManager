using System.ComponentModel.DataAnnotations;
using CarStoreManager.Domain.ValueObjects;
using SistemaEmpresa.Domain.Base;

namespace SistemaEmpresa.Domain.Entities.Concessionaria
{
    public class Vendedor : Entity
    {
        public string Nome { get; private set; } = null!;

        public FuncionarioData Cpf { get; private set; } = null!;

        public bool Ativo { get; private set; } = true;

        public Vendedor() { }

        public Vendedor(string nome, FuncionarioData cpf)
        {
            this.Nome = nome;
            this.Cpf = cpf;
        }
    }
}