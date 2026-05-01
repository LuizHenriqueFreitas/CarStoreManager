namespace CarStoreManager.Tests.Domain.Entities;

using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

public class UsuarioTest
{
    [Fact]
    public void ValidPasswordVerification()
    {
        // Given
        var email = new Email("pedro@email.com");
        var telefone = new Telefone("11996620201");
        var role = RoleUsuario.Admin;
        var senha = "123456";

        // Hash da senha (simula como o sistema cria usuários)
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);

        Usuario pedro = new Usuario(
            "Pedro Silveira",
            email,
            telefone,
            senhaHash, // o construtor espera o hash
            role);

        // When
        bool senhaValida = BCrypt.Net.BCrypt.Verify(senha, pedro.SenhaHash);

        // Then
        Assert.True(senhaValida, "A senha deveria ser válida.");
    }
}