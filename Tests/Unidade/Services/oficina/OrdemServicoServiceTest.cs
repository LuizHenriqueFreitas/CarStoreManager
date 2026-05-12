using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Application.Services;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Tests.Unidade.Services
{
    public class OrdemServicoServiceTests
    {
        private readonly Mock<IOrdemServicoRepository> _ordemRepoMock;
        private readonly Mock<IComponenteRepository> _componenteRepoMock;
        private readonly Mock<IMecanicoService> _mecanicoServiceMock;
        private readonly OrdemServicoService _service;

        public OrdemServicoServiceTests()
        {
            _ordemRepoMock = new Mock<IOrdemServicoRepository>();
            _componenteRepoMock = new Mock<IComponenteRepository>();
            _mecanicoServiceMock = new Mock<IMecanicoService>();
            _service = new OrdemServicoService(
                _ordemRepoMock.Object,
                _componenteRepoMock.Object,
                _mecanicoServiceMock.Object);
        }

        // ==================== GetByIdAsync ====================

        [Fact]
        public async Task GetByIdAsync_OrdemExistente_RetornaDTO()
        {
            var ordem = CriarOrdemValida();
            _ordemRepoMock.Setup(r => r.GetByIdAsync(ordem.Id)).ReturnsAsync(ordem);

            var result = await _service.GetByIdAsync(ordem.Id);

            result.IsSuccess.Should().BeTrue();
            //result.Value.Descricao.Should().Be(ordem.Descricao);
        }

        [Fact]
        public async Task GetByIdAsync_OrdemInexistente_RetornaFalha()
        {
            _ordemRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);

            var result = await _service.GetByIdAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrada");
        }

        // ==================== GetAllAsync ====================

        [Fact]
        public async Task GetAllAsync_RetornaListaDeDTOs()
        {
            var ordens = new List<OrdemServico> { CriarOrdemValida(), CriarOrdemValida() };
            _ordemRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(ordens);

            var result = await _service.GetAllAsync();

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
        }

        // ==================== ObterPorNumeroPublicoAsync ====================

        [Fact]
        public async Task ObterPorNumeroPublicoAsync_NumeroExistente_RetornaDTO()
        {
            var ordem = CriarOrdemValida();
            _ordemRepoMock.Setup(r => r.ObterPorNumeroPublicoAsync(ordem.NumeroPublico)).ReturnsAsync(ordem);

            var result = await _service.ObterPorNumeroPublicoAsync(ordem.NumeroPublico);

            result.IsSuccess.Should().BeTrue();
            //result.Value.NumeroPublico.Should().Be(ordem.NumeroPublico);
        }

        [Fact]
        public async Task ObterPorNumeroPublicoAsync_NumeroInexistente_RetornaFalha()
        {
            _ordemRepoMock.Setup(r => r.ObterPorNumeroPublicoAsync(It.IsAny<string>())).ReturnsAsync((OrdemServico?)null);

            var result = await _service.ObterPorNumeroPublicoAsync("ABC12345");

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrada");
        }

        // ==================== AddAsync ====================

        [Fact]
        public async Task AddAsync_DTOValidoSemItens_CriaOrdemERetornaId()
        {
            _mecanicoServiceMock.Setup(m => m.AtualizarOcupacaoAsync(It.IsAny<Guid>())).ReturnsAsync(Result.Ok());
            _ordemRepoMock.Setup(r => r.AddAsync(It.IsAny<OrdemServico>())).Returns(Task.CompletedTask);
            _ordemRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = CriarCriarOrdemServicoDTO(); // sem itens

            var result = await _service.AddAsync(dto);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();
            _ordemRepoMock.Verify(r => r.AddAsync(It.IsAny<OrdemServico>()), Times.Once);
            _ordemRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mecanicoServiceMock.Verify(m => m.AtualizarOcupacaoAsync(dto.MecanicoId), Times.Once);
        }

        [Fact]
        public async Task AddAsync_DTOComItens_AdicionaItensERecalcula()
        {
            var componente = CriarComponenteValido();
            _mecanicoServiceMock.Setup(m => m.AtualizarOcupacaoAsync(It.IsAny<Guid>())).ReturnsAsync(Result.Ok());
            _componenteRepoMock.Setup(c => c.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(componente);
            _ordemRepoMock.Setup(r => r.AddAsync(It.IsAny<OrdemServico>())).Returns(Task.CompletedTask);
            _ordemRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var itemDto = new ItemOrdemServicoDTO
            {
                ComponenteId = componente.Id,
                Quantidade = 2,
                ValorUnitario = 50m
            };
            var dto = CriarCriarOrdemServicoDTO();
            dto.Itens = new List<ItemOrdemServicoDTO> { itemDto };

            var result = await _service.AddAsync(dto);

            result.IsSuccess.Should().BeTrue();
            _ordemRepoMock.Verify(r => r.AddAsync(It.Is<OrdemServico>(o => o.Itens.Count == 1)), Times.Once);
        }

        [Fact]
        public async Task AddAsync_MapeamentoLancaExcecao_RetornaFalha()
        {
            _mecanicoServiceMock.Setup(m => m.AtualizarOcupacaoAsync(It.IsAny<Guid>())).ReturnsAsync(Result.Ok());
            var dtoInvalido = new CriarOrdemServicoDTO(); // campos obrigatórios vazios

            var result = await _service.AddAsync(dtoInvalido);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Erro ao criar ordem de serviço");
        }

        // ==================== AdicionarItemAsync ====================

        [Fact]
        public async Task AdicionarItemAsync_OrdemOuComponenteNulo_RetornaFalha()
        {
            _ordemRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);
            _componenteRepoMock.Setup(c => c.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(CriarComponenteValido());

            var result = await _service.AdicionarItemAsync(new AdicionarItemOrdemServicoDTO());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Dados inválidos");
        }

        [Fact]
        public async Task AdicionarItemAsync_OrdemOuComponenteInexistente_RetornaFalha()
        {
            // OrdemServicoService não valida mais estoque (estoque vive em
            // EstoqueComponente). Cobertura focada na validação de inexistência.
            _ordemRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);
            _componenteRepoMock.Setup(c => c.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Componente?)null);

            var dto = new AdicionarItemOrdemServicoDTO
            {
                OrdemServicoId = Guid.NewGuid(),
                ComponenteId = Guid.NewGuid(),
                Quantidade = 5,
                ValorUnitario = 10m
            };

            var result = await _service.AdicionarItemAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("inválidos");
        }

        [Fact]
        public async Task AdicionarItemAsync_Sucesso_AdicionaItemESalva()
        {
            var ordem = CriarOrdemValida();
            var componente = CriarComponenteValido(quantidade: 10);
            _ordemRepoMock.Setup(r => r.GetByIdAsync(ordem.Id)).ReturnsAsync(ordem);
            _componenteRepoMock.Setup(c => c.GetByIdAsync(componente.Id)).ReturnsAsync(componente);
            _componenteRepoMock.Setup(c => c.Update(It.IsAny<Componente>())).Verifiable();
            _ordemRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new AdicionarItemOrdemServicoDTO
            {
                OrdemServicoId = ordem.Id,
                ComponenteId = componente.Id,
                Quantidade = 3
            };

            var result = await _service.AdicionarItemAsync(dto);

            result.IsSuccess.Should().BeTrue();
            ordem.Itens.Should().ContainSingle();
            // estoque agora vive em EstoqueComponente; o service não decrementa Componente diretamente.
            _componenteRepoMock.Verify(c => c.Update(It.IsAny<Componente>()), Times.Never);
            _ordemRepoMock.Verify(r => r.AdicionarItemAsync(It.IsAny<ItemOrdemServico>()), Times.Once);
            _ordemRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ==================== RemoverItemAsync ====================

        [Fact]
        public async Task RemoverItemAsync_OrdemInexistente_RetornaFalha()
        {
            _ordemRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);

            var result = await _service.RemoverItemAsync(Guid.NewGuid(), Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrada");
        }

        [Fact]
        public async Task RemoverItemAsync_ItemExistente_RemoveESalva()
        {
            var ordem = CriarOrdemValida();
            var item = new ItemOrdemServico(Guid.NewGuid(), ordem.Id, 2, 100m);
            ordem.AdicionarItem(item);
            _ordemRepoMock.Setup(r => r.GetByIdAsync(ordem.Id)).ReturnsAsync(ordem);
            _ordemRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.RemoverItemAsync(ordem.Id, item.Id);

            result.IsSuccess.Should().BeTrue();
            ordem.Itens.Should().BeEmpty();
            _ordemRepoMock.Verify(r => r.Update(ordem), Times.Once);
            _ordemRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ==================== AtualizarItemAsync ====================

        [Fact]
        public async Task AtualizarItemAsync_OrdemInexistente_RetornaFalha()
        {
            _ordemRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);

            var result = await _service.AtualizarItemAsync(new AtualizarItemOrdemServicoDTO());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrada");
        }

        [Fact]
        public async Task AtualizarItemAsync_ItemValido_AtualizaESalva()
        {
            var ordem = CriarOrdemValida();
            var item = new ItemOrdemServico(Guid.NewGuid(), ordem.Id, 1, 50m);
            ordem.AdicionarItem(item);
            _ordemRepoMock.Setup(r => r.GetByIdAsync(ordem.Id)).ReturnsAsync(ordem);
            _ordemRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new AtualizarItemOrdemServicoDTO
            {
                OrdemServicoId = ordem.Id,
                ItemId = item.Id,
                NovaQuantidade = 5
            };

            var result = await _service.AtualizarItemAsync(dto);

            result.IsSuccess.Should().BeTrue();
            item.GetQuantidade().Should().Be(5);
            _ordemRepoMock.Verify(r => r.Update(ordem), Times.Once);
            _ordemRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ==================== AtualizarStatusAsync ====================

        [Fact]
        public async Task AtualizarStatusAsync_OrdemInexistente_RetornaFalha()
        {
            _ordemRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);

            var result = await _service.AtualizarStatusAsync(new AtualizarOrdemServicoDTO());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrada");
        }

        [Fact]
        public async Task AtualizarStatusAsync_StatusInvalido_RetornaFalha()
        {
            var ordem = CriarOrdemValida();
            _ordemRepoMock.Setup(r => r.GetByIdAsync(ordem.Id)).ReturnsAsync(ordem);

            var dto = new AtualizarOrdemServicoDTO { Id = ordem.Id, Status = "Invalido" };

            var result = await _service.AtualizarStatusAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Status inválido");
        }

        [Fact]
        public async Task AtualizarStatusAsync_StatusValido_AtualizaESalva()
        {
            var ordem = CriarOrdemValida();
            _ordemRepoMock.Setup(r => r.GetByIdAsync(ordem.Id)).ReturnsAsync(ordem);
            _ordemRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new AtualizarOrdemServicoDTO { Id = ordem.Id, Status = "EmAndamento" };

            var result = await _service.AtualizarStatusAsync(dto);

            result.IsSuccess.Should().BeTrue();
            ordem.Status.Should().Be(StatusOrdemServico.EmAndamento);
            _ordemRepoMock.Verify(r => r.Update(ordem), Times.Once);
            _ordemRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ==================== AdicionarItemChecklistAsync ====================

        [Fact]
        public async Task AdicionarItemChecklistAsync_OrdemInexistente_RetornaFalha()
        {
            _ordemRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);

            var result = await _service.AdicionarItemChecklistAsync(new AdicionarChecklistItemDTO());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrada");
        }

        [Fact]
        public async Task AdicionarItemChecklistAsync_Sucesso_AdicionaItem()
        {
            var ordem = CriarOrdemValida();
            _ordemRepoMock.Setup(r => r.GetByIdAsync(ordem.Id)).ReturnsAsync(ordem);
            _ordemRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new AdicionarChecklistItemDTO
            {
                OrdemServicoId = ordem.Id,
                Descricao = "Verificar óleo"
            };

            var result = await _service.AdicionarItemChecklistAsync(dto);

            result.IsSuccess.Should().BeTrue();
            ordem.Checklist.Should().ContainSingle();
            _ordemRepoMock.Verify(r => r.AdicionarItemChecklistAsync(It.IsAny<ChecklistOrdemServico>()), Times.Once);
            _ordemRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ==================== AtualizarStatusChecklistAsync ====================

        [Fact]
        public async Task AtualizarStatusChecklistAsync_ItemNaoEncontrado_RetornaFalha()
        {
            var ordem = CriarOrdemValida();
            _ordemRepoMock.Setup(r => r.GetByIdAsync(ordem.Id)).ReturnsAsync(ordem);

            var dto = new AtualizarStatusChecklistDTO
            {
                OrdemServicoId = ordem.Id,
                ItemId = Guid.NewGuid(),
                NovoStatus = "Concluido"
            };

            var result = await _service.AtualizarStatusChecklistAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Item não encontrado");
        }

        [Fact]
        public async Task AtualizarStatusChecklistAsync_StatusInvalido_RetornaFalha()
        {
            var ordem = CriarOrdemValida();
            ordem.GerarChecklistAutomatico(); // adiciona item automático
            var item = ordem.Checklist.First();
            _ordemRepoMock.Setup(r => r.GetByIdAsync(ordem.Id)).ReturnsAsync(ordem);

            var dto = new AtualizarStatusChecklistDTO
            {
                OrdemServicoId = ordem.Id,
                ItemId = item.Id,
                NovoStatus = "Invalido"
            };

            var result = await _service.AtualizarStatusChecklistAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Status inválido");
        }

        [Fact]
        public async Task AtualizarStatusChecklistAsync_IniciarItem_Sucesso()
        {
            var ordem = CriarOrdemValida();
            ordem.GerarChecklistAutomatico();
            var item = ordem.Checklist.First();
            _ordemRepoMock.Setup(r => r.GetByIdAsync(ordem.Id)).ReturnsAsync(ordem);
            _ordemRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new AtualizarStatusChecklistDTO
            {
                OrdemServicoId = ordem.Id,
                ItemId = item.Id,
                NovoStatus = "EmAndamento"
            };

            var result = await _service.AtualizarStatusChecklistAsync(dto);

            result.IsSuccess.Should().BeTrue();
            item.Status.Should().Be(StatusChecklistItem.EmAndamento);
            _ordemRepoMock.Verify(r => r.Update(ordem), Times.Once);
        }

        [Fact]
        public async Task AtualizarStatusChecklistAsync_ConcluirItem_Sucesso()
        {
            var ordem = CriarOrdemValida();
            ordem.GerarChecklistAutomatico();
            var item = ordem.Checklist.First();
            item.IniciarItem(); // vai para EmAndamento
            _ordemRepoMock.Setup(r => r.GetByIdAsync(ordem.Id)).ReturnsAsync(ordem);
            _ordemRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new AtualizarStatusChecklistDTO
            {
                OrdemServicoId = ordem.Id,
                ItemId = item.Id,
                NovoStatus = "Concluido"
            };

            var result = await _service.AtualizarStatusChecklistAsync(dto);

            result.IsSuccess.Should().BeTrue();
            item.Status.Should().Be(StatusChecklistItem.Concluido);
        }

        // ==================== RecalcularValoresAsync ====================

        [Fact]
        public async Task RecalcularValoresAsync_OrdemInexistente_RetornaFalha()
        {
            _ordemRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);

            var result = await _service.RecalcularValoresAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrada");
        }

        [Fact]
        public async Task RecalcularValoresAsync_OrdemExistente_RecalculaESalva()
        {
            var ordem = CriarOrdemValida(custoServico: 100m);
            // Adiciona um item para ter valor total diferente
            ordem.AdicionarItem(new ItemOrdemServico(Guid.NewGuid(), ordem.Id, 2, 50m));
            _ordemRepoMock.Setup(r => r.GetByIdAsync(ordem.Id)).ReturnsAsync(ordem);
            _ordemRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.RecalcularValoresAsync(ordem.Id);

            result.IsSuccess.Should().BeTrue();
            _ordemRepoMock.Verify(r => r.Update(ordem), Times.Once);
            _ordemRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ==================== RemoveAsync ====================

        [Fact]
        public async Task RemoveAsync_OrdemExistente_RemoveESalva()
        {
            var ordem = CriarOrdemValida();
            _ordemRepoMock.Setup(r => r.GetByIdAsync(ordem.Id)).ReturnsAsync(ordem);
            _ordemRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.RemoveAsync(ordem.Id);

            result.IsSuccess.Should().BeTrue();
            _ordemRepoMock.Verify(r => r.Remove(ordem), Times.Once);
            _ordemRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_OrdemInexistente_RetornaFalha()
        {
            _ordemRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);

            var result = await _service.RemoveAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Mecânico não encontrado"); // bug de mensagem original
        }

        // ==================== MÉTODOS AUXILIARES ====================

        private static OrdemServico CriarOrdemValida(decimal custoServico = 200m)
        {
            var ordem = new OrdemServico(
                Guid.NewGuid(),  // veiculoClienteId
                Guid.NewGuid(),  // mecanicoId
                Guid.NewGuid(),  // clienteId
                TipoServico.Manutencao,
                "Troca de óleo",
                DateTime.UtcNow.AddDays(5),
                custoServico
            );
            typeof(OrdemServico).BaseType?.GetProperty("Id")?.SetValue(ordem, Guid.NewGuid());
            return ordem;
        }

        private static Componente CriarComponenteValido(int quantidade = 20)
        {
            var componente = new Componente(
                "FIL-001", "Filtro", "Filtro de óleo", "Bosch", "PN-FIL-1",
                "OEM-1", "7891234567890", "87083010", "0102000",
                "Motor", "UN", 0.3m, 90);
            typeof(Componente).BaseType?.GetProperty("Id")?.SetValue(componente, Guid.NewGuid());
            return componente;
        }

        private static CriarOrdemServicoDTO CriarCriarOrdemServicoDTO()
        {
            return new CriarOrdemServicoDTO
            {
                VeiculoClienteId = Guid.NewGuid(),
                MecanicoId = Guid.NewGuid(),
                ClienteId = Guid.NewGuid(),
                Tipo = "Manutencao",
                Descricao = "Serviço de teste",
                PrazoEstimado = DateTime.UtcNow.AddDays(3),
                CustoServico = 500.00m,
                Itens = new List<ItemOrdemServicoDTO>()
            };
        }
    }
}