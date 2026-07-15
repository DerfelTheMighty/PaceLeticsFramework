

function scrollToTop() {
    window.scrollTo(0, 0);
}

document.addEventListener("click", event => {
    const toggle = event.target.closest("[data-password-toggle]");
    if (!toggle) return;

    const input = document.getElementById(toggle.dataset.passwordToggle);
    if (!input) return;

    const shouldShow = input.type === "password";
    input.type = shouldShow ? "text" : "password";
    toggle.setAttribute("aria-pressed", shouldShow ? "true" : "false");
    input.focus({ preventScroll: true });
});


window.PlayDing_1 = () => {
    const el = document.getElementById("ding1");
    if (!el) return;              // <-- wichtig
    el.currentTime = 0;
    const p = el.play();
    if (p && typeof p.catch === "function") p.catch(() => { });
};

window.PlayDing_3 = () => {
    const el = document.getElementById("ding3");
    if (!el) return;
    el.currentTime = 0;
    const p = el.play();
    if (p && typeof p.catch === "function") p.catch(() => { });
};

window.PlayTimer = () => {
    const el = document.getElementById("timer");
    if (!el) return;
    el.currentTime = 0;
    const p = el.play();
    if (p && typeof p.catch === "function") p.catch(() => { });
};


window.scrollToNextSession = () => {
    const el = document.getElementById("next-session");
if (el) {
    el.scrollIntoView({
        behavior: "smooth",
        block: "center"
    });
    }
};

window.paceleticsAcademyInfo = (() => {
    const key = "paceletics.academyInfo.enabled";

    function isEnabled() {
        try {
            return window.localStorage.getItem(key) !== "false";
        } catch {
            return true;
        }
    }

    function setEnabled(enabled) {
        try {
            window.localStorage.setItem(key, enabled ? "true" : "false");
        } catch {
        }

        window.dispatchEvent(new CustomEvent("paceletics-academy-info-changed", {
            detail: { enabled }
        }));
    }

    return {
        isEnabled,
        setEnabled,
        disable: () => setEnabled(false)
    };
})();

window.paceleticsAcademy = (() => {
    function enhanceTables(selector) {
        const root = selector ? document.querySelector(selector) : document;
        if (!root) return;

        root.querySelectorAll("table").forEach(table => {
            const headers = Array.from(table.querySelectorAll("thead th"))
                .map(header => header.textContent.trim())
                .filter(Boolean);

            if (headers.length === 0) return;

            table.querySelectorAll("tbody tr").forEach(row => {
                Array.from(row.children).forEach((cell, index) => {
                    if (cell.tagName !== "TD") return;

                    const label = headers[index];
                    if (label) {
                        cell.setAttribute("data-label", label);
                    }
                });
            });

            table.classList.add("pl-academy-table--enhanced");
        });
    }

    return {
        enhanceTables
    };
})();
