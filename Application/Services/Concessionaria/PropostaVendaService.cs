using System.Globalization;
using System.Text;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Application.Mappings.Concessionaria;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Concessionaria;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

public class PropostaVendaService : IPropostaVendaService
{
    private readonly IPropostaVendaRepository _repository;
    private readonly IVeiculoVendaRepository _veiculoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IConfiguracaoSistemaRepository _configRepository;
    private readonly IEmailService _emailService;
    private readonly IVistoriaRepository _vistoriaRepository;
    private readonly ITermoEntregaRepository _termoRepository;
    private readonly IPagamentoPropostaRepository? _pagamentoRepository;

    public PropostaVendaService(
        IPropostaVendaRepository repository,
        IVeiculoVendaRepository veiculoRepository,
        IClienteRepository clienteRepository,
        IConfiguracaoSistemaRepository configRepository,
        IEmailService emailService,
        IVistoriaRepository vistoriaRepository,
        ITermoEntregaRepository termoRepository,
        IPagamentoPropostaRepository? pagamentoRepository = null)
    {
        _repository = repository;
        _veiculoRepository = veiculoRepository;
        _clienteRepository = clienteRepository;
        _configRepository = configRepository;
        _emailService = emailService;
        _vistoriaRepository = vistoriaRepository;
        _termoRepository = termoRepository;
        _pagamentoRepository = pagamentoRepository;
    }

    public async Task<Result<PropostaVendaDTO>> GetByIdAsync(Guid id)
    {
        var proposta = await _repository.GetByIdAsync(id);
        if (proposta is null) return Result<PropostaVendaDTO>.Fail("Proposta não encontrada");

        await ExpirarSeNecessarioAsync(proposta);
        return Result<PropostaVendaDTO>.Ok(PropostaVendaMapping.ToDto(proposta));
    }

    public async Task<Result<IEnumerable<PropostaVendaListaDTO>>> GetAllAsync()
    {
        var propostas = (await _repository.GetAllAsync()).ToList();
        await ExpirarLoteSeNecessarioAsync(propostas);
        var dtos = await EnriquecerListaAsync(propostas);
        return Result<IEnumerable<PropostaVendaListaDTO>>.Ok(dtos);
    }

    public async Task<Result<IEnumerable<PropostaVendaListaDTO>>> ObterPorVendedorAsync(Guid vendedorId)
    {
        var propostas = (await _repository.ObterPorVendedorAsync(vendedorId)).ToList();
        await ExpirarLoteSeNecessarioAsync(propostas);
        var dtos = await EnriquecerListaAsync(propostas);
        return Result<IEnumerable<PropostaVendaListaDTO>>.Ok(dtos);
    }

    public async Task<Result<IEnumerable<PropostaVendaListaDTO>>> ObterPorClienteAsync(Guid clienteId)
    {
        var propostas = (await _repository.ObterPorClienteAsync(clienteId)).ToList();
        await ExpirarLoteSeNecessarioAsync(propostas);
        var dtos = await EnriquecerListaAsync(propostas);
        return Result<IEnumerable<PropostaVendaListaDTO>>.Ok(dtos);
    }

    /// <summary>
    /// Para cada proposta: anexa nome do cliente e marca/modelo do veículo.
    /// Best-effort: falhas em lookups individuais ficam com strings vazias.
    /// </summary>
    private async Task<List<PropostaVendaListaDTO>> EnriquecerListaAsync(IEnumerable<PropostaVenda> propostas)
    {
        var dtos = new List<PropostaVendaListaDTO>();
        foreach (var p in propostas)
        {
            var dto = PropostaVendaMapping.ToListaDto(p);
            try
            {
                var cli = await _clienteRepository.GetByIdAsync(p.GetClienteId());
                if (cli is not null) dto.ClienteNome = cli.GetNome();

                var veic = await _veiculoRepository.GetByIdAsync(p.GetVeiculoId());
                if (veic is not null)
                {
                    dto.VeiculoMarca = veic.GetMarca();
                    dto.VeiculoModelo = veic.GetModelo();
                }
            }
            catch { /* best-effort */ }
            dtos.Add(dto);
        }
        return dtos;
    }

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

