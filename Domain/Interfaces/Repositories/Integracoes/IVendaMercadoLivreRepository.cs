using CarStoreManager.Domain.Entities.Integracoes;

namespace CarStoreManager.Domain.Repositories;

public interface IVendaMercadoLivreRepository : IRepository<VendaMercadoLivre>
{
    /// <summary>Checagem de idempotência — chamada antes de processar qualquer notificação de venda.</summary>
    Task<VendaMercadoLivre?> ObterPorPedidoEItemAsync(string idPedidoPlataforma, string idItemPlataforma);

    /// <summary>Fila de reconciliação manual — vendas confirmadas pelo ML mas com falha ao aplicar localmente.</summary>
    Task<IEnumerable<VendaMercadoLivre>> ObterComErroAsync();
}
