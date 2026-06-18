#!/usr/bin/env python3
"""Generate a didactic mechanical-load-vs-pace diagram for PaceLetics.

The curves are a conceptual synthesis, not a direct fit to one measurement
dataset. They visualize the rationale supported by the existing PaceLetics
sources: running speed is a practical external proxy for mechanical exposure,
while impact-related forces, loading rates, and muscle-tendon demand tend to
increase as pace gets faster.
"""

from __future__ import annotations

import argparse
import html
import math
from pathlib import Path
from textwrap import wrap


WIDTH = 1600
HEIGHT = 900
PLOT_LEFT = 150
PLOT_TOP = 145
PLOT_RIGHT = 1470
PLOT_BOTTOM = 685
PACE_MIN = 7.0
PACE_MAX = 3.0


def speed_from_pace(pace_min_per_km: float) -> float:
    return 1000 / (pace_min_per_km * 60)


SPEED_MIN = speed_from_pace(PACE_MIN)
SPEED_MAX = speed_from_pace(PACE_MAX)


def normalize_speed(pace_min_per_km: float) -> float:
    speed = speed_from_pace(pace_min_per_km)
    return (speed - SPEED_MIN) / (SPEED_MAX - SPEED_MIN)


def mechanical_load(pace_min_per_km: float) -> float:
    x = max(0.0, min(1.0, normalize_speed(pace_min_per_km)))
    return 0.16 + 0.72 * x**1.45


def loading_rate(pace_min_per_km: float) -> float:
    x = max(0.0, min(1.0, normalize_speed(pace_min_per_km)))
    return 0.10 + 0.78 * x**1.95


def tendon_demand(pace_min_per_km: float) -> float:
    x = max(0.0, min(1.0, normalize_speed(pace_min_per_km)))
    return 0.14 + 0.66 * (1 - math.exp(-2.7 * x))


def x_to_px(pace_min_per_km: float) -> float:
    # Faster paces are on the right, even though min/km gets numerically lower.
    return PLOT_LEFT + (PACE_MIN - pace_min_per_km) / (PACE_MIN - PACE_MAX) * (PLOT_RIGHT - PLOT_LEFT)


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
    width_chars: int = 42,
    line_height: int = 23,
    size: int = 18,
    fill: str = "#475569",
) -> str:
    return "\n".join(
        svg_text(line, x, y + index * line_height, size=size, fill=fill)
        for index, line in enumerate(wrap(text, width_chars))
    )


def path(points: list[tuple[float, float]]) -> str:
    first, *rest = points
    return " ".join([f"M {first[0]:.1f} {first[1]:.1f}", *[f"L {x:.1f} {y:.1f}" for x, y in rest]])


def pace_label(pace_min_per_km: float) -> str:
    minutes = int(pace_min_per_km)
    seconds = int(round((pace_min_per_km - minutes) * 60))
    return f"{minutes}:{seconds:02d}/km"


def curve_label(x: float, y: float, title: str, color: str) -> str:
    return "\n".join(
        [
            f'<rect x="{x:.1f}" y="{y - 25:.1f}" width="238" height="42" rx="8" fill="#ffffff" opacity="0.95" stroke="#dbe4ef" stroke-width="2"/>',
            svg_text(title, x + 15, y + 2, size=18, weight=800, fill=color),
        ]
    )


def callout_box() -> str:
    x = 190
    y = 205
    width = 380
    height = 170
    body = (
        "Pace is not perfect, but it is an external load target: faster running "
        "usually means higher impact forces, loading rates, and elastic tissue demand."
    )
    return "\n".join(
        [
            f'<rect x="{x}" y="{y}" width="{width}" height="{height}" rx="10" fill="#ffffff" stroke="#dbe4ef" stroke-width="2"/>',
            f'<rect x="{x}" y="{y}" width="8" height="{height}" rx="4" fill="#0891b2"/>',
            svg_text("Why pace matters", x + 24, y + 39, size=24, weight=850, fill="#0f172a"),
            wrapped_text(body, x + 24, y + 76, width_chars=39, line_height=24, size=19),
        ]
    )


