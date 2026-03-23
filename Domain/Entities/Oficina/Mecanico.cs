using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Oficina;

public class Mecanico : Entity
{
    public string Nome { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public Telefone Telefone { get; private set; } = null!;

    public EspecialidadeMecanico Especialidade { get; private set; }

    public Dinheiro ValorHora { get; private set; } = null!;

    public NivelOcupacaoMecanico NivelOcupacao { get; private set; }

    // controle interno (opcional)
    public int OrdensAtivas { get; private set; }

    protected Mecanico() { }

    public Mecanico(
        string nome,
        Email email,
        Telefone telefone,
        EspecialidadeMecanico especialidade,
        Dinheiro valorHora)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome inválido");

        Nome = nome.Trim();
        Email = email;
        Telefone = telefone;
        Especialidade = especialidade;
        ValorHora = valorHora;

        OrdensAtivas = 0;
        AtualizarNivel();
    }

    // =========================
    // REGRAS DE NEGÓCIO
    // =========================

    public void AtualizarDados(string nome, Email email, Telefone telefone)
    {
        Nome = nome;
        Email = email;
        Telefone = telefone;
    }

    public void AtualizarValorHora(Dinheiro novoValor)
    {
        ValorHora = novoValor;
    }

    // =========================
    // OCUPAÇÃO
    // =========================

    public void AtribuirOrdem()
    {
        if (OrdensAtivas >= 5)
            throw new InvalidOperationException("Mecânico sobrecarregado");
            
        OrdensAtivas++;
        AtualizarNivel();
    }

    public void FinalizarOrdem()
    {
        if (OrdensAtivas > 0)
            OrdensAtivas--;

        AtualizarNivel();
    }

    private void AtualizarNivel()
    {
        if (OrdensAtivas == 0)
            NivelOcupacao = NivelOcupacaoMecanico.Livre;
        else if (OrdensAtivas <= 2)
            NivelOcupacao = NivelOcupacaoMecanico.Disponivel;
        else
            NivelOcupacao = NivelOcupacaoMecanico.Ocupado;
    }

    // =========================
    // CÁLCULO
    // =========================

    public Dinheiro CalcularCustoServico(int horas)
    {
        if (horas <= 0)
            throw new ArgumentException("Horas inválidas");

        return ValorHora.Multiplicar(horas);
    }
}