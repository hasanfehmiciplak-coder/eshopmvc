document.addEventListener("DOMContentLoaded", () => {
    if (document.body.dataset.area !== "Admin") return;

    const canvas = document.getElementById("orderStatusChart");
    const input = document.getElementById("orderStatusData");

    if (!canvas || !input) return;

    const data = JSON.parse(input.value);

    new Chart(canvas, {
        type: "doughnut",
        data: {
            labels: data.map(x => x.status),
            datasets: [{
                data: data.map(x => x.count)
            }]
        }
    });
});