def generate_svg() -> str:
    samples = [PACE_MIN - i * (PACE_MIN - PACE_MAX) / 220 for i in range(221)]
    mechanical_points = [(x_to_px(pace), y_to_px(mechanical_load(pace))) for pace in samples]
    loading_points = [(x_to_px(pace), y_to_px(loading_rate(pace))) for pace in samples]
    tendon_points = [(x_to_px(pace), y_to_px(tendon_demand(pace))) for pace in samples]

    parts: list[str] = [
        '<?xml version="1.0" encoding="UTF-8"?>',
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{WIDTH}" height="{HEIGHT}" viewBox="0 0 {WIDTH} {HEIGHT}" role="img" aria-labelledby="title desc">',
        '<title id="title">Mechanical load increases with faster running pace</title>',
        '<desc id="desc">Conceptual diagram showing normalized external mechanical load rising as running pace gets faster.</desc>',
        '<rect width="1600" height="900" fill="#f8fafc"/>',
        svg_text("Mechanical Load vs Pace", 80, 62, size=36, weight=860, fill="#102033"),
        svg_text("Conceptual synthesis: faster pace increases external mechanical exposure, even when internal effort varies.", 80, 97, size=19, fill="#64748b"),
        f'<rect x="{PLOT_LEFT}" y="{PLOT_TOP}" width="{PLOT_RIGHT - PLOT_LEFT}" height="{PLOT_BOTTOM - PLOT_TOP}" rx="12" fill="#ffffff" stroke="#d9e2ec" stroke-width="2"/>',
    ]

    zones = [
        (7.0, 5.3, "#ecfeff", "easy / low load"),
        (5.3, 4.2, "#f0fdf4", "steady / moderate"),
        (4.2, 3.0, "#fff1f2", "fast / high load"),
    ]
    for slow, fast, color, label in zones:
        x1 = x_to_px(slow)
        x2 = x_to_px(fast)
        parts.append(f'<rect x="{x1:.1f}" y="{PLOT_TOP}" width="{x2 - x1:.1f}" height="{PLOT_BOTTOM - PLOT_TOP}" fill="{color}" opacity="0.72"/>')
        parts.append(svg_text(label, (x1 + x2) / 2, PLOT_TOP + 48, size=18, weight=740, anchor="middle", fill="#64748b"))

    for pace in [7.0, 6.0, 5.0, 4.0, 3.5, 3.0]:
        x = x_to_px(pace)
        parts.append(f'<line x1="{x:.1f}" y1="{PLOT_TOP}" x2="{x:.1f}" y2="{PLOT_BOTTOM}" stroke="#e2e8f0" stroke-width="2"/>')
        parts.append(svg_text(pace_label(pace), x, PLOT_BOTTOM + 39, size=18, anchor="middle", fill="#64748b"))

    for frac in [0.2, 0.4, 0.6, 0.8, 1.0]:
        y = y_to_px(frac)
        parts.append(f'<line x1="{PLOT_LEFT}" y1="{y:.1f}" x2="{PLOT_RIGHT}" y2="{y:.1f}" stroke="#edf2f7" stroke-width="2"/>')

    parts.extend(
        [
            f'<line x1="{PLOT_LEFT}" y1="{PLOT_BOTTOM}" x2="{PLOT_RIGHT}" y2="{PLOT_BOTTOM}" stroke="#243447" stroke-width="3"/>',
            f'<line x1="{PLOT_LEFT}" y1="{PLOT_TOP}" x2="{PLOT_LEFT}" y2="{PLOT_BOTTOM}" stroke="#243447" stroke-width="3"/>',
            svg_text("running pace (faster to the right)", (PLOT_LEFT + PLOT_RIGHT) / 2, 875, size=24, weight=760, anchor="middle", fill="#1f2937"),
            svg_text("normalized mechanical exposure", 43, 435, size=23, weight=760, anchor="middle", fill="#1f2937", extra='transform="rotate(-90 43 435)"'),
        ]
    )

    # Shaded area under main curve.
    area = [(x_to_px(PACE_MIN), y_to_px(0.0)), *mechanical_points, (x_to_px(PACE_MAX), y_to_px(0.0))]
    parts.append(f'<path d="{path(area)} Z" fill="#0ea5e9" opacity="0.13"/>')

    parts.extend(
        [
            f'<path d="{path(mechanical_points)}" fill="none" stroke="#0891b2" stroke-width="8" stroke-linecap="round" stroke-linejoin="round"/>',
            f'<path d="{path(loading_points)}" fill="none" stroke="#dc2626" stroke-width="5" stroke-linecap="round" stroke-linejoin="round" stroke-dasharray="12 10"/>',
            f'<path d="{path(tendon_points)}" fill="none" stroke="#d97706" stroke-width="5" stroke-linecap="round" stroke-linejoin="round" stroke-dasharray="4 10"/>',
        ]
    )

    parts.append(curve_label(x_to_px(3.78), y_to_px(mechanical_load(3.78)) - 22, "external mechanical load", "#0891b2"))
    parts.append(curve_label(x_to_px(3.55), y_to_px(loading_rate(3.55)) + 32, "loading rate sensitivity", "#dc2626"))
    parts.append(curve_label(x_to_px(4.62), y_to_px(tendon_demand(4.62)) + 46, "muscle-tendon demand", "#d97706"))
    parts.append(callout_box())

    parts.extend(
        [
            f'<rect x="1010" y="192" width="390" height="134" rx="10" fill="#ffffff" stroke="#dbe4ef" stroke-width="2"/>',
            f'<rect x="1010" y="192" width="8" height="134" rx="4" fill="#7c3aed"/>',
            svg_text("Interpretation", 1034, 230, size=23, weight=850, fill="#0f172a"),
            wrapped_text("Heart rate describes internal strain. Pace constrains how much high-speed impact is repeated.", 1034, 264, width_chars=39, line_height=23, size=18),
        ]
    )

    footnote = (
        "Evidence anchors: Jiang et al. 2024 on speed-dependent GRF changes; "
        "Papagiannaki et al. 2020 on repetitive impact mechanics; Warden et al. 2021 on workload and bone stress."
    )
    parts.append(svg_text(footnote, 80, 822, size=16, fill="#64748b"))
    parts.append("</svg>")
    return "\n".join(parts)


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate the PaceLetics mechanical-load diagram.")
    parser.add_argument(
        "--output",
        type=Path,
        default=Path("PaceLetics.Web/wwwroot/images/diagrams/mechanical-load-vs-pace.svg"),
        help="Output SVG path.",
    )
    args = parser.parse_args()
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(generate_svg(), encoding="utf-8")
    print(f"Wrote {args.output}")


if __name__ == "__main__":
    main()
