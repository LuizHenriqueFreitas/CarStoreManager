using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarStoreManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertasOS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrdemServicoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MecanicoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataResolucao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolvidoPor = table.Column<Guid>(type: "TEXT", nullable: true),
                    ObservacaoCliente = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertasOS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnunciosMercadoLivre",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntidadeTipo = table.Column<string>(type: "TEXT", nullable: false),
                    EntidadeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemIdML = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    UltimoPrecoSincronizado = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DataUltimaSincronizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UltimoErro = table.Column<string>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnunciosMercadoLivre", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistPresets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataUltimaAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistPresets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", nullable: false),
                    CPF = table.Column<string>(type: "TEXT", nullable: false),
                    EnderecoLogradouro = table.Column<string>(type: "TEXT", nullable: false),
                    EnderecoNumero = table.Column<string>(type: "TEXT", nullable: false),
                    EnderecoComplemento = table.Column<string>(type: "TEXT", nullable: true),
                    EnderecoBairro = table.Column<string>(type: "TEXT", nullable: false),
                    EnderecoCidade = table.Column<string>(type: "TEXT", nullable: false),
                    EnderecoUf = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    EnderecoCep = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Componentes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SKUInterno = table.Column<string>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    MarcaFabricante = table.Column<string>(type: "TEXT", nullable: false),
                    PartNumber = table.Column<string>(type: "TEXT", nullable: false),
                    CodigoOEM = table.Column<string>(type: "TEXT", nullable: false),
                    CodigoBarras = table.Column<string>(type: "TEXT", nullable: false),
                    NCM = table.Column<string>(type: "TEXT", nullable: false),
                    CEST = table.Column<string>(type: "TEXT", nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", nullable: false),
                    Unidade = table.Column<string>(type: "TEXT", nullable: false),
                    Peso = table.Column<decimal>(type: "TEXT", nullable: false),
                    GarantiaDias = table.Column<int>(type: "INTEGER", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Sistema = table.Column<int>(type: "INTEGER", nullable: true),
                    CustoUnitario = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    MargemLucroPct = table.Column<decimal>(type: "TEXT", precision: 7, scale: 4, nullable: true),
                    ValorVenda = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Componentes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracoesMercadoLivre",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Conectado = table.Column<bool>(type: "INTEGER", nullable: false),
                    MercadoLivreUserId = table.Column<string>(type: "TEXT", nullable: true),
                    EmailContaConectada = table.Column<string>(type: "TEXT", nullable: true),
                    AccessTokenCriptografado = table.Column<string>(type: "TEXT", nullable: true),
                    RefreshTokenCriptografado = table.Column<string>(type: "TEXT", nullable: true),
                    DataExpiracaoToken = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SegredoWebhook = table.Column<string>(type: "TEXT", nullable: true),
                    DataConexao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesMercadoLivre", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracoesSistema",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MargensPorSistemaJson = table.Column<string>(type: "TEXT", nullable: false),
                    MargemPadraoGlobalPct = table.Column<decimal>(type: "TEXT", precision: 7, scale: 4, nullable: false),
                    ExigirEntradaMinima = table.Column<bool>(type: "INTEGER", nullable: false),
                    PercentualEntradaMinima = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    DataUltimaAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesSistema", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Despesas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Valor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Ativa = table.Column<bool>(type: "INTEGER", nullable: false),
                    Setor = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Geral"),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Outros"),
                    DataUltimaAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Despesas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PagamentosOrdemServico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrdemServicoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModoPagamento = table.Column<int>(type: "INTEGER", nullable: false),
                    Valor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RecebidoPor = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReferenciaExterna = table.Column<string>(type: "TEXT", nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagamentosOrdemServico", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PagamentosProposta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropostaVendaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModoPagamento = table.Column<int>(type: "INTEGER", nullable: false),
                    Valor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RecebidoPor = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReferenciaExterna = table.Column<string>(type: "TEXT", nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagamentosProposta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropostasVenda",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VendedorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VeiculoVendaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClienteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ValorBase = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DescontoPercentual = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorFinal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Entrada = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ModoPagamento = table.Column<int>(type: "INTEGER", nullable: false),
                    DataSolicitacaoFinanciamento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataRespostaFinanciadora = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ParcelasFinanciamento = table.Column<int>(type: "INTEGER", nullable: true),
                    ValorParcela = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    TaxaJurosMensal = table.Column<decimal>(type: "TEXT", precision: 7, scale: 4, nullable: true),
                    ObservacoesFinanciamento = table.Column<string>(type: "TEXT", nullable: true),
                    MotivoRejeicao = table.Column<string>(type: "TEXT", nullable: true),
                    MotivoCancelamento = table.Column<string>(type: "TEXT", nullable: true),
                    DataAprovacao = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropostasVenda", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequisicoesPeca",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrdemServicoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MecanicoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescricaoPeca = table.Column<string>(type: "TEXT", nullable: false),
                    Justificativa = table.Column<string>(type: "TEXT", nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataResolucao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolvidaPor = table.Column<Guid>(type: "TEXT", nullable: true),
                    ObservacaoAdmin = table.Column<string>(type: "TEXT", nullable: true),
                    ComponenteAtendidoId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequisicoesPeca", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TermosEntrega",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropostaVendaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TextoTermo = table.Column<string>(type: "TEXT", nullable: false),
                    AdminRedatorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DataRedacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataUltimaEdicao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TokenAssinatura = table.Column<string>(type: "TEXT", nullable: true),
                    DataAssinatura = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AssinaturaNomeCliente = table.Column<string>(type: "TEXT", nullable: true),
                    AssinaturaCpfCliente = table.Column<string>(type: "TEXT", nullable: true),
                    AssinaturaIp = table.Column<string>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermosEntrega", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", nullable: false),
                    SenhaHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TipoUsuario = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
                    Nivel = table.Column<int>(type: "INTEGER", nullable: true),
                    DataContratacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Especialidade = table.Column<int>(type: "INTEGER", nullable: true),
                    Ocupado = table.Column<int>(type: "INTEGER", nullable: true),
                    TrabalhosAtivos = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VeiculosCliente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClienteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Marca = table.Column<string>(type: "TEXT", nullable: false),
                    Modelo = table.Column<string>(type: "TEXT", nullable: false),
                    Cor = table.Column<string>(type: "TEXT", nullable: false),
                    Ano = table.Column<int>(type: "INTEGER", nullable: false),
                    Placa = table.Column<string>(type: "TEXT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VeiculosCliente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VeiculosConsignacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Marca = table.Column<string>(type: "TEXT", nullable: false),
                    Modelo = table.Column<string>(type: "TEXT", nullable: false),
                    Cor = table.Column<string>(type: "TEXT", nullable: false),
                    Motorizacao = table.Column<string>(type: "TEXT", nullable: false),
                    Ano = table.Column<int>(type: "INTEGER", nullable: false),
                    Quilometragem = table.Column<int>(type: "INTEGER", nullable: false),
                    Placa = table.Column<string>(type: "TEXT", nullable: false),
                    Renavam = table.Column<string>(type: "TEXT", nullable: false),
                    Cambio = table.Column<int>(type: "INTEGER", nullable: false),
                    Combustivel = table.Column<int>(type: "INTEGER", nullable: false),
                    Acessorios = table.Column<int>(type: "INTEGER", nullable: false),
                    ClienteProprietarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VendedorResponsavelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ComissaoTipo = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorVendaEsperado = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ComissaoValorFixoProprietario = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    ComissaoPorcentagemProprietario = table.Column<decimal>(type: "TEXT", nullable: true),
                    TextoContrato = table.Column<string>(type: "TEXT", nullable: false),
                    UrlContratoPdf = table.Column<string>(type: "TEXT", nullable: true),
                    DataInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VeiculosConsignacao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VeiculosVenda",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Marca = table.Column<string>(type: "TEXT", nullable: false),
                    Modelo = table.Column<string>(type: "TEXT", nullable: false),
                    Cor = table.Column<string>(type: "TEXT", nullable: false),
                    Motorizacao = table.Column<string>(type: "TEXT", nullable: false),
                    Ano = table.Column<int>(type: "INTEGER", nullable: false),
                    Quilometragem = table.Column<int>(type: "INTEGER", nullable: false),
                    Placa = table.Column<string>(type: "TEXT", nullable: false),
                    Renavam = table.Column<string>(type: "TEXT", nullable: false),
                    AnoUltimoIpvaPago = table.Column<int>(type: "INTEGER", nullable: true),
                    Cambio = table.Column<int>(type: "INTEGER", nullable: false),
                    Combustivel = table.Column<int>(type: "INTEGER", nullable: false),
                    Disponibilidade = table.Column<int>(type: "INTEGER", nullable: false),
                    Acessorios = table.Column<int>(type: "INTEGER", nullable: false),
                    Valor = table.Column<decimal>(type: "TEXT", nullable: false),
                    TextoTermoPreliminar = table.Column<string>(type: "TEXT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VeiculosVenda", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VendasMercadoLivre",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AnuncioMercadoLivreId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IdPedidoPlataforma = table.Column<string>(type: "TEXT", nullable: false),
                    IdItemPlataforma = table.Column<string>(type: "TEXT", nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DataVenda = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StatusSincronizacao = table.Column<int>(type: "INTEGER", nullable: false),
                    ErroSincronizacao = table.Column<string>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendasMercadoLivre", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vistorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropostaVendaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DataRealizada = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VistoriadorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: false),
                    Aprovado = table.Column<bool>(type: "INTEGER", nullable: false),
                    Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vistorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistPresetItens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChecklistPresetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    Ordem = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistPresetItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistPresetItens_ChecklistPresets_ChecklistPresetId",
                        column: x => x.ChecklistPresetId,
                        principalTable: "ChecklistPresets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComponenteEquivalente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ComponenteOriginalId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ComponenteEquivalenteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TipoEquivalencia = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponenteEquivalente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponenteEquivalente_Componentes_ComponenteEquivalenteId",
                        column: x => x.ComponenteEquivalenteId,
                        principalTable: "Componentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponenteEquivalente_Componentes_ComponenteOriginalId",
                        column: x => x.ComponenteOriginalId,
                        principalTable: "Componentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EstoqueComponentes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PecaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuantidadeAtual = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantidadeMinima = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstoqueComponentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstoqueComponentes_Componentes_PecaId",
                        column: x => x.PecaId,
                        principalTable: "Componentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrdensServico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VeiculoClienteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MecanicoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClienteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    NumeroPublico = table.Column<string>(type: "TEXT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PrazoEstimado = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CustoServico = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusAnterior = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdensServico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdensServico_VeiculosCliente_VeiculoClienteId",
                        column: x => x.VeiculoClienteId,
                        principalTable: "VeiculosCliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "HistoricosConsignacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VeiculoConsignacaoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TipoEvento = table.Column<int>(type: "INTEGER", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    UsuarioResponsavelId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricosConsignacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricosConsignacao_VeiculosConsignacao_VeiculoConsignacaoId",
                        column: x => x.VeiculoConsignacaoId,
                        principalTable: "VeiculosConsignacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntidadeTipo = table.Column<string>(type: "TEXT", nullable: false),
                    EntidadeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VeiculoVendaId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    NomeArquivo = table.Column<string>(type: "TEXT", nullable: false),
                    TamanhoBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", nullable: false),
                    DataUpload = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ordem = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fotos_VeiculosVenda_VeiculoVendaId",
                        column: x => x.VeiculoVendaId,
                        principalTable: "VeiculosVenda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistItens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrdemServicoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", nullable: true),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Origem = table.Column<int>(type: "INTEGER", nullable: false),
                    OrdemExibicao = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistItens_OrdensServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalTable: "OrdensServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItensOrdemServico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ComponenteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrdemServicoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorTotalItem = table.Column<decimal>(type: "TEXT", nullable: false),
                    Origem = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusItem = table.Column<int>(type: "INTEGER", nullable: false),
                    DataRecebimento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensOrdemServico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensOrdemServico_OrdensServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalTable: "OrdensServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertasOS_OrdemServicoId",
                table: "AlertasOS",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasOS_Status",
                table: "AlertasOS",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AnunciosMercadoLivre_EntidadeTipo_EntidadeId",
                table: "AnunciosMercadoLivre",
                columns: new[] { "EntidadeTipo", "EntidadeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnunciosMercadoLivre_ItemIdML",
                table: "AnunciosMercadoLivre",
                column: "ItemIdML");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItens_OrdemServicoId",
                table: "ChecklistItens",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistPresetItens_ChecklistPresetId",
                table: "ChecklistPresetItens",
                column: "ChecklistPresetId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponenteEquivalente_ComponenteEquivalenteId",
                table: "ComponenteEquivalente",
                column: "ComponenteEquivalenteId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponenteEquivalente_ComponenteOriginalId",
                table: "ComponenteEquivalente",
                column: "ComponenteOriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_EstoqueComponentes_PecaId",
                table: "EstoqueComponentes",
                column: "PecaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fotos_EntidadeTipo_EntidadeId",
                table: "Fotos",
                columns: new[] { "EntidadeTipo", "EntidadeId" });

            migrationBuilder.CreateIndex(
                name: "IX_Fotos_VeiculoVendaId",
                table: "Fotos",
                column: "VeiculoVendaId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosConsignacao_VeiculoConsignacaoId",
                table: "HistoricosConsignacao",
                column: "VeiculoConsignacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensOrdemServico_OrdemServicoId",
                table: "ItensOrdemServico",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensServico_NumeroPublico",
                table: "OrdensServico",
                column: "NumeroPublico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdensServico_VeiculoClienteId",
                table: "OrdensServico",
                column: "VeiculoClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PagamentosOrdemServico_OrdemServicoId",
                table: "PagamentosOrdemServico",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_PagamentosProposta_PropostaVendaId",
                table: "PagamentosProposta",
                column: "PropostaVendaId");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicoesPeca_OrdemServicoId",
                table: "RequisicoesPeca",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicoesPeca_Status",
                table: "RequisicoesPeca",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TermosEntrega_PropostaVendaId",
                table: "TermosEntrega",
                column: "PropostaVendaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TermosEntrega_TokenAssinatura",
                table: "TermosEntrega",
                column: "TokenAssinatura");

            migrationBuilder.CreateIndex(
                name: "IX_VendasMercadoLivre_IdPedidoPlataforma_IdItemPlataforma",
                table: "VendasMercadoLivre",
                columns: new[] { "IdPedidoPlataforma", "IdItemPlataforma" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vistorias_PropostaVendaId",
                table: "Vistorias",
                column: "PropostaVendaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertasOS");

            migrationBuilder.DropTable(
                name: "AnunciosMercadoLivre");

            migrationBuilder.DropTable(
                name: "ChecklistItens");

            migrationBuilder.DropTable(
                name: "ChecklistPresetItens");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "ComponenteEquivalente");

            migrationBuilder.DropTable(
                name: "ConfiguracoesMercadoLivre");

            migrationBuilder.DropTable(
                name: "ConfiguracoesSistema");

            migrationBuilder.DropTable(
                name: "Despesas");

            migrationBuilder.DropTable(
                name: "EstoqueComponentes");

            migrationBuilder.DropTable(
                name: "Fotos");

            migrationBuilder.DropTable(
                name: "HistoricosConsignacao");

            migrationBuilder.DropTable(
                name: "ItensOrdemServico");

            migrationBuilder.DropTable(
                name: "PagamentosOrdemServico");

            migrationBuilder.DropTable(
                name: "PagamentosProposta");

            migrationBuilder.DropTable(
                name: "PropostasVenda");

            migrationBuilder.DropTable(
                name: "RequisicoesPeca");

            migrationBuilder.DropTable(
                name: "TermosEntrega");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "VendasMercadoLivre");

            migrationBuilder.DropTable(
                name: "Vistorias");

            migrationBuilder.DropTable(
                name: "ChecklistPresets");

            migrationBuilder.DropTable(
                name: "Componentes");

            migrationBuilder.DropTable(
                name: "VeiculosVenda");

            migrationBuilder.DropTable(
                name: "VeiculosConsignacao");

            migrationBuilder.DropTable(
                name: "OrdensServico");

            migrationBuilder.DropTable(
                name: "VeiculosCliente");
        }
    }
}
