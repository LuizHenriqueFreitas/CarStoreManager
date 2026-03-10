namespace CarStoreManager.Domain.ValueObjects;

public class FuncionarioData
{
    public string cpf { get; private set; }

    public FuncionarioData(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("CPF não pode ser vazio.");

        this.cpf = cpf;
    }

    public override string ToString()
    {
        return cpf;
    }
}
