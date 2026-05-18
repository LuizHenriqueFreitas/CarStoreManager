// Este arquivo nao foi revisado nem documentado
using FluentAssertions;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Concessionaria;

public class VendedorTests
{
    // ==================== CONSTRUTOR ====================

    [Fact]
    public void Construtor_CamposValidos_AtribuiCorretamente()
    {
        var data = DateTime.UtcNow.AddDays(6);

        var vendedor = new Vendedor(
            "Ana Vendedora",
            "ana@concessionaria.com",
            "11977776666",
            "Senha1",
            NivelFuncionario.Junior,
            data
        );

        vendedor.Nome.Should().Be("Ana Vendedora");
        vendedor.Email.GetEmail().Should().Be("ana@concessionaria.com");
        vendedor.GetTelefone().Should().Be("(11) 97777-6666");
        vendedor.Role.Should().Be(RoleUsuario.Vendedor);
        vendedor.DadosFuncionario.GetNivel().Should().Be(NivelFuncionario.Junior);
        vendedor.DadosFuncionario.GetDataContratacao().Should().Be(data);
    }

    // ==================== GETTERS ====================

    [Fact]
    public void GetNivel_RetornaStringDoNivel()
    {
        var vendedor = CriarVendedor(NivelFuncionario.Pleno);
        vendedor.GetNivel().Should().Be("Pleno"); // ToString() do enum
    }

    [Fact]
    public void GetDataContratacao_RetornaDataFornecida()
    {
        var data = DateTime.UtcNow.AddDays(12);
        var vendedor = new Vendedor(
            "Nome",
            "email@teste.com", 
            "11999999999", 
            "Senha1", 
            NivelFuncionario.Senior, 
            data
        );

        vendedor.GetDataContratacao().Should().Be(data);
    }

    // ==================== ATUALIZAR NÍVEL ====================

    [Fact]
    public void AtualizarNivelVendedor_AlteraNivelCorretamente()
    {
        var vendedor = CriarVendedor(NivelFuncionario.Junior);
        vendedor.AtualizarNivelVendedor(NivelFuncionario.Pleno);
        vendedor.DadosFuncionario.GetNivel().Should().Be(NivelFuncionario.Pleno);
    }

    // ==================== MÉTODO AUXILIAR ====================

    private static Vendedor CriarVendedor(NivelFuncionario nivel = NivelFuncionario.Junior)
    {
        return new Vendedor(
            "Vendedor Padrão",
            "vendedor@teste.com",
            "11988888888",
            "Senha1",
            nivel,
            DateTime.UtcNow.AddDays(5)
        );
    }
}