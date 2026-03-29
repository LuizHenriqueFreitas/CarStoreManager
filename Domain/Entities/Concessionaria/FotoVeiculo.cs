//Classe de gerenciamento para as fotos dos veiculos da concessionaria

using CarStoreManager.Domain.Base;

namespace CarStoreManager.Domain.Entities.Concessionaria;

public class FotoVeiculo : Entity
{
    public Guid VeiculoVendaId { get; private set; }
    public string Url { get; private set; } = null!;
    public int Ordem { get; private set; }

    protected FotoVeiculo() { }

    //construtor
    public FotoVeiculo(Guid veiculoVendaId, string url, int ordem)
    {
        //validsções simples
        if (veiculoVendaId == Guid.Empty)
            throw new ArgumentException("Veículo inválido");

        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL da foto inválida");

        if (ordem < 0)
            throw new ArgumentException("Ordem inválida");

        VeiculoVendaId = veiculoVendaId;
        Url = url.Trim();
        Ordem = ordem;
    }

    //Muda a ordem do carrocel
    public void AlterarOrdem(int novaOrdem)
    {
        if (novaOrdem < 0)
            throw new ArgumentException("Ordem inválida");
        Ordem = novaOrdem;
    }
}