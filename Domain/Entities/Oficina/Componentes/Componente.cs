using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities.Oficina;

public class Componente : Entity
{
    // CONTROLE INTERNO
    public string SKUInterno { get; private set; } = null!;

    // IDENTIFICAÇÃO
    public string Nome { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;

    // FABRICANTE
    public string MarcaFabricante { get; private set; } = null!; // futuramente: enum MarcaFabricanteEnum

    // CÓDIGO DO FABRICANTE
    public string PartNumber { get; private set; } = null!; // futuramente: ValueObject PartNumber

    // CÓDIGO OEM
    public string CodigoOEM { get; private set; } = null!; // futuramente: ValueObject CodigoOEM

    // GTIN/EAN
    public string CodigoBarras { get; private set; } = null!;

    // FISCAL
    public string NCM { get; private set; } = null!;
    public string CEST { get; private set; } = null!;

    // CLASSIFICAÇÃO
    public string Categoria { get; private set; } = null!;
    public string Unidade { get; private set; } = null!;

    // PESO
    public decimal Peso { get; private set; }

    // GARANTIA
    public int GarantiaDias { get; private set; }
    public bool Ativo { get; private set; } = true;

    // SISTEMA do veículo a que pertence (Motor, Freios, etc) — usado para
    // aplicar margem de lucro padrão do segmento.
    public Domain.Enums.SistemaComponente? Sistema { get; private set; }

    // PRECIFICAÇÃO
    /// <summary>Custo unitário (média ponderada das entradas via NF-e ou valor inicial).</summary>
    public decimal CustoUnitario { get; private set; }

    /// <summary>
    /// Margem aplicada sobre o custo, em percentual (ex: 30 = 30%).
    /// Se null, sistema usa o padrão da config para o Sistema.
    /// </summary>
    public decimal? MargemLucroPct { get; private set; }

    /// <summary>
    /// Valor de venda calculado: custo * (1 + margem/100).
    /// Calculado em Application via <c>AplicarPrecificacao</c> e armazenado.
    /// </summary>
    public decimal ValorVenda { get; private set; }

    // RELACIONAMENTOS
    public ICollection<LoteComponente> Lotes { get; private set; } = new List<LoteComponente>();
    public ICollection<ComponenteEquivalente> EquivalenciasOriginais { get; private set; } = new List<ComponenteEquivalente>();
    public ICollection<ComponenteEquivalente> EquivalenciasRelacionadas { get; private set; } = new List<ComponenteEquivalente>();

    // Construtor protegido para ORM
    protected Componente() { }

    // Construtor principal com validações
    public Componente(
        string skuInterno,
        string nome,
        string descricao,
        string marcaFabricante,
        string partNumber,
        string codigoOEM,
        string codigoBarras,
        string ncm,
        string cest,
        string categoria,
        string unidade,
        decimal peso,
        int garantiaDias)
    {
        SetSKUInterno(skuInterno);
        SetNome(nome);
        SetDescricao(descricao);
        SetMarcaFabricante(marcaFabricante);
        SetPartNumber(partNumber);
        SetCodigoOEM(codigoOEM);
        SetCodigoBarras(codigoBarras);
        SetNCM(ncm);
        SetCEST(cest);
        SetCategoria(categoria);
        SetUnidade(unidade);
        SetPeso(peso);
        SetGarantiaDias(garantiaDias);
        // Ativo já é true por padrão
    }

    // ================================
    // GETTERS (explicitos)
    // ================================
    public string GetSKUInterno() => SKUInterno;
    public string GetNome() => Nome;
    public string GetDescricao() => Descricao;
    public string GetMarcaFabricante() => MarcaFabricante;
    public string GetPartNumber() => PartNumber;
    public string GetCodigoOEM() => CodigoOEM;
    public string GetCodigoBarras() => CodigoBarras;
    public string GetNCM() => NCM;
    public string GetCEST() => CEST;
    public string GetCategoria() => Categoria;
    public string GetUnidade() => Unidade;
    public decimal GetPeso() => Peso;
    public int GetGarantiaDias() => GarantiaDias;
    public bool GetAtivo() => Ativo;

    /// <summary>
    /// Define o custo (vindo da NF de entrada ou cadastro inicial) e recalcula
    /// o valor de venda usando a margem fornecida (margem null = aceita o atual).
    /// </summary>
    public void AplicarPrecificacao(decimal custo, decimal margemPct)
    {
        if (custo < 0)
            throw new ArgumentException("Custo unitário não pode ser negativo.", nameof(custo));
        if (margemPct < 0)
            throw new ArgumentException("Margem não pode ser negativa.", nameof(margemPct));

        CustoUnitario = custo;
        MargemLucroPct = margemPct;
        ValorVenda = Math.Round(custo * (1m + margemPct / 100m), 2);
    }

    public void DefinirSistema(Domain.Enums.SistemaComponente? sistema) => Sistema = sistema;

    /// <summary>
    /// Sobrescreve só a margem (mantém custo). Usado pelo botão "ajustar margem".
    /// </summary>
    public void AjustarMargem(decimal novaMargemPct)
        => AplicarPrecificacao(CustoUnitario, novaMargemPct);

    // ================================
    // SETTERS com validações
    // ================================

    public void SetSKUInterno(string skuInterno)
    {
        if (string.IsNullOrWhiteSpace(skuInterno))
            throw new ArgumentException("SKU interno não pode ser vazio ou nulo.", nameof(skuInterno));
        if (skuInterno.Length > 50)
            throw new ArgumentException("SKU interno não pode ter mais de 50 caracteres.", nameof(skuInterno));
        SKUInterno = skuInterno.Trim();
    }

    public void SetNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio ou nulo.", nameof(nome));
        if (nome.Length > 200)
            throw new ArgumentException("Nome não pode ter mais de 200 caracteres.", nameof(nome));
        Nome = nome.Trim();
    }

