using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.Componente;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de ComponenteService.cs.

    Esta classe tem testes automaticos implementados para:
        nada ainda
*/

public class ComponenteService : IComponenteService
{
    private readonly IComponenteRepository _repository;
    private readonly Domain.Interfaces.Repositories.Sistema.IConfiguracaoSistemaRepository _configRepo;
    private readonly IEstoqueRepository? _estoqueRepo;
    private readonly IFornecedorRepository _fornecedorRepo;
    private readonly IComponenteEquivalenteRepository _equivalenteRepo;

    public ComponenteService(
        IComponenteRepository repository,
        Domain.Interfaces.Repositories.Sistema.IConfiguracaoSistemaRepository configRepo,
        IFornecedorRepository fornecedorRepo,
        IComponenteEquivalenteRepository equivalenteRepo,
        IEstoqueRepository? estoqueRepo = null)
    {
        _repository = repository;
        _configRepo = configRepo;
        _fornecedorRepo = fornecedorRepo;
        _equivalenteRepo = equivalenteRepo;
        _estoqueRepo = estoqueRepo;
    }

    private async Task<int> ObterQuantidadeEmEstoqueAsync(Guid componenteId)
    {
        if (_estoqueRepo is null) return 0;
        var est = await _estoqueRepo.ObterPorComponenteAsync(componenteId);
        return est?.QuantidadeAtual ?? 0;
    }

    /* ======================
        metodos de PESQUISA
     =======================*/

    /*
        metodo de busca por id valida que
        caso componente buscado seja vazio
        retorna o aviso que não foi encontrado
    */
    public async Task<Result<ComponenteDTO>> GetByIdAsync(Guid id)
    {
        var componente = await _repository.GetByIdAsync(id);

        if (componente is null)
            return Result<ComponenteDTO>.Fail("Componente não encontrado");

        var dto = ComponenteMapping.ToDto(componente);

        var fornecedor = await _fornecedorRepo.GetByIdAsync(componente.FornecedorId);
        dto.FornecedorNome = fornecedor?.Nome ?? "";

        return Result<ComponenteDTO>.Ok(dto);
    }

    //busca todos os componentes
    public async Task<Result<IEnumerable<ComponenteListaDTO>>> GetAllAsync()
    {
        var componentes = await _repository.GetAllAsync();

        var lista = componentes
            .Select(ComponenteMapping.ToListaDto);

        return Result<IEnumerable<ComponenteListaDTO>>.Ok(lista);
    }

    /*
        Estoque agora vive em EstoqueComponente (entidade separada);
        depende de IEstoqueRepository ainda não implementado.
    */
    public Task<Result<IEnumerable<ComponenteDTO>>> ObterComEstoqueBaixoAsync()
        => Task.FromResult(Result<IEnumerable<ComponenteDTO>>.Fail(
            "ObterComEstoqueBaixoAsync ainda não implementado — depende de IEstoqueRepository."));

    /*
        Componente atualmente não tem propriedade Sistema; o filtro por
        SistemaComponente exige primeiro adicionar essa coluna na entidade.
    */
    public Task<Result<IEnumerable<ComponenteDTO>>> ObterPorSistemaAsync(string sistema)
        => Task.FromResult(Result<IEnumerable<ComponenteDTO>>.Fail(
            "ObterPorSistemaAsync ainda não implementado — Componente precisa do campo Sistema."));

