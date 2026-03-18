document.addEventListener("DOMContentLoaded", () => {

    /* ========= UPLOAD ẢNH ========= */
    let selectedFiles = [];
    const preview = document.getElementById("preview");
    const productIdInput = document.getElementById("productIdInput");
    const dropZone = document.getElementById("dropZone");
    const fileInput = document.getElementById("imageInput");

    if (dropZone) {

        window.setProductId = function (id) {
            productIdInput.value = id;
            selectedFiles = [];
            preview.innerHTML = "";
            fileInput.value = "";
        };

        window.submitImages = function () {
            if (selectedFiles.length === 0) return;

            const fd = new FormData();
            fd.append("productId", productIdInput.value);
            selectedFiles.forEach(f => fd.append("images", f));

            fetch("/Admin/ProductImage/AddMultiple", {
                method: "POST",
                body: fd
            }).then(() => location.reload());
        };

        window.deleteImage = function (id) {
            fetch("/Admin/ProductImage/Delete?id=" + id, {
                method: "POST"
            }).then(() => location.reload());
        };

        dropZone.onclick = () => fileInput.click();

        fileInput.onchange = () => {
            for (let f of fileInput.files) {
                selectedFiles.push(f);
                const img = document.createElement("img");
                img.src = URL.createObjectURL(f);
                img.style.height = "80px";
                img.className = "border rounded";
                preview.appendChild(img);
            }
        };
    }

    /* ========= LIVE SEARCH ========= */
    const searchInput = document.getElementById("searchInput");
    let timer = null;

    if (searchInput) {
        searchInput.addEventListener("input", function () {
            const keyword = this.value;

            clearTimeout(timer);
            timer = setTimeout(() => {
                fetch(`/Admin/ProductImage?keyword=${encodeURIComponent(keyword)}`)
                    .then(res => res.text())
                    .then(html => {
                        const doc = new DOMParser().parseFromString(html, "text/html");
                        const newTable = doc.querySelector("#tableWrap").innerHTML;
                        document.getElementById("tableWrap").innerHTML = newTable;
                    });
            }, 100); 
        });
    }

});
