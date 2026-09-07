// Wrapper do Chart.js para JSInterop a partir do Blazor.
// Cada gráfico é referenciado pelo id do canvas; chamadas subsequentes
// destroem a instância anterior para evitar duplicação ao re-renderizar.
//
// Diretrizes de data-viz (docs/redesign/11-batch2-melhorias.md §E):
//  - comparar magnitude entre muitas categorias  -> barra HORIZONTAL, 1 tom, ordenada
//  - tendência no tempo                           -> linha
//  - parte-do-todo (<= 6 segmentos, só relance)   -> rosca
//  - distinguir 2-5 séries                        -> paleta categórica, ordem fixa
//  - nunca eixo duplo; 9a categoria -> "Outros"

window.carstoreCharts = window.carstoreCharts || {};

// Paleta categórica validada (ordem fixa — não cicla). Ver skill dataviz.
window.CARSTORE_PALETA = ['#2a78d6', '#eb6834', '#1baf7a', '#eda100', '#e87ba4', '#008300', '#4a3aa7', '#e34948'];
// Sequencial (magnitude) — só azul, claro -> escuro.
window.CARSTORE_AZUL = '#2a78d6';
window.CARSTORE_AZUL_CLARO = 'rgba(42,120,214,0.14)';
const TINTA = '#1a1a1a', TINTA3 = '#888', LINHA = '#ececec';

Chart.defaults.font.family = 'system-ui, -apple-system, "Segoe UI", Roboto, sans-serif';
Chart.defaults.font.size = 12;
Chart.defaults.color = TINTA3;

function opcoesBase(extra) {
    return Object.assign({
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: { position: 'bottom', labels: { boxWidth: 10, boxHeight: 10, usePointStyle: true, padding: 14 } },
            tooltip: { padding: 10, boxPadding: 4, cornerRadius: 6 }
        }
    }, extra || {});
}

function eixoRecessivo() {
    return {
        grid: { color: LINHA, drawTicks: false },
        border: { display: false },
        ticks: { padding: 8 }
    };
}

window.carstoreChart = {
    // API antiga — mantida para telas ainda não migradas.
    render: function (canvasId, type, labels, datasets, options) {
        return montar(canvasId, { type: type, data: { labels: labels, datasets: datasets }, options: options || opcoesBase() });
    },

    // Barra HORIZONTAL de magnitude: 1 série, ordena desc, agrupa cauda em "Outros".
    // dados = [{ rotulo, valor }]  ·  unidade opcional ("R$", "un", "dias")
    barraMagnitude: function (canvasId, dados, unidade, maxItens) {
        maxItens = maxItens || 12;
        var ord = (dados || []).slice().sort(function (a, b) { return b.valor - a.valor; });
        if (ord.length > maxItens) {
            var cabeca = ord.slice(0, maxItens - 1);
            var resto = ord.slice(maxItens - 1).reduce(function (s, d) { return s + d.valor; }, 0);
            cabeca.push({ rotulo: 'Outros', valor: resto });
            ord = cabeca;
        }
        return montar(canvasId, {
            type: 'bar',
            data: {
                labels: ord.map(function (d) { return d.rotulo; }),
                datasets: [{
                    data: ord.map(function (d) { return d.valor; }),
                    backgroundColor: window.CARSTORE_AZUL,
                    borderRadius: 4, borderSkipped: false, barThickness: 'flex', maxBarThickness: 22
                }]
            },
            options: opcoesBase({
                indexAxis: 'y',
                plugins: {
                    legend: { display: false },
                    tooltip: { callbacks: { label: function (c) { return fmt(c.parsed.x, unidade); } } }
                },
                scales: {
                    x: Object.assign(eixoRecessivo(), { ticks: { callback: function (v) { return fmt(v, unidade); }, padding: 8 } }),
                    y: { grid: { display: false }, border: { display: false } }
                }
            })
        });
    },

    // Linha temporal — 1+ séries (paleta categórica), área só p/ série única.
    linhaTempo: function (canvasId, labels, series, unidade) {
        var umaSerie = series.length === 1;
        return montar(canvasId, {
            type: 'line',
            data: {
                labels: labels,
                datasets: series.map(function (s, i) {
                    var cor = s.cor || window.CARSTORE_PALETA[i % 8];
                    return {
                        label: s.nome, data: s.dados,
                        borderColor: cor, backgroundColor: umaSerie ? window.CARSTORE_AZUL_CLARO : 'transparent',
                        fill: umaSerie, tension: 0.3, borderWidth: 2, pointRadius: 0, pointHoverRadius: 4
                    };
                })
            },
            options: opcoesBase({
                interaction: { mode: 'index', intersect: false },
                plugins: {
                    legend: { display: series.length > 1, position: 'bottom' },
                    tooltip: { callbacks: { label: function (c) { return (c.dataset.label ? c.dataset.label + ': ' : '') + fmt(c.parsed.y, unidade); } } }
                },
                scales: {
                    x: { grid: { display: false }, border: { display: false }, ticks: { padding: 8 } },
                    y: Object.assign(eixoRecessivo(), { ticks: { callback: function (v) { return fmt(v, unidade); }, padding: 8 } })
                }
            })
        });
    },

    // Barra agrupada — comparação de 2-4 séries por categoria (paleta categórica).
    barraGrupo: function (canvasId, labels, series, unidade) {
        return montar(canvasId, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: series.map(function (s, i) {
                    return { label: s.nome, data: s.dados, backgroundColor: s.cor || window.CARSTORE_PALETA[i % 8], borderRadius: 4, borderSkipped: false, maxBarThickness: 40 };
                })
            },
            options: opcoesBase({
                plugins: { tooltip: { callbacks: { label: function (c) { return c.dataset.label + ': ' + fmt(c.parsed.y, unidade); } } } },
                scales: {
                    x: { grid: { display: false }, border: { display: false }, ticks: { padding: 8 } },
                    y: Object.assign(eixoRecessivo(), { ticks: { callback: function (v) { return fmt(v, unidade); }, padding: 8 } })
                }
            })
        });
    },

    // Rosca — SÓ parte-do-todo com poucos segmentos (<= 6).
    rosca: function (canvasId, dados, unidade) {
        return montar(canvasId, {
            type: 'doughnut',
            data: {
                labels: dados.map(function (d) { return d.rotulo; }),
                datasets: [{ data: dados.map(function (d) { return d.valor; }), backgroundColor: window.CARSTORE_PALETA.slice(0, dados.length), borderColor: '#fff', borderWidth: 2 }]
            },
            options: opcoesBase({
                cutout: '62%',
                plugins: { tooltip: { callbacks: { label: function (c) { return c.label + ': ' + fmt(c.parsed, unidade); } } } }
            })
        });
    },

    destroy: function (canvasId) {
        var prev = window.carstoreCharts[canvasId];
        if (prev) { try { prev.destroy(); } catch (e) { } delete window.carstoreCharts[canvasId]; }
    }
};

function montar(canvasId, config) {
    var canvas = document.getElementById(canvasId);
    if (!canvas) return false;
    var prev = window.carstoreCharts[canvasId];
    if (prev) { try { prev.destroy(); } catch (e) { } }
    window.carstoreCharts[canvasId] = new Chart(canvas.getContext('2d'), config);
    return true;
}

function fmt(v, unidade) {
    if (v == null || isNaN(v)) return '—';
    if (unidade === 'R$') return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 });
    if (unidade) return v.toLocaleString('pt-BR') + ' ' + unidade;
    return v.toLocaleString('pt-BR');
}
