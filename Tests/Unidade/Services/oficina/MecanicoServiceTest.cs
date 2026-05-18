using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.Mecanico;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Application.Services;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Tests.Unidade.Services
{
    public class MecanicoServiceTests
    {
        private readonly Mock<IMecanicoRepository> _repoMock;
        private readonly MecanicoService _service;

        public MecanicoServiceTests()
        {
            _repoMock = new Mock<IMecanicoRepository>();
            _service = new MecanicoService(_repoMock.Object);
        }

        // ==================== GetByIdAsync ====================

        [Fact]
        public async Task GetByIdAsync_MecanicoExistente_RetornaDTO()
        {
            var mecanico = CriarMecanicoValido();
            _repoMock.Setup(r => r.GetByIdAsync(mecanico.Id)).ReturnsAsync(mecanico);

            var result = await _service.GetByIdAsync(mecanico.Id);

            result.IsSuccess.Should().BeTrue();
            //result.Value.Nome.Should().Be(mecanico.Nome);
        }

        [Fact]
        public async Task GetByIdAsync_MecanicoInexistente_RetornaFalha()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Mecanico?)null);

            var result = await _service.GetByIdAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== GetAllAsync ====================

        [Fact]
        public async Task GetAllAsync_RetornaListaDeDTOs()
        {
            var mecanicos = new List<Mecanico> { CriarMecanicoValido(), CriarMecanicoValido() };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(mecanicos);

            var result = await _service.GetAllAsync();

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
        }

        // ==================== AddAsync ====================

        [Fact]
        public async Task AddAsync_DTOValido_CriaMecanicoERetornaId()
        {
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Mecanico>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new CriarMecanicoDTO
            {
                Nome = "João Mecânico",
                Email = "joao@oficina.com",
                Telefone = "11988888888",
                Senha = "Senha@123",
                Especialidade = "Mecanica",
                Nivel = "Pleno",
                DataContratacao = DateTime.Now.AddDays(1)
            };

            var result = await _service.AddAsync(dto);

            result.IsSuccess.Should().BeTrue(result.Error);
            result.Value.Should().NotBeEmpty();
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Mecanico>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddAsync_MapeamentoLancaExcecao_RetornaFalha()
        {
            // DTO sem campos obrigatórios gera exceção na criação da entidade
            var dtoInvalido = new CriarMecanicoDTO
            {
                Nome = "", // inválido
                Email = "invalido",
                Telefone = "invalido",
                Senha = "",
                Especialidade = "Invalida",
                Nivel = "",
                DataContratacao = DateTime.UtcNow.AddDays(3)
            };

            var result = await _service.AddAsync(dtoInvalido);

            result.IsSuccess.Should().BeFalse();
        }

        // ==================== UpdateAsync ====================

        [Fact]
        public async Task UpdateAsync_MecanicoExistente_AtualizaESalva()
        {
            var mecanico = CriarMecanicoValido();
            _repoMock.Setup(r => r.GetByIdAsync(mecanico.Id)).ReturnsAsync(mecanico);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new AtualizarMecanicoDTO
            {
                Id = mecanico.Id,
                Email = "novo@email.com",
                Telefone = "11912345678"
            };

            var result = await _service.UpdateAsync(dto);

            result.IsSuccess.Should().BeTrue();
            _repoMock.Verify(r => r.Update(mecanico), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_MecanicoInexistente_RetornaFalha()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Mecanico?)null);

            var result = await _service.UpdateAsync(new AtualizarMecanicoDTO { Id = Guid.NewGuid() });

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== ObterDisponiveisAsync ====================

        [Fact]
        public async Task ObterDisponiveisAsync_RetornaTodosMapeados()
        {
            var mecanicos = new List<Mecanico> { CriarMecanicoValido(), CriarMecanicoValido() };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(mecanicos);

            var result = await _service.ObterDisponiveisAsync();

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
        }

        // ==================== AtualizarOcupacaoAsync ====================

        [Fact]
        public async Task AtualizarOcupacaoAsync_MecanicoExistente_AlteraOcupacaoESalva()
        {
            var mecanico = CriarMecanicoValido();
            // Simular trabalhos ativos para testar a lógica de ocupação
            for (int i = 0; i < 5; i++)
                mecanico.TrabalhosAtivos.Add(Guid.NewGuid());
            _repoMock.Setup(r => r.GetByIdAsync(mecanico.Id)).ReturnsAsync(mecanico);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.AtualizarOcupacaoAsync(mecanico.Id);

            result.IsSuccess.Should().BeTrue();
            mecanico.Ocupado.Should().Be(NivelOcupacaoMecanico.Indisponivel); // 5 trabalhos
            _repoMock.Verify(r => r.Update(mecanico), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AtualizarOcupacaoAsync_MecanicoInexistente_RetornaFalha()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Mecanico?)null);

            var result = await _service.AtualizarOcupacaoAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== RemoveAsync ====================

        [Fact]
        public async Task RemoveAsync_MecanicoExistente_RemoveESalva()
        {
            var mecanico = CriarMecanicoValido();
            _repoMock.Setup(r => r.GetByIdAsync(mecanico.Id)).ReturnsAsync(mecanico);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.RemoveAsync(mecanico.Id);

            result.IsSuccess.Should().BeTrue();
            _repoMock.Verify(r => r.Remove(mecanico), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_MecanicoInexistente_RetornaFalha()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Mecanico?)null);

            var result = await _service.RemoveAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== MÉTODO AUXILIAR ====================

        private static Mecanico CriarMecanicoValido()
        {
            var mecanico = new Mecanico(
                "Mecânico Teste",
                "mecanico@teste.com",
                "11999999999",
                "Senha123",
                EspecialidadeMecanico.Mecanica,
                NivelFuncionario.Junior,
                DateTime.Now.AddDays(1)
            );
            typeof(Mecanico).BaseType?.GetProperty("Id")?.SetValue(mecanico, Guid.NewGuid());
            return mecanico;
        }
    }
}