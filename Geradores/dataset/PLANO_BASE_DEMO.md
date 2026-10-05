# Plano — Base de demonstração v2 (~10 mil registros, 3 anos de operação)

> Criado em 05/10/2026 (véspera da apresentação). O `Web/carstore.db`
> populado foi perdido; a base antiga (`demo_database.json`, 21/09) tinha
> **7.646 linhas** e é anterior a vistoria de OS (doc 32), termo de test drive
> (doc 30), test drive de consignado (doc 31) e componentes compatíveis (doc 35).

## 1. Objetivo

Reconstruir `Web/carstore.db` com **~10–11 mil linhas no banco** (contando
todas as tabelas: OS + checklist + itens + pagamentos + termos...) que pareçam
**3 anos reais de operação** (out/2023 → out/2026) de uma concessionária +
oficina de médio porte:

- todo fluxo do sistema aparece, inclusive os **mal-sucedidos** (proposta
  rejeitada/cancelada, financiamento negado, OS cancelada, orçamento recusado,
  peça encomendada, alerta recusado, cliente que não compareceu...);
- **valores redondos** (múltiplos de R$ 50/100/500/1.000);
- **coerência econômica**: custo de aquisição < preço de venda (veículo e
  peça); **margem líquida ANUAL entre 0% e 15%** (definição do dono) em cada
  ano da janela, na empresa e em cada setor — alvo ~6–10% com variação natural
  entre anos; prejuízo só em vendas isoladas explicáveis, nunca num ano;
- **coerência temporal**: toda data secundária (pagamento, vistoria, termo,
  assinatura, alerta, requisição, histórico de consignação, despesa
  automática) fica **na linha do tempo do registro**, nunca no dia da
  importação;
- documentos acessórios coerentes: termos de entrega assinados pelo próprio
  cliente (nome/CPF reais dele), templates, checklists, contratos de
  consignação, termos de test drive, vistorias de OS.

## 2. Estratégia (por que este caminho)

Mesmo pipeline que já funcionou (1423/1423 sem aviso), só que ampliado:

```
gerar_dataset.py  ──►  dataset_demo.json  ──►  ImportacaoDadosService  ──►  Web/carstore.db
 (Python, roteiro        (chave/cenario,         (regras de negócio REAIS
  exaustivo, seed fixa)   datas da linha           + IBackdateService pra
                          do tempo)                 datas históricas)
```

- **Nada de INSERT direto** — tudo passa pelos Application Services, então a
  base respeita as mesmas validações que a demo vai mostrar.
- **Roteiro, não sorteio**: cada cenário existe com quantidade garantida
  (dá pra abrir a OS cancelada/pausada/aguardando peça na apresentação).
- **Importação via CLI** (`dotnet run --project Geradores -- --importar
  <arquivo>`) além da UI — mais rápido e reproduzível; a UI continua aceitando
  o mesmo arquivo.
- **Backup**: ao final, cópia do banco pronto em
  `Geradores/dataset/carstore_demo.db` (não versionado) — pra nunca mais
  perder a base na véspera.

## 3. Volumes-alvo (linhas no banco)

| Área | Entidade | Alvo | Observação |
|---|---|---:|---|
| Pessoas | Usuários (+ tabela do papel) | ~30 (+30) | 1 admin extra, 1 gerente vendas, 6 vendedores, 1 chefe oficina, 8 mecânicos (especialidades distintas), 3 recepcionistas; contratações escalonadas no tempo |
| | Clientes (+ endereço) | ~300 (+300) | **recorrência**: ~40% voltam à oficina 2+ vezes; alguns compram carro e depois fazem revisão |
| Oficina | Fornecedores | 15 | |
| | Componentes + estoque | ~70 (+70) | ~15% abaixo do mínimo (alerta); reposições espalhadas no histórico |
| | Componentes compatíveis | ~20 | curadoria manual (doc 35) |
| | Checklist presets (+ itens) | ~9 (+~60) | |
| | Veículos de cliente | ~340 | |
| | Ordens de serviço | ~700 | ver cenários §4.2 |
| | Checklist da OS | ~3.200 | gerado pelo preset |
| | Itens da OS | ~1.400 | estoque / cliente / encomenda |
| | Vistorias de entrada OS | ~650 | doc 32 |
| | Pagamentos OS | ~650 | inclui parcial e dividido |
| | Requisições de peça | ~250 | atendidas / pendentes / rejeitadas |
| | Alertas | ~170 | aprovados / recusados / pendentes |
| Concessionária | Veículos à venda | ~380 | ~60% vendidos, resto em estoque/preparação |
| | Propostas | ~330 | ver §4.1 |
| | Vistorias / termos de entrega / pagamentos | ~290 / ~270 / ~330 | |
| | Test drives (+ termo) | ~220 (+220) | inclui de consignado |
| | Consignações (+ histórico) | ~48 (+~100) | 4 status × 2 tipos de comissão |
| Sistema | Templates de documento | ~5 | termo de entrega, contrato de consignação, vistoria de OS, termo de test drive, resposta de financiadora |
| | Despesas (modelo) | ~26 | |
| | Balanços mensais (+ itens) | 36 (+~500) | variação real: compra de veículo, peças, equipamento, R$ 50 de test drive |
| **Total** | | **~10.500** | conferido por script ao final (§7) |

