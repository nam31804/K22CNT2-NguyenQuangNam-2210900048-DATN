function openAddModal() {
    const today = new Date().toISOString().slice(0, 10);

    document.querySelector('#addModal input[name="StartDate"]').value = today;
    document.querySelector('#addModal input[name="EndDate"]').value = today;

    new bootstrap.Modal(document.getElementById('addModal')).show();
}

// ===== MỞ MODAL SỬA =====
function openEditModal(id, code, type, value, min, limit, start, end) {
    document.getElementById('editId').value = id;
    document.getElementById('editCode').value = code;
    document.getElementById('editType').value = type;
    document.getElementById('editValue').value = value ? parseFloat(value) : '';
    document.getElementById('editMin').value = min ? parseFloat(min) : '';
    document.getElementById('editLimit').value = limit ? parseInt(limit) : '';
    // start & end đã được truyền sẵn yyyy-MM-dd từ Razor
    document.getElementById('editStart').value = start || "";
    document.getElementById('editEnd').value = end || "";

    new bootstrap.Modal(document.getElementById('editModal')).show();
}

// ===== TÌM KIẾM THEO MÃ =====
function filterVoucher() {
    const keyword = document.getElementById('searchCode').value.toLowerCase();

    document.querySelectorAll('.voucher-row').forEach(row => {
        const code = row.dataset.code;
        const show = !keyword || code.includes(keyword);
        row.style.display = show ? '' : 'none';
    });
}