#!/usr/bin/env python3
"""
Gerador da base de demonstração v2 do DELORE (dataset_demo.json).

Plano: Geradores/dataset/PLANO_BASE_DEMO.md (Trilha B, §6). Contrato do JSON:
ImportacaoDadosDTO (C#) + campos novos opcionais da §8/§8.1.

O que muda em relação à v1 (backup em gerar_dataset_v1.py):
  - ~3 anos de operação (out/2023 → véspera de HOJE), com crescimento ao longo
    do tempo e sazonalidade leve (vendas fortes em dez/mar, oficina forte em
    jan/jul — férias);
  - ROTEIRO, não sorteio: cada cenário da §4 tem quantidade mínima garantida
    (constantes no topo) — a autoverificação no fim imprime a tabela;
  - linha do tempo coerente POR REGISTRO: toda data secundária (vistoria,
    aprovação, início, alerta, pagamentos, termo, assinatura, entrega...)
    vem em ordem crescente, nunca no futuro (exceto test drive agendado e
    prazos), nunca antes da contratação do funcionário nem do cadastro do
    cliente/veículo;
  - clientes com recorrência (oficina), compradores que voltam pra revisão,
    clientes "VIP" com histórico rico pra Consulta CPF;
  - coerência econômica (§5) com checagem de margem prevista por setor/ano
    usando a MESMA regra do DashboardService (receita de venda pela
    DataAprovacao de proposta concluída; receita de oficina pela DataCriacao
    de OS Finalizada/Entregue; despesa = balanço do mês, Geral dividida
    meio a meio);
  - despesas esporádicas aleatórias (§8.1) e fechamentos mensais com
    variações das contas variáveis (§8.1);
  - validador de integridade referencial + estimativa de linhas no banco.

Uso:
    python3 Geradores/dataset/gerar_dataset.py
    DATASET_HOJE=2026-10-06 python3 Geradores/dataset/gerar_dataset.py   # outra "data de hoje"

Sem dependências fora da biblioteca padrão; seed fixa (mesma saída sempre
pra mesma DATASET_HOJE).
"""

import json
import math
import os
import random
import sys
import unicodedata
from collections import Counter, defaultdict
from datetime import datetime, timedelta
from pathlib import Path

# =====================================================================
# PARÂMETROS
# =====================================================================

SEED = int(os.environ.get("DATASET_SEED", "20261132"))
rng = random.Random(SEED)

# padrão = data real do computador (o importador do sistema não aceita datas futuras)
HOJE = datetime.strptime(os.environ.get("DATASET_HOJE") or datetime.now().strftime("%Y-%m-%d"), "%Y-%m-%d")
# Nenhum evento "passado" depois disso (a importação roda em HOJE).
LIMITE = (HOJE - timedelta(days=1)).replace(hour=18, minute=0)
# Inauguração da operação (vendas/OS) e compra do estoque inicial.
INICIO_OPERACAO = datetime(2023, 10, 2, 9, 0)
# Estoque inicial (veículos e peças): pátio/prateleira que JÁ existia antes da
# adoção do sistema — cadastrado no dia da adoção com "estoqueInicial": true,
# então a compra NÃO vira despesa (o importador pula a despesa automática).
# da inauguração — sem isso a margem agregada da concessionária carregaria o
# estoque inicial E o estoque final dentro da mesma janela (ver README v2).
ABERTURA_ESTOQUE = (datetime(2023, 10, 2, 8, 0), datetime(2023, 10, 2, 17, 30))
PRIMEIRA_COMPETENCIA = (2023, 10)

NOME_LOJA = "DELORE Veículos e Serviços Automotivos"
CIDADE_LOJA = "Campinas/SP"

# ---- Pessoas ----
QTD_CLIENTES = 300
QTD_CLIENTES_VIP = 10           # histórico rico pra Consulta CPF
QTD_FORNECEDORES = 15

# ---- Concessionária ----
QTD_VENDAS_A_VISTA = 160        # concluidaAVista
QTD_VENDAS_FINANCIADAS = 75     # concluidaFinanciada
PCT_A_VISTA_COM_SINAL = 0.35    # à vista paga em 2 vezes (sinal + saldo)
PROPOSTAS_MIN = {               # §4.1 — mínimos garantidos
    "criada": 3, "rejeitada": 30, "financiamentoNegado": 15, "cancelada": 15,
    "aprovada": 2, "vistoriada": 2, "pagamentoParcial": 2,
    "termoRedigido": 2, "termoEnviado": 2,
}
QTD_ESTOQUE_LIVRE = 4           # disponível sem proposta no fim
QTD_EM_PREPARACAO = 3
QTD_PATIO = 42                  # carros no pátio (estoque em regime); estoque inicial na adoção (02/10/2023)
PCT_PERTO_DO_CUSTO = 0.03       # "problema oculto" — venda perto/abaixo do custo
PCT_REVENDA_VOLTA_OFICINA = 0.22  # compradores que depois fazem revisão aqui

# ---- Test drive (§4.4) ----
PCT_TD_ANTES_DA_VENDA = 0.38
PCT_TD_ANTES_DA_PROPOSTA_FALHA = 0.35
TD_AVULSOS = {"realizado": 10, "cancelado": 20, "naoCompareceu": 18, "reagendado": 10, "agendado": 12}
TD_CONSIGNADO = {"realizado": 12, "cancelado": 3, "naoCompareceu": 2, "agendado": 4, "reagendado": 1}

# ---- Consignação (§4.3) — por tipo de comissão (×2: Fixo e Porcentagem) ----
CONSIGNACOES_POR_TIPO = {"ativa": 7, "vendidaAguardando": 3, "concluida": 6, "devolvida": 4, "cancelada": 4}

# ---- Oficina (§4.2) ----
OS_QTD = {
    # estágios em aberto = fila REAL de uma oficina de ~20 OS/mês (todos presentes, sem acúmulo)
    "pendente": 4, "emVistoria": 2, "aguardandoCliente": 3, "orcamentoRecusado": 30,
    "aprovada": 2, "cancelada": 20, "canceladaEmAndamento": 15, "aguardandoPeca": 2,
    "pausada": 2, "emAndamento": 6, "finalizadaPendente": 2, "finalizadaPaga": 5,
    "entregue": 500,
}
# volume de OS concluídas acompanha o tamanho da janela (calibrado: 500 entregues em 36 meses)
_MESES_JANELA = (HOJE.year - INICIO_OPERACAO.year) * 12 + HOJE.month - INICIO_OPERACAO.month
OS_QTD["entregue"] = round(500 * max(36, _MESES_JANELA) / 36 / 10) * 10
OS_ABERTAS = {"pendente", "emVistoria", "aguardandoCliente", "aprovada", "aguardandoPeca",
              "pausada", "emAndamento", "finalizadaPendente"}
PCT_OS_COM_ALERTA = 0.25
PCT_OS_REQUISICAO_REJEITADA = 0.05
PCT_OS_COM_PRESET = 0.85

# ---- Despesas (§8.1) ----
DESPESAS_EXTRAS_POR_MES_PESOS = {0: 10, 1: 30, 2: 32, 3: 20, 4: 8}
QTD_EVENTOS_GRANDES = 5

SAIDA = Path(__file__).parent / "dataset_demo.json"


# =====================================================================
# DOCUMENTOS BRASILEIROS VÁLIDOS (mesmos algoritmos do Domain C#)
# =====================================================================

def cpf_valido(base9: str) -> str:
    nums = [int(c) for c in base9]
    soma = sum(nums[i] * (10 - i) for i in range(9))
    d1 = 0 if soma % 11 < 2 else 11 - soma % 11
    nums.append(d1)
    soma = sum(nums[i] * (11 - i) for i in range(10))
    d2 = 0 if soma % 11 < 2 else 11 - soma % 11
    return base9 + str(d1) + str(d2)


def renavam_valido(base10: str) -> str:
    inv = base10[::-1]
    mult = [2, 3, 4, 5, 6, 7, 8, 9, 2, 3]
    soma = sum(int(inv[i]) * mult[i] for i in range(10))
    mod = (soma * 10) % 11
    return base10 + str(0 if mod == 10 else mod)


def cnpj_valido(base12: str) -> str:
    nums = [int(c) for c in base12]
    m1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]
    m2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]
    r1 = sum(nums[i] * m1[i] for i in range(12)) % 11
    d1 = 0 if r1 < 2 else 11 - r1
    nums.append(d1)
    r2 = sum(nums[i] * m2[i] for i in range(13)) % 11
    d2 = 0 if r2 < 2 else 11 - r2
    return base12 + str(d1) + str(d2)


_usados = defaultdict(set)


def _unico(tipo, gerar):
    while True:
        v = gerar()
        if v not in _usados[tipo]:
            _usados[tipo].add(v)
            return v


def novo_cpf():
    def g():
        base = "".join(str(rng.randint(0, 9)) for _ in range(9))
        return None if len(set(base)) == 1 else cpf_valido(base)
    while True:
        v = _unico("cpf", g)
        if v:
            return v


def novo_cnpj():
    return _unico("cnpj", lambda: cnpj_valido("".join(str(rng.randint(0, 9)) for _ in range(8)) + "0001"))


def novo_renavam():
    return _unico("renavam", lambda: renavam_valido(str(rng.randint(1, 9)) + "".join(str(rng.randint(0, 9)) for _ in range(9))))


LETRAS_PLACA = "ABCDEFGHJKLMNOPQRSTUVWXYZ"


def nova_placa(ano: int) -> str:
    """Mercosul (ABC1D23) pra veículos 2019+, padrão antigo (ABC1234) antes."""
    def g():
        l3 = "".join(rng.choice(LETRAS_PLACA) for _ in range(3))
        if ano >= 2019:
            return f"{l3}{rng.randint(0, 9)}{rng.choice(LETRAS_PLACA)}{rng.randint(0, 9)}{rng.randint(0, 9)}"
        return f"{l3}{rng.randint(1000, 9999)}"
    return _unico("placa", g)


def novo_codigo(prefixo, digitos=4):
    return _unico("codigo", lambda: f"{prefixo}-{rng.randint(10 ** (digitos - 1), 10 ** digitos - 1)}")


def ean13():
    return _unico("ean", lambda: str(rng.randint(10 ** 12, 10 ** 13 - 1)))


# =====================================================================
# DATAS — linha do tempo
# =====================================================================

# Sazonalidade leve por mês (1 = normal).
SAZ_VENDAS = {1: 0.85, 2: 0.85, 3: 1.10, 4: 0.95, 5: 1.0, 6: 0.95, 7: 1.0, 8: 1.0, 9: 0.95, 10: 1.0, 11: 1.10, 12: 1.20}
SAZ_OFICINA = {1: 1.20, 2: 0.95, 3: 0.95, 4: 0.95, 5: 0.95, 6: 1.05, 7: 1.20, 8: 0.95, 9: 0.95, 10: 1.0, 11: 1.0, 12: 1.15}
SAZ_NEUTRA = {m: 1.0 for m in range(1, 13)}
MINUTOS = [0, 0, 15, 30, 30, 45]


def fmt(dt: datetime) -> str:
    return dt.replace(second=0, microsecond=0).isoformat()


def p(dt_str: str) -> datetime:
    return datetime.fromisoformat(dt_str)


def horario_comercial(d: datetime, hmin=8, hmax=17) -> datetime:
    """Mesmo dia, hora de expediente; domingo vira sábado (nunca empurra pra frente)."""
    if d.weekday() == 6:
        d = d - timedelta(days=1)
    return d.replace(hour=rng.randint(hmin, hmax), minute=rng.choice(MINUTOS), second=0, microsecond=0)


CRESC_VENDAS = (0.80, 0.40)     # peso = a + b·t (t: 0 → 1 ao longo da janela) — ~50% de crescimento
CRESC_OFICINA = (0.95, 0.15)    # oficina cresce menos (base de clientes já madura)
CRESC_NEUTRO = (0.80, 0.40)


def peso_data(d: datetime, saz, cresc=CRESC_NEUTRO) -> float:
    t = max(0.0, min(1.0, (d - INICIO_OPERACAO).days / max(1, (LIMITE - INICIO_OPERACAO).days)))
    return (cresc[0] + cresc[1] * t) * saz[d.month]


def sortear_data(a: datetime, b: datetime, saz=SAZ_NEUTRA, cresc=CRESC_NEUTRO) -> datetime:
    """Data em [a, b] ponderada por crescimento + sazonalidade, em horário comercial."""
    wmax = (cresc[0] + cresc[1]) * max(saz.values())
    for _ in range(10000):
        x = a + timedelta(seconds=rng.uniform(0, max(1, (b - a).total_seconds())))
        if rng.random() < peso_data(x, saz, cresc) / wmax:
            r = horario_comercial(x)
            return min(max(r, a), b)
    return a


def datas_estratificadas(n: int, a: datetime, b: datetime, saz=SAZ_NEUTRA, cresc=CRESC_NEUTRO):
    """n datas em [a, b] com contagem POR MÊS proporcional a crescimento ×
    sazonalidade (maior resto) — evita meses vazios/empilhados por puro acaso
    de sorteio — e dia/hora aleatórios dentro do mês."""
    meses = []
    cur = datetime(a.year, a.month, 1)
    while cur <= b:
        prox = datetime(cur.year + (cur.month == 12), cur.month % 12 + 1, 1)
        ini, fim = max(cur, a), min(prox - timedelta(minutes=1), b)
        if fim > ini:
            frac = (fim - ini).total_seconds() / (prox - cur).total_seconds()
            meio = ini + (fim - ini) / 2
            meses.append((ini, fim, peso_data(meio, saz, cresc) * frac))
        cur = prox
    tot = sum(w for _, _, w in meses)
    exatos = [n * w / tot for _, _, w in meses]
    cont = [int(x) for x in exatos]
    for i in sorted(range(len(meses)), key=lambda i: -(exatos[i] - cont[i]))[: n - sum(cont)]:
        cont[i] += 1
    out = []
    for (ini, fim, _), k in zip(meses, cont):
        for _ in range(k):
            x = ini + timedelta(seconds=rng.uniform(0, (fim - ini).total_seconds()))
            out.append(min(max(horario_comercial(x), ini), fim))
    return sorted(out)


def proximo(t: datetime, dmin: int, dmax: int, hmin=8, hmax=17) -> datetime:
    """Evento seguinte, `dmin..dmax` dias depois de t (0 = mesmo dia, algumas horas depois)."""
    d = rng.randint(dmin, dmax)
    if d == 0:
        return t + timedelta(minutes=rng.choice([30, 45, 60, 90, 120, 180, 240]))
    n = (t + timedelta(days=d)).replace(hour=rng.randint(hmin, hmax), minute=rng.choice(MINUTOS))
    if n.weekday() == 6:
        n += timedelta(days=1)
    return n


def comprimir(datas, limite=LIMITE):
    """Se a cadeia passou do limite, reescala linearmente entre o início e o limite
    (mantém a ordem estritamente crescente)."""
    if not datas or datas[-1] <= limite:
        return datas
    ini = datas[0]
    if ini >= limite:
        ini = limite - timedelta(hours=len(datas) + 1)
    total = (datas[-1] - datas[0]).total_seconds()
    disp = (limite - ini).total_seconds()
    out = []
    for i, d in enumerate(datas):
        frac = (d - datas[0]).total_seconds() / total if total else 0
        out.append(ini + timedelta(seconds=frac * disp))
    # garante crescente estrito (mínimo 10 min entre eventos)
    for i in range(1, len(out)):
        if out[i] <= out[i - 1]:
            out[i] = out[i - 1] + timedelta(minutes=10)
    return [o.replace(second=0, microsecond=0) for o in out]


def recente(dias_min, dias_max):
    """Data em horário comercial 'dias' antes do LIMITE (0 = no próprio dia limite)."""
    d = LIMITE - timedelta(days=rng.randint(dias_min, dias_max))
    r = horario_comercial(d, 8, 16)
    return min(r, LIMITE - timedelta(hours=2))


def competencia(dt):
    return (dt.year, dt.month)


def competencias():
    a, m = PRIMEIRA_COMPETENCIA
    out = []
    while (a, m) <= (HOJE.year, HOJE.month):
        out.append((a, m))
        m += 1
        if m == 13:
            a, m = a + 1, 1
    return out


# =====================================================================
# DINHEIRO
# =====================================================================

