// ===============================
// TOASTR AYARLARI
// ===============================
if (typeof toastr !== "undefined") {
    toastr.options = {
        closeButton: true,
        progressBar: true,
        positionClass: "toast-top-center",
        timeOut: 4000
    };
}

// ===============================
// LOGOUT (ADMIN + USER ORTAK)
// ===============================
$(document).on("submit", "#logoutForm", function (e) {
    e.preventDefault();

    $.post("/Account/Logout", $(this).serialize(), function (resp) {
        if (resp.success) {
            window.location.href = resp.redirectUrl;
        }
    });
});

// ===============================
// ADMIN → ROL DEĞİŞTİR
// ===============================
$(document).on("click", ".toggle-role-btn", function () {
    let userId = $(this).data("id");

    $.ajax({
        url: "/Admin/User/ToggleRole",
        type: "POST",
        data: { id: userId },
        success: function (resp) {
            if (resp.success) {
                $("#role-" + userId).text(resp.role);
                toastr.success("Rol güncellendi");
            } else {
                toastr.error(resp.message);
            }
        },
        error: function () {
            toastr.error("Sunucu hatası");
        }
    });
});

// ===============================
// ADMIN → AKTİF / PASİF
// ===============================
$(document).on("click", ".toggleStatusBtn", function () {
    let id = $(this).data("id");

    $.post("/Admin/Products/ToggleStatus", { id }, function (resp) {
        if (resp.success) {
            let badge = resp.status === "Aktif"
                ? '<span class="badge bg-success">Aktif</span>'
                : '<span class="badge bg-secondary">Pasif</span>';

            $("#status-" + id).html(badge);
            toastr.success("Durum güncellendi");
        } else {
            toastr.error("İşlem başarısız");
        }
    });
});

$(document).on("click", "#cancelRequestBtn", function () {
    let orderId = $(this).data("id");

    if (!confirm("Siparişi iptal etmek istediğinize emin misiniz?"))
        return;

    $.ajax({
        url: "/User/Orders/RequestCancel",
        type: "POST",
        data: { id: orderId },
        success: function (resp) {
            if (resp.success) {
                toastr.success("İptal talebi gönderildi");
                location.reload();
            } else {
                toastr.error(resp.message);
            }
        },
        error: function () {
            toastr.error("Sunucu hatası");
        }
    });
});

// ===============================
// ürün durum değiştir(AJAX)
// ===============================

$(document).on("click", ".toggleProductStatusBtn", function () {
    let id = $(this).data("id");

    $.ajax({
        url: "/Admin/Products/ToggleStatus",
        type: "POST",
        data: { id: id },
        success: function (resp) {
            if (resp.success) {
                let badge = resp.status === "Aktif"
                    ? '<span class="badge bg-success">Aktif</span>'
                    : '<span class="badge bg-danger">Pasif</span>';

                $("#status-" + id).html(badge);
                toastr.success("Durum güncellendi");
            }
            else {
                toastr.error(resp.message);
            }
        },
        error: function () {
            toastr.error("Sunucu hatası");
        }
    });
});

$(document).on("click", ".increase", function () {
    let row = $(this).closest("tr");
    let id = row.data("id");

    $.post("/Cart/Increase", { productId: id }, function (resp) {
        if (!resp.success) {
            toastr.warning(resp.message);
            return;
        }

        row.find(".qty").text(resp.quantity);
        row.find(".row-total").text(resp.rowTotal + " ₺");
        $("#cartTotal").text(resp.cartTotal + " ₺");
    });
});

$(document).on("click", ".decrease", function () {
    let row = $(this).closest("tr");
    let id = row.data("id");

    $.post("/Cart/Decrease", { productId: id }, function (resp) {
        if (resp.quantity === 0) {
            row.remove();
        } else {
            row.find(".qty").text(resp.quantity);
            row.find(".row-total").text(resp.rowTotal + " ₺");
        }

        $("#cartTotal").text(resp.cartTotal + " ₺");
    });
});

$(document).on("click", ".remove", function () {
    let row = $(this).closest("tr");
    let id = row.data("id");

    $.post("/Cart/Remove", { productId: id }, function () {
        row.remove();
    });
});

$(document).on("click", ".show-product-detail", function () {
    let productId = $(this).data("id");

    $.ajax({
        url: "/Products/DetailPartial",
        type: "GET",
        data: { id: productId },
        success: function (html) {
            $("#productModalContent").html(html);
            $("#productModal").modal("show");
        },
        error: function () {
            toastr.error("Ürün detayı yüklenemedi");
        }
    });
});

$(document).on("click", ".open-product-modal", function () {
    let id = $(this).data("id");

    $("#productModalContent").html("Yükleniyor...");

    $("#productModal").modal("show");

    $.get("/Products/DetailModal/" + id, function (html) {
        $("#productModalContent").html(html);
    });
});

async function fetchJson(url, options = {}) {
    try {
        const res = await fetch(url, options);

        // Yetkisiz / forbidden → sessizce çık
        if (res.status === 401 || res.status === 403) {
            return null;
        }

        if (!res.ok) {
            console.warn("HTTP error:", res.status, url);
            return null;
        }

        const text = await res.text();

        // HTML geldiyse (login page / error page)
        if (!text || text.trim().startsWith("<")) {
            return null;
        }

        return JSON.parse(text);
    }
    catch (err) {
        console.error("fetchJson error:", err);
        return null;
    }
}

document.addEventListener("DOMContentLoaded", async () => {
    const el = document.getElementById("cartCount");
    if (!el) return;

    const count = await fetchJson("/Cart/Count");
    if (count !== null) {
        el.innerText = count;
    }
});

$(function () {
    if (typeof CartModule !== "undefined") {
        CartModule.init();
    }
});

<script>
    document.addEventListener("click", function (e) {
    // 💸 REFUND TOGGLE
    const refundBtn = e.target.closest(".toggle-refund");
    if (refundBtn) {
        const details = refundBtn.parentElement
    .querySelector(".refund-details");

    if (!details) return;

    details.classList.toggle("d-none");

    refundBtn.innerText = details.classList.contains("d-none")
    ? "Detayı Gör"
    : "Detayı Gizle";

    return; // 👉 burada bitiriyoruz
    }

    // 🚨 FRAUD TOGGLE
    const fraudBtn = e.target.closest(".toggle-fraud");
    if (fraudBtn) {
        const fraudDetails = fraudBtn.parentElement
    .querySelector(".fraud-details");

    if (!fraudDetails) return;

    fraudDetails.classList.toggle("d-none");

    fraudBtn.innerText = fraudDetails.classList.contains("d-none")
    ? "Fraud Detayını Gör"
    : "Fraud Detayını Gizle";

    return;
    }
});
</script>