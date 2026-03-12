// =======================
// CART.JS (TEK KAYNAK)
// =======================

function updateCartCount() {
    fetch("/Cart/Count")
        .then(r => r.json())
        .then(count => {
            if ($("#cartCount").length === 0) return;

            if (count > 0) {
                $("#cartCount").text(count).removeClass("d-none");
            } else {
                $("#cartCount").addClass("d-none");
            }
        })
        .catch(() => {
            console.warn("Cart count alınamadı");
        });
}

$(document).ready(function () {
    updateCartCount();

    // ➕ Sepete ekle (adet = 1)
    $(document).on("click", ".add-to-cart", function (e) {
        e.preventDefault();

        const productId = $(this).data("id");

        $.post("/Cart/Add", { productId })
            .done(resp => {
                if (resp.success) {
                    toastr.success("Ürün sepete eklendi");
                    $("#cartCount").text(resp.count).removeClass("d-none");
                } else {
                    toastr.error(resp.message || "Hata oluştu");
                }
            })
            .fail(() => {
                toastr.error("Sepete eklenemedi");
            });
    });

    // ➕ Sepete ekle (adetli – modal / detay)
    $(document).on("click", ".add-to-cart-with-qty", function () {
        const productId = $(this).data("id");
        const quantity = parseInt($("#qtyInput").val());

        if (!quantity || quantity <= 0) {
            toastr.warning("Geçerli bir adet girin");
            return;
        }

        $.post("/Cart/Add", { productId, quantity })
            .done(resp => {
                if (resp.success) {
                    toastr.success("Ürün sepete eklendi 🛒");
                    $("#cartCount").text(resp.count).removeClass("d-none");
                    $("#productModal").modal("hide");
                } else {
                    toastr.error(resp.message);
                }
            })
            .fail(() => {
                toastr.error("Sepete eklenemedi");
            });
    });
});

function updateQty(productId, qty) {
    fetch('/Cart/UpdateQuantity', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: `productId=${productId}&quantity=${qty}`
    })
        .then(r => r.json())
        .then(data => {
            if (!data.success) {
                toastr.warning(data.message ?? "İşlem başarısız");
                return;
            }

            // 🔁 DB zaten doğru → UI’yi yenile
            location.reload();
        });
}