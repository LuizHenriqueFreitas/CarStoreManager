// Wrapper do Chart.js para JSInterop a partir do Blazor.
// Cada gráfico é referenciado pelo id do canvas; chamadas subsequentes
// destroem a instância anterior para evitar duplicação ao re-renderizar.
//
// Diretrizes de data-viz (docs/redesign/11-batch2-melhorias.md §E,
// docs/redesign/21-pizza-fatia-pequena.md):
//  - comparar magnitude entre muitas categorias  -> barra HORIZONTAL, 1 tom, ordenada
//  - tendência no tempo                           -> linha
//  - parte-do-todo (<= 6 segmentos, TODOS >= 10%) -> rosca
//  - parte-do-todo com alguma fatia < 10%         -> barra VERTICAL (rosca fica ilegível)
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

// Plugin nativo do Chart.js (sem lib externa) — desenha o valor formatado
// acima de cada barra. Só ativa por gráfico, via
// options.plugins.valorAcimaDaBarra = { enabled: true, unidade }; gráfico
// que não passa essa opção não é afetado. Usado pelas barras verticais que
// substituem a rosca com fatia pequena (ver docs/redesign/21-pizza-fatia-
// pequena.md) — mesma ideia do "% no hover" que a rosca já mostrava, só
// que sempre visível em vez de precisar passar o mouse.
Chart.register({
    id: 'valorAcimaDaBarra',
    afterDatasetsDraw: function (chart) {
        var opts = chart.options.plugins && chart.options.plugins.valorAcimaDaBarra;
        if (!opts || !opts.enabled) return;
        var ctx = chart.ctx;
        ctx.save();
        ctx.fillStyle = TINTA;
        ctx.font = '600 11px system-ui, -apple-system, "Segoe UI", Roboto, sans-serif';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'bottom';
        chart.data.datasets.forEach(function (dataset, di) {
            var meta = chart.getDatasetMeta(di);
            if (meta.hidden) return;
            meta.data.forEach(function (bar, i) {
                var valor = dataset.data[i];
                if (valor == null) return;
                ctx.fillText(fmt(valor, opts.unidade), bar.x, bar.y - 6);
            });
        });
        ctx.restore();
    }
});

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

    // Barra HORIZONTAL de magnitude: 1 série, ordena desc, corta silenciosamente
    // após maxItens — sem agregar "Outros" (os que ficam de fora, ficam de
    // fora mesmo; ver docs/redesign/13-padrao-graficos.md).
    // dados = [{ rotulo, valor }]  ·  unidade opcional ("R$", "un", "dias")
    barraMagnitude: function (canvasId, dados, unidade, maxItens) {
        maxItens = maxItens || 10;
        var ord = (dados || []).slice().sort(function (a, b) { return b.valor - a.valor; }).slice(0, maxItens);
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

    // Barra VERTICAL de magnitude — substitui a rosca quando pelo menos uma
    // fatia representaria menos de 10% do total (fatia fina em rosca fica
    // ilegível/sem espaço pro rótulo; ver docs/redesign/21-pizza-fatia-
    // pequena.md). Mesma ordenação desc de barraMagnitude, mas SEM cortar
    // em 10 itens — só chega aqui vindo de carstoreChart.comparativo, que
    // já garante <= 6 categorias antes de decidir entre rosca e esta.
    // Tooltip mostra o percentual do total, igual a rosca.
    barraVerticalMagnitude: function (canvasId, dados, unidade) {
        var ord = (dados || []).slice().sort(function (a, b) { return b.valor - a.valor; });
        var total = ord.reduce(function (s, d) { return s + d.valor; }, 0);
        return montar(canvasId, {
            type: 'bar',
            data: {
                labels: ord.map(function (d) { return d.rotulo; }),
                datasets: [{
                    data: ord.map(function (d) { return d.valor; }),
                    backgroundColor: window.CARSTORE_AZUL,
                    borderRadius: 4, borderSkipped: false, barThickness: 'flex', maxBarThickness: 56
                }]
            },
            options: opcoesBase({
                layout: { padding: { top: 20 } }, // espaço pro rótulo do valor não cortar no topo
                plugins: {
                    legend: { display: false },
                    valorAcimaDaBarra: { enabled: true, unidade: unidade },
                    tooltip: {
                        callbacks: {
                            label: function (c) {
                                var pct = total > 0 ? (c.parsed.y / total * 100) : 0;
                                var pctTxt = pct.toLocaleString('pt-BR', { maximumFractionDigits: 1 }) + '%';
                                return pctTxt + ' (' + fmt(c.parsed.y, unidade) + ')';
                            }
                        }
                    }
                },
                scales: {
                    x: { grid: { display: false }, border: { display: false }, ticks: { padding: 8 } },
                    y: Object.assign(eixoRecessivo(), { ticks: { callback: function (v) { return fmt(v, unidade); }, padding: 8 } })
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

    // Rosca — SÓ parte-do-todo com poucos segmentos (<= 6) e nenhuma fatia
    // abaixo de 10% do total (ver comparativo, que decide isso antes de
    // chamar aqui). Hover mostra o nome e a porcentagem que a fatia ocupa
    // do total, além do valor.
    rosca: function (canvasId, dados, unidade) {
        var total = (dados || []).reduce(function (s, d) { return s + d.valor; }, 0);
        return montar(canvasId, {
            type: 'doughnut',
            data: {
                labels: dados.map(function (d) { return d.rotulo; }),
                datasets: [{ data: dados.map(function (d) { return d.valor; }), backgroundColor: window.CARSTORE_PALETA.slice(0, dados.length), borderColor: '#fff', borderWidth: 2 }]
            },
            options: opcoesBase({
                cutout: '62%',
                plugins: {
                    tooltip: {
                        callbacks: {
                            label: function (c) {
                                var pct = total > 0 ? (c.parsed / total * 100) : 0;
                                var pctTxt = pct.toLocaleString('pt-BR', { maximumFractionDigits: 1 }) + '%';
                                return c.label + ': ' + pctTxt + ' (' + fmt(c.parsed, unidade) + ')';
                            }
                        }
                    }
                }
            })
        });
    },

    // Padrão único de comparação categórica do sistema (ver
    // docs/redesign/13-padrao-graficos.md, docs/redesign/21-pizza-fatia-
    // pequena.md): até 6 categorias E todas com 10% ou mais do total ->
    // rosca; mais de 6 -> barra horizontal só com as 10 maiores, sem
    // "Outros"; até 6 categorias mas ALGUMA com menos de 10% do total ->
    // barra vertical (rosca com fatia fina fica ilegível e some o rótulo).
    // Todo gráfico que compara "tipos" de um dado (status, marca,
    // categoria...) deve usar esta função em vez de chamar
    // rosca/barraMagnitude/barraVerticalMagnitude diretamente.
    // dados = [{ rotulo, valor }]  ·  unidade opcional ("R$", "un", "dias")
    comparativo: function (canvasId, dados, unidade) {
        var ord = (dados || [])
            .filter(function (d) { return d && d.valor > 0; })
            .slice()
            .sort(function (a, b) { return b.valor - a.valor; });

        if (ord.length === 0) {
            window.carstoreChart.destroy(canvasId);
            return false;
        }
        if (ord.length > 6) return window.carstoreChart.barraMagnitude(canvasId, ord.slice(0, 10), unidade, 10);

        var total = ord.reduce(function (s, d) { return s + d.valor; }, 0);
        var temFatiaAbaixoDe10Pct = ord.some(function (d) { return total > 0 && (d.valor / total) < 0.10; });
        if (temFatiaAbaixoDe10Pct) return window.carstoreChart.barraVerticalMagnitude(canvasId, ord, unidade);
        return window.carstoreChart.rosca(canvasId, ord, unidade);
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
