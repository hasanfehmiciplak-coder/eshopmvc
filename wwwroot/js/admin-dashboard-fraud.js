document.addEventListener("DOMContentLoaded", async () => {
    if (document.body.dataset.area !== "Admin") return;

    const canvas = document.getElementById("refundFraudChart");
    if (!canvas) return;

    const data = await fetchJson("/Admin/Dashboard/GetRefundFraudStats");
    if (!data) return;

    new Chart(canvas, {
        type: "doughnut",
        data: {
            labels: data.map(x => x.label),
            datasets: [{
                data: data.map(x => x.count)
            }]
        }
    });
});