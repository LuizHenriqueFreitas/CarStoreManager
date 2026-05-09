namespace CarStoreManager.Domain.ValueObjects;

/*
    Neste arquivo temos as implementações de verificações 
    e regras de negócios do VO de Telefone, alem dos metodos 
    get e set.

    Testes automaticos implementados para:
        Criar senhas permitidas
            (minimo 6 caracteres, 1 numero e 1 letra maiuscula),
        Bloquear senhas fora das diretrizes,
        Recriar senha apartir do rash,
        Atualizar uma senha por outra Valida,
        Bloquear atualização por senha invalida.
*/

public class Senha
{
    private string Hash { get; set; } = null!;

    protected Senha() { }

    /*
        metodo construtor.
        com encriptaçao interna
    */
    public Senha(string senha)
    {
        Validar(senha);
        Hash = BCrypt.Net.BCrypt.HashPassword(senha);
    }

    //cria um novo hash caso seja necessario
    public void Criar(string senhaTexto)
    {
        Validar(senhaTexto);
        var hash = BCrypt.Net.BCrypt.HashPassword(senhaTexto);
        Hash = hash;
    }

    //traduz de volta do rash para original
    public Senha FromHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Hash inválido");
        return new Senha {Hash = hash};
    }

    //pega o Hash da senha criptografado
    public string GetSenhaHash()
    {
        return Hash;
    }

    //atualizar uma senha ja existente
    public void AtualizarSenha(string novaSenha)
    {
        Validar(novaSenha);
        Hash = BCrypt.Net.BCrypt.HashPassword(novaSenha);
    }

    //verifica se o texto da senha bate com a senha emcriptada
    public bool Verificar(string senhaTexto)
        => BCrypt.Net.BCrypt.Verify(senhaTexto, Hash);

    /*
        tratamento de excessoes da senha
        Uma senha não deve:
        - ser vazia,
        - ter menos que 6 caracteres,
        - estar SEM nenhum caracteres maiúsculo,
        - estar SEM nenhum numero
    */
    public static void Validar(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
            throw new ArgumentException("Senha não pode ser vazia");

        if (senha.Length < 6)
            throw new ArgumentException("Senha deve ter pelo menos 6 caracteres");

        if (!senha.Any(char.IsUpper))
            throw new ArgumentException("Senha deve conter pelo menos uma letra maiúscula");

        if (!senha.Any(char.IsDigit))
            throw new ArgumentException("Senha deve conter pelo menos um número");
    }
}