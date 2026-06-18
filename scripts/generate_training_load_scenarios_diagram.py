#!/usr/bin/env python3
"""Generate a 12-week training-load scenario diagram for PaceLetics.

This is a simple deterministic teaching model, not fitted biology. It visualizes
the PaceLetics hypothesis: repeated mechanical load impulses can be too small to
adapt, appropriately dosed, or progressively exceed tissue adaptation when
internal effort is controlled while external pace/load drifts upward.
"""

from __future__ import annotations

import argparse
import html
from dataclasses import dataclass
from pathlib import Path


WIDTH = 1600
HEIGHT = 1040
DAYS = 84
Y_MAX = 1.25


@dataclass(frozen=True)
class Scenario:
    title: str
    subtitle: str
    load_kind: str
    capacity_start: float
    adapt_rate: float
    wear_decay: float
    wear_factor: float
    overload_factor: float


SCENARIOS = [
    Scenario(
        "1. Light load only",
        "low wear-off, but almost no adaptive stimulus",
        "light",
        capacity_start=0.43,
        adapt_rate=0.0018,
        wear_decay=0.58,
        wear_factor=0.030,
        overload_factor=0.10,
    ),
    Scenario(
        "2. Pace-guided load",
        "regular peaks stay close to current tissue capacity",
        "pace",
        capacity_start=0.54,
        adapt_rate=0.0260,
        wear_decay=0.64,
        wear_factor=0.040,
        overload_factor=0.12,
    ),
    Scenario(
        "3. HR-guided drift",
        "external load rises while tissue adaptation lags behind",
        "heart_rate",
        capacity_start=0.54,
        adapt_rate=0.0065,
        wear_decay=0.76,
        wear_factor=0.046,
        overload_factor=0.34,
    ),
]


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


def load_for_day(kind: str, day: int) -> float:
    week = day // 7
    dow = day % 7

    if kind == "light":
        if dow in (1, 3, 5):
            return 0.25 + 0.01 * (week / 11)
        if dow == 6 and week % 2 == 0:
            return 0.30
        return 0.0

    if kind == "pace":
        progression = min(0.16, week * 0.013)
        if dow in (1, 4):
            return 0.52 + progression
        if dow in (2, 6):
            return 0.34 + progression * 0.35
        return 0.0

    if kind == "heart_rate":
        drift = week * 0.045
        if dow in (1, 4):
            return min(1.12, 0.55 + drift)
        if dow in (2, 6):
            return min(0.94, 0.40 + drift * 0.70)
        return 0.0

    raise ValueError(f"Unknown load kind: {kind}")


def simulate(scenario: Scenario) -> tuple[list[float], list[float], list[float]]:
    loads: list[float] = []
    capacity: list[float] = []
    wear: list[float] = []

    current_capacity = scenario.capacity_start
    current_wear = 0.06

    for day in range(DAYS):
        load = load_for_day(scenario.load_kind, day)
        overload = max(0.0, load - current_capacity)
        useful_stimulus = max(0.0, min(load, current_capacity + 0.10) - 0.34)

        current_wear = min(
            Y_MAX,
            current_wear * scenario.wear_decay
            + scenario.wear_factor * load
            + scenario.overload_factor * overload,
        )
        current_capacity = min(
            1.08,
            current_capacity
            + scenario.adapt_rate * useful_stimulus
            - (0.00055 if load < 0.05 else 0.0),
        )

        loads.append(load)
        capacity.append(current_capacity)
        wear.append(current_wear)

    return loads, capacity, wear


