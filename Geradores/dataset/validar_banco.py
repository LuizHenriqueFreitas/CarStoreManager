"""Validação §7 do PLANO_BASE_DEMO.md: contagens, status, datas, fechamentos e margem anual
(0–15%). Sai com código 1 se algum ano/setor ficar fora da faixa.
Uso: python3 validar_banco.py [caminho.db]  (padrão: Web/carstore.db)"""
import sqlite3, sys, re
from datetime import datetime, timedelta

DB = sys.argv[1] if len(sys.argv) > 1 else "/mnt/F872C5CC72C59034/Softwares/CarStoreManager/Web/carstore.db"
HOJE = datetime.now().strftime("%Y-%m-%d")
con = sqlite3.connect(DB)
cur = con.cursor()

tabelas = [r[0] for r in cur.execute("select name from sqlite_master where type='table' and name not like '\\_%' escape '\\' and name not like 'sqlite%'")]
total = 0
print("== Linhas por tabela ==")
for t in sorted(tabelas):
    n = cur.execute(f'select count(*) from "{t}"').fetchone()[0]
    total += n
    print(f"  {t:35s} {n:6d}")
print(f"  {'TOTAL':35s} {total:6d}\n")

ENUM_OS = {1:"Pendente",2:"EmVistoria",8:"BuscandoPecas",7:"AguardandoCliente",3:"Aprovada",4:"EmAndamento",9:"Pausada",11:"PagamentoPendente",5:"Finalizada",10:"Entregue",6:"Cancelada"}
def dist(tabela, coluna="Status", nomes=None):
    try:
        rows = cur.execute(f'select "{coluna}", count(*) from "{tabela}" group by 1 order by 2 desc').fetchall()
    except sqlite3.Error as e:
        print(f"  ({tabela}.{coluna}: {e})"); return
    print(f"== {tabela}.{coluna} ==")
    for v, n in rows:
        print(f"  {nomes.get(v, v) if nomes else v}: {n}")

dist("OrdensServico", nomes=ENUM_OS)
for t in ["PropostasVenda", "VeiculosVenda", "VeiculosConsignacao", "TestDrives", "AlertasOS", "RequisicoesPeca", "TermosEntrega", "TermosTestDrive", "VistoriasOrdemServico"]:
    if t in tabelas:
        dist(t)

if "BalancosMensaisDespesa" in tabelas:
    print("\n== Fechamentos mensais ==")
    rows = cur.execute('select b.Competencia, b.Fechado, b.DataFechamento, count(i.Id) from BalancosMensaisDespesa b left join ItemBalancoDespesa i on i.BalancoId=b.Id group by b.Id order by b.Competencia').fetchall() if "ItemBalancoDespesa" in tabelas else \
           cur.execute('select Competencia, Fechado, DataFechamento, 0 from BalancosMensaisDespesa order by Competencia').fetchall()
    for comp, fech, dataf, n in rows:
        print(f"  {comp}  {'FECHADO' if fech else 'aberto '}  {dataf or '-':26s} itens={n}")
    print(f"  total={len(rows)} fechados={sum(1 for r in rows if r[1])}")

print("\n== Datas no dia de hoje (suspeita de backdate faltando) ==")
for t in tabelas:
    cols = [r[1] for r in cur.execute(f'pragma table_info("{t}")') if re.match(r"(Data|Prazo)", r[1])]
    for c in cols:
        n = cur.execute(f'select count(*) from "{t}" where substr("{c}",1,10)=?', (HOJE,)).fetchone()[0]
        if n:
            print(f"  {t}.{c}: {n}")
con.close()