    public async Task<Result> AplicarDescontoAsync(AplicarDescontoDTO dto)
    {
        var proposta = await _repository.GetByIdAsync(dto.PropostaId);
        if (proposta is null) return Result.Fail("Proposta não encontrada");
        if (await ExpirarSeNecessarioAsync(proposta))
            return Result.Fail("Proposta expirou.");

        try
        {
            proposta.AplicarDesconto(PropostaVendaMapping.ToDesconto(dto));
            _repository.Update(proposta);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> DefinirEntradaAsync(DefinirEntradaDTO dto)
    {
        var proposta = await _repository.GetByIdAsync(dto.PropostaId);
        if (proposta is null) return Result.Fail("Proposta não encontrada");
        if (await ExpirarSeNecessarioAsync(proposta))
            return Result.Fail("Proposta expirou.");

        try
        {
            proposta.AtualizarEntrada(dto.ValorEntrada);
            _repository.Update(proposta);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> DefinirModoPagamentoAsync(Guid propostaId, string modoPagamento)
    {
        var proposta = await _repository.GetByIdAsync(propostaId);
        if (proposta is null) return Result.Fail("Proposta não encontrada");
        if (await ExpirarSeNecessarioAsync(proposta))
            return Result.Fail("Proposta expirou.");

        try
        {
            var modo = PropostaVendaMapping.ParseModoPagamento(modoPagamento);
            proposta.DefinirModoPagamento(modo);
            _repository.Update(proposta);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> SolicitarFinanciamentoAsync(Guid propostaId)
    {
        var proposta = await _repository.GetByIdAsync(propostaId);
        if (proposta is null) return Result.Fail("Proposta não encontrada");
        if (await ExpirarSeNecessarioAsync(proposta))
            return Result.Fail("Proposta expirou.");

        var cfg = await _configRepository.ObterAsync();
        if (!cfg.FinanciadoraConfigurada())
            return Result.Fail("Financiadora não está configurada. Acesse Configurações do sistema.");
        if (!cfg.SmtpConfigurado())
            return Result.Fail("SMTP não está configurado. Acesse Configurações do sistema.");

        var veiculo = await _veiculoRepository.GetByIdAsync(proposta.VeiculoVendaId);
        var cliente = await _clienteRepository.GetByIdAsync(proposta.ClienteId);
        if (veiculo is null || cliente is null)
            return Result.Fail("Cliente ou veículo não encontrado.");

        try
        {
            // Persiste a transição PRIMEIRO — se o e-mail falhar, ainda marcamos
            // a tentativa de solicitação. Admin pode reenviar manualmente.
            proposta.SolicitarFinanciamento();
            _repository.Update(proposta);
            await _repository.SaveChangesAsync();

            var corpo = MontarEmailFinanciadora(proposta, veiculo, cliente, cfg.NomeFinanciadora);
            var assunto = $"[CarStore] Solicitação de financiamento — {cliente.GetNome()} — Proposta {proposta.Id.ToString()[..8]}";
            var envio = await _emailService.EnviarAsync(cfg.EmailFinanciadora, assunto, corpo, isHtml: true);

            if (!envio.IsSuccess)
                return Result.Fail($"Solicitação registrada, mas falha ao enviar e-mail: {envio.Error}");

            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> RegistrarRespostaFinanciadoraAsync(Guid propostaId, RegistrarRespostaFinanciadoraDTO dto)
    {
        var proposta = await _repository.GetByIdAsync(propostaId);
        if (proposta is null) return Result.Fail("Proposta não encontrada");
        if (await ExpirarSeNecessarioAsync(proposta))
            return Result.Fail("Proposta expirou.");

        try
        {
            proposta.RegistrarRespostaFinanciadora(
                dto.Parcelas, dto.ValorParcela, dto.TaxaJurosMensal, dto.Observacoes);
            _repository.Update(proposta);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> AprovarAsync(Guid propostaId)
    {
        var proposta = await _repository.GetByIdAsync(propostaId);
        if (proposta is null) return Result.Fail("Proposta não encontrada");
        if (await ExpirarSeNecessarioAsync(proposta))
            return Result.Fail("Proposta expirou.");

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

    public async Task<Result> RejeitarAsync(Guid propostaId, string motivo)
    {
        var proposta = await _repository.GetByIdAsync(propostaId);
        if (proposta is null) return Result.Fail("Proposta não encontrada");
        if (await ExpirarSeNecessarioAsync(proposta))
            return Result.Fail("Proposta expirou.");

        try
        {
            proposta.Rejeitar(motivo);
            _repository.Update(proposta);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> CancelarAsync(Guid propostaId, string motivo)
    {
        var proposta = await _repository.GetByIdAsync(propostaId);
        if (proposta is null) return Result.Fail("Proposta não encontrada");

        try
        {
            proposta.Cancelar(motivo);
            _repository.Update(proposta);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    // ============================================================
    // Vistoria
    // ============================================================

    public async Task<Result> IniciarVistoriaAsync(Guid propostaId)
    {
        var proposta = await _repository.GetByIdAsync(propostaId);
        if (proposta is null) return Result.Fail("Proposta não encontrada");

        try
        {
            proposta.IniciarVistoria();
            _repository.Update(proposta);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result<VistoriaDTO>> RegistrarVistoriaAsync(
        Guid propostaId, Guid vistoriadorId, RegistrarVistoriaDTO dto)
    {
        var proposta = await _repository.GetByIdAsync(propostaId);
        if (proposta is null) return Result<VistoriaDTO>.Fail("Proposta não encontrada");

        try
        {
            var vistoria = new Vistoria(propostaId, vistoriadorId, dto.Observacoes, dto.Aprovado);
            await _vistoriaRepository.AddAsync(vistoria);

            proposta.RegistrarResultadoVistoria(dto.Aprovado);
            _repository.Update(proposta);

            await _repository.SaveChangesAsync();
            return Result<VistoriaDTO>.Ok(MapVistoria(vistoria));
        }
        catch (Exception ex) { return Result<VistoriaDTO>.Fail(ex.Message); }
    }

    public async Task<Result<IEnumerable<VistoriaDTO>>> ListarVistoriasAsync(Guid propostaId)
    {
        var vistorias = await _vistoriaRepository.ObterPorPropostaAsync(propostaId);
        return Result<IEnumerable<VistoriaDTO>>.Ok(vistorias.Select(MapVistoria));
    }

    // ============================================================
    // Termo de entrega
    // ============================================================

    public async Task<Result<TermoEntregaDTO>> CriarOuEditarTermoAsync(
        Guid propostaId, Guid adminId, CriarOuEditarTermoDTO dto)
    {
        var proposta = await _repository.GetByIdAsync(propostaId);
        if (proposta is null) return Result<TermoEntregaDTO>.Fail("Proposta não encontrada");

        try
        {
            var termo = await _termoRepository.ObterPorPropostaAsync(propostaId);
            if (termo is null)
            {
                termo = new TermoEntrega(propostaId, adminId, dto.TextoTermo);
                await _termoRepository.AddAsync(termo);
            }
            else
            {
                termo.EditarTexto(dto.TextoTermo);
                _termoRepository.Update(termo);
            }
            await _termoRepository.SaveChangesAsync();
            return Result<TermoEntregaDTO>.Ok(MapTermo(termo));
        }
        catch (Exception ex) { return Result<TermoEntregaDTO>.Fail(ex.Message); }
    }

    public async Task<Result<TermoEntregaDTO>> ObterTermoAsync(Guid propostaId)
    {
        var termo = await _termoRepository.ObterPorPropostaAsync(propostaId);
        if (termo is null)
            return Result<TermoEntregaDTO>.Fail("Termo não criado para esta proposta.");

        var dto = MapTermo(termo);
        await EnriquecerComValoresAsync(dto);
        return Result<TermoEntregaDTO>.Ok(dto);
    }

    public async Task<Result> EnviarTermoParaAssinaturaAsync(Guid propostaId)
    {
        var proposta = await _repository.GetByIdAsync(propostaId);
        if (proposta is null) return Result.Fail("Proposta não encontrada");

        var termo = await _termoRepository.ObterPorPropostaAsync(propostaId);
        if (termo is null) return Result.Fail("Crie o termo antes de enviá-lo para assinatura.");

        // Bloqueia o envio se o veículo ainda não estiver totalmente pago.
        if (_pagamentoRepository is not null)
        {
            var pagamentos = await _pagamentoRepository.ObterPorPropostaAsync(propostaId);
            var pago = pagamentos.Sum(p => p.Valor.GetValorDinheiro());
            var total = proposta.GetValorFinal();
            if (pago < total)
            {
                var restante = total - pago;
                return Result.Fail(
                    $"Cobrança pendente: faltam R$ {restante:N2} de R$ {total:N2}. " +
                    "Registre o pagamento completo antes de enviar o termo para assinatura.");
            }
        }

        try
        {
            termo.EnviarParaAssinatura();
            proposta.EnviarTermoParaAssinatura();
            _termoRepository.Update(termo);
            _repository.Update(proposta);
            await _termoRepository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result<TermoEntregaDTO>> ObterTermoPorTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Result<TermoEntregaDTO>.Fail("Token inválido.");

        var termo = await _termoRepository.ObterPorTokenAsync(token);
        if (termo is null)
            return Result<TermoEntregaDTO>.Fail("Termo não encontrado ou link inválido.");

        var dto = MapTermo(termo);
        await EnriquecerComValoresAsync(dto);
        return Result<TermoEntregaDTO>.Ok(dto);
    }

    /// <summary>
    /// Anexa ValorVeiculo (proposta.ValorFinal) e ValorPago (soma dos pagamentos)
    /// ao DTO do termo — usado tanto pela tela do admin quanto pela página pública.
    /// </summary>
    private async Task EnriquecerComValoresAsync(TermoEntregaDTO dto)
    {
        try
        {
            var proposta = await _repository.GetByIdAsync(dto.PropostaVendaId);
            if (proposta is null) return;
            dto.ValorVeiculo = proposta.GetValorFinal();

            if (_pagamentoRepository is not null)
            {
                var pagamentos = await _pagamentoRepository.ObterPorPropostaAsync(dto.PropostaVendaId);
                dto.ValorPago = pagamentos.Sum(p => p.Valor.GetValorDinheiro());
            }
        }
        catch { /* best-effort */ }
    }

    public async Task<Result> AssinarTermoAsync(string token, AssinarTermoDTO dto, string ipOrigem)
    {
        if (!dto.Aceite)
            return Result.Fail("É necessário marcar o aceite explícito do termo.");

        var termo = await _termoRepository.ObterPorTokenAsync(token);
        if (termo is null) return Result.Fail("Termo não encontrado ou link inválido.");

        var proposta = await _repository.GetByIdAsync(termo.PropostaVendaId);
        if (proposta is null) return Result.Fail("Proposta vinculada não encontrada.");

        try
        {
            termo.Assinar(dto.NomeCliente, dto.CpfCliente, ipOrigem);
            proposta.Concluir();
            _termoRepository.Update(termo);
            _repository.Update(proposta);
            await _termoRepository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    private static VistoriaDTO MapVistoria(Vistoria v) => new()
    {
        Id = v.Id,
        PropostaVendaId = v.PropostaVendaId,
        VistoriadorId = v.VistoriadorId,
        DataRealizada = v.DataRealizada,
        Observacoes = v.Observacoes,
        Aprovado = v.Aprovado
    };

    private static TermoEntregaDTO MapTermo(TermoEntrega t) => new()
    {
        Id = t.Id,
        PropostaVendaId = t.PropostaVendaId,
        TextoTermo = t.TextoTermo,
        Status = t.Status.ToString(),
        DataRedacao = t.DataRedacao,
        DataUltimaEdicao = t.DataUltimaEdicao,
        TokenAssinatura = t.TokenAssinatura,
        DataAssinatura = t.DataAssinatura,
        AssinaturaNomeCliente = t.AssinaturaNomeCliente,
        AssinaturaCpfCliente = t.AssinaturaCpfCliente,
        AssinaturaIp = t.AssinaturaIp
    };

    public Task<Result> GerarFinanciamentoAsync(GerarFinanciamentoDTO dto)
        => Task.FromResult(Result.Fail(
            "Endpoint depreciado. Use DefinirModoPagamento + SolicitarFinanciamento + RegistrarRespostaFinanciadora."));

    public Task<Result> UpdateAsync(PropostaVendaDTO dto)
        => Task.FromResult(Result.Fail("Update direto não suportado — use as transições específicas."));

    public async Task<Result> RemoveAsync(Guid propostaVendaId)
    {
        var proposta = await _repository.GetByIdAsync(propostaVendaId);
        if (proposta is null) return Result.Fail("Proposta não encontrada");

        _repository.Remove(proposta);
        await _repository.SaveChangesAsync();
        return Result.Ok();
    }

    // ============================================================
    // helpers internos
    // ============================================================

    private async Task<bool> ExpirarSeNecessarioAsync(PropostaVenda proposta)
    {
        if (!proposta.TentarExpirar()) return false;
        _repository.Update(proposta);
        await _repository.SaveChangesAsync();
        return true;
    }

    private async Task ExpirarLoteSeNecessarioAsync(IEnumerable<PropostaVenda> propostas)
    {
        var alterada = false;
        foreach (var p in propostas)
            if (p.TentarExpirar())
            {
                _repository.Update(p);
                alterada = true;
            }
        if (alterada) await _repository.SaveChangesAsync();
    }

    private static string MontarEmailFinanciadora(
        PropostaVenda proposta,
        Domain.Entities.Concessionaria.VeiculoVenda veiculo,
        Domain.Entities.Cliente cliente,
        string nomeFinanciadora)
    {
        var ci = CultureInfo.GetCultureInfo("pt-BR");
        var sb = new StringBuilder();
        sb.AppendLine("<html><body style='font-family: Arial, sans-serif; color:#222;'>");
        sb.AppendLine($"<h2>Solicitação de simulação de financiamento</h2>");
        sb.AppendLine($"<p>Prezados {nomeFinanciadora},</p>");
        sb.AppendLine("<p>Solicitamos análise para o financiamento abaixo:</p>");

        sb.AppendLine("<h3>Cliente</h3><table cellpadding='4'>");
        sb.AppendLine($"<tr><td><b>Nome:</b></td><td>{cliente.GetNome()}</td></tr>");
        sb.AppendLine($"<tr><td><b>CPF:</b></td><td>{cliente.GetCpf()}</td></tr>");
        sb.AppendLine($"<tr><td><b>Telefone:</b></td><td>{cliente.GetTelefone()}</td></tr>");
        sb.AppendLine($"<tr><td><b>E-mail:</b></td><td>{cliente.GetEmail()}</td></tr>");
        sb.AppendLine("</table>");

        sb.AppendLine("<h3>Veículo</h3><table cellpadding='4'>");
        sb.AppendLine($"<tr><td><b>Modelo:</b></td><td>{veiculo.GetMarca()} {veiculo.GetModelo()}</td></tr>");
        sb.AppendLine($"<tr><td><b>Ano:</b></td><td>{veiculo.GetAno()}</td></tr>");
        sb.AppendLine($"<tr><td><b>Placa:</b></td><td>{veiculo.Placa}</td></tr>");
        sb.AppendLine($"<tr><td><b>Renavam:</b></td><td>{veiculo.Renavam}</td></tr>");
        sb.AppendLine("</table>");

        sb.AppendLine("<h3>Valores da operação</h3><table cellpadding='4'>");
        sb.AppendLine($"<tr><td><b>Valor do veículo:</b></td><td>{proposta.GetValorFinal().ToString("C", ci)}</td></tr>");
        sb.AppendLine($"<tr><td><b>Entrada:</b></td><td>{proposta.GetEntrada().ToString("C", ci)}</td></tr>");
        sb.AppendLine($"<tr><td><b>Valor a financiar:</b></td><td><b>{proposta.ValorLiquidoFinanciamento().ToString("C", ci)}</b></td></tr>");
        sb.AppendLine("</table>");

        sb.AppendLine($"<p>Proposta interna: <code>{proposta.Id}</code></p>");
        sb.AppendLine("<p>Aguardamos retorno com simulação de parcelas, taxa e demais condições.</p>");
        sb.AppendLine("<p>Atenciosamente,<br/>Equipe CarStore</p>");
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }
}
