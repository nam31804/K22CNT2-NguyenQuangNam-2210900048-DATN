document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll(".podium-item").forEach((item, i) => {
        item.style.opacity = 0;
        setTimeout(() => {
            item.style.transition = "0.5s";
            item.style.opacity = 1;
        }, i * 200);
    });
});