def panel_svg(scenario: Scenario, index: int) -> str:
    x = 80
    y = 145 + index * 278
    width = 1440
    height = 238
    plot_left = x + 250
    plot_right = x + width - 45
    plot_top = y + 56
    plot_bottom = y + height - 42

    def px(day: int) -> float:
        return plot_left + day / (DAYS - 1) * (plot_right - plot_left)

    def py(value: float) -> float:
        return plot_bottom - min(Y_MAX, max(0.0, value)) / Y_MAX * (plot_bottom - plot_top)

    loads, capacity, wear = simulate(scenario)
    capacity_points = [(px(day), py(value)) for day, value in enumerate(capacity)]
    wear_points = [(px(day), py(value)) for day, value in enumerate(wear)]
    wear_area = [(plot_left, plot_bottom), *wear_points, (plot_right, plot_bottom)]

    parts: list[str] = [
        f'<rect x="{x}" y="{y}" width="{width}" height="{height}" rx="14" fill="#ffffff" stroke="#d9e2ec" stroke-width="2"/>',
        svg_text(scenario.title, x + 28, y + 42, size=23, weight=860, fill="#0f172a"),
        svg_text(scenario.subtitle, x + 28, y + 72, size=16, fill="#64748b"),
    ]

    for week in range(13):
        tick_x = plot_left + week / 12 * (plot_right - plot_left)
        parts.append(f'<line x1="{tick_x:.1f}" y1="{plot_top:.1f}" x2="{tick_x:.1f}" y2="{plot_bottom:.1f}" stroke="#e2e8f0" stroke-width="1.5"/>')
        if week in (0, 4, 8, 12):
            parts.append(svg_text(f"W{week}", tick_x, plot_bottom + 26, size=14, anchor="middle", fill="#64748b"))

    for value in (0.4, 0.8, 1.2):
        line_y = py(value)
        parts.append(f'<line x1="{plot_left:.1f}" y1="{line_y:.1f}" x2="{plot_right:.1f}" y2="{line_y:.1f}" stroke="#edf2f7" stroke-width="1.5"/>')

    bar_width = max(2.8, (plot_right - plot_left) / DAYS * 0.42)
    for day, load in enumerate(loads):
        if load <= 0:
            continue
        bar_x = px(day) - bar_width / 2
        bar_y = py(load)
        color = "#0891b2" if load <= capacity[day] + 0.10 else "#e11d48"
        opacity = "0.42" if color == "#0891b2" else "0.56"
        parts.append(f'<rect x="{bar_x:.1f}" y="{bar_y:.1f}" width="{bar_width:.1f}" height="{plot_bottom - bar_y:.1f}" rx="2" fill="{color}" opacity="{opacity}"/>')

    parts.extend(
        [
            f'<path d="{path(wear_area)} Z" fill="#fb7185" opacity="0.16"/>',
            f'<path d="{path(wear_points)}" fill="none" stroke="#e11d48" stroke-width="5" stroke-linecap="round" stroke-linejoin="round"/>',
            f'<path d="{path(capacity_points)}" fill="none" stroke="#059669" stroke-width="6" stroke-linecap="round" stroke-linejoin="round"/>',
            f'<line x1="{plot_left:.1f}" y1="{plot_bottom:.1f}" x2="{plot_right:.1f}" y2="{plot_bottom:.1f}" stroke="#243447" stroke-width="2.5"/>',
            f'<line x1="{plot_left:.1f}" y1="{plot_top:.1f}" x2="{plot_left:.1f}" y2="{plot_bottom:.1f}" stroke="#243447" stroke-width="2.5"/>',
        ]
    )

    if index == 0:
        parts.extend(
            [
                f'<rect x="{plot_right - 445:.1f}" y="{plot_top + 8:.1f}" width="420" height="42" rx="9" fill="#ffffff" opacity="0.92" stroke="#dbe4ef" stroke-width="2"/>',
                f'<rect x="{plot_right - 425:.1f}" y="{plot_top + 22:.1f}" width="30" height="10" rx="2" fill="#0891b2" opacity="0.45"/>',
                svg_text("load peaks", plot_right - 386, plot_top + 36, size=15, weight=700, fill="#334155"),
                f'<line x1="{plot_right - 290:.1f}" y1="{plot_top + 28:.1f}" x2="{plot_right - 250:.1f}" y2="{plot_top + 28:.1f}" stroke="#059669" stroke-width="5" stroke-linecap="round"/>',
                svg_text("adaptation", plot_right - 242, plot_top + 36, size=15, weight=700, fill="#334155"),
                f'<line x1="{plot_right - 135:.1f}" y1="{plot_top + 28:.1f}" x2="{plot_right - 96:.1f}" y2="{plot_top + 28:.1f}" stroke="#e11d48" stroke-width="5" stroke-linecap="round"/>',
                svg_text("wear-off", plot_right - 88, plot_top + 36, size=15, weight=700, fill="#334155"),
            ]
        )

    return "\n".join(parts)


def generate_svg() -> str:
    parts: list[str] = [
        '<?xml version="1.0" encoding="UTF-8"?>',
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{WIDTH}" height="{HEIGHT}" viewBox="0 0 {WIDTH} {HEIGHT}" role="img" aria-labelledby="title desc">',
        '<title id="title">Twelve-week mechanical load scenarios</title>',
        '<desc id="desc">Three simplified 12-week training scenarios showing load peaks, adaptation capacity, and wear-off.</desc>',
        '<rect width="1600" height="1040" fill="#f8fafc"/>',
        svg_text("12-Week Mechanical Load Scenarios", 80, 62, size=36, weight=860, fill="#102033"),
        svg_text("Simplified model: repeated load peaks, tissue adaptation, and residual wear-off over time.", 80, 97, size=19, fill="#64748b"),
    ]

    for index, scenario in enumerate(SCENARIOS):
        parts.append(panel_svg(scenario, index))

    parts.append(
        svg_text(
            "Model assumption: mechanical load peaks are external impulses; adaptation changes slowly; wear-off decays between sessions but accumulates when peaks exceed current capacity.",
            80,
            1000,
            size=15,
            fill="#64748b",
        )
    )
    parts.append("</svg>")
    return "\n".join(parts)


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate the PaceLetics 12-week training-load scenario diagram.")
    parser.add_argument(
        "--output",
        type=Path,
        default=Path("PaceLetics.Web/wwwroot/images/diagrams/training-load-scenarios.svg"),
        help="Output SVG path.",
    )
    args = parser.parse_args()
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(generate_svg(), encoding="utf-8")
    print(f"Wrote {args.output}")


if __name__ == "__main__":
    main()
