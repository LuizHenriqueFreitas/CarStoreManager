// classe base para carros de clientes da oficina

using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Oficina;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de VeiculoCliente.cs

    Esta classe tem testes automaticos implementados para:
        Nada ainda
*/

public class VeiculoCliente : Entity
{
    public Guid ClienteId { get; private set; }

    public string Marca { get; private set; } = null!;
    public string Modelo { get; private set; } = null!;
    public string Cor { get; private set; } = null!;
    public Ano Ano { get; private set; } = null!;

    public List<OrdemServico> HistoricoServicos { get; private set; } = new();

    protected VeiculoCliente() { }

    //construtor verifica se o Id do cliente não é vazio
    public VeiculoCliente(
        Guid clienteId,
        string marca,
        string modelo,
        string cor,
        int ano)
    {
        if (clienteId == Guid.Empty)
            throw new ArgumentException("Cliente inválido");

        ClienteId = clienteId;

        AtualizarMarcaVeiculoCliente(marca);
        AtualizarModeloVeiculoCliente(modelo);
        AtualizarCorVeiculoCliente(cor);

        Ano = new Ano(ano);
    }

    /* ================================
        metodos GETTERS dos atributos
     ================================*/
    public Guid GetClienteId() => ClienteId;
    public string GetMarca() => Marca;
    public string GetModelo() => Modelo;
    public string GetCor() => Cor;
    public int GetAno() => Ano.GetValorAno();
    public string GetHistorico()
    {
        
        if (HistoricoServicos.Count == 0)
        {
            return "Veiculo não possui histórico.";
        }
        string temp = "Relatorio: ";
        for(int i =0; i< HistoricoServicos.Count; i++)
        {
            temp += HistoricoServicos[i] + "\n";
        }
        return $"{temp}.";
    }
    //a descrição não é um atributo, mas é uma formatação com os dados do veiculo
    public string GetDescricao() => $"{Marca} {Modelo} {Cor} {Ano.GetValorAno()}";
    
    /* ================================
        metodos SETTERS dos atributos
        os metodos para mudar:
        Marca, Modelo e Cor do veiculo
        verificam que o campo não esteja vazio.
     ================================*/
    public void AtualizarMarcaVeiculoCliente(string marca)
    {
        if (string.IsNullOrWhiteSpace(marca))
            throw new ArgumentException("Marca inválida");
        Marca = marca.Trim();
    }

    public void AtualizarModeloVeiculoCliente(string modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo))
            throw new ArgumentException("Modelo inválido");
        Modelo = modelo.Trim();
    }

    public void AtualizarCorVeiculoCliente(string cor)
    {
        if (string.IsNullOrWhiteSpace(cor))
            throw new ArgumentException("Cor inválida");
        Cor = cor.Trim();
    }

    //atualizar todos os atributos de uma vez utilizando dos metodos separados
    public void AtualizarDadosVeiculoCliente(string marca, string modelo, string cor, int ano)
    {
        AtualizarMarcaVeiculoCliente(marca);
        AtualizarModeloVeiculoCliente(modelo);
        AtualizarCorVeiculoCliente(cor);
        Ano.AtualizaAno(ano);
    }
}