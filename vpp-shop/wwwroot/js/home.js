document.addEventListener("DOMContentLoaded", () => {
    const slides = document.querySelectorAll(".banner-slide");
    const dots = document.querySelectorAll(".dot");
    let index = 0;

    setInterval(() => {
        slides[index].classList.remove("active");
        dots[index].classList.remove("active");

        index = (index + 1) % slides.length;

        slides[index].classList.add("active");
        dots[index].classList.add("active");
    }, 4000);
});
