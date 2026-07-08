/**
 * dashboard-charts.js
 * Wrappers Chart.js para Home.razor — diseño corporativo azul-violeta.
 * window._charts persiste entre navegaciones SPA para evitar re-declaración.
 */

window._charts = window._charts || {};

/**
 * Devuelve una versión del color más clara u oscura según su luminosidad.
 * - Color oscuro (promedio RGB ≤ 180) → aclara sumando amount.
 * - Color claro  (promedio RGB  > 180) → oscurece restando amount.
 * Evita que colores ya muy claros (ej. gris #CBD5E1) se vuelvan invisibles.
 */
function hoverColor(hex, amount = 50) {
    const r = parseInt(hex.slice(1, 3), 16);
    const g = parseInt(hex.slice(3, 5), 16);
    const b = parseInt(hex.slice(5, 7), 16);
    const esClaro = (r + g + b) / 3 > 180;
    const ajustar = (v) => esClaro
        ? Math.max(0,   v - amount)
        : Math.min(255, v + amount);
    return '#' + [ajustar(r), ajustar(g), ajustar(b)]
        .map(v => v.toString(16).padStart(2, '0')).join('');
}

/**
 * Dona de Estado de Competencias.
 * Paleta: indigo (finalizadas), azul (en ejecución), rojo (próx. cerrar), gris (pendientes).
 */
window.renderDonutChart = function (canvasId, labels, data, colors) {
    if (window._charts[canvasId]) {
        window._charts[canvasId].destroy();
        delete window._charts[canvasId];
    }

    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    window._charts[canvasId] = new Chart(canvas, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: colors,
                borderColor: '#ffffff',
                borderWidth: 4,
                hoverBorderColor: colors.map(c => hoverColor(c)),
                hoverBorderWidth: 4,
                hoverOffset: 10
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '74%',
            layout: {
                padding: 10   // evita que los bordes y el hoverOffset se recorten
            },
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: '#1E293B',
                    titleFont: { family: 'Poppins', size: 12 },
                    bodyFont: { family: 'Poppins', size: 12 },
                    callbacks: {
                        label: ctx => `  ${ctx.label}: ${ctx.parsed}`
                    }
                }
            }
        }
    });
};

/**
 * Barras verticales de Progreso por Fichas.
 * Planeadas → gris claro.
 * Ejecutadas → color según ratio (rojo / azul / violeta).
 */
window.renderBarChart = function (canvasId, labels, planeadas, ejecutadas) {
    if (window._charts[canvasId]) {
        window._charts[canvasId].destroy();
        delete window._charts[canvasId];
    }

    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ejecutadasColors = ejecutadas.map((ej, i) => {
        const ratio = planeadas[i] > 0 ? ej / planeadas[i] : 0;
        if (ratio >= 0.70) return '#6D28D9'; // violeta — buen avance
        if (ratio >= 0.30) return '#3B82F6'; // azul    — avance normal
        return '#EF4444';                     // rojo    — avance bajo
    });

    window._charts[canvasId] = new Chart(canvas, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Horas Planeadas',
                    data: planeadas,
                    backgroundColor: '#E2E8F0',
                    borderWidth: 0,
                    borderRadius: 6,
                    borderSkipped: false
                },
                {
                    label: 'Horas Ejecutadas',
                    data: ejecutadas,
                    backgroundColor: ejecutadasColors,
                    borderWidth: 0,
                    borderRadius: 6,
                    borderSkipped: false
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: {
                    grid: { display: false },
                    ticks: { font: { family: 'Poppins', size: 11 }, color: '#94A3B8' }
                },
                y: {
                    beginAtZero: true,
                    grid: { color: '#F1F5F9', drawBorder: false },
                    ticks: { font: { family: 'Poppins', size: 11 }, color: '#94A3B8' }
                }
            },
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        font: { family: 'Poppins', size: 12 },
                        color: '#64748B',
                        usePointStyle: true,
                        pointStyle: 'rectRounded',
                        padding: 20
                    }
                },
                tooltip: {
                    backgroundColor: '#1E293B',
                    titleFont: { family: 'Poppins', size: 12 },
                    bodyFont: { family: 'Poppins', size: 12 }
                }
            }
        }
    });
};
