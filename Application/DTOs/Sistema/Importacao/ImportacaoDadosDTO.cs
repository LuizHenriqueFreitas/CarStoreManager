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
///   1. Usuarios         (vendedores/mecânicos/recepcionistas — referenciados por quase tudo)
///   2. Clientes
///   3. Fornecedores                                 (referenciado por Componentes)
///   4. Despesas (modelo) — ANTES de tudo que lança despesa automática, pra
///      todo balanço mensal gerado durante a importação já nascer com as
///      linhas do modelo (salário, aluguel, contas)
///   5. Componentes (+ reposições de estoque históricas)
///   6. ComponentesEquivalentes (doc 35)
///   7. ChecklistPresets / TemplatesDocumento
///   8. VeiculosVenda (compra lança despesa, movida pra competência da DataCriacao)
///   9. VeiculosConsignados — criados Ativos; status final + datas do
///      histórico aplicados só depois dos TestDrives (test drive de
///      consignado exige consignação Ativa)
///   10. VeiculosCliente
///   11. TestDrives     (ANTES de PropostasVenda — precisa do veículo ainda
///                        "Disponivel"; proposta concluída marca como Vendido)
///   12. PropostasVenda
///   13. OrdensServico
///   14. DespesasExtras — itens extras num mês específico (vários por mês,
///       qualquer setor/categoria: investimento, manutenção, perda...)
///   15. FechamentosMensais — sempre por último (mês fechado rejeita item novo)
///
/// Datas secundárias (pagamentos, vistorias, termos, assinaturas, alertas,
/// requisições, histórico de consignação, despesas automáticas) vão pra
/// linha do tempo do registro — campos do arquivo têm prioridade; o que
/// faltar é derivado da etapa anterior, nunca "hoje".
/// </summary>
public class ImportacaoDadosDTO
{
    public List<UsuarioImportDTO> Usuarios { get; set; } = new();
    public List<ClienteImportDTO> Clientes { get; set; } = new();
    public List<FornecedorImportDTO> Fornecedores { get; set; } = new();
    public List<ComponenteImportDTO> Componentes { get; set; } = new();
    /// <summary>Componentes compatíveis (doc 35) — processado logo após Componentes.</summary>
    public List<ComponenteEquivalenteImportDTO> ComponentesEquivalentes { get; set; } = new();
    public List<ChecklistPresetImportDTO> ChecklistPresets { get; set; } = new();
    public List<TemplateDocumentoImportDTO> TemplatesDocumento { get; set; } = new();
    public List<DespesaImportDTO> Despesas { get; set; } = new();
    public List<VeiculoVendaImportDTO> VeiculosVenda { get; set; } = new();
    public List<VeiculoConsignacaoImportDTO> VeiculosConsignados { get; set; } = new();
    public List<VeiculoClienteImportDTO> VeiculosCliente { get; set; } = new();
    public List<PropostaVendaImportDTO> PropostasVenda { get; set; } = new();
    public List<OrdemServicoImportDTO> OrdensServico { get; set; } = new();
    public List<TestDriveImportDTO> TestDrives { get; set; } = new();
    public List<DespesaExtraImportDTO> DespesasExtras { get; set; } = new();
    /// <summary>
    /// Fechamentos de mês (variação dos itens do modelo + fechamento com data
    /// histórica) — ÚLTIMA etapa da importação, depois de todas as despesas
    /// automáticas já estarem na competência certa (balanço fechado rejeita item).
    /// </summary>
    public List<FechamentoMensalImportDTO> FechamentosMensais { get; set; } = new();

    /// <summary>
    /// true quando o arquivo já traz TODAS as despesas lançadas em cada
    /// competência (é o que o exportador gera): despesasExtras inclui as
    /// automáticas (compra de veículo/componente, combustível de test drive)
    /// e fechamentosMensais traz o valor/ausência de cada item do modelo. Aí
    /// o importador DESCARTA as despesas que as regras de negócio lançam
    /// sozinhas durante a importação, em vez de movê-las de competência —
    /// senão elas contariam 2x. Padrão false (arquivo "roteiro" do gerador).
    /// </summary>
    public bool DespesasAutomaticasNoArquivo { get; set; }
}

/// <summary>Item de checklist explícito da OS (alternativa ao preset — é o que o exportador gera).</summary>
public class ChecklistItemImportDTO
{
    public string Descricao { get; set; } = "";
    public bool Concluido { get; set; }
}

