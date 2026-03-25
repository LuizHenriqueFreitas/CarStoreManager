using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities;

public class Admin : Usuario
{
    protected Admin() { }

    public Admin(
        string nome,
        Email email,
        Telefone telefone,
        string senhaHash)
        : base(nome, email, telefone, senhaHash, RoleUsuario.Admin)
    { }
}