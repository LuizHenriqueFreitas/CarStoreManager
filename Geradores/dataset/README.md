# Dataset de demonstração (Python)

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
- **Ordem de serviço**: os 5 status × os 5 tipos de serviço = 25 combinações,
  cada "finalizada e paga" com uma forma de pagamento diferente.
- **Consignação**: os 4 status × os 2 tipos de comissão (fixo/porcentagem).
- **Componentes de estoque**: os 11 sistemas do veículo (motor, freios,
  suspensão...) com várias peças típicas cada — e ~15% propositalmente com
  estoque abaixo do mínimo, pra mostrar o alerta de estoque baixo do
  dashboard.

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
o arquivo foi validado rodando o pipeline de importação real de ponta a
ponta antes deste commit, sem nenhum aviso.

## Ajustando quantidade/variedade

Os números (40 clientes, 25 veículos de cliente...) estão como parâmetro nas
funções `gerar_*()` em `gerar_dataset.py` — aumente ali se quiser uma base
maior. Pra mais volume sem mexer em nada além de um número, aumente
`REPETICOES_PROPOSTA` / `REPETICOES_OS` / `REPETICOES_CONSIGNACAO` no topo
do arquivo — a cobertura combinatória (status × forma de pagamento × tipo)
fica igual não importa a escala, só aumenta quantas vezes cada combinação
aparece (com valores diferentes).

Última geração: 388 registros (18 usuários, 40 clientes, 54 componentes,
85 veículos de venda, 16 consignações, 25 veículos de cliente, 75 propostas,
75 ordens de serviço).
