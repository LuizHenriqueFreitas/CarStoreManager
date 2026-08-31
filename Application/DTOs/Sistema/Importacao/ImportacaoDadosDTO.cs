using CarStoreManager.Application.DTOs.Shared.Cliente;

namespace CarStoreManager.Application.DTOs.Sistema.Importacao;

/// <summary>
/// Formato do arquivo JSON aceito em Configurações → Importar dados. Cada
/// seção é opcional e processada nessa ordem (uma depende da anterior via
/// "chave" — um identificador de texto só válido dentro do arquivo, usado
/// pra referenciar um registro de outra seção sem precisar saber o Guid real
/// de antemão. Ex.: uma proposta usa "clienteChave": "cli-joao" em vez do
/// Guid do cliente, e o importador resolve isso na hora de criar):
///
///   1. Usuarios         (vendedores/mecânicos/etc. — referenciado por 9 e 10)
///   2. Clientes                                    (referenciado por 7, 8, 9, 10)
///   3. Fornecedores                                 (referenciado por Componentes)
///   4. Componentes                                  (referenciado por 10)
///   5. ChecklistPresets                             (referenciado por 10)
///   6. Despesas                                     (sem dependências, sem "chave")
///   7. VeiculosVenda                                (referenciado por 10)
///   8. VeiculosConsignados
///   9. VeiculosCliente                              (referenciado por 11)
///   10. PropostasVenda
///   11. OrdensServico
/// </summary>
public class ImportacaoDadosDTO
{
    public List<UsuarioImportDTO> Usuarios { get; set; } = new();
    public List<ClienteImportDTO> Clientes { get; set; } = new();
    public List<FornecedorImportDTO> Fornecedores { get; set; } = new();
    public List<ComponenteImportDTO> Componentes { get; set; } = new();
    public List<ChecklistPresetImportDTO> ChecklistPresets { get; set; } = new();
    public List<DespesaImportDTO> Despesas { get; set; } = new();
    public List<VeiculoVendaImportDTO> VeiculosVenda { get; set; } = new();
    public List<VeiculoConsignacaoImportDTO> VeiculosConsignados { get; set; } = new();
    public List<VeiculoClienteImportDTO> VeiculosCliente { get; set; } = new();
    public List<PropostaVendaImportDTO> PropostasVenda { get; set; } = new();
    public List<OrdemServicoImportDTO> OrdensServico { get; set; } = new();
}

/// <summary>Despesa mensal fixa (recorrente) — sem data própria, é um valor "atual" cadastrado, não um lançamento pontual.</summary>
public class DespesaImportDTO
{
    public string Nome { get; set; } = "";
    public decimal Valor { get; set; }
    /// <summary>Geral, Oficina ou Concessionaria.</summary>
    public string Setor { get; set; } = "Geral";
    /// <summary>Salario, Aluguel, Utilidades, Manutencao, Marketing, Impostos, Seguro, Investimento, Servicos ou Outros.</summary>
    public string Tipo { get; set; } = "Outros";
}

/// <summary>Modelo de checklist reutilizável entre OS (ex.: "Revisão Geral", "Troca de Óleo").</summary>
public class ChecklistPresetImportDTO
{
    public string Chave { get; set; } = "";
    public string Nome { get; set; } = "";
    public List<string> Itens { get; set; } = new();
}