def redondo(minimo, maximo, passo):
    return minimo + rng.randint(0, max(0, (maximo - minimo) // passo)) * passo


def arred(v, passo):
    return int(round(v / passo)) * passo


def brl(v):
    s = f"{v:,.2f}"
    return "R$ " + s.replace(",", "X").replace(".", ",").replace("X", ".")


def sem_acento(s):
    return "".join(c for c in unicodedata.normalize("NFD", s) if unicodedata.category(c) != "Mn")


# =====================================================================
# CATÁLOGOS
# =====================================================================

NOMES_M = ["Adriano", "André", "Antônio", "Bruno", "Caio", "Carlos", "Daniel", "Diego", "Eduardo", "Felipe",
           "Fernando", "Gabriel", "Guilherme", "Gustavo", "Henrique", "Igor", "João", "Jorge", "José", "Leandro",
           "Leonardo", "Lucas", "Luiz", "Marcelo", "Marcos", "Mateus", "Murilo", "Otávio", "Paulo", "Pedro",
           "Rafael", "Renato", "Ricardo", "Roberto", "Rodrigo", "Samuel", "Sérgio", "Thiago", "Vinícius", "Wagner"]
NOMES_F = ["Adriana", "Aline", "Amanda", "Ana", "Beatriz", "Bruna", "Camila", "Carolina", "Cláudia", "Daniela",
           "Débora", "Eliane", "Fabiana", "Fernanda", "Gabriela", "Helena", "Isabela", "Juliana", "Larissa", "Letícia",
           "Luciana", "Marcela", "Mariana", "Michele", "Natália", "Patrícia", "Paula", "Priscila", "Renata", "Sabrina",
           "Simone", "Sílvia", "Tatiane", "Vanessa", "Viviane", "Yasmin", "Lívia", "Rosana", "Sandra", "Cristina"]
SOBRENOMES = ["Silva", "Santos", "Oliveira", "Souza", "Rodrigues", "Ferreira", "Alves", "Pereira", "Lima", "Gomes",
              "Costa", "Ribeiro", "Martins", "Carvalho", "Almeida", "Lopes", "Soares", "Fernandes", "Vieira", "Barbosa",
              "Rocha", "Dias", "Nascimento", "Andrade", "Moreira", "Nunes", "Marques", "Machado", "Mendes", "Freitas",
              "Cardoso", "Ramos", "Gonçalves", "Santana", "Teixeira", "Moraes", "Pinto", "Campos", "Correia", "Prado",
              "Bueno", "Tavares", "Monteiro", "Siqueira", "Castro", "Fonseca", "Cunha", "Brito", "Rezende", "Toledo"]

# Região de Campinas/SP — a loja é local, clientes da região metropolitana.
CIDADES = [  # (cidade, uf, ddd, prefixo CEP (5 díg.), peso)
    ("Campinas", "SP", "19", ["13010", "13015", "13025", "13040", "13045", "13050", "13060", "13070", "13083", "13092"], 50),
    ("Valinhos", "SP", "19", ["13270", "13271", "13272"], 8),
    ("Vinhedo", "SP", "19", ["13280", "13282"], 5),
    ("Sumaré", "SP", "19", ["13170", "13171", "13175"], 8),
    ("Hortolândia", "SP", "19", ["13183", "13184", "13185"], 7),
    ("Indaiatuba", "SP", "19", ["13330", "13334", "13339"], 7),
    ("Paulínia", "SP", "19", ["13140", "13145"], 5),
    ("Americana", "SP", "19", ["13465", "13466", "13468"], 5),
    ("Jundiaí", "SP", "11", ["13201", "13207", "13212", "13214"], 5),
]
BAIRROS = ["Centro", "Cambuí", "Taquaral", "Barão Geraldo", "Jardim Chapadão", "Vila Industrial", "Jardim Proença",
           "Parque Prado", "Nova Campinas", "Jardim do Trevo", "Vila Nova", "Jardim Paulista", "Jardim Santa Rosa",
           "Bosque", "Castelo", "Swift", "Jardim Flamboyant", "Parque Taquaral", "Jardim Nova Europa", "Vila Teixeira"]
LOGRADOUROS = ["Rua", "Rua", "Rua", "Avenida", "Alameda", "Travessa"]
NOMES_RUA = ["Barão de Jaguara", "José Paulino", "Coronel Quirino", "Orosimbo Maia", "das Amoreiras", "Brasil",
             "Norte-Sul", "Júlio de Mesquita", "Benjamin Constant", "Dr. Moraes Salles", "Antônio Carlos Couto de Barros",
             "Francisco Glicério", "Andrade Neves", "General Osório", "Maria Monteiro", "dos Expedicionários",
             "Santos Dumont", "Tiradentes", "Marechal Deodoro", "15 de Novembro", "Sete de Setembro", "dos Ipês",
             "das Palmeiras", "Hermantino Coelho", "Heitor Penteado", "Guilherme da Silva", "Princesa D'Oeste"]

# (marca, modelo, motorização, preço "novo" hoje, combustível, câmbios possíveis, ano_min, ano_max)
CATALOGO_VEICULOS = [
    ("Fiat", "Mobi", "1.0", 75000, "Flex", ["Manual"], 2017, 2025),
    ("Fiat", "Argo", "1.0", 90000, "Flex", ["Manual", "Automatico"], 2018, 2025),
    ("Fiat", "Cronos", "1.3", 102000, "Flex", ["Manual", "Automatico"], 2019, 2025),
    ("Fiat", "Pulse", "1.0 Turbo", 122000, "Flex", ["Automatico"], 2022, 2025),
    ("Fiat", "Strada", "1.3", 112000, "Flex", ["Manual", "Automatico"], 2018, 2025),
    ("Fiat", "Toro", "1.3 Turbo", 165000, "Flex", ["Automatico"], 2018, 2025),
    ("Volkswagen", "Gol", "1.0", 72000, "Flex", ["Manual"], 2014, 2023),
    ("Volkswagen", "Polo", "1.0 TSI", 108000, "Flex", ["Manual", "Automatico"], 2018, 2025),
    ("Volkswagen", "Virtus", "1.0 TSI", 122000, "Flex", ["Automatico"], 2018, 2025),
    ("Volkswagen", "T-Cross", "1.0 TSI", 152000, "Flex", ["Automatico"], 2019, 2025),
    ("Volkswagen", "Nivus", "1.0 TSI", 138000, "Flex", ["Automatico"], 2020, 2025),
    ("Volkswagen", "Saveiro", "1.6", 98000, "Flex", ["Manual"], 2014, 2023),
    ("Chevrolet", "Onix", "1.0", 96000, "Flex", ["Manual", "Automatico"], 2014, 2025),
    ("Chevrolet", "Onix Plus", "1.0 Turbo", 112000, "Flex", ["Automatico"], 2020, 2025),
    ("Chevrolet", "Tracker", "1.0 Turbo", 148000, "Flex", ["Automatico"], 2020, 2025),
    ("Chevrolet", "Spin", "1.8", 118000, "Flex", ["Manual", "Automatico"], 2014, 2024),
    ("Chevrolet", "S10", "2.8 Diesel", 265000, "Diesel", ["Automatico"], 2015, 2025),
    ("Toyota", "Yaris", "1.5", 118000, "Flex", ["Automatico"], 2019, 2024),
    ("Toyota", "Corolla", "2.0", 168000, "Flex", ["Automatico"], 2014, 2025),
    ("Toyota", "Corolla Cross", "2.0", 188000, "Flex", ["Automatico"], 2021, 2025),
    ("Toyota", "Hilux", "2.8 Diesel", 295000, "Diesel", ["Automatico"], 2016, 2025),
    ("Honda", "Fit", "1.5", 98000, "Flex", ["Automatico"], 2014, 2021),
    ("Honda", "City", "1.5", 128000, "Flex", ["Automatico"], 2015, 2025),
    ("Honda", "Civic", "2.0", 182000, "Flex", ["Automatico"], 2015, 2025),
    ("Honda", "HR-V", "1.5", 162000, "Flex", ["Automatico"], 2016, 2025),
    ("Hyundai", "HB20", "1.0", 92000, "Flex", ["Manual", "Automatico"], 2014, 2025),
    ("Hyundai", "Creta", "1.0 Turbo", 152000, "Flex", ["Automatico"], 2017, 2025),
    ("Renault", "Kwid", "1.0", 72000, "Flex", ["Manual"], 2018, 2025),
    ("Renault", "Sandero", "1.0", 78000, "Flex", ["Manual"], 2014, 2022),
    ("Renault", "Duster", "1.6", 118000, "Flex", ["Manual", "Automatico"], 2016, 2025),
    ("Jeep", "Renegade", "1.3 Turbo", 142000, "Flex", ["Automatico"], 2016, 2025),
    ("Jeep", "Compass", "1.3 Turbo", 192000, "Flex", ["Automatico"], 2017, 2025),
    ("Nissan", "Kicks", "1.6", 132000, "Flex", ["Automatico"], 2017, 2025),
    ("Nissan", "Versa", "1.6", 112000, "Flex", ["Automatico"], 2016, 2025),
    ("Ford", "Ka", "1.0", 66000, "Flex", ["Manual"], 2015, 2021),
    ("Ford", "EcoSport", "1.5", 88000, "Flex", ["Manual", "Automatico"], 2014, 2021),
    ("Ford", "Ranger", "3.2 Diesel", 255000, "Diesel", ["Automatico"], 2015, 2025),
    ("Volkswagen", "Amarok", "3.0 V6 Diesel", 320000, "Diesel", ["Automatico"], 2018, 2025),
    # gasolina pura (importados premium)
    ("Mini", "Cooper S", "2.0 Turbo", 290000, "Gasolina", ["Automatico"], 2016, 2024),
    ("Audi", "A3 Sedan", "2.0 TFSI", 260000, "Gasolina", ["Automatico"], 2016, 2024),
    ("Ford", "Mustang", "5.0 V8", 520000, "Gasolina", ["Automatico"], 2018, 2023),
    # álcool puro — últimos carros só a etanol (anos 2000), usados de entrada
    ("Volkswagen", "Gol", "1.0 Álcool", 180000, "Alcool", ["Manual"], 2003, 2006),
    ("Fiat", "Uno Mille", "1.0 Álcool", 170000, "Alcool", ["Manual"], 2003, 2005),
    # elétricos
    ("Byd", "Dolphin", "Elétrico 95 cv", 150000, "Eletrico", ["Automatico"], 2023, 2025),
    ("Renault", "Kwid E-Tech", "Elétrico 65 cv", 100000, "Eletrico", ["Automatico"], 2022, 2025),
    ("Volvo", "EX30", "Elétrico 272 cv", 230000, "Eletrico", ["Automatico"], 2024, 2025),
    # híbridos
    ("Toyota", "Corolla Cross", "1.8 Hybrid", 200000, "Hibrido", ["Automatico"], 2021, 2025),
    ("Byd", "Song Plus", "1.5 DM-i Híbrido", 235000, "Hibrido", ["Automatico"], 2023, 2025),
    ("Haval", "H6", "1.5 HEV", 215000, "Hibrido", ["Automatico"], 2023, 2025),
]
CORES = ["Branco", "Branco", "Prata", "Prata", "Preto", "Preto", "Cinza", "Cinza", "Vermelho", "Azul", "Bege", "Marrom"]
ACESSORIOS = ["ArCondicionado", "VidrosEletricos", "DirecaoHidraulica", "TetoSolar", "BancoCouro", "CameraRe",
              "SensorEstacionamento", "CentralMultimidia", "Alarme", "RodaLiga", "Bluetooth", "TravasEletrica"]


def preco_mercado(preco_novo, ano):
    idade = max(0, HOJE.year - ano)
    return preco_novo * (0.90 ** idade)


# ---- Componentes: (sistema, nome, categoria, unidade, custo_min, custo_max, passo, qtd_min, qtd_max, marcas) ----
COMPONENTES = [
    ("Motor", "Filtro de óleo", "Filtro", "UN", 25, 45, 5, 14, 28, ["Tecfil", "Mann", "Mahle"]),
    ("Motor", "Filtro de ar do motor", "Filtro", "UN", 35, 70, 5, 10, 20, ["Tecfil", "Mann", "Mahle"]),
    ("Motor", "Filtro de combustível", "Filtro", "UN", 30, 60, 5, 8, 16, ["Tecfil", "Bosch"]),
    ("Motor", "Jogo de velas de ignição (4 un.)", "Ignição", "JG", 120, 260, 10, 5, 10, ["NGK", "Bosch"]),
    ("Motor", "Bobina de ignição", "Ignição", "UN", 220, 380, 10, 2, 5, ["Bosch", "Magneti Marelli"]),
    ("Motor", "Kit correia dentada com tensor", "Correia", "KIT", 260, 480, 10, 3, 6, ["Gates", "Dayco", "Contitech"]),
    ("Motor", "Óleo do motor 5W30 sintético", "Lubrificante", "L", 35, 55, 5, 40, 80, ["Mobil", "Castrol", "Shell"]),
    ("Motor", "Junta do cabeçote", "Vedação", "UN", 90, 180, 10, 2, 4, ["Sabó", "Taranto"]),
    ("Motor", "Sonda lambda", "Sensor", "UN", 280, 450, 10, 1, 3, ["Bosch", "NGK"]),
    ("Motor", "Bomba de combustível", "Combustível", "UN", 350, 600, 10, 1, 3, ["Bosch", "Magneti Marelli"]),
    ("Freios", "Pastilha de freio dianteira (jogo)", "Pastilha", "JG", 90, 160, 10, 8, 16, ["Fras-le", "Cobreq", "Bosch", "TRW"]),
    ("Freios", "Pastilha de freio traseira (jogo)", "Pastilha", "JG", 80, 140, 10, 5, 10, ["Fras-le", "Cobreq", "TRW"]),
    ("Freios", "Disco de freio dianteiro (par)", "Disco", "PAR", 240, 420, 10, 3, 7, ["Fremax", "Hipper Freios", "TRW"]),
    ("Freios", "Disco de freio traseiro (par)", "Disco", "PAR", 220, 380, 10, 2, 4, ["Fremax", "Hipper Freios"]),
    ("Freios", "Fluido de freio DOT4 (500 ml)", "Fluido", "UN", 25, 40, 5, 12, 24, ["Bosch", "Varga", "TRW"]),
    ("Freios", "Cilindro mestre de freio", "Cilindro", "UN", 280, 450, 10, 1, 2, ["TRW", "Bosch"]),
    ("Freios", "Lona de freio traseira (jogo)", "Lona", "JG", 110, 180, 10, 3, 6, ["Fras-le", "Cobreq"]),
    ("Suspensao", "Amortecedor dianteiro", "Amortecedor", "UN", 280, 480, 10, 4, 8, ["Cofap", "Monroe", "Nakata"]),
    ("Suspensao", "Amortecedor traseiro", "Amortecedor", "UN", 220, 380, 10, 4, 8, ["Cofap", "Monroe", "Nakata"]),
    ("Suspensao", "Kit batente e coifa do amortecedor", "Kit", "KIT", 60, 120, 10, 6, 12, ["Sampel", "Axios"]),
    ("Suspensao", "Bandeja de suspensão inferior", "Bandeja", "UN", 260, 450, 10, 2, 4, ["Nakata", "Viemar"]),
    ("Suspensao", "Pivô de suspensão", "Pivô", "UN", 70, 130, 10, 4, 8, ["Nakata", "Viemar", "TRW"]),
    ("Suspensao", "Bieleta da barra estabilizadora", "Bieleta", "UN", 60, 110, 10, 4, 8, ["Nakata", "Viemar"]),
    ("Direcao", "Terminal de direção", "Terminal", "UN", 70, 130, 10, 4, 8, ["Nakata", "Viemar", "TRW"]),
    ("Direcao", "Barra axial de direção", "Barra", "UN", 90, 160, 10, 2, 5, ["Nakata", "Viemar"]),
    ("Direcao", "Bomba de direção hidráulica", "Bomba", "UN", 650, 1100, 50, 1, 2, ["TRW", "ZF"]),
    ("Eletrica", "Bateria 60Ah", "Bateria", "UN", 380, 520, 10, 4, 8, ["Moura", "Heliar", "Bosch"]),
    ("Eletrica", "Alternador", "Alternador", "UN", 750, 1300, 50, 1, 2, ["Bosch", "Valeo"]),
    ("Eletrica", "Motor de arranque", "Motor de partida", "UN", 650, 1100, 50, 1, 2, ["Bosch", "Valeo"]),
    ("Eletrica", "Lâmpada H4 (par)", "Lâmpada", "PAR", 40, 80, 5, 8, 16, ["Philips", "Osram"]),
    ("Eletrica", "Sensor ABS", "Sensor", "UN", 180, 320, 10, 1, 3, ["Bosch", "Continental"]),
    ("Arrefecimento", "Radiador", "Radiador", "UN", 450, 800, 50, 1, 3, ["Valeo", "Visconde", "Denso"]),
    ("Arrefecimento", "Bomba d'água", "Bomba", "UN", 180, 320, 10, 2, 5, ["Urba", "Nakata", "SKF"]),
    ("Arrefecimento", "Válvula termostática", "Válvula", "UN", 90, 160, 10, 2, 5, ["MTE-Thomson", "Wahler"]),
    ("Arrefecimento", "Aditivo para radiador", "Aditivo", "L", 25, 40, 5, 12, 24, ["Paraflu", "Valeo"]),
    ("Arrefecimento", "Mangueira superior do radiador", "Mangueira", "UN", 60, 110, 10, 3, 6, ["Gates", "Dayco"]),
    ("Arrefecimento", "Compressor do ar-condicionado", "Ar-condicionado", "UN", 1500, 2600, 100, 1, 1, ["Denso", "Delphi"]),
    ("Arrefecimento", "Filtro de cabine (antipólen)", "Ar-condicionado", "UN", 30, 60, 5, 10, 20, ["Tecfil", "Mann", "Wega"]),
    ("Arrefecimento", "Carga de gás refrigerante R134a", "Ar-condicionado", "UN", 90, 150, 10, 6, 12, ["DuPont", "Chemours"]),
    ("Arrefecimento", "Eletroventilador do radiador", "Ventilador", "UN", 380, 650, 10, 1, 3, ["Valeo", "Bosch"]),
    ("Transmissão", "Kit de embreagem", "Embreagem", "KIT", 600, 1100, 50, 2, 3, ["LUK", "Sachs", "Valeo"]),
    ("Transmissão", "Rolamento/atuador de embreagem", "Embreagem", "UN", 180, 320, 10, 2, 4, ["LUK", "SKF"]),
    ("Transmissão", "Óleo de câmbio 75W90", "Lubrificante", "L", 45, 70, 5, 10, 20, ["Mobil", "Castrol"]),
    ("Transmissão", "Junta homocinética", "Junta", "UN", 280, 480, 10, 2, 4, ["Nakata", "SKF"]),
    ("Transmissão", "Coxim do câmbio", "Coxim", "UN", 110, 200, 10, 2, 4, ["Sampel", "Axios"]),
    ("Escapamento", "Catalisador", "Catalisador", "UN", 900, 1600, 100, 1, 2, ["Tuper", "Mastra"]),
    ("Escapamento", "Silencioso traseiro", "Silencioso", "UN", 250, 450, 10, 2, 3, ["Tuper", "Mastra"]),
    ("Escapamento", "Tubo intermediário do escapamento", "Tubo", "UN", 180, 320, 10, 1, 3, ["Tuper", "Mastra"]),
    ("Lataria", "Para-choque dianteiro", "Para-choque", "UN", 450, 900, 50, 1, 2, ["Arteb", "Original"]),
    ("Lataria", "Retrovisor externo", "Retrovisor", "UN", 220, 450, 10, 1, 3, ["Metagal", "Original"]),
    ("Lataria", "Farol dianteiro", "Farol", "UN", 450, 900, 50, 1, 2, ["Arteb", "Original"]),
    ("Acessorios", "Câmera de ré", "Câmera", "UN", 180, 320, 10, 2, 5, ["Positron", "Multilaser"]),
    ("Acessorios", "Central multimídia", "Multimídia", "UN", 900, 1600, 100, 1, 3, ["Pioneer", "Positron"]),
    ("Acessorios", "Película de controle solar (kit)", "Película", "KIT", 150, 250, 10, 3, 6, ["3M", "Insulfilm"]),
    ("Acessorios", "Alarme automotivo", "Alarme", "UN", 250, 400, 10, 2, 4, ["Positron", "Pósitron"]),
    ("Interior", "Cinto de segurança dianteiro", "Cinto", "UN", 180, 300, 10, 1, 3, ["TRW", "Original"]),
    ("Interior", "Forração de teto", "Forração", "UN", 350, 600, 50, 1, 2, ["Original", "Autoforro"]),
]
MARGENS_POR_SISTEMA = {  # margem % sobre o custo (§5: 30–80%)
    "Motor": [40, 50, 60], "Freios": [50, 60, 70], "Suspensao": [40, 50, 60], "Direcao": [40, 50],
    "Eletrica": [30, 40, 50], "Arrefecimento": [40, 50, 60], "Transmissão": [35, 45, 55],
    "Escapamento": [35, 45], "Lataria": [30, 40], "Acessorios": [60, 70, 80], "Interior": [40, 50],
}
# Peças que ganham uma 2ª marca (equivalente compatível — doc 35)
EQUIVALENCIAS = [
    ("Filtro de óleo", "Paralela", "Mesma rosca e vazão; equivalente homologado para motores 1.0/1.4 flex."),
    ("Filtro de ar do motor", "Paralela", "Mesmas dimensões e elemento filtrante — intercambiável."),
    ("Jogo de velas de ignição (4 un.)", "Similar", "Grau térmico equivalente; mesma rosca e folga de eletrodo."),
    ("Kit correia dentada com tensor", "Paralela", "Mesmo número de dentes e perfil; tensor compatível."),
    ("Pastilha de freio dianteira (jogo)", "Paralela", "Mesma aplicação e composto cerâmico similar."),
    ("Pastilha de freio traseira (jogo)", "Paralela", "Mesma aplicação; atrito equivalente."),
    ("Disco de freio dianteiro (par)", "Similar", "Mesmo diâmetro e espessura mínima — ventilado."),
    ("Amortecedor dianteiro", "Paralela", "Mesma carga e curso; linha reposição equivalente."),
    ("Amortecedor traseiro", "Paralela", "Mesma carga e curso; linha reposição equivalente."),
    ("Pivô de suspensão", "Similar", "Mesmo cone e fixação."),
    ("Terminal de direção", "Similar", "Rosca e cone equivalentes."),
    ("Bateria 60Ah", "Similar", "Mesma capacidade (60Ah) e polaridade; CCA equivalente."),
    ("Lâmpada H4 (par)", "Similar", "Mesma potência 60/55W, base P43t."),
    ("Bomba d'água", "Original", "Peça genuína de concessionária — mesmo código OEM."),
    ("Filtro de cabine (antipólen)", "Original", "Peça original da montadora — mesmo código OEM, outro fornecedor."),
    ("Kit de embreagem", "Paralela", "Mesmo diâmetro de disco e estrias do platô."),
    ("Junta homocinética", "Similar", "Mesmo número de estrias e anel do ABS."),
    ("Bieleta da barra estabilizadora", "Similar", "Mesmo comprimento e rosca."),
    ("Silencioso traseiro", "Similar", "Mesma bitola e fixação."),
    ("Radiador", "Remanufaturada", "Colmeia recondicionada com garantia de 6 meses — mesma aplicação."),
]

PRESETS = [
    ("Revisão por quilometragem — plano do fabricante", [
        "Trocar óleo do motor e filtros (óleo, ar, cabine)", "Verificar fluidos (freio, arrefecimento, direção)",
        "Inspecionar freios, suspensão e direção", "Testar bateria, carga e iluminação", "Calibrar pneus e verificar estepe"]),
    ("Troca de óleo e filtros", [
        "Drenar óleo usado e conferir aspecto", "Substituir filtro de óleo",
        "Abastecer com óleo na especificação do fabricante", "Verificar vazamentos e resetar indicador do painel"]),
    ("Freios — inspeção e troca", [
        "Medir espessura de pastilhas e discos", "Substituir componentes desgastados",
        "Sangrar sistema e completar fluido DOT4", "Testar frenagem em rodagem"]),
    ("Suspensão e direção", [
        "Inspecionar amortecedores, batentes e coifas", "Verificar pivôs, bandejas, buchas e terminais",
        "Substituir peças com folga", "Realizar alinhamento e balanceamento"]),
    ("Diagnóstico elétrico e injeção", [
        "Ler e registrar códigos de falha com scanner", "Testar bateria e sistema de carga",
        "Verificar chicote, conectores, bobinas e sensores", "Apagar códigos e validar em rodagem"]),
    ("Ar-condicionado", [
        "Medir temperatura de saída e pressões", "Verificar vazamentos com detector/UV",
        "Substituir filtro de cabine e higienizar dutos", "Recarregar gás refrigerante na carga correta"]),
    ("Correia dentada e arrefecimento", [
        "Conferir ponto do motor antes da desmontagem", "Substituir correia dentada, tensor e bomba d'água",
        "Trocar aditivo e sangrar o sistema", "Testar motor e temperatura de trabalho"]),
    ("Embreagem e transmissão", [
        "Testar acionamento e patinação da embreagem", "Substituir kit de embreagem e rolamento",
        "Verificar coxins e juntas homocinéticas", "Completar óleo do câmbio e testar trocas"]),
    ("Pré-viagem", [
        "Verificar pneus, estepe e calibragem", "Checar níveis de todos os fluidos",
        "Testar freios, iluminação e palhetas", "Checar ar-condicionado e bateria"]),
]

# ---- Temas de serviço (tipo, especialidade, preset, peças, mão de obra, duração em dias, descrições) ----
TEMAS_OS = [
    {"tema": "revisao", "tipo": "Revisao", "esp": "Mecanica", "preset": "Revisão por quilometragem — plano do fabricante", "peso": 22,
     "pecas": ["Filtro de óleo", "Óleo do motor 5W30 sintético", "Filtro de ar do motor", "Filtro de cabine (antipólen)", "Filtro de combustível", "Jogo de velas de ignição (4 un.)"],
     "mo": (500, 1100), "dur": (0, 1),
     "desc": ["Revisão dos {km} km conforme plano do fabricante", "Revisão anual — cliente pediu checagem geral",
              "Revisão programada dos {km} km com troca de filtros", "Revisão de garantia do veículo seminovo",
              "Revisão completa antes de vender o carro", "Revisão periódica atrasada — veículo rodando com óleo vencido"]},
    {"tema": "oleo", "tipo": "Manutencao", "esp": "Mecanica", "preset": "Troca de óleo e filtros", "peso": 12,
     "pecas": ["Filtro de óleo", "Óleo do motor 5W30 sintético", "Filtro de ar do motor"],
     "mo": (180, 350), "dur": (0, 0),
     "desc": ["Troca de óleo e filtro — luz de manutenção acesa", "Troca de óleo vencido por tempo",
              "Troca de óleo e filtros com {km} km", "Cliente notou nível de óleo baixo na vareta"]},
    {"tema": "freios", "tipo": "TrocaPecas", "esp": "Freios", "preset": "Freios — inspeção e troca", "peso": 14,
     "pecas": ["Pastilha de freio dianteira (jogo)", "Disco de freio dianteiro (par)", "Pastilha de freio traseira (jogo)", "Fluido de freio DOT4 (500 ml)", "Lona de freio traseira (jogo)", "Disco de freio traseiro (par)"],
     "mo": (350, 900), "dur": (0, 1),
     "desc": ["Barulho metálico ao frear", "Pedal de freio baixo e esponjoso", "Volante trepida nas frenagens em velocidade",
              "Troca de pastilhas e discos dianteiros", "Luz de freio acesa no painel", "Carro puxando para um lado ao frear"]},
    {"tema": "suspensao", "tipo": "Manutencao", "esp": "Suspensao", "preset": "Suspensão e direção", "peso": 11,
     "pecas": ["Amortecedor dianteiro", "Amortecedor traseiro", "Kit batente e coifa do amortecedor", "Pivô de suspensão", "Bieleta da barra estabilizadora", "Bandeja de suspensão inferior", "Terminal de direção", "Barra axial de direção"],
     "mo": (500, 1400), "dur": (0, 2),
     "desc": ["Ruído na suspensão dianteira ao passar em lombadas", "Carro balançando demais em curvas",
              "Batida seca na suspensão em buracos", "Pneus com desgaste irregular — suspeita de folga",
              "Volante desalinhado após impacto em buraco", "Estalo ao esterçar o volante parado"]},
    {"tema": "motor", "tipo": "Manutencao", "esp": "Motor", "preset": "Correia dentada e arrefecimento", "peso": 9,
     "pecas": ["Kit correia dentada com tensor", "Bomba d'água", "Junta do cabeçote", "Bobina de ignição", "Jogo de velas de ignição (4 un.)", "Sonda lambda", "Bomba de combustível", "Aditivo para radiador"],
     "mo": (1200, 2600), "dur": (1, 4),
     "desc": ["Troca preventiva da correia dentada com {km} km", "Motor falhando em marcha lenta",
              "Perda de potência em subidas", "Fumaça branca no escapamento e consumo de água",
              "Motor superaquecendo no trânsito", "Barulho de batida no motor quando frio",
              "Vazamento de óleo pela tampa de válvulas"]},
    {"tema": "eletrica", "tipo": "Diagnostico", "esp": "Eletrica", "preset": "Diagnóstico elétrico e injeção", "peso": 10,
     "pecas": ["Bateria 60Ah", "Alternador", "Motor de arranque", "Lâmpada H4 (par)", "Sensor ABS", "Bobina de ignição"],
     "mo": (300, 950), "dur": (0, 2),
     "desc": ["Carro não pega pela manhã", "Luz da bateria acesa no painel", "Luz da injeção eletrônica acesa",
              "Farol baixo queimando com frequência", "Luz do ABS acesa após lavagem",
              "Bateria descarregando com o carro parado", "Vidro elétrico do motorista parou de funcionar"]},
    {"tema": "ar", "tipo": "Manutencao", "esp": "ArCondicionado", "preset": "Ar-condicionado", "peso": 7,
     "pecas": ["Carga de gás refrigerante R134a", "Filtro de cabine (antipólen)", "Compressor do ar-condicionado", "Eletroventilador do radiador"],
     "mo": (350, 950), "dur": (0, 2),
     "desc": ["Ar-condicionado não gela", "Mau cheiro ao ligar o ar-condicionado", "Ar-condicionado gela e para de gelar",
              "Barulho no compressor ao ligar o ar", "Higienização e recarga do ar-condicionado antes do verão"]},
    {"tema": "transmissao", "tipo": "TrocaPecas", "esp": "Transmissao", "preset": "Embreagem e transmissão", "peso": 6,
     "pecas": ["Kit de embreagem", "Rolamento/atuador de embreagem", "Óleo de câmbio 75W90", "Junta homocinética", "Coxim do câmbio"],
     "mo": (1100, 2000), "dur": (1, 3),
     "desc": ["Embreagem patinando em subidas", "Pedal de embreagem duro", "Estalo ao fazer curvas fechadas (homocinética)",
              "Marchas arranhando ao engatar", "Troca do kit de embreagem com {km} km"]},
    {"tema": "arrefecimento", "tipo": "TrocaPecas", "esp": "Motor", "preset": "Correia dentada e arrefecimento", "peso": 4,
     "pecas": ["Radiador", "Válvula termostática", "Mangueira superior do radiador", "Aditivo para radiador", "Bomba d'água", "Eletroventilador do radiador"],
     "mo": (450, 1100), "dur": (0, 2),
     "desc": ["Vazamento de água no radiador", "Ponteiro de temperatura subindo no trânsito", "Ventoinha não liga",
              "Reservatório de água secando em poucos dias"]},
    {"tema": "escapamento", "tipo": "TrocaPecas", "esp": "Mecanica", "preset": "Pré-viagem", "peso": 3,
     "pecas": ["Silencioso traseiro", "Tubo intermediário do escapamento", "Catalisador"],
     "mo": (300, 700), "dur": (0, 1),
     "desc": ["Escapamento barulhento", "Cheiro de gás dentro do carro", "Catalisador entupido — carro sem força"]},
    {"tema": "funilaria", "tipo": "Outro", "esp": "Funilaria", "preset": None, "peso": 4,
     "pecas": ["Para-choque dianteiro", "Retrovisor externo", "Farol dianteiro"],
     "mo": (700, 1800), "dur": (2, 5),
     "desc": ["Reparo de para-choque após batida leve no estacionamento", "Troca de retrovisor quebrado",
              "Farol trincado e embaçado", "Pequeno amassado na porta traseira"]},
    {"tema": "pintura", "tipo": "Outro", "esp": "Pintura", "preset": None, "peso": 3,
     "pecas": ["Para-choque dianteiro", "Retrovisor externo"],
     "mo": (800, 2500), "dur": (2, 6),
     "desc": ["Repintura do para-choque riscado", "Pintura de capô com verniz queimado pelo sol",
              "Retoque de pintura na lateral após raspão", "Polimento e cristalização da pintura"]},
    {"tema": "interior", "tipo": "Manutencao", "esp": "Interior", "preset": None, "peso": 2,
     "pecas": ["Forração de teto", "Cinto de segurança dianteiro"],
     "mo": (300, 900), "dur": (1, 3),
     "desc": ["Troca de forração de teto descolada", "Cinto de segurança não recolhe — substituição",
              "Higienização interna e reparo do forro da porta", "Reparo no banco do motorista"]},
    {"tema": "acessorios", "tipo": "Outro", "esp": "Eletrica", "preset": None, "peso": 4,
     "pecas": ["Central multimídia", "Câmera de ré", "Película de controle solar (kit)", "Alarme automotivo"],
     "mo": (250, 600), "dur": (0, 1),
     "desc": ["Instalação de central multimídia com câmera de ré", "Aplicação de película de controle solar",
              "Instalação de alarme com bloqueio", "Instalação de câmera de ré"]},
    {"tema": "previagem", "tipo": "Revisao", "esp": "Mecanica", "preset": "Pré-viagem", "peso": 5,
     "pecas": ["Lâmpada H4 (par)", "Fluido de freio DOT4 (500 ml)", "Aditivo para radiador", "Filtro de cabine (antipólen)"],
     "mo": (250, 500), "dur": (0, 0),
     "desc": ["Checagem pré-viagem de férias", "Revisão rápida antes de viagem para o litoral", "Checagem antes de viagem longa com a família"]},
]
CONTEXTOS_OS = ["", "", "", "Cliente relata que começou há cerca de uma semana.", "Problema mais evidente com o motor frio.",
                "Cliente pediu orçamento antes de executar.", "Veículo usado diariamente para trabalho.",
                "Cliente vai aguardar na loja se for rápido.", "Cliente informa que outro mecânico não resolveu.",
                "Carro parado desde o fim de semana.", "Cliente pediu para guardar as peças substituídas."]
ALERTAS_POR_TEMA = {
    "revisao": ["Durante a revisão, identificadas pastilhas dianteiras abaixo de 3 mm — recomendada a troca.",
                "Vazamento leve no retentor do comando identificado na revisão.", "Bateria com carga fraca no teste — recomendada substituição."],
    "oleo": ["Filtro de ar muito saturado — recomendada troca imediata.", "Vazamento no bujão do cárter; necessária troca da arruela e do bujão."],
    "freios": ["Disco de freio abaixo da espessura mínima — necessário trocar o par.", "Flexível de freio ressecado com início de trinca."],
    "suspensao": ["Bandeja com bucha rompida além do amortecedor — recomendada troca.", "Terminal de direção com folga identificado na desmontagem."],
    "motor": ["Bomba d'água com folga no eixo — recomendada troca junto com a correia.", "Velas carbonizadas e cabos com fuga de corrente."],
    "eletrica": ["Alternador carregando abaixo do especificado — recomendada substituição.", "Chicote do farol oxidado precisando de reparo."],
    "ar": ["Vazamento no condensador detectado com UV.", "Compressor com embreagem eletromagnética desgastada."],
    "transmissao": ["Volante do motor com marcas de superaquecimento — recomendada retífica.", "Coxim do câmbio rompido identificado na desmontagem."],
    "arrefecimento": ["Válvula termostática travada além do vazamento no radiador.", "Tampa do reservatório sem pressão."],
    "escapamento": ["Coxim do escapamento rompido.", "Sonda lambda com leitura irregular após troca do catalisador."],
    "funilaria": ["Suporte do para-choque quebrado — necessário substituir.", "Grade do radiador danificada no impacto."],
    "pintura": ["Ferrugem sob a pintura — necessário tratar a chapa antes de pintar.", "Verniz comprometido em peça vizinha — ampliar área de pintura."],
    "interior": ["Trava do banco também danificada — necessário substituir.", "Fiação do airbag lateral solta sob o banco."],
    "acessorios": ["Chicote original sem adaptador — necessário módulo de interface.", "Alto-falante dianteiro rasgado."],
    "previagem": ["Pneu dianteiro com bolha lateral — recomendada troca antes da viagem.", "Palhetas ressecadas e farol desregulado."],
}
OBS_ALERTA_APROVADO = ["Cliente autorizou por telefone.", "Cliente aprovou pelo WhatsApp após receber fotos.",
                       "Aprovado pelo cliente na própria loja.", "Cliente autorizou desde que entregue no prazo."]
OBS_ALERTA_RECUSADO = ["Cliente vai avaliar e fazer em outro momento.", "Cliente achou o valor alto e não autorizou.",
                       "Cliente pediu para seguir só com o serviço original.", "Cliente vai trocar a peça por conta própria."]
MOTIVOS_REQ_REJEITADA = [
    "Peça fora de linha no fornecedor — substituída por equivalente compatível em estoque.",
    "Prazo de entrega do fornecedor acima de 15 dias — usada peça compatível disponível.",
    "Cotação acima do valor aprovado pelo cliente — optou-se pela peça paralela homologada.",
]
AVARIAS = ["Nenhuma avaria visível", "Risco leve no para-choque traseiro", "Pequeno amassado na porta dianteira direita",
           "Trinca no farol esquerdo", "Arranhões na roda dianteira esquerda", "Pintura do capô queimada pelo sol",
           "Lanterna traseira com infiltração", "Para-brisa com pequena trinca no canto inferior", "Nenhuma avaria visível",
           "Nenhuma avaria visível", "Retrovisor direito com capa solta"]
CONDICAO_GERAL = ["Lataria e pintura em bom estado, pneus com cerca de 60% de vida útil, interior limpo",
                  "Veículo em bom estado geral, pneus meia-vida, bancos sem rasgos",
                  "Lataria com marcas de uso, pneus dianteiros gastos, painel sem avarias",
                  "Veículo bem conservado, pneus novos, interior com sujeira leve",
                  "Desgaste compatível com a idade, estofado do motorista com desgaste"]
COMBUSTIVEL_NIVEL = ["Reserva", "1/4", "1/2", "1/2", "3/4", "Cheio"]
PERTENCES = ["Nenhum", "Nenhum", "Nenhum", "Cadeirinha infantil no banco traseiro", "Guarda-chuva e documentos no porta-luvas",
             "Óculos de sol no console", "Carregador de celular", "Kit de ferramentas no porta-malas"]
OBS_TECNICAS = ["Cliente relata o problema há alguns dias.", "Sem histórico de manutenção na loja.",
                "Última revisão feita há mais de um ano segundo o cliente.", "Cliente autorizou teste de rodagem.",
                "Veículo chegou guinchado.", "Odômetro conferido na presença do cliente."]

MOTIVOS_REJEICAO = [
    "Cliente desistiu da compra.", "Cliente optou por um veículo da concorrência.", "Cliente achou o preço acima do orçamento.",
    "Cliente pediu mais tempo para pensar e não retornou.", "Avaliação do veículo de troca ficou abaixo do esperado pelo cliente.",
    "Cliente decidiu esperar o lançamento do modelo novo.",
]
MOTIVOS_NEGATIVA = [
    "Cliente com restrição no CPF (SPC/Serasa).", "Renda comprovada insuficiente para o valor solicitado.",
    "Documentação incompleta para análise de crédito.", "Comprometimento de renda acima do limite da financiadora.",
    "Score de crédito abaixo do mínimo exigido.",
]
MOTIVOS_CANCELAMENTO_PROPOSTA = [
    "Cliente perdeu o emprego e desistiu da compra após a aprovação.",
    "Vistoria cautelar apontou divergência e o cliente desistiu.",
    "Cliente não conseguiu vender o carro atual a tempo.",
    "Cliente encontrou condição melhor em outra loja após aprovar.",
    "Problema de saúde na família — cliente cancelou a compra.",
    "Cônjuge não concordou com a compra.",
]
MOTIVOS_CANCELAMENTO_CONSIGNACAO = [
    "Proprietário decidiu vender o veículo por conta própria.", "Proprietário retirou o veículo para uso pessoal.",
    "Desacordo sobre o valor mínimo de venda.", "Proprietário optou por consignar em outra loja.",
]
OBS_TD = [None, None, None, "Cliente quer comparar com o modelo da concorrência.", "Cliente veio com a esposa.",
          "Interesse em financiamento.", "Cliente vai dar o carro atual na troca.", "Quer testar em estrada.",
          "Cliente indicou por outro comprador."]
FINANCIADORAS = ["Aliança Financiamentos", "Crédito Paulista S.A.", "Banco Horizonte Veículos", "Fortis Financeira",
                 "Valor Crédito e Financiamento"]

FORNECEDORES = [
    "Distribuidora Campineira de Autopeças Ltda", "Autopeças Paulista Comércio Ltda", "Rota Sul Distribuidora de Peças",
    "Central Freios e Embreagens Ltda", "Mega Suspensões Comércio de Peças", "Eletro Auto Baterias e Elétrica Ltda",
    "Frio Car Peças para Ar-Condicionado", "Lubrificantes Interior Paulista Ltda", "Escapamentos Imperial Comércio",
    "Funilaria Express Peças de Lataria", "Acessórios Automotivos Vale do Sol", "Distribuidora Nacional de Filtros",
    "Transmissões Viracopos Comércio de Peças", "Rede Autopeças Valinhos Ltda", "Importadora de Componentes Automotivos Bandeirantes",
]


# =====================================================================
# TEMPLATES (placeholders no formato [MAIÚSCULAS] do sistema)
# =====================================================================

TPL_TERMO_ENTREGA = """TERMO DE ENTREGA DE VEÍCULO

Pelo presente termo, [NOME DA LOJA/CONCESSIONÁRIA], inscrita no CNPJ nº [CNPJ DA LOJA], entrega ao(à) Sr(a). [NOME DO CLIENTE], portador(a) do CPF nº [CPF DO CLIENTE], o veículo abaixo descrito, nas condições apresentadas e aceitas no momento da venda:

Marca/Modelo: [MARCA E MODELO]
Ano: [ANO DE FABRICAÇÃO/MODELO]
Cor: [COR]
Placa: [PLACA]
RENAVAM: [RENAVAM]
Quilometragem na entrega: [QUILOMETRAGEM] km
Valor da venda: R$ [VALOR]
Forma de pagamento: [FORMA DE PAGAMENTO]

O(A) COMPRADOR(A) declara que:
1. Vistoriou o veículo no ato da entrega e o recebeu em condições de uso, com os itens obrigatórios de segurança (estepe, macaco, chave de roda, triângulo).
2. Recebeu manual do proprietário, [NÚMERO] chave(s), CRLV e documento de transferência devidamente preenchido.
3. Está ciente das pendências identificadas na vistoria: [DESCREVER PENDÊNCIAS OU "NENHUMA"].
4. Está ciente da garantia legal de 90 (noventa) dias para motor e câmbio, conforme o Código de Defesa do Consumidor, e das condições adicionais: [CONDIÇÕES DE GARANTIA].
5. Assume, a partir desta data e hora, a responsabilidade civil, administrativa e criminal sobre o veículo, inclusive multas e tributos.

Local e data: [CIDADE], [DATA]

_______________________________          _______________________________
[NOME DO CLIENTE]                         [NOME DO VENDEDOR/REPRESENTANTE]
Comprador(a)                              [NOME DA LOJA/CONCESSIONÁRIA]"""

TPL_CONSIGNACAO = """CONTRATO DE CONSIGNAÇÃO PARA VENDA DE VEÍCULO

Pelo presente instrumento particular, de um lado [NOME DA LOJA/CONCESSIONÁRIA], inscrita no CNPJ nº [CNPJ DA LOJA], doravante CONSIGNATÁRIA, e de outro [NOME DO PROPRIETÁRIO], CPF nº [CPF DO PROPRIETÁRIO], doravante CONSIGNANTE, ajustam o seguinte:

1. OBJETO — O CONSIGNANTE entrega à CONSIGNATÁRIA, para exposição e venda, o veículo [MARCA E MODELO], ano [ANO DE FABRICAÇÃO/MODELO], cor [COR], placa [PLACA], RENAVAM [RENAVAM], com [QUILOMETRAGEM] km na entrada.

2. PRAZO — [PRAZO EM DIAS] dias corridos a contar da assinatura, renováveis por acordo entre as partes.

3. VALOR E REMUNERAÇÃO — Valor de venda anunciado: R$ [VALOR DE VENDA]. O CONSIGNANTE receberá R$ [VALOR LÍQUIDO DO PROPRIETÁRIO] (ou [PERCENTUAL DO PROPRIETÁRIO]% do valor efetivamente recebido), cabendo à CONSIGNATÁRIA a diferença a título de comissão.

4. OBRIGAÇÕES DA CONSIGNATÁRIA — zelar pela guarda do veículo, anunciá-lo nos canais da loja e repassar o valor ao CONSIGNANTE em até [PRAZO DE REPASSE] dias úteis após a quitação pelo comprador.

5. OBRIGAÇÕES DO CONSIGNANTE — garantir que o veículo está livre de ônus, multas e restrições e entregar a documentação de transferência quando solicitada.

6. RESCISÃO — qualquer parte pode rescindir mediante aviso prévio de [PRAZO DE AVISO PRÉVIO] dias, desde que não haja negociação em andamento com terceiro.

Local e data: [CIDADE], [DATA]

_______________________________          _______________________________
[NOME DO PROPRIETÁRIO]                     [NOME DO REPRESENTANTE DA LOJA]"""

TPL_VISTORIA_OS = """CONTRATO DE ORDEM DE SERVIÇO — VISTORIA DE ENTRADA

OS nº [NÚMERO DA OS]
Cliente: [NOME DO CLIENTE] — CPF [CPF DO CLIENTE]
Veículo: [MARCA E MODELO] — Placa [PLACA] — Quilometragem: [QUILOMETRAGEM] km
Data e hora da vistoria: [DATA E HORA]
Recepcionista responsável: [NOME DO RECEPCIONISTA]

1. ESTADO DO VEÍCULO NA ENTRADA
Condição geral (lataria, pneus, vidros, interior): [DESCREVER]
Avarias identificadas: [DESCREVER OU "NENHUMA AVARIA VISÍVEL"]
Nível de combustível: [NÍVEL]
Pertences deixados no veículo: [DESCREVER OU "NENHUM"]

2. SERVIÇO SOLICITADO
Descrição: [DESCRIÇÃO DO SERVIÇO]
Peças previstas: [LISTAR PEÇAS]
Observações técnicas: [OBSERVAÇÕES]

3. PRAZO E VALOR ESTIMADOS
Previsão de conclusão: [DATA PREVISTA] — Valor estimado: R$ [VALOR ESTIMADO]

4. CIÊNCIA DO CLIENTE
O(A) cliente declara estar ciente do estado do veículo registrado nesta vistoria e dos serviços previstos. Serviços adicionais só serão executados mediante nova autorização. A aprovação é registrada no sistema pela recepção."""

TPL_TEST_DRIVE = """TERMO DE RESPONSABILIDADE — TEST DRIVE

Eu, [NOME DO CLIENTE], CPF nº [CPF DO CLIENTE], CNH nº [NÚMERO DA CNH], categoria [CATEGORIA DA CNH], válida até [VALIDADE DA CNH], declaro estar ciente e de acordo com as condições abaixo para realizar test drive no veículo:

Marca/Modelo: [MARCA E MODELO] — Ano: [ANO] — Placa: [PLACA]
Data e horário: [DATA E HORA] — Acompanhante da loja: [NOME DO VENDEDOR]

1. Possuo CNH válida e compatível com a categoria do veículo e assumo a condução durante todo o trajeto.
2. Respeitarei a legislação de trânsito e o trajeto indicado pela loja, com duração máxima de [DURAÇÃO] minutos.
3. Multas, danos ao veículo ou a terceiros decorrentes de imprudência, negligência ou infração cometida durante o test drive são de minha responsabilidade, inclusive a franquia do seguro.
4. A loja responde pelas condições mecânicas e de segurança do veículo entregue para o test drive.

Local e data: [CIDADE], [DATA]

_______________________________
[NOME DO CLIENTE]"""

TPL_FINANCIADORA = """RESPOSTA DA FINANCIADORA — REGISTRO DO VENDEDOR

Financiadora: [NOME DA FINANCIADORA]
Contato: [NOME DO ATENDENTE / TELEFONE / E-MAIL]
Data do retorno: [DATA]
Proposta/protocolo: [NÚMERO DA PROPOSTA NA FINANCIADORA]

Condições aprovadas:
- Valor do veículo: R$ [VALOR DO VEÍCULO]
- Entrada: R$ [VALOR DA ENTRADA]
- Valor financiado: R$ [VALOR FINANCIADO]
- Parcelas: [PARCELAS]x de R$ [VALOR DA PARCELA]
- Taxa de juros: [TAXA]% a.m. — CET: [CET]% a.a.
- Primeira parcela: [DATA DA PRIMEIRA PARCELA]

Exigências e observações: [CONDIÇÕES ADICIONAIS, VALIDADE DA APROVAÇÃO, DOCUMENTOS PENDENTES]"""

# Templates de documento importados: SÓ finalidades que o seed do Web não cobre
# (o seed já cria "Termo de responsabilidade — Test drive (padrão)" e "Contrato
# de OS — vistoria de entrada (padrão)"). O sistema NÃO substitui placeholders
# (o template só é copiado pro editor — SeletorTemplateDocumento), então o texto
# é genérico, com lacunas "______" pro vendedor completar, nunca [MAIÚSCULAS].
TEMPLATES = [
    ("Termo de entrega de veículo — modelo da loja", """TERMO DE ENTREGA DE VEÍCULO

A loja entrega ao(à) comprador(a) identificado(a) na assinatura deste termo o veículo descrito na proposta de venda, nas condições apresentadas e aceitas no momento da negociação.

O(A) COMPRADOR(A) declara que:
1. Vistoriou o veículo no ato da entrega e o recebeu em condições de uso, com estepe, macaco, chave de roda e triângulo.
2. Recebeu o manual do proprietário, as chaves do veículo, o CRLV e o documento de transferência devidamente preenchido.
3. Está ciente das observações registradas na vistoria de entrega.
4. Está ciente da garantia legal de 90 (noventa) dias para motor e câmbio, conforme o Código de Defesa do Consumidor.
5. Assume, a partir da assinatura deste termo, a responsabilidade civil, administrativa e criminal sobre o veículo, inclusive multas e tributos.

Observações da entrega: ______________________________"""),
    ("Contrato de consignação — modelo da loja", """CONTRATO DE CONSIGNAÇÃO PARA VENDA DE VEÍCULO

A loja (CONSIGNATÁRIA) e o(a) proprietário(a) identificado(a) no cadastro desta consignação (CONSIGNANTE) ajustam o seguinte:

1. OBJETO — O CONSIGNANTE entrega o veículo descrito no cadastro para exposição e venda pela CONSIGNATÁRIA.
2. PRAZO — Vigência pelo prazo cadastrado no sistema, renovável por acordo entre as partes.
3. VALOR E REMUNERAÇÃO — O CONSIGNANTE receberá o valor líquido (ou o percentual) registrado no cadastro; a diferença em relação ao valor de venda cabe à CONSIGNATÁRIA a título de comissão.
4. OBRIGAÇÕES DA CONSIGNATÁRIA — Zelar pela guarda do veículo, anunciá-lo nos canais da loja e repassar o valor ao CONSIGNANTE em até 5 (cinco) dias úteis após a quitação pelo comprador.
5. OBRIGAÇÕES DO CONSIGNANTE — Garantir que o veículo está livre de ônus, multas e restrições e entregar a documentação de transferência quando solicitada.
6. RESCISÃO — Qualquer parte pode rescindir mediante aviso prévio de 7 (sete) dias, desde que não haja negociação em andamento com terceiro.

Condições adicionais: ______________________________"""),
    ("Resposta da financiadora — registro detalhado", """RESPOSTA DA FINANCIADORA

Financiadora: ______________________
Contato (atendente / telefone / e-mail): ______________________
Data do retorno: ____/____/______
Número da proposta na financiadora: ______________________

Condições aprovadas:
- Entrada: R$ ______________
- Valor financiado: R$ ______________
- Parcelas: ____ x de R$ ______________
- Taxa de juros: ______ % a.m. — CET: ______ % a.a.
- Primeira parcela: ____/____/______

Exigências e validade da aprovação: ______________________________"""),
]


# =====================================================================
# DESPESAS-MODELO (valor = valor ATUAL; histórico via variações mensais)
# =====================================================================
# (nome, setor, tipo, valor_atual, regra_de_variacao)
DESPESAS_MODELO = [
    # Geral (o Financeiro divide meio a meio entre os setores) — só o que é de fato compartilhado
    ("Energia elétrica", "Geral", "Utilidades", 1300, "energia"),
    ("Água e esgoto", "Geral", "Utilidades", 350, "agua"),
    ("Internet e telefonia", "Geral", "Utilidades", 300, "reajuste_jan"),
    ("Contabilidade terceirizada", "Geral", "Servicos", 1200, "reajuste_jan"),
    ("Sistema de gestão e softwares", "Geral", "Servicos", 350, "fixo"),
    ("Seguro predial e responsabilidade civil", "Geral", "Seguro", 400, "reajuste_jan"),
    ("IPTU e taxas municipais", "Geral", "Impostos", 450, "reajuste_jan"),
    ("Limpeza e conservação", "Geral", "Servicos", 700, "dissidio"),
    ("Material de limpeza e copa", "Geral", "Outros", 250, "ruido15"),
    ("Salários administrativos (recepção e financeiro)", "Geral", "Salario", 4800, "salario_recepcao"),
    # Oficina — mecânicos comissionados sobre a mão de obra (prática comum em oficina)
    ("Aluguel do galpão da oficina", "Oficina", "Aluguel", 1800, "aluguel"),
    ("Salário do chefe de oficina", "Oficina", "Salario", 3400, "dissidio"),
    ("Comissão dos mecânicos (30% da mão de obra)", "Oficina", "Salario", 6000, "comissao_mecanicos"),
    ("Encargos e benefícios da oficina", "Oficina", "Outros", 1500, "encargos_oficina"),
    ("Reposição de peças de giro (óleo, filtros, fluidos)", "Oficina", "Outros", 1500, "giro_pecas"),
    ("Ferramentas e consumíveis da oficina", "Oficina", "Manutencao", 300, "ruido15"),
    ("Descarte de resíduos (óleo usado e filtros)", "Oficina", "Servicos", 150, "fixo"),
    # Concessionária
    ("Aluguel do showroom", "Concessionaria", "Aluguel", 4500, "aluguel"),
    ("Salários da equipe de vendas", "Concessionaria", "Salario", 8400, "salario_vendas"),
    ("Salário do gerente de vendas", "Concessionaria", "Salario", 5000, "dissidio"),
    ("Comissões de vendas", "Concessionaria", "Salario", 5000, "comissoes"),
    ("Encargos e benefícios da concessionária", "Concessionaria", "Outros", 2200, "salario_vendas"),
    ("Marketing digital e portais de anúncio", "Concessionaria", "Marketing", 2500, "marketing"),
    ("Despachante e documentação de transferência", "Concessionaria", "Servicos", 2000, "despachante"),
    ("Seguro da frota em estoque", "Concessionaria", "Seguro", 1500, "reajuste_jan"),
    ("Preparação estética dos veículos (lavagem e polimento)", "Concessionaria", "Servicos", 1500, "preparacao"),
]

# ---- Despesas esporádicas (§8.1) — (nome, setor, categoria, min, max, passo) ----
# Categoria (texto livre do formulário de despesas) — taxonomia FIXA para que os
# relatórios agrupem certo. Chave = nome exato da despesa (modelo ou esporádica).
CATEGORIA_DESPESA = {
    # --- recorrentes (modelo) ---
    "Energia elétrica": "Energia elétrica",
    "Água e esgoto": "Água e esgoto",
    "Internet e telefonia": "Telefonia e internet",
    "Contabilidade terceirizada": "Serviços contábeis",
    "Sistema de gestão e softwares": "Software e sistemas",
    "Seguro predial e responsabilidade civil": "Seguros",
    "Seguro da frota em estoque": "Seguros",
    "IPTU e taxas municipais": "Impostos e taxas",
    "Limpeza e conservação": "Limpeza e conservação",
    "Material de limpeza e copa": "Limpeza e conservação",
    "Salários administrativos (recepção e financeiro)": "Folha de pagamento",
    "Salário do chefe de oficina": "Folha de pagamento",
    "Salários da equipe de vendas": "Folha de pagamento",
    "Salário do gerente de vendas": "Folha de pagamento",
    "Comissão dos mecânicos (30% da mão de obra)": "Comissões",
    "Comissões de vendas": "Comissões",
    "Encargos e benefícios da oficina": "Encargos e benefícios",
    "Encargos e benefícios da concessionária": "Encargos e benefícios",
    "Aluguel do galpão da oficina": "Aluguel",
    "Aluguel do showroom": "Aluguel",
    "Reposição de peças de giro (óleo, filtros, fluidos)": "Insumos da oficina",
    "Ferramentas e consumíveis da oficina": "Insumos da oficina",
    "Descarte de resíduos (óleo usado e filtros)": "Descarte de resíduos",
    "Marketing digital e portais de anúncio": "Marketing e publicidade",
    "Despachante e documentação de transferência": "Documentação e despachante",
    "Preparação estética dos veículos (lavagem e polimento)": "Preparação de veículos",
    # --- esporádicas: manutenção ---
    "Conserto do ar-condicionado do showroom": "Manutenção predial",
    "Desentupimento da caixa separadora de óleo": "Manutenção predial",
    "Pintura da fachada": "Manutenção predial",
    "Recarga e inspeção dos extintores": "Manutenção predial",
    "Reparo de infiltração no telhado": "Manutenção predial",
    "Reparo do portão automático": "Manutenção predial",
    "Reparo no piso do showroom": "Manutenção predial",
    "Troca de lâmpadas e reparo na rede elétrica": "Manutenção predial",
    "Manutenção do gerador de energia": "Manutenção de equipamentos",
    "Conserto do elevador automotivo": "Manutenção de equipamentos",
    "Manutenção corretiva do compressor de ar": "Manutenção de equipamentos",
    "Manutenção da alinhadora/balanceadora": "Manutenção de equipamentos",
    # --- esporádicas: investimentos ---
    "Computadores e impressora": "Investimento em tecnologia",
    "Licença de software de orçamentos": "Investimento em tecnologia",
    "Equipamento de recarga de ar-condicionado": "Investimento em equipamentos",
    "Scanner automotivo de diagnóstico": "Investimento em equipamentos",
    "Renovação da frota de apoio (utilitário de entregas)": "Investimento em equipamentos",
    "Mobiliário do showroom": "Investimento em infraestrutura",
    "Reforma completa do showroom": "Investimento em infraestrutura",
    "Sinalização e comunicação visual da loja": "Investimento em infraestrutura",
    "Treinamento e certificação de mecânicos": "Treinamento e capacitação",
    "Campanha de marketing sazonal (feirão)": "Marketing e publicidade",
    "Feirão de fim de ano com estande em shopping": "Marketing e publicidade",
    # --- esporádicas: perdas ---
    "Avaria em veículo de cliente indenizada": "Perdas e avarias",
    "Baixa de estoque de peças obsoletas": "Perdas e avarias",
    "Ferramenta furtada da oficina": "Perdas e avarias",
    "Franquia de seguro — sinistro de veículo do estoque": "Perdas e avarias",
    "Motor fundido — troca de motor de veículo do estoque": "Perdas e avarias",
    "Reparo de avaria em veículo do estoque (pátio)": "Perdas e avarias",
    "Sinistro com perda parcial de veículo do estoque": "Perdas e avarias",
    "Multa de trânsito em test drive": "Multas",
    "Peça danificada no manuseio": "Perdas e avarias",
    "Ferramentas especiais (kit de sincronismo e torquímetros)": "Investimento em equipamentos",
    "Câmeras de segurança e monitoramento": "Investimento em infraestrutura",
    "Inadimplência — cheque devolvido de cliente": "Inadimplência",
}

EXTRAS_MANUTENCAO = [
    ("Conserto do elevador automotivo", "Oficina", "Manutencao", 600, 1800, 100),
    ("Manutenção corretiva do compressor de ar", "Oficina", "Manutencao", 400, 1200, 100),
    ("Conserto do ar-condicionado do showroom", "Concessionaria", "Manutencao", 1200, 3500, 100),
    ("Reparo de infiltração no telhado", "Geral", "Manutencao", 800, 2500, 100),
    ("Troca de lâmpadas e reparo na rede elétrica", "Geral", "Manutencao", 300, 1200, 100),
    ("Manutenção do gerador de energia", "Geral", "Manutencao", 400, 1200, 100),
    ("Pintura da fachada", "Concessionaria", "Manutencao", 3000, 7000, 500),
    ("Reparo do portão automático", "Geral", "Manutencao", 300, 900, 100),
    ("Recarga e inspeção dos extintores", "Geral", "Manutencao", 300, 700, 50),
    ("Manutenção da alinhadora/balanceadora", "Oficina", "Manutencao", 400, 1000, 100),
    ("Desentupimento da caixa separadora de óleo", "Oficina", "Manutencao", 300, 700, 50),
    ("Reparo no piso do showroom", "Concessionaria", "Manutencao", 1500, 4000, 500),
]
EXTRAS_PERDAS = [
    ("Peça danificada no manuseio", "Oficina", "Outros", 150, 600, 50),
    ("Ferramenta furtada da oficina", "Oficina", "Outros", 300, 1000, 100),
    ("Multa de trânsito em test drive", "Concessionaria", "Outros", 150, 300, 50),
    ("Franquia de seguro — sinistro de veículo do estoque", "Concessionaria", "Outros", 3000, 8000, 500),
    ("Avaria em veículo de cliente indenizada", "Oficina", "Outros", 300, 1200, 100),
    ("Baixa de estoque de peças obsoletas", "Oficina", "Outros", 200, 800, 100),
    ("Inadimplência — cheque devolvido de cliente", "Concessionaria", "Outros", 1000, 5000, 500),
    ("Reparo de avaria em veículo do estoque (pátio)", "Concessionaria", "Outros", 800, 3000, 100),
]
EXTRAS_INVESTIMENTOS = [
    ("Scanner automotivo de diagnóstico", "Oficina", "Investimento", 1500, 3000, 500),
    ("Computadores e impressora", "Geral", "Investimento", 1500, 4000, 500),
    ("Licença de software de orçamentos", "Oficina", "Investimento", 300, 800, 100),
    ("Campanha de marketing sazonal (feirão)", "Concessionaria", "Marketing", 3000, 9000, 500),
    ("Treinamento e certificação de mecânicos", "Oficina", "Investimento", 500, 1500, 100),
    ("Mobiliário do showroom", "Concessionaria", "Investimento", 2000, 6000, 500),
    ("Câmeras de segurança e monitoramento", "Geral", "Investimento", 1000, 3000, 500),
    ("Ferramentas especiais (kit de sincronismo e torquímetros)", "Oficina", "Investimento", 500, 1500, 100),
    ("Equipamento de recarga de ar-condicionado", "Oficina", "Investimento", 1500, 3000, 500),
    ("Sinalização e comunicação visual da loja", "Concessionaria", "Investimento", 1500, 4000, 500),
]
EXTRAS_GRANDES = [
    ("Reforma completa do showroom", "Concessionaria", "Investimento", 35000, 55000, 1000),
    ("Feirão de fim de ano com estande em shopping", "Concessionaria", "Marketing", 15000, 22000, 1000),
    ("Motor fundido — troca de motor de veículo do estoque", "Concessionaria", "Outros", 15000, 22000, 1000),
    ("Sinistro com perda parcial de veículo do estoque", "Concessionaria", "Outros", 18000, 28000, 1000),
    ("Renovação da frota de apoio (utilitário de entregas)", "Concessionaria", "Investimento", 25000, 40000, 1000),
]


# =====================================================================
# ESTADO GLOBAL DO GERADOR
# =====================================================================

class Gerador:
    def __init__(self):
        self.usuarios = []
        self.clientes = []
        self.fornecedores = []
        self.componentes = []
        self.equivalentes = []
        self.presets = []
        self.templates = []
        self.despesas = []
        self.veiculos_venda = []
        self.consignacoes = []
        self.veiculos_cliente = []
        self.propostas = []
        self.ordens = []
        self.test_drives = []
        self.extras = []
        self.fechamentos = []
        # auxiliares
        self.usr_por_chave = {}
        self.cli_por_chave = {}
        self.comp_por_nome = {}
        self.comp_por_chave = {}
        self.preset_por_nome = {}
        self.vv_por_chave = {}
        self.csg_por_chave = {}
        self.vc_por_chave = {}
        self.eventos_cliente = defaultdict(list)   # chave -> [datetime]
        self.cnpj_loja = novo_cnpj()
        self.seq = Counter()

    def chave(self, prefixo):
        self.seq[prefixo] += 1
        return f"{prefixo}-{self.seq[prefixo]}"

    # -----------------------------------------------------------------
    # Usuários
    # -----------------------------------------------------------------
    def gerar_usuarios(self):
        equipe = [
            # (tipo, nivel, especialidade, data de contratação)
            ("Admin", None, None, None),
            ("GerenteVendas", "Senior", None, datetime(2023, 8, 14, 9)),
            ("Vendedor", "Senior", None, datetime(2023, 8, 21, 9)),
            ("Vendedor", "Pleno", None, datetime(2023, 9, 4, 9)),
            ("Vendedor", "Pleno", None, datetime(2023, 9, 11, 9)),
            ("Vendedor", "Junior", None, datetime(2023, 10, 2, 9)),
            ("Vendedor", "Junior", None, datetime(2024, 6, 10, 9)),
            ("Vendedor", "Junior", None, datetime(2025, 3, 3, 9)),
            ("ChefeOficina", "Senior", None, datetime(2023, 8, 7, 9)),
            ("Mecanico", "Senior", "Mecanica", datetime(2023, 8, 14, 8)),
            ("Mecanico", "Senior", "Motor", datetime(2023, 8, 21, 8)),
            ("Mecanico", "Pleno", "Freios", datetime(2023, 9, 4, 8)),
            ("Mecanico", "Pleno", "Suspensao", datetime(2023, 9, 18, 8)),
            ("Mecanico", "Pleno", "Eletrica", datetime(2023, 10, 2, 8)),
            ("Mecanico", "Junior", "ArCondicionado", datetime(2024, 2, 5, 8)),
            ("Mecanico", "Pleno", "Transmissao", datetime(2024, 9, 2, 8)),
            ("Mecanico", "Junior", "Funilaria", datetime(2025, 5, 5, 8)),
            ("Mecanico", "Pleno", "Pintura", datetime(2024, 4, 1, 8)),
            ("Mecanico", "Junior", "Interior", datetime(2025, 2, 3, 8)),
            ("Recepcionista", "Pleno", None, datetime(2023, 9, 4, 8)),
            ("Recepcionista", "Junior", None, datetime(2023, 9, 25, 8)),
            ("Recepcionista", "Junior", None, datetime(2025, 1, 13, 8)),
        ]
        nomes_usados = set()
        for tipo, nivel, esp, contratacao in equipe:
            nome = self._nome_pessoa(nomes_usados)
            partes = sem_acento(nome).lower().split()
            email = f"{partes[0]}.{partes[-1]}@delore.com.br"
            ch = self.chave("usr")
            u = {"chave": ch, "tipo": tipo, "nome": nome, "email": email,
                 "telefone": "19" + "9" + str(rng.randint(10000000, 99999999)), "senha": "Teste@123"}
            if tipo != "Admin":
                u["nivel"] = nivel
                u["dataContratacao"] = fmt(contratacao)
            if esp:
                u["especialidade"] = esp
            self.usuarios.append(u)
            self.usr_por_chave[ch] = u

    def _nome_pessoa(self, usados):
        while True:
            primeiro = rng.choice(NOMES_M if rng.random() < 0.55 else NOMES_F)
            s1, s2 = rng.sample(SOBRENOMES, 2)
            nome = f"{primeiro} {s1} {s2}" if rng.random() < 0.6 else f"{primeiro} {s2}"
            if nome not in usados:
                usados.add(nome)
                return nome

    def funcionarios(self, tipo, data, especialidade=None, folga_dias=7):
        lst = [u for u in self.usuarios if u["tipo"] == tipo
               and (u.get("dataContratacao") is None or p(u["dataContratacao"]) + timedelta(days=folga_dias) <= data)]
        if especialidade:
            lst = [u for u in lst if u.get("especialidade") == especialidade]
        return lst

    def vendedor(self, data):
        lst = self.funcionarios("Vendedor", data, folga_dias=10)
        pesos = [3 if u["nivel"] == "Senior" else 2 if u["nivel"] == "Pleno" else 1.5 for u in lst]
        return rng.choices(lst, weights=pesos)[0]

    def mecanico(self, especialidade, data):
        lst = self.funcionarios("Mecanico", data, especialidade)
        if not lst:
            fallback = {"Transmissao": "Motor", "ArCondicionado": "Eletrica", "Funilaria": "Mecanica", "Pintura": "Funilaria", "Interior": "Mecanica"}.get(especialidade, "Mecanica")
            lst = self.funcionarios("Mecanico", data, fallback) or self.funcionarios("Mecanico", data)
        return rng.choice(lst)

    def recepcionista(self, data):
        return rng.choice(self.funcionarios("Recepcionista", data))

    # -----------------------------------------------------------------
    # Clientes (identidade; dataCriacao é derivada do 1º evento no fim)
    # -----------------------------------------------------------------
    def gerar_clientes(self):
        nomes_usados = {u["nome"] for u in self.usuarios}
        pesos_cidade = [c[4] for c in CIDADES]
        for i in range(QTD_CLIENTES):
            nome = self._nome_pessoa(nomes_usados)
            cidade, uf, ddd, ceps, _ = rng.choices(CIDADES, weights=pesos_cidade)[0]
            ch = self.chave("cli")
            partes = sem_acento(nome).lower().split()
            email = f"{partes[0]}.{partes[-1]}{rng.randint(1, 99)}@{rng.choice(['gmail.com', 'hotmail.com', 'outlook.com', 'yahoo.com.br', 'uol.com.br'])}"
            # 'entrada' = a partir de quando esse cliente pode aparecer em eventos
            # (base de clientes crescendo ao longo dos 3 anos).
            if i < 25:
                entrada = INICIO_OPERACAO
            else:
                entrada = sortear_data(INICIO_OPERACAO, LIMITE - timedelta(days=20), SAZ_NEUTRA)
            c = {
                "chave": ch, "nome": nome, "cpf": novo_cpf(),
                "telefone": ddd + "9" + str(rng.randint(10000000, 99999999)),
                "email": email,
                "endereco": {
                    "logradouro": f"{rng.choice(LOGRADOUROS)} {rng.choice(NOMES_RUA)}",
                    "numero": str(rng.randint(10, 3500)),
                    "complemento": "" if rng.random() < 0.7 else f"Apto {rng.randint(11, 184)}",
                    "bairro": rng.choice(BAIRROS) if cidade == "Campinas" else rng.choice(["Centro", "Jardim América", "Vila Santana", "Jardim Europa", "Parque das Flores"]),
                    "cidade": cidade, "uf": uf, "cep": rng.choice(ceps) + f"{rng.randint(0, 999):03d}",
                },
                "_entrada": entrada,
                "_fidelidade": rng.choices([1, 2, 4], weights=[55, 30, 15])[0],
                "_vip": False,
                "_veiculos": [],
                "_compras": 0,
                "_ultima_os": None,
                "_os_count": 0,
            }
            self.clientes.append(c)
            self.cli_por_chave[ch] = c
        # VIPs: clientes antigos, fiéis — compram carro, fazem várias revisões, alguns consignam.
        antigos = [c for c in self.clientes if c["_entrada"] < INICIO_OPERACAO + timedelta(days=200)]
        for c in rng.sample(antigos, QTD_CLIENTES_VIP):
            c["_vip"] = True
            c["_fidelidade"] = 6

    def evento(self, cli_chave, data):
        self.eventos_cliente[cli_chave].append(data)

    def escolher_cliente(self, data, papel, excluir=()):
        """papel: 'comprador' | 'interessado' | 'consignante'."""
        elegiveis = [c for c in self.clientes if c["_entrada"] <= data and c["chave"] not in excluir]
        pesos = []
        for c in elegiveis:
            w = 1.0
            if papel == "comprador":
                if c["_compras"] >= (2 if c["_vip"] else 1):
                    w = 0.02
                elif c["_vip"]:
                    w = 6
            elif papel == "consignante" and c["_vip"]:
                w = 4
            if not self.eventos_cliente[c["chave"]]:
                w *= 2.5  # prioriza quem ainda não tem histórico
            pesos.append(w)
        return rng.choices(elegiveis, weights=pesos)[0]

    # -----------------------------------------------------------------
    # Cadastros de suporte
    # -----------------------------------------------------------------
    def gerar_fornecedores(self):
        for nome in FORNECEDORES[:QTD_FORNECEDORES]:
            cidade, uf, ddd, ceps, _ = rng.choice(CIDADES[:6])
            ch = self.chave("forn")
            slug = sem_acento(nome.split()[0]).lower()
            self.fornecedores.append({
                "chave": ch, "nome": nome, "cnpj": novo_cnpj(),
                "email": f"vendas@{slug}{rng.randint(1, 99)}.com.br",
                "telefone": ddd + str(rng.randint(30000000, 39999999)),
                "endereco": {"logradouro": f"Avenida {rng.choice(['John Boyd Dunlop', 'das Indústrias', 'Lix da Cunha', 'Mercedes-Benz', 'Comendador Aladino Selmi'])}",
                             "numero": str(rng.randint(100, 5000)), "complemento": rng.choice(["", "Galpão 2", "Bloco B"]),
                             "bairro": "Distrito Industrial", "cidade": cidade, "uf": uf, "cep": rng.choice(ceps) + f"{rng.randint(0, 999):03d}"},
            })
        # fornecedor por sistema (coerência)
        self.forn_por_sistema = {}
        mapa = {"Motor": [0, 1, 7, 11], "Freios": [0, 3], "Suspensao": [4, 13], "Direcao": [4, 13], "Eletrica": [5, 0],
                "Arrefecimento": [6, 1], "Transmissão": [12, 3], "Escapamento": [8], "Lataria": [9], "Acessorios": [10],
                "Interior": [10, 14]}
        for s, idxs in mapa.items():
            self.forn_por_sistema[s] = [self.fornecedores[i]["chave"] for i in idxs if i < len(self.fornecedores)]

    def _novo_componente(self, sistema, nome, categoria, unidade, cmin, cmax, passo, qmin, qmax, marca, custo=None):
        ch = self.chave("cmp")
        custo = custo if custo is not None else redondo(cmin, cmax, passo)
        margem = rng.choice(MARGENS_POR_SISTEMA[sistema])
        minimo = max(1, qmin // 2 + (1 if qmax > 4 else 0))
        abaixo = rng.random() < 0.15
        qtd_final = rng.randint(0, max(0, minimo - 1)) if abaixo else rng.randint(max(minimo, int(qmin * 0.7)), max(minimo, int(qmax * 0.7)))
        comp = {
            "chave": ch, "fornecedorChave": rng.choice(self.forn_por_sistema[sistema]),
            "skuInterno": novo_codigo("SKU", 5), "nome": nome,
            "descricao": f"{nome} — {marca}. Peça de reposição do sistema de {sistema.lower().replace('suspensao', 'suspensão').replace('direcao', 'direção').replace('eletrica', 'elétrica')}.",
            "marcaFabricante": marca, "partNumber": novo_codigo(sem_acento(marca[:3]).upper(), 6),
            "codigoBarras": ean13(), "ncm": rng.choice(["87083090", "87089990", "84213100", "85071000", "27101932", "40169990", "87088000"]),
            "categoria": categoria, "unidade": unidade, "sistema": sistema,
            "peso": round(rng.uniform(0.2, 12.0), 1), "garantiaDias": rng.choice([90, 90, 180, 365]),
            "custoUnitario": custo, "margemLucroPct": margem,
            "quantidadeMinima": minimo,
            "_qtd_final": qtd_final,
        }
        comp["_valor_venda"] = round(custo * (1 + margem / 100), 2)
        self.componentes.append(comp)
        self.comp_por_chave[ch] = comp
        return comp

    def gerar_componentes(self):
        for sistema, nome, cat, un, cmin, cmax, passo, qmin, qmax, marcas in COMPONENTES:
            c = self._novo_componente(sistema, nome, cat, un, cmin, cmax, passo, qmin, qmax, marcas[0])
            self.comp_por_nome[nome] = c
        # equivalentes: mesma peça, outra marca
        for nome, tipo, obs in EQUIVALENCIAS:
            orig = self.comp_por_nome[nome]
            linha = next(x for x in COMPONENTES if x[1] == nome)
            sistema, _, cat, un, cmin, cmax, passo, qmin, qmax, marcas = linha
            outra = next((m for m in marcas[1:] if m != orig["marcaFabricante"]), "Original")
            custo = arred(orig["custoUnitario"] * rng.uniform(0.82, 0.95), passo)
            alt = self._novo_componente(sistema, nome, cat, un, cmin, cmax, passo, max(1, qmin // 2), max(2, qmax // 2), outra, custo=custo)
            self.equivalentes.append({"componenteChave": orig["chave"], "equivalenteChave": alt["chave"],
                                      "observacao": obs, "tipoEquivalencia": tipo})
        # reposições de estoque: estoque inicial (out/2023) + 1–3 reposições no histórico.
        # (O sistema não baixa estoque ao usar a peça na OS — ver OrdemServicoService —
        # então a SOMA das reposições = quantidade final; o consumo do dia a dia está
        # na despesa-modelo "Reposição de peças de giro".)
        for comp in self.componentes:
            q = comp.pop("_qtd_final")
            reps = []
            if q > 0:
                n = 1 if q <= 2 else rng.randint(2, 4)
                partes = []
                resto = q
                for i in range(n - 1):
                    parte = max(1, resto // (n - i))
                    partes.append(parte)
                    resto -= parte
                partes.append(resto)
                datas = [sortear_data(*ABERTURA_ESTOQUE)] + sorted(
                    sortear_data(INICIO_OPERACAO + timedelta(days=30), LIMITE - timedelta(days=3)) for _ in range(n - 1))
                reps = [{"data": fmt(d), "quantidade": qq} for d, qq in zip(datas, partes) if qq > 0]
                if reps:
                    reps[0]["estoqueInicial"] = True   # carga da prateleira na adoção — sem despesa
                    comp["estoqueInicial"] = True
            comp["reposicoesEstoque"] = reps

    def gerar_presets(self):
        for nome, itens in PRESETS:
            ch = self.chave("chk")
            self.presets.append({"chave": ch, "nome": nome, "itens": itens})
            self.preset_por_nome[nome] = ch

    def gerar_templates(self):
        for nome, conteudo in TEMPLATES:
            self.templates.append({"chave": self.chave("tpl"), "nome": nome, "conteudo": conteudo})

    def gerar_despesas_modelo(self):
        for nome, setor, tipo, valor, _ in DESPESAS_MODELO:
            if nome.startswith("Encargos"):
                tipo = "Salario"   # encargos trabalhistas são custo de pessoal
            self.despesas.append({"nome": nome, "valor": valor, "setor": setor, "tipo": tipo,
                                  "categoria": CATEGORIA_DESPESA[nome]})

    # -----------------------------------------------------------------
    # Concessionária
    # -----------------------------------------------------------------
    def _sortear_modelo(self, data_compra):
        modelo = rng.choice(CATALOGO_VEICULOS)
        amin, amax = modelo[6], modelo[7]
        idade = rng.choices(range(0, 10), weights=[4, 12, 15, 15, 13, 11, 9, 8, 7, 6])[0]
        ano = min(max(amin, data_compra.year - idade), amax, data_compra.year)
        return modelo, ano

    def _novo_veiculo_venda(self, data_compra, cenario="disponivel", perto_do_custo=False, aquisicao_alvo=None):
        modelo, ano = self._sortear_modelo(data_compra)
        if aquisicao_alvo:
            # reposição: carro de faixa de preço parecida com o que acabou de sair
            for _ in range(200):
                m2, a2 = self._sortear_modelo(data_compra)
                if abs(preco_mercado(m2[3], a2) / 1.2 - aquisicao_alvo) <= 0.12 * aquisicao_alvo:
                    modelo, ano = m2, a2
                    break
        marca, mod, motor, preco_novo, comb, cambios, amin, amax = modelo
        mercado = preco_mercado(preco_novo, ano) * rng.uniform(0.95, 1.05)
        valor = max(30000, arred(mercado, 1000))
        if perto_do_custo:
            markup = rng.uniform(0.97, 1.04)
        else:
            markup = rng.uniform(1.15, 1.30)   # margem de 13% a 23% sobre o preço
        aquisicao = arred(valor / markup, 500)
        idade = max(0, data_compra.year - ano)
        km = max(0, arred(idade * rng.uniform(9000, 16000) + rng.uniform(0, 8000), 500))
        ch = self.chave("vv")
        placa = nova_placa(ano)
        cor = rng.choice(CORES)
        cambio = rng.choice(cambios)
        texto = (
            f"TERMO DE ENTREGA DE VEÍCULO\n\n"
            f"{NOME_LOJA}, inscrita no CNPJ nº {self.cnpj_loja}, entrega ao(à) COMPRADOR(A) identificado(a) na assinatura "
            f"eletrônica deste termo o veículo abaixo descrito, nas condições apresentadas e aceitas no momento da venda:\n\n"
            f"Marca/Modelo: {marca} {mod} {motor}\nAno: {ano}/{min(ano + 1, HOJE.year)}\nCor: {cor}\nPlaca: {placa}\n"
            f"Câmbio: {'Automático' if cambio == 'Automatico' else 'Manual'} — Combustível: {comb}\n"
            f"Quilometragem: {km:,} km\n\n".replace(",", ".") +
            "O(A) COMPRADOR(A) declara que vistoriou o veículo no ato da entrega e o recebeu em condições de uso, com "
            "estepe, macaco, chave de roda e triângulo; que recebeu manual do proprietário, 2 (duas) chaves, CRLV e "
            "documento de transferência; e que está ciente da garantia legal de 90 dias para motor e câmbio. A partir "
            "da assinatura, assume a responsabilidade civil, administrativa e criminal sobre o veículo.\n\n"
            f"Local: {CIDADE_LOJA}."
        )
        v = {
            "chave": ch, "marca": marca, "modelo": mod, "cor": cor, "motorizacao": motor, "ano": ano,
            "quilometragem": km, "placa": placa, "renavam": novo_renavam(), "cambio": cambio, "combustivel": comb,
            "valor": valor, "valorAquisicao": aquisicao,
            "acessorios": rng.sample(ACESSORIOS, k=rng.randint(3, 7)),
            "anoUltimoIpvaPago": max(ano, data_compra.year - (0 if data_compra.month >= 3 else 1)),
            "textoTermoPreliminar": texto,
            "dataCriacao": fmt(data_compra), "cenario": cenario,
            "_venda": None,  # datetime da conclusão da venda, se vendido
        }
        self.veiculos_venda.append(v)
        self.vv_por_chave[ch] = v
        return v

    def _valores_proposta(self, v, financiada=False):
        base = v["valor"]
        # desconto ≤ 5%, valor final redondo (múltiplo de 100; 1.000 se financiada) e ≥ aquisição
        opcoes = [0]
        for d in [1, 2, 3, 4, 5]:
            final = base * (100 - d) / 100
            if final != int(final):
                continue
            if final % (1000 if financiada else 100) == 0 and (final >= v["valorAquisicao"] or v["valor"] < v["valorAquisicao"] * 1.05):
                opcoes.append(d)
        d = rng.choice(opcoes) if rng.random() < 0.6 else 0
        return base, d, int(base * (100 - d) / 100)

    def _proposta(self, cenario, v, cli, vend, criacao, financiada=False, chave=None):
        base, desc, final = self._valores_proposta(v, financiada)
        pr = {"chave": chave or self.chave("prop"), "veiculoVendaChave": v["chave"], "clienteChave": cli["chave"],
              "vendedorChave": vend["chave"], "valorBase": base, "descontoPercentual": desc,
              "dataCriacao": fmt(criacao), "cenario": cenario, "_final": final}
        self.evento(cli["chave"], criacao)
        return pr

    def _texto_financiadora(self, final, entrada, data):
        fin = final - entrada
        n = rng.choice([24, 36, 48, 48, 60])
        taxa = rng.choice([1.29, 1.39, 1.49, 1.59, 1.69, 1.89])
        i = taxa / 100
        parcela = fin * i / (1 - (1 + i) ** -n)
        cet = ((1 + i) ** 12 - 1) * 100 + rng.uniform(1.5, 3.5)
        return (
            f"Financiadora: {rng.choice(FINANCIADORAS)}\nContato: Central de atendimento ao lojista\n"
            f"Data do retorno: {data:%d/%m/%Y}\nProposta/protocolo: {rng.randint(10 ** 7, 10 ** 8 - 1)}\n\n"
            f"Condições aprovadas:\n- Valor do veículo: {brl(final)}\n- Entrada: {brl(entrada)}\n- Valor financiado: {brl(fin)}\n"
            f"- Parcelas: {n}x de {brl(round(parcela, 2))}\n- Taxa de juros: {taxa:.2f}% a.m. — CET: {cet:.2f}% a.a.\n"
            f"- Primeira parcela: {(data + timedelta(days=30)):%d/%m/%Y}\n\n"
            f"Exigências: aprovação válida por 10 dias; contrato assinado digitalmente pelo cliente."
        )

    def _cadeia_proposta(self, pr, ate, cli, vend, v, financiada):
        """Preenche datas/pagamentos até o estágio `ate` (aprovada..concluida)."""
        C = p(pr["dataCriacao"])
        final = pr["_final"]
        etapas = [C]
        A = proximo(C, 1, 5) if not financiada else proximo(C, 2, 7)
        etapas.append(A)
        out = {}
        if financiada:
            pct_e = rng.choice([20, 20, 25, 30, 30, 40, 50])
            entrada = final * pct_e // 100
        if ate in ("aprovada",):
            return comprimir(etapas), out
        V = proximo(A, 1, 3)
        etapas.append(V)
        if ate == "vistoriada":
            return comprimir(etapas), out
        pagamentos = []
        if financiada:
            P1 = proximo(V, 0, 1)
            P2 = proximo(P1, 2, 8)
            pagamentos = [("Pix", pct_e, P1, entrada), ("Financiamento", 100 - pct_e, P2, final - entrada)]
            out["valorEntrada"] = entrada
        else:
            modo = pr["modoPagamento"]
            if ate == "pagamentoParcial" or rng.random() < PCT_A_VISTA_COM_SINAL:
                pct = rng.choice([10, 20, 20, 30])
                P1 = proximo(V, 0, 1)
                pagamentos = [("Pix", pct, P1, final * pct // 100)]
                if ate != "pagamentoParcial":
                    P2 = proximo(P1, 1, 6)
                    pagamentos.append((modo, 100 - pct, P2, final - final * pct // 100))
            else:
                pagamentos = [(modo, 100, proximo(V, 0, 3), final)]
        etapas += [x[2] for x in pagamentos]
        if ate == "pagamentoParcial":
            etapas = comprimir(etapas)
            return etapas, {"pagamentos": pagamentos}
        T = proximo(etapas[-1], 0, 1)
        etapas.append(T)
        if ate in ("termoRedigido", "termoEnviado"):
            return comprimir(etapas), {"pagamentos": pagamentos, **out}
        S = proximo(T, 0, 2)
        etapas.append(S)
        return comprimir(etapas), {"pagamentos": pagamentos, **out}

    def _aplicar_cadeia(self, pr, etapas, extra, ate, financiada):
        pr["dataCriacao"] = fmt(etapas[0])
        pr["dataAprovacao"] = fmt(etapas[1])
        if len(etapas) > 2:
            pr["dataVistoria"] = fmt(etapas[2])
        pags = extra.get("pagamentos", [])
        if pags:
            n = len(pags)
            datas_pag = etapas[3:3 + n]
            pr["pagamentos"] = [{"modo": m, "percentual": pc, "valor": val, "data": fmt(d)}
                                for (m, pc, _, val), d in zip(pags, datas_pag)]
            resto = etapas[3 + n:]
            if resto:
                pr["dataTermo"] = fmt(resto[0])
            if len(resto) > 1:
                pr["dataAssinatura"] = fmt(resto[1])
        if "valorEntrada" in extra:
            pr["valorEntrada"] = extra["valorEntrada"]
        if financiada:
            pr["textoPropostaFinanciadora"] = self._texto_financiadora(pr["_final"], extra.get("valorEntrada", 0), etapas[1] - timedelta(hours=3))

    def gerar_concessionaria(self):
        n_total = QTD_VENDAS_A_VISTA + QTD_VENDAS_FINANCIADAS
        tipos = ["avista"] * QTD_VENDAS_A_VISTA + ["financiada"] * QTD_VENDAS_FINANCIADAS
        rng.shuffle(tipos)
        datas = datas_estratificadas(n_total, INICIO_OPERACAO + timedelta(days=3), LIMITE - timedelta(days=8), SAZ_VENDAS, CRESC_VENDAS)
        # Pátio em regime: começa com QTD_PATIO carros comprados na montagem
        # (set/2023) e cada venda dispara a compra de um carro de reposição de
        # valor parecido poucos dias depois. Assim a compra de veículos de cada
        # ano ≈ custo dos carros vendidos no ano (estoque estável), condição pra
        # margem ANUAL da concessionária ficar na faixa pedida.
        patio = [self._novo_veiculo_venda(sortear_data(*ABERTURA_ESTOQUE), perto_do_custo=rng.random() < PCT_PERTO_DO_CUSTO)
                 for _ in range(QTD_PATIO)]
        for v in patio:
            v["estoqueInicial"] = True
        self.n_estoque_inicial = QTD_PATIO
        vendas = []
        for C, tipo in zip(datas, tipos):
            elegiveis = sorted((v for v in patio if v.get("estoqueInicial") or p(v["dataCriacao"]) <= C - timedelta(days=12)),
                               key=lambda v: v["dataCriacao"])
            if elegiveis:
                v = rng.choice(elegiveis[:4])   # quase FIFO: os mais antigos do pátio saem primeiro
                patio.remove(v)
            else:
                v = self._novo_veiculo_venda(horario_comercial(C - timedelta(days=rng.randint(12, 30))),
                                             perto_do_custo=rng.random() < PCT_PERTO_DO_CUSTO)
            vendas.append((C, tipo, v))
            reposicao = horario_comercial(C + timedelta(days=rng.randint(1, 12)))
            if reposicao <= LIMITE - timedelta(days=2):
                patio.append(self._novo_veiculo_venda(reposicao, perto_do_custo=rng.random() < PCT_PERTO_DO_CUSTO,
                                                      aquisicao_alvo=v["valorAquisicao"]))
        self.patio_final = sorted(patio, key=lambda v: v["dataCriacao"])

        # ---- vendas concluídas ----
        concluidas = []
        for C, tipo, v in vendas:
            vend = self.vendedor(C)
            cli = self.escolher_cliente(C, "comprador")
            cli["_compras"] += 1
            financiada = tipo == "financiada"
            cen = "concluidaFinanciada" if financiada else "concluidaAVista"
            pr = self._proposta(cen, v, cli, vend, C, financiada)
            if not financiada:
                pr["modoPagamento"] = rng.choices(["Pix", "Transferencia", "Boleto"], weights=[50, 35, 15])[0]
            etapas, extra = self._cadeia_proposta(pr, "concluida", cli, vend, v, financiada)
            self._aplicar_cadeia(pr, etapas, extra, "concluida", financiada)
            v["_venda"] = p(pr["dataAssinatura"])
            v["_comprador"] = cli["chave"]
            for d in etapas:
                self.evento(cli["chave"], d)
            self.propostas.append(pr)
            concluidas.append((pr, v, cli, vend))
            if rng.random() < PCT_TD_ANTES_DA_VENDA:
                self._td_antes(v, cli, vend, C)

        # ---- estoque no fim: propostas abertas, livres, em preparação ----
        abertos = []
        cenarios_abertos = []
        for cen in ["criada", "aprovada", "vistoriada", "pagamentoParcial", "termoRedigido", "termoEnviado"]:
            cenarios_abertos += [cen] * PROPOSTAS_MIN[cen]
        # carros que sobraram no pátio: os 3 mais novos estão em preparação,
        # os demais recebem as propostas abertas (as mais antigas nos carros mais
        # antigos) e o resto fica disponível sem proposta.
        patio = list(self.patio_final)
        prep = patio[-QTD_EM_PREPARACAO:]
        patio = patio[:-QTD_EM_PREPARACAO]
        for v in prep:
            v["cenario"] = "emPreparacao"
        while len(patio) < len(cenarios_abertos) + QTD_ESTOQUE_LIVRE:
            patio.insert(0, self._novo_veiculo_venda(recente(60, 150)))
        dias_por_cen = {"criada": (0, 4), "aprovada": (3, 14), "vistoriada": (5, 18), "pagamentoParcial": (6, 22),
                        "termoRedigido": (8, 26), "termoEnviado": (8, 28)}
        planejadas = sorted(((recente(*dias_por_cen[cen]), cen) for cen in cenarios_abertos), key=lambda x: x[0])
        livres = []
        for C, cen in planejadas:
            v = next((x for x in patio if p(x["dataCriacao"]) <= C - timedelta(days=8)), None)
            if v is None:
                v = patio[0]
                C = min(max(C, p(v["dataCriacao"]) + timedelta(days=8)), LIMITE - timedelta(hours=4))
            patio.remove(v)
            vend = self.vendedor(C)
            cli = self.escolher_cliente(C, "comprador")
            # abertas são à vista: o importador só percorre o funil de financiamento
            # no cenário concluidaFinanciada (ver AvancarFunilPropostaAsync)
            financiada = False
            pr = self._proposta(cen, v, cli, vend, C, False)
            pr["modoPagamento"] = rng.choice(["Pix", "Transferencia", "Boleto"])
            if cen == "pagamentoParcial":
                pr["modoPagamento"] = rng.choice(["Transferencia", "Boleto", "Pix"])
            if cen != "criada":
                etapas, extra = self._cadeia_proposta(pr, cen, cli, vend, v, pr["modoPagamento"] == "Financiamento")
                self._aplicar_cadeia(pr, etapas, extra, cen, pr["modoPagamento"] == "Financiamento")
                for d in etapas:
                    self.evento(cli["chave"], d)
            if pr["modoPagamento"] == "Financiamento":
                pr.pop("modoPagamento")
            self.propostas.append(pr)
            abertos.append((pr, v, cli, vend))
            if rng.random() < 0.5:
                self._td_antes(v, cli, vend, C)
        self.estoque_livre = patio   # disponíveis sem proposta

        # ---- propostas mal-sucedidas antes da venda (outro cliente) ----
        hospedeiros = [(v, p(v["dataCriacao"]), p(pr["dataCriacao"])) for pr, v, _, _ in concluidas + abertos
                       if pr["cenario"] != "criada"]
        rng.shuffle(hospedeiros)
        falhas = (["rejeitada"] * PROPOSTAS_MIN["rejeitada"] + ["financiamentoNegado"] * PROPOSTAS_MIN["financiamentoNegado"]
                  + ["cancelada"] * PROPOSTAS_MIN["cancelada"])
        usados = set()
        for cen in falhas:
            for v, compra, venda_c in hospedeiros:
                if v["chave"] in usados or (venda_c - compra).days < 14:
                    continue
                janela_ini = max(compra + timedelta(days=2), INICIO_OPERACAO)
                janela_fim = venda_c - timedelta(days=12 if cen == "cancelada" else 4)
                if janela_fim <= janela_ini:
                    continue
                C = sortear_data(janela_ini, janela_fim)
                comprador = v.get("_comprador")
                cli = self.escolher_cliente(C, "interessado", excluir={comprador} if comprador else ())
                vend = self.vendedor(C)
                financiada = cen == "financiamentoNegado"
                pr = self._proposta(cen, v, cli, vend, C)
                if cen == "rejeitada":
                    pr["motivoRejeicao"] = rng.choice(MOTIVOS_REJEICAO)
                elif cen == "financiamentoNegado":
                    pr["motivoRejeicao"] = rng.choice(MOTIVOS_NEGATIVA)
                else:
                    pr["motivoCancelamento"] = rng.choice(MOTIVOS_CANCELAMENTO_PROPOSTA)
                    pr["modoPagamento"] = rng.choice(["Pix", "Transferencia", "Boleto"])
                    A = proximo(C, 1, 4)
                    etapas = [C, A]
                    if rng.random() < 0.5:
                        etapas.append(proximo(A, 1, 3))
                    etapas = comprimir(etapas, venda_c - timedelta(days=2))
                    pr["dataAprovacao"] = fmt(etapas[1])
                    if len(etapas) > 2:
                        pr["dataVistoria"] = fmt(etapas[2])
                    for d in etapas:
                        self.evento(cli["chave"], d)
                usados.add(v["chave"])
                self.propostas.append(pr)
                if rng.random() < PCT_TD_ANTES_DA_PROPOSTA_FALHA:
                    self._td_antes(v, cli, vend, C)
                break

        # ordem cronológica (falha sempre antes da venda do mesmo veículo)
        self.propostas.sort(key=lambda x: x["dataCriacao"])
        self.concluidas = concluidas

    # ---- test drives ----
    def _novo_td(self, cenario, cli, vend, data, veiculo=None, consignado=None, termo=None, obs=None):
        td = {"chave": self.chave("td"), "clienteChave": cli["chave"], "vendedorChave": vend["chave"],
              "dataHora": fmt(data), "cenario": cenario}
        if consignado:
            td["veiculoConsignadoChave"] = consignado["chave"]
        else:
            td["veiculoVendaChave"] = veiculo["chave"]
        td["termo"] = termo or ("assinado" if cenario == "realizado" else "enviado")
        if obs:
            td["observacao"] = obs
        self.test_drives.append(td)
        if cenario in ("realizado", "cancelado", "naoCompareceu") or data <= LIMITE:
            self.evento(cli["chave"], data)
        return td

    def _td_antes(self, v, cli, vend, C):
        ini = p(v["dataCriacao"]) + timedelta(days=1)
        d = horario_comercial(C - timedelta(days=rng.randint(1, 10)), 9, 17)
        if d <= ini or d >= C:
            d = C - timedelta(hours=rng.randint(2, 5))
            if d <= ini:
                return
        if d < INICIO_OPERACAO:
            return
        self._novo_td("realizado", cli, vend, d, veiculo=v, obs=rng.choice(OBS_TD))

    def gerar_test_drives_avulsos(self):
        vendidos = [v for v in self.veiculos_venda if v["_venda"] and (v["_venda"] - p(v["dataCriacao"])).days > 6
                    and p(v["dataCriacao"]) >= INICIO_OPERACAO - timedelta(days=20)]
        for cen in ["realizado", "cancelado", "naoCompareceu"]:
            for _ in range(TD_AVULSOS[cen]):
                v = rng.choice(vendidos)
                ini = max(p(v["dataCriacao"]) + timedelta(days=1), INICIO_OPERACAO)
                fim = v["_venda"] - timedelta(days=3)
                if fim <= ini:
                    fim = ini + timedelta(hours=4)
                d = sortear_data(ini, fim)
                cli = self.escolher_cliente(d, "interessado", excluir={v.get("_comprador")})
                self._novo_td(cen, cli, self.vendedor(d), d, veiculo=v,
                              termo="assinado" if cen == "realizado" else rng.choice(["enviado", "rascunho"]),
                              obs=rng.choice(OBS_TD))
        # futuros: veículos ainda à venda (livres e com proposta criada/aprovada)
        candidatos = self.estoque_livre + [self.vv_por_chave[pr["veiculoVendaChave"]] for pr in self.propostas
                                           if pr["cenario"] in ("criada", "aprovada")]
        for _ in range(TD_AVULSOS["agendado"]):
            v = rng.choice(candidatos)
            d = (HOJE + timedelta(days=rng.randint(1, 14))).replace(hour=rng.choice([9, 10, 11, 14, 15, 16, 17]), minute=rng.choice([0, 30]))
            if d.weekday() == 6:
                d += timedelta(days=1)
            cli = self.escolher_cliente(LIMITE, "interessado")
            self._novo_td("agendado", cli, self.vendedor(LIMITE), d, veiculo=v,
                          termo=rng.choice(["enviado", "rascunho"]), obs=rng.choice(OBS_TD))
        for _ in range(TD_AVULSOS["reagendado"]):
            v = rng.choice(candidatos)
            original = (HOJE + timedelta(days=rng.randint(-3, 2))).replace(hour=rng.choice([9, 10, 14, 15, 16]), minute=0)
            if original > LIMITE and original < HOJE + timedelta(hours=8):
                original = LIMITE - timedelta(hours=2)
            novo = (max(original, HOJE) + timedelta(days=rng.randint(2, 9))).replace(hour=rng.choice([9, 10, 11, 14, 15, 16]), minute=0)
            if novo.weekday() == 6:
                novo += timedelta(days=1)
            cli = self.escolher_cliente(LIMITE, "interessado")
            td = self._novo_td("reagendado", cli, self.vendedor(LIMITE), novo, veiculo=v, termo="enviado", obs=rng.choice(OBS_TD))
            td["dataReagendamentoOriginal"] = fmt(original)

    # -----------------------------------------------------------------
    # Consignação
    # -----------------------------------------------------------------
    def gerar_consignacoes(self):
        for tipo in ["Fixo", "Porcentagem"]:
            for cen, qtd in CONSIGNACOES_POR_TIPO.items():
                for _ in range(qtd):
                    self._consignacao(cen, tipo)
        self.consignacoes.sort(key=lambda x: x["dataCriacao"])

    def _consignacao(self, cen, tipo):
        prazo = rng.choice([60, 90, 90, 120, 180])
        if cen == "ativa":
            # ativo HOJE com folga (o importador expira por DataVencimento)
            C = recente(5, max(6, prazo - 15))
        elif cen == "vendidaAguardando":
            venda = recente(1, 20)
            C = horario_comercial(venda - timedelta(days=rng.randint(15, prazo - 5)))
        else:
            C = sortear_data(INICIO_OPERACAO + timedelta(days=10), LIMITE - timedelta(days=prazo + 20))
        while True:
            modelo = rng.choice(CATALOGO_VEICULOS)
            marca, mod, motor, preco_novo, comb, cambios, amin, amax = modelo
            lo, hi = (amin if amax < 2014 else max(amin, 2014)), min(amax, C.year - 1)
            if lo <= hi:
                break
        ano = rng.randint(lo, hi)
        valor = arred(preco_mercado(preco_novo, ano) * rng.uniform(0.97, 1.08), 1000)
        cli = self.escolher_cliente(C, "consignante")
        vend = self.vendedor(C)
        km = arred((C.year - ano) * rng.uniform(9000, 15000) + 3000, 500)
        cor = rng.choice(CORES)
        placa = nova_placa(ano)
        renavam = novo_renavam()
        ch = self.chave("csg")
        cs = {"chave": ch, "marca": marca, "modelo": mod, "cor": cor, "motorizacao": motor, "ano": ano,
              "quilometragem": km, "placa": placa, "renavam": renavam, "cambio": rng.choice(cambios), "combustivel": comb,
              "acessorios": rng.sample(ACESSORIOS, k=rng.randint(2, 6)),
              "clienteProprietarioChave": cli["chave"], "vendedorResponsavelChave": vend["chave"],
              "tipoComissao": tipo, "valorVendaEsperado": valor, "prazoDias": prazo,
              "dataCriacao": fmt(C), "cenario": cen}
        if tipo == "Fixo":
            comissao = redondo(2000, 5000, 500)
            cs["valorFixoProprietario"] = valor - comissao
            liquido, pct_txt = valor - comissao, "—"
        else:
            pct = rng.choice([88, 89, 90, 91, 92])
            cs["porcentagemProprietario"] = pct
            liquido, pct_txt = int(valor * pct / 100), str(pct)
        cs["textoContrato"] = (
            TPL_CONSIGNACAO.replace("[NOME DA LOJA/CONCESSIONÁRIA]", NOME_LOJA).replace("[CNPJ DA LOJA]", self.cnpj_loja)
            .replace("[NOME DO PROPRIETÁRIO]", cli["nome"]).replace("[CPF DO PROPRIETÁRIO]", cli["cpf"])
            .replace("[MARCA E MODELO]", f"{marca} {mod} {motor}").replace("[ANO DE FABRICAÇÃO/MODELO]", str(ano))
            .replace("[COR]", cor).replace("[PLACA]", placa).replace("[RENAVAM]", renavam)
            .replace("[QUILOMETRAGEM]", f"{km:,}".replace(",", ".")).replace("[PRAZO EM DIAS]", str(prazo))
            .replace("[VALOR DE VENDA]", f"{valor:,.2f}".replace(",", "X").replace(".", ",").replace("X", "."))
            .replace("[VALOR LÍQUIDO DO PROPRIETÁRIO]", f"{liquido:,.2f}".replace(",", "X").replace(".", ",").replace("X", "."))
            .replace("[PERCENTUAL DO PROPRIETÁRIO]", pct_txt).replace("[PRAZO DE REPASSE]", "5")
            .replace("[PRAZO DE AVISO PRÉVIO]", "7").replace("[CIDADE]", CIDADE_LOJA).replace("[DATA]", f"{C:%d/%m/%Y}")
            .replace("[NOME DO REPRESENTANTE DA LOJA]", vend["nome"])
        )
        datas = [C]
        if cen == "vendidaAguardando":
            cs["dataVenda"] = fmt(venda)
            datas.append(venda)
        elif cen == "concluida":
            venda = horario_comercial(C + timedelta(days=rng.randint(10, prazo - 5)))
            concl = proximo(venda, 3, 12)
            cs["dataVenda"], cs["dataConclusao"] = fmt(venda), fmt(concl)
            datas += [venda, concl]
        elif cen == "devolvida":
            dev = horario_comercial(C + timedelta(days=prazo + rng.randint(0, 10)))
            cs["dataDevolucao"] = fmt(min(dev, LIMITE))
            datas.append(dev)
        elif cen == "cancelada":
            canc = horario_comercial(C + timedelta(days=rng.randint(5, prazo - 5)))
            cs["dataCancelamento"] = fmt(canc)
            cs["motivoCancelamento"] = rng.choice(MOTIVOS_CANCELAMENTO_CONSIGNACAO)
            datas.append(canc)
        for d in datas:
            self.evento(cli["chave"], d)
        cs["_ini"], cs["_fim"] = C, C + timedelta(days=prazo)
        self.consignacoes.append(cs)
        self.csg_por_chave[ch] = cs

    def gerar_test_drives_consignado(self):
        ativas = [c for c in self.consignacoes if c["cenario"] == "ativa"]
        for cen, qtd in TD_CONSIGNADO.items():
            for _ in range(qtd):
                cs = rng.choice(ativas)
                if cen in ("agendado", "reagendado"):
                    d = (HOJE + timedelta(days=rng.randint(1, 12))).replace(hour=rng.choice([9, 10, 14, 15, 16]), minute=0)
                    if d.weekday() == 6:
                        d += timedelta(days=1)
                    base = LIMITE
                else:
                    d = sortear_data(cs["_ini"] + timedelta(days=2), LIMITE - timedelta(hours=3))
                    base = d
                cli = self.escolher_cliente(base, "interessado", excluir={cs["clienteProprietarioChave"]})
                td = self._novo_td(cen, cli, self.vendedor(base), d, consignado=cs,
                                   termo=None if cen == "realizado" else ("enviado" if cen != "agendado" else rng.choice(["enviado", "rascunho"])))
                if cen == "reagendado":
                    td["dataReagendamentoOriginal"] = fmt(LIMITE - timedelta(hours=rng.randint(3, 40)))
        self.test_drives.sort(key=lambda x: x["dataHora"])

    # -----------------------------------------------------------------
    # Oficina
    # -----------------------------------------------------------------
    def _novo_veiculo_cliente(self, cli, ref=None):
        if ref:
            marca, mod, cor, ano, placa = ref["marca"], ref["modelo"], ref["cor"], ref["ano"], ref["placa"]
        else:
            marca, mod, motor, *_ , amin, amax = rng.choice(CATALOGO_VEICULOS)
            lo = amin if amax < 2008 else max(2008, amin)
            ano = rng.randint(lo, max(lo, min(amax, HOJE.year - 1)))
            cor = rng.choice(CORES)
            placa = nova_placa(ano)
        ch = self.chave("vcli")
        vc = {"chave": ch, "clienteChave": cli["chave"], "marca": marca, "modelo": mod, "cor": cor, "ano": ano, "placa": placa}
        self.veiculos_cliente.append(vc)
        self.vc_por_chave[ch] = vc
        vc["_desde"] = INICIO_OPERACAO if not ref else ref["_venda"] + timedelta(days=15)
        vc["_aberta"] = False
        vc["_revenda"] = bool(ref)
        cli["_veiculos"].append(ch)
        return vc

    def _cliente_para_os(self, data, recentes=False):
        """Novo cliente (sem OS) ou recorrente ponderado por fidelidade."""
        t = (data - INICIO_OPERACAO).days / max(1, (LIMITE - INICIO_OPERACAO).days)
        p_novo = 0.85 if t < 0.08 else 0.42
        novos = [c for c in self.clientes if c["_entrada"] <= data and c["_os_count"] == 0]
        if novos and rng.random() < p_novo:
            pesos = [3 if not self.eventos_cliente[c["chave"]] else 1 for c in novos]
            cli = rng.choices(novos, weights=pesos)[0]
        else:
            rec = [c for c in self.clientes if c["_os_count"] > 0 and c["_entrada"] <= data
                   and (c["_ultima_os"] is None or (data - c["_ultima_os"]).days >= 40)]
            if not rec:
                rec = novos or [c for c in self.clientes if c["_entrada"] <= data]
            cli = rng.choices(rec, weights=[c["_fidelidade"] for c in rec])[0]
        # veículo do cliente
        livres = [self.vc_por_chave[k] for k in cli["_veiculos"]
                  if self.vc_por_chave[k]["_desde"] <= data and not self.vc_por_chave[k]["_aberta"]]
        if not livres or (len(cli["_veiculos"]) == 1 and rng.random() < (0.30 if cli["_os_count"] == 0 else 0.04)):
            livres.append(self._novo_veiculo_cliente(cli))
        return cli, rng.choice(livres)

    def _descricao(self, tema, ano_veic):
        km = arred(max(1, HOJE.year - ano_veic) * rng.uniform(9000, 15000), 10000) or 10000
        d = rng.choice(tema["desc"]).format(km=f"{km:,}".replace(",", "."))
        ctx = rng.choice(CONTEXTOS_OS)
        return (d + ". " + ctx).strip() if ctx else d

    def _itens_os(self, tema, encomenda_forcada=False):
        nomes = tema["pecas"]
        k = rng.choices([1, 2, 3], weights=[45, 40, 15])[0]
        # peças listadas da mais comum pra menos comum no tema
        escolhidos, pool = [], list(nomes)
        while pool and len(escolhidos) < k:
            x = rng.choices(pool, weights=[1 / (nomes.index(n) + 1) ** 1.3 for n in pool])[0]
            escolhidos.append(x)
            pool.remove(x)
        itens = []
        for nome in escolhidos:
            base = self.comp_por_nome[nome]
            # às vezes usa a marca equivalente (se existir)
            eq = [e for e in self.equivalentes if e["componenteChave"] == base["chave"]]
            comp = self.comp_por_chave[eq[0]["equivalenteChave"]] if eq and rng.random() < 0.3 else base
            origem = rng.choices(["Estoque", "Cliente", "Encomenda"], weights=[72, 10, 18])[0]
            q = 1
            if comp["unidade"] == "L":
                q = rng.choice([3, 4, 4, 5])
            elif comp["unidade"] == "UN" and comp["categoria"] in ("Amortecedor", "Pivô", "Terminal", "Bieleta", "Filtro"):
                q = rng.choice([1, 2, 2])
            itens.append({"componenteChave": comp["chave"], "quantidade": q, "origem": origem})
        if encomenda_forcada and not any(i["origem"] == "Encomenda" for i in itens):
            itens[0]["origem"] = "Encomenda"
        return itens

    def _texto_vistoria(self, os_, cli, vc, recep, data, tema):
        pecas = ", ".join(self.comp_por_chave[i["componenteChave"]]["nome"] for i in os_["itens"]) or "a definir após diagnóstico"
        prev = data + timedelta(days=os_["prazoDiasAPartirDaCriacao"])
        km = arred(max(1, HOJE.year - vc["ano"]) * rng.uniform(9000, 15000), 100)
        return (
            "CONTRATO DE ORDEM DE SERVIÇO — VISTORIA DE ENTRADA\n\n"
            f"Cliente: {cli['nome']} — CPF {cli['cpf']}\n"
            f"Veículo: {vc['marca']} {vc['modelo']} {vc['ano']} — Placa {vc['placa']} — Quilometragem: {km:,} km\n".replace(",", ".") +
            f"Data e hora da vistoria: {data:%d/%m/%Y %H:%M}\nRecepcionista responsável: {recep['nome']}\n\n"
            "1. ESTADO DO VEÍCULO NA ENTRADA\n"
            f"Condição geral: {rng.choice(CONDICAO_GERAL)}.\nAvarias identificadas: {rng.choice(AVARIAS)}.\n"
            f"Nível de combustível: {rng.choice(COMBUSTIVEL_NIVEL)}.\nPertences deixados no veículo: {rng.choice(PERTENCES)}.\n\n"
            "2. SERVIÇO SOLICITADO\n"
            f"Descrição: {os_['descricao']}\nPeças previstas: {pecas}.\nObservações técnicas: {rng.choice(OBS_TECNICAS)}\n\n"
            "3. PRAZO ESTIMADO\n"
            f"Previsão de conclusão: {prev:%d/%m/%Y}.\n\n"
            "4. CIÊNCIA DO CLIENTE\n"
            "O(A) cliente declara estar ciente do estado do veículo registrado nesta vistoria e dos serviços previstos. "
            "Serviços adicionais só serão executados mediante nova autorização."
        )

    def _pagamentos_os(self, cen, aprov, final, sinal_possivel=True):
        modos = ["Pix", "CartaoCredito", "CartaoDebito", "Dinheiro", "Transferencia"]
        pesos = [36, 26, 20, 10, 8]
        r = rng.random()
        if cen == "emAndamento":
            if r < 0.4:
                pct = rng.choice([30, 40, 50])
                return [{"modo": rng.choice(["Pix", "Pix", "CartaoDebito", "Dinheiro"]), "percentual": pct, "data": fmt(aprov + timedelta(minutes=5))}]
            return []
        if r < 0.72:
            return [{"modo": rng.choices(modos, weights=pesos)[0], "percentual": 100, "data": fmt(final)}]
        if r < 0.90:
            m1, m2 = rng.sample(modos, 2)
            pct = rng.choice([30, 40, 50, 60])
            return [{"modo": m1, "percentual": pct, "data": fmt(final)},
                    {"modo": m2, "percentual": 100 - pct, "data": fmt(final + timedelta(minutes=5))}]
        pct = rng.choice([30, 40, 50])
        return [{"modo": rng.choice(["Pix", "Dinheiro", "CartaoDebito"]), "percentual": pct, "data": fmt(aprov + timedelta(minutes=5))},
                {"modo": rng.choices(modos, weights=pesos)[0], "percentual": 100 - pct, "data": fmt(final)}]

    def _nova_os(self, cen, C, tema=None, cli=None, vc=None, prazo=None):
        tema = tema or rng.choices(TEMAS_OS, weights=[t["peso"] for t in TEMAS_OS])[0]
        if cli is None:
            cli, vc = self._cliente_para_os(C)
        mec = self.mecanico(tema["esp"], C)
        dur = rng.randint(*tema["dur"])
        if prazo is None:
            prazo = dur + rng.choice([1, 2, 2, 3, 4, 5]) if rng.random() < 0.9 else max(1, dur)
        os_ = {"chave": self.chave("os"), "veiculoClienteChave": vc["chave"], "clienteChave": cli["chave"],
               "mecanicoChave": mec["chave"], "tipo": tema["tipo"], "descricao": self._descricao(tema, vc["ano"]),
               "prazoDiasAPartirDaCriacao": max(1, prazo), "custoServico": redondo(tema["mo"][0], tema["mo"][1], 50),
               "dataCriacao": fmt(C), "cenario": cen}
        if tema["preset"] and rng.random() < PCT_OS_COM_PRESET:
            os_["checklistPresetChave"] = self.preset_por_nome[tema["preset"]]
        os_["itens"] = self._itens_os(tema, encomenda_forcada=(cen == "aguardandoPeca"))
        if cen == "aguardandoPeca":
            # a peça ainda não chegou: só a encomenda pendente; resto do orçamento depende dela
            os_["itens"] = [i for i in os_["itens"] if i["origem"] == "Encomenda"][:1]

        # ---- linha do tempo ----
        etapas = {"C": C}
        cadeia = [C]
        if cen in ("pendente", "cancelada"):
            pass
        else:
            recep = self.recepcionista(C)
            os_["recepcionistaChave"] = recep["chave"]
            V = proximo(C, 0, 1)
            cadeia.append(V)
            if cen not in ("emVistoria", "aguardandoPeca"):
                cadeia.append(proximo(V, 0, 2) if cen != "aguardandoCliente" else V + timedelta(minutes=rng.choice([40, 60, 90])))
            if cen not in ("emVistoria", "aguardandoPeca", "aguardandoCliente", "orcamentoRecusado", "aprovada"):
                cadeia.append(proximo(cadeia[-1], 0, 2))  # início
                fim_servico = proximo(cadeia[-1], dur, dur + (1 if rng.random() < 0.2 else 0))
                if cen in ("finalizadaPendente", "finalizadaPaga", "entregue"):
                    cadeia.append(fim_servico)
                    if cen == "entregue":
                        cadeia.append(proximo(fim_servico, 0, 2))
        cadeia = comprimir(cadeia, LIMITE - timedelta(hours=3) if cen in OS_ABERTAS else LIMITE)
        nomes = ["C", "V", "A", "I", "F", "E"]
        if cen == "aguardandoCliente":
            nomes = ["C", "V", "A"]  # 'A' aqui = vistoria concluída/orçamento apresentado (não aprovação)
        etapas = dict(zip(nomes, cadeia))
        if "V" in etapas:
            os_["dataVistoria"] = fmt(etapas["V"])
            if cen != "emVistoria":
                os_["textoVistoria"] = self._texto_vistoria(os_, cli, vc, self.usr_por_chave[os_["recepcionistaChave"]], etapas["V"], tema)
        if "A" in etapas and cen not in ("aguardandoCliente", "orcamentoRecusado"):
            os_["dataAprovacaoCliente"] = fmt(etapas["A"])
        if "I" in etapas:
            os_["dataInicio"] = fmt(etapas["I"])
        if "F" in etapas and cen in ("finalizadaPaga", "entregue"):
            os_["dataFinalizacao"] = fmt(etapas["F"])
        if "E" in etapas:
            os_["dataEntrega"] = fmt(etapas["E"])

        # ---- alerta (problema adicional) ----
        iniciou = "I" in etapas
        if cen == "pausada":
            # pausada: alerta emitido, aguardando decisão
            I = etapas.get("I") or proximo(etapas["A"], 0, 1)
            if "I" not in etapas:
                I = min(I, LIMITE - timedelta(hours=3))
                os_["dataInicio"] = fmt(I)
            ad = min(I + timedelta(hours=rng.randint(2, 30)), LIMITE - timedelta(minutes=30))
            os_["alerta"] = {"descricao": rng.choice(ALERTAS_POR_TEMA[tema["tema"]]), "decisao": "pendente", "data": fmt(ad)}
        elif iniciou and rng.random() < (PCT_OS_COM_ALERTA if cen != "canceladaEmAndamento" else 0.5):
            I = etapas["I"]
            limite_alerta = etapas.get("F", LIMITE)
            ad = I + timedelta(hours=rng.randint(1, 20))
            dd = ad + timedelta(hours=rng.randint(1, 26))
            if dd >= limite_alerta:
                span = (limite_alerta - I).total_seconds()
                ad = I + timedelta(seconds=span * 0.3)
                dd = I + timedelta(seconds=span * 0.6)
            recusado = rng.random() < (0.2 if cen != "canceladaEmAndamento" else 0.7)
            os_["alerta"] = {"descricao": rng.choice(ALERTAS_POR_TEMA[tema["tema"]]),
                             "decisao": "recusado" if recusado else "aprovado",
                             "observacaoCliente": rng.choice(OBS_ALERTA_RECUSADO if recusado else OBS_ALERTA_APROVADO),
                             "data": fmt(ad), "dataDecisao": fmt(dd)}
        # ---- requisição rejeitada (peça trocada por compatível) ----
        if iniciou and cen != "canceladaEmAndamento" and rng.random() < PCT_OS_REQUISICAO_REJEITADA * 1.4:
            I = etapas["I"]
            fim = etapas.get("F", LIMITE)
            rd = I + (fim - I) * 0.25
            peca = self.comp_por_chave[os_["itens"][0]["componenteChave"]]["nome"]
            os_["requisicaoRejeitada"] = {"descricaoPeca": f"{peca} — marca original da montadora",
                                          "motivo": rng.choice(MOTIVOS_REQ_REJEITADA), "data": fmt(rd.replace(second=0, microsecond=0))}
        # ---- pagamentos ----
        if cen in ("finalizadaPaga", "entregue"):
            pg = self._pagamentos_os(cen, etapas["A"], etapas["F"] - timedelta(minutes=20))
            os_["pagamentos"] = pg
        elif cen == "emAndamento":
            pg = self._pagamentos_os(cen, etapas["A"], None)
            if pg:
                os_["pagamentos"] = pg
        # eventos do cliente / estado do veículo
        for k, d in etapas.items():
            self.evento(cli["chave"], d)
        cli["_os_count"] += 1
        cli["_ultima_os"] = C
        if cen in OS_ABERTAS or cen == "finalizadaPaga":
            vc["_aberta"] = True
        os_["_tema"] = tema["tema"]
        self.ordens.append(os_)
        return os_

    def gerar_oficina(self):
        # 1) compradores que voltam pra revisão do carro comprado aqui
        revisoes = []
        candidatos = [(pr, v, cli) for pr, v, cli, _ in self.concluidas if v["_venda"] < LIMITE - timedelta(days=100)]
        rng.shuffle(candidatos)
        tema_rev = next(t for t in TEMAS_OS if t["tema"] == "revisao")
        n_rev = int(len(self.concluidas) * PCT_REVENDA_VOLTA_OFICINA)
        for pr, v, cli in candidatos[:n_rev]:
            vc = self._novo_veiculo_cliente(cli, ref=v)
            d = v["_venda"] + timedelta(days=rng.randint(90, 300))
            while d < LIMITE - timedelta(days=12):
                revisoes.append((horario_comercial(d), cli, vc))
                d += timedelta(days=rng.randint(150, 360))
        revisoes.sort(key=lambda x: x[0])

        # 2) terminais espalhados (entregue + canceladas + orçamento recusado)
        terminais = []
        n_entregue = OS_QTD["entregue"] - len(revisoes)
        for cen, n in [("entregue", n_entregue), ("cancelada", OS_QTD["cancelada"]),
                       ("canceladaEmAndamento", OS_QTD["canceladaEmAndamento"]), ("orcamentoRecusado", OS_QTD["orcamentoRecusado"])]:
            for d in datas_estratificadas(n, INICIO_OPERACAO, LIMITE - timedelta(days=9), SAZ_OFICINA, CRESC_OFICINA):
                terminais.append((d, cen, None, None))
        for d, cli, vc in revisoes:
            terminais.append((d, "entregue", cli, vc))
        terminais.sort(key=lambda x: x[0])
        for d, cen, cli, vc in terminais:
            if cli is not None:
                self._nova_os(cen, d, tema=tema_rev, cli=cli, vc=vc)
                cli_obj = cli
            else:
                self._nova_os(cen, d)
            # veículo volta a ficar livre depois de uma OS terminal
            self.vc_por_chave[self.ordens[-1]["veiculoClienteChave"]]["_aberta"] = False

        # 3) recentes (abertos): ~75% dentro do prazo (regra herdada da v1)
        recentes = []
        for cen in ["pendente", "emVistoria", "aguardandoCliente", "aprovada", "aguardandoPeca", "pausada",
                    "emAndamento", "finalizadaPendente", "finalizadaPaga"]:
            for _ in range(OS_QTD[cen]):
                prazo = rng.choice([2, 3, 5, 7, 10, 15])
                if cen == "finalizadaPaga":
                    k = rng.randint(1, 12)
                elif rng.random() < 0.75:
                    k = rng.randint(0, max(0, prazo - 2))
                else:
                    k = prazo + rng.randint(1, 8)
                k = min(k, 28)
                C = horario_comercial(LIMITE - timedelta(days=k), 8, 15)
                C = min(C, LIMITE - timedelta(hours=3))
                recentes.append((C, cen, prazo))
        recentes.sort(key=lambda x: x[0])
        for C, cen, prazo in recentes:
            self._nova_os(cen, C, prazo=prazo if cen != "finalizadaPaga" else None)
        self.ordens.sort(key=lambda x: x["dataCriacao"])

    # -----------------------------------------------------------------
    # Despesas esporádicas e fechamentos
    # -----------------------------------------------------------------
    def gerar_extras(self):
        comps = competencias()
        comps_hist = [c for c in comps if c != (HOJE.year, HOJE.month)]
        grandes_meses = rng.sample(comps_hist[2:], QTD_EVENTOS_GRANDES)
        familias = [EXTRAS_MANUTENCAO, EXTRAS_PERDAS, EXTRAS_INVESTIMENTOS]
        for (a, m) in comps:
            if (a, m) == (HOJE.year, HOJE.month):
                n = rng.choice([0, 1])
            else:
                n = rng.choices(list(DESPESAS_EXTRAS_POR_MES_PESOS), weights=list(DESPESAS_EXTRAS_POR_MES_PESOS.values()))[0]
            for _ in range(n):
                fam = rng.choices(familias, weights=[45, 25, 30])[0]
                nome, setor, cat, vmin, vmax, passo = rng.choice(fam)
                self._extra(a, m, nome, setor, cat, redondo(vmin, vmax, passo))
            if (a, m) in grandes_meses:
                nome, setor, cat, vmin, vmax, passo = EXTRAS_GRANDES[grandes_meses.index((a, m))]
                self._extra(a, m, nome, setor, cat, redondo(vmin, vmax, passo))
        self.extras.sort(key=lambda x: x["data"])

    def _extra(self, a, m, nome, setor, cat, valor):
        ultimo = 28
        d = datetime(a, m, rng.randint(1, ultimo), rng.randint(9, 17), 0)
        if d > LIMITE:
            d = LIMITE - timedelta(hours=2)
        self.extras.append({"data": fmt(d), "nome": nome, "setor": setor,
                            "categoria": CATEGORIA_DESPESA.get(nome, cat), "valor": valor})

    def valor_modelo_no_mes(self, regra, atual, a, m, ctx):
        """Valor histórico de um item do modelo na competência (a, m)."""
        def reajustes(mes_reajuste, taxa):
            # quantos reajustes aconteceram depois de (a, m) até hoje
            n = 0
            ano, mes = a, m
            while (ano, mes) < (HOJE.year, HOJE.month):
                mes += 1
                if mes == 13:
                    ano, mes = ano + 1, 1
                if mes == mes_reajuste:
                    n += 1
            return (1 + taxa) ** -n

        def headcount(tipo, filtro=None):
            ref = datetime(a, m, 15)
            total = [u for u in self.usuarios if u["tipo"] == tipo]
            ativos = [u for u in total if p(u["dataContratacao"]) <= ref]
            return len(ativos) / len(total)

        ruido = lambda pct: rng.uniform(1 - pct, 1 + pct)
        if regra == "fixo":
            return atual
        if regra == "aluguel":
            return arred(atual * reajustes(3, 0.045), 100)
        if regra == "reajuste_jan":
            return arred(atual * reajustes(1, 0.05), 50)
        if regra == "dissidio":
            return arred(atual * reajustes(5, 0.05), 100)
        if regra == "energia":
            saz = {12: 1.25, 1: 1.3, 2: 1.25, 3: 1.15, 4: 1.0, 5: 0.92, 6: 0.85, 7: 0.85, 8: 0.9, 9: 1.0, 10: 1.05, 11: 1.12}[m]
            return arred(atual * reajustes(7, 0.06) * saz * ruido(0.06), 50)
        if regra == "agua":
            saz = 1.12 if m in (12, 1, 2, 3) else 0.95
            return arred(atual * saz * ruido(0.10), 50)
        if regra == "ruido15":
            return arred(atual * ruido(0.15), 50)
        if regra == "salario_mecanicos":
            return arred(atual * reajustes(5, 0.05) * headcount("Mecanico"), 100)
        if regra == "salario_vendas":
            return arred(atual * reajustes(5, 0.05) * headcount("Vendedor"), 100)
        if regra == "salario_recepcao":
            return arred(atual * reajustes(5, 0.05) * (0.8 + 0.2 * headcount("Recepcionista")), 100)
        if regra == "comissoes":
            return arred(600 * ctx["vendas"].get((a, m), 0), 100)
        if regra == "despachante":
            return arred(250 * ctx["vendas"].get((a, m), 0) + 200, 50)
        if regra == "preparacao":
            return arred(200 * ctx["compras_veic"].get((a, m), 0) + 100, 50)
        if regra == "marketing":
            boost = 1.4 if m in (11, 3) else 1.0
            return arred(atual * boost * ruido(0.20), 100)
        if regra == "giro_pecas":
            return arred(70 * ctx["os_pagas"].get((a, m), 0) + 200, 100)
        if regra == "encargos_oficina":
            return arred(0.08 * ctx["mao_obra"].get((a, m), 0) + 300, 100)
        if regra == "comissao_mecanicos":
            return arred(0.30 * ctx["mao_obra"].get((a, m), 0), 100)
        return atual

    def contexto_mensal(self):
        vendas = Counter(competencia(p(pr["dataAprovacao"])) for pr in self.propostas if pr["cenario"].startswith("concluida"))
        compras = Counter(competencia(p(v["dataCriacao"])) for v in self.veiculos_venda)
        os_pagas = Counter(competencia(p(o["dataCriacao"])) for o in self.ordens if o["cenario"] in ("finalizadaPaga", "entregue"))
        mao_obra = Counter()
        for o in self.ordens:
            if o["cenario"] in ("finalizadaPaga", "entregue"):
                mao_obra[competencia(p(o["dataCriacao"]))] += o["custoServico"]
        return {"vendas": vendas, "compras_veic": compras, "os_pagas": os_pagas, "mao_obra": mao_obra}

    def gerar_fechamentos(self):
        ctx = self.contexto_mensal()
        self.valores_mensais = {}
        for (a, m) in competencias():
            atual = (a, m) == (HOJE.year, HOJE.month)
            variacoes = []
            valores = {}
            for nome, setor, tipo, valor, regra in DESPESAS_MODELO:
                if atual:
                    v = valor
                else:
                    v = self.valor_modelo_no_mes(regra, valor, a, m, ctx)
                    v = max(v, 50)
                    if v != valor:
                        variacoes.append({"nome": nome, "valor": v})
                valores[nome] = (setor, v)
            # critério §8.1: cada mês com total diferente do anterior
            total = sum(v for _, v in valores.values())
            if getattr(self, "_total_anterior", None) == total:
                nome_aj = "Material de limpeza e copa"
                setor_aj, v_aj = valores[nome_aj]
                valores[nome_aj] = (setor_aj, v_aj + 50)
                variacoes = [x for x in variacoes if x["nome"] != nome_aj] + [{"nome": nome_aj, "valor": v_aj + 50}]
                total += 50
            self._total_anterior = total
            self.valores_mensais[(a, m)] = valores
            f = {"ano": a, "mes": m, "fechar": not atual, "variacoes": variacoes}
            if not atual:
                # 1º–5º dia útil do mês seguinte, 17–19h, nunca depois do LIMITE
                na, nm = (a + 1, 1) if m == 12 else (a, m + 1)
                d = datetime(na, nm, 1)
                uteis = []
                while len(uteis) < 5:
                    if d.weekday() < 5:
                        uteis.append(d)
                    d += timedelta(days=1)
                opcoes = [u.replace(hour=rng.choice([17, 18, 18, 19]), minute=rng.choice([0, 15, 30, 45])) for u in uteis]
                opcoes = [o for o in opcoes if o <= LIMITE] or [LIMITE - timedelta(hours=1)]
                f["dataFechamento"] = fmt(rng.choice(opcoes))
            self.fechamentos.append(f)

    # -----------------------------------------------------------------
    # Finalização
    # -----------------------------------------------------------------
    def finalizar_clientes(self):
        for c in self.clientes:
            evs = self.eventos_cliente[c["chave"]]
            if evs:
                primeiro = min(evs)
                d = primeiro - timedelta(days=rng.choice([0, 0, 0, 1, 2]), hours=rng.randint(0, 2))
                d = min(d, primeiro - timedelta(minutes=10))
            else:
                d = c["_entrada"]
            d = max(d, datetime(2023, 10, 2, 8))
            if d.weekday() == 6:
                d -= timedelta(days=1)
            c["dataCriacao"] = fmt(d)

    def documento(self):
        def limpo(lst):
            return [{k: v for k, v in x.items() if not k.startswith("_")} for x in lst]
        props = []
        for pr in self.propostas:
            x = {k: v for k, v in pr.items() if not k.startswith("_")}
            props.append(x)
        return {
            "usuarios": limpo(self.usuarios),
            "clientes": limpo(self.clientes),
            "fornecedores": limpo(self.fornecedores),
            "componentes": limpo(self.componentes),
            "componentesEquivalentes": self.equivalentes,
            "checklistPresets": self.presets,
            "templatesDocumento": self.templates,
            "despesas": self.despesas,
            "veiculosVenda": limpo(self.veiculos_venda),
            "veiculosConsignados": limpo(self.consignacoes),
            "veiculosCliente": limpo(self.veiculos_cliente),
            "testDrives": self.test_drives,
            "propostasVenda": props,
            "ordensServico": limpo(self.ordens),
            # calibração/aferição anual obrigatória dos equipamentos da oficina
            # (determinística — não consome o rng, não altera o resto da base)
            "despesasExtras": self.extras + [
                {"data": f"{ano}-06-15T10:00:00", "nome": "Calibração e aferição anual de equipamentos (INMETRO)",
                 "setor": "Oficina", "categoria": "Manutenção de equipamentos", "valor": valor}
                for ano, valor in ((2024, 2500), (2025, 3000), (2026, 3000))] + [
                # expansão da oficina com o crescimento do movimento em 2026
                {"data": "2026-08-10T10:00:00", "nome": "Compra do segundo elevador automotivo (expansão)",
                 "setor": "Oficina", "categoria": "Investimento em equipamentos", "valor": 12000}],
            "fechamentosMensais": self.fechamentos,
        }


# =====================================================================
# AUTOVERIFICAÇÃO
# =====================================================================

def validar(doc, g):
    erros = []

    def err(msg):
        erros.append(msg)

    # chaves únicas e índices
    idx = {}
    for sec in ["usuarios", "clientes", "fornecedores", "componentes", "checklistPresets", "templatesDocumento",
                "veiculosVenda", "veiculosConsignados", "veiculosCliente", "testDrives", "propostasVenda", "ordensServico"]:
        for x in doc[sec]:
            if x["chave"] in idx:
                err(f"chave duplicada {x['chave']}")
            idx[x["chave"]] = (sec, x)

    def ref(chave, sec, onde, tipo_usr=None):
        if chave not in idx or idx[chave][0] != sec:
            err(f"{onde}: referência inválida {chave} (esperado {sec})")
            return None
        obj = idx[chave][1]
        if tipo_usr and obj["tipo"] != tipo_usr:
            err(f"{onde}: {chave} é {obj['tipo']}, esperado {tipo_usr}")
        return obj

    # documentos únicos
    for campo, lst in [("cpf", doc["clientes"]), ("cnpj", doc["fornecedores"]),
                       ("renavam", doc["veiculosVenda"] + doc["veiculosConsignados"]),
                       ("email", doc["usuarios"]), ("email", doc["clientes"])]:
        vals = [x[campo] for x in lst]
        if len(vals) != len(set(vals)):
            err(f"{campo} duplicado")
    placas = [x["placa"] for x in doc["veiculosVenda"] + doc["veiculosConsignados"]]
    placas += [x["placa"] for x in g.veiculos_cliente if not x["_revenda"]]
    if len(placas) != len(set(placas)):
        err("placa duplicada")
    for x in doc["clientes"]:
        if cpf_valido(x["cpf"][:9]) != x["cpf"]:
            err(f"CPF inválido {x['cpf']}")

    cli_criacao = {c["chave"]: p(c["dataCriacao"]) for c in doc["clientes"]}
    contratacao = {u["chave"]: (p(u["dataContratacao"]) if u.get("dataContratacao") else datetime(2000, 1, 1)) for u in doc["usuarios"]}

    def crescente(onde, datas, permitir_futuro=False):
        ds = [d for d in datas if d is not None]
        for a, b in zip(ds, ds[1:]):
            if b < a:
                err(f"{onde}: datas fora de ordem {a} > {b}")
        if not permitir_futuro:
            for d in ds:
                if d > LIMITE:
                    err(f"{onde}: data no futuro {d}")

    def depois_de(onde, d, base, nome):
        if d < base:
            err(f"{onde}: {d} antes de {nome} ({base})")

    for c in doc["componentes"]:
        ref(c["fornecedorChave"], "fornecedores", c["chave"])
    for e in doc["componentesEquivalentes"]:
        ref(e["componenteChave"], "componentes", "equivalente")
        ref(e["equivalenteChave"], "componentes", "equivalente")
    for v in doc["veiculosCliente"]:
        ref(v["clienteChave"], "clientes", v["chave"])
    for cs in doc["veiculosConsignados"]:
        ref(cs["clienteProprietarioChave"], "clientes", cs["chave"])
        ref(cs["vendedorResponsavelChave"], "usuarios", cs["chave"], "Vendedor")
        d0 = p(cs["dataCriacao"])
        depois_de(cs["chave"], d0, cli_criacao[cs["clienteProprietarioChave"]], "cadastro do cliente")
        depois_de(cs["chave"], d0, contratacao[cs["vendedorResponsavelChave"]], "contratação")
        seq = [d0] + [p(cs[k]) for k in ("dataVenda", "dataConclusao", "dataDevolucao", "dataCancelamento") if k in cs]
        crescente(cs["chave"], seq)
        if cs["cenario"] == "ativa" and d0 + timedelta(days=cs["prazoDias"]) <= HOJE + timedelta(days=3):
            err(f"{cs['chave']}: consignação ativa vencendo antes da importação")

    vendido_em = {}
    for pr in doc["propostasVenda"]:
        v = ref(pr["veiculoVendaChave"], "veiculosVenda", pr["chave"])
        ref(pr["clienteChave"], "clientes", pr["chave"])
        ref(pr["vendedorChave"], "usuarios", pr["chave"], "Vendedor")
        C = p(pr["dataCriacao"])
        seq = [C] + [p(pr[k]) for k in ("dataAprovacao", "dataVistoria") if k in pr]
        seq += [p(x["data"]) for x in pr.get("pagamentos", [])]
        seq += [p(pr[k]) for k in ("dataTermo", "dataAssinatura") if k in pr]
        crescente(pr["chave"], seq)
        depois_de(pr["chave"], C, cli_criacao[pr["clienteChave"]], "cadastro do cliente")
        depois_de(pr["chave"], C, contratacao[pr["vendedorChave"]], "contratação")
        depois_de(pr["chave"], C, p(v["dataCriacao"]), "compra do veículo")
        if v["chave"] in vendido_em:
            err(f"{pr['chave']}: proposta depois da venda de {v['chave']}")
        if pr["cenario"].startswith("concluida"):
            vendido_em[v["chave"]] = seq[-1]
            if sum(x["percentual"] for x in pr["pagamentos"]) != 100 and abs(sum(x["percentual"] for x in pr["pagamentos"]) - 100) > 1e-6:
                err(f"{pr['chave']}: percentuais não somam 100")
            final = pr["valorBase"] * (100 - pr["descontoPercentual"]) / 100
            if sum(x["valor"] for x in pr["pagamentos"]) != final:
                err(f"{pr['chave']}: pagamentos ≠ valor final")
        if pr["cenario"] == "criada" and (HOJE - C).days >= 7:
            err(f"{pr['chave']}: criada vai expirar")
        if pr["descontoPercentual"] > 5:
            err(f"{pr['chave']}: desconto > 5%")
    for td in doc["testDrives"]:
        ref(td["clienteChave"], "clientes", td["chave"])
        ref(td["vendedorChave"], "usuarios", td["chave"], "Vendedor")
        d = p(td["dataHora"])
        futuro = td["cenario"] in ("agendado", "reagendado")
        if futuro and d <= HOJE:
            err(f"{td['chave']}: agendado no passado")
        if not futuro and d > LIMITE:
            err(f"{td['chave']}: realizado no futuro")
        if not futuro:
            depois_de(td["chave"], d, cli_criacao[td["clienteChave"]], "cadastro do cliente")
        depois_de(td["chave"], d, contratacao[td["vendedorChave"]], "contratação")
        if "veiculoVendaChave" in td:
            v = ref(td["veiculoVendaChave"], "veiculosVenda", td["chave"])
            depois_de(td["chave"], d, p(v["dataCriacao"]), "compra do veículo")
            if v["chave"] in vendido_em and d >= vendido_em[v["chave"]]:
                err(f"{td['chave']}: test drive depois da venda")
        else:
            cs = ref(td["veiculoConsignadoChave"], "veiculosConsignados", td["chave"])
            if cs["cenario"] != "ativa":
                err(f"{td['chave']}: consignação não ativa")
            depois_de(td["chave"], d, p(cs["dataCriacao"]), "início da consignação")
    abertas_por_veic = Counter()
    for o in doc["ordensServico"]:
        vc = ref(o["veiculoClienteChave"], "veiculosCliente", o["chave"])
        ref(o["clienteChave"], "clientes", o["chave"])
        if vc and vc["clienteChave"] != o["clienteChave"]:
            err(f"{o['chave']}: veículo não pertence ao cliente")
        ref(o["mecanicoChave"], "usuarios", o["chave"], "Mecanico")
        if "recepcionistaChave" in o:
            ref(o["recepcionistaChave"], "usuarios", o["chave"], "Recepcionista")
            depois_de(o["chave"], p(o["dataCriacao"]), contratacao[o["recepcionistaChave"]], "contratação recep.")
        if o.get("checklistPresetChave"):
            ref(o["checklistPresetChave"], "checklistPresets", o["chave"])
        for it in o["itens"]:
            ref(it["componenteChave"], "componentes", o["chave"])
        C = p(o["dataCriacao"])
        depois_de(o["chave"], C, cli_criacao[o["clienteChave"]], "cadastro do cliente")
        depois_de(o["chave"], C, contratacao[o["mecanicoChave"]], "contratação mec.")
        seq = [C] + [p(o[k]) for k in ("dataVistoria", "dataAprovacaoCliente", "dataInicio") if k in o]
        pag = [p(x["data"]) for x in o.get("pagamentos", [])]
        fin = [p(o[k]) for k in ("dataFinalizacao", "dataEntrega") if k in o]
        crescente(o["chave"], seq + fin)
        crescente(o["chave"] + "/pag", seq[:3] + pag + fin[1:])
        if fin and pag and max(pag) > fin[0]:
            err(f"{o['chave']}: pagamento depois da finalização")
        if o.get("pagamentos") and o["cenario"] in ("finalizadaPaga", "entregue") and sum(x["percentual"] for x in o["pagamentos"]) != 100:
            err(f"{o['chave']}: pagamentos não somam 100%")
        if "alerta" in o:
            a = o["alerta"]
            ds = [p(o["dataInicio"]), p(a["data"])] + ([p(a["dataDecisao"])] if "dataDecisao" in a else []) + fin[:1]
            crescente(o["chave"] + "/alerta", ds)
        if o["cenario"] in OS_ABERTAS:
            if (LIMITE - C).days > 31:
                err(f"{o['chave']}: aberta antiga")
            abertas_por_veic[o["veiculoClienteChave"]] += 1
    for k, n in abertas_por_veic.items():
        if n > 1:
            err(f"veículo {k} com {n} OS abertas")
    for c in doc["componentes"]:
        for r in c["reposicoesEstoque"]:
            if p(r["data"]) > LIMITE:
                err(f"{c['chave']}: reposição no futuro")
    for f in doc["fechamentosMensais"]:
        if f["fechar"] and p(f["dataFechamento"]) > LIMITE:
            err("fechamento no futuro")
    nomes_modelo = {d["nome"] for d in doc["despesas"]}
    for f in doc["fechamentosMensais"]:
        for v in f["variacoes"]:
            if v["nome"] not in nomes_modelo:
                err(f"variação com nome fora do modelo: {v['nome']}")
    return erros


def estimar_linhas(doc, g):
    """Estimativa de linhas no banco depois da importação (todas as tabelas)."""
    L = Counter()
    L["Usuario (+tabela do papel)"] = 2 * len(doc["usuarios"])
    L["Cliente + Endereco"] = 2 * len(doc["clientes"])
    L["Fornecedor + Endereco"] = 2 * len(doc["fornecedores"])
    L["Componente + EstoqueComponente"] = 2 * len(doc["componentes"])
    L["ComponenteEquivalente"] = len(doc["componentesEquivalentes"])
    L["ChecklistPreset + itens"] = len(doc["checklistPresets"]) + sum(len(x["itens"]) for x in doc["checklistPresets"])
    L["TemplateDocumento"] = len(doc["templatesDocumento"])
    L["Despesa (modelo)"] = len(doc["despesas"])
    L["VeiculoVenda"] = len(doc["veiculosVenda"])
    hist = {"ativa": 1, "vendidaAguardando": 2, "concluida": 3, "devolvida": 2, "cancelada": 2}
    L["VeiculoConsignacao + Historico"] = sum(1 + hist[c["cenario"]] for c in doc["veiculosConsignados"])
    L["VeiculoCliente"] = len(doc["veiculosCliente"])
    L["TestDrive + TermoTestDrive"] = 2 * len(doc["testDrives"])
    prop = doc["propostasVenda"]
    L["PropostaVenda"] = len(prop)
    L["Vistoria (proposta)"] = sum(1 for x in prop if "dataVistoria" in x)
    L["TermoEntrega"] = sum(1 for x in prop if "dataTermo" in x)
    L["PagamentoProposta"] = sum(len(x.get("pagamentos", [])) for x in prop)
    os_ = doc["ordensServico"]
    preset_itens = {x["chave"]: len(x["itens"]) for x in doc["checklistPresets"]}
    L["OrdemServico"] = len(os_)
    L["ChecklistOrdemServico"] = sum(preset_itens.get(x.get("checklistPresetChave"), 0) for x in os_)
    L["ItemOrdemServico"] = sum(len(x["itens"]) for x in os_)
    L["VistoriaOrdemServico"] = sum(1 for x in os_ if "recepcionistaChave" in x)
    L["PagamentoOrdemServico"] = sum(len(x.get("pagamentos", [])) for x in os_)
    n_enc = sum(1 for x in os_ for i in x["itens"] if i["origem"] == "Encomenda")
    L["RequisicaoPecaOS"] = n_enc + sum(1 for x in os_ if "requisicaoRejeitada" in x)
    L["AlertaOS"] = sum(1 for x in os_ if "alerta" in x)
    n_comp = len(competencias())
    n_td_real = sum(1 for t in doc["testDrives"] if t["cenario"] == "realizado")
    n_rep = sum(len(c["reposicoesEstoque"]) for c in doc["componentes"])
    L["BalancoMensalDespesa"] = n_comp
    n_ini = sum(1 for v in doc["veiculosVenda"] if v.get("estoqueInicial")) + sum(
        1 for c in doc["componentes"] for r in c["reposicoesEstoque"] if r.get("estoqueInicial"))
    L["ItemBalancoDespesa"] = (n_comp * len(doc["despesas"]) + len(doc["veiculosVenda"]) + n_rep - n_ini + n_enc
                               + n_td_real + len(doc["despesasExtras"]))
    L["Seed do sistema (admin, config, presets/templates padrão)"] = 30
    return L


MARGEM_MIN, MARGEM_MAX = 0.0, 15.0   # faixa exigida pelo dono — por ano e por setor


def margens_mensais(g):
    """Mesma regra do DashboardService: receita de venda = ValorFinal de proposta
    concluída na competência da DataAprovacao; receita de oficina = ValorTotal de
    OS Finalizada/Entregue na competência da DataCriacao; despesa = itens do
    balanço (modelo com variações + compra de veículo + compra de peça
    (reposição e encomenda) + combustível de test drive + extras), com a parte
    Geral dividida meio a meio entre os setores. Devolve {(ano, mes): Counter}."""
    m = defaultdict(Counter)
    for pr in g.propostas:
        if pr["cenario"].startswith("concluida"):
            m[competencia(p(pr["dataAprovacao"]))]["rec_conc"] += pr["_final"]
    for o in g.ordens:
        total = o["custoServico"]
        for it in o["itens"]:
            if it["origem"] != "Cliente":
                total += g.comp_por_chave[it["componenteChave"]]["_valor_venda"] * it["quantidade"]
        o["_total"] = total
        if o["cenario"] in ("finalizadaPaga", "entregue"):
            m[competencia(p(o["dataCriacao"]))]["rec_ofi"] += total
        for it in o["itens"]:
            if it["origem"] == "Encomenda":
                c = g.comp_por_chave[it["componenteChave"]]
                d = p(o.get("dataInicio") or o.get("dataVistoria") or o["dataCriacao"])
                m[competencia(d)]["desp_ofi"] += c["custoUnitario"] * it["quantidade"]
    for v in g.veiculos_venda:
        if not v.get("estoqueInicial"):
            m[competencia(p(v["dataCriacao"]))]["desp_conc"] += v["valorAquisicao"]
    for c in g.componentes:
        for r in c["reposicoesEstoque"]:
            if r.get("estoqueInicial"):
                continue
            m[competencia(p(r["data"]))]["desp_ofi"] += c["custoUnitario"] * r["quantidade"]
    for t in g.test_drives:
        if t["cenario"] == "realizado":
            m[competencia(p(t["dataHora"]))]["desp_conc"] += 50

    def lanca(comp, setor, valor):
        if setor == "Geral":
            m[comp]["desp_ofi"] += valor / 2
            m[comp]["desp_conc"] += valor / 2
        else:
            m[comp]["desp_ofi" if setor == "Oficina" else "desp_conc"] += valor
    for comp, valores in g.valores_mensais.items():
        for nome, (setor, v) in valores.items():
            lanca(comp, setor, v)
    for e in g.extras:
        lanca(competencia(p(e["data"])), e["setor"], e["valor"])
    return m


def periodos():
    """(rótulo, lista de competências, conta_na_faixa)."""
    oper = competencias()
    out = []
    for ano in sorted({a for a, _ in oper}):
        cs = [c for c in oper if c[0] == ano]
        parcial = len(cs) < 12
        out.append((f"{ano}{' (parcial)' if parcial else ''}", cs, True))
    # janelas de 12 meses a partir da inauguração (a última absorve o mês corrente)
    blocos = [oper[i:i + 12] for i in range(0, len(oper), 12)]
    if len(blocos) > 1 and len(blocos[-1]) < 6:
        ultimo = blocos.pop()
        blocos[-1] = blocos[-1] + ultimo
    for cs in blocos:
        out.append((f"12m {cs[0][1]:02d}/{cs[0][0]}–{cs[-1][1]:02d}/{cs[-1][0]}", cs, True))
    return out


def checar_margens(doc, g):
    m = margens_mensais(g)
    linhas, fora = [], []
    for rotulo, cs, avaliar in periodos():
        ag = Counter()
        for c in cs:
            ag.update(m[c])
        ro, do, rc, dc = ag["rec_ofi"], ag["desp_ofi"], ag["rec_conc"], ag["desp_conc"]
        mg = lambda r, d: 100 * (r - d) / r if r else float("nan")
        mo, mc, me = mg(ro, do), mg(rc, dc), mg(ro + rc, do + dc)
        linhas.append((rotulo, ro, do, mo, rc, dc, mc, me))
        if avaliar:
            for nome, val in (("oficina", mo), ("concessionária", mc), ("empresa", me)):
                if not (MARGEM_MIN <= val <= MARGEM_MAX):
                    fora.append(f"{rotulo} {nome}: {val:.1f}%")
    return linhas, fora


def imprimir_relatorio(doc, g):
    print(f"\nGerado: {SAIDA}  (HOJE={HOJE:%d/%m/%Y}, limite de eventos passados={LIMITE:%d/%m/%Y %H:%M})")
    print("\n== Registros por seção ==")
    for k, v in doc.items():
        print(f"  {k:26s} {len(v):6d}")
    print(f"  {'TOTAL':26s} {sum(len(v) for v in doc.values()):6d}")

    print("\n== Cenários ==")
    def tabela(titulo, cont, minimos=None):
        print(f"  {titulo}:")
        for k, n in sorted(cont.items(), key=lambda x: -x[1]):
            mn = (minimos or {}).get(k)
            ok = "" if mn is None else ("  OK" if n >= mn else f"  ABAIXO DO MÍNIMO ({mn})")
            print(f"    {k:24s} {n:5d}{ok}")
    tabela("Propostas", Counter(x["cenario"] for x in doc["propostasVenda"]), PROPOSTAS_MIN)
    tabela("Ordens de serviço", Counter(x["cenario"] for x in doc["ordensServico"]), OS_QTD)
    tabela("Test drives", Counter(x["cenario"] + (" (consignado)" if "veiculoConsignadoChave" in x else "") for x in doc["testDrives"]))
    tabela("Consignações", Counter(f"{x['cenario']}/{x['tipoComissao']}" for x in doc["veiculosConsignados"]))
    tabela("Veículos à venda", Counter("vendido" if v["_venda"] else v["cenario"] for v in g.veiculos_venda))
    os_ = doc["ordensServico"]
    print("  Variações transversais OS:")
    iniciadas = [o for o in os_ if "dataInicio" in o]
    print(f"    alertas {sum(1 for o in os_ if 'alerta' in o)} de {len(iniciadas)} iniciadas — "
          + ", ".join(f"{k}={v}" for k, v in Counter(o['alerta']['decisao'] for o in os_ if 'alerta' in o).items()))
    print(f"    requisição rejeitada {sum(1 for o in os_ if 'requisicaoRejeitada' in o)}")
    print("    origem dos itens: " + ", ".join(f"{k}={v}" for k, v in Counter(i['origem'] for o in os_ for i in o['itens']).items()))
    pagas = [o for o in os_ if o["cenario"] in ("finalizadaPaga", "entregue")]
    print(f"    pagamento dividido em 2: {sum(1 for o in pagas if len(o['pagamentos']) == 2)} de {len(pagas)} pagas; "
          f"emAndamento com sinal: {sum(1 for o in os_ if o['cenario'] == 'emAndamento' and o.get('pagamentos'))}")
    print("    modos: " + ", ".join(f"{k}={v}" for k, v in Counter(x['modo'] for o in os_ for x in o.get('pagamentos', [])).items()))
    atrasadas = sum(1 for o in os_ if o["cenario"] in OS_ABERTAS and p(o["dataCriacao"]) + timedelta(days=o["prazoDiasAPartirDaCriacao"]) < HOJE)
    print(f"    OS abertas atrasadas na data da importação: {atrasadas} (todas dos últimos 30 dias)")
    print("    OS por tema: " + ", ".join(f"{k}={v}" for k, v in Counter(o['_tema'] for o in g.ordens).most_common()))

    # recorrência
    por_cli = Counter(o["clienteChave"] for o in os_)
    rec2 = sum(1 for n in por_cli.values() if n >= 2)
    veics = Counter(v["clienteChave"] for v in doc["veiculosCliente"])
    compradores = {pr["clienteChave"] for pr in doc["propostasVenda"] if pr["cenario"].startswith("concluida")}
    voltaram = {o["clienteChave"] for o in os_ if g.vc_por_chave[o["veiculoClienteChave"]]["_revenda"]}
    sem_hist = sum(1 for c in g.clientes if not g.eventos_cliente[c["chave"]])
    print("\n== Clientes ==")
    print(f"  clientes de oficina: {len(por_cli)}; com 2+ OS: {rec2} ({100 * rec2 / max(1, len(por_cli)):.0f}%); "
          f"com 2+ veículos: {sum(1 for n in veics.values() if n >= 2)}")
    print(f"  compradores: {len(compradores)}; compradores que voltaram à oficina com o carro comprado: {len(voltaram)}")
    print(f"  clientes sem nenhum evento (só cadastro): {sem_hist}")
    vips = sorted(g.clientes, key=lambda c: -len(g.eventos_cliente[c['chave']]))[:5]
    print("  históricos mais ricos (Consulta CPF): " + "; ".join(
        f"{c['nome']} CPF {c['cpf']} — {por_cli.get(c['chave'], 0)} OS, "
        f"{sum(1 for pr in doc['propostasVenda'] if pr['clienteChave'] == c['chave'])} propostas, "
        f"{sum(1 for t in doc['testDrives'] if t['clienteChave'] == c['chave'])} TD, "
        f"{sum(1 for x in doc['veiculosConsignados'] if x['clienteProprietarioChave'] == c['chave'])} consig." for c in vips))

    print("\n== Economia (§5) ==")
    vendas = [(pr, g.vv_por_chave[pr["veiculoVendaChave"]]) for pr in g.propostas if pr["cenario"].startswith("concluida")]
    prej = sum(1 for pr, v in vendas if pr["_final"] <= v["valorAquisicao"])
    margens = [(pr["_final"] - v["valorAquisicao"]) / pr["_final"] for pr, v in vendas]
    print(f"  vendas concluídas: {len(vendas)}; no prejuízo/zero: {prej} ({100 * prej / len(vendas):.1f}%); "
          f"margem bruta média sobre o preço: {100 * sum(margens) / len(margens):.1f}%")
    print(f"  veículos do estoque inicial (estoqueInicial, 02/10/2023, sem despesa): {g.n_estoque_inicial}; "
          f"em estoque no fim: {sum(1 for v in g.veiculos_venda if not v['_venda'])}")
    estoque = sum(c["custoUnitario"] * sum(r["quantidade"] for r in c["reposicoesEstoque"]) for c in g.componentes)
    abaixo = sum(1 for c in g.componentes if sum(r["quantidade"] for r in c["reposicoesEstoque"]) < c["quantidadeMinima"])
    print(f"  estoque de peças (custo): {brl(estoque)}; componentes abaixo do mínimo: {abaixo}/{len(g.componentes)}")
    tickets = [o["_total"] for o in g.ordens if o["cenario"] in ("finalizadaPaga", "entregue")] if all("_total" in o for o in g.ordens) else []
    print(f"  extras: {len(g.extras)} eventos, total {brl(sum(e['valor'] for e in g.extras))}; "
          f"meses sem extra: {sum(1 for c in competencias() if not any(p(e['data']).year == c[0] and p(e['data']).month == c[1] for e in g.extras))}")

    linhas, fora = checar_margens(doc, g)
    tickets = [o["_total"] for o in g.ordens if o["cenario"] in ("finalizadaPaga", "entregue")]
    print(f"  ticket médio OS paga: {brl(sum(tickets) / len(tickets))}")
    print(f"\n  Margem líquida prevista por período × setor (regra do DashboardService; faixa exigida {MARGEM_MIN:.0f}–{MARGEM_MAX:.0f}%):")
    print(f"    {'período':34s} {'rec. oficina':>12s} {'desp. oficina':>13s} {'oficina':>8s} {'rec. conc.':>12s} {'desp. conc.':>12s} {'conc.':>7s} {'empresa':>8s}")
    for rot, ro, do, mo, rc, dc, mc, me in linhas:
        print(f"    {rot:34s} {ro:12,.0f} {do:13,.0f} {mo:7.1f}% {rc:12,.0f} {dc:12,.0f} {mc:6.1f}% {me:7.1f}%")
    print("  → TODOS os anos/janelas × setores dentro da faixa" if not fora else "  → FORA DA FAIXA: " + "; ".join(fora))
    g._margens_fora = fora
    totais = {nome: sum(valores[nome][1] for valores in g.valores_mensais.values()) for nome in g.valores_mensais[competencias()[0]]}
    tot_mes = [sum(v for _, v in g.valores_mensais[c].values()) for c in competencias()]
    iguais = sum(1 for a, b in zip(tot_mes, tot_mes[1:]) if a == b)
    print(f"  fechamentos: {sum(1 for f in g.fechamentos if f['fechar'])} fechados + "
          f"{sum(1 for f in g.fechamentos if not f['fechar'])} aberto; total do modelo por mês de "
          f"{brl(min(tot_mes))} a {brl(max(tot_mes))}; meses com total igual ao anterior: {iguais}")

    print("\n== Estimativa de linhas no banco ==")
    L = estimar_linhas(doc, g)
    for k, v in sorted(L.items(), key=lambda x: -x[1]):
        print(f"  {k:52s} {v:6d}")
    print(f"  {'TOTAL ESTIMADO':52s} {sum(L.values()):6d}   (alvo: 10.000–11.000)")


def main():
    g = Gerador()
    g.gerar_usuarios()
    g.gerar_clientes()
    g.gerar_fornecedores()
    g.gerar_componentes()
    g.gerar_presets()
    g.gerar_templates()
    g.gerar_despesas_modelo()
    g.gerar_concessionaria()
    g.gerar_consignacoes()
    g.gerar_test_drives_avulsos()
    g.gerar_test_drives_consignado()
    g.gerar_oficina()
    g.gerar_extras()
    g.gerar_fechamentos()
    g.finalizar_clientes()
    # modelo de despesas: valor atual do mês corrente
    doc = g.documento()

    erros = validar(doc, g)
    SAIDA.write_text(json.dumps(doc, ensure_ascii=False, indent=1), encoding="utf-8")
    json.loads(SAIDA.read_text(encoding="utf-8"))  # JSON válido
    imprimir_relatorio(doc, g)
    print("\n== Integridade ==")
    if erros:
        print(f"  {len(erros)} problema(s):")
        for e in erros[:40]:
            print("   -", e)
        sys.exit(1)
    print("  OK — todas as chaves resolvem, datas em ordem, documentos únicos.")


if __name__ == "__main__":
    main()
