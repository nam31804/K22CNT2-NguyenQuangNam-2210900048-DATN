// ===============================
// LOAD TỈNH / HUYỆN
// ===============================
let cityDataLoaded = false;

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

                const selected =
                    this.options[this.selectedIndex];
                if (!selected || !selected.dataset.districts) return;

                const districts =
                    JSON.parse(selected.dataset.districts);

                districts.forEach(d => {
                    const opt = document.createElement('option');
                    opt.value = d.Name;
                    opt.textContent = d.Name;
                    districtSelect.appendChild(opt);
                });
            });

            cityDataLoaded = true; // 🔥 Đánh dấu đã load xong
        });
});


// ===============================
// AUTO FILL ĐỊA CHỈ ĐÃ LƯU
// ===============================
document.addEventListener('DOMContentLoaded', function () {

    const addressRadios =
        document.querySelectorAll('input[name="SelectedAddressId"]');

    if (!addressRadios || addressRadios.length === 0) return;

    addressRadios.forEach(radio => {
        radio.addEventListener('change', function () {

            const nameInput =
                document.querySelector('[name="ReceiverName"]');
            const phoneInput =
                document.querySelector('[name="Phone"]');
            const addressInput =
                document.querySelector('[name="Address"]');
            const citySelect =
                document.querySelector('[name="City"]');
            const districtSelect =
                document.querySelector('[name="District"]');

            // 👉 Nhập địa chỉ mới
            if (this.value === "" || this.value === null) {

                if (citySelect) citySelect.required = true;
                if (districtSelect) districtSelect.required = true;

                if (nameInput) nameInput.value = "";
                if (phoneInput) phoneInput.value = "";
                if (addressInput) addressInput.value = "";
                if (citySelect) citySelect.value = "";
                if (districtSelect) districtSelect.value = "";
                return;
            }

            // 👉 Chọn địa chỉ cũ
            if (citySelect) citySelect.required = false;
            if (districtSelect) districtSelect.required = false;

            if (nameInput) nameInput.value = this.dataset.name || "";
            if (phoneInput) phoneInput.value = this.dataset.phone || "";
            if (addressInput) addressInput.value = this.dataset.address || "";

            // Chỉ set tỉnh khi đã load xong data
            if (citySelect && cityDataLoaded) {
                citySelect.value = this.dataset.city || "";
                citySelect.dispatchEvent(new Event('change'));
            }

            // Delay để huyện load xong
            setTimeout(() => {
                if (districtSelect && cityDataLoaded) {
                    districtSelect.value = this.dataset.district || "";
                }
            }, 100);
        });
    });
});


// ===============================
// AUTO CHỌN ĐỊA CHỈ CŨ KHI LOAD TRANG
// ===============================
document.addEventListener('DOMContentLoaded', function () {

    const radios =
        document.querySelectorAll('input[name="SelectedAddressId"]');
    if (!radios || radios.length === 0) return;

    const waitForCityData = setInterval(() => {
        if (!cityDataLoaded) return;

        clearInterval(waitForCityData);

        const firstOldAddress =
            Array.from(radios).find(r => r.value !== "");

        if (firstOldAddress) {
            firstOldAddress.checked = true;
            firstOldAddress.dispatchEvent(new Event('change'));
        }
    }, 50);
});


// ===============================
// VOUCHER
// ===============================
let originalTotal = null;
let voucherApplied = false;

function applyVoucher() {

    const code =
        document.getElementById("voucherCode").value.trim();
    const msg =
        document.getElementById("voucher-msg");

    if (code === "") {
        msg.innerHTML =
            "<span class='text-danger'>Vui lòng nhập mã</span>";
        return;
    }

    if (voucherApplied) {
        msg.innerHTML =
            "<span class='text-warning'>Voucher đã được áp dụng</span>";
        return;
    }

    if (originalTotal === null) {
        originalTotal = parseInt(
            document.getElementById("totalMoney")
                .innerText.replace(/\D/g, "")
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
                msg.innerHTML =
                    `<span class="text-danger">${data.message}</span>`;
                return;
            }

            const newTotal = originalTotal - data.discount;

            document.getElementById("totalMoney").innerText =
                newTotal.toLocaleString();

            document.getElementById("VoucherCode").value = code;
            document.getElementById("DiscountAmount").value =
                data.discount;

            voucherApplied = true;

            msg.innerHTML =
                `<span class="text-success">
                    Áp dụng thành công (-${data.discount.toLocaleString()} đ)
                </span>`;

            const btn =
                document.querySelector(".voucher-box button");
            if (btn) btn.disabled = true;
        });
}
document.addEventListener('DOMContentLoaded', function () {
    const toggle = document.getElementById('toggleAddress');
    const list = document.getElementById('addressList');

    if (!toggle || !list) return;

    // đảm bảo trạng thái ban đầu
    list.classList.add('hidden');

    toggle.addEventListener('click', function () {
        list.classList.toggle('hidden');
    });
});
document.addEventListener('click', function (e) {
    const btn = e.target.closest('.delete-address');
    if (!btn) return;

    const id = btn.dataset.id;
    if (!confirm('Xoá địa chỉ này?')) return;

    fetch('/Checkout/DeleteAddress', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id: id })
    })
        .then(res => {
            if (!res.ok) throw new Error('Delete failed');
            return res.json();
        })
        .then(data => {
            if (data.success) {
                btn.closest('.address-item').remove();
            }
        })
        .catch(err => {
            console.error(err);
            alert('Không xoá được địa chỉ');
        });
});
