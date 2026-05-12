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

    /// <summary>
    /// Calcula sugestões automáticas de componentes que podem corresponder
    /// a este item da NF-e. Ranqueia por score: matches em PartNumber/OEM
    /// (alta confiança) → SKU → similaridade de Nome → NCM (baixa).
    /// Retorna no máximo 5 candidatos com score &gt; 0.
    /// </summary>
    Task<Result<IEnumerable<SugestaoComponenteDTO>>> SugerirComponentesAsync(Guid itemId);

    /// <summary>
    /// Cria componente novo a partir do item de NF e já vincula. Útil quando
    /// o admin não acha a peça no cadastro e quer registrá-la imediatamente
    /// usando os dados que vieram do XML.
    /// </summary>
    Task<Result<Guid>> CriarComponenteEVincularAsync(Guid itemId, CarStoreManager.Application.DTOs.Oficina.Componente.CriarComponenteDTO componente);
}
