let categoryModal;
let categoryForm;

document.addEventListener("DOMContentLoaded", function () {
    categoryModal = new bootstrap.Modal(document.getElementById('categoryModal'));
    categoryForm = document.getElementById('categoryForm');

    categoryForm.addEventListener('submit', saveCategory);
});

// =======================
// OPEN CREATE
// =======================
function openCreateCategory() {
    document.getElementById('categoryModalTitle').innerText = 'Thêm thể loại';

    document.getElementById('catId').value = '';
    document.getElementById('catName').value = '';
    document.getElementById('catDesc').value = '';
    document.getElementById('catGroup').value = '';

    document.getElementById('categoryProductsBox').classList.add('d-none');

    categoryModal.show();
}

// =======================
// OPEN EDIT
// =======================
async function openEditCategory(id) {
    const res = await fetch(`/Admin/Category/Get?id=${id}`);
    if (!res.ok) {
        alert('Không tải được dữ liệu');
        return;
    }

    const data = await res.json();

    document.getElementById('categoryModalTitle').innerText = 'Sửa thể loại';

    document.getElementById('catId').value = data.id;
    document.getElementById('catName').value = data.name;
    document.getElementById('catDesc').value = data.description ?? '';
    document.getElementById('catGroup').value = data.categoryGroupId;

    const productBox = document.getElementById('categoryProductsBox');
    const productList = document.getElementById('categoryProducts');

    productList.innerHTML = '';

    if (data.products && data.products.length > 0) {
        data.products.forEach(p => {
            const li = document.createElement('li');
            li.className = 'list-group-item';
            li.innerText = p.name;
            productList.appendChild(li);
        });
        productBox.classList.remove('d-none');
    } else {
        productBox.classList.add('d-none');
    }

    categoryModal.show();
}

// =======================
// SAVE
// =======================
async function saveCategory(e) {
    e.preventDefault();

    const formData = new FormData(categoryForm);

    const res = await fetch('/Admin/Category/Save', {
        method: 'POST',
        body: formData
    });

    if (!res.ok) {
        const msg = await res.text();
        alert(msg);
        return;
    }

    categoryModal.hide();
    await reloadCategoryTable();
}

// =======================
// DELETE
// =======================
async function deleteCategory(id) {
    if (!confirm('Bạn có chắc muốn xoá thể loại này?')) return;

    const res = await fetch('/Admin/Category/Delete', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: `id=${id}`
    });

    if (!res.ok) {
        const msg = await res.text();
        alert(msg);
        return;
    }

    await reloadCategoryTable();
}
