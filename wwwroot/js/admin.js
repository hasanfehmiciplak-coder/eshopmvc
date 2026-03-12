function initOrderFilter() {
    $(document).on("change", "#orderStatusFilter", function () {
        var status = $(this).val();

        $.ajax({
            url: '/Admin/FilterOrders',
            type: 'GET',
            data: { status: status },
            success: function (html) {
                // Sipariş tablosunu güncelle
                $("#ordersTableBody").html(html);
                toastr.info("Siparişler filtrelendi: " + status);
            },
            error: function () {
                toastr.error("Siparişler filtrelenemedi!");
            }
        });
    });
}

function initOrderStatusUpdateInModal() {
    $(document).on("submit", ".order-status-form", function (e) {
        e.preventDefault();
        var form = $(this);

        $.ajax({
            url: form.attr("action"),
            type: 'POST',
            data: form.serialize(),
            success: function (response) {
                if (response.success) {
                    toastr.success("Sipariş durumu güncellendi!");
                    // Modal içindeki badge güncelle
                    form.closest("div").find(".order-status-badge")
                        .text(response.newStatusText)
                        .attr("class", "badge order-status-badge " + response.newStatusClass);
                } else {
                    toastr.error("Durum güncellenemedi!");
                }
            }
        });
    });
}
function initOrderDetailsModal() {
    $(document).on("click", ".order-details-btn", function (e) {
        e.preventDefault();
        var orderId = $(this).data("id");

        $.ajax({
            url: '/Admin/OrderDetails',
            type: 'GET',
            data: { id: orderId },
            success: function (html) {
                $("#orderDetailsContent").html(html);
                $("#orderDetailsModal").modal('show');
            },
            error: function () {
                toastr.error("Sipariş detayı yüklenemedi!");
            }
        });
    });
}

function initOrderStatusUpdateInModal() {
    $(document).on("submit", ".order-status-form", function (e) {
        e.preventDefault();
        var form = $(this);

        $.ajax({
            url: form.attr("action"),
            type: 'POST',
            data: form.serialize(),
            success: function (response) {
                if (response.success) {
                    toastr.success("Sipariş durumu güncellendi!");
                    // Modal içindeki badge güncelle
                    form.closest("div").find(".order-status-badge")
                        .text(response.newStatusText)
                        .attr("class", "badge order-status-badge " + response.newStatusClass);
                } else {
                    toastr.error("Durum güncellenemedi!");
                }
            },
            error: function () {
                toastr.error("Sunucu hatası oluştu!");
            }
        });
    });
}

function initOrderStatusUpdate() {
    $(document).on("change", ".order-status-select", function () {
        var orderId = $(this).data("id");
        var newStatus = $(this).val();

        $.ajax({
            url: '/Admin/UpdateOrderStatus',
            type: 'POST',
            data: { id: orderId, status: newStatus },
            success: function (response) {
                if (response.success) {
                    toastr.success("Sipariş durumu güncellendi!");
                    $("#orderStatusBadge_" + orderId)
                        .text(response.newStatusText)
                        .attr("class", "badge " + response.newStatusClass);
                } else {
                    toastr.error("Durum güncellenemedi!");
                }
            },
            error: function () {
                toastr.error("Sunucu hatası oluştu!");
            }
        });
    });
}

// admin.js
$(document).ready(function () {
    initOrderFilter();
    initOrderStatusUpdate();
    initOrderDetailsModal();
    initOrderStatusUpdateInModal();
});

body: JSON.stringify({
    orderId: document.getElementById("refundOrderId").value,
    orderItemId: document.getElementById("refundItemId").value,
    quantity: document.getElementById("refundQty").value
})

document.addEventListener("click", function (e) {
    const btn = e.target.closest(".timeline-filter");
    if (!btn) return;

    const filter = btn.dataset.filter;
    const items = document.querySelectorAll(".timeline-item");

    items.forEach(item => {
        if (filter === "all") {
            item.style.display = "";
            return;
        }

        const type = item.dataset.type;

        const map = {
            payment: ["PaymentReceived", "PaymentFailed"],
            refund: ["Refund", "UndoRefund"],
            fraud: ["Fraud"],
            status: ["OrderCreated", "Shipped", "Delivered"]
        };

        if (map[filter]?.includes(type)) {
            item.style.display = "";
        } else {
            item.style.display = "none";
        }
    });
});

document.querySelectorAll(".timeline-filter")
    .forEach(b => b.classList.remove("active"));
btn.classList.add("active");