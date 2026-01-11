<script>
document.addEventListener("DOMContentLoaded", () => {

    let selectedFiles = [];
    let uploadModal = null;

    const dropZone = document.getElementById("dropZone");
    const fileInput = document.getElementById("imageInput");
    const preview = document.getElementById("preview");
    const productIdInput = document.getElementById("productIdInput");
    const modalEl = document.getElementById("uploadModal");

    // Khởi tạo modal 1 lần
    uploadModal = new bootstrap.Modal(modalEl);

    // ===== MỞ MODAL =====
    window.openUploadModal = function (productId) {
        productIdInput.value = productId;
        selectedFiles = [];
        preview.innerHTML = "";
        fileInput.value = "";
        uploadModal.show();
    };

    // ===== CLICK CHỌN FILE =====
    dropZone.addEventListener("click", () => fileInput.click());

    // ===== DRAG DROP =====
    dropZone.addEventListener("dragover", e => {
        e.preventDefault();
        dropZone.classList.add("bg-light");
    });

    dropZone.addEventListener("dragleave", () => {
        dropZone.classList.remove("bg-light");
    });

    dropZone.addEventListener("drop", e => {
        e.preventDefault();
        dropZone.classList.remove("bg-light");
        handleFiles(e.dataTransfer.files);
    });

    fileInput.addEventListener("change", () => {
        handleFiles(fileInput.files);
    });

    function handleFiles(files) {
        for (let f of files) {
            selectedFiles.push(f);

            const img = document.createElement("img");
            img.src = URL.createObjectURL(f);
            img.style.height = "80px";
            img.className = "border rounded";

            preview.appendChild(img);
        }
    }

    // ===== UPLOAD =====
    window.submitImages = function () {
        if (selectedFiles.length === 0) return;

        const fd = new FormData();
        fd.append("productId", productIdInput.value);
        selectedFiles.forEach(f => fd.append("images", f));

        fetch("/Admin/ProductImage/AddMultiple", {
            method: "POST",
            body: fd
        })
        .then(res => {
            if (res.ok) location.reload();
            else alert("Upload lỗi");
        });
    };

    // ===== XOÁ ẢNH =====
    window.deleteImage = function (id) {
        fetch("/Admin/ProductImage/Delete?id=" + id, {
            method: "POST"
        }).then(() => location.reload());
    };

});
</script>
