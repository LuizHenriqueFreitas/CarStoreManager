using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Shared.Cliente;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

public class ClienteConsultaService : IClienteConsultaService
{
    private readonly IClienteRepository _clientes;
    private readonly IVeiculoClienteRepository _veiculos;
    private readonly IOrdemServicoRepository _ordens;
    private readonly IPropostaVendaRepository _propostas;

    private static readonly string[] OsAbertas =
    {
        "Pendente", "EmAnalise", "AguardandoCliente", "BuscandoPecasParaOrcamento",
        "Aprovada", "EmAndamento", "Pausada", "PagamentoPendente", "Finalizada"
    };

    public ClienteConsultaService(
        IClienteRepository clientes,
        IVeiculoClienteRepository veiculos,
        IOrdemServicoRepository ordens,
        IPropostaVendaRepository propostas)
    {
        _clientes = clientes;
        _veiculos = veiculos;
        _ordens = ordens;
        _propostas = propostas;
    }

    public async Task<Result<ClienteFichaDTO>> ObterFichaAsync(Guid clienteId)
    {
        var cliente = await _clientes.GetByIdAsync(clienteId);
        if (cliente is null) return Result<ClienteFichaDTO>.Fail("Cliente não encontrado");

        var veiculos = (await _veiculos.ObterPorClienteAsync(clienteId)).ToList();
        var ordens = (await _ordens.ObterPorClienteAsync(clienteId)).ToList();
        var propostas = (await _propostas.ObterPorClienteAsync(clienteId)).ToList();

        var osPorVeiculo = ordens.GroupBy(o => o.GetVeiculoClienteId())
                                 .ToDictionary(g => g.Key, g => g.Count());

        var histOficina = ordens
            .OrderByDescending(o => o.DataCriacao)
            .Select(o => new FichaEventoDTO
            {
                Id = o.Id,
                Data = o.DataCriacao,
                Referencia = o.NumeroPublico,
                Descricao = o.Tipo.ToString(),
                Status = o.Status.ToString(),
                Valor = o.GetValorTotal()
            }).ToList();

        var histConc = propostas
            .OrderByDescending(p => p.DataCriacao)
            .Select(p => new FichaEventoDTO
            {
                Id = p.Id,
                Data = p.DataCriacao,
                Referencia = $"#{p.Id.ToString()[..6].ToUpper()}",
                Descricao = "Proposta de venda",
                Status = p.Status.ToString(),
                Valor = p.GetValorFinal()
            }).ToList();

        var ultimoOficina = ordens.Any() ? ordens.Max(o => o.DataCriacao) : (DateTime?)null;
        var ultimoConc = propostas.Any() ? propostas.Max(p => p.DataCriacao) : (DateTime?)null;
        DateTime? ultimo = null; string setor = "";
        if (ultimoOficina.HasValue && (!ultimoConc.HasValue || ultimoOficina >= ultimoConc))
        { ultimo = ultimoOficina; setor = "Oficina"; }
        else if (ultimoConc.HasValue)
        { ultimo = ultimoConc; setor = "Concessionária"; }

        var ficha = new ClienteFichaDTO
        {
            Id = cliente.Id,
            Nome = cliente.GetNome(),
            Cpf = cliente.GetCpf(),
            Telefone = cliente.GetTelefone(),
            Email = cliente.GetEmail(),
            Endereco = cliente.GetEndereco(),
            ClienteDesde = cliente.DataCriacao,
            TotalOrdensServico = ordens.Count,
            OrdensServicoAbertas = ordens.Count(o => OsAbertas.Contains(o.Status.ToString())),
            TotalPropostas = propostas.Count,
            ComprasConcluidas = propostas.Count(p => p.Status == StatusPropostaVenda.Concluida),
            TotalGastoOficina = ordens.Where(o => o.Status is StatusOrdemServico.Entregue or StatusOrdemServico.Finalizada)
                                      .Sum(o => o.GetValorTotal()),
            TotalGastoConcessionaria = propostas.Where(p => p.Status == StatusPropostaVenda.Concluida)
                                                .Sum(p => p.GetValorFinal()),
            UltimoAtendimento = ultimo,
            UltimoAtendimentoSetor = setor,
            Veiculos = veiculos.Select(v => new FichaVeiculoDTO
            {
                Id = v.Id,
                Descricao = $"{v.GetMarca()} {v.GetModelo()} {v.GetAno()}",
                Placa = v.Placa.GetPlaca(),
                Atendimentos = osPorVeiculo.GetValueOrDefault(v.Id, 0)
            }).ToList(),
            HistoricoOficina = histOficina,
            HistoricoConcessionaria = histConc
        };

        return Result<ClienteFichaDTO>.Ok(ficha);
    }
}