/// <summary>Ligação de equivalência entre dois componentes já importados (doc 35).</summary>
public class ComponenteEquivalenteImportDTO
{
    public string ComponenteChave { get; set; } = "";
    public string EquivalenteChave { get; set; } = "";
    /// <summary>Similar (padrão), Paralela, Original ou Remanufaturada.</summary>
    public string? TipoEquivalencia { get; set; }
    /// <summary>Aceito por compatibilidade com o contrato, mas a entidade não guarda observação — ignorado.</summary>
    public string? Observacao { get; set; }
}

/// <summary>
/// Fechamento de uma competência: garante o balanço gerado do modelo, troca o
/// valor dos itens do modelo listados em <see cref="Variacoes"/> (conta de luz
/// mais cara num mês etc.) e, se <see cref="Fechar"/>, fecha o mês com
/// <see cref="DataFechamento"/> histórica.
/// </summary>
public class FechamentoMensalImportDTO
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public bool Fechar { get; set; }
    public DateTime? DataFechamento { get; set; }
    public List<VariacaoDespesaImportDTO> Variacoes { get; set; } = new();
    /// <summary>Itens do modelo que NÃO existiam nesse mês (modelo mudou depois) — removidos do balanço gerado.</summary>
    public List<string>? RemoverItensModelo { get; set; }
}

public class VariacaoDespesaImportDTO
{
    /// <summary>Nome exato do item do modelo (Despesas) nessa competência.</summary>
    public string Nome { get; set; } = "";
    public decimal Valor { get; set; }
}

/// <summary>
/// Pagamento de proposta/OS: valor = total × Percentual/100 (2 casas); quando
/// o cenário quita o total, o ÚLTIMO pagamento recebe o saldo restante exato.
/// </summary>
public class PagamentoImportDTO
{
    public string Modo { get; set; } = "Pix";
    public decimal Percentual { get; set; }
    public DateTime? Data { get; set; }
}

/// <summary>Alerta de problema adicional emitido pelo mecânico durante o serviço.</summary>
public class AlertaImportDTO
{
    public string Descricao { get; set; } = "";
    /// <summary>"aprovado" (padrão), "recusado" ou "pendente" (OS fica Pausada).</summary>
    public string Decisao { get; set; } = "aprovado";
    public string? ObservacaoCliente { get; set; }
    public DateTime? Data { get; set; }
    public DateTime? DataDecisao { get; set; }
}

/// <summary>Requisição de peça aberta durante o orçamento e rejeitada pelo admin.</summary>
public class RequisicaoRejeitadaImportDTO
{
    public string DescricaoPeca { get; set; } = "";
    public string Motivo { get; set; } = "";
    public DateTime? Data { get; set; }
}

/// <summary>Despesa mensal fixa (recorrente) — sem data própria, é um valor "atual" cadastrado, não um lançamento pontual.</summary>
public class DespesaImportDTO
{
    public string Nome { get; set; } = "";
    public decimal Valor { get; set; }
    public string? Categoria { get; set; }
    /// <summary>false = despesa-modelo desativada (fica cadastrada, mas não entra em balanço novo).</summary>
    public bool Ativa { get; set; } = true;
    /// <summary>Geral, Oficina ou Concessionaria.</summary>
    public string Setor { get; set; } = "Geral";
    /// <summary>Salario, Aluguel, Utilidades, Manutencao, Marketing, Impostos, Seguro, Investimento, Servicos ou Outros.</summary>
    public string Tipo { get; set; } = "Outros";
}

/// <summary>
/// Lançamento pontual num mês específico do histórico — usado pra variação
/// (compra de equipamento, manutenção fora do comum, perda de um veículo
/// próprio com motor fundido etc.), somado às despesas recorrentes do
/// modelo naquela competência (não as substitui). "Data" só usa Ano/Mês.
/// </summary>
public class DespesaExtraImportDTO
{
    public DateTime Data { get; set; }
    public string Nome { get; set; } = "";
    public decimal Valor { get; set; }
    /// <summary>Geral, Oficina ou Concessionaria.</summary>
    public string Setor { get; set; } = "Geral";
    public string? Categoria { get; set; }
}

/// <summary>Modelo de checklist reutilizável entre OS (ex.: "Revisão Geral", "Troca de Óleo").</summary>
public class ChecklistPresetImportDTO
{
    public string Chave { get; set; } = "";
    public string Nome { get; set; } = "";
    public List<string> Itens { get; set; } = new();
}

