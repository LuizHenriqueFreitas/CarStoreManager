using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Domain.ValueObjects;

/*
    Este arquivo implementa os testes automaticos da classe Telefone.cs
    um VO do sistema, que inclui regra internas de funcionamento.

    Testes automaticos implementados para:
        Criar senhas permitidas
            (minimo 6 caracteres, 1 numero e 1 letra maiuscula),
        Bloquear senhas fora das diretrizes,
        Recriar senha apartir do rash,
        Atualizar uma senha por outra Valida,
        Bloquear atualização por senha invalida.
*/

public class SenhaTest
{
    //criando senhas validas
    [Theory]
    [InlineData("Senha1")]
    [InlineData("10 Senha")]
    [InlineData("S3NH45")]
    [InlineData("123A56")]
    public void Deve_Criar_Senha_Valida(string senha)
    {
        // cenario
        var secreta = new Senha(senha);

        Assert.False(secreta.GetSenhaHash() == senha);
        Assert.True(secreta.Verificar(senha));
    }

    //bloqueando senhas invalidas
    [Theory]
    [InlineData("senha")]
    [InlineData("Senha")]
    [InlineData("senha 1")]
    [InlineData("123456")]
    
    public void Deve_Bloquear_Senha_Invalida(string senha)
    {
        Assert.Throws<ArgumentException>(() => new Senha(senha));
    }

    //recriar a mesma senha apartir do hash
    [Fact]
    public void Deve_Recriar_Senha_A_Partir_Do_Hash()
    {
        var senha = new Senha("Senha1");
        var hash = senha.GetSenhaHash();

        Senha reconstruida = senha.FromHash(hash);

        Assert.True(reconstruida.Verificar("Senha1"));
    }

    //verificando a atualização de senha permitida
    [Theory]
    [InlineData("Senha1")]
    [InlineData("10 Senha")]
    [InlineData("S3NH45")]
    [InlineData("123A56")]
    public void Deve_Atualizar_Senha_Valida(string novaSenha)
    {
        // cenario
        var senha1 = new Senha("SenhaOriginal1");
        senha1.AtualizarSenha(novaSenha);

        Assert.True(senha1.Verificar(novaSenha));
    }

    //verificando o bloqueio de senha NÃO permitida
    [Theory]
    [InlineData("senha")]
    [InlineData("Senha")]
    [InlineData("senha 1")]
    [InlineData("123456")]
    public void Deve_Bloquear_Atualizacao_Invalida(string novaSenha)
    {
        // cenario
        var senha1 = new Senha("SenhaOriginal1");

        Assert.Throws<ArgumentException>(() => senha1.AtualizarSenha(novaSenha));
    }
}