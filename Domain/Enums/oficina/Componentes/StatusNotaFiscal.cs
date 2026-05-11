namespace CarStoreManager.Domain.Enums;

/// <summary>
/// Ciclo de vida da NF-e dentro do sistema. Quando o XML é importado entra
/// em <see cref="ImportadaAguardandoAprovacao"/>; o admin então mapeia os
/// itens e aprova (entrada de estoque) ou rejeita (descarta).
/// </summary>
public enum StatusNotaFiscal
{
    ImportadaAguardandoAprovacao = 1,
    Aprovada = 2,
    Rejeitada = 3
}
