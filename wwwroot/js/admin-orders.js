$(document).on("change", ".order-status", function (e) {
    e.preventDefault(); // 🔥 ÇOK ÖNEMLİ

    const orderId = $(this).data("order-id");
    const status = $(this).val();

    $.ajax({
        url: "/Admin/Orders/ChangeStatus",
        type: "POST",
        data: {
            orderId: orderId,
            status: status
        },
        success: function () {
            toastr.success("Sipariş durumu güncellendi");
        },
        error: function (xhr) {
            toastr.error("Hata: " + xhr.status);
        }
    });
});

$("#approveCancelBtn").on("click", function () {
    let id = $(this).data("id");

    $.post("/Admin/Orders/ApproveCancel", { id }, function (resp) {
        if (resp.success) {
            toastr.success("Sipariş iptal edildi");
            location.reload();
        }
    });
});

$("#rejectCancelBtn").on("click", function () {
    let id = $(this).data("id");

    $.post("/Admin/Orders/RejectCancel", { id }, function (resp) {
        if (resp.success) {
            toastr.info("İptal talebi reddedildi");
            location.reload();
        }
    });
});

$(document).on("click", "#approveCancelBtn", function () {
    let id = $(this).data("id");

    $.post("/Admin/Orders/ApproveCancel", { id }, function (resp) {
        if (resp.success) {
            toastr.success("Sipariş iptal edildi");
            location.reload();
        }
    });
});

$(document).on("click", "#rejectCancelBtn", function () {
    let id = $(this).data("id");

    $.post("/Admin/Orders/RejectCancel", { id }, function (resp) {
        if (resp.success) {
            toastr.info("İptal talebi reddedildi");
            location.reload();
        }
    });
});