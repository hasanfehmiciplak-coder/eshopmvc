document.addEventListener("DOMContentLoaded", async () => {
    if (document.body.dataset.area !== "Admin") return;

    const canvas = document.getElementById("weeklyChart");
    if (!canvas) return;

    const data = await fetchJson("/Admin/Dashboard/GetWeeklyStats");
    if (!data || data.length === 0) return;

    const labels = data.map(x => x.date);
    const orderCounts = data.map(x => x.orderCount);
    const revenues = data.map(x => x.revenue);

    new Chart(canvas, {
        type: "bar",
        data: {
            labels,
            datasets: [
                {
                    label: "Sipariş",
                    data: orderCounts
                },
                {
                    label: "Ciro (₺)",
                    data: revenues
                }
            ]
        },
        options: {
            responsive: true,
            scales: {
                y: { beginAtZero: true }
            }
        }
    });
});