$(document).on("click", ".toggle-status", function () {
    const id = $(this).data("id");

    $.post("/Admin/Product/ToggleStatus", { id })
        .done(function () {
            toastr.success("Durum güncellendi");
            location.reload();
        })
        .fail(function () {
            toastr.error("İşlem başarısız");
        });
});