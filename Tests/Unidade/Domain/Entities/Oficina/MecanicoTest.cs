// Este arquivo nao foi revisado nem documentado
using FluentAssertions;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Oficina;

public class MecanicoTests
{
    // ==================== CONSTRUTOR ====================

    [Fact]
    public void Construtor_CamposValidos_AtribuiCorretamente()
    {
        var data = DateTime.UtcNow.AddDays(6);
        var mecanico = new Mecanico(
            "João Mecânico",
            "joao@oficina.com",
            "11988887777",
            "Senha1",
            EspecialidadeMecanico.Mecanica,
            NivelFuncionario.Pleno,
            data
        );

        mecanico.Nome.Should().Be("João Mecânico");
        mecanico.Email.GetEmail().Should().Be("joao@oficina.com");
        mecanico.GetTelefone().Should().Be("(11) 98888-7777"); // assumindo que ToString() retorna o número formatado
        mecanico.Role.Should().Be(RoleUsuario.Mecanico);
        mecanico.Especialidade.Should().Be(EspecialidadeMecanico.Mecanica);
        mecanico.DadosFuncionario.GetNivel().Should().Be(NivelFuncionario.Pleno);
        mecanico.DadosFuncionario.GetDataContratacao().Should().Be(data);
        mecanico.Ocupado.Should().Be(NivelOcupacaoMecanico.Disponivel); // inicialmente disponível (lista vazia)
        mecanico.TrabalhosAtivos.Should().BeEmpty();
    }

    // ==================== ALTERAR OCUPAÇÃO (regra de negócio principal) ====================

    [Theory]
    [InlineData(0, NivelOcupacaoMecanico.Disponivel)]
    [InlineData(2, NivelOcupacaoMecanico.Disponivel)]
    [InlineData(3, NivelOcupacaoMecanico.Ocupado)]
    [InlineData(4, NivelOcupacaoMecanico.Ocupado)]
    [InlineData(5, NivelOcupacaoMecanico.Indisponivel)]
    [InlineData(10, NivelOcupacaoMecanico.Indisponivel)]
    public void AlterarOcupado_DeAcordoComQuantidadeDeTrabalhos_DefineNivelCorreto(int quantidadeTrabalhos, NivelOcupacaoMecanico nivelEsperado)
    {
        var mecanico = CriarMecanicoValido();
        // Preenche a lista de trabalhos ativos com Guids fictícios
        for (int i = 0; i < quantidadeTrabalhos; i++)
            mecanico.TrabalhosAtivos.Add(Guid.NewGuid());

        mecanico.AlterarOcupado();

        mecanico.Ocupado.Should().Be(nivelEsperado);
    }

    [Fact]
    public void AlterarOcupado_ListaVazia_DefineDisponivel()
    {
        var mecanico = CriarMecanicoValido();
        mecanico.TrabalhosAtivos.Clear();
        mecanico.AlterarOcupado();
        mecanico.Ocupado.Should().Be(NivelOcupacaoMecanico.Disponivel);
    }

    // ==================== MÉTODOS AUXILIARES ====================

    private static Mecanico CriarMecanicoValido()
    {
        return new Mecanico(
            "Mecânico Padrão",
            "mecanico@teste.com",
            "11999999999",
            "Senha1",
            EspecialidadeMecanico.Funilaria,
            NivelFuncionario.Junior,
            DateTime.UtcNow.AddDays(6)
        );
    }
}