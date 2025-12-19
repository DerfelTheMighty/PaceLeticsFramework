

function scrollToTop() {
    window.scrollTo(0, 0);
}


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
