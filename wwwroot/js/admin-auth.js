$("input[name='UserName']").val($("input[name='Email']").val());

$(document).off("submit", "#registerForm"); // 🔥 EMNİYET
$(document).on("submit", "#registerForm", function (e) {
    e.preventDefault();

    console.log("REGISTER SUBMIT (TEK)");

    $.ajax({
        url: "/Account/Register",
        type: "POST",
        data: $(this).serialize(),
        success: function (res) {
            if (res.success) {
                window.location.href = res.redirectUrl;
            } else {
                toastr.error(res.message);
            }
        }
    });
});

$(document).off("submit", "#userLoginForm");
$(document).on("submit", "#userLoginForm", function (e) {
    e.preventDefault();

    $.ajax({
        url: "/Account/Login",
        type: "POST",
        data: $(this).serialize(),
        success: function (res) {
            if (res.success) {
                window.location.href = res.redirectUrl;
            } else {
                toastr.error(res.message);
            }
        }
    });
});

$(document).off("submit", "#adminLoginForm");
$(document).on("submit", "#adminLoginForm", function (e) {
    e.preventDefault();

    $.ajax({
        url: "/Admin/Account/Login",
        type: "POST",
        data: $(this).serialize(),
        success: function (res) {
            if (res.success) {
                window.location.href = res.redirectUrl;
            } else {
                toastr.error(res.message);
            }
        }
    });
});

$(document).off("submit", "#adminLogoutForm");
$(document).on("submit", "#adminLogoutForm", function (e) {
    e.preventDefault();

    $.post("/Admin/Account/Logout", $(this).serialize())
        .done(() => window.location.href = "/Admin/Account/Login");
});

$(document).off("click", "#logoutBtn");
$(document).on("click", "#logoutBtn", function () {
    $.post("/Admin/Account/Logout", {
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    })
        .done(() => {
            window.location.href = "/Admin/Account/Login";
        });
});