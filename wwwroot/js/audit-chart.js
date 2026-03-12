document.addEventListener("DOMContentLoaded", async () => {
    // 🔒 SADECE ADMIN AREA
    if (document.body.dataset.area !== "Admin") return;

    // 🎯 Canvas var mı?
    const canvas = document.getElementById("auditChart");
    if (!canvas) return;

    // 📡 Veri çek
    const data = await fetchJson("/Admin/Dashboard/GetAuditStats");
    if (!data || data.length === 0) return;

    const labels = data.map(x => x.date);
    const values = data.map(x => x.count);

    new Chart(canvas, {
        type: "line",
        data: {
            labels: labels,
            datasets: [{
                label: "Başarısız Giriş",
                data: values,
                borderColor: "#dc3545",
                backgroundColor: "rgba(220,53,69,0.15)",
                fill: true,
                tension: 0.3
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { display: false }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: { precision: 0 }
                }
            }
        }
    });
});