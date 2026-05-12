using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Concessionaria;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de VeiculoVenda.cs

    Esta classe tem testes automaticos implementados para:
        Nada ainda
*/

public class VeiculoVenda : Entity
{
    public string Marca { get; private set; } = null!;
    public string Modelo { get; private set; } = null!;
    public string Cor { get; private set; } = null!;
    public string Motorizacao { get; private set; } = null!;

    public Ano Ano { get; private set; } = null!;
    public Quilometragem Quilometragem { get; private set; } = null!;
    public PlacaVeiculo Placa { get; private set; } = null!;
    public Renavam Renavam { get; private set; } = null!;

    // IPVA — controle do imposto anual.
    // AnoUltimoIpvaPago null indica que nunca foi pago via sistema.
    public int? AnoUltimoIpvaPago { get; private set; }

    public TipoCambio Cambio { get; private set; }
    public TipoCombustivel Combustivel { get; private set; }
    public DisponibilidadeVeiculo Disponibilidade { get; private set; }
    public AcessoriosVeiculo Acessorios { get; private set; }

    public Dinheiro Valor { get; private set; } = null!;

    public List<Foto> Fotos { get; private set; } = new();

    protected VeiculoVenda() { }

    public VeiculoVenda(
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
        decimal valor,
        AcessoriosVeiculo acessorios = AcessoriosVeiculo.Nenhum,
        int? anoUltimoIpvaPago = null)
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

        Disponibilidade = DisponibilidadeVeiculo.Disponivel;
        Valor = new Dinheiro(valor);
        Acessorios = acessorios;
        AnoUltimoIpvaPago = anoUltimoIpvaPago;
    }

    /*
        Abaixo estão em sequencia todos os getters
        eles estao utilizando a sintaxe mais compacta.
    */
    public string GetMarca() => Marca;
    public string GetModelo() => Modelo;
    public string GetCor() => Cor;
    public string GetMotorizacao() => Motorizacao; 
    public int GetAno() => Ano.GetValorAno();
    public int GetQuilometragem() => Quilometragem.GetQuilometragem();
    public string GetPlacaCarro() => Placa.GetPlaca();
    public string GetRenavam() => Renavam.GetNumeroRenavam();
    public bool IpvaEmDia(int anoReferencia) => AnoUltimoIpvaPago.HasValue && AnoUltimoIpvaPago.Value >= anoReferencia;
    public string GetCambio() => Cambio.ToString();
    public string GetCombustivel() => Combustivel.ToString();
    public decimal GetValor() => Valor.GetValorDinheiro();

    public AcessoriosVeiculo GetAcessoriosVeiculo() => Acessorios;

    public List<string> GetAcessoriosLista()
    {
        return Enum.GetValues<AcessoriosVeiculo>()
            .Where(a => a != AcessoriosVeiculo.Nenhum && Acessorios.HasFlag(a))
            .Select(a => a.ToString())
            .ToList();
    }

    /*
        A seguir temos metodos de atualização que 
        servem vomo Setters. Mesmo que muitos nem
        tenham perspectiva de serem usados, 
        ja estao implementados.
        
        Recomendado aumentar a validação:
        - AlterarMarca()
        - AlterarModelo()
        - AlterarCor()
        - AlterarMotorizacao()
    */
    public void AlterarMarca(string marca)
    {
        if (string.IsNullOrWhiteSpace(marca))
            throw new ArgumentException("Marca inválida");
        Marca = NormalizarMarca(marca);
    }

    /// <summary>
    /// Normaliza marca para evitar duplicatas no dropdown: trim, colapsa
    /// espaços, e Title Case via cultura invariante. "VOLKSWAGEN", "volkswagen"
    /// e "VolksWagen" viram todos "Volkswagen".
    /// </summary>
    private static string NormalizarMarca(string raw)
    {
        var trim = raw.Trim();
        var unico = System.Text.RegularExpressions.Regex.Replace(trim, @"\s+", " ");
        var ti = System.Globalization.CultureInfo.InvariantCulture.TextInfo;
        return ti.ToTitleCase(unico.ToLowerInvariant());
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
    public void AlterarQuilometragem(int novaKm)
        => Quilometragem.AtualizarQuilometragem(novaKm);

    public void AlterarRenavam(string novoRenavam)
        => Renavam = new Renavam(novoRenavam);

    public void RegistrarPagamentoIpva(int ano)
    {
        if (ano < 1900 || ano > DateTime.UtcNow.Year + 1)
            throw new ArgumentException("Ano de IPVA inválido.", nameof(ano));
        if (AnoUltimoIpvaPago.HasValue && ano < AnoUltimoIpvaPago.Value)
            throw new InvalidOperationException("Não é possível registrar IPVA de ano anterior ao último pago.");
        AnoUltimoIpvaPago = ano;
    }

    public void LimparPagamentoIpva() => AnoUltimoIpvaPago = null;

    public void AtualizarValor(Dinheiro novoValor)
    {
        if (novoValor.GetValorDinheiro() <= 0)
            throw new ArgumentException("Valor inválido");
        Valor.SetValorDinheiro(novoValor);
    }

    //atualizar dados relevantes do veiculo
    public void AtualizarVeiculoVendaDados(
        Dinheiro novoValor,
        DisponibilidadeVeiculo disponibilidade)
    {
        AtualizarValor(novoValor);
        AlterarDisponibilidade(disponibilidade);
    }

    /*
        gerenciamento dos acessorios
    */
    public void AdicionarAcessorio(AcessoriosVeiculo acessorio)
        => Acessorios |= acessorio;
    public void RemoverAcessorio(AcessoriosVeiculo acessorio)
        => Acessorios &= ~acessorio;
    public void DefinirAcessorios(AcessoriosVeiculo acessorios)
        => Acessorios = acessorios;

    /*
        gerenciamento dos status
    */
    public void AlterarDisponibilidade(DisponibilidadeVeiculo disponibilidade)
        => Disponibilidade = disponibilidade;
    public void MarcarComoVendido()
        => Disponibilidade = DisponibilidadeVeiculo.Vendido;
    public void MarcarComoDisponivel()
        => Disponibilidade = DisponibilidadeVeiculo.Disponivel;

    /*
        gerenciamento das fotos
    */
    public void AdicionarFoto(string url)
    {
        var ordem = Fotos.Count;
        Fotos.Add(new Foto(Id, url, ordem));
    }
    public void RemoverFoto(Guid fotoId)
    {
        var foto = Fotos.FirstOrDefault(f => f.Id == fotoId)
            ?? throw new InvalidOperationException("Foto não encontrada");

        Fotos.Remove(foto);
        ReordenarFotos();
    }
    private void ReordenarFotos()
    {
        for (int i = 0; i < Fotos.Count; i++)
            Fotos[i].AtualizarOrdem(i);
    }
}