#!/usr/bin/env python3
"""Generate a scripted PaceLetics pace-model diagram as SVG.

The diagram is intentionally didactic rather than a physiological simulation:
it models a smooth workload curve with a visibly steeper severe-intensity
region above Critical Speed. Daniels/VDOT pace markers are placed on the same
relative speed axis so both concepts can be explained in one figure.
"""

from __future__ import annotations

import argparse
import html
from pathlib import Path
from textwrap import wrap


WIDTH = 1600
HEIGHT = 900
PLOT_LEFT = 145
PLOT_TOP = 105
PLOT_RIGHT = 1485
PLOT_BOTTOM = 730
X_MIN = 0.65
X_MAX = 1.18
Y_MIN = 0.0
Y_MAX = 1.0


def workload(relative_speed: float) -> float:
    """Return a normalized workload for a relative speed where CS == 1.0."""
    if relative_speed <= 1.0:
        # Moderate-to-heavy intensities rise smoothly and remain controlled.
        scaled = (relative_speed - X_MIN) / (1.0 - X_MIN)
        return 0.16 + 0.37 * max(0.0, scaled) ** 1.65

    # Severe intensity: fatigue pressure rises sharply above CS.
    severe = (relative_speed - 1.0) / (X_MAX - 1.0)
    return min(0.96, 0.53 + 0.43 * severe ** 1.35)


def x_to_px(value: float) -> float:
    return PLOT_LEFT + (value - X_MIN) / (X_MAX - X_MIN) * (PLOT_RIGHT - PLOT_LEFT)


def y_to_px(value: float) -> float:
    return PLOT_BOTTOM - (value - Y_MIN) / (Y_MAX - Y_MIN) * (PLOT_BOTTOM - PLOT_TOP)


def svg_text(
    text: str,
    x: float,
    y: float,
    *,
    size: int = 28,
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
    line_height: int = 25,
    size: int = 22,
    fill: str = "#263442",
) -> str:
    lines = wrap(text, width_chars)
    output = []
    for index, line in enumerate(lines):
        output.append(svg_text(line, x, y + index * line_height, size=size, fill=fill))
    return "\n".join(output)


def callout(
    x: float,
    y: float,
    width: float,
    title: str,
    body: str,
    *,
    accent: str,
    body_width_chars: int = 42,
    title_size: int = 21,
    body_size: int = 18,
    line_height: int = 22,
) -> str:
    lines = wrap(body, body_width_chars)
    height = 56 + len(lines) * line_height
    content = [
        f'<rect x="{x:.1f}" y="{y:.1f}" width="{width:.1f}" height="{height:.1f}" rx="8" fill="#ffffff" stroke="#d9e2ec" stroke-width="2"/>',
        f'<rect x="{x:.1f}" y="{y:.1f}" width="8" height="{height:.1f}" rx="4" fill="{accent}"/>',
        svg_text(title, x + 22, y + 32, size=title_size, weight=700, fill="#1f2d3d"),
        wrapped_text(body, x + 22, y + 61, width_chars=body_width_chars, size=body_size, line_height=line_height),
    ]
    return "\n".join(content)


def zone_label(
    title: str,
    subtitle: str,
    x: float,
    y: float,
    *,
    fill: str,
    stroke: str,
    text_fill: str,
) -> str:
    width = 230
    height = 62
    return "\n".join(
        [
            f'<rect x="{x - width / 2:.1f}" y="{y - 27:.1f}" width="{width}" height="{height}" rx="8" fill="{fill}" opacity="0.96" stroke="{stroke}" stroke-width="2"/>',
            svg_text(title, x, y, size=21, weight=700, anchor="middle", fill=text_fill),
            svg_text(subtitle, x, y + 26, size=18, weight=600, anchor="middle", fill=text_fill),
        ]
    )


def pace_band(center: float, low: float, high: float, *, color: str) -> str:
    x1 = x_to_px(low)
    x2 = x_to_px(high)
    y1 = PLOT_BOTTOM - 104
    y2 = PLOT_BOTTOM - 8
    return "\n".join(
        [
            f'<rect x="{x1:.1f}" y="{y1:.1f}" width="{x2 - x1:.1f}" height="{y2 - y1:.1f}" rx="5" fill="{color}" opacity="0.13"/>',
            f'<line x1="{x1:.1f}" y1="{y1:.1f}" x2="{x1:.1f}" y2="{y2:.1f}" stroke="{color}" stroke-width="1.5" opacity="0.35"/>',
            f'<line x1="{x2:.1f}" y1="{y1:.1f}" x2="{x2:.1f}" y2="{y2:.1f}" stroke="{color}" stroke-width="1.5" opacity="0.35"/>',
        ]
    )


def path_from_points(points: list[tuple[float, float]]) -> str:
    head, *tail = points
    commands = [f"M {head[0]:.1f} {head[1]:.1f}"]
    commands.extend(f"L {x:.1f} {y:.1f}" for x, y in tail)
    return " ".join(commands)


