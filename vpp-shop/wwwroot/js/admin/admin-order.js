let currentStatus = 'ALL';

function getToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]').value;
}

function setStatus(s) {
    currentStatus = s;
    filterOrders();
}

function filterOrders() {
    const key = document.getElementById('orderSearch').value.toLowerCase();
    document.querySelectorAll('tbody tr').forEach(r => {
        const okStatus = currentStatus === 'ALL' || r.dataset.status === currentStatus;
        const okSearch = key === '' || r.dataset.search.toLowerCase().includes(key);
        r.style.display = okStatus && okSearch ? '' : 'none';
    });
}

function updateStatus(orderId) {
    const select = document.getElementById(`status-${orderId}`);
    const status = select.value;
    const token = getToken();

    fetch('/Admin/Order/UpdateStatus', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `id=${orderId}&status=${status}&__RequestVerificationToken=${token}`
    })
        .then(r => r.json())
        .then(d => {
            if (!d.success) {
                alert(d.message || 'Không thể cập nhật đơn hàng');
                return;
            }

            const badge = document.getElementById(`status-badge-${orderId}`);
            const row = document.getElementById(`row-${orderId}`);

            // ✅ đổi text
            badge.innerText = d.status;

            // ✅ xoá class cũ
            badge.className = 'order-status';

            // ✅ thêm class mới theo trạng thái
            badge.classList.add(d.status.toLowerCase());

            // ✅ cập nhật để filter hoạt động đúng
            row.dataset.status = d.status;

            // 🔒 nếu đơn kết thúc → khoá luôn
            if (d.status === 'COMPLETED' || d.status === 'CANCELLED') {
                select.disabled = true;
                document.getElementById(`btn-update-${orderId}`).disabled = true;
            }
        })
        .catch(err => {
            console.error(err);
            alert('Có lỗi xảy ra');
        });
}



function deleteOrder(id) {
    if (!confirm('Xoá đơn này?')) return;

    fetch('/Admin/Order/Delete', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `id=${id}&__RequestVerificationToken=${getToken()}`
    })
        .then(r => r.json())
        .then(d => {
            if (d.success) document.getElementById('row-' + id).remove();
        });
}
