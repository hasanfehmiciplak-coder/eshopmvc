var ProductModule = (function () {
    function loadProducts(url, data) {
        $("#productContainer").html(`
            <div class="text-center my-4">
                <div class="spinner-border text-primary"></div>
            </div>
        `);

        $.get(url || "/Product/Index", data || {}, function (resp) {
            $("#productContainer").html(resp);
        });
    }

    function bindFilter() {
        $(document).on("submit", "#filterForm", function (e) {
            e.preventDefault();
            loadProducts("/Product/Index", $(this).serialize());
        });
    }

    function bindPagination() {
        $(document).on("click", ".pagination a", function (e) {
            e.preventDefault();
            loadProducts($(this).attr("href"));
        });
    }

    return {
        init: function () {
            bindFilter();
            bindPagination();
        }
    };
})();

$(document).ready(function () {
    ProductModule.init();
});

$(document).on("click", ".filter-btn", function () {
    let categoryId = $(this).data("id");

    $.ajax({
        url: "/Products/FilterByCategory",
        type: "GET",
        data: { categoryId: categoryId },
        success: function (html) {
            $("#productList").html(html);
        },
        error: function () {
            toastr.error("Ürünler yüklenemedi");
        }
    });
});

$(document).on("click", ".product-detail-btn", function () {
    let productId = $(this).data("id");

    $("#productDetailContent").html(
        '<div class="modal-body text-center"><div class="spinner-border"></div></div>'
    );

    $.get("/Products/DetailPartial/" + productId, function (html) {
        $("#productDetailContent").html(html);
    });
});

function openProductModal(productId) {
    $.get('/Product/DetailModal/' + productId, function (data) {
        $('#productModalContent').html(data);
        $('#productModal').modal('show');
    });
}