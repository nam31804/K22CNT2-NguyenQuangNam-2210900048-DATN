// Load danh sách staff
function loadStaff() {
    $("#staffTable").load("/Admin/Staff/List");
}

// Hiện / ẩn form tạo staff
function toggleCreate() {
    $("#createBox").slideToggle();
}

// Tạo staff
function createStaff() {
    $.post("/Admin/Staff/Create", {
        FullName: $("#fullName").val(),
        Username: $("#username").val(),
        Password: $("#password").val()
    })
        .done(() => {
            $("#fullName,#username,#password").val("");
            $("#createBox").slideUp();
            loadStaff();
        })
        .fail(res => alert(res.responseText));
}

// Khoá / mở staff
function toggleStaff(id) {
    $.post("/Admin/Staff/Toggle", { id })
        .done(() => loadStaff());
}

// HIỆN / ẨN MẬT KHẨU (FIX BẤM 1 LẦN)
function togglePassword(id) {
    const el = document.getElementById("pw-" + id);
    const realPw = el.getAttribute("data-pw");
    const isShow = el.getAttribute("data-show") === "1";

    if (!isShow) {
        el.innerText = realPw;
        el.setAttribute("data-show", "1");
    } else {
        el.innerText = "••••••";
        el.setAttribute("data-show", "0");
    }
}

// Load khi mở trang
$(function () {
    loadStaff();
});
