document.addEventListener("DOMContentLoaded", () => {
    const datos = JSON.parse(document.getElementById("datos-reporte").textContent);
    const dinero = new Intl.NumberFormat("es-MX", { style: "currency", currency: "MXN" });
    const azul = "#2563eb";

    function grafica(idCanvas, tipo, etiquetas, valores, titulo, esDinero) {
        const canvas = document.getElementById(idCanvas);
        if (!canvas) return;

        new Chart(canvas, {
            type: tipo,
            data: { labels: etiquetas, datasets: [{ label: titulo, data: valores, backgroundColor: azul, borderRadius: 4 }] },
            options: {
                indexAxis: tipo === "bar" && idCanvas === "grafica-productos" ? "y" : "x",
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: { callbacks: { label: c => esDinero ? dinero.format(c.parsed.y ?? c.parsed.x) : c.formattedValue + " uds" } }
                },
                scales: { x: { grid: { display: false }, ticks: { precision: 0 } }, y: { beginAtZero: true, ticks: { precision: 0 } } }
            }
        });
    }

    grafica("grafica-diaria", "bar", datos.diarias.map(d => d.etiqueta), datos.diarias.map(d => d.total), "Ventas", true);
    grafica("grafica-mensual", "bar", datos.mensuales.map(m => m.etiqueta), datos.mensuales.map(m => m.total), "Ventas", true);
    grafica("grafica-productos", "bar", datos.masVendidos.map(p => p.nombre), datos.masVendidos.map(p => p.unidades), "Unidades", false);
});
