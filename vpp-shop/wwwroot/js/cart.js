// =========================
// UPDATE GRAND TOTAL
// =========================
function updateGrandTotal() {
    let total = 0;

    document.querySelectorAll('.item-check:checked').forEach(cb => {
        const id = cb.dataset.id;
        const price = parseFloat(cb.dataset.price);
        const qty = parseInt(document.getElementById('qty-' + id).innerText);
        total += price * qty;
    });

    document.getElementById('grand-total').innerText =
        total.toLocaleString();
}

// =========================
// INCREASE
// =========================
function increase(id) {
    fetch('/Cart/Increase', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'id=' + id
    })
        .then(res => res.json())
        .then(data => {
            document.getElementById('qty-' + id).innerText = data.quantity;
            document.getElementById('total-' + id).innerText =
                data.itemTotal.toLocaleString();
            updateGrandTotal();
        });
}

// =========================
// DECREASE
// =========================
function decrease(id) {
    fetch('/Cart/Decrease', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'id=' + id
    })
        .then(res => res.json())
        .then(data => {
            document.getElementById('qty-' + id).innerText = data.quantity;
            document.getElementById('total-' + id).innerText =
                data.itemTotal.toLocaleString();
            updateGrandTotal();
        });
}

// =========================
// REMOVE
// =========================
function removeItem(id) {
    if (!confirm('Xóa sản phẩm này?')) return;

    fetch('/Cart/Remove', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'id=' + id
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                document.getElementById('row-' + id).remove();
                updateGrandTotal();
            }
        });
}

// =========================
// CHECKOUT
// =========================
function checkout() {
    const checked = document.querySelectorAll('.item-check:checked');
    if (checked.length === 0) {
        alert('Vui lòng chọn sản phẩm');
        return;
    }

    let ids = [];
    checked.forEach(i => ids.push(i.dataset.id));
    window.location.href = '/Checkout/Index?itemIds=' + ids.join(',');
}