## 4. Matriz de cenários (cada um com quantidade mínima garantida)

### 4.1 Proposta de venda

| Cenário | Funil percorrido | Qtd mín. |
|---|---|---:|
| `criada` | recém-aberta (últimas 2 semanas) | 6 |
| `rejeitada` | cliente desistiu / preço / achou outro (4+ motivos) | 30 |
| `financiamentoNegado` | solicitou → financiadora negou | 15 |
| `cancelada` **(novo)** | aprovada → cancelada com motivo | 15 |
| `aprovada` / `vistoriada` | paradas no meio (recentes) | 5 / 5 |
| `pagamentoParcial` **(novo)** | vistoriada + sinal pago, saldo em aberto (recente) | 6 |
| `termoRedigido` / `termoEnviado` | aguardando assinatura (recentes) | 5 / 5 |
| `concluidaAVista` | Pix / Transferência / Boleto; parte com **2 pagamentos** (sinal + saldo) | ~150 |
| `concluidaFinanciada` | entrada (Pix) + financiamento; 5 planos | ~70 |

Termo de entrega: texto vindo do **rascunho do template** do sistema
(`ObterRascunhoInicialTermoAsync`), assinado com **nome e CPF do cliente da
proposta**. Datas: criação → aprovação (+1–5 d) → vistoria (+1–3 d) →
pagamentos → termo (+0–2 d) → assinatura (+0–2 d).

### 4.2 Ordem de serviço

Fluxo completo novo (doc 32): `Pendente → EmVistoria → AguardandoCliente →
Aprovada → EmAndamento [→ Pausada ↔] → Finalizada (já paga) → Entregue`.

| Cenário | Status final | Qtd mín. |
|---|---|---:|
| `pendente` | Pendente (recente) | 10 |
| `emVistoria` **(novo)** | EmVistoria (recente) | 8 |
| `aguardandoCliente` **(novo)** | orçamento apresentado (recente) | 10 |
| `orcamentoRecusado` **(novo)** | vistoria feita → cliente recusou → Cancelada | 30 |
| `aprovada` **(novo)** | aprovada, aguardando mecânico (recente) | 8 |
| `cancelada` | cancelada ainda pendente | 20 |
| `canceladaEmAndamento` | desistência no meio do serviço | 15 |
| `aguardandoPeca` **(novo)** | BuscandoPecasParaOrcamento — requisição aberta pendente (recente) | 8 |
| `pausada` **(novo)** | alerta emitido, aguardando decisão do cliente (recente) | 8 |
| `emAndamento` | mecânico trabalhando (recente); parte com **sinal pago** | 20 |
| `finalizadaPendente` | checklist concluída, falta cobrar (recente) | 8 |
| `finalizadaPaga` | paga, aguardando retirada | ~60 |
| `entregue` | ciclo completo | ~450 |

Variações transversais (sobre os cenários que passam pela etapa):
- **Alerta** (~25% das iniciadas): aprovado (maioria) / **recusado** pelo cliente.
- **Peças**: estoque / trazida pelo cliente / **encomenda** (requisição
  atendida); ~5% com **requisição rejeitada** (peça substituída por compatível).
- **Pagamento**: 1 forma (maioria) ou **dividido** em 2 formas; 5 modos (Dinheiro,
  Pix, Débito, Crédito, Transferência).
- **Mecânico** escolhido pela especialidade × tipo de serviço; recepcionista
  faz a vistoria.

Datas: abertura → vistoria (0–1 d) → aprovação (0–2 d) → início (0–2 d) →
alerta/peça (+1–5 d) → pagamento → entrega (0–3 d). Status abertos só nos
últimos ~30 dias, ~75% dentro do prazo (regra já existente).

### 4.3 Consignação
`ativa`, `vendidaAguardando`, `concluida`, `devolvida`, `cancelada` × `Fixo` /
`Porcentagem`, ~48 no total. Histórico de status com datas da linha do tempo.

