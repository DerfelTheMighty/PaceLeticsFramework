#!/usr/bin/env python3
"""Generate a conceptual adaptation-timeline diagram for PaceLetics.

The plotted curves are not fitted to individual measurement data. They are a
didactic synthesis of the literature on different adaptation time courses:
neural and cardiovascular responses tend to appear early, skeletal muscle
continues over weeks, while tendon and bone capacity require more gradual
mechanical exposure.
"""

from __future__ import annotations

import argparse
import html
import math
from pathlib import Path
from textwrap import wrap


WIDTH = 1600
HEIGHT = 920
PLOT_LEFT = 150
PLOT_TOP = 150
PLOT_RIGHT = 1470
PLOT_BOTTOM = 690
X_MAX_WEEKS = 52


SYSTEMS = [
    {
        "name": "Nervous system",
        "short": "neural control",
        "color": "#7c3aed",
        "tau": 2.6,
        "cap": 0.84,
        "note": "coordination / recruitment",
        "label_week": 5,
        "label_dx": 28,
        "label_dy": -78,
    },
    {
        "name": "Cardiovascular",
        "short": "heart + blood flow",
        "color": "#0891b2",
        "tau": 5.5,
        "cap": 0.88,
        "note": "VO2max / delivery",
        "label_week": 9,
        "label_dx": 60,
        "label_dy": 16,
    },
    {
        "name": "Skeletal muscle",
        "short": "muscle metabolism",
        "color": "#16a34a",
        "tau": 10.0,
        "cap": 0.90,
        "note": "mitochondria / strength",
        "label_week": 16,
        "label_dx": 34,
        "label_dy": -64,
    },
    {
        "name": "Tendon",
        "short": "connective tissue",
        "color": "#d97706",
        "tau": 19.0,
        "cap": 0.82,
        "note": "stiffness / collagen",
        "label_week": 28,
        "label_dx": 18,
        "label_dy": -24,
    },
    {
        "name": "Bone",
        "short": "skeletal structure",
        "color": "#dc2626",
        "tau": 31.0,
        "cap": 0.76,
        "note": "fatigue resistance",
        "label_week": 42,
        "label_dx": 24,
        "label_dy": -24,
    },
]


def adaptation(week: float, tau: float, cap: float) -> float:
    """Saturating didactic adaptation curve."""
    return cap * (1.0 - math.exp(-week / tau))


def x_to_px(week: float) -> float:
    return PLOT_LEFT + (week / X_MAX_WEEKS) * (PLOT_RIGHT - PLOT_LEFT)


def y_to_px(value: float) -> float:
    return PLOT_BOTTOM - value * (PLOT_BOTTOM - PLOT_TOP)


def svg_text(
    text: str,
    x: float,
    y: float,
    *,
    size: int = 24,
    weight: int | str = 400,
    anchor: str = "start",
    fill: str = "#17202a",
    extra: str = "",
) -> str:
    return (
        f'<text x="{x:.1f}" y="{y:.1f}" text-anchor="{anchor}" '
        f'font-family="Inter, Segoe UI, Arial, sans-serif" font-size="{size}" '
        f'font-weight="{weight}" fill="{fill}" {extra}>{html.escape(text)}</text>'
    )


def wrapped_text(
    text: str,
    x: float,
    y: float,
    *,
    width_chars: int = 72,
    line_height: int = 24,
    size: int = 19,
    fill: str = "#475569",
) -> str:
    lines = wrap(text, width_chars)
    return "\n".join(
        svg_text(line, x, y + index * line_height, size=size, fill=fill)
        for index, line in enumerate(lines)
    )


def path(points: list[tuple[float, float]]) -> str:
    first, *rest = points
    return " ".join([f"M {first[0]:.1f} {first[1]:.1f}", *[f"L {x:.1f} {y:.1f}" for x, y in rest]])


