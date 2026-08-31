using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Concessionaria;

/// <summary>
/// Representa o arranjo de consignação de um veículo: o proprietário (Cliente)
/// deixa o veículo na concessionária para venda, mediante comissão e prazo.
/// Composição, não herança — não estende VeiculoVenda.
/// </summary>
public class VeiculoConsignacao : Entity
{
    /// <summary>Janela de renovação: só fica disponível nos últimos N dias antes do vencimento (ou já vencida).</summary>
    public const int DiasParaHabilitarRenovacao = 10;

    public string Marca { get; private set; } = null!;
    public string Modelo { get; private set; } = null!;
    public string Cor { get; private set; } = null!;
    public string Motorizacao { get; private set; } = null!;

    public Ano Ano { get; private set; } = null!;
    public Quilometragem Quilometragem { get; private set; } = null!;
    public PlacaVeiculo Placa { get; private set; } = null!;
    public Renavam Renavam { get; private set; } = null!;

    public TipoCambio Cambio { get; private set; }
    public TipoCombustivel Combustivel { get; private set; }
    public AcessoriosVeiculo Acessorios { get; private set; }

    public Guid ClienteProprietarioId { get; private set; }
    public Guid VendedorResponsavelId { get; private set; }

    public ComissaoConsignacao Comissao { get; private set; } = null!;

    public string TextoContrato { get; private set; } = "";
    public string? UrlContratoPdf { get; private set; }

    public DateTime DataInicio { get; private set; }
    public DateTime DataVencimento { get; private set; }
    public StatusConsignacao Status { get; private set; }

    public List<HistoricoConsignacao> Historico { get; private set; } = new();

    protected VeiculoConsignacao() { }

    public VeiculoConsignacao(
        string marca,
        string modelo,
        string cor,
        string motorizacao,
        int ano,
        int quilometragem,
        string placa,
        string renavam,
        TipoCambio cambio,
        TipoCombustivel combustivel,
        Guid clienteProprietarioId,
        Guid vendedorResponsavelId,
        ComissaoConsignacao comissao,
        string textoContrato,
        string? urlContratoPdf = null,
        int prazoDias = 90,
        AcessoriosVeiculo acessorios = AcessoriosVeiculo.Nenhum)
    {
        AlterarMarca(marca);
        AlterarModelo(modelo);
        AlterarCor(cor);
        AlterarMotorizacao(motorizacao);

        Ano = new Ano(ano);
        Quilometragem = new Quilometragem(quilometragem);
        Placa = new PlacaVeiculo(placa);
        Renavam = new Renavam(renavam);

        Cambio = cambio;
        Combustivel = combustivel;
        Acessorios = acessorios;

        if (clienteProprietarioId == Guid.Empty)
            throw new ArgumentException("Cliente proprietário é obrigatório.", nameof(clienteProprietarioId));
        if (vendedorResponsavelId == Guid.Empty)
            throw new ArgumentException("Vendedor responsável é obrigatório.", nameof(vendedorResponsavelId));

        ClienteProprietarioId = clienteProprietarioId;
        VendedorResponsavelId = vendedorResponsavelId;
        Comissao = comissao ?? throw new ArgumentNullException(nameof(comissao));

        AtualizarContrato(textoContrato, urlContratoPdf);

        DataInicio = DateTime.UtcNow;
        DataVencimento = DataInicio.AddDays(prazoDias <= 0 ? 90 : prazoDias);
        Status = StatusConsignacao.Ativa;

        RegistrarEvento(TipoEventoConsignacao.Criacao, $"Consignação criada: {marca} {modelo} {ano}.");
    }

    public string GetMarca() => Marca;
    public string GetModelo() => Modelo;
    public string GetCor() => Cor;
    public string GetMotorizacao() => Motorizacao;
    public int GetAno() => Ano.GetValorAno();
    public int GetQuilometragem() => Quilometragem.GetQuilometragem();
    public string GetPlacaCarro() => Placa.GetPlaca();
    public string GetRenavam() => Renavam.GetNumeroRenavam();
    public string GetCambio() => Cambio.ToString();
    public string GetCombustivel() => Combustivel.ToString();

    public List<string> GetAcessoriosLista()
        => Enum.GetValues<AcessoriosVeiculo>()
            .Where(a => a != AcessoriosVeiculo.Nenhum && Acessorios.HasFlag(a))
            .Select(a => a.ToString())
            .ToList();

    /// <summary>Dias restantes até o vencimento (negativo se já vencido); 0 quando fora de estado ativo/expirado.</summary>
    public int DiasRestantes
        => Status is StatusConsignacao.Ativa or StatusConsignacao.Expirada
            ? (int)Math.Ceiling((DataVencimento.Date - DateTime.UtcNow.Date).TotalDays)
            : 0;

    public bool EstaVencido => Status == StatusConsignacao.Ativa && DateTime.UtcNow > DataVencimento;

