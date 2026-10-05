#!/usr/bin/env bash
# Gera e instala a base de demonstração terminando na data informada (padrão: hoje).
# NÃO altera o DELORE: usa o gerador Python + a importação e a restauração do próprio
# sistema (Geradores --importar / --restaurar). A restauração guarda antes uma cópia do
# banco atual em Web/backups/pre-restauracao-*.db.
#
#   bash Geradores/dataset/preparar_base_apresentacao.sh            # termina hoje
#   bash Geradores/dataset/preparar_base_apresentacao.sh 2026-11-18 # termina nesse dia (não pode ser futuro)
#
# Tenta sementes até achar uma em que TODOS os anos/setores fiquem com margem líquida
# entre 0% e 15% no banco real e a importação dê 0 avisos.
set -euo pipefail
RAIZ="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$RAIZ"
DATA="${1:-$(date +%F)}"
if [[ "$DATA" > "$(date +%F)" ]]; then
  echo "A data $DATA é futura: o importador do sistema grava datas futuras como 'hoje'. Rode no próprio dia (ou depois)." >&2
  exit 1
fi
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT
for SEED in 20261132 $(seq 20261016 20261215); do
  SAIDA=$(DATASET_HOJE="$DATA" DATASET_SEED="$SEED" python3 Geradores/dataset/gerar_dataset.py 2>&1) || true
  echo "$SAIDA" | grep -q "TODOS os anos" || continue   # margem prevista fora da faixa (ou falha) — próxima
  echo "== Semente $SEED (base até $DATA): margem prevista OK, importando =="
  rm -f "$TMP"/base.db*
  dotnet run --project Geradores -- --importar Geradores/dataset/dataset_demo.json --banco "$TMP/base.db" > "$TMP/imp.log" 2>&1
  grep -q "Avisos: 0 ==" "$TMP/imp.log" || { echo "   importação com avisos — próxima"; grep -A5 "Avisos:" "$TMP/imp.log" | head; continue; }
  if python3 Geradores/dataset/validar_banco.py "$TMP/base.db" > "$TMP/val.log"; then
    grep -E "^  TOTAL|EMPRESA|fechados=" "$TMP/val.log"
    dotnet run --project Geradores -- --restaurar "$TMP/base.db" 2>&1 | grep -E "restaurado|Erro|erro" || true
    cp "$TMP/base.db" Geradores/dataset/carstore_demo.db
    echo "Pronto: base instalada em Web/carstore.db (cópia em Geradores/dataset/carstore_demo.db). Recarregue o site."
    exit 0
  fi
  echo "   reprovada no banco real (margem ou cobertura):"; grep -E "FORA|FALTA" "$TMP/val.log" | cut -c1-140 || true
done
echo "Nenhuma semente atendeu a faixa de margem — nada foi instalado." >&2
exit 1
