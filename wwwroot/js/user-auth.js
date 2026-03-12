$(document).on("submit", "#registerForm", function (e) {
    e.preventDefault();

    $.ajax({
        url: "/Account/Register",
        type: "POST",
        data: $(this).serialize(),
        success: function (resp) {
            if (resp.success) {
                toastr.success("Kayıt başarılı, giriş yapıldı");
                window.location.href = resp.redirectUrl;
            } else {
                $("#registerError").text(resp.message);
            }
        },
        error: function () {
            toastr.error("Sunucu hatası");
        }
    });
});