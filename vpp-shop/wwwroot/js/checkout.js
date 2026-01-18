document.addEventListener('DOMContentLoaded', function () {

    const citySelect = document.getElementById('city');
    const districtSelect = document.getElementById('district');
    if (!citySelect || !districtSelect) return;

    fetch('/js/vn-location.json')
        .then(res => res.json())
        .then(data => {

            // Load tỉnh
            data.forEach(city => {
                const opt = document.createElement('option');
                opt.value = city.Name;
                opt.textContent = city.Name;
                opt.dataset.districts = JSON.stringify(city.Districts);
                citySelect.appendChild(opt);
            });

            // Khi chọn tỉnh → load huyện
            citySelect.addEventListener('change', function () {
                districtSelect.innerHTML =
                    '<option value="">Quận / Huyện</option>';

                const selected = this.options[this.selectedIndex];
                if (!selected.dataset.districts) return;

                const districts = JSON.parse(selected.dataset.districts);
                districts.forEach(d => {
                    const opt = document.createElement('option');
                    opt.value = d.Name;
                    opt.textContent = d.Name;
                    districtSelect.appendChild(opt);
                });
            });
        });
});
let originalTotal = null;
let voucherApplied = false;

function applyVoucher() {
    const code = document.getElementById("voucherCode").value.trim();
    const msg = document.getElementById("voucher-msg");

    if (code === "") {
        msg.innerHTML = "<span class='text-danger'>Vui lòng nhập mã</span>";
        return;
    }

    // 🚫 Không cho áp nhiều lần
    if (voucherApplied) {
        msg.innerHTML = "<span class='text-warning'>Voucher đã được áp dụng</span>";
        return;
    }

    // Lưu tổng tiền gốc lần đầu
    if (originalTotal === null) {
        originalTotal = parseInt(
            document.getElementById("totalMoney").innerText.replace(/\D/g, "")
        );
    }

    fetch("/Voucher/Apply", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            code: code,
            total: originalTotal
        })
    })
        .then(res => res.json())
        .then(data => {
            if (!data.success) {
                msg.innerHTML = `<span class="text-danger">${data.message}</span>`;
                return;
            }

            const newTotal = originalTotal - data.discount;

            document.getElementById("totalMoney").innerText =
                newTotal.toLocaleString();

            document.getElementById("VoucherCode").value = code;
            document.getElementById("DiscountAmount").value = data.discount;

            msg.innerHTML = `<span class="text-success">
            Áp dụng thành công (-${data.discount.toLocaleString()} đ)
        </span>`;

            document.querySelector(".checkout-voucher button").disabled = true;
 
        });
}
