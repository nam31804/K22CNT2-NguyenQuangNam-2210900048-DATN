let categoryModal = null;

$(function () {
    categoryModal = new bootstrap.Modal(
        document.getElementById("categoryModal")
    );
});

// reload bảng
function reloadCategoryTable() {
    reloadPartial("/Admin/Category/ReloadTable", "categoryTable");
}

// mở modal thêm
function openCreateCategory() {
    $("#categoryModalTitle").text("Thêm thể loại");
    resetCategoryForm();
    $("#categoryProductsBox").addClass("d-none");
    categoryModal.show();
}

// mở modal sửa
function openEditCategory(id) {
    $("#categoryModalTitle").text("Sửa thể loại");

    $.get("/Admin/Category/Get/" + id, function (data) {
        $("#catId").val(data.id);
        $("#catName").val(data.name);
        $("#catDesc").val(data.description);

        // hiển thị sản phẩm trong thể loại
        if (data.products && data.products.length > 0) {
            let html = "";
            data.products.forEach(p => {
                html += `<li class="list-group-item">${p.name}</li>`;
            });
            $("#categoryProducts").html(html);
            $("#categoryProductsBox").removeClass("d-none");
        } else {
            $("#categoryProductsBox").addClass("d-none");
        }

        categoryModal.show();
    });
}

// submit form
$("#categoryForm").on("submit", function (e) {
    e.preventDefault();

    $.post("/Admin/Category/Save", $(this).serialize())
        .done(function () {
            categoryModal.hide();
            reloadCategoryTable();
            resetCategoryForm();
        })
        .fail(function (err) {
            alert(err.responseText);
        });
});

// xoá
function deleteCategory(id) {
    if (!confirm("Xóa thể loại này?")) return;

    $.post("/Admin/Category/Delete", { id: id })
        .done(function () {
            reloadCategoryTable();
        })
        .fail(function (err) {
            alert(err.responseText);
        });
}

function resetCategoryForm() {
    $("#catId").val("");
    $("#categoryForm")[0].reset();
}
