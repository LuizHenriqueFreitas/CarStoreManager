using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;
using CarStoreManager.Infrastructure.Data;
using CarStoreManager.Infrastructure.Repositories;

namespace CarStoreManager.Tests.Integration.Repositories
{
public class UsuarioRepositoryTests : IDisposable
{
private readonly SqliteConnection _connection;
private readonly AppDbContext _context;
private readonly UsuarioRepository _repository;

public UsuarioRepositoryTests()
{
_connection = new SqliteConnection("DataSource=:memory:");
_connection.Open();

var options = new DbContextOptionsBuilder<AppDbContext>()
.UseSqlite(_connection)
.Options;

_context = new AppDbContext(options);
_context.Database.EnsureCreated();

_repository = new UsuarioRepository(_context);
}

public void Dispose()
{
_connection.Close();
_connection.Dispose();
}

// ==================== GetByIdAsync ====================

[Fact]
public async Task GetByIdAsync_UsuarioExistente_RetornaUsuario()
{
var admin = CriarAdmin("admin@teste.com");
await _repository.AddAsync(admin);
await _repository.SaveChangesAsync();

var resultado = await _repository.GetByIdAsync(admin.Id);

resultado.Should().NotBeNull();
resultado!.Email.Endereco.Should().Be("admin@teste.com");
}

[Fact]
public async Task GetByIdAsync_IdInexistente_RetornaNull()
{
var resultado = await _repository.GetByIdAsync(Guid.NewGuid());
resultado.Should().BeNull();
}

// ==================== GetAllAsync ====================

[Fact]
public async Task GetAllAsync_ComDados_RetornaTodos()
{
var a1 = CriarAdmin("a1@teste.com");
var a2 = CriarAdmin("a2@teste.com");
await _repository.AddAsync(a1);
await _repository.AddAsync(a2);
await _repository.SaveChangesAsync();

var resultado = await _repository.GetAllAsync();
resultado.Should().HaveCount(2);
}

[Fact]
public async Task GetAllAsync_SemDados_RetornaListaVazia()
{
var resultado = await _repository.GetAllAsync();
resultado.Should().BeEmpty();
}

// ==================== ObterPorEmailAsync ====================

[Fact]
public async Task ObterPorEmailAsync_EmailExistente_RetornaUsuario()
{
var admin = CriarAdmin("admin@teste.com");
await _repository.AddAsync(admin);
await _repository.SaveChangesAsync();

var resultado = await _repository.ObterPorEmailAsync("admin@teste.com");
resultado.Should().NotBeNull();
resultado!.Email.Endereco.Should().Be("admin@teste.com");
}

[Fact]
public async Task ObterPorEmailAsync_EmailInexistente_RetornaNull()
{
var resultado = await _repository.ObterPorEmailAsync("naoexiste@teste.com");
resultado.Should().BeNull();
}

[Fact]
public async Task ObterPorEmailAsync_CaseInsensitive_RetornaUsuario()
{
var admin = CriarAdmin("Admin@Teste.com");
await _repository.AddAsync(admin);
await _repository.SaveChangesAsync();

// O repositório aplica ToLower no email
var resultado = await _repository.ObterPorEmailAsync("admin@teste.com");
resultado.Should().NotBeNull();
}

// ==================== EmailExisteAsync ====================

[Fact]
public async Task EmailExisteAsync_EmailExistente_RetornaTrue()
{
var admin = CriarAdmin("existente@teste.com");
await _repository.AddAsync(admin);
await _repository.SaveChangesAsync();

var existe = await _repository.EmailExisteAsync("existente@teste.com");
existe.Should().BeTrue();
}

[Fact]
public async Task EmailExisteAsync_EmailInexistente_RetornaFalse()
{
var existe = await _repository.EmailExisteAsync("naoexiste@teste.com");
existe.Should().BeFalse();
}

// ==================== AddAsync ====================

[Fact]
public async Task AddAsync_UsuarioValido_PersisteCorretamente()
{
var admin = CriarAdmin("novo@teste.com");
await _repository.AddAsync(admin);
await _repository.SaveChangesAsync();

var salvo = await _repository.GetByIdAsync(admin.Id);
salvo.Should().NotBeNull();
salvo!.Nome.Should().Be("Admin Teste");
salvo.Email.Endereco.Should().Be("novo@teste.com");
}

// ==================== Update ====================

[Fact]
public async Task Update_UsuarioExistente_AtualizaDados()
{
var admin = CriarAdmin("old@teste.com");
await _repository.AddAsync(admin);
await _repository.SaveChangesAsync();

admin.AtualizarDadosPessoais("Novo Nome", "new@teste.com", "11912345678");
_repository.Update(admin);
await _repository.SaveChangesAsync();

var atualizado = await _repository.GetByIdAsync(admin.Id);
atualizado!.Nome.Should().Be("Novo Nome");
atualizado.Email.Endereco.Should().Be("new@teste.com");
}

// ==================== Remove ====================

[Fact]
public async Task Remove_UsuarioExistente_RemoveRegistro()
{
var admin = CriarAdmin("remove@teste.com");
await _repository.AddAsync(admin);
await _repository.SaveChangesAsync();

_repository.Remove(admin);
await _repository.SaveChangesAsync();

var removido = await _repository.GetByIdAsync(admin.Id);
removido.Should().BeNull();
}

// ==================== MÉTODO AUXILIAR ====================

private static Admin CriarAdmin(string email)
{
return new Admin(
"Admin Teste",
email,
"00000000000",
"Admin@123",
3000
);
}
}
}