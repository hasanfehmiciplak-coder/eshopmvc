// Düzenleme işlemi için kod

$(document).ready(function () {
    // Düzenle butonuna basınca modal aç
    $(".editBtn").on("click", function () {
        var id = $(this).data("id");

        $.get("/Admin/Category/GetCategory", { id: id }, function (data) {
            $("#editId").val(data.id);
            $("#editName").val(data.name);
            $("#editModal").modal("show");
        });
    });

    // Edit form submit
    $("#editForm").on("submit", function (e) {
        e.preventDefault();

        $.ajax({
            url: '/Admin/Category/EditAjax',
            type: 'POST',
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    $("#editModal").modal("hide");
                    location.reload(); // listeyi yenile
                } else {
                    toastr.error(response.message);
                }
            },
            error: function () {
                toastr.error("Bir hata oluştu.");
            }
        });
    });
});

// Silme işlemi için kod

$(document).ready(function () {
    // Sil butonuna basınca modal aç
    $(".deleteBtn").on("click", function () {
        var id = $(this).data("id");
        $("#deleteId").val(id);
        $("#deleteModal").modal("show");
    });

    // Delete form submit
    $("#deleteForm").on("submit", function (e) {
        e.preventDefault();
        var id = $("#deleteId").val();

        $.ajax({
            url: '/Admin/Category/DeleteAjax',
            type: 'POST',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    $("#deleteModal").modal("hide");
                    $("#row-" + id).remove(); // tablo satırını kaldır
                } else {
                    toastr.error(response.message);
                }
            },
            error: function () {
                toastr.error("Bir hata oluştu.");
            }
        });
    });
});