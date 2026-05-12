using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.NotaFiscal;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services.Oficina.NotaFiscal;

public class NotaFiscalEntradaService : INotaFiscalEntradaService
{
    private readonly INotaFiscalRepository _notasRepo;
    private readonly IFornecedorRepository _fornecedoresRepo;
    private readonly IComponenteRepository _componentesRepo;
    private readonly IEstoqueRepository _estoqueRepo;
    private readonly ILoteComponenteRepository _lotesRepo;
    private readonly IOrdemServicoRepository _ordensRepo;
    private readonly Domain.Interfaces.Repositories.Sistema.IConfiguracaoSistemaRepository _configRepo;

    public NotaFiscalEntradaService(
        INotaFiscalRepository notasRepo,
        IFornecedorRepository fornecedoresRepo,
        IComponenteRepository componentesRepo,
        IEstoqueRepository estoqueRepo,
        ILoteComponenteRepository lotesRepo,
        IOrdemServicoRepository ordensRepo,
        Domain.Interfaces.Repositories.Sistema.IConfiguracaoSistemaRepository configRepo)
    {
        _notasRepo = notasRepo;
        _fornecedoresRepo = fornecedoresRepo;
        _componentesRepo = componentesRepo;
        _estoqueRepo = estoqueRepo;
        _lotesRepo = lotesRepo;
        _ordensRepo = ordensRepo;
        _configRepo = configRepo;
    }

