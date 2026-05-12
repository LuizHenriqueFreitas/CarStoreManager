// Wrapper minimalista do Chart.js para JSInterop a partir do Blazor.
// Cada gráfico é referenciado pelo id do canvas; chamadas subsequentes
// destroem a instância anterior para evitar duplicação ao re-renderizar.

window.carstoreCharts = window.carstoreCharts || {};

window.carstoreChart = {
    render: function (canvasId, type, labels, datasets, options) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return false;

        // Destrói instância antiga se existir
        const prev = window.carstoreCharts[canvasId];
        if (prev) {
            try { prev.destroy(); } catch (e) { /* ignora */ }
        }

        const ctx = canvas.getContext('2d');
        const chart = new Chart(ctx, {
            type: type,
            data: { labels: labels, datasets: datasets },
            options: options || {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { position: 'bottom' } }
            }
        });
        window.carstoreCharts[canvasId] = chart;
        return true;
    },

    destroy: function (canvasId) {
        const prev = window.carstoreCharts[canvasId];
        if (prev) {
            try { prev.destroy(); } catch (e) { }
            delete window.carstoreCharts[canvasId];
        }
    }
};
