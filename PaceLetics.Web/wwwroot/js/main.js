

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

window.paceleticsStorage = {
    get: key => {
        try { return window.localStorage.getItem(key); } catch { return null; }
    },
    set: (key, value) => {
        try { window.localStorage.setItem(key, value); } catch { }
    }
};

window.paceleticsTrainingContext = (() => {
    function getPosition() {
        return new Promise((resolve, reject) => {
            if (!navigator.geolocation) {
                reject(new Error("geolocation-unavailable"));
                return;
            }
            navigator.geolocation.getCurrentPosition(resolve, reject, {
                enableHighAccuracy: false,
                timeout: 10000,
                maximumAge: 30 * 60 * 1000
            });
        });
    }

    async function load(timestamp) {
        const requested = new Date(timestamp);
        const date = timestamp.slice(0, 10);
        const days = Math.ceil((requested.getTime() - Date.now()) / 86400000);
        if (!Number.isFinite(days) || days < -1 || days > 15) return null;

        const position = await getPosition();
        const latitude = position.coords.latitude;
        const longitude = position.coords.longitude;
        const query = new URLSearchParams({
            latitude: latitude.toString(),
            longitude: longitude.toString(),
            hourly: "temperature_2m,weather_code",
            daily: "sunrise,sunset",
            timezone: "auto",
            start_date: date,
            end_date: date
        });
        const response = await fetch(`https://api.open-meteo.com/v1/forecast?${query}`);
        if (!response.ok) throw new Error("weather-unavailable");
        const data = await response.json();
        const targetHour = requested.getHours();
        let hourlyIndex = (data.hourly?.time || []).findIndex(value => new Date(value).getHours() >= targetHour);
        if (hourlyIndex < 0) hourlyIndex = 0;
        const sunrise = data.daily?.sunrise?.[0] ?? null;
        const sunset = data.daily?.sunset?.[0] ?? null;
        const target = requested.getTime();
        return {
            temperatureC: data.hourly?.temperature_2m?.[hourlyIndex] ?? null,
            weatherCode: data.hourly?.weather_code?.[hourlyIndex] ?? 0,
            sunrise,
            sunset,
            isDaylight: sunrise && sunset ? target >= new Date(sunrise).getTime() && target <= new Date(sunset).getTime() : true
        };
    }

    return { load };
})();