    public async Task<Result<NotaFiscalDTO>> ImportarXmlAsync(string xml)
    {
        var parse = NotaFiscalXmlParser.Parse(xml);
        if (!parse.IsSuccess) return Result<NotaFiscalDTO>.Fail(parse.Error!);
        var dados = parse.Value!;

        // Idempotência: chave já importada → devolve a existente.
        var existente = await _notasRepo.ObterPorChaveAsync(dados.ChaveAcesso);
        if (existente is not null)
            return Result<NotaFiscalDTO>.Fail(
                $"NF-e já importada anteriormente (chave {dados.ChaveAcesso}). Status atual: {existente.Status}.");

        try
        {
            var fornecedor = await ObterOuCriarFornecedorAsync(dados);

            var nota = new Domain.Entities.Oficina.NotaFiscal(
                tipo: TipoNotaFiscal.Entrada,
                numero: dados.Numero,
                serie: dados.Serie,
                chaveAcesso: dados.ChaveAcesso,
                dataEmissao: dados.DataEmissao,
                xmlConteudo: dados.XmlOriginal,
                valorProdutos: dados.ValorProdutos,
                valorImpostos: dados.ValorImpostos,
                valorTotal: dados.ValorTotal,
                fornecedorId: fornecedor.Id);

            foreach (var x in dados.Itens)
            {
                var item = new ItemNotaFiscal(
                    notaFiscalId: nota.Id,
                    codigoProdutoFornecedor: x.CodigoProdutoFornecedor,
                    descricaoProdutoFornecedor: x.DescricaoProdutoFornecedor,
                    ncm: x.Ncm,
                    unidade: x.Unidade,
                    quantidade: x.Quantidade,
                    valorUnitario: x.ValorUnitario,
                    cfop: x.Cfop,
                    cst: x.Cst,
                    csosn: x.Csosn,
                    aliquotaIcms: x.AliquotaIcms,
                    valorIcms: x.ValorIcms,
                    numeroLoteFornecedor: x.NumeroLoteFornecedor,
                    dataFabricacao: x.DataFabricacao,
                    dataValidade: x.DataValidade);

                // Tenta auto-mapear pelo PartNumber == cProd (heurística leve;
                // o admin pode trocar antes de aprovar).
                var componenteSugerido = await TentarAutoMapearAsync(x.CodigoProdutoFornecedor);
                if (componenteSugerido is not null)
                    item.VincularComponente(componenteSugerido.Id);

                nota.AdicionarItem(item);
            }

            await _notasRepo.AddAsync(nota);
            await _notasRepo.SaveChangesAsync();

            var completo = await _notasRepo.ObterCompletoAsync(nota.Id);
            return Result<NotaFiscalDTO>.Ok(MapToDto(completo!));
        }
        catch (Exception ex)
        {
            return Result<NotaFiscalDTO>.Fail($"Falha ao importar nota: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<NotaFiscalListaDTO>>> ListarAsync()
    {
        var notas = await _notasRepo.GetAllAsync();
        // GetAllAsync não traz Itens; carregamos separadamente para o contador.
        var lista = new List<NotaFiscalListaDTO>();
        foreach (var n in notas)
        {
            var completo = await _notasRepo.ObterCompletoAsync(n.Id);
            lista.Add(MapToListaDto(completo!));
        }
        return Result<IEnumerable<NotaFiscalListaDTO>>.Ok(lista);
    }

    public async Task<Result<NotaFiscalDTO>> ObterAsync(Guid id)
    {
        var nota = await _notasRepo.ObterCompletoAsync(id);
        return nota is null
            ? Result<NotaFiscalDTO>.Fail("Nota fiscal não encontrada.")
            : Result<NotaFiscalDTO>.Ok(MapToDto(nota));
    }

    public async Task<Result> VincularComponenteAsync(Guid itemId, Guid componenteId)
    {
        var item = await _notasRepo.ObterItemAsync(itemId);
        if (item is null) return Result.Fail("Item não encontrado.");
        if (item.NotaFiscal.Status != StatusNotaFiscal.ImportadaAguardandoAprovacao)
            return Result.Fail("Itens só podem ser alterados enquanto a nota está pendente.");

        var componente = await _componentesRepo.GetByIdAsync(componenteId);
        if (componente is null) return Result.Fail("Componente não encontrado.");

        try
        {
            item.VincularComponente(componenteId);
            await _notasRepo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> AlterarQuantidadeItemAsync(Guid itemId, int quantidade)
    {
        var item = await _notasRepo.ObterItemAsync(itemId);
        if (item is null) return Result.Fail("Item não encontrado.");
        if (item.NotaFiscal.Status != StatusNotaFiscal.ImportadaAguardandoAprovacao)
            return Result.Fail("Itens só podem ser alterados enquanto a nota está pendente.");

        try
        {
            item.AlterarQuantidade(quantidade);
            await _notasRepo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> AprovarAsync(Guid notaFiscalId)
    {
        var nota = await _notasRepo.ObterCompletoAsync(notaFiscalId);
        if (nota is null) return Result.Fail("Nota fiscal não encontrada.");

        try
        {
            // Aprovar() valida pré-condições (status pendente + itens mapeados).
            nota.Aprovar();

            foreach (var item in nota.Itens)
            {
                var componenteId = item.ComponenteId!.Value;

                // 1) cria lote vinculado à NF-e — rastreabilidade fiscal.
                var numeroLote = item.NumeroLoteFornecedor
                    ?? $"NF{nota.Numero}-{item.Id.ToString()[..8]}";
                var lote = new LoteComponente(
                    componenteId: componenteId,
                    numeroLote: numeroLote,
                    dataFabricacao: item.DataFabricacao ?? nota.DataEmissao,
                    dataValidade: item.DataValidade,
                    fornecedorId: nota.FornecedorId,
                    notaFiscalId: nota.Id,
                    quantidadeRecebida: item.Quantidade);
                await _lotesRepo.AddAsync(lote);

                // 2) entrada no estoque agregado do componente.
                var estoque = await _estoqueRepo.ObterPorComponenteAsync(componenteId);
                if (estoque is null)
                {
                    estoque = new EstoqueComponente(componenteId, 0);
                    estoque.Adicionar(item.Quantidade);
                    await _estoqueRepo.AddAsync(estoque);
                }
                else
                {
                    estoque.Adicionar(item.Quantidade);
                    _estoqueRepo.Update(estoque);
                }

                // 3) atualiza CustoUnitario do componente com o valor da NF
                // e recalcula ValorVenda mantendo a margem atual.
                var componente = await _componentesRepo.GetByIdAsync(componenteId);
                if (componente is not null)
                {
                    var margem = componente.MargemLucroPct
                        ?? (await _configRepo.ObterAsync()).ObterMargemParaSistema(componente.Sistema);
                    componente.AplicarPrecificacao(item.ValorUnitario, margem);
                    _componentesRepo.Update(componente);
                }
            }

            await _notasRepo.SaveChangesAsync();

            // 3) auto-conciliação de itens de OS aguardando encomenda destes componentes.
            foreach (var componenteId in nota.Itens.Select(i => i.ComponenteId!.Value).Distinct())
                await ConciliarItensAguardandoAsync(componenteId);

            return Result.Ok();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Falha ao aprovar nota: {ex.Message}");
        }
    }

    public async Task<Result> RejeitarAsync(Guid notaFiscalId, string motivo)
    {
        var nota = await _notasRepo.GetByIdAsync(notaFiscalId);
        if (nota is null) return Result.Fail("Nota fiscal não encontrada.");

        try
        {
            nota.Rejeitar(motivo);
            await _notasRepo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    // ============================================================
    // helpers internos
    // ============================================================

    private async Task<Fornecedor> ObterOuCriarFornecedorAsync(XmlNotaFiscalParseado dados)
    {
        var existente = await _fornecedoresRepo.ObterPorCnpjAsync(dados.EmitenteCnpj);
        if (existente is not null) return existente;

        // ValueObjects exigem valores válidos — usamos placeholders para campos
        // que a NF-e não traz (e-mail/telefone do emitente são opcionais no schema).
        var email = string.IsNullOrWhiteSpace(dados.EmitenteEmail)
            ? $"sem-email-{dados.EmitenteCnpj}@fornecedor.local"
            : dados.EmitenteEmail;
        var telefone = string.IsNullOrWhiteSpace(dados.EmitenteTelefone)
            ? "0000000000"
            : dados.EmitenteTelefone;

        var fornecedor = new Fornecedor(
            razaoSocial: dados.EmitenteRazaoSocial,
            nomeFantasia: dados.EmitenteNomeFantasia,
            cnpj: dados.EmitenteCnpj,
            email: email,
            telefone: telefone);

        await _fornecedoresRepo.AddAsync(fornecedor);
        await _fornecedoresRepo.SaveChangesAsync();
        return fornecedor;
    }

    private async Task<Componente?> TentarAutoMapearAsync(string codigoFornecedor)
    {
        if (string.IsNullOrWhiteSpace(codigoFornecedor)) return null;
        var todos = await _componentesRepo.GetAllAsync();
        return todos.FirstOrDefault(c =>
            string.Equals(c.PartNumber, codigoFornecedor, StringComparison.OrdinalIgnoreCase)
            || string.Equals(c.CodigoOEM, codigoFornecedor, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Result<IEnumerable<SugestaoComponenteDTO>>> SugerirComponentesAsync(Guid itemId)
    {
        var item = await _notasRepo.ObterItemAsync(itemId);
        if (item is null) return Result<IEnumerable<SugestaoComponenteDTO>>.Fail("Item não encontrado.");

        var todos = (await _componentesRepo.GetAllAsync()).Where(c => c.Ativo).ToList();
        if (todos.Count == 0)
            return Result<IEnumerable<SugestaoComponenteDTO>>.Ok(Enumerable.Empty<SugestaoComponenteDTO>());

        var cProd = (item.CodigoProdutoFornecedor ?? "").Trim();
        var xProd = (item.DescricaoProdutoFornecedor ?? "").Trim();
        var ncm = (item.Ncm ?? "").Trim();

        var sugestoes = new List<SugestaoComponenteDTO>();

        foreach (var c in todos)
        {
            int score = 0;
            var motivos = new List<string>();

            // 1. Match exato em PartNumber → confiança máxima
            if (!string.IsNullOrEmpty(cProd) && !string.IsNullOrEmpty(c.PartNumber)
                && string.Equals(cProd, c.PartNumber, StringComparison.OrdinalIgnoreCase))
            { score += 100; motivos.Add("PartNumber idêntico ao cProd"); }

            // 2. Match exato em CodigoOEM
            if (!string.IsNullOrEmpty(cProd) && !string.IsNullOrEmpty(c.CodigoOEM)
                && string.Equals(cProd, c.CodigoOEM, StringComparison.OrdinalIgnoreCase))
            { score += 95; motivos.Add("OEM idêntico ao cProd"); }

            // 3. Match exato em SKU
            if (!string.IsNullOrEmpty(cProd) && !string.IsNullOrEmpty(c.SKUInterno)
                && string.Equals(cProd, c.SKUInterno, StringComparison.OrdinalIgnoreCase))
            { score += 80; motivos.Add("SKU idêntico"); }

            // 4. cProd aparece como substring em PartNumber/OEM (ou vice-versa)
            if (score == 0 && !string.IsNullOrEmpty(cProd))
            {
                if (!string.IsNullOrEmpty(c.PartNumber)
                    && (c.PartNumber.Contains(cProd, StringComparison.OrdinalIgnoreCase)
                        || cProd.Contains(c.PartNumber, StringComparison.OrdinalIgnoreCase)))
                { score += 70; motivos.Add("PartNumber parcial"); }
                else if (!string.IsNullOrEmpty(c.CodigoOEM)
                    && (c.CodigoOEM.Contains(cProd, StringComparison.OrdinalIgnoreCase)
                        || cProd.Contains(c.CodigoOEM, StringComparison.OrdinalIgnoreCase)))
                { score += 65; motivos.Add("OEM parcial"); }
            }

            // 5. Tokens da descrição (xProd) presentes no Nome do componente
            if (score < 80 && !string.IsNullOrEmpty(xProd) && !string.IsNullOrEmpty(c.Nome))
            {
                var tokens = xProd.Split(new[] { ' ', '-', '/', ',' },
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(t => t.Length >= 3)
                    .ToList();
                if (tokens.Count > 0)
                {
                    var nomeUp = c.Nome.ToUpperInvariant();
                    var hits = tokens.Count(t => nomeUp.Contains(t.ToUpperInvariant()));
                    if (hits > 0)
                    {
                        // até 60 pontos baseado em proporção de tokens encontrados
                        var ganho = (int)(60 * ((double)hits / tokens.Count));
                        score += ganho;
                        motivos.Add($"{hits}/{tokens.Count} palavras da descrição no nome");
                    }
                }
            }

            // 6. NCM igual — sinal fraco mas indica mesma classe fiscal
            if (!string.IsNullOrEmpty(ncm) && !string.IsNullOrEmpty(c.NCM)
                && string.Equals(ncm, c.NCM, StringComparison.OrdinalIgnoreCase))
            { score += 20; motivos.Add("Mesmo NCM"); }

            if (score > 0)
            {
                sugestoes.Add(new SugestaoComponenteDTO
                {
                    ComponenteId = c.Id,
                    Nome = c.Nome,
                    SKUInterno = c.SKUInterno,
                    PartNumber = c.PartNumber,
                    CodigoOEM = c.CodigoOEM,
                    MarcaFabricante = c.MarcaFabricante,
                    Score = Math.Min(100, score),
                    Motivo = string.Join(" + ", motivos)
                });
            }
        }

        var top = sugestoes
            .OrderByDescending(s => s.Score)
            .Take(5)
            .ToList();

        return Result<IEnumerable<SugestaoComponenteDTO>>.Ok(top);
    }

    public async Task<Result<Guid>> CriarComponenteEVincularAsync(
        Guid itemId, CarStoreManager.Application.DTOs.Oficina.Componente.CriarComponenteDTO componenteDto)
    {
        var item = await _notasRepo.ObterItemAsync(itemId);
        if (item is null) return Result<Guid>.Fail("Item não encontrado.");

        if (item.NotaFiscal.Status != Domain.Enums.StatusNotaFiscal.ImportadaAguardandoAprovacao)
            return Result<Guid>.Fail("Itens só podem ser alterados enquanto a nota está pendente.");

        try
        {
            // Pré-popula campos faltantes do DTO com info do XML — admin pode
            // ter deixado em branco confiando no preenchimento automático.
            if (string.IsNullOrWhiteSpace(componenteDto.Nome))
                componenteDto.Nome = item.DescricaoProdutoFornecedor;
            if (string.IsNullOrWhiteSpace(componenteDto.PartNumber))
                componenteDto.PartNumber = item.CodigoProdutoFornecedor;
            if (string.IsNullOrWhiteSpace(componenteDto.NCM))
                componenteDto.NCM = item.Ncm;
            if (string.IsNullOrWhiteSpace(componenteDto.Unidade))
                componenteDto.Unidade = item.Unidade;
            if (componenteDto.CustoUnitario == 0)
                componenteDto.CustoUnitario = item.ValorUnitario;

            var componente = Application.Mappings.Oficina.ComponenteMapping.FromCriarDto(componenteDto);
            if (!string.IsNullOrWhiteSpace(componenteDto.Sistema)
                && Enum.TryParse<Domain.Enums.SistemaComponente>(componenteDto.Sistema, true, out var s))
                componente.DefinirSistema(s);

            // Aplica precificação automática (custo da NF + margem do sistema)
            var cfg = await _configRepo.ObterAsync();
            var margem = componenteDto.MargemLucroPct
                ?? cfg.ObterMargemParaSistema(componente.Sistema);
            componente.AplicarPrecificacao(componenteDto.CustoUnitario, margem);

            await _componentesRepo.AddAsync(componente);
            // Já vincula o item à peça recém-criada
            item.VincularComponente(componente.Id);

            await _notasRepo.SaveChangesAsync();
            return Result<Guid>.Ok(componente.Id);
        }
        catch (Exception ex) { return Result<Guid>.Fail(ex.Message); }
    }

    private async Task ConciliarItensAguardandoAsync(Guid componenteId)
    {
        try
        {
            var ordens = (await _ordensRepo.ObterComItensAguardandoAsync(componenteId)).ToList();
            if (ordens.Count == 0) return;

            foreach (var ordem in ordens)
            {
                var itens = ordem.Itens.Where(i =>
                    i.ComponenteId == componenteId
                    && i.Origem == OrigemItemOrdemServico.Encomenda
                    && i.StatusItem == StatusItemOrdemServico.AguardandoChegada);

                foreach (var item in itens)
                    item.MarcarComoRecebido();

                _ordensRepo.Update(ordem);
            }

            await _ordensRepo.SaveChangesAsync();
        }
        catch
        {
            // não falha a aprovação por causa da conciliação
        }
    }

    // ============================================================
    // mapping
    // ============================================================

    private static NotaFiscalDTO MapToDto(Domain.Entities.Oficina.NotaFiscal n) => new()
    {
        Id = n.Id,
        Numero = n.Numero,
        Serie = n.Serie,
        ChaveAcesso = n.ChaveAcesso,
        DataEmissao = n.DataEmissao,
        DataImportacao = n.DataImportacao,
        DataAprovacao = n.DataAprovacao,
        Status = n.Status.ToString(),
        MotivoRejeicao = n.MotivoRejeicao,
        FornecedorId = n.FornecedorId,
        FornecedorRazaoSocial = n.Fornecedor?.RazaoSocial ?? "",
        FornecedorCnpj = n.Fornecedor?.Cnpj?.Numero ?? "",
        ValorProdutos = n.ValorProdutos,
        ValorImpostos = n.ValorImpostos,
        ValorTotal = n.ValorTotal,
        Itens = n.Itens.Select(MapItemToDto).ToList()
    };

    private static ItemNotaFiscalDTO MapItemToDto(ItemNotaFiscal i) => new()
    {
        Id = i.Id,
        ComponenteId = i.ComponenteId,
        ComponenteNome = i.Componente?.Nome,
        ComponentePartNumber = i.Componente?.PartNumber,
        CodigoProdutoFornecedor = i.CodigoProdutoFornecedor,
        DescricaoProdutoFornecedor = i.DescricaoProdutoFornecedor,
        Ncm = i.Ncm,
        Unidade = i.Unidade,
        Quantidade = i.Quantidade,
        ValorUnitario = i.ValorUnitario,
        ValorTotal = i.ValorTotal,
        Cfop = i.Cfop,
        AliquotaIcms = i.AliquotaIcms,
        ValorIcms = i.ValorIcms,
        NumeroLoteFornecedor = i.NumeroLoteFornecedor,
        DataValidade = i.DataValidade
    };

    private static NotaFiscalListaDTO MapToListaDto(Domain.Entities.Oficina.NotaFiscal n) => new()
    {
        Id = n.Id,
        Numero = n.Numero,
        Serie = n.Serie,
        ChaveAcesso = n.ChaveAcesso,
        DataEmissao = n.DataEmissao,
        DataImportacao = n.DataImportacao,
        Status = n.Status.ToString(),
        FornecedorRazaoSocial = n.Fornecedor?.RazaoSocial ?? "",
        FornecedorCnpj = n.Fornecedor?.Cnpj?.Numero ?? "",
        ValorTotal = n.ValorTotal,
        TotalItens = n.Itens.Count,
        ItensMapeados = n.Itens.Count(i => i.ComponenteId is not null)
    };
}
