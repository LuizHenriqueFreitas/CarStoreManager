using CarStoreManager.Application.DTOs.Sistema.TemplateDocumento;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Geradores.Nucleo;

namespace CarStoreManager.Geradores.Entidades;

/// <summary>
/// Semeia os templates de documento (Configurações → Documentos) via
/// ITemplateDocumentoService.AddAsync. Diferente dos outros geradores, não
/// cria variantes aleatórias duplicadas — são templates reais pensados para
/// uso de verdade, então semeia exatamente esse conjunto fixo uma vez.
/// </summary>
public static class TemplateDocumentoGerador
{
    private static readonly (string Nome, string Conteudo)[] Modelos =
    {
        ("Termo de entrega (padrão)", TemplatesDocumentosPadrao.TermoEntrega),
        ("Contrato de consignação (padrão)", TemplatesDocumentosPadrao.ContratoConsignacao),
        ("Resposta da financiadora (roteiro)", TemplatesDocumentosPadrao.RespostaFinanciadora),
    };

    public static Task<List<Guid>> GerarAsync(IServiceProvider provider)
    {
        return ExecutorLote.ExecutarAsync<ITemplateDocumentoService>(
            provider,
            Modelos.Length,
            (servico, i) => servico.AddAsync(new SalvarTemplateDocumentoDTO
            {
                Nome = Modelos[i].Nome,
                Conteudo = Modelos[i].Conteudo,
                Ativo = true
            }),
            "Templates de documento");
    }
}
