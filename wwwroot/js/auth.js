document.addEventListener("DOMContentLoaded", () => {
    const loginForm = document.getElementById("loginForm");
    if (!loginForm) return;

    loginForm.addEventListener("submit", async (e) => {
        e.preventDefault();

        const formData = new FormData(loginForm);

        try {
            const res = await fetch(loginForm.action, {
                method: "POST",
                body: formData,
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            if (!res.ok) {
                toastr.error("Sunucu hatası");
                return;
            }

            const data = await res.json();

            if (data.success) {
                window.location.href = data.redirectUrl;
            } else {
                toastr.error(data.message || "Giriş başarısız");
            }
        } catch {
            toastr.error("Bağlantı hatası");
        }
    });
});