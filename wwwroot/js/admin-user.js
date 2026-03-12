$(document).on("click", ".change-role", function () {
    const userId = $(this).data("id");
    const newRole = $(this).data("role");

    $.post("/Admin/User/ChangeRole",
        { userId, newRole },
        function (resp) {
            if (resp.success) {
                toastr.success("Rol güncellendi");
                location.reload();
            } else {
                toastr.error(resp.message || "İşlem başarısız");
            }
        }
    ).fail(function () {
        toastr.error("Sunucu hatası");
    });
});

$(document).on("click", ".toggle-status-btn", function () {
    let userId = $(this).data("id");

    $.post("/Admin/User/ToggleStatus", { id: userId }, function (resp) {
        if (!resp.success) {
            toastr.error(resp.message);
            return;
        }

        let badge = resp.isActive
            ? '<span class="badge bg-success">Aktif</span>'
            : '<span class="badge bg-danger">Pasif</span>';

        $("#status-" + userId).html(badge);

        toastr.success("Kullanıcı durumu güncellendi");
    });
});