    public void AlterarMarca(string marca)
    {
        if (string.IsNullOrWhiteSpace(marca))
            throw new ArgumentException("Marca inválida");
        Marca = marca.Trim();
    }
    public void AlterarModelo(string modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo))
            throw new ArgumentException("Modelo inválido");
        Modelo = modelo.Trim();
    }
    public void AlterarCor(string cor)
    {
        if (string.IsNullOrWhiteSpace(cor))
            throw new ArgumentException("Cor inválida");
        Cor = cor.Trim();
    }
    public void AlterarMotorizacao(string motorizacao)
    {
        if (string.IsNullOrWhiteSpace(motorizacao))
            throw new ArgumentException("Motorização inválida");
        Motorizacao = motorizacao.Trim();
    }

    public void AtualizarContrato(string textoContrato, string? urlContratoPdf = null)
    {
        TextoContrato = (textoContrato ?? "").Trim();
        UrlContratoPdf = string.IsNullOrWhiteSpace(urlContratoPdf) ? null : urlContratoPdf.Trim();
    }

    public void AlterarVendedorResponsavel(Guid vendedorResponsavelId)
    {
        if (vendedorResponsavelId == Guid.Empty)
            throw new ArgumentException("Vendedor responsável é obrigatório.", nameof(vendedorResponsavelId));
        VendedorResponsavelId = vendedorResponsavelId;
    }

    /// <summary>Lazy-expire: chamado pelo service na leitura, mirroring PropostaVenda.TentarExpirar().</summary>
    public bool TentarExpirar()
    {
        if (Status != StatusConsignacao.Ativa || DateTime.UtcNow <= DataVencimento)
            return false;

        Status = StatusConsignacao.Expirada;
        RegistrarEvento(TipoEventoConsignacao.MudancaStatus, "Prazo de consignação expirado.");
        return true;
    }

    public void RenovarConsignacao(int diasAdicionais = 90)
    {
        BloquearSeTerminal();
        if (Status is not (StatusConsignacao.Ativa or StatusConsignacao.Expirada))
            throw new InvalidOperationException($"Só é possível renovar consignações ativas ou expiradas (atual: {Status}).");

        if (Status == StatusConsignacao.Ativa && DiasRestantes > DiasParaHabilitarRenovacao)
            throw new InvalidOperationException(
                $"A renovação só fica disponível a partir de {DiasParaHabilitarRenovacao} dias antes do vencimento (faltam {DiasRestantes} dia(s)).");

        var dias = diasAdicionais <= 0 ? 90 : diasAdicionais;
        DataVencimento = DataVencimento.AddDays(dias);
        Status = StatusConsignacao.Ativa;
        RegistrarEvento(TipoEventoConsignacao.Renovacao, $"Prazo renovado por {dias} dias. Novo vencimento: {DataVencimento:dd/MM/yyyy}.");
    }

    public void MarcarComoVendida()
    {
        BloquearSeTerminal();
        if (Status is not (StatusConsignacao.Ativa or StatusConsignacao.Expirada))
            throw new InvalidOperationException($"Só é possível marcar como vendida a partir de Ativa/Expirada (atual: {Status}).");

        Status = StatusConsignacao.VendidoAguardandoPagamento;
        RegistrarEvento(TipoEventoConsignacao.Venda, "Veículo vendido — aguardando pagamento ao proprietário.");
    }

    public void ConcluirVenda()
    {
        if (Status != StatusConsignacao.VendidoAguardandoPagamento)
            throw new InvalidOperationException($"Só é possível concluir a partir de VendidoAguardandoPagamento (atual: {Status}).");

        Status = StatusConsignacao.Concluida;
        RegistrarEvento(TipoEventoConsignacao.MudancaStatus, "Pagamento ao proprietário concluído — venda finalizada.");
    }

    public void Devolver()
    {
        BloquearSeTerminal();
        if (Status is not (StatusConsignacao.Ativa or StatusConsignacao.Expirada))
            throw new InvalidOperationException($"Só é possível devolver a partir de Ativa/Expirada (atual: {Status}).");

        Status = StatusConsignacao.Devolvida;
        RegistrarEvento(TipoEventoConsignacao.Devolucao, "Veículo devolvido ao proprietário sem venda.");
    }

    public void Cancelar(string motivo)
    {
        BloquearSeTerminal();
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Motivo do cancelamento é obrigatório.", nameof(motivo));

        Status = StatusConsignacao.Cancelada;
        RegistrarEvento(TipoEventoConsignacao.Cancelamento, motivo.Trim());
    }

    public bool EhTerminal()
        => Status is StatusConsignacao.Concluida or StatusConsignacao.Devolvida or StatusConsignacao.Cancelada;

    private void BloquearSeTerminal()
    {
        if (EhTerminal())
            throw new InvalidOperationException($"Operação não permitida: consignação está em estado terminal ({Status}).");
    }

    private void RegistrarEvento(TipoEventoConsignacao tipo, string descricao, Guid? usuarioId = null)
        => Historico.Add(new HistoricoConsignacao(Id, tipo, descricao, usuarioId));
}
