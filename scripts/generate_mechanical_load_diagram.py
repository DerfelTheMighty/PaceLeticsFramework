#!/usr/bin/env python3
"""Generate the mechanical-load-vs-pace diagram for PaceLetics.

The plotted trend is intentionally limited to what Jiang et al. (2024) support:
ground reaction force variables change systematically with running speed across
seven tested speeds. The line is normalized and illustrative, not a fitted data
reproduction of the paper's figure values.
"""

from __future__ import annotations

import argparse
import html
from pathlib import Path


WIDTH = 1600
HEIGHT = 900
PLOT_LEFT = 150
PLOT_TOP = 145
PLOT_RIGHT = 1470
PLOT_BOTTOM = 685
PACE_MIN = 6.4
PACE_MAX = 3.55
JIANG_SPEEDS_KMH = [10, 11, 12, 13, 14, 15, 16]


def speed_from_pace(pace_min_per_km: float) -> float:
    return 1000 / (pace_min_per_km * 60)


SPEED_MIN = speed_from_pace(PACE_MIN)
SPEED_MAX = speed_from_pace(PACE_MAX)


def normalize_speed(pace_min_per_km: float) -> float:
    speed = speed_from_pace(pace_min_per_km)
    return (speed - SPEED_MIN) / (SPEED_MAX - SPEED_MIN)


def normalized_grf_trend(pace_min_per_km: float) -> float:
    x = max(0.0, min(1.0, normalize_speed(pace_min_per_km)))
    return 0.18 + 0.70 * x


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


def path(points: list[tuple[float, float]]) -> str:
    first, *rest = points
    return " ".join([f"M {first[0]:.1f} {first[1]:.1f}", *[f"L {x:.1f} {y:.1f}" for x, y in rest]])


def pace_label(pace_min_per_km: float) -> str:
    minutes = int(pace_min_per_km)
    seconds = int(round((pace_min_per_km - minutes) * 60))
    return f"{minutes}:{seconds:02d}/km"


def generate_svg() -> str:
    samples = [PACE_MIN - i * (PACE_MIN - PACE_MAX) / 220 for i in range(221)]
    trend_points = [(x_to_px(pace), y_to_px(normalized_grf_trend(pace))) for pace in samples]
    measured_points = [
        (x_to_px(60 / speed_kmh), y_to_px(normalized_grf_trend(60 / speed_kmh)), speed_kmh)
        for speed_kmh in JIANG_SPEEDS_KMH
    ]

    parts: list[str] = [
        '<?xml version="1.0" encoding="UTF-8"?>',
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{WIDTH}" height="{HEIGHT}" viewBox="0 0 {WIDTH} {HEIGHT}" role="img" aria-labelledby="title desc">',
        '<title id="title">Selected GRF variables increase with faster running pace</title>',
        '<desc id="desc">Evidence-based diagram showing selected normalized ground reaction force variables rising as running speed increases.</desc>',
        '<rect width="1600" height="900" fill="#f8fafc"/>',
        svg_text("Selected Ground Reaction Force Variables vs Pace", 80, 62, size=34, weight=860, fill="#102033"),
        svg_text("Jiang et al. 2024: peak propulsive force, VALR, and peak vertical impact force increased linearly with speed.", 80, 97, size=19, fill="#64748b"),
        f'<rect x="{PLOT_LEFT}" y="{PLOT_TOP}" width="{PLOT_RIGHT - PLOT_LEFT}" height="{PLOT_BOTTOM - PLOT_TOP}" rx="12" fill="#ffffff" stroke="#d9e2ec" stroke-width="2"/>',
    ]

    zones = [
        (6.4, 5.45, "#ecfeff", "lower speed"),
        (5.45, 4.35, "#f0fdf4", "moderate speed"),
        (4.35, 3.55, "#fff1f2", "higher speed"),
    ]
    for slow, fast, color, label in zones:
        x1 = x_to_px(slow)
        x2 = x_to_px(fast)
        parts.append(f'<rect x="{x1:.1f}" y="{PLOT_TOP}" width="{x2 - x1:.1f}" height="{PLOT_BOTTOM - PLOT_TOP}" fill="{color}" opacity="0.72"/>')
        parts.append(svg_text(label, (x1 + x2) / 2, PLOT_TOP + 48, size=18, weight=740, anchor="middle", fill="#64748b"))

    for pace in [6.0, 5.5, 5.0, 4.5, 4.0, 3.75]:
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
            svg_text("normalized selected GRF variables", 43, 435, size=23, weight=760, anchor="middle", fill="#1f2937", extra='transform="rotate(-90 43 435)"'),
        ]
    )

    # Shaded area under the normalized GRF trend.
    area = [(x_to_px(PACE_MIN), y_to_px(0.0)), *trend_points, (x_to_px(PACE_MAX), y_to_px(0.0))]
    parts.append(f'<path d="{path(area)} Z" fill="#0ea5e9" opacity="0.13"/>')

    parts.extend(
        [
            f'<path d="{path(trend_points)}" fill="none" stroke="#0891b2" stroke-width="8" stroke-linecap="round" stroke-linejoin="round"/>',
        ]
    )
    for x, y, speed_kmh in measured_points:
        parts.append(f'<circle cx="{x:.1f}" cy="{y:.1f}" r="9" fill="#ffffff" stroke="#0891b2" stroke-width="5"/>')
        parts.append(svg_text(f"{speed_kmh} km/h", x, y - 22, size=15, weight=720, anchor="middle", fill="#334155"))

    footnote = (
        "Evidence anchor: Jiang et al. 2024 measured GRF variables at 10-16 km/h. Peak propulsive force (r=0.627), "
        "VALR (r=0.639), and peak vertical impact force (r=0.691) increased linearly with speed."
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