/// <summary>Template de documento/contrato reutilizável (termo de entrega, contrato de consignação, resposta de financiadora etc.) — ver TemplateDocumento.</summary>
public class TemplateDocumentoImportDTO
{
    public string Chave { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Conteudo { get; set; } = "";
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
    /// <summary>
    /// Quantidade já em estoque (entrada inicial), lançada como uma única
    /// despesa "hoje" — 0 se omitida. Ignorado quando <see cref="ReposicoesEstoque"/>
    /// vem preenchido (uma vale a outra, nunca as duas).
    /// </summary>
    public int QuantidadeEstoque { get; set; }
    /// <summary>
    /// Reposições históricas de estoque — cada uma vira sua própria entrada
    /// (e sua própria despesa "Compra de componentes", movida pra
    /// competência de <c>Data</c>) em vez de uma única entrada "hoje" com a
    /// quantidade toda. Preferível a <see cref="QuantidadeEstoque"/> quando
    /// se quer espalhar o custo de compra pelo histórico, como já acontece
    /// com veículo.
    /// </summary>
    public List<ReposicaoEstoqueImportDTO>? ReposicoesEstoque { get; set; }
    /// <summary>Quantidade mínima antes de disparar alerta de estoque baixo.</summary>
    public int QuantidadeMinima { get; set; } = 5;
    /// <summary>
    /// true = estoque que já existia antes de a loja adotar o sistema: a
    /// despesa automática "Compra de componente" de cada entrada é REMOVIDA
    /// (não vai pra competência nenhuma). Padrão false.
    /// </summary>
    public bool EstoqueInicial { get; set; }
}

public class ReposicaoEstoqueImportDTO
{
    public DateTime Data { get; set; }
    public int Quantidade { get; set; }
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
    /// <summary>
    /// true = veículo que já estava no pátio antes de a loja adotar o
    /// sistema: a despesa automática "Compra de veículo para concessionária"
    /// é REMOVIDA (não vai pra competência nenhuma). Padrão false.
    /// </summary>
    public bool EstoqueInicial { get; set; }
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
    /// <summary>Datas dos eventos do histórico (vendidaAguardando/concluida → DataVenda; concluida → DataConclusao; devolvida; cancelada).</summary>
    public DateTime? DataVenda { get; set; }
    public DateTime? DataConclusao { get; set; }
    public DateTime? DataDevolucao { get; set; }
    public DateTime? DataCancelamento { get; set; }
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
/// "criada", "rejeitada", "financiamentoNegado" (financiadora recusou),
/// "aprovada", "cancelada" (aprovada → cancelada com motivo), "vistoriada",
/// "pagamentoParcial" (vistoriada + pagamentos que não quitam),
/// "termoRedigido", "termoEnviado", "concluidaAVista", "concluidaFinanciada".
/// Estágios intermediários (usados pelo exportador): "aguardandoFinanciadora"
/// (financiamento solicitado), "respostaFinanciadora" (resposta registrada,
/// cliente ainda não aprovou), "aguardandoVistoria" (vistoria iniciada).
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
    /// <summary>Texto livre anotado pelo vendedor com o que a financiadora propôs — usado quando Cenario = "concluidaFinanciada".</summary>
    public string? TextoPropostaFinanciadora { get; set; }
    public string? MotivoRejeicao { get; set; }
    public DateTime? DataCriacao { get; set; }
    public DateTime? DataAprovacao { get; set; }
    public string Cenario { get; set; } = "criada";
    /// <summary>Usado quando Cenario = "cancelada".</summary>
    public string? MotivoCancelamento { get; set; }
    /// <summary>Entrada da proposta financiada (DefinirEntradaAsync) — antes de solicitar o financiamento.</summary>
    public decimal? ValorEntrada { get; set; }
    /// <summary>Se ausente: 1 pagamento integral no modo da proposta (comportamento antigo).</summary>
    public List<PagamentoImportDTO>? Pagamentos { get; set; }
    public DateTime? DataVistoria { get; set; }
    public DateTime? DataTermo { get; set; }
    public DateTime? DataAssinatura { get; set; }
    /// <summary>Alternativa a VeiculoVendaChave — proposta de veículo consignado.</summary>
    public string? VeiculoConsignadoChave { get; set; }
    /// <summary>Observações da vistoria (padrão: "Veículo vistoriado, sem avarias relevantes.").</summary>
    public string? ObservacoesVistoria { get; set; }
    /// <summary>Texto do termo de entrega; ausente ⇒ rascunho do veículo/template preenchido.</summary>
    public string? TextoTermo { get; set; }
    /// <summary>Quem assinou o termo; ausente ⇒ nome/CPF do cliente da proposta.</summary>
    public string? AssinaturaNome { get; set; }
    public string? AssinaturaCpf { get; set; }
}

/// <summary>
/// Cenário: "pendente" (padrão, recém-aberta), "cancelada" (cancelada ainda
/// pendente, antes de iniciar), "canceladaEmAndamento" (iniciada e depois
/// cancelada — simula desistência no meio do serviço), "emAndamento"
/// (iniciada, mecânico trabalhando), "finalizadaPendente" (checklist
/// concluída, pronta pra finalizar, mas ainda em EmAndamento — cobrança tem
/// que acontecer antes de finalizar, ver <see cref="OrdemServicoImportDTO"/>
/// abaixo e OrdemServicoService.FinalizarAsync), "finalizadaPaga" (paga,
/// aguardando retirada) ou "entregue" (paga E retirada pelo cliente —
/// conclusão real do fluxo). Novos (fluxo de vistoria, doc 32 — exigem
/// RecepcionistaChave): "emVistoria", "aguardandoCliente",
/// "orcamentoRecusado" (vistoria concluída → cliente recusou → Cancelada),
/// "aprovada". Também: "aguardandoPeca" (requisição aberta pendente →
/// BuscandoPecasParaOrcamento) e "pausada" (alerta emitido, sem decisão).
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
    /// <summary>
    /// Se true (e o cenário chegar a "emAndamento" ou além), simula o
    /// mecânico encontrando um problema novo durante o serviço: emite um
    /// alerta (pausa a OS) e já registra a aprovação do cliente pra o
    /// aumento de escopo, retomando o serviço — ver IAlertaOSService.
    /// </summary>
    public bool ComAlerta { get; set; }

