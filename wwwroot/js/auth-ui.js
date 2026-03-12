$(document).on("submit", "form[data-auth-form]", function () {
    const btn = $(this).find("button[type=submit]");
    btn.prop("disabled", true);
    btn.text("Lütfen bekleyin...");
});