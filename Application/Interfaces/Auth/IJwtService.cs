using CarStoreManager.Domain.Entities;

namespace CarStoreManager.Application.Interfaces;

public interface IJwtService
{
    string GerarToken(Usuario usuario);
}