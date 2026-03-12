document.addEventListener("DOMContentLoaded", async () => {
    // 🔒 SADECE ADMIN
    if (document.body.dataset.area !== "Admin") return;

    const canvas = document.getElementById("auditIpChart");
    if (!canvas) return;

    const data = await fetchJson("/Admin/Dashboard/GetIpAuditStats");
    if (!data || data.length === 0) return;

    const labels = data.map(x => x.ip);
    const values = data.map(x => x.count);

    new Chart(canvas, {
        type: "bar",
        data: {
            labels: labels,
            datasets: [{
                label: "IP Bazlı Şüpheli Giriş",
                data: values,
                backgroundColor: "#0dcaf0"
            }]
        },
        options: {
            responsive: true,
            indexAxis: "y", // yatay bar
            plugins: {
                legend: { display: false }
            },
            scales: {
                x: {
                    beginAtZero: true,
                    ticks: { precision: 0 }
                }
            }
        }
    });
});