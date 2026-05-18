// classe de administrador

using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de Admin.cs. Essa classe herda de
    Usuario.cs, ela não aplica nada de novo propriamente dito.

    Esta classe tem testes automaticos implementados para:
        Nada ainda
*/

public class Admin : Usuario
{
    protected Admin() { }

    public Admin(
        string nome,
        string email,
        string telefone,
        string senha)
        : base(nome, email, telefone, senha, RoleUsuario.Admin)
    { }
}