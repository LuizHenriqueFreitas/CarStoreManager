#!/usr/bin/env python3
"""
Gera um documento de dados de demonstração (dataset_demo.json) no formato
aceito por Configurações -> Importar dados (ImportacaoDadosDTO).

Por que Python puro em vez de pandas: o documento final é um JSON aninhado
(cliente com endereço embutido, proposta referenciando cliente/veículo/vendedor
por "chave"...), não uma tabela — pandas brilha em dados tabulares (agrupar,
cruzar colunas, exportar CSV), mas aqui só atrapalharia (ia exigir "achatar"
tudo em DataFrames e depois remontar a árvore na mão). dicionários + json.dump
da biblioteca padrão fazem o trabalho de forma mais direta, sem dependência
extra pra instalar.

Diferente do Geradores em C# (que sorteia valores e decide aleatoriamente que
fração de cada entidade chega a cada estágio), este script é EXAUSTIVO: gera
pelo menos um registro pra cada combinação de cenário relevante que a
aplicação oferece (todo status de Proposta x forma de pagamento onde isso
muda o fluxo, toda combinação Tipo x Status de Ordem de Serviço — incluindo
cancelamento em etapas diferentes e "novos problemas encontrados" via
alerta —, todo cenário de consignação x tipo de comissão, todo status de
test drive) — e cada combinação se repete algumas vezes com valores/datas
diferentes (REPETICOES_*), pra dashboard não mostrar tudo empatado.

Valores continuam redondos (múltiplos de 50/100/500, nunca centavos) mas
sorteados dentro de uma faixa ampla — redondo != idêntico. Mesma lógica pras
datas: espalhadas pelos últimos ~3 anos com alguma aleatoriedade, não em
degraus perfeitamente uniformes.

Uso:
    python3 Geradores/dataset/gerar_dataset.py
    (escreve Geradores/dataset/dataset_demo.json)
"""

import json
import random
from datetime import datetime, timedelta
from pathlib import Path

RNG_SEED = 42
random.seed(RNG_SEED)

HOJE = datetime.now().replace(hour=10, minute=0, second=0, microsecond=0)

# Quantas vezes cada combinação (status x forma de pagamento / tipo / etc.)
# se repete, cada vez com cliente/valor/data diferentes — sobe o volume e
# evita barras empatadas no dashboard sem perder a cobertura exaustiva.
# Repetições da parte "fechada"/pendente do funil (aprovada..termoEnviado,
# rejeitada, criada; e OS pendente/cancelada/canceladaEmAndamento/
# emAndamento/finalizadaPendente): baixo de propósito — pra demonstração,
# vendas paradas no meio do caminho e OS aguardando pagamento devem ser a
# MINORIA, não a maior parte do volume.
REPETICOES_PROPOSTA_PENDENTE = 1
REPETICOES_OS_PENDENTE = 6

# Repetições da parte CONCLUÍDA (proposta vendida, OS paga): alto de
# propósito — dominam o volume total e são o que sustenta um fluxo de caixa
# saudável no dashboard (ver DESPESAS_CATALOGO mais abaixo pra mais contexto).
# A concessionária não tem um estágio "vendido mas não retirado" — "concluida"
# já é o fim de fluxo. Pra nivelar a oficina com isso, a fatia "paga" da OS é
# dividida: a maioria vira "entregue" (paga E retirada — fim de fluxo real,
# equivalente a "concluida"), sobrando só uma minoria em "finalizadaPaga"
# (paga, aguardando retirada) — mesma soma de antes, mesma densidade total.
#
# REPETICOES_OS_PENDENTE=6 + OS_PAGA=25 + OS_ENTREGUE=65 dá (5*5*6) + (5*25) +
# (5*65) = 150+125+325 = 600 ordens de serviço nos 36 meses (~156 semanas) —
# "entregue" sozinha já passa de 300 (pedido explícito: "pelo menos 300
# ordens finalizadas e recebidas"), e 450 de 600 chegam a
# "finalizadaPaga"/"entregue": passam de verdade pelo painel de pagamento
# real (RegistrarPagamentoAsync via ImportacaoDadosService, não INSERT
# direto) antes de fechar.
#
# REPETICOES_PROPOSTA_CONCLUIDA=30 dá (3+5)*30 = 240 propostas concluídas —
# subiu de 20 (160 vendas) pra reduzir a variância de receita por janela de
# 30 dias: com só ~4-5 vendas/mês, uma janela podia pegar um "buraco" sem
# nenhuma venda por acaso (ver docs/redesign/19-bug-margem-mensal-negativa.md)
# — com ~6-7/mês distribuídas por `ciclo_meses`, isso fica bem mais raro.
REPETICOES_PROPOSTA_CONCLUIDA = 30
REPETICOES_OS_PAGA = 25
REPETICOES_OS_ENTREGUE = 65

REPETICOES_CONSIGNACAO = 3

# Test drive: "agendado" sempre no futuro próximo (compromisso de verdade pra
# mostrar na agenda, nunca backdatado) — minoria, igual aos outros estágios
# não concluídos (cancelado/naoCompareceu). "Realizado" domina o volume, é
# o que efetivamente alimenta propostas de venda depois.
REPETICOES_TESTDRIVE_PENDENTE = 5
REPETICOES_TESTDRIVE_REALIZADO = 20

# Janela operacional: 3 anos "sem lacunas" — todo mês precisa ter pelo menos
# um registro de cada operação (proposta, OS, veículo anunciado/consignado).
MESES_OPERACAO = 36

# Compra de veículo: ver gerar_veiculos_venda — todo mês recebe pelo menos
# uma compra, com uma fração sorteada como "mês de pico" (peso maior), pra
# manter contraste com os meses mais fracos sem o salto extremo de lote
# único que existia antes (ver docs/redesign/17-margem-financeira-realista.md).


def valor_redondo(minimo: int, maximo: int, passo: int) -> int:
    """Número redondo (múltiplo de `passo`) sorteado numa faixa ampla —
    redondo pra leitura, mas sem ficar preso a um punhado de valores fixos
    (o que gerava barras empatadas nos gráficos)."""
    n_passos = (maximo - minimo) // passo
    return minimo + random.randint(0, max(1, n_passos)) * passo


# =====================================================================
# DOCUMENTOS BRASILEIROS VÁLIDOS (mesmos algoritmos usados no Domain C#)
# =====================================================================

def cpf_valido(base9: str) -> str:
    """Recebe 9 dígitos e devolve o CPF de 11 dígitos com DVs corretos
    (mesmo algoritmo de Domain/ValueObjects/Cpf.cs)."""
    nums = [int(c) for c in base9]

    soma = sum(nums[i] * (10 - i) for i in range(9))
    resto = soma % 11
    d1 = 0 if resto < 2 else 11 - resto

    nums10 = nums + [d1]
    soma = sum(nums10[i] * (11 - i) for i in range(10))
    resto = soma % 11
    d2 = 0 if resto < 2 else 11 - resto

    return base9 + str(d1) + str(d2)


def renavam_valido(base10: str) -> str:
    """Recebe 10 dígitos e devolve o RENAVAM de 11 dígitos com DV correto
    (mesmo algoritmo de Domain/ValueObjects/Renavam.cs)."""
    invertido = base10[::-1]
    mult = [2, 3, 4, 5, 6, 7, 8, 9, 2, 3]
    soma = sum(int(invertido[i]) * mult[i] for i in range(10))
    mod = (soma * 10) % 11
    dv = 0 if mod == 10 else mod
    return base10 + str(dv)


_placas_usadas = set()