### 4.4 Test drive
`realizado` (maioria; termo **assinado** pelo cliente antes de sair, lança
R$ 50 de combustível na competência certa), `cancelado`, `naoCompareceu`,
`reagendado`, `agendado` (futuro próximo, parte com termo enviado e não
assinado). ~10% em **veículo consignado**. Datas antes da venda do veículo.

### 4.5 Cadastros de suporte
Templates (5), presets (9), componentes compatíveis (~20), despesas-modelo,
despesas extras (equipamento, reforma, motor fundido...), config. do sistema.

## 5. Coerência econômica (regras do gerador)

- Veículo: `valor_venda = aquisição × (1,10 … 1,25)` arredondado a R$ 500;
  ~3% vendidos perto do custo (problema oculto), justificando prejuízo pontual.
- Desconto de proposta ≤ 5% e nunca abaixo da aquisição.
- Peça: margem 30–80% sobre custo (padrão por sistema); estoque total ~R$ 30–60 mil.
- Mão de obra (`CustoServico`): R$ 150–2.500 conforme tipo.
- Consignação: comissão da loja 8–12% ou fixo R$ 2.000–5.000.
- Despesas: salários/aluguel/utilidades fixos + compras pontuais; checagem
  final da **margem líquida anual (0–15%) por ano × setor** (§7).

## 6. Divisão do trabalho (execução paralela)

### Trilha A — C# (importador + CLI) — `Application/…/ImportacaoDadosService.cs`, DTOs, `Geradores/`
1. **CLI** `--importar <arquivo.json>`: aplica migrations + seed iguais ao Web,
   roda `IImportacaoDadosService`, imprime contagens e avisos.
2. **Contrato novo do JSON** (§8) — tudo **opcional e retrocompatível**.
3. **Backdate das datas secundárias** (pagamentos, vistorias, termos,
   assinaturas, alertas, requisições, `DataRecebimento` de item, histórico de
   consignação, termo de test drive).
4. **Despesas automáticas** criadas "hoje" pela regra de negócio (R$ 50 do
   test drive, entrada de estoque por requisição) movidas pra competência
   histórica (mesmo padrão de `MoverDespesaParaCompetenciaHistoricaAsync`).
5. Textos reais: termo/vistoria a partir do rascunho do template; assinatura
   com nome/CPF do cliente.
6. `dotnet build` + `dotnet test` verdes.

### Trilha B — Python (`gerar_dataset.py`)
1. Volumes da §3 e matriz da §4 (constantes `REPETICOES_*`/`QTD_*` no topo).
2. Linha do tempo coerente por registro (datas da §4) emitida no JSON.
3. Regras econômicas da §5; recorrência de clientes; especialidade do mecânico.
4. Script de autoverificação local (contagem por cenário, margens previstas).

### Integração (agente principal)
Gerar JSON → banco zerado (apagar `carstore.db`, `-shm`, `-wal`) → importar
via CLI → **0 avisos** → validação §7 → backup → subir o site e conferir telas.

## 7. Validação (critérios de aceite)

- [ ] Importação com **0 avisos**; total de linhas no banco entre 9.500 e 12.000.
- [ ] Cada cenário de §4 presente no banco com a contagem mínima (SQL por status).
- [ ] Nenhum `DataPagamento`/`DataAssinatura`/`DataConclusao` de registro
      histórico no dia da importação (só os recentes legítimos).
- [ ] 100% das vendas concluídas com `ValorFinal > ValorAquisicao`, exceto ≤3%.
- [ ] Margem líquida ANUAL entre 0% e 15% em cada ano, na empresa e em cada setor (oficina, concessionária).
- [ ] OS atrasadas ≤ ~25, todas recentes.
- [ ] Mês corrente sem despesa absurda (nada "empilhado" no dia da importação).
- [ ] 36 balanços mensais, 35 fechados com data histórica, ~70–90 despesas
      esporádicas (manutenção / perda / investimento) — §8.1.
- [ ] Telas conferidas no navegador: início, Financeiro, hubs, Kanban da
      oficina, detalhe de OS/proposta (termo assinado), Consulta CPF, relatórios.
- [ ] `dotnet test` verde. **Nenhum commit.**

## 8. Contrato do JSON — campos novos (todos opcionais)

