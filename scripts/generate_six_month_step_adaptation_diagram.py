#!/usr/bin/env python3
"""Generate a 24-month stepwise pace-adaptation diagram for PaceLetics.

The simulation reuses the component time constants from
generate_adaptation_timeline_diagram.py. It is a deterministic teaching model:
training pace/load is held constant for six months, then stepped up with smaller
increments over time. Each system adapts toward the same external demand with
its unchanged first-order response.
"""

from __future__ import annotations

import argparse
from pathlib import Path

from generate_adaptation_timeline_diagram import SYSTEMS, path, svg_text, wrapped_text


WIDTH = 1600
HEIGHT = 980
PLOT_LEFT = 150
PLOT_TOP = 160
PLOT_RIGHT = 1470
PLOT_BOTTOM = 710
WEEKS = 104
PHASE_WEEKS = 26
LOAD_STEPS = [0.48, 0.60, 0.67, 0.71]
INITIAL_CAPACITY = 0.36
MAX_WEEKLY_ADAPTATION = {
    "Nervous system": 0.060,
    "Cardiovascular": 0.040,
    "Skeletal muscle": 0.025,
    "Tendon": 0.012,
    "Bone": 0.007,
}


def x_to_px(week: float) -> float:
    return PLOT_LEFT + (week / WEEKS) * (PLOT_RIGHT - PLOT_LEFT)


def y_to_px(value: float) -> float:
    return PLOT_BOTTOM - value * (PLOT_BOTTOM - PLOT_TOP)


