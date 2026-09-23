document.addEventListener("DOMContentLoaded", function () {

    const toggle = document.getElementById("themeToggle");

    if (!toggle) {
        return;
    }

    const savedTheme = localStorage.getItem("chathub-theme");

    if (savedTheme === "dark") {
        document.body.classList.add("dark-theme");
        toggle.textContent = "☀️";
    }

    toggle.addEventListener("click", function () {

        const isDark =
            document.body.classList.toggle("dark-theme");

        localStorage.setItem(
            "chathub-theme",
            isDark ? "dark" : "light"
        );

        toggle.textContent =
            isDark ? "☀️" : "🌙";
    });

});