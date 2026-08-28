using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities.Integracoes;

/// <summary>
/// Rastreia a publicação de um produto local (VeiculoVenda, Componente ou
/// VeiculoConsignacao) como anúncio no Mercado Livre. Chaveado de forma
/// genérica (EntidadeTipo/EntidadeId), igual ao padrão já usado por Foto —
/// evita três colunas de FK para três tipos de produto diferentes.
/// </summary>
public class AnuncioMercadoLivre : Entity
{
    public string EntidadeTipo { get; private set; } = null!;
    public Guid EntidadeId { get; private set; }

    public string ItemIdML { get; private set; } = "";
    public StatusAnuncioMercadoLivre Status { get; private set; }
    public decimal UltimoPrecoSincronizado { get; private set; }
    public DateTime? DataUltimaSincronizacao { get; private set; }
    public string? UltimoErro { get; private set; }

    protected AnuncioMercadoLivre() { }

    public AnuncioMercadoLivre(string entidadeTipo, Guid entidadeId)
    {
        if (string.IsNullOrWhiteSpace(entidadeTipo))
            throw new ArgumentException("Tipo de entidade é obrigatório.", nameof(entidadeTipo));
        if (entidadeId == Guid.Empty)
            throw new ArgumentException("Id da entidade é obrigatório.", nameof(entidadeId));

        EntidadeTipo = entidadeTipo;
        EntidadeId = entidadeId;
        Status = StatusAnuncioMercadoLivre.Rascunho;
    }

    public void MarcarComoPublicado(string itemIdML, decimal preco)
    {
        if (string.IsNullOrWhiteSpace(itemIdML))
            throw new ArgumentException("ItemIdML é obrigatório.", nameof(itemIdML));

        ItemIdML = itemIdML;
        UltimoPrecoSincronizado = preco;
        Status = StatusAnuncioMercadoLivre.Publicado;
        DataUltimaSincronizacao = DateTime.UtcNow;
        UltimoErro = null;
    }

    public void MarcarComoPausado()
    {
        Status = StatusAnuncioMercadoLivre.Pausado;
        DataUltimaSincronizacao = DateTime.UtcNow;
    }

    public void MarcarComoEncerrado()
    {
        Status = StatusAnuncioMercadoLivre.Encerrado;
        DataUltimaSincronizacao = DateTime.UtcNow;
    }

    public void RegistrarErro(string mensagem)
    {
        Status = StatusAnuncioMercadoLivre.ErroPublicacao;
        UltimoErro = mensagem;
        DataUltimaSincronizacao = DateTime.UtcNow;
    }

    public void AtualizarPrecoSincronizado(decimal preco)
    {
        UltimoPrecoSincronizado = preco;
        DataUltimaSincronizacao = DateTime.UtcNow;
    }

    /// <summary>Marca como vendido — chamado pelo fluxo de sincronização de vendas.</summary>
    public void MarcarComoVendido()
    {
        Status = StatusAnuncioMercadoLivre.Encerrado;
        DataUltimaSincronizacao = DateTime.UtcNow;
    }
}
