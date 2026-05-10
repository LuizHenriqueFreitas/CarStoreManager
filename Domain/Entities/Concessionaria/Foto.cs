// Domain/Entities/Foto.cs
using CarStoreManager.Domain.Base;

namespace CarStoreManager.Domain.Entities;

public class Foto : Entity
{
    public Guid VeiculoId { get; private set; }
    public string Url { get; private set; } = null!;
    public int Ordem { get; private set; }

    protected Foto() { }

    public Foto(Guid veiculoId, string url, int ordem)
    {
        VeiculoId = veiculoId;
        Url = url;
        Ordem = ordem;
    }

    public void AtualizarOrdem(int novaOrdem) => Ordem = novaOrdem;
}