def load_for_week(week: float) -> float:
    index = min(int(week // PHASE_WEEKS), len(LOAD_STEPS) - 1)
    return LOAD_STEPS[index]


def adaptation_step(system: dict[str, object], value: float, target: float) -> float:
    raw_gain = max(0.0, (target - value) * (1.0 - pow(2.718281828459045, -1.0 / float(system["tau"]))))
    max_gain = MAX_WEEKLY_ADAPTATION[str(system["name"])]
    return min(raw_gain, max_gain)


def simulate_system(system: dict[str, object]) -> list[float]:
    value = INITIAL_CAPACITY
    values: list[float] = []

    for week in range(WEEKS + 1):
        target = load_for_week(week)
        values.append(value)
        value += adaptation_step(system, value, target)

    return values


def load_step_points() -> list[tuple[float, float]]:
    points: list[tuple[float, float]] = []
    for index, load in enumerate(LOAD_STEPS):
        start = index * PHASE_WEEKS
        end = (index + 1) * PHASE_WEEKS
        if index == 0:
            points.append((x_to_px(start), y_to_px(load)))
        else:
            previous = LOAD_STEPS[index - 1]
            points.append((x_to_px(start), y_to_px(previous)))
            points.append((x_to_px(start), y_to_px(load)))
        points.append((x_to_px(end), y_to_px(load)))
    return points


def generate_svg() -> str:
    parts: list[str] = [
        '<?xml version="1.0" encoding="UTF-8"?>',
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{WIDTH}" height="{HEIGHT}" viewBox="0 0 {WIDTH} {HEIGHT}" role="img" aria-labelledby="title desc">',
        '<title id="title">Twenty-four-month stepwise pace adaptation</title>',
        '<desc id="desc">Conceptual simulation where training pace is held constant for six months and then increased every six months.</desc>',
        f'<rect width="{WIDTH}" height="{HEIGHT}" fill="#f8fafc"/>',
        svg_text("24-Month Adaptation With 6-Month Pace Blocks", 76, 62, size=35, weight=850, fill="#102033"),
        svg_text("Didactic model: six-month pace blocks with unchanged adaptation rates and smaller load steps. Vertical scale: qualitative and unitless.", 78, 98, size=19, fill="#64748b"),
        f'<rect x="{PLOT_LEFT}" y="{PLOT_TOP}" width="{PLOT_RIGHT - PLOT_LEFT}" height="{PLOT_BOTTOM - PLOT_TOP}" rx="12" fill="#ffffff" stroke="#d9e2ec" stroke-width="2"/>',
    ]

    phase_colors = ["#ecfeff", "#f0fdf4", "#fff7ed", "#fef2f2"]
    phase_labels = ["0-6 mo", "6-12 mo", "12-18 mo", "18-24 mo"]
    for index, (color, label) in enumerate(zip(phase_colors, phase_labels, strict=True)):
        x1 = x_to_px(index * PHASE_WEEKS)
        x2 = x_to_px((index + 1) * PHASE_WEEKS)
        parts.append(f'<rect x="{x1:.1f}" y="{PLOT_TOP}" width="{x2 - x1:.1f}" height="{PLOT_BOTTOM - PLOT_TOP}" fill="{color}" opacity="0.58"/>')
        parts.append(svg_text(label, (x1 + x2) / 2, PLOT_TOP + 48, size=18, weight=720, anchor="middle", fill="#64748b"))

    for week, label in [(0, "start"), (26, "6 mo"), (52, "12 mo"), (78, "18 mo"), (104, "24 mo")]:
        x = x_to_px(week)
        parts.append(f'<line x1="{x:.1f}" y1="{PLOT_TOP}" x2="{x:.1f}" y2="{PLOT_BOTTOM}" stroke="#d1dce8" stroke-width="2"/>')
        parts.append(svg_text(label, x, PLOT_BOTTOM + 39, size=18, anchor="middle", fill="#64748b"))

    qualitative_ticks = [
        (0.25, "low"),
        (0.50, "moderate"),
        (0.75, "high"),
        (1.00, "very high"),
    ]
    for value, label in qualitative_ticks:
        y = y_to_px(value)
        parts.append(f'<line x1="{PLOT_LEFT}" y1="{y:.1f}" x2="{PLOT_RIGHT}" y2="{y:.1f}" stroke="#e8eef5" stroke-width="2"/>')
        parts.append(svg_text(label, PLOT_LEFT - 18, y + 6, size=15, anchor="end", fill="#64748b"))

    parts.extend(
        [
            f'<line x1="{PLOT_LEFT}" y1="{PLOT_BOTTOM}" x2="{PLOT_RIGHT}" y2="{PLOT_BOTTOM}" stroke="#243447" stroke-width="3"/>',
            f'<line x1="{PLOT_LEFT}" y1="{PLOT_TOP}" x2="{PLOT_LEFT}" y2="{PLOT_BOTTOM}" stroke="#243447" stroke-width="3"/>',
            svg_text("training time", (PLOT_LEFT + PLOT_RIGHT) / 2, 900, size=24, weight=760, anchor="middle", fill="#1f2937"),
        ]
    )

    load_points = load_step_points()
    parts.append(
        f'<path d="{path(load_points)}" fill="none" stroke="#111827" stroke-width="6" stroke-linecap="butt" stroke-linejoin="miter" stroke-dasharray="12 9"/>'
    )
    parts.append(svg_text("degressive pace/load steps per 6-month block", x_to_px(61), y_to_px(0.89), size=18, weight=800, fill="#111827"))
    parts.append(svg_text("+0.12, +0.07, +0.04", x_to_px(72), y_to_px(0.845), size=16, weight=760, fill="#475569"))

    for system in SYSTEMS:
        values = simulate_system(system)
        curve_points = [(x_to_px(week), y_to_px(value)) for week, value in enumerate(values)]
        parts.append(
            f'<path d="{path(curve_points)}" fill="none" stroke="{system["color"]}" stroke-width="6" stroke-linecap="round" stroke-linejoin="round"/>'
        )

    label_specs = [
        ("Nervous system", 10, 40, -62),
        ("Cardiovascular", 20, 36, 10),
        ("Skeletal muscle", 43, 34, -48),
        ("Tendon", 73, 34, -34),
        ("Bone", 95, -248, -26),
    ]
    by_name = {system["name"]: system for system in SYSTEMS}
    for name, week, dx, dy in label_specs:
        system = by_name[name]
        value = simulate_system(system)[week]
        x = x_to_px(week)
        y = y_to_px(value)
        label_x = x + dx
        label_y = y + dy
        parts.extend(
            [
                f'<circle cx="{x:.1f}" cy="{y:.1f}" r="7" fill="{system["color"]}" stroke="#ffffff" stroke-width="4"/>',
                f'<rect x="{label_x:.1f}" y="{label_y:.1f}" width="232" height="50" rx="8" fill="#ffffff" opacity="0.96" stroke="#e2e8f0" stroke-width="2"/>',
                svg_text(system["name"], label_x + 13, label_y + 21, size=17, weight=820, fill=system["color"]),
                svg_text(f"tau: {system['tau']} weeks", label_x + 13, label_y + 42, size=14, fill="#64748b"),
            ]
        )

    callout_x = 955
    callout_y = 565
    callout_body = (
        "The adaptation rates stay constant. Smaller later pace steps let muscle, tendon, "
        "and bone close the gap instead of chasing ever larger jumps."
    )
    parts.extend(
        [
            f'<rect x="{callout_x}" y="{callout_y}" width="490" height="126" rx="10" fill="#ffffff" stroke="#dbe4ef" stroke-width="2"/>',
            f'<rect x="{callout_x}" y="{callout_y}" width="8" height="126" rx="4" fill="#0ea5e9"/>',
            svg_text("Model reading", callout_x + 24, callout_y + 36, size=22, weight=800, fill="#0f172a"),
            wrapped_text(callout_body, callout_x + 24, callout_y + 68, width_chars=52, line_height=22, size=18),
        ]
    )

    parts.append("</svg>")
    return "\n".join(parts)


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate the PaceLetics 24-month stepwise pace-adaptation diagram.")
    parser.add_argument(
        "--output",
        type=Path,
        default=Path("PaceLetics.Web/wwwroot/images/diagrams/six-month-step-adaptation.svg"),
        help="Output SVG path.",
    )
    args = parser.parse_args()
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(generate_svg(), encoding="utf-8")
    print(f"Wrote {args.output}")


if __name__ == "__main__":
    main()