/// <summary>Role: "Vendedor", "Mecanico", "Recepcionista", "ChefeOficina", "GerenteVendas" ou "Admin".</summary>
public class UsuarioImportDTO
{
    public string Chave { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string Telefone { get; set; } = "";
    /// <summary>Se omitida, usa a senha padrão de demonstração "Teste@123".</summary>
    public string? Senha { get; set; }
    /// <summary>"Junior", "Pleno" ou "Senior" — obrigatório pra todos os tipos, exceto Admin.</summary>
    public string? Nivel { get; set; }
    /// <summary>Obrigatória só pra Mecânico.</summary>
    public string? Especialidade { get; set; }
    /// <summary>Opcional — se informada, sobrescreve a data de contratação depois de criado (senão fica hoje).</summary>
    public DateTime? DataContratacao { get; set; }
}

public class ClienteImportDTO
{
    public string Chave { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Cpf { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string Email { get; set; } = "";
    public EnderecoDTO Endereco { get; set; } = new();
    /// <summary>Opcional — data em que o cliente "teria" sido cadastrado, pro histórico de ~2 anos.</summary>
    public DateTime? DataCriacao { get; set; }
}

public class FornecedorImportDTO
{
    public string Chave { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Cnpj { get; set; } = "";
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public EnderecoDTO? Endereco { get; set; }
}

/// <summary>Sistema (opcional): Motor, Suspensao, Direcao, Freios, Eletrica, Lataria, Transmissão, Arrefecimento, Escapamento, Acessorios ou Interior.</summary>
public class ComponenteImportDTO
{
    public string Chave { get; set; } = "";
    /// <summary>Obrigatório — todo componente precisa de um fornecedor já cadastrado (nesta seção ou por chave de um já existente).</summary>
    public string FornecedorChave { get; set; } = "";
    public string SKUInterno { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Descricao { get; set; } = "";
    public string MarcaFabricante { get; set; } = "";
    public string PartNumber { get; set; } = "";
    public string? CodigoOEM { get; set; }
    public string? CodigoBarras { get; set; }
    /// <summary>8 dígitos.</summary>
    public string NCM { get; set; } = "";
    /// <summary>7 dígitos, opcional.</summary>
    public string? CEST { get; set; }
    public string Categoria { get; set; } = "";
    public string Unidade { get; set; } = "UN";
    public string? Sistema { get; set; }
    public decimal Peso { get; set; }
    public int GarantiaDias { get; set; } = 90;
    /// <summary>Custo de compra unitário.</summary>
    public decimal CustoUnitario { get; set; }
    /// <summary>Margem em % sobre o custo (ex.: 30 = 30%). Se omitida, usa o padrão configurado pro Sistema.</summary>
    public decimal? MargemLucroPct { get; set; }
    /// <summary>Quantidade já em estoque (entrada inicial) — 0 se omitida.</summary>
    public int QuantidadeEstoque { get; set; }
    /// <summary>Quantidade mínima antes de disparar alerta de estoque baixo.</summary>
    public int QuantidadeMinima { get; set; } = 5;
}

/// <summary>Cenário: "emPreparacao" (padrão, nada feito) ou "disponivel" (liberado pra venda).</summary>
public class VeiculoVendaImportDTO
{
    public string Chave { get; set; } = "";
    public string Marca { get; set; } = "";
    public string Modelo { get; set; } = "";
    public string Cor { get; set; } = "";
    public string Motorizacao { get; set; } = "";
    public int Ano { get; set; }
    public int Quilometragem { get; set; }
    public string Placa { get; set; } = "";
    public string Renavam { get; set; } = "";
    public string Cambio { get; set; } = "";
    public string Combustivel { get; set; } = "";
    public decimal Valor { get; set; }
    /// <summary>Quanto a concessionária pagou para adquirir o veículo — obrigatório.</summary>
    public decimal ValorAquisicao { get; set; }
    public List<string> Acessorios { get; set; } = new();
    /// <summary>Ano do último IPVA pago — obrigatório.</summary>
    public int AnoUltimoIpvaPago { get; set; }
    public string TextoTermoPreliminar { get; set; } = "";
    public DateTime? DataCriacao { get; set; }
    public string Cenario { get; set; } = "emPreparacao";
}

/// <summary>Cenário: "ativa" (padrão), "vendidaAguardando", "concluida", "devolvida" ou "cancelada".</summary>
public class VeiculoConsignacaoImportDTO
{
    public string Chave { get; set; } = "";
    public string Marca { get; set; } = "";
    public string Modelo { get; set; } = "";
    public string Cor { get; set; } = "";
    public string Motorizacao { get; set; } = "";
    public int Ano { get; set; }
    public int Quilometragem { get; set; }
    public string Placa { get; set; } = "";
    public string Renavam { get; set; } = "";
    public string Cambio { get; set; } = "";
    public string Combustivel { get; set; } = "";
    public List<string> Acessorios { get; set; } = new();
    public string ClienteProprietarioChave { get; set; } = "";
    public string VendedorResponsavelChave { get; set; } = "";
    /// <summary>"Fixo" ou "Porcentagem".</summary>
    public string TipoComissao { get; set; } = "Porcentagem";
    public decimal ValorVendaEsperado { get; set; }
    public decimal? ValorFixoProprietario { get; set; }
    public decimal? PorcentagemProprietario { get; set; }
    public string TextoContrato { get; set; } = "";
    public int PrazoDias { get; set; } = 90;
    public DateTime? DataCriacao { get; set; }
    /// <summary>Cenário: "ativa" (padrão), "vendidaAguardando", "concluida", "devolvida" ou "cancelada".</summary>
    public string Cenario { get; set; } = "ativa";
    /// <summary>Usado só quando Cenario = "cancelada".</summary>
    public string? MotivoCancelamento { get; set; }
}

public class VeiculoClienteImportDTO
{
    public string Chave { get; set; } = "";
    public string ClienteChave { get; set; } = "";
    public string Marca { get; set; } = "";
    public string Modelo { get; set; } = "";
    public string Cor { get; set; } = "";
    public int Ano { get; set; }
    public string Placa { get; set; } = "";
}

/// <summary>
/// Cenário (cada um implica os anteriores no funil real da proposta):
/// "criada", "rejeitada", "aprovada", "vistoriada", "termoRedigido",
/// "termoEnviado", "concluidaAVista", "concluidaFinanciada".
/// </summary>
public class PropostaVendaImportDTO
{
    public string Chave { get; set; } = "";
    public string VeiculoVendaChave { get; set; } = "";
    public string ClienteChave { get; set; } = "";
    public string VendedorChave { get; set; } = "";
    public decimal ValorBase { get; set; }
    public decimal DescontoPercentual { get; set; }
    /// <summary>Pix, Transferencia ou Boleto — ignorado se o cenário for "concluidaFinanciada" (compra de veículo não aceita Dinheiro/cartão).</summary>
    public string? ModoPagamento { get; set; }
    public int ParcelasFinanciamento { get; set; } = 36;
    public decimal ValorParcelaFinanciamento { get; set; } = 1500;
    public decimal TaxaJurosMensalFinanciamento { get; set; } = 1.99m;
    public string? MotivoRejeicao { get; set; }
    public DateTime? DataCriacao { get; set; }
    public DateTime? DataAprovacao { get; set; }
    public string Cenario { get; set; } = "criada";
}

/// <summary>
/// Cenário: "pendente" (padrão, recém-aberta), "cancelada", "emAndamento"
/// (iniciada, mecânico trabalhando), "finalizadaPendente" (serviço pronto,
/// cobrança em aberto), "finalizadaPaga" (paga, aguardando retirada) ou
/// "entregue" (paga E retirada pelo cliente — conclusão real do fluxo).
/// </summary>
public class OrdemServicoImportDTO
{
    public string Chave { get; set; } = "";
    public string VeiculoClienteChave { get; set; } = "";
    public string ClienteChave { get; set; } = "";
    public string MecanicoChave { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Descricao { get; set; } = "";
    /// <summary>Dias de prazo a partir da criação da OS.</summary>
    public int PrazoDiasAPartirDaCriacao { get; set; } = 7;
    public decimal CustoServico { get; set; }
    public string? ModoPagamento { get; set; }
    public DateTime? DataCriacao { get; set; }
    public string Cenario { get; set; } = "pendente";
    /// <summary>Opcional — chave de um preset já cadastrado na seção ChecklistPresets.</summary>
    public string? ChecklistPresetChave { get; set; }
    /// <summary>Componentes usados na OS, com sua origem (Estoque, Cliente ou Encomenda).</summary>
    public List<ItemOrdemServicoImportDTO> Itens { get; set; } = new();
}

/// <summary>Origem: "Estoque" (peça já cadastrada, padrão), "Cliente" (cliente trouxe a peça) ou "Encomenda" (oficina precisou comprar — entra via fluxo de requisição de peça atendida).</summary>
public class ItemOrdemServicoImportDTO
{
    public string ComponenteChave { get; set; } = "";
    public int Quantidade { get; set; } = 1;
    public string Origem { get; set; } = "Estoque";
}