    public async Task<Result<IEnumerable<ComponenteBuscaDTO>>> BuscarAsync(string termo, int limite = 20)
    {
        var componentes = (await _repository.BuscarAsync(termo, limite)).ToList();

        // Faz lookup do estoque atual para cada componente — assim o autocomplete
        // já mostra "X em estoque" e o frontend pode decidir se mostra warning
        // quando o operador tenta vender mais do que existe.
        var resultado = new List<ComponenteBuscaDTO>(componentes.Count);
        foreach (var c in componentes)
        {
            int qtd = 0;
            if (_estoqueRepo is not null)
            {
                var est = await _estoqueRepo.ObterPorComponenteAsync(c.Id);
                qtd = est?.QuantidadeAtual ?? 0;
            }

            resultado.Add(new ComponenteBuscaDTO
            {
                Id = c.Id,
                Nome = c.Nome,
                SKUInterno = c.SKUInterno,
                PartNumber = c.PartNumber,
                CodigoOEM = c.CodigoOEM,
                MarcaFabricante = c.MarcaFabricante,
                Categoria = c.Categoria,
                ValorVenda = c.ValorVenda,
                QuantidadeEmEstoque = qtd
            });
        }

        return Result<IEnumerable<ComponenteBuscaDTO>>.Ok(resultado);
    }

