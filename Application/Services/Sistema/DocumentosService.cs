using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema.Documentos;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Interfaces.Repositories.Concessionaria;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services.Sistema;

/// <summary>
/// Central de Documentos — reúne os 4 documentos já produzidos pelo sistema
/// (termo de entrega, termo de test drive, contrato de consignação, contrato
/// de OS) numa listagem só, enriquecida com cliente/responsável/placa pra
/// busca. Mesmo padrão best-effort de
/// PropostaVendaService.EnriquecerListaAsync: falha em lookup individual não
/// derruba a listagem inteira, só deixa o campo vazio.
/// </summary>
public class DocumentosService : IDocumentosService
{
    private readonly ITermoEntregaRepository _termoEntregaRepository;
    private readonly IPropostaVendaRepository _propostaRepository;
    private readonly IVistoriaRepository _vistoriaRepository;
    private readonly IVeiculoVendaRepository _veiculoVendaRepository;
    private readonly IVeiculoConsignacaoRepository _consignacaoRepository;

    private readonly ITermoTestDriveRepository _termoTestDriveRepository;
    private readonly ITestDriveRepository _testDriveRepository;

    private readonly IVistoriaOrdemServicoRepository _vistoriaOSRepository;
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly IVeiculoClienteRepository _veiculoClienteRepository;

