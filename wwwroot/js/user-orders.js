$(document).on("click", "#cancelOrderBtn", function () {
    let id = $(this).data("id");

    if (!confirm("Siparişi iptal etmek istiyor musunuz?"))
        return;

    $.post("/Orders/RequestCancel", { id }, function (resp) {
        if (resp.success) {
            toastr.success("İptal talebi oluşturuldu");
            location.reload();
        } else {
            toastr.error(resp.message || "İşlem başarısız");
        }
    });
});