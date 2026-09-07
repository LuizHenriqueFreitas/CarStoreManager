# Geradores

Ferramenta de linha de comando para popular o banco SQLite local (carstore.db)
com dados fake realistas, distribuídos entre todas as entidades cadastráveis
do sistema.
Feita para ser reaproveitada sempre que a base precisar crescer — não é um
script de uso único.

## Por que isso existe

Em vez de inserir registros direto no banco (via SQL ou EF cru), cada gerador
chama os **mesmos Application Services que a interface web usa**
(`IClienteService.AddAsync`, `IAuthService.CriarUsuarioAsync`,
`IVeiculoVendaService.AddAsync`, etc.). Isso significa que os dados gerados:

- passam pelas mesmas validações de domínio (CPF/RENAVAM com dígito
  verificador correto, placas em formato válido, senha com maiúscula+número,
  etc.);
- respeitam as mesmas regras de negócio (ex.: um veículo só pode ir para uma
  proposta de venda depois de "liberado para venda"; uma OS não pode ter
  prazo no passado);
- continuam válidos mesmo que essas regras mudem no futuro — o gerador nunca
  fica dessincronizado da aplicação real, porque ele *é* a aplicação real.

## Como rodar

```bash
# a partir da raiz do repositório
dotnet run --project Geradores
```

Sem argumentos, gera a **base de demonstração completa**: a loja é tratada
como se estivesse **em operação há ~4 anos** (`--anos-operacao`, faixa útil
3–5), e o resultado é da ordem de **12 mil registros** no banco, com dados em
todas as tabelas cadastráveis e processos em **todos os estágios** dos fluxos
de oficina e concessionária. Ao final imprime quantos registros foram criados
por entidade, com o detalhamento por cenário de OS/consignação/test drive.

O banco alvo é sempre `Web/carstore.db` (o mesmo arquivo que o site abre),
independente de qual diretório o `dotnet run` é disparado.

### Cobertura de cenários

- **Ordens de serviço** — concluídas com sucesso (feitas, pagas, entregues),
  concluídas com problema no meio (requisição de peça atendida, pausa por
  aumento de escopo, pagamento parcial em aberto) e não concluídas (paradas em
  orçamento, revisão, aprovação, execução, busca de peças, pausa). Cada OS
  ganha checklist (de um preset), itens de peça do estoque e pagamentos.
- **Propostas de venda** — funil completo (rejeitada, em vistoria, aguardando
  assinatura, concluída à vista e via financiamento, recém-criada).
- **Consignações** — ativas, vendidas aguardando pagamento, concluídas,
  devolvidas.
- **Test drives** — agendados (futuros), realizados, cancelados, não
  compareceu, reagendados.
- **Balanços mensais de despesas** — uma competência por mês de todo o
  período histórico, geradas do formulário-modelo + linhas reais, fechadas
  (menos as 2 mais recentes) — base das análises retro-avaliativas.

### Rodar de novo para aumentar ainda mais a base

Pode ser executado quantas vezes for preciso — a primeira coisa que o
programa faz é carregar do banco todos os e-mails, CPFs, placas, RENAVAMs e
SKUs já existentes, então novas execuções nunca colidem com dados de
execuções anteriores.

```bash
# gerar só mais clientes e ordens de serviço, sem mexer no resto
dotnet run --project Geradores -- --vendedores 0 --mecanicos 0 --recepcionistas 0 \
  --chefes-oficina 0 --gerentes-vendas 0 --admins 0 --clientes 0 --componentes 0 \
  --despesas 0 --checklists 0 --veiculos-venda 0 --veiculos-cliente 0 \
  --veiculos-consignados 0 --propostas 0 --clientes 300 --ordens-servico 200
```

Ou simplesmente rodar sem argumentos de novo para dobrar a base.

### Opções disponíveis

```
dotnet run --project Geradores -- --ajuda
```

| Opção                     | Entidade                          | Padrão |
|---------------------------|------------------------------------|--------|
| `--seed N`                | semente do RNG (reprodutibilidade) | aleatória |
| `--anos-operacao N`       | janela histórica (anos) — 3 a 5    | 4 |
| `--vendedores N`          | Usuário — Vendedor                 | 30 |
| `--mecanicos N`           | Usuário — Mecânico                 | 30 |
| `--recepcionistas N`      | Usuário — Recepcionista            | 18 |
| `--chefes-oficina N`      | Usuário — Chefe de oficina         | 6 |
| `--gerentes-vendas N`     | Usuário — Gerente de vendas        | 6 |
| `--admins N`              | Usuário — Admin                    | 4 |
| `--clientes N`            | Cliente                            | 800 |
| `--fornecedores N`        | Fornecedor (peças da oficina)      | 60 |
| `--componentes N`         | Componente (peça de estoque)       | 300 |
| `--despesas N`            | Despesa (formulário-modelo)        | 45 |
| `--checklists N`          | Checklist preset                   | 24 |
| `--veiculos-venda N`      | Veículo (concessionária)           | 900 |
| `--veiculos-cliente N`    | Veículo de cliente (oficina)       | 900 |
| `--veiculos-consignados N`| Veículo consignado                 | 350 |
| `--propostas N`           | Proposta de venda                  | 550 |
| `--test-drives N`         | Test drive                         | 700 |
| `--ordens-servico N`      | Ordem de serviço                   | 900 |