    public void SetDescricao(string descricao)
    {
        if (descricao == null)
            throw new ArgumentNullException(nameof(descricao));
        if (descricao.Length > 1000)
            throw new ArgumentException("Descrição não pode ter mais de 1000 caracteres.", nameof(descricao));
        Descricao = descricao.Trim();
    }

    public void SetMarcaFabricante(string marcaFabricante)
    {
        if (string.IsNullOrWhiteSpace(marcaFabricante))
            throw new ArgumentException("Marca do fabricante não pode ser vazia ou nula.", nameof(marcaFabricante));
        if (marcaFabricante.Length > 100)
            throw new ArgumentException("Marca do fabricante não pode ter mais de 100 caracteres.", nameof(marcaFabricante));
        MarcaFabricante = marcaFabricante.Trim();
    }

    public void SetPartNumber(string partNumber)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
            throw new ArgumentException("Part number não pode ser vazio ou nulo.", nameof(partNumber));
        if (partNumber.Length > 50)
            throw new ArgumentException("Part number não pode ter mais de 50 caracteres.", nameof(partNumber));
        PartNumber = partNumber.Trim();
    }

    public void SetCodigoOEM(string codigoOEM)
    {
        // OEM pode ser opcional
        CodigoOEM = codigoOEM?.Trim() ?? string.Empty;
        if (CodigoOEM.Length > 50)
            throw new ArgumentException("Código OEM não pode ter mais de 50 caracteres.", nameof(codigoOEM));
    }

    public void SetCodigoBarras(string codigoBarras)
    {
        // GTIN/EAN: validação básica (digit verification pode ser adicionada)
        if (!string.IsNullOrWhiteSpace(codigoBarras))
        {
            if (!codigoBarras.All(char.IsDigit))
                throw new ArgumentException("Código de barras deve conter apenas dígitos.", nameof(codigoBarras));
            if (codigoBarras.Length < 8 || codigoBarras.Length > 14)
                throw new ArgumentException("Código de barras deve ter entre 8 e 14 dígitos.", nameof(codigoBarras));
        }
        CodigoBarras = codigoBarras?.Trim() ?? string.Empty;
    }

    public void SetNCM(string ncm)
    {
        if (string.IsNullOrWhiteSpace(ncm))
            throw new ArgumentException("NCM não pode ser vazio ou nulo.", nameof(ncm));
        if (ncm.Length != 8 || !ncm.All(char.IsDigit))
            throw new ArgumentException("NCM deve conter exatamente 8 dígitos numéricos.", nameof(ncm));
        NCM = ncm;
    }

    public void SetCEST(string cest)
    {
        // CEST pode ser opcional
        if (!string.IsNullOrWhiteSpace(cest))
        {
            if (cest.Length != 7 || !cest.All(char.IsDigit))
                throw new ArgumentException("CEST deve conter 7 dígitos numéricos quando informado.", nameof(cest));
        }
        CEST = cest?.Trim() ?? string.Empty;
    }

    public void SetCategoria(string categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria))
            throw new ArgumentException("Categoria não pode ser vazia ou nula.", nameof(categoria));
        if (categoria.Length > 100)
            throw new ArgumentException("Categoria não pode ter mais de 100 caracteres.", nameof(categoria));
        Categoria = categoria.Trim();
    }

    public void SetUnidade(string unidade)
    {
        if (string.IsNullOrWhiteSpace(unidade))
            throw new ArgumentException("Unidade de medida não pode ser vazia ou nula.", nameof(unidade));
        if (unidade.Length > 10)
            throw new ArgumentException("Unidade não pode ter mais de 10 caracteres.", nameof(unidade));
        Unidade = unidade.Trim().ToUpperInvariant();
    }

    public void SetPeso(decimal peso)
    {
        if (peso < 0)
            throw new ArgumentException("Peso não pode ser negativo.", nameof(peso));
        if (peso > 500)
            throw new ArgumentException("Peso excede o limite máximo de 500 kg.", nameof(peso));
        Peso = Math.Round(peso, 3);
    }

    public void SetGarantiaDias(int garantiaDias)
    {
        if (garantiaDias < 0)
            throw new ArgumentException("Garantia em dias não pode ser negativa.", nameof(garantiaDias));
        GarantiaDias = garantiaDias;
    }

    // Métodos de controle de ativação
    public void Ativar() => Ativo = true;
    public void Desativar() => Ativo = false;

    // Métodos auxiliares para relacionamentos
    public void AdicionarLote(LoteComponente lote)
    {
        if (lote == null) throw new ArgumentNullException(nameof(lote));
        if (Lotes.Any(l => l.Id == lote.Id))
            throw new InvalidOperationException("Este lote já está associado ao componente.");
        Lotes.Add(lote);
    }

    public void RemoverLote(LoteComponente lote)
    {
        if (lote == null) throw new ArgumentNullException(nameof(lote));
        if (!Lotes.Contains(lote))
            throw new InvalidOperationException("O lote não pertence a este componente.");
        Lotes.Remove(lote);
    }

    public void AdicionarEquivalencia(Componente componenteEquivalente, TipoEquivalencia tipo)
    {
        if (componenteEquivalente == null) throw new ArgumentNullException(nameof(componenteEquivalente));
        if (componenteEquivalente.Id == Id)
            throw new InvalidOperationException("Um componente não pode ser equivalente a si mesmo.");
        if (EquivalenciasOriginais.Any(e => e.ComponenteEquivalenteId == componenteEquivalente.Id))
            throw new InvalidOperationException("Essa equivalência já está registrada.");

        var equivalencia = new ComponenteEquivalente(this.Id, componenteEquivalente.Id, tipo);
        EquivalenciasOriginais.Add(equivalencia);
    }

    public void RemoverEquivalencia(Componente componenteEquivalente)
    {
        var equivalencia = EquivalenciasOriginais
            .FirstOrDefault(e => e.ComponenteEquivalenteId == componenteEquivalente.Id);
        if (equivalencia == null)
            throw new InvalidOperationException("Equivalência não encontrada.");
        EquivalenciasOriginais.Remove(equivalencia);
    }
}