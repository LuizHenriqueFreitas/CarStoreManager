using System.Security.Cryptography;
using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities.Concessionaria;

/// <summary>
/// Termo redigido pelo administrador descrevendo o estado em que o veículo
/// é entregue (avarias visíveis, equipamentos, quilometragem etc).
/// O cliente assina eletronicamente — Lei 14.063/2020 (assinatura simples):
/// nome + CPF + IP + timestamp + aceite explícito sobre conteúdo conhecido.
/// </summary>
public class TermoEntrega : Entity
{
    public Guid PropostaVendaId { get; private set; }

    public string TextoTermo { get; private set; } = "";
    public Guid AdminRedatorId { get; private set; }
    public DateTime DataRedacao { get; private set; }
    public DateTime? DataUltimaEdicao { get; private set; }

    public StatusTermoEntrega Status { get; private set; }

    /// <summary>
    /// Token único usado na URL pública de assinatura. Cliente acessa
    /// <c>/concessionaria/assinar/{TokenAssinatura}</c> sem precisar logar.
    /// </summary>
    public string? TokenAssinatura { get; private set; }

    public DateTime? DataAssinatura { get; private set; }
    public string? AssinaturaNomeCliente { get; private set; }
    public string? AssinaturaCpfCliente { get; private set; }
    public string? AssinaturaIp { get; private set; }

    protected TermoEntrega() { }

    public TermoEntrega(Guid propostaVendaId, Guid adminRedatorId, string textoInicial)
    {
        if (string.IsNullOrWhiteSpace(textoInicial))
            throw new ArgumentException("Texto inicial do termo é obrigatório.", nameof(textoInicial));

        PropostaVendaId = propostaVendaId;
        AdminRedatorId = adminRedatorId;
        TextoTermo = textoInicial.Trim();
        DataRedacao = DateTime.UtcNow;
        Status = StatusTermoEntrega.Rascunho;
    }

    public void EditarTexto(string novoTexto)
    {
        if (Status == StatusTermoEntrega.Assinado)
            throw new InvalidOperationException("Termo já assinado não pode ser editado.");
        if (string.IsNullOrWhiteSpace(novoTexto))
            throw new ArgumentException("Texto do termo é obrigatório.", nameof(novoTexto));

        TextoTermo = novoTexto.Trim();
        DataUltimaEdicao = DateTime.UtcNow;
    }

    /// <summary>
    /// Gera token único de assinatura e libera o termo para o cliente assinar.
    /// O link com o token deve ser entregue ao cliente fora da plataforma
    /// (e-mail, mensagem, papel impresso).
    /// </summary>
    public void EnviarParaAssinatura()
    {
        if (Status == StatusTermoEntrega.Assinado)
            throw new InvalidOperationException("Termo já assinado.");

        // Token de 32 bytes em base64-url (≈43 chars sem padding).
        var bytes = RandomNumberGenerator.GetBytes(32);
        TokenAssinatura = Convert.ToBase64String(bytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        Status = StatusTermoEntrega.AguardandoAssinatura;
    }

    /// <summary>
    /// Cliente assina eletronicamente. Validação: nome + CPF preenchidos +
    /// CPF com 11 dígitos (numérico). Não validamos DV aqui — a entidade
    /// confia que o service já normalizou.
    /// </summary>
    public void Assinar(string nomeCliente, string cpfCliente, string ipOrigem)
    {
        if (Status != StatusTermoEntrega.AguardandoAssinatura)
            throw new InvalidOperationException(
                $"Termo não está aguardando assinatura (status: {Status}).");
        if (string.IsNullOrWhiteSpace(nomeCliente))
            throw new ArgumentException("Nome do cliente é obrigatório.", nameof(nomeCliente));
        if (string.IsNullOrWhiteSpace(cpfCliente))
            throw new ArgumentException("CPF do cliente é obrigatório.", nameof(cpfCliente));

        var cpfDigitos = new string(cpfCliente.Where(char.IsDigit).ToArray());
        if (cpfDigitos.Length != 11)
            throw new ArgumentException("CPF deve ter 11 dígitos.", nameof(cpfCliente));

        AssinaturaNomeCliente = nomeCliente.Trim();
        AssinaturaCpfCliente = cpfDigitos;
        AssinaturaIp = ipOrigem ?? "desconhecido";
        DataAssinatura = DateTime.UtcNow;
        Status = StatusTermoEntrega.Assinado;
    }
}