# ---- Margem líquida anual por setor (meta do dono: 0% a 15% em cada ano) ----
# Receita oficina: OS Finalizada(5)/Entregue(10) por DataCriacao (regra do DashboardService).
# Receita concessionária: proposta Concluida(12) por DataAprovacao.
# Despesa: itens do balanço por competência; setor Geral rateado meio a meio (doc 27).
def margem_anual():
    con = sqlite3.connect(DB); c = con.cursor()
    def cols(t): return [r[1] for r in c.execute(f'pragma table_info("{t}")')]
    def achar(t, *pref):
        for p in pref:
            for col in cols(t):
                if col.lower().startswith(p.lower()): return col
        raise KeyError(f"{t}: {pref} em {cols(t)}")
    rec = {}
    vt = achar("OrdensServico", "ValorTotal")
    for ano, v in c.execute(f'select substr(DataCriacao,1,4), sum({vt}) from OrdensServico where Status in (5,10) group by 1'):
        rec.setdefault(ano, {})["Oficina"] = v or 0
    vf = achar("PropostasVenda", "ValorFinal")
    for ano, v in c.execute(f'select substr(DataAprovacao,1,4), sum({vf}) from PropostasVenda where Status=12 group by 1'):
        rec.setdefault(ano, {})["Concessionaria"] = v or 0
    itens = [t for t in tabelas if "ItemBalanco" in t or "ItensBalanco" in t][0]
    vi = achar(itens, "Valor"); st = achar(itens, "Setor")
    desp = {}
    for ano, setor, v in c.execute(f'select substr(b.Competencia,1,4), i.{st}, sum(i.{vi}) from "{itens}" i join BalancosMensaisDespesa b on b.Id=i.BalancoId group by 1,2'):
        d = desp.setdefault(ano, {"Oficina": 0, "Concessionaria": 0})
        if str(setor) in ("Geral", "0"):
            d["Oficina"] += v / 2; d["Concessionaria"] += v / 2
        elif str(setor) in ("Oficina", "1"): d["Oficina"] += v
        else: d["Concessionaria"] += v
    print("\n== Margem líquida anual (meta 0–15%) ==")
    for ano in sorted(set(rec) | set(desp)):
        linha = []
        rt = dt = 0
        for s in ("Oficina", "Concessionaria"):
            r = rec.get(ano, {}).get(s, 0); d = desp.get(ano, {}).get(s, 0); rt += r; dt += d
            m = (r - d) / r * 100 if r else float("nan")
            linha.append(f"{s}: rec {r:12,.0f} desp {d:12,.0f} margem {m:6.1f}% {'OK' if 0 <= m <= 15 else 'FORA'}")
        mt = (rt - dt) / rt * 100 if rt else float("nan")
        if any("FORA" in l for l in linha) or not (0 <= mt <= 15):
            FORA.append(ano)
        print(f"  {ano}  " + " | ".join(linha) + f" | EMPRESA {mt:6.1f}% {'OK' if 0 <= mt <= 15 else 'FORA'}")

FORA = []
try:
    margem_anual()
except Exception as e:
    print("margem anual: erro ->", e); FORA.append("erro")
# ---- Cobertura: toda opção relevante presente na base ----
con = sqlite3.connect(DB); c = con.cursor()
COBERTURA = {
    "combustível entre os VENDIDOS (Flex, Álcool, Gasolina, Diesel, Elétrico, Híbrido)": (
        "select distinct v.Combustivel from PropostasVenda p join VeiculosVenda v on v.Id=p.VeiculoVendaId where p.Status=12", {1, 2, 3, 4, 5, 6}),
    "combustível nas consignações": ("select distinct Combustivel from VeiculosConsignacao", {1, 2, 3, 4, 5, 6}),
    "câmbio entre os vendidos": (
        "select distinct v.Cambio from PropostasVenda p join VeiculosVenda v on v.Id=p.VeiculoVendaId where p.Status=12", {1, 2}),
    "status de OS alcançáveis": ("select distinct Status from OrdensServico", {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}),
    "status de test drive": ("select distinct Status from TestDrives", {1, 2, 3, 4}),
}
print("\n== Cobertura de opções ==")
for nome, (sql, esperado) in COBERTURA.items():
    falta = esperado - {r[0] for r in c.execute(sql)}
    print(f"  {nome}: {'OK' if not falta else 'FALTA ' + str(sorted(falta))}")
    if falta:
        FORA.append(nome)
semcat = c.execute("select count(*) from ItemBalancoDespesa where ifnull(Categoria,'')=''").fetchone()[0]
print(f"  despesas sem categoria: {semcat}")
if semcat:
    FORA.append("categoria")
sys.exit(1 if FORA else 0)
