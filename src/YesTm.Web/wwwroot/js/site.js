// YES TM – shared client behaviour
(function () {
    "use strict";

    // Responsive sidebar toggle
    var shell = document.querySelector(".app-shell");
    var burger = document.getElementById("burger");
    var backdrop = document.getElementById("backdrop");

    function closeNav() { if (shell) shell.classList.remove("nav-open"); }

    if (burger && shell) {
        burger.addEventListener("click", function () { shell.classList.toggle("nav-open"); });
    }
    if (backdrop) backdrop.addEventListener("click", closeNav);

    // Close the mobile nav when a real link is clicked
    document.querySelectorAll(".sidebar-nav a.nav-link:not(.soon)").forEach(function (a) {
        a.addEventListener("click", function () {
            if (window.matchMedia("(max-width: 992px)").matches) closeNav();
        });
    });
})();
