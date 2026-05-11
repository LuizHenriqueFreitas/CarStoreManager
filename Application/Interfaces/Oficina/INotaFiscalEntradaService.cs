using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.NotaFiscal;

namespace CarStoreManager.Application.Interfaces;

public interface INotaFiscalEntradaService
{
    Task<Result<NotaFiscalDTO>> ImportarXmlAsync(string xml);

    Task<Result<IEnumerable<NotaFiscalListaDTO>>> ListarAsync();
    Task<Result<NotaFiscalDTO>> ObterAsync(Guid id);

    Task<Result> VincularComponenteAsync(Guid itemId, Guid componenteId);
    Task<Result> AlterarQuantidadeItemAsync(Guid itemId, int quantidade);

    /// <summary>
    /// Aprova a nota. Cria um <c>LoteComponente</c> por item e dá entrada
    /// no <c>EstoqueComponente</c> de cada componente vinculado.
    /// Operação atômica — falha em qualquer passo desfaz tudo.
    /// </summary>
    Task<Result> AprovarAsync(Guid notaFiscalId);

    Task<Result> RejeitarAsync(Guid notaFiscalId, string motivo);
}