> Os **balanços mensais de despesas** não têm opção de quantidade — é sempre
> uma competência por mês de `--anos-operacao` (≈ 48 balanços no padrão).

### Login nos usuários gerados

Todo usuário criado pelo gerador (Vendedor, Mecânico, Recepcionista, Chefe de
oficina, Gerente de vendas, Admin) tem a senha **`Teste@123`**. Os e-mails
individuais não são impressos no console (para não poluir o output com
centenas de linhas) — consulte a tela **Usuários** no admin, com busca por
nome/telefone, para ver a lista completa.

## Estrutura

```
Geradores/
  Program.cs              orquestra a ordem de geração e imprime o resumo
  Geradores.csproj         referencia só o Infrastructure (que já traz Domain + Application)

  Nucleo/                  peças reaproveitáveis por qualquer gerador novo
    ComposicaoServicos.cs   monta o mesmo grafo de DI que o Web usa, apontando pro carstore.db local
    CaminhosProjeto.cs      acha a raiz do repo e o appsettings*.json sem depender do cwd
    OpcoesCli.cs             parsing dos argumentos de linha de comando + defaults
    PoolsIniciais.cs         carrega valores já existentes no banco (evita colisão em reexecuções)
    UniquePool.cs            reserva valores únicos (retry automático se colidir)
    DocumentoUtils.cs        gera CPF/RENAVAM com dígito verificador válido, placa, telefone, CEP
    ExecutorLote.cs           laço genérico "para N itens, abre escopo de DI, chama AddAsync"
    AmbienteWebFake.cs        stub de IWebHostEnvironment (dependência transitiva fora de um host web)

  Dados/
    NomesPt.cs                pools de nomes, cidades, bairros, marcas/modelos de veículo, peças

  Entidades/                 um gerador por entidade cadastrável
    UsuarioGerador.cs
    ClienteGerador.cs
    FornecedorGerador.cs
    ComponenteGerador.cs         (depende de FornecedorGerador)
    DespesaGerador.cs
    BalancoDespesaGerador.cs     (depende de DespesaGerador — 1 competência/mês)
    ChecklistPresetGerador.cs
    VeiculoVendaGerador.cs
    VeiculoClienteGerador.cs
    VeiculoConsignacaoGerador.cs
    PropostaVendaGerador.cs
    TestDriveGerador.cs
    OrdemServicoGerador.cs       (checklist + itens + pagamentos + requisições + alertas)
```

## Adicionando um gerador para uma entidade nova

1. Olhe o `Criar<Entidade>DTO` e o método `AddAsync` do Application Service
   correspondente — são a fonte da verdade de quais campos preencher e quais
   regras respeitar (ex.: algum campo depende de outra entidade já existir?).
2. Crie `Entidades/<Entidade>Gerador.cs` com um método estático
   `GerarAsync(IServiceProvider provider, int quantidade, Random rng, ...)`
   que usa `ExecutorLote.ExecutarAsync<IServicoDaEntidade>(...)` — não escreva
   o laço de criação/contagem na mão, reaproveite o executor.
3. Se precisar de nome/cidade/CPF/placa/etc., puxe de `Dados/NomesPt.cs` e
   `Nucleo/DocumentoUtils.cs` em vez de inventar geração nova.
4. Se o campo precisar ser único (email, CPF, placa, SKU...), passe por um
   `UniquePool` (crie um novo em `PoolsIniciais.cs` se for um campo novo).
5. Registre a chamada em `Program.cs`, na ordem certa de dependência (crie
   entidades que outras referenciam por Id ANTES das que dependem delas).

## Entidades fora do escopo

`AnuncioMercadoLivre`, `VendaMercadoLivre` e `ConfiguracaoMercadoLivre` não
têm gerador — não são criadas por um formulário de cadastro comum, e sim
sincronizadas a partir de uma conta real (ou mock) do Mercado Livre via OAuth.
`ConfiguracaoSistema` é um singleton administrativo (não um cadastro
repetível). `Fornecedor`/`NotaFiscal` foram removidos do sistema nesta
sessão e não existem mais.
