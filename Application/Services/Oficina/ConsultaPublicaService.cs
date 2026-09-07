using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

public class ConsultaPublicaService : IConsultaPublicaService
{
    private readonly IClienteRepository _clientes;
    private readonly IVeiculoClienteRepository _veiculos;
    private readonly IOrdemServicoRepository _ordens;

    // Status "em aberto" — trabalho ainda em curso, não entregue nem cancelado.
    private static readonly StatusOrdemServico[] EmAberto =
    {
        StatusOrdemServico.Pendente, StatusOrdemServico.EmAnalise,
        StatusOrdemServico.BuscandoPecasParaOrcamento, StatusOrdemServico.AguardandoCliente,
        StatusOrdemServico.Aprovada, StatusOrdemServico.EmAndamento,
        StatusOrdemServico.Pausada, StatusOrdemServico.PagamentoPendente,
        StatusOrdemServico.Finalizada
    };

    public ConsultaPublicaService(
        IClienteRepository clientes,
        IVeiculoClienteRepository veiculos,
        IOrdemServicoRepository ordens)
    {
        _clientes = clientes;
        _veiculos = veiculos;
        _ordens = ordens;
    }

    public async Task<Result<ConsultaPublicaResultadoDTO>> ConsultarPorCpfEPlacaAsync(string cpf, string placa)
    {
        const string generico = "Não encontramos nenhum cadastro com esse CPF e placa. Confira os dados e tente novamente.";

        if (string.IsNullOrWhiteSpace(cpf) || string.IsNullOrWhiteSpace(placa))
            return Result<ConsultaPublicaResultadoDTO>.Fail("Informe o CPF e a placa de um dos seus veículos.");

        var cpfDigitos = new string(cpf.Where(char.IsDigit).ToArray());
        var placaNorm = new string(placa.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        var cliente = await _clientes.ObterPorCpfAsync(cpfDigitos);
        if (cliente is null)
            return Result<ConsultaPublicaResultadoDTO>.Fail(generico);

        var veiculos = (await _veiculos.ObterPorClienteAsync(cliente.Id)).ToList();

        // A placa informada precisa pertencer a um veículo daquele CPF (2º fator).
        var placaConfere = veiculos.Any(v =>
            new string(v.Placa.GetPlaca().Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant() == placaNorm);
        if (!placaConfere)
            return Result<ConsultaPublicaResultadoDTO>.Fail(generico);

        var ordens = (await _ordens.ObterPorClienteAsync(cliente.Id)).ToList();
        var porVeiculo = ordens.GroupBy(o => o.GetVeiculoClienteId())
                               .ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.DataCriacao).ToList());

        var resultado = new ConsultaPublicaResultadoDTO
        {
            PrimeiroNome = cliente.GetNome().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? cliente.GetNome(),
            Veiculos = veiculos.Select(v => new VeiculoHistoricoPublicoDTO
            {
                Id = v.Id,
                Descricao = $"{v.GetMarca()} {v.GetModelo()} {v.GetAno()}",
                Placa = v.Placa.GetPlaca(),
                Historico = (porVeiculo.TryGetValue(v.Id, out var lst) ? lst : new())
                    .Select(OrdemServicoMapping.ToPublicaDto).ToList()
            }).ToList()
        };

        var emAberto = ordens
            .Where(o => EmAberto.Contains(o.Status))
            .OrderByDescending(o => o.DataCriacao)
            .FirstOrDefault();
        if (emAberto is not null)
            resultado.OrdemEmAberto = OrdemServicoMapping.ToPublicaDto(emAberto);

        return Result<ConsultaPublicaResultadoDTO>.Ok(resultado);
    }
}
