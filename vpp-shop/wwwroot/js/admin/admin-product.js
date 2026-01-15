let productModal;

document.addEventListener("DOMContentLoaded", () => {
    productModal = new bootstrap.Modal(
        document.getElementById("productModal")
    );
});

// ================= THÊM =================
function openCreateModal() {
    const form = document.getElementById("productForm");
    form.action = "/Admin/Product/Create";
    form.reset();

    document.getElementById("modalTitle").innerText = "Thêm sản phẩm";
    document.getElementById("productId").value = "";
    document.getElementById("imagePreview").style.display = "none";
    document.getElementById("imageFile").value = "";

    productModal.show();
}

// ================= SỬA =================
function openEditModal(id) {
    fetch(`/Admin/Product/GetProduct?id=${id}`)
        .then(res => res.json())
        .then(p => {
            const form = document.getElementById("productForm");
            form.action = "/Admin/Product/Edit";

            document.getElementById("modalTitle").innerText = "Cập nhật sản phẩm";

            document.getElementById("productId").value = p.id;
            document.getElementById("name").value = p.name;
            document.getElementById("price").value = p.price;
            document.getElementById("stock").value = p.stock ?? 0;
            document.getElementById("description").value = p.description ?? "";
            document.getElementById("categoryId").value = p.categoryId;

            const img = document.getElementById("imagePreview");
            if (p.image) {
                img.src = "/images/products/" + p.image;
                img.style.display = "block";
            } else {
                img.style.display = "none";
            }

            document.getElementById("imageFile").value = "";

            productModal.show();
        });
}

// ================= PREVIEW ẢNH =================
function previewImage(input) {
    if (input.files && input.files[0]) {
        const img = document.getElementById("imagePreview");
        img.src = URL.createObjectURL(input.files[0]);
        img.style.display = "block";
    }
}
