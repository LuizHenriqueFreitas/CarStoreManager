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

    /// <summary>
    /// Corpo técnico original (JSON cru) do último erro do ML — separado de
    /// UltimoErro (mensagem já traduzida) pra poder ser exibido num detalhe
    /// expansível na tela, sem poluir o card do anúncio com JSON por padrão.
    /// </summary>
    public string? UltimoErroDetalheTecnico { get; private set; }

    /// <summary>
    /// Categoria e modalidade de anúncio EFETIVAMENTE usadas na última
    /// publicação — persistidas (em vez de recalculadas a cada vez) pra uma
    /// republicação/atualização não cair numa categoria diferente da
    /// original só porque a sugestão automática mudou de ideia. Vivem aqui
    /// (no anúncio) e não no produto local porque são metadados do lado do
    /// ML, específicos dessa integração — o produto (VeiculoVenda,
    /// Componente...) não precisa saber nada sobre Mercado Livre.
    /// </summary>
    public string? CategoriaML { get; private set; }
    public string? ListingTypeML { get; private set; }

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

    public void MarcarComoPublicado(string itemIdML, decimal preco, string categoriaML, string listingTypeML)
    {
        if (string.IsNullOrWhiteSpace(itemIdML))
            throw new ArgumentException("ItemIdML é obrigatório.", nameof(itemIdML));

        ItemIdML = itemIdML;
        UltimoPrecoSincronizado = preco;
        CategoriaML = categoriaML;
        ListingTypeML = listingTypeML;
        Status = StatusAnuncioMercadoLivre.Publicado;
        DataUltimaSincronizacao = DateTime.UtcNow;
        UltimoErro = null;
        UltimoErroDetalheTecnico = null;
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

    public void RegistrarErro(string mensagem, string? detalheTecnico = null)
    {
        Status = StatusAnuncioMercadoLivre.ErroPublicacao;
        UltimoErro = mensagem;
        UltimoErroDetalheTecnico = detalheTecnico;
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
