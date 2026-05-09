using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades;

/*
    Este arquivo implementa os testes automaticos da classe Usuario.cs
    um VO do sistema, que inclui regra internas de funcionamento.

    Esta classe tem testes automaticos implementados para:
        Criar um usuario valido com todos os roles,
        Bloquear criação de um usuario com nome invalido,
        Bloquear criação de um usuario com email invalido,
        Bloquear criação de um usuario com telefone invalido,
        Verifica a atualização de dados pessoais,
        Verifica a atualização de senha,
        Verifica a função de Desativar Usuario,
        Verifica a função de Reativar Usuario
*/

public class UsuarioTest
{
    //cria usuarios validos de cada role
    [Theory]
    [InlineData(RoleUsuario.Admin)]
    [InlineData(RoleUsuario.Mecanico)]
    [InlineData(RoleUsuario.Vendedor)]
    public void Deve_Criar_Usuario_Valido(RoleUsuario role)
    {
        // cenario
        var user = new Usuario(
            "Pedro Silveira",
            "pedro@email.com",
            "11 98556-7124",
            "Senha1",
            3000,
            role
        );
        // validação
        Assert.Equal("Pedro Silveira", user.GetNome());
        Assert.Equal("pedro@email.com", user.GetEmail());
        Assert.Equal("(11) 98556-7124", user.GetTelefone());
        Assert.True(user.GetSenha().Verificar("Senha1"));
        Assert.Equal(role.ToString(), user.GetRole());
    }

    //bloqueia criação de usuario com nome vazio
    [Fact]
    public void Deve_Bloquear_Usuario_Nome_Invalido()
    {
        // validação
        Assert.Throws<ArgumentException>(() => 
            new Usuario(
            "   ",
            "pedro@email.com",
            "11 98556-7124",
            "Senha1",
            3000,
            RoleUsuario.Admin
        ));
    }

    //bloqueia criação de usuario com email invalido
    [Theory]
    [InlineData("teste")]
    [InlineData("teste@")]
    [InlineData("@email.com")]
    [InlineData("teste@email")]
    [InlineData("teste@.com")]
    public void Deve_Bloquear_Usuario_Email_Invalido(string email)
    {
        // validação
        Assert.Throws<ArgumentException>(() => 
            new Usuario(
            "Pedro Silveira",
            email,
            "11 98556-7124",
            "Senha1",
            3000,
            RoleUsuario.Admin
        ));
    }

    //bloqueia criação de usuario com telefone invalido
    [Theory]
    [InlineData("+55 (11) 99746-2324")]
    [InlineData("11 99746 23246")]
    [InlineData("11 9974-324")]
    [InlineData("")]
    public void Deve_Bloquear_Usuario_Telefone_Invalido(string numero)
    {
        // validação
        Assert.Throws<ArgumentException>(() => 
            new Usuario(
            "Pedro Silveira",
            "pedro@email.com",
            numero,
            "Senha1",
            3000,
            RoleUsuario.Admin
        ));
    }

    /*
        valida a atualização dos dados:
        nome, email e telefone
    */
    [Fact]
    public void Deve_Atualizar_Dados_Usuario()
    {
        // cenario
        var user = new Usuario(
            "Pedro Silveira",
            "pedro@email.com",
            "11 98556-7124",
            "Senha1",
            3000,
            RoleUsuario.Admin
        );
        // aplicação
        user.AtualizarDadosPessoais("Pedro Silva", "Silva@email.com", "41998860542");
        // validação
        Assert.Equal("Pedro Silva", user.GetNome());
        Assert.Equal("Silva@email.com", user.GetEmail());
        Assert.Equal("(41) 99886-0542", user.GetTelefone());
        Assert.True(user.GetSenha().Verificar("Senha1"));
        Assert.Equal("Admin", user.GetRole());
    }

    //verifica a atualização de senha do usuario
    [Fact]
    public void Deve_Atualizar_Senha_Usuario()
    {
        // cenario
        var user = new Usuario(
            "Pedro Silveira",
            "pedro@email.com",
            "11 98556-7124",
            "Senha1",
            3000,
            RoleUsuario.Admin
        );
        // aplicação
        user.AtualizarSenha("Senha2");
        // validação
        Assert.Equal("Pedro Silveira", user.GetNome());
        Assert.Equal("pedro@email.com", user.GetEmail());
        Assert.Equal("(11) 98556-7124", user.GetTelefone());
        Assert.True(user.GetSenha().Verificar("Senha2"));
        Assert.Equal("Admin", user.GetRole());
    }

    //verifica a função de desativar o usuario
    [Fact]
    public void Deve_Desativar_Usuario()
    {
        // cenario
        var user = new Usuario(
            "Pedro Silveira",
            "pedro@email.com",
            "11 98556-7124",
            "Senha1",
            3000,
            RoleUsuario.Admin
        );
        // aplicação
        user.Desativar();
        // validação
        Assert.False(user.GetAtivo());
    }

    //verifica a função de reativar o usuario
    [Fact]
    public void Deve_Ativar_Usuario()
    {
        // cenario
        var user = new Usuario(
            "Pedro Silveira",
            "pedro@email.com",
            "11 98556-7124",
            "Senha1",
            3000,
            RoleUsuario.Admin
        );
        // aplicação
        user.Desativar();
        if(!user.GetAtivo())
            user.Reativar();
        else    
            Assert.Fail("O usuario não foi desativado para ser reativado depois");
        // validação
        Assert.True(user.GetAtivo());
    }
}