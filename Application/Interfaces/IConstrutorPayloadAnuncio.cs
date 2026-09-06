using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

namespace CarStoreManager.Application.Interfaces;

/// <summary>
/// Monta o payload de POST /items para um tipo de entidade local (Componente,
/// VeiculoVenda, VeiculoConsignacao). Cada implementação sabe resolver os
/// atributos obrigatórios da sua categoria a partir dos dados que o sistema
/// realmente tem — quando não tem, falha localmente com uma mensagem dizendo
/// exatamente o que falta, em vez de deixar a API do ML rejeitar sem contexto
/// (ver item 3 da tarefa: pré-validação antes de publicar).
/// </summary>
public interface IConstrutorPayloadAnuncio
{
    /// <summary>"VeiculoVenda", "Componente" ou "VeiculoConsignacao".</summary>
    bool Aceita(string entidadeTipo);

    /// <param name="categoriaOverride">
    /// Categoria escolhida manualmente pelo operador na tela de anúncios, se
    /// houver — tem prioridade sobre a sugestão automática por domain_discovery.
    /// </param>
    /// <param name="accessToken">Token OAuth válido — necessário pra consultar modalidades disponíveis por conta (available_listing_types).</param>
    /// <param name="mercadoLivreUserId">Id do usuário ML conectado — mesma razão.</param>
    Task<Result<PayloadAnuncioMLDTO>> ConstruirAsync(
        string entidadeTipo,
        Guid entidadeId,
        string? categoriaOverride,
        string accessToken,
        string? mercadoLivreUserId);
}
