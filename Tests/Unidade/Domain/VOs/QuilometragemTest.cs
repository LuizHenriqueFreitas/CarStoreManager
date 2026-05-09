using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Domain.ValueObjects;

/*
    Este arquivo implementa os testes automaticos da classe Quilometragem.cs
    um VO do sistema, que inclui regra internas de funcionamento.

    Testes automaticos implementados para:
        Criar uma quilometragem valida,
        Bloquear a criação de quilometragem invalida,
        Valida atualização de quilometragem,
        Bloqueia deminuir uma quilometragem ja registrada,
        Valida formatação de saida do metodo ToString
*/

public class QuilometragemTest
{
    //cria uma quilometragem que deve ser aceita
    [Fact]
    public void Deve_Criar_Quilometragem_Valida()
    {
        // cenario
        var novoKm = new Quilometragem(100);
        // validação
        Assert.True(novoKm.GetQuilometragem() == 100);
    }

    //cria uma quilometragem que deve ser bloqueada
    [Fact]
    public void Deve_Bloquear_Quilometragem_Invalida()
    {
        Assert.Throws<ArgumentException>(() => new Quilometragem(-1));
    }

    /*
        faz uma atualização que deve ser permitida
        tambem valida se a atualização foi realmente feita
    */
    [Fact]
    public void Deve_Validar_Atualizar_Quilometragem()
    {
        // cenario
        var novoKm = new Quilometragem(100);
        // aplicação
        novoKm.AtualizarQuilometragem(200);
        // validação
        Assert.True(novoKm.GetQuilometragem() == 200);
    }

    //tenta fazer uma atualização que deve ser bloqueada
    [Fact]
    public void Deve_Bloquear_Atualizar_Quilometragem_Invalida()
    {
        var km = new Quilometragem(20);

        Assert.Throws<InvalidOperationException>(() => km.AtualizarQuilometragem(10));
    }

    //compara a saida do metodo ToString para validar a formatação
    [Fact]
    public void Verfica_Saida_Formatada_Quilometragem()
    {
        // cenario
        var km = new Quilometragem(160000);
        // verificação
        Assert.Equal("160.000 Km", km.ToString());
    }
}