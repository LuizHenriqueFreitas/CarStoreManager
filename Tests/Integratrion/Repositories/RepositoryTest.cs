
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Infrastructure.Data;
using CarStoreManager.Infrastructure.Repositories;

namespace CarStoreManager.Tests.Integration.Repositories;

public class RepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly Repository<VeiculoCliente> _repository;

    //cenario de conexão do sistema
    public RepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new Repository<VeiculoCliente>(_context);
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    /* ================================
        Abaixo seguem os testes em si
        =============================== */

    //busca entidade por id
    [Fact]
    public async Task GetByIdAsync_EntidadeExistente_RetornaEntidade()
    {
        // cenario
        var veiculo = CriarVeiculoCliente();
        _context.Set<VeiculoCliente>().Add(veiculo);
        await _context.SaveChangesAsync();

        // aplicação
        var resultado = await _repository.GetByIdAsync(veiculo.Id);

        // validação
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(veiculo.Id);
        resultado.Marca.Should().Be(veiculo.Marca);
    }

    //deve retornar null para id inexistente
    [Fact]
    public async Task GetByIdAsync_IdInexistente_RetornaNull()
    {
        var resultado = await _repository.GetByIdAsync(Guid.NewGuid());
        resultado.Should().BeNull();
    }

    //deve retornar lista vazia caso nao haja cadastros
    [Fact]
    public async Task GetAllAsync_SemDados_RetornaListaVazia()
    {
        var resultado = await _repository.GetAllAsync();
        resultado.Should().BeEmpty();
    }

    //deve retornar todas as entidades cadastradas
    [Fact]
    public async Task GetAllAsync_ComDados_RetornaTodos()
    {
        // cenario
        var v1 = CriarVeiculoCliente();
        var v2 = CriarVeiculoCliente();
        _context.Set<VeiculoCliente>().AddRange(v1, v2);
        await _context.SaveChangesAsync();

        // aplicação
        var resultado = await _repository.GetAllAsync();

        // validação
        resultado.Should().HaveCount(2);
        resultado.Select(v => v.Id).Should().Contain(new[] { v1.Id, v2.Id });
    }

    //adiciona entidade persistente no banco
    [Fact]
    public async Task AddAsync_EntidadeValida_PersisteNoBanco()
    {
        // cenario
        var veiculo = CriarVeiculoCliente();

        // aplicação
        await _repository.AddAsync(veiculo);
        await _repository.SaveChangesAsync();

        // validação
        var salvo = await _repository.GetByIdAsync(veiculo.Id);
        salvo.Should().NotBeNull();
        salvo!.Marca.Should().Be(veiculo.Marca);
    }

    //utilizando o metodo AddAsync sem SaveChangesAsync - nao deve persistir no banco
    [Fact]
    public async Task AddAsync_EntidadeSemSalvar_NaoPersiste()
    {
        // cenario
        var veiculo = CriarVeiculoCliente();

        // aplicação
        await _repository.AddAsync(veiculo);

        // validação
        var naoSalvo = await _context.Set<VeiculoCliente>()
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == veiculo.Id);
        naoSalvo.Should().BeNull();
    }

    //valida atualização de entidade existente e persistencia dos novos dados
    [Fact]
    public async Task Update_EntidadeExistente_AtualizaDados()
    {
        // cenario
        var veiculo = CriarVeiculoCliente();
        _context.Set<VeiculoCliente>().Add(veiculo);
        await _context.SaveChangesAsync();

        // aplicação
        veiculo.AtualizarDadosVeiculoCliente("NovaMarca", "NovoModelo", "Azul", 2022);
        _repository.Update(veiculo);
        await _repository.SaveChangesAsync();

        // validação
        var atualizado = await _repository.GetByIdAsync(veiculo.Id);
        atualizado!.Marca.Should().Be("NovaMarca");
        atualizado.Modelo.Should().Be("NovoModelo");
    }

    //valida operações de desanexar entidade do contexto, fazer alterações e anexar novamente atualizada
    [Fact]
    public async Task Update_EntidadeNaoRastreada_DeveSerAnexadaEAtualizada()
    {
        // cenario
        var veiculo = CriarVeiculoCliente();
        _context.Set<VeiculoCliente>().Add(veiculo);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear(); // desanexa do contexto

        // Modifica a entidade após desanexar
        veiculo.AtualizarMarcaVeiculoCliente("MarcaDesanexada");

        // aplicação
        _repository.Update(veiculo); // deve anexar e marcar como modificada
        await _repository.SaveChangesAsync();

        // validação
        var atualizado = await _repository.GetByIdAsync(veiculo.Id);
        atualizado!.Marca.Should().Be("MarcaDesanexada");
    }

    //deve remover entidade existente do banco de dados
    [Fact]
    public async Task Remove_EntidadeExistente_RemoveDoBanco()
    {
        // cenario
        var veiculo = CriarVeiculoCliente();
        _context.Set<VeiculoCliente>().Add(veiculo);
        await _context.SaveChangesAsync();

        // aplicação
        _repository.Remove(veiculo);
        await _repository.SaveChangesAsync();

        // validação
        var removido = await _repository.GetByIdAsync(veiculo.Id);
        removido.Should().BeNull();
    }

    //remove entidade nao rastreada - anexa e remove
    [Fact]
    public async Task Remove_EntidadeNaoRastreada_AnexaERemove()
    {
        // cenario
        var veiculo = CriarVeiculoCliente();
        _context.Set<VeiculoCliente>().Add(veiculo);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear(); // desanexa

        // aplicação
        _repository.Remove(veiculo); // anexa e marca como deletado
        await _repository.SaveChangesAsync();

        // validação
        var removido = await _repository.GetByIdAsync(veiculo.Id);
        removido.Should().BeNull();
    }
    
    //valida funcionamento de SaveChangesAsync para persistir alterações apos modificações
    [Fact]
    public async Task SaveChangesAsync_AposModificacoes_PersisteAlteracoes()
    {
        // cenario
        var veiculo = CriarVeiculoCliente();
        await _repository.AddAsync(veiculo);

        // aplicação
        await _repository.SaveChangesAsync();

        // validação
        var salvo = await _context.Set<VeiculoCliente>()
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == veiculo.Id);
        salvo.Should().NotBeNull();
    }
    
    //metodo auxiliar de cenario para teste das funcionalidades
    private static VeiculoCliente CriarVeiculoCliente(Guid? clienteId = null)
    {
        return new VeiculoCliente(
            clienteId ?? Guid.NewGuid(),
            "Honda",
            "Civic",
            "Preto",
            2020
        );
    }
}