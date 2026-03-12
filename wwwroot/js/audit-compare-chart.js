document.addEventListener("DOMContentLoaded", async () => {
    if (document.body.dataset.area !== "Admin") return;

    const canvas = document.getElementById("auditCompareChart");
    if (!canvas) return;

    const data = await fetchJson("/Admin/Dashboard/GetAuditCompareStats");
    if (!data) return;

    const labels = data.map(x => x.date);
    const success = data.map(x => x.success);
    const fail = data.map(x => x.fail);

    new Chart(canvas, {
        type: "bar",
        data: {
            labels: labels,
            datasets: [
                {
                    label: "Başarılı Giriş",
                    data: success,
                    backgroundColor: "#198754"
                },
                {
                    label: "Başarısız Giriş",
                    data: fail,
                    backgroundColor: "#dc3545"
                }
            ]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: { precision: 0 }
                }
            }
        }
    });
});