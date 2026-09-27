using System.Security.Cryptography;
using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities.Concessionaria;

/// <summary>
/// Termo de responsabilidade do test drive — o cliente assume a
/// responsabilidade pelo veículo durante o passeio de teste. Mesmo padrão de
/// assinatura eletrônica simples do <see cref="TermoEntrega"/> (Lei
/// 14.063/2020: nome + CPF + IP + timestamp + aceite explícito) — entidade
/// separada porque o termo de entrega está fortemente acoplado à
/// PropostaVenda (funil de venda, pagamento, conclusão da proposta), o que
/// não se aplica ao test drive.
/// </summary>
public class TermoTestDrive : Entity
{
    public Guid TestDriveId { get; private set; }

    public string TextoTermo { get; private set; } = "";
    public Guid VendedorRedatorId { get; private set; }
    public DateTime DataRedacao { get; private set; }
    public DateTime? DataUltimaEdicao { get; private set; }

    public StatusTermoTestDrive Status { get; private set; }

    public string? TokenAssinatura { get; private set; }

    public DateTime? DataAssinatura { get; private set; }
    public string? AssinaturaNomeCliente { get; private set; }
    public string? AssinaturaCpfCliente { get; private set; }
    public string? AssinaturaIp { get; private set; }

    protected TermoTestDrive() { }

    public TermoTestDrive(Guid testDriveId, Guid vendedorRedatorId, string textoInicial)
    {
        if (string.IsNullOrWhiteSpace(textoInicial))
            throw new ArgumentException("Texto inicial do termo é obrigatório.", nameof(textoInicial));

        TestDriveId = testDriveId;
        VendedorRedatorId = vendedorRedatorId;
        TextoTermo = textoInicial.Trim();
        DataRedacao = DateTime.UtcNow;
        Status = StatusTermoTestDrive.Rascunho;
    }

    public void EditarTexto(string novoTexto)
    {
        if (Status == StatusTermoTestDrive.Assinado)
            throw new InvalidOperationException("Termo já assinado não pode ser editado.");
        if (string.IsNullOrWhiteSpace(novoTexto))
            throw new ArgumentException("Texto do termo é obrigatório.", nameof(novoTexto));

        TextoTermo = novoTexto.Trim();
        DataUltimaEdicao = DateTime.UtcNow;
    }

    public void EnviarParaAssinatura()
    {
        if (Status == StatusTermoTestDrive.Assinado)
            throw new InvalidOperationException("Termo já assinado.");

        var bytes = RandomNumberGenerator.GetBytes(32);
        TokenAssinatura = Convert.ToBase64String(bytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        Status = StatusTermoTestDrive.AguardandoAssinatura;
    }

    public void Assinar(string nomeCliente, string cpfCliente, string ipOrigem)
    {
        if (Status != StatusTermoTestDrive.AguardandoAssinatura)
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
        Status = StatusTermoTestDrive.Assinado;
    }
}