```jsonc
// usuarios[]            (sem mudança)
// componentesEquivalentes[]  — NOVA seção, processada após componentes
{ "componenteChave": "cmp-...", "equivalenteChave": "cmp-...", "observacao": "..." }

// propostasVenda[]
{
  "cenario": "... | cancelada | pagamentoParcial",
  "motivoCancelamento": "string?",
  "valorEntrada": 10000,                // financiada: entrada (DefinirEntradaAsync)
  "pagamentos": [                       // se ausente: 1 pagamento integral (comportamento atual)
    { "modo": "Pix", "percentual": 20, "data": "2024-03-10T14:00:00" },
    { "modo": "Transferencia", "percentual": 80, "data": "2024-03-12T10:00:00" }
  ],                                    // último pagamento de cenário quitado = saldo restante exato
  "dataVistoria": "...", "dataTermo": "...", "dataAssinatura": "..."
}

// ordensServico[]
{
  "cenario": "... | emVistoria | aguardandoCliente | orcamentoRecusado | aprovada | aguardandoPeca | pausada",
  "recepcionistaChave": "usr-...",      // presente => passa pela vistoria de entrada (doc 32)
  "textoVistoria": "string?",           // ausente => rascunho do template
  "alerta": { "descricao": "...", "decisao": "aprovado|recusado|pendente",
              "observacaoCliente": "...", "data": "...", "dataDecisao": "..." },
  "requisicaoRejeitada": { "descricaoPeca": "...", "motivo": "...", "data": "..." },
  "pagamentos": [ { "modo": "Pix", "percentual": 100, "data": "..." } ],
  "dataVistoria": "...", "dataAprovacaoCliente": "...", "dataInicio": "...",
  "dataFinalizacao": "...", "dataEntrega": "..."
  // "comAlerta": true continua aceito (= alerta aprovado)
}

// testDrives[]
{
  "cenario": "... | reagendado",
  "veiculoConsignadoChave": "csg-...",  // alternativa a veiculoVendaChave
  "termo": "assinado|enviado|rascunho", // padrão: assinado se realizado
  "dataReagendamentoOriginal": "..."
}

// veiculosConsignados[]
{ "dataVenda": "...", "dataConclusao": "...", "dataDevolucao": "...", "dataCancelamento": "..." }
```

### 8.1 Fechamento de mês e despesas esporádicas (pedido adicional do dono)

Hoje o importador **nunca fecha mês** e meses sem evento ficam **sem balanço**
(o dashboard cai na estimativa do modelo). Na v2:

- **Todo mês dos 36 tem balanço** gerado do modelo, com **variações** nas
  contas variáveis (energia mais alta no verão, água, internet, combustível,
  dissídio de ~5% em maio, reajuste anual de aluguel) e **fechado** no 1º–5º
  dia útil do mês seguinte. Só o mês corrente fica aberto.
- **Despesas esporádicas aleatórias** (~70–90 no período, seed fixa), em três famílias:
  - *Manutenção* — elevador automotivo, compressor, ar-condicionado,
    telhado, fachada, portão, extintores, gerador;
  - *Perdas* — peça danificada, ferramenta furtada, multa em test drive,
    franquia de sinistro de veículo do estoque, avaria indenizada a cliente,
    estoque obsoleto baixado, inadimplência, motor fundido;
  - *Investimentos* — scanner, alinhadora/balanceadora, elevador novo,
    computadores, software, reforma do showroom, feirão, treinamento de
    mecânicos, câmeras.
  Distribuição: alguns meses sem extra, a maioria com 1–3, poucos com evento
  grande (R$ 15–60 mil). A margem anual continua entre 0% e 15% (§5).

```jsonc
// fechamentosMensais[] — ÚLTIMA etapa da importação (balanço fechado rejeita item)
{ "ano": 2024, "mes": 3, "fechar": true, "dataFechamento": "2024-04-03T18:00:00",
  "variacoes": [ { "nome": "Energia elétrica", "valor": 4300 } ] }  // substitui o valor do item do modelo
```

Critério de aceite extra: 36 balanços, 35 fechados com `DataFechamento`
histórica, mês corrente aberto, e cada mês com total diferente do anterior.

## 8.2 Revisão de exportação/importação — por que a base foi perdida

### Achado 1 — o backup salvo nunca teve os dados (causa da perda)
`demo_database.json` (commit `6755eea`, 21/09 02:09) saiu do exportador
**antigo** (dump bruto por tabela, anterior à reescrita do doc 26). Esse dump
serializava os *value objects* (campos privados) como `{}` — **7.115 campos
vazios**, entre eles **todo valor monetário do sistema**:

| Perdido | Linhas |
|---|---:|
| `ItemOrdemServico.ValorUnitario` / `ValorTotal` | 1.161 cada |
| `OrdemServico.CustoServico` / `ValorTotal` | 601 cada |
| `PagamentoOrdemServico.Valor` | 450 |
| `ItemBalancoDespesa.Valor` | 397 |
| `VeiculoVenda.Valor` / `ValorAquisicao` / `Ano` / `Quilometragem` | 266 cada |
| `PropostaVenda.ValorBase` / `Desconto` / `ValorFinal` / `Entrada` | 260 cada |
| `PagamentoProposta.Valor` | 246 |
| Telefone, hash de senha e dados de funcionário de todos os usuários | — |