def generate_svg(cs_mps: float, repetition_speed_mps: float | None) -> str:
    if repetition_speed_mps is None:
        repetition_speed_mps = cs_mps / 0.90

    r_marker = max(X_MIN, min(X_MAX, repetition_speed_mps / cs_mps))
    daniels_markers = [
        ("E", 0.87, "Easy", "#bfdbfe"),
        ("M", 0.94, "Marathon", "#a5f3fc"),
        ("T", 1.00, "Threshold", "#fde68a"),
        ("I", 1.09, "Intervall", "#e9d5ff"),
        ("R", r_marker, "Repetition", "#ddd6fe"),
    ]

    samples = [X_MIN + i * (X_MAX - X_MIN) / 180 for i in range(181)]
    curve_points = [(x_to_px(x), y_to_px(workload(x))) for x in samples]
    severe_samples = [x for x in samples if x >= 1.0]
    severe_polygon = (
        [(x_to_px(1.0), y_to_px(0))]
        + [(x_to_px(x), y_to_px(workload(x))) for x in severe_samples]
        + [(x_to_px(X_MAX), y_to_px(0))]
    )

    parts: list[str] = [
        '<?xml version="1.0" encoding="UTF-8"?>',
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{WIDTH}" height="{HEIGHT}" viewBox="0 0 {WIDTH} {HEIGHT}" role="img" aria-labelledby="title desc">',
        '<title id="title">Workload curve with Daniels/VDOT and Critical Speed</title>',
        '<desc id="desc">Didactic diagram with Critical Speed, D-prime region, and Daniels pace markers.</desc>',
        '<rect width="1600" height="900" fill="#f8fafc"/>',
        svg_text("Workload Curve: Daniels/VDOT and Critical Speed", 80, 58, size=34, weight=800, fill="#14213d"),
        svg_text("Didactic model: speed relative to CS, not an individual diagnostic curve", 80, 91, size=19, fill="#5f6f7f"),
    ]

    # Plot background and intensity bands.
    parts.extend(
        [
            f'<rect x="{PLOT_LEFT}" y="{PLOT_TOP}" width="{PLOT_RIGHT - PLOT_LEFT}" height="{PLOT_BOTTOM - PLOT_TOP}" rx="12" fill="#ffffff" stroke="#d9e2ec" stroke-width="2"/>',
            f'<rect x="{x_to_px(X_MIN):.1f}" y="{PLOT_TOP}" width="{x_to_px(0.96) - x_to_px(X_MIN):.1f}" height="{PLOT_BOTTOM - PLOT_TOP}" fill="#eaf7ef"/>',
            f'<rect x="{x_to_px(0.96):.1f}" y="{PLOT_TOP}" width="{x_to_px(1.03) - x_to_px(0.96):.1f}" height="{PLOT_BOTTOM - PLOT_TOP}" fill="#fff4d6"/>',
            f'<rect x="{x_to_px(1.03):.1f}" y="{PLOT_TOP}" width="{x_to_px(X_MAX) - x_to_px(1.03):.1f}" height="{PLOT_BOTTOM - PLOT_TOP}" fill="#fdecea"/>',
        ]
    )

    # Grid.
    for rel in [0.70, 0.80, 0.90, 1.00, 1.10]:
        x = x_to_px(rel)
        parts.append(f'<line x1="{x:.1f}" y1="{PLOT_TOP}" x2="{x:.1f}" y2="{PLOT_BOTTOM}" stroke="#e8eef5" stroke-width="2"/>')
        parts.append(svg_text(f"{rel:.0%}", x, PLOT_BOTTOM + 39, size=19, anchor="middle", fill="#66788a"))
    for yv in [0.2, 0.4, 0.6, 0.8]:
        y = y_to_px(yv)
        parts.append(f'<line x1="{PLOT_LEFT}" y1="{y:.1f}" x2="{PLOT_RIGHT}" y2="{y:.1f}" stroke="#edf2f7" stroke-width="2"/>')

    # Severe / D' fill and curve.
    parts.append(f'<path d="{path_from_points(severe_polygon)} Z" fill="#ef4444" opacity="0.16"/>')

    # Daniels/VDOT pace zones are small ranges, not single exact speeds.
    bands = [
        ("E", 0.87, 0.84, 0.90, "#2563eb"),
        ("M", 0.94, 0.92, 0.96, "#0891b2"),
        ("T", 1.00, 0.985, 1.015, "#d97706"),
        ("I", 1.09, 1.084, 1.096, "#9333ea"),
        ("R", r_marker, max(X_MIN, r_marker - 0.006), min(X_MAX, r_marker + 0.006), "#581c87"),
    ]
    for _label, center, low, high, color in bands:
        parts.append(pace_band(center, low, high, color=color))

    parts.append(f'<path d="{path_from_points(curve_points)}" fill="none" stroke="#17202a" stroke-width="6" stroke-linecap="round" stroke-linejoin="round"/>')

    # CS line.
    cs_x = x_to_px(1.0)
    cs_label_x = cs_x
    cs_label_y = y_to_px(workload(1.0)) - 178
    parts.extend(
        [
            f'<line x1="{cs_x:.1f}" y1="{PLOT_TOP + 10}" x2="{cs_x:.1f}" y2="{PLOT_BOTTOM}" stroke="#c2410c" stroke-width="4" stroke-dasharray="12 10"/>',
            f'<circle cx="{cs_x:.1f}" cy="{y_to_px(workload(1.0)):.1f}" r="9" fill="#c2410c" stroke="#ffffff" stroke-width="4"/>',
            f'<rect x="{cs_label_x - 107:.1f}" y="{cs_label_y - 25:.1f}" width="214" height="38" rx="8" fill="#ffffff" opacity="0.94" stroke="#fed7aa" stroke-width="2"/>',
            svg_text("Critical Speed (CS)", cs_label_x, cs_label_y, size=20, weight=650, anchor="middle", fill="#9a3412"),
        ]
    )

    # Daniels markers are rendered after test markers so their letters stay visible.
    for code, rel, label, fill in daniels_markers:
        marker_x = x_to_px(rel)
        marker_y = y_to_px(workload(rel))
        parts.extend(
            [
                f'<line x1="{marker_x:.1f}" y1="{PLOT_BOTTOM}" x2="{marker_x:.1f}" y2="{marker_y + 14:.1f}" stroke="#334155" stroke-width="2" stroke-dasharray="5 7"/>',
                f'<circle cx="{marker_x:.1f}" cy="{marker_y:.1f}" r="18" fill="{fill}" stroke="#334155" stroke-width="3"/>',
                svg_text(code, marker_x, marker_y + 8, size=23, weight=800, anchor="middle", fill="#0f172a"),
            ]
        )

    # Zone labels.
    parts.extend(
        [
            zone_label("below CS", "controlled / steady", x_to_px(0.79), PLOT_TOP + 34, fill="#f0fdf4", stroke="#bbf7d0", text_fill="#166534"),
            zone_label("near CS", "threshold region", x_to_px(0.995), PLOT_TOP + 34, fill="#fffbeb", stroke="#fde68a", text_fill="#92400e"),
            zone_label("above CS", "D' is consumed", x_to_px(1.105), PLOT_TOP + 34, fill="#fef2f2", stroke="#fecaca", text_fill="#991b1b"),
            svg_text("Speed relative to CS", (PLOT_LEFT + PLOT_RIGHT) / 2, 884, size=24, weight=700, anchor="middle", fill="#1f2d3d"),
            svg_text("metabolic load / fatigue pressure", 43, 450, size=23, weight=700, anchor="middle", fill="#1f2d3d", extra='transform="rotate(-90 43 450)"'),
        ]
    )

    # Axis lines.
    parts.extend(
        [
            f'<line x1="{PLOT_LEFT}" y1="{PLOT_BOTTOM}" x2="{PLOT_RIGHT}" y2="{PLOT_BOTTOM}" stroke="#243447" stroke-width="3"/>',
            f'<line x1="{PLOT_LEFT}" y1="{PLOT_TOP}" x2="{PLOT_LEFT}" y2="{PLOT_BOTTOM}" stroke="#243447" stroke-width="3"/>',
        ]
    )

    # Callouts.
    parts.append(
        callout(
            205,
            190,
            355,
            "Daniels/VDOT",
            "VDOT maps a running performance to one fitness value. E, M, T, I, and R are derived as training paces.",
            accent="#2563eb",
            body_width_chars=34,
        )
    )
    parts.append(
        callout(
            205,
            356,
            355,
            "CS model",
            "CS marks the heavy -> severe boundary. Above it, the finite D' reserve is consumed.",
            accent="#dc2626",
            body_width_chars=34,
        )
    )

    parts.append("</svg>")
    return "\n".join(parts)


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate the PaceLetics pace-model diagram.")
    parser.add_argument("--cs", type=float, default=4.0, help="Critical Speed in m/s. Default: 4.0")
    parser.add_argument(
        "--repetition-speed",
        type=float,
        default=None,
        help="Optional repetition speed in m/s. Defaults to CS / 0.90, equivalent to a 1200 m estimate.",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=Path("PaceLetics.Web/wwwroot/images/diagrams/pace-model-belastungskurve.svg"),
        help="Output SVG path.",
    )
    args = parser.parse_args()

    if args.cs <= 0:
        raise SystemExit("--cs must be positive.")
    if args.repetition_speed is not None and args.repetition_speed <= 0:
        raise SystemExit("--repetition-speed must be positive.")

    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(generate_svg(args.cs, args.repetition_speed), encoding="utf-8")
    print(f"Wrote {args.output}")


if __name__ == "__main__":
    main()