def legend_item(x: float, y: float, name: str, note: str, color: str) -> str:
    return "\n".join(
        [
            f'<rect x="{x:.1f}" y="{y - 19:.1f}" width="18" height="18" rx="5" fill="{color}"/>',
            svg_text(name, x + 29, y - 3, size=20, weight=760, fill="#1e293b"),
            svg_text(note, x + 29, y + 22, size=17, fill="#64748b"),
        ]
    )


def callout_box() -> str:
    x = 1010
    y = 186
    w = 390
    h = 138
    title = "Why pace helps"
    body = (
        "Heart and breathing can feel ready before tendons and bone have adapted. "
        "Pace gives an external target for repeated mechanical loading."
    )
    return "\n".join(
        [
            f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="10" fill="#ffffff" stroke="#dbe4ef" stroke-width="2"/>',
            f'<rect x="{x}" y="{y}" width="8" height="{h}" rx="4" fill="#0ea5e9"/>',
            svg_text(title, x + 24, y + 36, size=22, weight=800, fill="#0f172a"),
            wrapped_text(body, x + 24, y + 68, width_chars=39, line_height=22, size=18),
        ]
    )


def generate_svg() -> str:
    samples = [i * X_MAX_WEEKS / 220 for i in range(221)]

    parts: list[str] = [
        '<?xml version="1.0" encoding="UTF-8"?>',
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{WIDTH}" height="{HEIGHT}" viewBox="0 0 {WIDTH} {HEIGHT}" role="img" aria-labelledby="title desc">',
        '<title id="title">Adaptation rates of training systems</title>',
        '<desc id="desc">Conceptual adaptation curves for neural, cardiovascular, muscle, tendon, and bone systems over 52 weeks.</desc>',
        '<rect width="1600" height="920" fill="#f8fafc"/>',
        svg_text("Adaptation Rates: Why Mechanical Load Needs Pace Control", 76, 62, size=35, weight=850, fill="#102033"),
        svg_text("Conceptual synthesis across training literature; curves show relative timing, not diagnostic thresholds.", 78, 96, size=19, fill="#64748b"),
        f'<rect x="{PLOT_LEFT}" y="{PLOT_TOP}" width="{PLOT_RIGHT - PLOT_LEFT}" height="{PLOT_BOTTOM - PLOT_TOP}" rx="12" fill="#ffffff" stroke="#d9e2ec" stroke-width="2"/>',
    ]

    # Background phases.
    phases = [
        (0, 4, "#f3e8ff", "early"),
        (4, 12, "#ecfeff", "weeks"),
        (12, 28, "#f0fdf4", "months"),
        (28, 52, "#fff7ed", "longer horizon"),
    ]
    for start, end, color, label in phases:
        x1 = x_to_px(start)
        x2 = x_to_px(end)
        parts.append(f'<rect x="{x1:.1f}" y="{PLOT_TOP}" width="{x2 - x1:.1f}" height="{PLOT_BOTTOM - PLOT_TOP}" fill="{color}" opacity="0.62"/>')
        parts.append(svg_text(label, (x1 + x2) / 2, PLOT_TOP + 58, size=18, weight=720, anchor="middle", fill="#64748b"))

    # Grid and axes.
    for week in [0, 4, 8, 12, 26, 39, 52]:
        x = x_to_px(week)
        parts.append(f'<line x1="{x:.1f}" y1="{PLOT_TOP}" x2="{x:.1f}" y2="{PLOT_BOTTOM}" stroke="#e2e8f0" stroke-width="2"/>')
        label = f"{week}w" if week else "start"
        parts.append(svg_text(label, x, PLOT_BOTTOM + 39, size=18, anchor="middle", fill="#64748b"))
    for frac in [0.25, 0.5, 0.75, 1.0]:
        y = y_to_px(frac)
        parts.append(f'<line x1="{PLOT_LEFT}" y1="{y:.1f}" x2="{PLOT_RIGHT}" y2="{y:.1f}" stroke="#e8eef5" stroke-width="2"/>')

    parts.extend(
        [
            f'<line x1="{PLOT_LEFT}" y1="{PLOT_BOTTOM}" x2="{PLOT_RIGHT}" y2="{PLOT_BOTTOM}" stroke="#243447" stroke-width="3"/>',
            f'<line x1="{PLOT_LEFT}" y1="{PLOT_TOP}" x2="{PLOT_LEFT}" y2="{PLOT_BOTTOM}" stroke="#243447" stroke-width="3"/>',
            svg_text("Training time", (PLOT_LEFT + PLOT_RIGHT) / 2, 884, size=24, weight=760, anchor="middle", fill="#1f2937"),
            svg_text("relative adaptation", 43, 435, size=23, weight=760, anchor="middle", fill="#1f2937", extra='transform="rotate(-90 43 435)"'),
        ]
    )

    # Risk window between fast physiology and slow tissues.
    risk_x1 = x_to_px(4)
    risk_x2 = x_to_px(20)
    risk_y = y_to_px(0.93)
    parts.extend(
        [
            f'<path d="M {risk_x1:.1f} {risk_y:.1f} C {x_to_px(8):.1f} {risk_y - 34:.1f}, {x_to_px(15):.1f} {risk_y - 34:.1f}, {risk_x2:.1f} {risk_y:.1f}" fill="none" stroke="#ef4444" stroke-width="3" stroke-dasharray="9 8"/>',
            svg_text("mismatch window", (risk_x1 + risk_x2) / 2, risk_y - 46, size=20, weight=800, anchor="middle", fill="#b91c1c"),
        ]
    )

    # Curves.
    for system in SYSTEMS:
        points = [
            (x_to_px(week), y_to_px(adaptation(week, system["tau"], system["cap"])))
            for week in samples
        ]
        parts.append(
            f'<path d="{path(points)}" fill="none" stroke="{system["color"]}" stroke-width="7" stroke-linecap="round" stroke-linejoin="round"/>'
        )

    # Labels on curves.
    for system in SYSTEMS:
        week = system["label_week"]
        val = adaptation(week, system["tau"], system["cap"])
        x = x_to_px(week)
        y = y_to_px(val)
        label_x = x + system["label_dx"]
        label_y = y + system["label_dy"]
        parts.extend(
            [
                f'<circle cx="{x:.1f}" cy="{y:.1f}" r="8" fill="{system["color"]}" stroke="#ffffff" stroke-width="4"/>',
                f'<line x1="{x:.1f}" y1="{y:.1f}" x2="{label_x - 8:.1f}" y2="{label_y + 22:.1f}" stroke="{system["color"]}" stroke-width="2" opacity="0.45"/>',
                f'<rect x="{label_x:.1f}" y="{label_y:.1f}" width="248" height="54" rx="8" fill="#ffffff" opacity="0.96" stroke="#e2e8f0" stroke-width="2"/>',
                svg_text(system["name"], label_x + 15, label_y + 22, size=18, weight=820, fill=system["color"]),
                svg_text(system["note"], label_x + 15, label_y + 44, size=15, fill="#64748b"),
            ]
        )

    parts.append(callout_box())

    # Legend / source note.
    legend_y = 750
    legend_xs = [105, 410, 705, 1000, 1278]
    for x, system in zip(legend_xs, SYSTEMS, strict=True):
        parts.append(legend_item(x, legend_y, system["short"], system["name"], system["color"]))

    footnote = (
        "Evidence anchors: MacInnis & Gibala 2017; Hawley et al. 2018; "
        "Kubo et al. 2010; Bohm et al. 2019; Warden et al. 2021."
    )
    parts.append(svg_text(footnote, 80, 842, size=16, fill="#64748b"))
    parts.append("</svg>")
    return "\n".join(parts)


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate the PaceLetics adaptation-timeline diagram.")
    parser.add_argument(
        "--output",
        type=Path,
        default=Path("PaceLetics.Web/wwwroot/images/diagrams/adaptation-timeline.svg"),
        help="Output SVG path.",
    )
    args = parser.parse_args()
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(generate_svg(), encoding="utf-8")
    print(f"Wrote {args.output}")


if __name__ == "__main__":
    main()
