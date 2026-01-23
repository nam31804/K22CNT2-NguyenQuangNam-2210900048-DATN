let promotionModal;

document.addEventListener("DOMContentLoaded", () => {
    promotionModal = new bootstrap.Modal(
        document.getElementById("promotionModal")
    );
});

// ================= THÊM =================
function openCreateModal() {
    const form = document.getElementById("promotionForm");

    // ✅ reset CHỈ KHI THÊM
    form.reset();
    form.action = "/Admin/ProductPromotion/Create";

    document.getElementById("promotionId").value = "";
    document.getElementById("modalTitle").innerText = "Thêm bài viết";

    promotionModal.show();
}

// ================= SỬA (KHÔNG RESET) =================
function openEditModal(id) {
    fetch(`/Admin/ProductPromotion/GetPromotion?id=${id}`)
        .then(res => res.json())
        .then(p => {

            const form = document.getElementById("promotionForm");
            form.action = "/Admin/ProductPromotion/Edit";

            // ❌ TUYỆT ĐỐI KHÔNG reset ở đây

            document.getElementById("modalTitle").innerText = "Cập nhật bài viết";

            document.getElementById("promotionId").value = p.id;
            document.getElementById("productId").value = p.productId;
            document.getElementById("title").value = p.title;
            document.getElementById("content").value = p.content; // ✅ SẼ HIỆN

            promotionModal.show();
        });
}


// ================= TOGGLE =================
function togglePromotion(id, btn) {
    fetch('/Admin/ProductPromotion/Toggle', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'id=' + id
    })
        .then(res => res.json())
        .then(data => {
            if (!data.success) return;

            btn.classList.toggle('btn-success');
            btn.classList.toggle('btn-secondary');
            btn.innerText = data.isActive ? 'Bật' : 'Tắt';
        });
}

// ================= UPDATE POSITION (DỒN ĐÚNG) =================
function updatePosition(id, value) {
    fetch('/Admin/ProductPromotion/UpdatePosition', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `id=${id}&position=${value}`
    })
        .then(res => res.json())
        .then(ok => {
            if (ok) location.reload();
        });
}