    /// <summary>Presente ⇒ a OS passa pela vistoria de entrada (doc 32) antes de iniciar.</summary>
    public string? RecepcionistaChave { get; set; }
    /// <summary>Texto do contrato da vistoria; ausente ⇒ template "Contrato de OS" preenchido com os dados da OS.</summary>
    public string? TextoVistoria { get; set; }
    public AlertaImportDTO? Alerta { get; set; }
    public RequisicaoRejeitadaImportDTO? RequisicaoRejeitada { get; set; }
    /// <summary>Se ausente: 1 pagamento integral (comportamento antigo) nos cenários pagos.</summary>
    public List<PagamentoImportDTO>? Pagamentos { get; set; }
    public DateTime? DataVistoria { get; set; }
    public DateTime? DataAprovacaoCliente { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public DateTime? DataEntrega { get; set; }
    /// <summary>Checklist explícito (sem preset): itens criados na abertura; "concluido" marcado quando a OS fica em andamento.</summary>
    public List<ChecklistItemImportDTO>? Checklist { get; set; }
}

/// <summary>Cenário: "agendado" (padrão), "realizado", "cancelado", "naoCompareceu" ou "reagendado".</summary>
public class TestDriveImportDTO
{
    public string Chave { get; set; } = "";
    public string VeiculoVendaChave { get; set; } = "";
    public string ClienteChave { get; set; } = "";
    public string VendedorChave { get; set; } = "";
    public DateTime DataHora { get; set; }
    public string? Observacao { get; set; }
    public string Cenario { get; set; } = "agendado";
    /// <summary>Alternativa a VeiculoVendaChave — test drive de veículo consignado (doc 31).</summary>
    public string? VeiculoConsignadoChave { get; set; }
    /// <summary>"assinado", "enviado" ou "rascunho" — padrão: assinado se realizado, rascunho nos demais.</summary>
    public string? Termo { get; set; }
    /// <summary>Cenário "reagendado": data/hora ORIGINAL do agendamento (DataHora é a nova, após reagendar).</summary>
    public DateTime? DataReagendamentoOriginal { get; set; }
}

/// <summary>Origem: "Estoque" (peça já cadastrada, padrão), "Cliente" (cliente trouxe a peça) ou "Encomenda" (oficina precisou comprar — entra via fluxo de requisição de peça atendida).</summary>
public class ItemOrdemServicoImportDTO
{
    /// <summary>Opcional só quando Origem = "Cliente" e DescricaoLivre vier preenchida.</summary>
    public string ComponenteChave { get; set; } = "";
    public int Quantidade { get; set; } = 1;
    public string Origem { get; set; } = "Estoque";
    /// <summary>Nome da peça trazida pelo cliente (sem cadastro no catálogo).</summary>
    public string? DescricaoLivre { get; set; }
    /// <summary>Preço unitário histórico; ausente ⇒ preço de venda atual do componente.</summary>
    public decimal? ValorUnitario { get; set; }
}
