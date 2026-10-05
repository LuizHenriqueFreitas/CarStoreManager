# Dataset de demonstração (Python)

## v2 — base de 3 anos (out/2023 → hoje), out/2026

Plano: [`PLANO_BASE_DEMO.md`](./PLANO_BASE_DEMO.md) (Trilha B). A v1 ficou
guardada em `gerar_dataset_v1.py` / `dataset_demo_v1.json`.

```bash
python3 Geradores/dataset/gerar_dataset.py                          # HOJE = 05/10/2026
DATASET_HOJE=2026-10-06 python3 Geradores/dataset/gerar_dataset.py  # outra data de importação
```

- **Roteiro com mínimos garantidos** (constantes no topo: `PROPOSTAS_MIN`,
  `OS_QTD`, `CONSIGNACOES_POR_TIPO`, `TD_*`, `QTD_*`); o script imprime a
  tabela de cenários e marca `OK`/`ABAIXO DO MÍNIMO`.
- **Linha do tempo por registro** no contrato novo (§8): datas de vistoria,
  aprovação, início, alerta/decisão, requisição rejeitada, pagamentos,
  finalização, entrega, termo e assinatura — sempre crescentes, nunca depois
  de ontem 18h (exceto test drive agendado/reagendado), nunca antes da
  contratação do funcionário nem do cadastro do cliente (o `dataCriacao` do
  cliente é derivado do 1º evento dele). Status abertos só nos últimos 30 dias.
- **Pátio em regime**: 42 carros comprados na montagem (set/2023, mês
  pré-operação); cada venda dispara a compra de um carro de reposição de
  valor parecido. Assim a compra de veículos de cada ano ≈ custo do que foi
  vendido — sem isso nenhum ano fecha positivo (a compra entra como despesa
  na competência). Margem bruta 13–23% sobre o preço; ~2,6% das vendas perto
  do custo.
- **Despesas**: 26 itens de modelo com variação mensal (energia sazonal,
  dissídio em maio, reajuste de aluguel em março, comissões ∝ vendas,
  comissão dos mecânicos = 30% da mão de obra...) via `fechamentosMensais`
  (§8.1; todo mês fechado no 1º–5º dia útil seguinte, exceto o corrente) +
  ~70–90 despesas esporádicas aleatórias (manutenção / perdas /
  investimentos, poucos eventos grandes de R$ 15–55 mil).
- **Checagem de margem** com a regra do `DashboardService` (receita de OS
  paga pela DataCriacao, de venda pela DataAprovacao; Geral dividida meio a
  meio): por ano e por janela de 12 meses × oficina/concessionária/empresa,
  faixa exigida 0–15%.
- **Autoverificação**: integridade referencial (toda chave resolve no tipo
  certo, veículo da OS pertence ao cliente, nenhuma proposta depois da venda
  do veículo, test drive sempre antes da venda, consignado só ativo),
  documentos únicos e estimativa de linhas no banco (~11 mil). Sai com
  código 1 se algo falhar.
- Placa repetida de propósito: o carro vendido que volta pra revisão vira
  `VeiculoCliente` do comprador com a **mesma placa** do `VeiculoVenda`.

---

## v1 (histórico)


Gera `dataset_demo.json` — o arquivo pronto pra subir em **Configurações →
Importar dados** — a partir de um script Python, sem depender de rodar nada
em .NET.

## Por que não pandas

O documento final é JSON aninhado (proposta referenciando cliente/veículo/
vendedor por "chave", endereço embutido no cliente...), não uma tabela.
Pandas ajuda quando o problema é tabular — aqui só obrigaria a achatar tudo
em DataFrames e remontar a árvore na mão depois. `dict` + `json.dump` da
biblioteca padrão resolvem de forma mais direta, sem dependência extra.

## Por que não é aleatório como o `Geradores/` em C#

O gerador em C# sorteia e só uma fração de cada entidade chega a cada
estágio do funil — bom pra simular volume, ruim pra apresentação (você não
controla se vai existir uma OS cancelada pra mostrar). Este script é
**exaustivo**: cobre pelo menos um registro por combinação relevante que a
aplicação oferece —

- **Proposta de venda**: os 4 estágios intermediários (aprovada, vistoriada,
  termo redigido, termo enviado) × as 3 formas de pagamento aceitas (Pix,
  Transferência, Boleto), mais concluída à vista (3 formas), financiada
  (5 planos de parcelamento), rejeitada (4 motivos) e recém-criada.