    private readonly IClienteRepository _clienteRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public DocumentosService(
        ITermoEntregaRepository termoEntregaRepository,
        IPropostaVendaRepository propostaRepository,
        IVistoriaRepository vistoriaRepository,
        IVeiculoVendaRepository veiculoVendaRepository,
        IVeiculoConsignacaoRepository consignacaoRepository,
        ITermoTestDriveRepository termoTestDriveRepository,
        ITestDriveRepository testDriveRepository,
        IVistoriaOrdemServicoRepository vistoriaOSRepository,
        IOrdemServicoRepository ordemServicoRepository,
        IVeiculoClienteRepository veiculoClienteRepository,
        IClienteRepository clienteRepository,
        IUsuarioRepository usuarioRepository)
    {
        _termoEntregaRepository = termoEntregaRepository;
        _propostaRepository = propostaRepository;
        _vistoriaRepository = vistoriaRepository;
        _veiculoVendaRepository = veiculoVendaRepository;
        _consignacaoRepository = consignacaoRepository;
        _termoTestDriveRepository = termoTestDriveRepository;
        _testDriveRepository = testDriveRepository;
        _vistoriaOSRepository = vistoriaOSRepository;
        _ordemServicoRepository = ordemServicoRepository;
        _veiculoClienteRepository = veiculoClienteRepository;
        _clienteRepository = clienteRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Result<List<DocumentoDTO>>> ListarTermosEntregaAsync()
    {
        var termos = await _termoEntregaRepository.GetAllAsync();
        var lista = new List<DocumentoDTO>();

        foreach (var termo in termos)
        {
            var dto = new DocumentoDTO
            {
                Id = termo.Id,
                TipoDocumento = "Termo de entrega",
                ResponsavelPapel = "Vendedor",
                Status = termo.Status.ToString(),
                Data = termo.DataUltimaEdicao ?? termo.DataRedacao,
                TextoDocumento = termo.TextoTermo,
            };

            try
            {
                var proposta = await _propostaRepository.GetByIdAsync(termo.PropostaVendaId);
                if (proposta is not null)
                {
                    var cliente = await _clienteRepository.GetByIdAsync(proposta.GetClienteId());
                    if (cliente is not null) dto.ClienteNome = cliente.GetNome();

                    var vendedor = await _usuarioRepository.GetByIdAsync(proposta.GetVendedorId());
                    if (vendedor is not null) dto.ResponsavelNome = vendedor.GetNome();

                    if (proposta.IsConsignado)
                    {
                        var consig = await _consignacaoRepository.GetByIdAsync(proposta.GetVeiculoId());
                        if (consig is not null) dto.Placa = consig.GetPlacaCarro();
                    }
                    else
                    {
                        var veic = await _veiculoVendaRepository.GetByIdAsync(proposta.GetVeiculoId());
                        if (veic is not null) dto.Placa = veic.GetPlacaCarro();
                    }

                    var vistorias = await _vistoriaRepository.ObterPorPropostaAsync(termo.PropostaVendaId);
                    var vistoria = vistorias.Where(v => v.Concluida).OrderByDescending(v => v.DataConclusao).FirstOrDefault();
                    if (vistoria is not null)
                    {
                        dto.EntidadeTipoFotos = "Vistoria";
                        dto.EntidadeIdFotos = vistoria.Id;
                    }
                }
            }
            catch { /* best-effort */ }

            lista.Add(dto);
        }

        return Result<List<DocumentoDTO>>.Ok(lista);
    }

    public async Task<Result<List<DocumentoDTO>>> ListarTermosTestDriveAsync()
    {
        var termos = await _termoTestDriveRepository.GetAllAsync();
        var lista = new List<DocumentoDTO>();

        foreach (var termo in termos)
        {
            var dto = new DocumentoDTO
            {
                Id = termo.Id,
                TipoDocumento = "Termo de test drive",
                ResponsavelPapel = "Vendedor",
                Status = termo.Status.ToString(),
                Data = termo.DataUltimaEdicao ?? termo.DataRedacao,
                TextoDocumento = termo.TextoTermo,
            };

            try
            {
                var testDrive = await _testDriveRepository.GetByIdAsync(termo.TestDriveId);
                if (testDrive is not null)
                {
                    var cliente = await _clienteRepository.GetByIdAsync(testDrive.ClienteId);
                    if (cliente is not null) dto.ClienteNome = cliente.GetNome();

                    var vendedor = await _usuarioRepository.GetByIdAsync(testDrive.VendedorId);
                    if (vendedor is not null) dto.ResponsavelNome = vendedor.GetNome();

                    if (testDrive.IsConsignado)
                    {
                        var consig = await _consignacaoRepository.GetByIdAsync(testDrive.VeiculoVendaId);
                        if (consig is not null) dto.Placa = consig.GetPlacaCarro();
                    }
                    else
                    {
                        var veic = await _veiculoVendaRepository.GetByIdAsync(testDrive.VeiculoVendaId);
                        if (veic is not null) dto.Placa = veic.GetPlacaCarro();
                    }
                }
            }
            catch { /* best-effort */ }

            lista.Add(dto);
        }

        return Result<List<DocumentoDTO>>.Ok(lista);
    }

    public async Task<Result<List<DocumentoDTO>>> ListarContratosConsignacaoAsync()
    {
        var veiculos = await _consignacaoRepository.GetAllAsync();
        var lista = new List<DocumentoDTO>();

        foreach (var v in veiculos.Where(v => !string.IsNullOrWhiteSpace(v.TextoContrato)))
        {
            var dto = new DocumentoDTO
            {
                Id = v.Id,
                TipoDocumento = "Contrato de consignação",
                ResponsavelPapel = "Vendedor",
                Status = v.Status.ToString(),
                Data = v.DataInicio,
                TextoDocumento = v.TextoContrato,
                Placa = v.GetPlacaCarro(),
                EntidadeTipoFotos = "VeiculoConsignacao",
                EntidadeIdFotos = v.Id,
            };

            try
            {
                var cliente = await _clienteRepository.GetByIdAsync(v.ClienteProprietarioId);
                if (cliente is not null) dto.ClienteNome = cliente.GetNome();

                var vendedor = await _usuarioRepository.GetByIdAsync(v.VendedorResponsavelId);
                if (vendedor is not null) dto.ResponsavelNome = vendedor.GetNome();
            }
            catch { /* best-effort */ }

            lista.Add(dto);
        }

        return Result<List<DocumentoDTO>>.Ok(lista);
    }

    public async Task<Result<List<DocumentoDTO>>> ListarContratosOSAsync()
    {
        var vistorias = await _vistoriaOSRepository.GetAllAsync();
        var lista = new List<DocumentoDTO>();

        foreach (var vistoria in vistorias)
        {
            var dto = new DocumentoDTO
            {
                Id = vistoria.Id,
                TipoDocumento = "Contrato de OS",
                ResponsavelPapel = "Recepcionista",
                Status = vistoria.Concluida ? "Concluída" : "Em andamento",
                Data = vistoria.DataConclusao ?? vistoria.DataInicio,
                TextoDocumento = vistoria.TextoContrato,
                EntidadeTipoFotos = "VistoriaOrdemServico",
                EntidadeIdFotos = vistoria.Id,
            };

            try
            {
                var recepcionista = await _usuarioRepository.GetByIdAsync(vistoria.RecepcionistaId);
                if (recepcionista is not null) dto.ResponsavelNome = recepcionista.GetNome();

                var ordem = await _ordemServicoRepository.GetByIdAsync(vistoria.OrdemServicoId);
                if (ordem is not null)
                {
                    var cliente = await _clienteRepository.GetByIdAsync(ordem.GetClienteId());
                    if (cliente is not null) dto.ClienteNome = cliente.GetNome();

                    var veiculo = await _veiculoClienteRepository.GetByIdAsync(ordem.GetVeiculoClienteId());
                    if (veiculo is not null) dto.Placa = veiculo.Placa.GetPlaca();
                }
            }
            catch { /* best-effort */ }

            lista.Add(dto);
        }

        return Result<List<DocumentoDTO>>.Ok(lista);
    }
}