Além disso, nenhum importador lê esse formato. **O arquivo é irrecuperável.**
A base de 21/09 foi gerada a partir de `dataset_demo.json` v1 (preservado como
`dataset_demo_v1.json`), então pode ser **reproduzida** importando o v1 — é o
plano B da apresentação.

### Achado 2 — o exportador atual (doc 26) não é backup: ele é "lossy" por projeto
Ele exporta um *roteiro* (chave/cenario) que o importador **reencena** pelos
services. O que se perde num ciclo exportar → importar:

| # | Perda | Efeito na demo |
|---|---|---|
| 1 | Senhas (hash) → todas viram `Teste@123` | logins mudam |
| 2 | OS em EmVistoria/AguardandoCliente/Aprovada/Pausada/BuscandoPeças → **"emAndamento"** | Kanban errado, OS "iniciadas" que não foram |
| 3 | Checklist da OS: preset não é exportado → OS reimportada **sem checklist** | |
| 4 | Pagamentos: só o 1º modo; valor recalculado; **data = dia da importação**; parciais/divididos perdidos | |
| 5 | Item de OS: preço recalculado pelo preço **atual** da peça; peça do cliente sem cadastro descartada | valores históricos mudam |
| 6 | Alertas (texto, decisão recusada), requisições (atendida/rejeitada/pendente) perdidos | |
| 7 | Proposta Cancelada/Expirada/AguardandoFinanciadora → **"criada"** (volta a ficar aberta) | funil e KPIs errados |
| 8 | Vistoria, termo de entrega (texto, assinatura nome/CPF/IP/data) → recriados com placeholder "Cliente Demonstração" | |
| 9 | Estoque: histórico vira **1 entrada hoje** → despesa "Compra de componentes" inteira cai no **mês corrente** | Financeiro do mês explode |
| 10 | Balanços: fechamento, data de fechamento e valores editados dos itens do modelo perdidos (regenerados do modelo atual); despesas-modelo inativas perdidas | variação mensal some |
| 11 | Test drive de **consignado** exporta o Id do consignado como `veiculoVendaChave` → aviso e **registro descartado**; termo de test drive e reagendamento perdidos; R$ 50 de combustível pode duplicar | |
| 12 | Não exportados: componentes equivalentes, permissões (papel e individuais), configuração do sistema (módulos), vistorias, histórico de consignação, fotos | |
| 13 | Importação sem transação e sem checar banco vazio: importar 2× duplica/conflita; falha no meio deixa estado parcial | |

### Correções (trilhas C e A-2)
- **Trilha C — backup fiel (novo)**: "Baixar backup completo" = cópia
  consistente do SQLite (API de backup online / `VACUUM INTO`), e "Restaurar
  backup" (valida o arquivo, faz cópia de segurança do banco atual antes,
  restaura e aplica migrations pendentes). **Backup automático** ao subir o
  Web (mantém os últimos 7). UI em Configurações deixa claro: *backup = cópia
  exata*, *exportar JSON = dados editáveis, com perdas*.
- **Trilha A-2 — exportador alinhado ao contrato v2**: depois da trilha A,
  o exportador passa a emitir todos os cenários/campos da §8 (status exatos,
  pagamentos com data/modo/percentual, alerta/requisição, vistoria/termo com
  assinatura, test drive de consignado, equivalências, fechamentos mensais
  com variações, despesas sem duplicar), mais **teste automatizado de
  round-trip**: importar → exportar → importar em banco novo → exportar e
  comparar (contagem por status, soma de valores por mês, pagamentos).
- CLI: `--backup <arquivo.db>` e `--restaurar <arquivo.db>`.

## 9. Riscos e mitigação

| Risco | Mitigação |
|---|---|
| Regra de negócio rejeita data passada / transição | importador roda o funil com datas reais e só depois aplica backdate (padrão atual) |
| Importação lenta (~10k linhas via services) | CLI fora do Blazor; aceitável até ~15 min |
| WAL deixa dado antigo | apagar `carstore.db`, `-shm` e `-wal` juntos |
| Despesa automática empilhada no mês atual | mover pra competência histórica + checagem §7 |
| Tempo (apresentação amanhã) | trilhas A e B em paralelo; corte de escopo na ordem: fotos (fora) → permissões individuais (fora) → teste de consignado → reagendado |