- **Ordem de serviço**: os 5 tipos de serviço × 5 cenários de status
  (pendente, cancelada, **canceladaEmAndamento** — desistência já com o
  mecânico trabalhando, diferente de cancelar ainda no orçamento —,
  emAndamento, finalizadaPendente), mais finalizadaPaga/entregue, cada
  "paga" com uma forma de pagamento diferente. ~25% das OS que chegam a
  iniciar o serviço ganham `comAlerta: true` — simula o mecânico
  encontrando um problema novo durante o serviço (emite alerta, pausa a OS,
  cliente aprova o escopo maior, serviço retoma). Peças por origem
  (estoque/cliente/**encomenda**) também variam em toda OS, não só numa
  categoria — "encomenda" já cobre o cenário de peça sob encomenda pedido
  explicitamente.
  - **Importante**: `OrdemServicoService.FinalizarAsync` agora exige que a
    OS já esteja totalmente paga — não existe mais "finalizar sem pagar".
    Por isso o cenário `finalizadaPendente` pára em EmAndamento com a
    checklist concluída (pronta pra finalizar, falta cobrar) em vez de
    alcançar o antigo status "Pagamento Pendente", que deixou de ser
    alcançável por esse caminho.
  - **Status ainda "abertos" (pendente/emAndamento/finalizadaPendente) só
    usam datas RECENTES** (últimas ~3-4 semanas), nunca os 36 meses
    inteiros — uma OS "pendente" datada de 2 anos atrás não é realidade de
    oficina nenhuma, e como o prazo dela também cairia no passado, contava
    como "atrasada" pro resto da vida do sistema (era assim que surgiam as
    "88 OS atrasadas" relatadas antes desse ajuste). Dentro da janela
    recente, prazo e data de criação são sorteados JUNTOS (não soltos) pra
    ~75% ficar dentro do prazo e só uma minoria realista ficar atrasada.
    "cancelada"/"canceladaEmAndamento" são estados terminais (não contam
    como atrasada) e continuam espalhados pelos 36 meses inteiros, como
    "finalizadaPaga"/"entregue".
- **Consignação**: os 4 status × os 2 tipos de comissão (fixo/porcentagem).
- **Test drive**: agendado (sempre no futuro próximo — compromisso de
  verdade, nunca backdatado), realizado (maioria, domina o volume — é o que
  normalmente leva a uma proposta depois), cancelado e naoCompareceu.
  Importado **antes** das propostas de venda no pipeline — agendar test
  drive de um veículo já marcado "Vendido" é bloqueado, então precisa
  acontecer enquanto o carro ainda está disponível.
- **Componentes de estoque**: os 11 sistemas do veículo (motor, freios,
  suspensão...) com várias peças típicas cada — e ~15% propositalmente com
  estoque abaixo do mínimo, pra mostrar o alerta de estoque baixo do
  dashboard. Faixas de custo/quantidade calibradas pro VALOR TOTAL do
  estoque (custo × quantidade, somado) ficar realista pra uma oficina
  pequena/média (~R$25-50 mil) — nunca as centenas de milhares que uma
  versão anterior deste script produzia.
- **Despesas — variação histórica real, não só o modelo recorrente**:
  - **Compra de veículo**: cadastrar um `VeiculoVenda` lança automaticamente
    uma despesa "Compra de veículo para concessionária" na competência da
    própria data de cadastro (`VeiculoVendaService.AddAsync` — regra de
    negócio do sistema, não algo que o script decide). Só ~60% dos meses
    (`MESES_COM_COMPRA_VEICULO_PCT`) recebem veículo — o resto fica sem essa
    despesa, criando contraste real entre "mês com compra de carro" e "mês
    sem". A importação, ao aplicar a data histórica retroativa no veículo,
    também move essa despesa (criada em "hoje" pela regra de negócio) pra
    competência histórica certa — ver `ImportacaoDadosService
    .MoverDespesaParaCompetenciaHistoricaAsync`.
  - **`DESPESAS_EXTRAS_CATALOGO`** (10 eventos): compra/troca de
    equipamento da oficina, reforma, e a perda de um veículo próprio (motor
    fundido) — espalhados pelo histórico, cada um lançado como item extra
    na competência certa (`despesasExtras` no JSON,
    `ImportacaoDadosService.ImportarDespesasExtrasAsync`).
  - Meses sem compra de veículo nem evento do catálogo de extras ficam com
    **nenhum balanço criado** — o dashboard cai automaticamente na
    estimativa do modelo recorrente (~R$26 mil/mês) pra esses meses, exatamente
    o "só o básico" pedido. Resultado observado numa geração: totais mensais
    variando de ~R$28 mil (só o básico) a ~R$830 mil (mês com várias compras
    de veículo), com 10 dos 36 meses sem balanço nenhum.

Cada combinação acima se repete algumas vezes (`REPETICOES_*` no topo do
script) com cliente/valor/data diferentes a cada rodada — sem isso, o
dashboard mostrava tudo empatado (a mesma quantidade em cada barra), porque
cada combinação só existia uma vez.

Os valores continuam redondos de propósito (múltiplos de R$50/100/500,
quilometragem em centenas/milhares) — mas sorteados dentro de uma faixa
ampla, não fixos: redondo para leitura, não idêntico entre registros.

## Uso

```bash
python3 Geradores/dataset/gerar_dataset.py
```

Sem dependências além da biblioteca padrão do Python 3. Escreve
`Geradores/dataset/dataset_demo.json` ao lado do script. Depois é só subir
esse arquivo em Configurações → Importar dados, logado como Admin.

CPF, CNPJ e RENAVAM gerados já vêm com dígito verificador calculado pelo
mesmo algoritmo do backend (`Domain/ValueObjects/Cpf.cs` e `Renavam.cs`) —
o arquivo foi validado rodando o pipeline de importação real (`Configurações
→ Importar dados`, mesmo `ImportacaoDadosService` usado pela UI) de ponta a
ponta contra um banco descartável antes deste commit: **1423/1423 registros
criados, 0 avisos**.

## Ajustando quantidade/variedade

Os números (40 clientes, 25 veículos de cliente...) estão como parâmetro nas
funções `gerar_*()` em `gerar_dataset.py` — aumente ali se quiser uma base
maior. Pra mais volume sem mexer em nada além de um número, aumente
`REPETICOES_PROPOSTA` / `REPETICOES_OS` / `REPETICOES_CONSIGNACAO` no topo
do arquivo — a cobertura combinatória (status × forma de pagamento × tipo)
fica igual não importa a escala, só aumenta quantas vezes cada combinação
aparece (com valores diferentes).

Última geração: 1423 registros (21 usuários, 50 clientes, 12 fornecedores,
54 componentes, 6 checklist presets, 3 templates de documento, 26 despesas,
266 veículos de venda, 30 consignações, 50 veículos de cliente,
260 propostas, 600 ordens de serviço, 35 test drives, 10 despesas extras) —
janela operacional de **3 anos** (`MESES_OPERACAO = 36`). Números-alvo
pedidos explicitamente pelo dono, todos batidos com folga na última geração:
**450 OS "Entregue"/"Finalizada"** (pedido: ≥300 finalizadas e recebidas),
**240 propostas concluídas** (pedido: >150 carros vendidos). Valor total do
estoque de peças: ~R$48 mil. OS "atrasadas" (status aberto + prazo
vencido): ~21, todas recentes — nenhuma "esquecida" há anos.

Margem financeira (ver `docs/redesign/17-margem-financeira-realista.md` e
`19-bug-margem-mensal-negativa.md`): preço de venda de cada veículo é
acoplado ao custo de aquisição daquele veículo específico (só ~3% dos
casos, "problema oculto na compra", pode sair perto ou abaixo do custo) —
3,3% das vendas no prejuízo (era 39,4% quando o preço era sorteado solto).
Margem líquida agregada (36 meses): oficina 8,5% (alvo 3-10%) ✅,
concessionária 8,8% (alvo 10-20%, ficou logo abaixo — limite estrutural do
regime de caixa com estoque ainda não vendido no fim do período, ver
"Capital em estoque"). Contagem de compra de veículo e de venda por mês
agora é quase uniforme (`ciclo_meses`, não mais aleatória) — dois bugs reais
no `DashboardService` (receita de oficina ignorava OS "Entregue"; despesa
do período somava o mês inteiro mesmo em janelas parciais) também foram
corrigidos, ver doc 19.

## Base para a apresentação — um comando

```bash
bash Geradores/dataset/preparar_base_apresentacao.sh              # base termina HOJE
bash Geradores/dataset/preparar_base_apresentacao.sh 2026-11-18   # termina nesse dia (hoje ou passado)
```

Rode **no dia da apresentação** (ou na véspera): a base cobre de out/2023 até a
data, então o mês corrente aparece preenchido e nada fica datado no futuro. O
script gera o JSON, importa num banco temporário pela importação do próprio
sistema, valida (`validar_banco.py`: 0 avisos, margem líquida anual 0–15% por
ano × setor, todos os combustíveis vendidos, todos os status de OS/test drive,
nenhuma despesa sem categoria) e só então restaura em `Web/carstore.db` pela
restauração do sistema, que guarda antes `Web/backups/pre-restauracao-*.db`.
Se uma semente reprova, tenta a próxima (~1–6 min no total). Funciona com o
site aberto; depois é só recarregar a página.

Datas futuras não são possíveis: o importador do DELORE grava qualquer data
posterior a "agora" como "agora" — por isso a base é gerada no próprio dia.