def proxima_placa() -> str:
    """Formato antigo ABC1234 (Domain/ValueObjects/PlacaVeiculo.cs aceita
    esse ou o padrão Mercosul) — sequencial, garantida única no dataset."""
    letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
    i = len(_placas_usadas)
    l1 = letras[(i // (26 * 26 * 10000)) % 26]
    l2 = letras[(i // (26 * 10000)) % 26]
    l3 = letras[(i // 10000) % 26]
    numero = 1000 + (i % 9000)
    placa = f"{l1}{l2}{l3}{numero}"
    _placas_usadas.add(placa)
    return placa


_cpfs_gerados = 0


def proximo_cpf() -> str:
    global _cpfs_gerados
    _cpfs_gerados += 1
    base = str(111222333 + _cpfs_gerados * 137).rjust(9, "0")[-9:]
    if len(set(base)) == 1:
        base = base[:-1] + ("0" if base[-1] != "0" else "1")
    return cpf_valido(base)


_renavams_gerados = 0


def proximo_renavam() -> str:
    global _renavams_gerados
    _renavams_gerados += 1
    base = str(1000000000 + _renavams_gerados * 987).rjust(10, "0")[-10:]
    return renavam_valido(base)


def cnpj_valido(base12: str) -> str:
    """Recebe 12 dígitos e devolve o CNPJ de 14 dígitos com DVs corretos
    (mesmo algoritmo de Domain/ValueObjects/Cnpj.cs)."""
    nums = [int(c) for c in base12]
    m1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]
    m2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]

    soma1 = sum(nums[i] * m1[i] for i in range(12))
    r1 = soma1 % 11
    d1 = 0 if r1 < 2 else 11 - r1

    nums13 = nums + [d1]
    soma2 = sum(nums13[i] * m2[i] for i in range(13))
    r2 = soma2 % 11
    d2 = 0 if r2 < 2 else 11 - r2

    return base12 + str(d1) + str(d2)


_cnpjs_gerados = 0


def proximo_cnpj() -> str:
    global _cnpjs_gerados
    _cnpjs_gerados += 1
    base = str(110000000000 + _cnpjs_gerados * 9137).rjust(12, "0")[-12:]
    if len(set(base)) == 1:
        base = base[:-1] + ("0" if base[-1] != "0" else "1")
    return cnpj_valido(base)


_skus_gerados = 0


def proximo_sku(prefixo: str) -> str:
    global _skus_gerados
    _skus_gerados += 1
    return f"{prefixo}-{1000 + _skus_gerados}"


def codigo_barras_valido() -> str:
    # EAN-13 simplificado (13 dígitos) — Componente só valida "só dígitos,
    # 8 a 14 caracteres", sem checksum, então sequência numérica já basta.
    return str(random.randint(10**12, 10**13 - 1))


def ncm_redondo() -> str:
    return str(random.randint(10**7, 10**8 - 1))


def telefone_valido() -> str:
    ddd = random.choice(["11", "21", "31", "41", "51", "61", "71", "81"])
    return ddd + "9" + str(random.randint(10000000, 99999999))


def cep_redondo() -> str:
    return f"{random.randint(1, 99):02d}{random.randint(0, 9)}00000"


def data_aleatoria(meses_min: int, meses_max: int) -> str:
    """Data dentro da janela [hoje - meses_max, hoje - meses_min], dia e hora
    também sorteados dentro do mês (redondo na hora — sempre em ponto — mas
    sem cair todo mundo no dia 1, que deixava os pontos do gráfico mensal
    meio robóticos)."""
    meses_atras = random.randint(meses_min, meses_max)
    ano = HOJE.year
    mes = HOJE.month - meses_atras
    while mes <= 0:
        mes += 12
        ano -= 1
    dia = random.randint(1, 28)
    hora = random.choice([8, 9, 10, 11, 13, 14, 15, 16, 17])
    return datetime(ano, mes, dia, hora, 0, 0).isoformat()


def data_estratificada_dt(indice: int, meses_max: int = MESES_OPERACAO) -> datetime:
    """Mesma ideia de data_estratificada(), mas devolve datetime em vez de
    string — usada quando outra data precisa ser derivada dela (ex.:
    dataAprovacao = dataCriacao + alguns dias, nunca o contrário)."""
    meses_atras = indice % meses_max
    ano = HOJE.year
    mes = HOJE.month - meses_atras
    while mes <= 0:
        mes += 12
        ano -= 1
    dia = random.randint(1, 28)
    hora = random.choice([8, 9, 10, 11, 13, 14, 15, 16, 17])
    return datetime(ano, mes, dia, hora, 0, 0)


def data_estratificada(indice: int, meses_max: int = MESES_OPERACAO) -> str:
    """Distribui registros ciclicamente pelos últimos `meses_max` meses via
    `indice % meses_max` — GARANTE que todo mês do período tenha pelo menos
    um registro (diferente de data_aleatoria(), que sorteia o mês livremente
    e pode, por azar, deixar algum mês zerado quando o volume não é grande o
    bastante). Dia e hora dentro do mês continuam aleatórios."""
    return data_estratificada_dt(indice, meses_max).isoformat()


def ciclo_meses(tamanho: int, meses_max: int = MESES_OPERACAO) -> list:
    """Lista de índices de mês (0..meses_max-1) com `tamanho` posições,
    construída embaralhando blocos completos de meses_max.

    Por que não um índice sequencial simples (`i % meses_max`): quando um
    laço aninhado gera exatamente meses_max itens por "rodada" pra um
    cenário (ex.: 24 propostas datadas por rodada, mesmo tanto que
    MESES_OPERACAO), `i % meses_max` cai SEMPRE nos mesmos meses em toda
    rodada — cada subcenário (ex. "concluída à vista") fica grudado nos
    mesmos 2-3 meses pra sempre, nunca aparecendo no mês atual. Embaralhar
    cada bloco quebra esse alinhamento: mesma garantia de cobertura (todo
    bloco tem os MESES_OPERACAO meses uma vez), mas em ordem diferente a cada volta."""
    sequencia = []
    while len(sequencia) < tamanho:
        bloco = list(range(meses_max))
        random.shuffle(bloco)
        sequencia.extend(bloco)
    return sequencia[:tamanho]


def apos(data_base: datetime, dias_min: int, dias_max: int) -> str:
    """Data alguns dias depois de `data_base` (ex.: aprovação depois da
    criação) — nunca cai no futuro, capada em HOJE."""
    resultado = data_base + timedelta(days=random.randint(dias_min, dias_max))
    return min(resultado, HOJE).isoformat()


# =====================================================================
# CATÁLOGO DE DADOS
# =====================================================================

NOMES = [
    "Ana Silva", "Bruno Costa", "Carla Souza", "Diego Alves", "Elaine Ferreira",
    "Fábio Lima", "Gabriela Rocha", "Hugo Martins", "Isabela Ribeiro", "João Pereira",
    "Karina Gomes", "Lucas Barbosa", "Mariana Dias", "Nelson Cardoso", "Olivia Teixeira",
    "Paulo Nunes", "Queila Araujo", "Rafael Correia", "Sabrina Melo", "Tiago Duarte",
    "Ursula Farias", "Vinicius Moraes", "Wagner Pinto", "Yasmin Castro", "Zeca Monteiro",
    "Alice Fontes", "Breno Xavier", "Camila Prado", "Danilo Reis", "Eduarda Lopes",
    "Felipe Cunha", "Giovana Brito", "Henrique Sales", "Ingrid Machado", "Julio Vieira",
    "Larissa Moura", "Marcelo Tavares", "Natalia Peixoto", "Otavio Ramos", "Priscila Andrade",
]

CIDADES_UF = [
    ("São Paulo", "SP"), ("Campinas", "SP"), ("Santos", "SP"), ("Rio de Janeiro", "RJ"),
    ("Niterói", "RJ"), ("Belo Horizonte", "MG"), ("Uberlândia", "MG"), ("Curitiba", "PR"),
    ("Londrina", "PR"), ("Porto Alegre", "RS"), ("Salvador", "BA"), ("Recife", "PE"),
]

VEICULOS_CATALOGO = [
    ("Fiat", "Argo", "1.3"), ("Fiat", "Toro", "2.0"), ("Fiat", "Pulse", "1.0"),
    ("Volkswagen", "Polo", "1.0"), ("Volkswagen", "T-Cross", "1.4"), ("Volkswagen", "Nivus", "1.0"),
    ("Chevrolet", "Onix", "1.0"), ("Chevrolet", "Tracker", "1.2"), ("Chevrolet", "S10", "2.8"),
    ("Toyota", "Corolla", "2.0"), ("Toyota", "Hilux", "2.8"), ("Toyota", "Yaris", "1.5"),
    ("Honda", "Civic", "2.0"), ("Honda", "HR-V", "1.5"), ("Honda", "City", "1.5"),
    ("Hyundai", "HB20", "1.0"), ("Hyundai", "Creta", "1.6"), ("Renault", "Kwid", "1.0"),
    ("Jeep", "Compass", "1.3"), ("Nissan", "Kicks", "1.6"),
]
CORES = ["Branco", "Prata", "Preto", "Cinza", "Vermelho", "Azul", "Bege"]

TIPOS_OS = ["Manutencao", "Revisao", "TrocaPecas", "Diagnostico", "Outro"]
DESCRICOES_OS = {
    "Manutencao": [
        "Barulho estranho ao frear", "Vazamento de óleo identificado",
        "Suspensão fazendo ruído em buracos", "Vibração no volante em alta velocidade",
    ],
    "Revisao": [
        "Revisão programada dos 10.000 km", "Revisão programada dos 20.000 km",
        "Revisão programada dos 40.000 km", "Revisão pré-viagem",
    ],
    "TrocaPecas": [
        "Troca de pastilhas e disco de freio", "Troca de correia dentada",
        "Troca de amortecedores dianteiros", "Troca de bateria",
    ],
    "Diagnostico": [
        "Luz de injeção acesa no painel", "Carro não liga pela manhã",
        "Consumo elevado de combustível", "Ar-condicionado não gela",
    ],
    "Outro": [
        "Instalação de acessório solicitado pelo cliente", "Polimento e higienização interna",
        "Aplicação de insulfilm", "Instalação de multimídia",
    ],
}
MODOS_PAGAMENTO_OS = ["Dinheiro", "Pix", "CartaoDebito", "CartaoCredito", "Transferencia"]
MODOS_PAGAMENTO_PROPOSTA = ["Pix", "Transferencia", "Boleto"]  # PropostaVenda não aceita Dinheiro/cartão

MOTIVOS_REJEICAO = [
    "Cliente desistiu da compra.", "Cliente optou por outro veículo na concorrência.",
    "Financiamento do cliente não foi aprovado por fora.", "Cliente pediu mais tempo e não retornou.",
]

# Cenário "financiamentoNegado": a financiadora (contatada por fora do
# sistema) recusa financiar o cliente/veículo depois de avaliar os dados que
# o vendedor repassou — distinto de MOTIVOS_REJEICAO porque passa de fato
# pelo status AguardandoFinanciadora antes de ser negado.
MOTIVOS_NEGATIVA_FINANCIADORA = [
    "Cliente com restrição no CPF (SPC/Serasa).",
    "Renda comprovada insuficiente para o valor solicitado.",
    "Documentação incompleta para análise de crédito.",
]

MOTIVOS_CANCELAMENTO_CONSIGNACAO = [
    "Proprietário decidiu vender o veículo por conta própria.",
    "Proprietário retirou o veículo para uso pessoal.",
    "Desacordo sobre o valor mínimo de venda.",
    "Proprietário optou por consignar em outra loja.",
]

# Catálogo de componentes — 11 sistemas do veículo (Domain/Enums/.../SistemaComponente.cs)
# x peças típicas de cada um. "Muitos componentes no estoque" pedido explicitamente.
COMPONENTES_POR_SISTEMA = {
    "Motor": [
        ("Filtro de óleo", "Filtro"), ("Filtro de ar do motor", "Filtro"),
        ("Vela de ignição", "Ignição"), ("Correia dentada", "Correia"),
        ("Bomba de combustível", "Combustível"), ("Óleo do motor 5W30 (litro)", "Lubrificante"),
        ("Junta do cabeçote", "Vedação"), ("Sensor de oxigênio", "Sensor"),
    ],
    "Suspensao": [
        ("Amortecedor dianteiro", "Amortecedor"), ("Amortecedor traseiro", "Amortecedor"),
        ("Mola helicoidal", "Mola"), ("Bandeja de suspensão", "Bandeja"),
        ("Bucha de suspensão", "Bucha"), ("Coxim do motor", "Coxim"),
    ],
    "Direcao": [
        ("Terminal de direção", "Terminal"), ("Bomba de direção hidráulica", "Bomba"),
        ("Caixa de direção", "Caixa"), ("Barra de direção", "Barra"),
    ],
    "Freios": [
        ("Pastilha de freio dianteira", "Pastilha"), ("Pastilha de freio traseira", "Pastilha"),
        ("Disco de freio dianteiro", "Disco"), ("Disco de freio traseiro", "Disco"),
        ("Fluido de freio DOT4 (litro)", "Fluido"), ("Cilindro mestre de freio", "Cilindro"),
    ],
    "Eletrica": [
        ("Bateria 60Ah", "Bateria"), ("Alternador", "Alternador"),
        ("Motor de arranque", "Motor"), ("Lâmpada de farol H4", "Lâmpada"),
        ("Chicote elétrico", "Chicote"), ("Fusível 15A (caixa)", "Fusível"),
    ],
    "Lataria": [
        ("Para-choque dianteiro", "Para-choque"), ("Para-choque traseiro", "Para-choque"),
        ("Retrovisor externo", "Retrovisor"), ("Maçaneta externa", "Maçaneta"),
    ],
    "Transmissão": [
        ("Kit de embreagem", "Embreagem"), ("Óleo de câmbio (litro)", "Lubrificante"),
        ("Junta homocinética", "Junta"), ("Coxim do câmbio", "Coxim"),
    ],
    "Arrefecimento": [
        ("Radiador", "Radiador"), ("Bomba d'água", "Bomba"),
        ("Válvula termostática", "Válvula"), ("Aditivo para radiador (litro)", "Aditivo"),
        ("Mangueira do radiador", "Mangueira"),
    ],
    "Escapamento": [
        ("Catalisador", "Catalisador"), ("Silencioso traseiro", "Silencioso"),
        ("Tubo de escape intermediário", "Tubo"),
    ],
    "Acessorios": [
        ("Tapete automotivo (jogo)", "Tapete"), ("Capa de banco", "Capa"),
        ("Suporte de celular veicular", "Suporte"), ("Câmera de ré", "Câmera"),
    ],
    "Interior": [
        ("Kit de acabamento de painel", "Acabamento"), ("Manopla de câmbio", "Manopla"),
        ("Cinto de segurança", "Cinto"), ("Forração de teto", "Forração"),
    ],
}

MARCAS_COMPONENTES = ["Bosch", "Fras-le", "Continental", "NGK", "Cofap", "TRW", "Mahle", "Valeo", "Original"]

# Presets de checklist reutilizáveis entre OSs — o recepcionista escolhe um
# (ou nenhum) na criação, e vira snapshot na OS. Cobre os cenários mais
# comuns de oficina pra variar o que aparece em cada tipo de serviço.
CHECKLIST_PRESETS = [
    ("Revisão Geral", [
        "Verificar nível de óleo do motor", "Verificar nível do fluido de freio",
        "Checar pressão e desgaste dos pneus", "Inspecionar pastilhas e discos de freio",
        "Testar carga da bateria", "Verificar funcionamento de luzes e setas",
    ]),
    ("Troca de Óleo e Filtros", [
        "Drenar óleo usado", "Substituir filtro de óleo", "Substituir filtro de ar do motor",
        "Verificar vazamentos após a troca", "Reabastecer com óleo novo na especificação correta",
        "Resetar lembrete de manutenção do painel",
    ]),
    ("Diagnóstico Elétrico", [
        "Ler códigos de falha na central eletrônica", "Testar alternador sob carga",
        "Testar bateria e terminais", "Verificar fiação e conectores da região afetada",
        "Testar sistema de ignição",
    ]),
    ("Freios e Suspensão", [
        "Inspecionar pastilhas e discos de freio", "Verificar nível e condição do fluido de freio",
        "Checar amortecedores dianteiros e traseiros", "Inspecionar buchas e batentes de suspensão",
        "Testar alinhamento e geometria",
    ]),
    ("Pré-Viagem", [
        "Verificar pneus e estepe", "Checar níveis de todos os fluidos", "Testar eficiência dos freios",
        "Verificar funcionamento de todas as luzes", "Checar ar-condicionado",
    ]),
    ("Troca de Correia Dentada", [
        "Verificar tensor e polias antes da troca", "Substituir correia dentada",
        "Substituir bomba d'água (recomendado no mesmo serviço)", "Verificar alinhamento do comando de válvulas",
        "Testar motor após a montagem",
    ]),
]

# Templates de documento (Configurações > Documentos) — presets reais usados
# pelos seletores de template espalhados pelo sistema (consignação, termo de
# entrega, resposta de financiadora). Não são dados randomizados como o resto
# do dataset: é um conjunto fixo pequeno, pensado pra já vir utilizável.
TEMPLATES_DOCUMENTO = [
    ("Termo de entrega (padrão)", (
        "TERMO DE ENTREGA DE VEÍCULO\n\n"
        "Pelo presente termo, [NOME DA LOJA], entrega ao(à) Sr(a). [NOME DO CLIENTE], "
        "portador(a) do CPF nº [CPF DO CLIENTE], o veículo [MARCA E MODELO], ano [ANO], "
        "placa [PLACA], quilometragem [QUILOMETRAGEM] km, pelo valor de R$ [VALOR].\n\n"
        "O(A) comprador(a) declara ter vistoriado o veículo no ato da entrega e recebido "
        "manual, [NÚMERO] chave(s) e documentação de transferência.\n\n"
        "Local e data: [CIDADE], [DATA]"
    )),
    ("Contrato de consignação (padrão)", (
        "CONTRATO DE CONSIGNAÇÃO PARA VENDA DE VEÍCULO\n\n"
        "[NOME DA LOJA], CONSIGNATÁRIA, e [NOME DO PROPRIETÁRIO], CPF [CPF DO PROPRIETÁRIO], "
        "CONSIGNANTE, acordam a consignação do veículo [MARCA E MODELO], ano [ANO], "
        "placa [PLACA], pelo prazo de [PRAZO EM DIAS] dias.\n\n"
        "Valor esperado pelo proprietário: R$ [VALOR ESPERADO]. Comissão da consignatária: "
        "[PERCENTUAL]% sobre o valor da venda.\n\n"
        "Local e data: [CIDADE], [DATA]"
    )),
    ("Resposta da financiadora (roteiro)", (
        "Financiadora: [NOME DA FINANCIADORA]\n"
        "Contato: [NOME DO ATENDENTE / TELEFONE / E-MAIL]\n"
        "Data do retorno: [DATA]\n\n"
        "Condições propostas:\n"
        "- Valor financiado: R$ [VALOR]\n"
        "- Número de parcelas: [PARCELAS]\n"
        "- Valor aproximado da parcela: R$ [VALOR DA PARCELA]\n"
        "- Taxa de juros informada: [TAXA]\n\n"
        "Observações da financiadora: [CONDIÇÕES ADICIONAIS]"
    )),
]


def gerar_templates_documento():
    templates = []
    seq = id_seq("tpl")
    for nome, conteudo in TEMPLATES_DOCUMENTO:
        templates.append({"chave": next(seq), "nome": nome, "conteudo": conteudo})
    return templates


def id_seq(prefixo):
    n = 0
    while True:
        n += 1
        yield f"{prefixo}-{n}"


# =====================================================================
# GERAÇÃO
# =====================================================================

def gerar_usuarios():
    usuarios = []
    contagens = [
        ("Vendedor", 5), ("Mecanico", 8), ("Recepcionista", 3),
        ("ChefeOficina", 2), ("GerenteVendas", 2), ("Admin", 1),
    ]
    especialidades = ["Motor", "Freios", "Suspensao", "Eletrica", "ArCondicionado"]
    niveis = ["Junior", "Pleno", "Senior"]
    seq = id_seq("usr")

    for tipo, qtd in contagens:
        for i in range(qtd):
            chave = next(seq)
            nome = f"{NOMES[len(usuarios) % len(NOMES)]} ({tipo})"
            usuario = {
                "chave": chave,
                "tipo": tipo,
                "nome": nome,
                "email": f"{chave}@carstore.com.br",
                "telefone": telefone_valido(),
                "senha": "Teste@123",
            }
            if tipo != "Admin":
                usuario["nivel"] = niveis[i % len(niveis)]
                usuario["dataContratacao"] = data_aleatoria(4, 36)
            if tipo == "Mecanico":
                usuario["especialidade"] = especialidades[i % len(especialidades)]
            usuarios.append(usuario)

    return usuarios


def gerar_clientes(qtd=40):
    clientes = []
    seq = id_seq("cli")
    for i in range(qtd):
        chave = next(seq)
        nome = NOMES[i % len(NOMES)]
        cidade, uf = random.choice(CIDADES_UF)
        clientes.append({
            "chave": chave,
            "nome": nome,
            "cpf": proximo_cpf(),
            "telefone": telefone_valido(),
            "email": f"{chave}@emaildemo.com.br",
            "endereco": {
                "logradouro": f"Rua {nome.split()[0]} {random.choice(['Silva', 'Santos', 'Lima', 'Souza'])}",
                "numero": str(random.randint(10, 2000)),
                "complemento": "" if i % 3 else f"Apto {random.randint(11, 909)}",
                "bairro": random.choice(["Centro", "Jardins", "Vila Nova", "Boa Vista", "Alto da Serra"]),
                "cidade": cidade,
                "uf": uf,
                "cep": cep_redondo(),
            },
            "dataCriacao": data_estratificada(i),
        })
    return clientes


def gerar_fornecedores(qtd=12):
    """Fornecedores de peças automotivas — todo componente precisa referenciar
    um pela chave (FornecedorChave), então essa seção roda antes."""
    fornecedores = []
    seq = id_seq("forn")
    nomes = [
        "Auto Peças Bosch Distribuidora", "Fras-le Comércio de Autopeças", "Continental Peças Ltda",
        "NGK Distribuidora Automotiva", "Cofap Peças e Acessórios", "TRW Componentes Automotivos",
        "Mahle Distribuidora de Peças", "Valeo Peças do Brasil", "Central de Peças Original",
        "Peças & Cia Distribuidora", "Grupo Automotivo Peninsular", "Distribuidora Nacional de Autopeças",
    ]
    for i in range(qtd):
        chave = next(seq)
        cidade, uf = random.choice(CIDADES_UF)
        fornecedores.append({
            "chave": chave,
            "nome": nomes[i % len(nomes)],
            "cnpj": proximo_cnpj(),
            "email": f"{chave}@fornecedordemo.com.br",
            "telefone": telefone_valido(),
            "endereco": {
                "logradouro": "Avenida Industrial",
                "numero": str(random.randint(100, 5000)),
                "complemento": "",
                "bairro": "Distrito Industrial",
                "cidade": cidade,
                "uf": uf,
                "cep": cep_redondo(),
            },
        })
    return fornecedores


def gerar_checklist_presets():
    presets = []
    seq = id_seq("chk")
    for nome, itens in CHECKLIST_PRESETS:
        presets.append({"chave": next(seq), "nome": nome, "itens": itens})
    return presets


# Catálogo de despesas fixas mensais — sem data própria (são um valor "atual"
# cadastrado, não lançamentos pontuais), por isso não entram na estratificação
# de MESES_OPERACAO meses. Setor Geral é compartilhado; Oficina/Concessionária ficam
# equilibradas em quantidade e variedade de Tipo, pro dashboard de cada setor
# ter dado real pra mostrar.
#
# Valores calibrados pro volume de receita que o dataset consegue gerar POR
# MÊS (poucas dezenas de OS finalizadas e 1-2 vendas concluídas, não uma rede
# com dezenas de vendedores) — não pra folha de pagamento real de mercado.
# Com despesas em escala de salário real (o catálogo anterior somava quase
# R$95mil/mês), qualquer mês sem uma venda de veículo caindo exatamente nele
# aparecia com prejuízo de dezenas de milhares, dando a falsa impressão de
# loja fracassada. Nessa escala menor, mesmo um mês fraco fica só levemente
# negativo, e um mês com 1 venda concluída (comum, dado o volume gerado) já
# fecha no azul — fluxo de caixa plausível pra demonstração.
DESPESAS_CATALOGO = [
    # (nome, setor, tipo, valor_base)
    ("Aluguel do prédio principal", "Geral", "Aluguel", 2200),
    ("Conta de luz", "Geral", "Utilidades", 420),
    ("Conta de água", "Geral", "Utilidades", 160),
    ("Internet e telefonia", "Geral", "Utilidades", 180),
    ("Contabilidade terceirizada", "Geral", "Servicos", 550),
    ("Licença do sistema de gestão", "Geral", "Servicos", 220),
    ("Seguro predial", "Geral", "Seguro", 280),
    ("Marketing institucional (site e redes sociais)", "Geral", "Marketing", 400),
    ("Impostos e taxas municipais", "Geral", "Impostos", 650),
    ("Limpeza e conservação", "Geral", "Servicos", 300),
    ("Salários da equipe de mecânicos", "Oficina", "Salario", 4200),
    ("Salário do chefe de oficina", "Oficina", "Salario", 1900),
    ("Aluguel do galpão da oficina", "Oficina", "Aluguel", 1200),
    ("Manutenção de equipamentos e ferramentas", "Oficina", "Manutencao", 380),
    ("Seguro de responsabilidade civil da oficina", "Oficina", "Seguro", 220),
    ("Investimento em novas ferramentas", "Oficina", "Investimento", 550),
    ("Uniformes e EPIs", "Oficina", "Outros", 150),
    ("Marketing da oficina (panfletos e parcerias)", "Oficina", "Marketing", 200),
    ("Salários da equipe de vendas", "Concessionaria", "Salario", 4600),
    ("Salário do gerente de vendas", "Concessionaria", "Salario", 2100),
    ("Aluguel do showroom", "Concessionaria", "Aluguel", 1800),
    ("Marketing digital de veículos (anúncios)", "Concessionaria", "Marketing", 950),
    ("Seguro da frota em estoque", "Concessionaria", "Seguro", 500),
    ("Comissões de despachante/documentação", "Concessionaria", "Servicos", 400),
    ("Manutenção e decoração do showroom", "Concessionaria", "Manutencao", 300),
    ("Fotos e vídeos profissionais dos veículos", "Concessionaria", "Investimento", 250),
]


def gerar_despesas():
    return [
        {
            "nome": nome,
            "valor": valor_redondo(int(base * 0.9), int(base * 1.15), 50),
            "setor": setor,
            "tipo": tipo,
        }
        for nome, setor, tipo, base in DESPESAS_CATALOGO
    ]


# Eventos extraordinários espalhados pelo histórico — o que faz um mês
# "gastar mais" além das despesas básicas recorrentes: compra/troca de
# equipamento da oficina, reforma, ou a perda de um veículo próprio (motor
# fundido). Não inclui "compra de veículo para concessionária" — isso já
# é lançado automaticamente pelo próprio sistema quando o veículo é
# cadastrado (VeiculoVendaService.AddAsync), então cada um dos ~110 veículos
# gerados já cria sozinho sua linha de despesa na competência certa. Meses
# que NÃO aparecem aqui ficam só com o básico recorrente (nenhum balanço
# especial é criado pra eles — o dashboard cai no valor do modelo).
DESPESAS_EXTRAS_CATALOGO = [
    # (meses_atras, nome, setor, categoria, valor)
    (34, "Troca do elevador hidráulico principal da oficina", "Oficina", "Investimento", 8500),
    (30, "Motor fundido — troca de motor do veículo de apoio da loja", "Concessionaria", "Manutencao", 9800),
    (26, "Reforma do telhado do galpão da oficina", "Geral", "Manutencao", 6200),
    (22, "Compra de scanner automotivo novo", "Oficina", "Investimento", 4200),
    (18, "Recall do fabricante — troca do compressor de ar-condicionado da oficina", "Oficina", "Manutencao", 2100),
    (15, "Reforma do showroom da concessionária", "Concessionaria", "Investimento", 7300),
    (11, "Substituição de elevador de 2 colunas", "Oficina", "Investimento", 6700),
    (8, "Manutenção corretiva do gerador de energia", "Geral", "Manutencao", 1850),
    (5, "Troca do compressor de ar do setor de pintura", "Oficina", "Manutencao", 3400),
    (2, "Reparo emergencial no sistema elétrico do galpão", "Geral", "Manutencao", 2600),
]


def gerar_despesas_extras():
    return [
        {
            "data": data_estratificada_dt(meses_atras).isoformat(),
            "nome": nome,
            "setor": setor,
            "categoria": categoria,
            "valor": valor,
        }
        for meses_atras, nome, setor, categoria, valor in DESPESAS_EXTRAS_CATALOGO
    ]


def gerar_reposicoes_estoque(quantidade_total):
    """Divide a quantidade final de estoque em 1-3 reposições históricas
    espalhadas pelos MESES_OPERACAO meses, em vez de uma entrada só "hoje"
    com a quantidade toda — cada reposição vira sua própria despesa
    "Compra de componentes" na competência certa (ver ImportacaoDadosService
    .ImportarComponentesAsync), em vez de empilhar o custo de compra de
    todo o catálogo no dia da importação. Ver docs/redesign/18-despesa-
    compra-componente.md."""
    if quantidade_total <= 0:
        return []
    n_entregas = 1 if quantidade_total <= 3 else random.randint(2, 3)
    meses = random.sample(range(MESES_OPERACAO), k=n_entregas)

    partes = []
    restante = quantidade_total
    for i in range(n_entregas - 1):
        parte = max(1, restante // (n_entregas - i))
        partes.append(parte)
        restante -= parte
    partes.append(restante)

    return [
        {"data": data_estratificada(mes), "quantidade": qtd}
        for mes, qtd in zip(meses, partes)
    ]


def gerar_componentes(fornecedores_chaves):
    """Um bom número de peças por sistema, com preço/margem/estoque variados
    (inclusive alguns propositalmente abaixo do mínimo, pra mostrar o alerta
    de estoque baixo no dashboard)."""
    componentes = []
    seq = id_seq("comp")

    for sistema, itens in COMPONENTES_POR_SISTEMA.items():
        for nome, categoria in itens:
            chave = next(seq)
            # Faixas calibradas pra o VALOR TOTAL do estoque (custo × quantidade,
            # somado por todos os componentes) ficar realista pra uma oficina
            # pequena/média — algo entre R$25 mil e R$50 mil no total, nunca
            # centenas de milhares (o dono sinalizou que 100 mil já é alto
            # demais pra ser normal). Peça isolada custa pouco; é o volume de
            # SKUs diferentes que soma, não estoque gigante de cada uma.
            custo = valor_redondo(15, 150, 5)
            estoque_baixo = random.random() < 0.15  # ~15% dos itens já perto de faltar
            minimo = valor_redondo(2, 6, 1)
            estoque = random.randint(0, minimo - 1) if estoque_baixo else valor_redondo(minimo, 20, 1)

            componentes.append({
                "chave": chave,
                "fornecedorChave": random.choice(fornecedores_chaves),
                "skuInterno": proximo_sku("SKU"),
                "nome": nome,
                "descricao": f"{nome} — peça de reposição, sistema {sistema.lower()}.",
                "marcaFabricante": random.choice(MARCAS_COMPONENTES),
                "partNumber": proximo_sku("PN"),
                "codigoBarras": codigo_barras_valido(),
                "ncm": ncm_redondo(),
                "categoria": categoria,
                "unidade": "UN",
                "sistema": sistema,
                "peso": round(random.uniform(0.1, 15.0), 1),
                "garantiaDias": random.choice([90, 180, 365]),
                "custoUnitario": custo,
                "margemLucroPct": random.choice([20, 25, 30, 35, 40, 45, 50]),
                "reposicoesEstoque": gerar_reposicoes_estoque(estoque),
                "quantidadeMinima": minimo,
            })

    return componentes


def gerar_veiculos_venda(qtd_disponiveis, qtd_em_preparacao=10):
    """Devolve (lista_json, lista_de_chaves_disponiveis) — uma entrada por
    proposta que vai precisar de um veículo "reservável", mais alguns extras
    só pra mostrar o estoque em preparação.

    Cadastrar um veículo lança automaticamente uma despesa de "compra de
    veículo" na competência da dataCriacao (ver VeiculoVendaService
    .AddAsync). Duas rodadas de ajuste já passaram por aqui: primeiro
    (~60% dos meses recebiam um LOTE de 6-10 carros de uma vez), depois
    peso aleatório 2,2x pra ~30% dos meses "pico" — ambas ainda deixavam
    variância grande demais: qualquer janela de 30 dias podia cair bem em
    cima de um mês de pico (ver docs/redesign/19-bug-margem-mensal-
    negativa.md — uma janela real chegou a -336% de margem só por causa de
    disso). Agora usa `ciclo_meses`, o mesmo mecanismo já usado pras
    propostas — cada mês recebe uma contagem quase idêntica de compras
    (distribuição determinística, não aleatória), o que reduz a amplitude
    do "dente de serra" sem eliminar toda variação (o VALOR de cada compra
    ainda varia — carros diferentes, preços diferentes)."""
    veiculos = []
    chaves_disponiveis = []
    seq = id_seq("vv")

    ciclo_compra = iter(ciclo_meses(qtd_disponiveis))

    total = qtd_disponiveis + qtd_em_preparacao
    for i in range(total):
        chave = next(seq)
        marca, modelo, motor = random.choice(VEICULOS_CATALOGO)
        ano = random.randint(2016, HOJE.year)
        cenario = "disponivel" if i < qtd_disponiveis else "emPreparacao"
        acessorios_possiveis = ["ArCondicionado", "VidrosEletricos", "DirecaoHidraulica", "TetoSolar",
                                 "BancoCouro", "CameraRe", "SensorEstacionamento", "CentralMultimidia",
                                 "Alarme", "RodaLiga", "Bluetooth"]
        valor = valor_redondo(38000, 220000, 500)
        veiculo = {
            "chave": chave,
            "marca": marca,
            "modelo": modelo,
            "cor": random.choice(CORES),
            "motorizacao": motor,
            "ano": ano,
            "quilometragem": valor_redondo(0, 140000, 500),
            "placa": proxima_placa(),
            "renavam": proximo_renavam(),
            "cambio": random.choice(["Manual", "Automatico"]),
            "combustivel": random.choice(["Flex", "Gasolina", "Diesel", "Hibrido"]),
            "valor": valor,
            # Valor de aquisição obrigatório — a concessionária compra abaixo do
            # preço de venda (margem de 10% a 30% sobre o custo de compra).
            "valorAquisicao": valor_redondo(int(valor * 0.7), int(valor * 0.9), 500),
            "acessorios": random.sample(acessorios_possiveis, k=random.randint(2, 6)),
            "anoUltimoIpvaPago": random.randint(ano, HOJE.year),
            "textoTermoPreliminar": "Veículo vendido no estado em que se encontra, conforme vistoria realizada na entrega.",
            # "disponivel" segue o ciclo de meses quase uniforme (ver
            # ciclo_compra acima); "emPreparacao" é recente, mas espalhada
            # pelos últimos ~6 meses (não só os últimos 2) — concentrar
            # poucos meses recentes empurrava demais a despesa de compra
            # bem no período que qualquer usuário vê primeiro ("últimos 30
            # dias"), distorcendo justo a janela mais olhada (ver
            # docs/redesign/19-bug-margem-mensal-negativa.md).
            "dataCriacao": data_estratificada(next(ciclo_compra)) if cenario == "disponivel" else data_aleatoria(0, 5),
            "cenario": cenario,
        }
        veiculos.append(veiculo)
        if cenario == "disponivel":
            chaves_disponiveis.append(chave)

    return veiculos, chaves_disponiveis


def gerar_veiculos_consignados(clientes_chaves, vendedores_chaves):
    """4 cenários x 2 tipos de comissão, repetido REPETICOES_CONSIGNACAO vezes
    com valores/clientes diferentes a cada rodada."""
    consignacoes = []
    seq = id_seq("vc")
    cenarios = ["ativa", "vendidaAguardando", "concluida", "devolvida", "cancelada"]
    tipos_comissao = ["Fixo", "Porcentagem"]
    # 8 consignações não-"ativa" por rodada (4 cenários x 2 tipos) — ciclo
    # embaralhado evita que cada cenário grude sempre nos mesmos meses.
    ciclo = iter(ciclo_meses(8 * REPETICOES_CONSIGNACAO))

    for _ in range(REPETICOES_CONSIGNACAO):
        for cenario in cenarios:
            for tipo_comissao in tipos_comissao:
                chave = next(seq)
                marca, modelo, motor = random.choice(VEICULOS_CATALOGO)
                valor_venda = valor_redondo(22000, 190000, 500)
                consignacao = {
                    "chave": chave,
                    "marca": marca,
                    "modelo": modelo,
                    "cor": random.choice(CORES),
                    "motorizacao": motor,
                    "ano": random.randint(2014, HOJE.year - 1),
                    "quilometragem": valor_redondo(5000, 160000, 500),
                    "placa": proxima_placa(),
                    "renavam": proximo_renavam(),
                    "cambio": random.choice(["Manual", "Automatico"]),
                    "combustivel": random.choice(["Flex", "Gasolina", "Diesel"]),
                    "clienteProprietarioChave": random.choice(clientes_chaves),
                    "vendedorResponsavelChave": random.choice(vendedores_chaves),
                    "tipoComissao": tipo_comissao,
                    "valorVendaEsperado": valor_venda,
                    "textoContrato": "Contrato de consignação: o proprietário autoriza a concessionária a intermediar a venda do veículo pelo prazo estipulado.",
                    "prazoDias": random.choice([60, 90, 120, 150, 180]),
                    "cenario": cenario,
                }
                # "ativa" fica enviesada pro período recente (pra ter contratos
                # de fato dentro do prazo hoje) e não consome índice — só os
                # cenários já resolvidos usam o contador, senão a estratificação
                # pulava meses (o índice avançava sem nunca cair naquele mês).
                if cenario == "ativa":
                    consignacao["dataCriacao"] = data_aleatoria(0, 8)
                else:
                    consignacao["dataCriacao"] = data_estratificada(next(ciclo))
                if cenario == "cancelada":
                    consignacao["motivoCancelamento"] = random.choice(MOTIVOS_CANCELAMENTO_CONSIGNACAO)
                if tipo_comissao == "Fixo":
                    consignacao["valorFixoProprietario"] = valor_redondo(int(valor_venda * 0.7), int(valor_venda * 0.9), 500)
                else:
                    consignacao["porcentagemProprietario"] = random.choice([75, 80, 85, 90])
                consignacoes.append(consignacao)

    return consignacoes


def gerar_test_drives(veiculos_disponiveis_chaves, clientes_chaves, vendedores_chaves):
    """Cenários: agendado (minoria, sempre no futuro próximo — compromisso de
    verdade pra aparecer na agenda, nunca backdatado), cancelado e
    naoCompareceu (minoria, espalhados pelo histórico) e realizado (maioria,
    domina o volume — é o que normalmente leva a uma proposta de venda
    depois). Mesmo padrão "minoria pendente / maioria concluída" já usado em
    propostas e OS."""
    test_drives = []
    seq = id_seq("td")

    def novo(cenario, mes_idx=None):
        if cenario == "agendado":
            data_hora = (HOJE + timedelta(days=random.randint(1, 14))).replace(
                hour=random.choice([9, 10, 11, 14, 15, 16]), minute=0, second=0, microsecond=0)
        else:
            data_hora = data_estratificada_dt(mes_idx)
        return {
            "chave": next(seq),
            "veiculoVendaChave": random.choice(veiculos_disponiveis_chaves),
            "clienteChave": random.choice(clientes_chaves),
            "vendedorChave": random.choice(vendedores_chaves),
            "dataHora": data_hora.isoformat(),
            "cenario": cenario,
        }

    for _ in range(REPETICOES_TESTDRIVE_PENDENTE):
        test_drives.append(novo("agendado"))

    ciclo_pendente = iter(ciclo_meses(2 * REPETICOES_TESTDRIVE_PENDENTE))
    for _ in range(REPETICOES_TESTDRIVE_PENDENTE):
        test_drives.append(novo("cancelado", next(ciclo_pendente)))
        test_drives.append(novo("naoCompareceu", next(ciclo_pendente)))

    ciclo_realizado = iter(ciclo_meses(REPETICOES_TESTDRIVE_REALIZADO))
    for _ in range(REPETICOES_TESTDRIVE_REALIZADO):
        test_drives.append(novo("realizado", next(ciclo_realizado)))

    return test_drives


def gerar_veiculos_cliente(clientes_chaves, qtd=25):
    veiculos = []
    seq = id_seq("vcli")
    for i in range(qtd):
        marca, modelo, _ = random.choice(VEICULOS_CATALOGO)
        veiculos.append({
            "chave": next(seq),
            "clienteChave": random.choice(clientes_chaves),
            "marca": marca,
            "modelo": modelo,
            "cor": random.choice(CORES),
            "ano": random.randint(2008, HOJE.year),
            "placa": proxima_placa(),
        })
    return veiculos


def gerar_propostas(veiculos_disponiveis_chaves, clientes_chaves, vendedores_chaves, valor_aquisicao_por_chave):
    """Cobertura exaustiva (todo estágio do funil x forma de pagamento onde
    isso já foi escolhido). O funil aberto (aprovada..termoEnviado) e
    rejeitada repetem pouco (REPETICOES_PROPOSTA_PENDENTE) — minoria de
    propósito, pra demonstração não parecer cheia de vendas travadas. As
    concluídas (à vista/financiada) repetem muito mais
    (REPETICOES_PROPOSTA_CONCLUIDA) — maioria do volume e da receita.

    `valorBase` era sorteado solto (`valor_redondo(38000,220000,500)`), sem
    nenhuma relação com o veículo realmente sendo vendido — resultado
    medido: quase 4 em cada 10 vendas concluídas saíam abaixo do custo de
    aquisição daquele carro (prejuízo por unidade, às vezes de -280%). Uma
    concessionária de verdade não opera assim: o preço de venda parte do
    custo de aquisição DAQUELE veículo específico, com uma margem de
    tabela — só existe prejuízo no caso raro de um problema descoberto
    depois da compra, que o comprador de carros deveria evitar ao máximo.
    `valor_base_por_veiculo` modela exatamente isso: ~96% das vendas usa
    uma margem de 12% a 32% sobre o custo; só ~4% (minoria de propósito,
    "problema oculto") cai numa faixa que pode ficar perto ou abaixo do
    custo — nunca a maioria. Ver docs/redesign/17-margem-financeira-realista.md."""
    propostas = []
    seq = id_seq("prop")
    idx_veiculo = 0

    def valor_base_por_veiculo(chave):
        aquisicao = valor_aquisicao_por_chave[chave]
        if random.random() < 0.04:
            multiplicador = random.uniform(0.85, 1.05)  # problema oculto — raro, prejuízo limitado
        else:
            multiplicador = random.uniform(1.18, 1.40)  # margem de tabela normal
        alvo = aquisicao * multiplicador
        return valor_redondo(int(alvo - 1000), int(alvo + 1000), 500)

    def proximo_veiculo():
        nonlocal idx_veiculo
        chave = veiculos_disponiveis_chaves[idx_veiculo]
        idx_veiculo += 1
        return chave

    estagios_com_modo = ["aprovada", "vistoriada", "termoRedigido", "termoEnviado"]

    # ---- Funil aberto + rejeitada: minimizado ----
    # (4 estágios x 3 modos + 4 motivos de rejeição) datados por rodada.
    dated_pendente = (len(estagios_com_modo) * len(MODOS_PAGAMENTO_PROPOSTA)
                      + len(MOTIVOS_REJEICAO) + len(MOTIVOS_NEGATIVA_FINANCIADORA))
    ciclo_pendente = iter(ciclo_meses(dated_pendente * REPETICOES_PROPOSTA_PENDENTE))

    for _ in range(REPETICOES_PROPOSTA_PENDENTE):
        for estagio in estagios_com_modo:
            for modo in MODOS_PAGAMENTO_PROPOSTA:
                veiculo_chave = proximo_veiculo()
                propostas.append({
                    "chave": next(seq),
                    "veiculoVendaChave": veiculo_chave,
                    "clienteChave": random.choice(clientes_chaves),
                    "vendedorChave": random.choice(vendedores_chaves),
                    "valorBase": valor_base_por_veiculo(veiculo_chave),
                    "descontoPercentual": random.choice([0, 3, 5, 8, 10, 12, 15]),
                    "modoPagamento": modo,
                    "dataCriacao": data_estratificada(next(ciclo_pendente)),
                    "cenario": estagio,
                })

        for motivo in MOTIVOS_REJEICAO:
            veiculo_chave = proximo_veiculo()
            propostas.append({
                "chave": next(seq),
                "veiculoVendaChave": veiculo_chave,
                "clienteChave": random.choice(clientes_chaves),
                "vendedorChave": random.choice(vendedores_chaves),
                "valorBase": valor_base_por_veiculo(veiculo_chave),
                "descontoPercentual": random.choice([0, 5, 10]),
                "motivoRejeicao": motivo,
                "dataCriacao": data_estratificada(next(ciclo_pendente)),
                "cenario": "rejeitada",
            })

        for motivo in MOTIVOS_NEGATIVA_FINANCIADORA:
            veiculo_chave = proximo_veiculo()
            propostas.append({
                "chave": next(seq),
                "veiculoVendaChave": veiculo_chave,
                "clienteChave": random.choice(clientes_chaves),
                "vendedorChave": random.choice(vendedores_chaves),
                "valorBase": valor_base_por_veiculo(veiculo_chave),
                "descontoPercentual": random.choice([0, 5, 10]),
                "motivoRejeicao": motivo,
                "dataCriacao": data_estratificada(next(ciclo_pendente)),
                "cenario": "financiamentoNegado",
            })

        veiculo_chave = proximo_veiculo()
        propostas.append({
            "chave": next(seq),
            "veiculoVendaChave": veiculo_chave,
            "clienteChave": random.choice(clientes_chaves),
            "vendedorChave": random.choice(vendedores_chaves),
            "valorBase": valor_base_por_veiculo(veiculo_chave),
            "descontoPercentual": random.choice([0, 5]),
            # Sem dataCriacao de propósito: "criada" é o estágio inicial ainda em
            # aberto — a proposta expira 7 dias após a criação, então backdatar
            # pra qualquer ponto dos últimos MESES_OPERACAO meses faria TODAS caírem como
            # "Expirada" antes mesmo da demonstração começar. Fica com a data
            # real de importação, garantindo que sempre haja propostas "Criada"
            # genuinamente abertas pra mostrar.
            "cenario": "criada",
        })

    # ---- Concluídas: dominam o volume ----
    # A receita é reconhecida por DataAprovacao (ver DashboardService), não
    # por DataCriacao — então é o MÊS DE APROVAÇÃO que precisa ficar bem
    # distribuído pelo ciclo_meses, não o de criação. Gerar criacao primeiro
    # e derivar aprovacao = criacao + alguns dias fazia ~1/3 dos casos
    # "vazar" pro mês seguinte (dia sorteado tarde no mês + delta de até 15
    # dias), destruindo a uniformidade mesmo com ciclo_meses garantindo
    # contagem quase igual por mês de CRIAÇÃO — sobrava receita variando de
    # 2 a 11 vendas num mesmo mês por acaso (ver docs/redesign/19-bug-
    # margem-mensal-negativa.md). Corrigido invertendo a ordem: sorteia o
    # mês/dia de APROVAÇÃO direto do ciclo, e a criação é só um pouco antes.
    dated_concluida = len(MODOS_PAGAMENTO_PROPOSTA) + 5  # à vista + financiada (5 opções de parcelas)
    ciclo_concluida = iter(ciclo_meses(dated_concluida * REPETICOES_PROPOSTA_CONCLUIDA))

    def aprovacao_e_criacao(mes_idx, dias_min=3, dias_max=15):
        aprovacao = data_estratificada_dt(mes_idx)
        criacao = aprovacao - timedelta(days=random.randint(dias_min, dias_max))
        return criacao, aprovacao

    for _ in range(REPETICOES_PROPOSTA_CONCLUIDA):
        for modo in MODOS_PAGAMENTO_PROPOSTA:
            criacao, aprovacao = aprovacao_e_criacao(next(ciclo_concluida))
            veiculo_chave = proximo_veiculo()
            propostas.append({
                "chave": next(seq),
                "veiculoVendaChave": veiculo_chave,
                "clienteChave": random.choice(clientes_chaves),
                "vendedorChave": random.choice(vendedores_chaves),
                "valorBase": valor_base_por_veiculo(veiculo_chave),
                "descontoPercentual": random.choice([0, 5, 10]),
                "modoPagamento": modo,
                "dataCriacao": criacao.isoformat(),
                "dataAprovacao": min(aprovacao, HOJE).isoformat(),
                "cenario": "concluidaAVista",
            })

        for parcelas in [12, 24, 36, 48, 60]:
            criacao, aprovacao = aprovacao_e_criacao(next(ciclo_concluida), dias_min=5, dias_max=20)
            valor_parcela = valor_redondo(500, 4000, 100)
            taxa = random.choice([1, 1.5, 2, 2.5])
            veiculo_chave = proximo_veiculo()
            propostas.append({
                "chave": next(seq),
                "veiculoVendaChave": veiculo_chave,
                "clienteChave": random.choice(clientes_chaves),
                "vendedorChave": random.choice(vendedores_chaves),
                "valorBase": valor_base_por_veiculo(veiculo_chave),
                "descontoPercentual": random.choice([0, 5]),
                # Texto livre — o sistema não simula/calcula financiamento, o
                # vendedor negocia com a financiadora por fora e anota aqui o
                # que foi proposto (ver PropostaVenda.RegistrarRespostaFinanciadora).
                "textoPropostaFinanciadora": (
                    f"Financeira parceira aprovou o financiamento em {parcelas}x de "
                    f"R$ {valor_parcela:.2f}, taxa de {taxa}% a.m., sujeito a análise "
                    f"cadastral final na assinatura."
                ),
                "dataCriacao": criacao.isoformat(),
                "dataAprovacao": min(aprovacao, HOJE).isoformat(),
                "cenario": "concluidaFinanciada",
            })

    return propostas


def gerar_itens_os(componentes_chaves):
    """1 a 4 componentes por OS, com origem variada — a maior parte já em
    estoque, uma parcela trazida pelo cliente (só instalação) e outra sob
    encomenda (oficina precisou comprar). Cobrir as 3 origens em diferentes
    OSs é justamente o que torna a demonstração da oficina realista."""
    qtd = random.randint(1, 4)
    escolhidos = random.sample(componentes_chaves, k=min(qtd, len(componentes_chaves)))
    itens = []
    for chave in escolhidos:
        origem = random.choices(["Estoque", "Cliente", "Encomenda"], weights=[60, 20, 20])[0]
        itens.append({
            "componenteChave": chave,
            "quantidade": random.choice([1, 1, 1, 2, 3]),
            "origem": origem,
        })
    return itens


def gerar_ordens_servico(veiculos_cliente_chaves, clientes_por_veiculo, mecanicos_chaves, componentes_chaves, checklist_chaves):
    """5 tipos de serviço em cada status.

    Ponto importante (bug corrigido depois de detectado na demonstração):
    status ainda ABERTOS — pendente, emAndamento, finalizadaPendente ("pronta
    pra finalizar, falta cobrar") — só fazem sentido como "recentes". Uma OS
    "pendente" datada de 2 anos atrás não é realidade de oficina nenhuma —
    é uma OS que qualquer negócio real teria resolvido (ou cancelado) há
    muito tempo, e como o prazo dela (poucos dias após a criação) também
    ficaria no passado, ela contava como "atrasada" pro resto da vida do
    sistema. Por isso esses 3 status usam DIAS recentes (últimas
    ~3-4 semanas), não os MESES_OPERACAO meses inteiros do histórico —
    resultado: poucas OS realmente atrasadas (prazo curto + criada há pouco
    tempo), a maioria ainda dentro do prazo, do jeito que uma oficina de
    verdade opera no dia a dia.

    "cancelada"/"canceladaEmAndamento" são ESTADOS TERMINAIS (não contam como
    "atrasada" — o trabalho não vai mais acontecer), então essas sim ficam
    espalhadas pelos MESES_OPERACAO meses inteiros do histórico, como
    qualquer outro registro concluído. "cancelada" simula desistência ainda
    na etapa de orçamento (antes do mecânico começar); "canceladaEmAndamento"
    simula desistência no meio do serviço já iniciado.

    A parte paga se divide em finalizadaPaga (paga, aguardando retirada —
    minoria, REPETICOES_OS_PAGA) e entregue (paga E retirada pelo cliente —
    conclusão real do fluxo, maioria, REPETICOES_OS_ENTREGUE), nivelando a
    oficina com o funil da concessionária onde "concluida" já é o fim de
    fluxo — essas também ficam espalhadas pelo histórico inteiro (são
    trabalho já concluído, o "quando" pode ser qualquer época).

    Cada OS ganha de 1 a 4 componentes (estoque, cliente ou encomenda) e, na
    maioria das vezes, um preset de checklist. Uma fração das OS que chegam
    a iniciar o serviço (todo status exceto pendente/cancelada) ganha
    "comAlerta" — simula o mecânico encontrando um problema novo durante o
    serviço, pausando a OS até o cliente aprovar o escopo maior (ver
    IAlertaOSService)."""
    ordens = []
    seq = id_seq("os")

    def nova_ordem(status, tipo, data_criacao_iso, prazo_dias=None):
        veiculo_chave = random.choice(veiculos_cliente_chaves)
        ordem = {
            "chave": next(seq),
            "veiculoClienteChave": veiculo_chave,
            "clienteChave": clientes_por_veiculo[veiculo_chave],
            "mecanicoChave": random.choice(mecanicos_chaves),
            "tipo": tipo,
            "descricao": random.choice(DESCRICOES_OS[tipo]),
            "prazoDiasAPartirDaCriacao": prazo_dias if prazo_dias is not None else random.choice([2, 3, 5, 7, 10, 15, 20]),
            # Faixa calibrada (ver docs/redesign/17-margem-financeira-realista.md)
            # pra que a margem líquida agregada da oficina (receita de mão de
            # obra + peças - custo de peças - despesa recorrente) feche entre
            # 3% e 10%, não os ~47% que a faixa antiga (120-2200) produzia —
            # mão de obra cobrada não pode crescer sem limite em relação à
            # despesa recorrente real da oficina (salários, aluguel etc.).
            "custoServico": valor_redondo(550, 1250, 50),
            "dataCriacao": data_criacao_iso,
            "cenario": status,
            "itens": gerar_itens_os(componentes_chaves),
        }
        if random.random() < 0.85:
            ordem["checklistPresetChave"] = random.choice(checklist_chaves)
        if status in ("finalizadaPaga", "entregue"):
            ordem["modoPagamento"] = random.choice(MODOS_PAGAMENTO_OS)
        # "novos problemas encontrados" — só faz sentido pra quem chega a
        # EmAndamento (pendente/cancelada nunca iniciam o serviço).
        if status not in ("pendente", "cancelada") and random.random() < 0.25:
            ordem["comAlerta"] = True
        return ordem

    def nova_ordem_estratificada(status, tipo, mes_idx):
        return nova_ordem(status, tipo, data_estratificada(mes_idx))

    def nova_ordem_recente(status, tipo):
        # Últimas ~3 semanas — janela de "trabalho em curso agora" de uma
        # oficina real, nunca anos de profundidade. Prazo e data de criação
        # são escolhidos JUNTOS (não independentes) pra maioria (~75%) ficar
        # dentro do prazo e só uma minoria realista ficar atrasada — sorteando
        # os dois soltos, quase tudo virava "atrasada" por acaso (prazo curto
        # de 2-3 dias quase sempre já vencido se a criação for de 2+ semanas
        # atrás, mesmo a OS sendo genuinamente recente).
        prazo_dias = random.choice([2, 3, 5, 7, 10, 15, 20])
        if random.random() < 0.75:
            dias_atras = random.randint(0, max(0, prazo_dias - 1))  # ainda dentro do prazo
        else:
            dias_atras = prazo_dias + random.randint(1, 10)  # atrasada de propósito
        return nova_ordem(status, tipo, (HOJE - timedelta(days=dias_atras)).isoformat(), prazo_dias=prazo_dias)

    # ---- Ainda abertas (pendente/emAndamento/finalizadaPendente): só recentes ----
    status_abertos_recentes = ["pendente", "emAndamento", "finalizadaPendente"]
    for status in status_abertos_recentes:
        for tipo in TIPOS_OS:
            for _ in range(REPETICOES_OS_PENDENTE):
                ordens.append(nova_ordem_recente(status, tipo))

    # ---- Canceladas (estado terminal, não conta como atrasada): espalhadas ----
    status_cancelados = ["cancelada", "canceladaEmAndamento"]
    ciclo_cancelado = iter(ciclo_meses(len(status_cancelados) * len(TIPOS_OS) * REPETICOES_OS_PENDENTE))
    for _ in range(REPETICOES_OS_PENDENTE):
        for status in status_cancelados:
            for tipo in TIPOS_OS:
                ordens.append(nova_ordem_estratificada(status, tipo, next(ciclo_cancelado)))

    # ---- Paga mas ainda não retirada: minoria dentro da parte concluída ----
    ciclo_paga = iter(ciclo_meses(len(TIPOS_OS) * REPETICOES_OS_PAGA))
    for _ in range(REPETICOES_OS_PAGA):
        for tipo in TIPOS_OS:
            ordens.append(nova_ordem_estratificada("finalizadaPaga", tipo, next(ciclo_paga)))

    # ---- Entregue: paga E retirada — domina o volume, fim de fluxo real ----
    ciclo_entregue = iter(ciclo_meses(len(TIPOS_OS) * REPETICOES_OS_ENTREGUE))
    for _ in range(REPETICOES_OS_ENTREGUE):
        for tipo in TIPOS_OS:
            ordens.append(nova_ordem_estratificada("entregue", tipo, next(ciclo_entregue)))

    return ordens


def main():
    usuarios = gerar_usuarios()
    vendedores_chaves = [u["chave"] for u in usuarios if u["tipo"] == "Vendedor"]
    mecanicos_chaves = [u["chave"] for u in usuarios if u["tipo"] == "Mecanico"]

    clientes = gerar_clientes(qtd=50)
    clientes_chaves = [c["chave"] for c in clientes]

    fornecedores = gerar_fornecedores(qtd=12)
    fornecedores_chaves = [f["chave"] for f in fornecedores]

    componentes = gerar_componentes(fornecedores_chaves)
    componentes_chaves = [c["chave"] for c in componentes]

    checklist_presets = gerar_checklist_presets()
    checklist_chaves = [c["chave"] for c in checklist_presets]

    templates_documento = gerar_templates_documento()

    despesas = gerar_despesas()
    despesas_extras = gerar_despesas_extras()

    # Conta quantas propostas serão geradas (mesma matemática de gerar_propostas)
    # pra saber quantos veículos "disponíveis" preparar antes.
    qtd_propostas_pendente_por_rodada = (len(["aprovada", "vistoriada", "termoRedigido", "termoEnviado"]) * 3
                                         + len(MOTIVOS_REJEICAO) + len(MOTIVOS_NEGATIVA_FINANCIADORA) + 1)
    qtd_propostas_concluida_por_rodada = 3 + 5
    qtd_propostas = (
        qtd_propostas_pendente_por_rodada * REPETICOES_PROPOSTA_PENDENTE +
        qtd_propostas_concluida_por_rodada * REPETICOES_PROPOSTA_CONCLUIDA
    )

    veiculos_venda, veiculos_disponiveis_chaves = gerar_veiculos_venda(
        qtd_disponiveis=qtd_propostas, qtd_em_preparacao=6)
    valor_aquisicao_por_chave = {v["chave"]: v["valorAquisicao"] for v in veiculos_venda}

    veiculos_consignados = gerar_veiculos_consignados(clientes_chaves, vendedores_chaves)

    test_drives = gerar_test_drives(veiculos_disponiveis_chaves, clientes_chaves, vendedores_chaves)

    veiculos_cliente = gerar_veiculos_cliente(clientes_chaves, qtd=50)
    veiculos_cliente_chaves = [v["chave"] for v in veiculos_cliente]
    clientes_por_veiculo = {v["chave"]: v["clienteChave"] for v in veiculos_cliente}

    propostas = gerar_propostas(veiculos_disponiveis_chaves, clientes_chaves, vendedores_chaves, valor_aquisicao_por_chave)
    ordens_servico = gerar_ordens_servico(
        veiculos_cliente_chaves, clientes_por_veiculo, mecanicos_chaves, componentes_chaves, checklist_chaves)

    documento = {
        "usuarios": usuarios,
        "clientes": clientes,
        "fornecedores": fornecedores,
        "componentes": componentes,
        "checklistPresets": checklist_presets,
        "templatesDocumento": templates_documento,
        "despesas": despesas,
        "veiculosVenda": veiculos_venda,
        "veiculosConsignados": veiculos_consignados,
        "veiculosCliente": veiculos_cliente,
        "propostasVenda": propostas,
        "ordensServico": ordens_servico,
        "testDrives": test_drives,
        "despesasExtras": despesas_extras,
    }

    saida = Path(__file__).parent / "dataset_demo.json"
    saida.write_text(json.dumps(documento, ensure_ascii=False, indent=2), encoding="utf-8")

    total = sum(len(v) for v in documento.values())
    print(f"Gerado: {saida}")
    for chave, valor in documento.items():
        print(f"  {chave}: {len(valor)}")
    print(f"  TOTAL: {total} registros")


if __name__ == "__main__":
    main()
