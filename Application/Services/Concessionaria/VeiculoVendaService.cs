// Service da entidade VeiculoVenda : Herda da interface IVeiculoVenda.cs

using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs;
using CarStoreManager.Application.DTOs.Concessionaria.VeiculoVenda;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Concessionaria;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de VeiculoVendaService.cs.

    Esta classe tem testes automaticos implementados para:
        nada ainda
*/

public class VeiculoVendaService : IVeiculoVendaService
{
    private readonly IVeiculoVendaRepository _repository;

    public VeiculoVendaService(IVeiculoVendaRepository repository)
    {
        _repository = repository;
    }

    /*
        metodo de busca por id valida que
        caso veiculoVenda buscado seja vazio
        retorna o aviso que não foi encontrado
    */
    public async Task<Result<VeiculoVendaDTO>> GetByIdAsync(Guid id)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        if (veiculo is null)
            return Result<VeiculoVendaDTO>.Fail("Veículo não encontrado");

        return Result<VeiculoVendaDTO>.Ok(VeiculoVendaMapping.ToDto(veiculo));
    }

    //buscar todos os veiculoVenda
    public async Task<Result<IEnumerable<VeiculoVendaListaDTO>>> GetAllAsync()
    {
        var veiculos = await _repository.GetAllAsync();
        return Result<IEnumerable<VeiculoVendaListaDTO>>.Ok(
            veiculos.Select(VeiculoVendaMapping.ToListaDto));
    }

    //metodo que filtra os veiculoVenda com status disponivel
    public async Task<Result<IEnumerable<VeiculoVendaListaDTO>>> ObterDisponiveisAsync()
    {
        var veiculos = await _repository.ObterDisponiveisAsync();
        return Result<IEnumerable<VeiculoVendaListaDTO>>.Ok(
            veiculos.Select(VeiculoVendaMapping.ToListaDto));
    }

    public async Task<Result<IEnumerable<string>>> ListarMarcasDistintasAsync()
    {
        var veiculos = await _repository.GetAllAsync();
        var marcas = veiculos
            .Select(v => v.GetMarca())
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(m => m)
            .ToList();
        return Result<IEnumerable<string>>.Ok(marcas);
    }

    //metodo para criar novo veiculoVenda
    public async Task<Result<Guid>> AddAsync(CriarVeiculoVendaDTO dto)
    {
        try
        {
            var veiculo = VeiculoVendaMapping.ToEntity(dto);
            await _repository.AddAsync(veiculo);
            await _repository.SaveChangesAsync();
            return Result<Guid>.Ok(veiculo.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar veículo: {ex.Message}");
        }
    }

    /*
        metodo que atualiza veiculoVenda ja existente
        faz busca por id e caso veiculoVenda seja vazio 
        retona o aviso que nao foi encontrado
    */
    public async Task<Result> UpdateAsync(AtualizarVeiculoVendaDTO dto)
    {
        var veiculo = await _repository.GetByIdAsync(dto.Id);
        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        try
        {
            VeiculoVendaMapping.UpdateEntity(veiculo, dto);
            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /*
        metodo que muda o status do veiculoVenda
        para vendido falha caso o veiculo seja vazio
    */
    public async Task<Result> MarcarComoVendidoAsync(Guid id)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        try
        {
            veiculo.MarcarComoVendido();
            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /*
        metodo que muda o status do veiculoVenda
        para disponivel falha caso o veiculo seja vazio
    */
    public async Task<Result> MarcarComoDisponivelAsync(Guid id)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        try
        {
            veiculo.MarcarComoDisponivel();
            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /// <summary>
    /// Tira o veículo da fase de preparação e o disponibiliza para venda.
    /// Falha se o veículo não estiver em <c>EmPreparacao</c>.
    /// </summary>
    public async Task<Result> LiberarParaVendaAsync(Guid id)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        if (veiculo is null) return Result.Fail("Veículo não encontrado");

        try
        {
            veiculo.LiberarParaVenda();
            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    /// <summary>
    /// Volta o veículo para preparação (precisa de retoque, doc adicional, etc).
    /// Falha se o veículo não estiver em <c>Disponivel</c>.
    /// </summary>
    public async Task<Result> VoltarParaPreparacaoAsync(Guid id)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        if (veiculo is null) return Result.Fail("Veículo não encontrado");

        try
        {
            veiculo.VoltarParaPreparacao();
            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    /*
        metodo que atualiza a quilometragem do veiculoVenda
        falha caso o veiculo seja vazio
    */
    public async Task<Result> AtualizarQuilometragemAsync(Guid id, int km)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        try
        {
            veiculo.AlterarQuilometragem(km);
            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /*
        metodo que adiciona uma foto ao anuncio
        falha caso o veiculo nao exista
    */
    public async Task<Result> AdicionarFotoAsync(Guid id, string url)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        try
        {
            veiculo.AdicionarFoto(url);
            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /*
        metodo que remove uma foto do anuncio
        falha caso o veiculo nao exista
    */
    public async Task<Result> RemoverFotoAsync(Guid id, Guid fotoId)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        try
        {
            veiculo.RemoverFoto(fotoId);
            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> ReordenarFotosAsync(Guid id, List<Guid> fotos)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        try
        {
            // Reordena conforme a sequência informada de IDs de foto.
            // Fotos não listadas mantêm sua ordem relativa após as listadas.
            var lookup = veiculo.Fotos.ToDictionary(f => f.Id);
            var ordem = 0;
            foreach (var fotoId in fotos)
            {
                if (!lookup.TryGetValue(fotoId, out var foto))
                    return Result.Fail($"Foto {fotoId} não pertence ao veículo");
                foto.AtualizarOrdem(ordem++);
            }
            foreach (var foto in veiculo.Fotos.Where(f => !fotos.Contains(f.Id)))
                foto.AtualizarOrdem(ordem++);

            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /*
        metodo que remove veiculoVenda por id
        caso seja vazio retona 
        o aviso que nao foi encontrado
    */
    public async Task<Result> RemoveAsync(Guid id)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        _repository.Remove(veiculo);
        await _repository.SaveChangesAsync();
        return Result.Ok();
    }
}