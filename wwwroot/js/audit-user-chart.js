document.addEventListener("DOMContentLoaded", async () => {
    // 🔒 SADECE ADMIN
    if (document.body.dataset.area !== "Admin") return;

    const canvas = document.getElementById("auditUserChart");
    if (!canvas) return;

    const data = await fetchJson("/Admin/Dashboard/GetUserAuditStats");
    if (!data || data.length === 0) return;

    const labels = data.map(x => x.user);
    const values = data.map(x => x.count);

    new Chart(canvas, {
        type: "bar",
        data: {
            labels: labels,
            datasets: [{
                label: "Kullanıcı Bazlı Şüpheli Giriş",
                data: values,
                backgroundColor: "#ffc107"
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