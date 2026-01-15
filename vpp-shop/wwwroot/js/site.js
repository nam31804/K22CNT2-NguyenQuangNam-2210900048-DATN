// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {

    const input = document.getElementById("searchInput");
    const suggestBox = document.getElementById("searchSuggest");

    // Nếu trang không có search → thoát
    if (!input || !suggestBox) return;

    input.addEventListener("keyup", function () {
        const keyword = this.value.trim();

        if (keyword.length === 0) {
            suggestBox.style.display = "none";
            suggestBox.innerHTML = "";
            return;
        }

        fetch(`/Product/SearchSuggest?q=${encodeURIComponent(keyword)}`)
            .then(res => res.json())
            .then(data => {

                if (data.length === 0) {
                    suggestBox.style.display = "none";
                    return;
                }

                let html = "";
                data.forEach(item => {
                    html += `
                     <a href="/Product/Detail/${item.id}" class="suggest-item">
                        <img src="/images/products/${item.image}" alt="${item.name}" />
                        <div class="suggest-info">
                        <div class="suggest-name">${item.name}</div>
                        <div class="suggest-price">${item.price.toLocaleString()} đ</div>
                        </div>
                    </a>
                    `;
                });
                html += `
                <a href="/Product/Index?keyword=${encodeURIComponent(keyword)}"
                   class="suggest-view-all">
                    Xem tất cả kết quả
                </a>
            `;

                suggestBox.innerHTML = html;
                suggestBox.style.display = "block";
            });
    });

    // Click ra ngoài thì ẩn gợi ý
    document.addEventListener("click", function (e) {
        if (!e.target.closest(".search-box")) {
            suggestBox.style.display = "none";
        }
    });

});