    public async Task<Result<Guid>> AddAsync(CriarComponenteDTO dto)
    {
        var fornecedor = await _fornecedorRepo.GetByIdAsync(dto.FornecedorId);
        if (fornecedor is null)
            return Result<Guid>.Fail("Selecione um fornecedor já cadastrado na lista de sugestões.");

        try
        {
            var componente = ComponenteMapping.FromCriarDto(dto);

            // Sistema (enum) — opcional no DTO via string
            Domain.Enums.SistemaComponente? sistema = null;
            if (!string.IsNullOrWhiteSpace(dto.Sistema)
                && Enum.TryParse<Domain.Enums.SistemaComponente>(dto.Sistema, ignoreCase: true, out var s))
                sistema = s;
            componente.DefinirSistema(sistema);

            // Aplica precificação automática: usa margem do DTO se vier;
            // senão pega o padrão configurado para o Sistema (ou global).
            decimal margem;
            if (dto.MargemLucroPct.HasValue)
            {
                margem = dto.MargemLucroPct.Value;
            }
            else
            {
                var cfg = await _configRepo.ObterAsync();
                margem = cfg.ObterMargemParaSistema(sistema);
            }
            componente.AplicarPrecificacao(dto.CustoUnitario, margem);

            await _repository.AddAsync(componente);
            await _repository.SaveChangesAsync();
            return Result<Guid>.Ok(componente.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar componente: {ex.Message}");
        }
    }

    /// <summary>
    /// Sobrescreve a margem individual do componente (recalcula ValorVenda
    /// mantendo o custo atual). Endpoint admin "ajustar margem".
    /// </summary>
    public async Task<Result> AjustarMargemAsync(Guid id, decimal novaMargemPct)
    {
        var c = await _repository.GetByIdAsync(id);
        if (c is null) return Result.Fail("Componente não encontrado");
        try
        {
            c.AjustarMargem(novaMargemPct);
            _repository.Update(c);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    /*
        metodo que atualiza componente ja existente
        faz busca por id e caso componente seja vazio 
        retona o aviso que nao foi encontrado
    */
    public async Task<Result> UpdateAsync(AtualizarComponenteDTO dto)
    {
        var componente = await _repository.GetByIdAsync(dto.Id);

        if (componente is null)
            return Result.Fail("Componente não encontrado");

        var fornecedor = await _fornecedorRepo.GetByIdAsync(dto.FornecedorId);
        if (fornecedor is null)
            return Result.Fail("Selecione um fornecedor já cadastrado na lista de sugestões.");

        try
        {
            ComponenteMapping.ApplyAtualizarDto(componente, dto);
            _repository.Update(componente);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /* ===========================
        gerencimento de ESTOQUE
     ===========================*/

    // Estoque vive em EstoqueComponente (entidade separada).
    // EntradaEstoque/SaidaEstoque foram desativados aqui até IEstoqueRepository existir.
    public Task<Result> EntradaEstoqueAsync(Guid id, int quantidade)
        => Task.FromResult(Result.Fail("EntradaEstoqueAsync ainda não implementado — depende de IEstoqueRepository."));

    public Task<Result> SaidaEstoqueAsync(Guid id, int quantidade)
        => Task.FromResult(Result.Fail("SaidaEstoqueAsync ainda não implementado — depende de IEstoqueRepository."));

    /// <summary>
    /// Encontra peças que servem como substituto — união de duas trilhas:
    /// (1) mesmo CodigoOEM (cross-brand, automático — ex.: pastilha Bosch e
    /// pastilha Fras-le com o mesmo OEM da montadora), e (2) vínculos
    /// curados manualmente em ComponenteEquivalente (peças de marca/tipo
    /// diferente sem OEM em comum, mas que um usuário confirmou que servem).
    /// O componente-alvo NÃO precisa estar ativo (peça descontinuada
    /// continua sendo um ponto de partida válido pra busca de substituto) —
    /// só os candidatos sugeridos precisam estar ativos.
    /// </summary>
    public async Task<Result<List<ComponenteSugestaoDTO>>> ObterEquivalentesAsync(Guid componenteId)
    {
        var alvo = await _repository.GetByIdAsync(componenteId);
        if (alvo is null)
            return Result<List<ComponenteSugestaoDTO>>.Fail("Componente não encontrado");

        var candidatos = new Dictionary<Guid, ComponenteSugestaoDTO>();

        // Trilha 1 — mesmo CodigoOEM.
        if (!string.IsNullOrWhiteSpace(alvo.CodigoOEM))
        {
            var todos = await _repository.GetAllAsync();
            foreach (var c in todos)
            {
                if (c.Id == alvo.Id || !c.Ativo) continue;
                if (string.IsNullOrWhiteSpace(c.CodigoOEM)) continue;
                if (!string.Equals(c.CodigoOEM, alvo.CodigoOEM, StringComparison.OrdinalIgnoreCase)) continue;

                candidatos[c.Id] = MapSugestao(c, origemOEM: true, origemCurada: false, tipo: null);
            }
        }

        // Trilha 2 — vínculos curados (nas duas direções da FK).
        var ligacoes = await _equivalenteRepo.ObterPorComponenteAsync(componenteId);
        foreach (var l in ligacoes)
        {
            var outro = l.ComponenteOriginalId == componenteId
                ? l.ComponenteEquivalenteRelacionado
                : l.ComponenteOriginal;
            if (outro is null || outro.Id == alvo.Id || !outro.Ativo) continue;

            if (candidatos.TryGetValue(outro.Id, out var existente))
            {
                existente.OrigemCurada = true;
                existente.TipoEquivalencia = l.TipoEquivalencia;
            }
            else
            {
                candidatos[outro.Id] = MapSugestao(outro, origemOEM: false, origemCurada: true, tipo: l.TipoEquivalencia);
            }
        }

        foreach (var dto in candidatos.Values)
            dto.QuantidadeEmEstoque = await ObterQuantidadeEmEstoqueAsync(dto.Id);

        var resultado = candidatos.Values
            .OrderByDescending(d => d.QuantidadeEmEstoque > 0)
            .ThenBy(d => d.Nome)
            .ToList();

        return Result<List<ComponenteSugestaoDTO>>.Ok(resultado);
    }

    private static ComponenteSugestaoDTO MapSugestao(Componente c, bool origemOEM, bool origemCurada, TipoEquivalencia? tipo) => new()
    {
        Id = c.Id,
        Nome = c.Nome,
        MarcaFabricante = c.MarcaFabricante,
        PartNumber = c.PartNumber,
        CodigoOEM = c.CodigoOEM,
        Categoria = c.Categoria,
        ValorVenda = c.ValorVenda,
        OrigemOEM = origemOEM,
        OrigemCurada = origemCurada,
        TipoEquivalencia = tipo,
    };

    /// <summary>
    /// Vínculos curados manualmente do componente (as duas direções da FK)
    /// — pra tela de curadoria. Não filtra por Ativo do relacionado: um
    /// vínculo pra uma peça já descontinuada continua aparecendo, com
    /// Ativo=false pra a tela sinalizar "descontinuado".
    /// </summary>
    public async Task<Result<List<ComponenteEquivalenteDTO>>> ListarLigacoesEquivalenciaAsync(Guid componenteId)
    {
        var alvo = await _repository.GetByIdAsync(componenteId);
        if (alvo is null)
            return Result<List<ComponenteEquivalenteDTO>>.Fail("Componente não encontrado");

        var ligacoes = await _equivalenteRepo.ObterPorComponenteAsync(componenteId);
        var resultado = new List<ComponenteEquivalenteDTO>();

        foreach (var l in ligacoes)
        {
            var outro = l.ComponenteOriginalId == componenteId
                ? l.ComponenteEquivalenteRelacionado
                : l.ComponenteOriginal;
            if (outro is null) continue;

            resultado.Add(new ComponenteEquivalenteDTO
            {
                Id = l.Id,
                ComponenteRelacionadoId = outro.Id,
                Nome = outro.Nome,
                MarcaFabricante = outro.MarcaFabricante,
                PartNumber = outro.PartNumber,
                TipoEquivalencia = l.TipoEquivalencia,
                Ativo = outro.Ativo,
                QuantidadeEmEstoque = await ObterQuantidadeEmEstoqueAsync(outro.Id),
            });
        }

        return Result<List<ComponenteEquivalenteDTO>>.Ok(resultado.OrderBy(r => r.Nome).ToList());
    }

    /// <summary>
    /// Cria um vínculo de equivalência curado manualmente. Checa duplicidade
    /// nas duas direções via repositório (não confia só na checagem em
    /// memória de Componente.AdicionarEquivalencia, que só olha uma direção).
    /// </summary>
    public async Task<Result<Guid>> CriarLigacaoEquivalenciaAsync(CriarComponenteEquivalenteDTO dto)
    {
        if (dto.ComponenteOriginalId == dto.ComponenteEquivalenteId)
            return Result<Guid>.Fail("Um componente não pode ser equivalente a si mesmo.");

        var original = await _repository.GetByIdAsync(dto.ComponenteOriginalId);
        if (original is null) return Result<Guid>.Fail("Componente original não encontrado");

        var equivalente = await _repository.GetByIdAsync(dto.ComponenteEquivalenteId);
        if (equivalente is null) return Result<Guid>.Fail("Componente equivalente não encontrado");

        var existente = await _equivalenteRepo.ObterLigacaoEntreAsync(dto.ComponenteOriginalId, dto.ComponenteEquivalenteId);
        if (existente is not null)
            return Result<Guid>.Fail("Essa equivalência já está registrada.");

        try
        {
            var ligacao = original.AdicionarEquivalencia(equivalente, dto.TipoEquivalencia);
            await _equivalenteRepo.AddAsync(ligacao);
            await _equivalenteRepo.SaveChangesAsync();
            return Result<Guid>.Ok(ligacao.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Fail(ex.Message);
        }
    }

    /// <summary>Remove um vínculo pelo próprio Id — sem ambiguidade de direção.</summary>
    public async Task<Result> RemoverLigacaoEquivalenciaAsync(Guid ligacaoId)
    {
        var ligacao = await _equivalenteRepo.GetByIdAsync(ligacaoId);
        if (ligacao is null)
            return Result.Fail("Vínculo de equivalência não encontrado");

        _equivalenteRepo.Remove(ligacao);
        await _equivalenteRepo.SaveChangesAsync();
        return Result.Ok();
    }

    /*
        metodo que remove componente por id
        caso seja vazio retona 
        o aviso que nao foi encontrado
    */
    public async Task<Result> RemoveAsync(Guid id)
    {
        var componente = await _repository.GetByIdAsync(id);

        if (componente is null)
            return Result.Fail("Componente não encontrado");

        try
        {
            _repository.Remove(componente);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception)
        {
            return Result.Fail("Não foi possível excluir o componente. Ele pode estar em uso em alguma ordem de serviço — considere desativá-lo em vez de excluir.");
        }
    }
}