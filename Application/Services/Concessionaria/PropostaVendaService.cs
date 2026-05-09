using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Concessionaria;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de PropostaVendaService.cs.

    Esta classe tem testes automaticos implementados para:
        nada ainda
*/

public class PropostaVendaService : IPropostaVendaService
{
    private readonly IPropostaVendaRepository _repository;
    private readonly IVeiculoVendaRepository _veiculoRepository;

    public PropostaVendaService(
        IPropostaVendaRepository repository,
        IVeiculoVendaRepository veiculoRepository)
    {
        _repository = repository;
        _veiculoRepository = veiculoRepository;
    }

    /*
        metodo que busca a proposta por Id
        falha caso seja vazia
    */
    public async Task<Result<PropostaVendaDTO>> GetByIdAsync(Guid id)
    {
        var proposta = await _repository.GetByIdAsync(id);
        if (proposta is null)
            return Result<PropostaVendaDTO>.Fail("Proposta não encontrada");

        return Result<PropostaVendaDTO>.Ok(PropostaVendaMapping.ToDto(proposta));
    }

    //busca todas as propostas
    public async Task<Result<IEnumerable<PropostaVendaListaDTO>>> GetAllAsync()
    {
        var propostas = await _repository.GetAllAsync();
        return Result<IEnumerable<PropostaVendaListaDTO>>.Ok(
            propostas.Select(PropostaVendaMapping.ToListaDto));
    }

    
    //metodo que busca a proposta pelo id do vendedor vinculado
    public async Task<Result<IEnumerable<PropostaVendaListaDTO>>> ObterPorVendedorAsync(Guid vendedorId)
    {
        var propostas = await _repository.ObterPorVendedorAsync(vendedorId);
        return Result<IEnumerable<PropostaVendaListaDTO>>.Ok(
            propostas.Select(PropostaVendaMapping.ToListaDto));
    }

    //metodo que busca a proposta pelo id do cliente vinculado
    public async Task<Result<IEnumerable<PropostaVendaListaDTO>>> ObterPorClienteAsync(Guid clienteId)
    {
        var propostas = await _repository.ObterPorClienteAsync(clienteId);
        return Result<IEnumerable<PropostaVendaListaDTO>>.Ok(
            propostas.Select(PropostaVendaMapping.ToListaDto));
    }

    /*
        metodo que cria novas propostas
        valida que o veiculo exista,
        valida que o veiculo estaja disponivel
    */
    public async Task<Result<Guid>> AddAsync(CriarPropostaVendaDTO dto)
    {
        var veiculo = await _veiculoRepository.GetByIdAsync(dto.VeiculoVendaId);
        if (veiculo is null)
            return Result<Guid>.Fail("Veículo não encontrado");

        if (veiculo.Disponibilidade != Domain.Enums.DisponibilidadeVeiculo.Disponivel)
            return Result<Guid>.Fail("Veículo não está disponível para venda");

        try
        {
            var proposta = PropostaVendaMapping.ToEntity(dto);
            await _repository.AddAsync(proposta);
            await _repository.SaveChangesAsync();
            return Result<Guid>.Ok(proposta.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar proposta: {ex.Message}");
        }
    }

    /*
        metodo que aplica o desconto da venda
        falha caso a proposta nao seja encontrada
    */
    public async Task<Result> AplicarDescontoAsync(AplicarDescontoDTO dto)
    {
        var proposta = await _repository.GetByIdAsync(dto.PropostaId);
        if (proposta is null) 
            return Result.Fail("Proposta não encontrada");

        try
        {
            proposta.AplicarDesconto(PropostaVendaMapping.ToDesconto(dto));
            _repository.Update(proposta);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    /*
        metodo para colocar valor da entrada do financiamento
        falha caso a proposta nao seja encontrada
    */
    public async Task<Result> DefinirEntradaAsync(DefinirEntradaDTO dto)
    {
        var proposta = await _repository.GetByIdAsync(dto.PropostaId);
        if (proposta is null) 
            return Result.Fail("Proposta não encontrada");

        try
        {
            proposta.AtualizarEntrada(dto.ValorEntrada);
            _repository.Update(proposta);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    /*
        metodo que gera o financiamento completo
        falha caso a proposta nao seja encontrada
    */
    public async Task<Result> GerarFinanciamentoAsync(GerarFinanciamentoDTO dto)
    {
        var proposta = await _repository.GetByIdAsync(dto.PropostaId);
        if (proposta is null) return Result.Fail("Proposta não encontrada");

        try
        {
            proposta.GerarFinanciamento(dto.valorBase, dto.Parcelas, dto.entrada);
            _repository.Update(proposta);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    /*
        metodo para aprovar a proposta
        marca o veiculo como vendido automaticamente
        falha caso a proposta nao seja encontrada
    */
    public async Task<Result> AprovarAsync(Guid propostaId)
    {
        var proposta = await _repository.GetByIdAsync(propostaId);
        if (proposta is null) 
            return Result.Fail("Proposta não encontrada");

        try
        {
            proposta.Aprovar();

            var veiculo = await _veiculoRepository.GetByIdAsync(proposta.VeiculoVendaId);
            veiculo?.MarcarComoVendido();
            if (veiculo is not null) _veiculoRepository.Update(veiculo);

            _repository.Update(proposta);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    /*
        metodo para marcar proposta como rejeitada
        falha caso a proposta nao seja encontrada
    */
    public async Task<Result> RejeitarAsync(Guid propostaId)
    {
        var proposta = await _repository.GetByIdAsync(propostaId);
        if (proposta is null) 
            return Result.Fail("Proposta não encontrada");

        try
        {
            proposta.Rejeitar();
            _repository.Update(proposta);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    /*
        metodo para marcar proposta como cancelada
        falha caso a proposta nao seja encontrada
    */
    public async Task<Result> CancelarAsync(Guid propostaId)
    {
        var proposta = await _repository.GetByIdAsync(propostaId);
        if (proposta is null) 
            return Result.Fail("Proposta não encontrada");

        proposta.Cancelar();
        _repository.Update(proposta);
        await _repository.SaveChangesAsync();
        return Result.Ok();
    }

    //metodo ainda nao implementado
    public async Task<Result> UpdateAsync(PropostaVendaDTO dto)
    {
        return Result.Fail("Metodo vazio");   
    }

    /*
        metodo que remove proposta por id
        caso seja vazia retorna erro
    */    
    public async Task<Result> RemoveAsync(Guid propostaVendaId)
    {
        var proposta = await _repository.GetByIdAsync(propostaVendaId);

        if (proposta is null)
            return Result.Fail("Mecânico não encontrado");

        _repository.Remove(proposta);
        await _repository.SaveChangesAsync();

        return Result.Ok();   
    }
}