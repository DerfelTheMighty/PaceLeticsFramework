#!/usr/bin/env python3
"""Generate a Bohm et al. (2015) tendon adaptation effect-size diagram.

The chart uses aggregate data reported in:
Bohm, Mersmann & Arampatzis (2015), "Human tendon adaptation in response to
mechanical loading", Sports Medicine - Open.
"""

from __future__ import annotations

import argparse
import html
from dataclasses import dataclass
from pathlib import Path


WIDTH = 1600
HEIGHT = 900
X_MIN = -0.60
X_MAX = 1.30


@dataclass(frozen=True)
class Effect:
    label: str
    n: int
    smd: float
    ci_low: float
    ci_high: float
    color: str


PROPERTY_EFFECTS = [
    Effect("Tendon stiffness", 37, 0.70, 0.51, 0.88, "#0891b2"),
    Effect("Young's modulus", 17, 0.69, 0.36, 1.03, "#7c3aed"),
    Effect("Cross-sectional area", 33, 0.24, 0.07, 0.42, "#64748b"),
]

INTENSITY_EFFECTS = [
    Effect("High intensity >70% MVC/RM", 27, 0.90, 0.71, 1.08, "#059669"),
    Effect("Low intensity <70% MVC/RM", 5, 0.04, -0.46, 0.53, "#e11d48"),
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


def x_to_px(value: float, left: float, right: float) -> float:
    return left + (value - X_MIN) / (X_MAX - X_MIN) * (right - left)


def ci_line(effect: Effect, y: float, left: float, right: float) -> str:
    x_low = x_to_px(effect.ci_low, left, right)
    x_high = x_to_px(effect.ci_high, left, right)
    x_mid = x_to_px(effect.smd, left, right)
    return "\n".join(
        [
            f'<line x1="{x_low:.1f}" y1="{y:.1f}" x2="{x_high:.1f}" y2="{y:.1f}" stroke="{effect.color}" stroke-width="6" stroke-linecap="round"/>',
            f'<line x1="{x_low:.1f}" y1="{y - 13:.1f}" x2="{x_low:.1f}" y2="{y + 13:.1f}" stroke="{effect.color}" stroke-width="4" stroke-linecap="round"/>',
            f'<line x1="{x_high:.1f}" y1="{y - 13:.1f}" x2="{x_high:.1f}" y2="{y + 13:.1f}" stroke="{effect.color}" stroke-width="4" stroke-linecap="round"/>',
            f'<polygon points="{x_mid:.1f},{y - 15:.1f} {x_mid + 15:.1f},{y:.1f} {x_mid:.1f},{y + 15:.1f} {x_mid - 15:.1f},{y:.1f}" fill="{effect.color}"/>',
        ]
    )


def panel(
    *,
    x: float,
    y: float,
    width: float,
    height: float,
    title: str,
    subtitle: str,
    effects: list[Effect],
) -> str:
    plot_left = x + 290
    plot_right = x + width - 60
    plot_top = y + 115
    row_gap = 105
    row_start = plot_top + 54
    axis_y = y + height - 72

    parts: list[str] = [
        f'<rect x="{x:.1f}" y="{y:.1f}" width="{width:.1f}" height="{height:.1f}" rx="16" fill="#ffffff" stroke="#d9e2ec" stroke-width="2"/>',
        svg_text(title, x + 34, y + 44, size=25, weight=850, fill="#0f172a"),
        svg_text(subtitle, x + 34, y + 74, size=16, fill="#64748b"),
    ]

    for tick in [-0.5, 0.0, 0.5, 1.0]:
        tick_x = x_to_px(tick, plot_left, plot_right)
        parts.append(f'<line x1="{tick_x:.1f}" y1="{plot_top:.1f}" x2="{tick_x:.1f}" y2="{axis_y:.1f}" stroke="#e2e8f0" stroke-width="2"/>')
        parts.append(svg_text(f"{tick:g}", tick_x, axis_y + 33, size=15, anchor="middle", fill="#64748b"))

    zero_x = x_to_px(0, plot_left, plot_right)
    parts.append(f'<line x1="{zero_x:.1f}" y1="{plot_top:.1f}" x2="{zero_x:.1f}" y2="{axis_y:.1f}" stroke="#475569" stroke-width="3" stroke-dasharray="8 8"/>')
    parts.append(svg_text("no effect", zero_x, plot_top - 17, size=14, anchor="middle", fill="#475569"))

    for index, effect in enumerate(effects):
        row_y = row_start + index * row_gap
        parts.append(svg_text(effect.label, x + 34, row_y + 7, size=19, weight=760, fill="#1e293b"))
        parts.append(svg_text(f"N={effect.n}", x + 34, row_y + 35, size=15, fill="#64748b"))
        parts.append(ci_line(effect, row_y, plot_left, plot_right))
        parts.append(
            svg_text(
                f"{effect.smd:.2f} [{effect.ci_low:.2f}, {effect.ci_high:.2f}]",
                plot_right,
                row_y + 37,
                size=15,
                anchor="end",
                fill="#475569",
            )
        )

    parts.append(f'<line x1="{plot_left:.1f}" y1="{axis_y:.1f}" x2="{plot_right:.1f}" y2="{axis_y:.1f}" stroke="#243447" stroke-width="3"/>')
    parts.append(svg_text("standardized mean difference (SMD, 95% CI)", (plot_left + plot_right) / 2, y + height - 16, size=16, weight=700, anchor="middle", fill="#334155"))
    return "\n".join(parts)


def generate_svg() -> str:
    parts: list[str] = [
        '<?xml version="1.0" encoding="UTF-8"?>',
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{WIDTH}" height="{HEIGHT}" viewBox="0 0 {WIDTH} {HEIGHT}" role="img" aria-labelledby="title desc">',
        '<title id="title">Bohm et al. 2015 tendon adaptation effect sizes</title>',
        '<desc id="desc">Effect-size diagram using pooled standardized mean differences and confidence intervals from Bohm et al. 2015.</desc>',
        '<rect width="1600" height="900" fill="#f8fafc"/>',
        svg_text("Tendon Adaptation From Mechanical Loading", 80, 62, size=36, weight=860, fill="#102033"),
        svg_text("Bohm et al. 2015 meta-analysis: pooled SMDs with 95% confidence intervals.", 80, 97, size=19, fill="#64748b"),
        panel(
            x=80,
            y=150,
            width=690,
            height=565,
            title="What changes in the tendon?",
            subtitle="All pooled intervention effects were significant.",
            effects=PROPERTY_EFFECTS,
        ),
        panel(
            x=830,
            y=150,
            width=690,
            height=565,
            title="Does loading intensity matter?",
            subtitle="Stiffness adaptation differed by intensity (p < 0.00001).",
            effects=INTENSITY_EFFECTS,
        ),
    ]

    parts.extend(
        [
            f'<rect x="80" y="745" width="1440" height="78" rx="14" fill="#eff6ff" stroke="#bfdbfe" stroke-width="2"/>',
            f'<rect x="80" y="745" width="8" height="78" rx="4" fill="#0891b2"/>',
            svg_text("Interpretation", 106, 779, size=20, weight=840, fill="#0f172a"),
            svg_text("High mechanical loading is not inherently bad: in Bohm et al., tendon stiffness improved strongly with high intensity, while low intensity was near zero.", 106, 807, size=17, fill="#475569"),
            svg_text("Source data: Bohm, Mersmann & Arampatzis 2015, Sports Medicine - Open; aggregate SMDs and 95% CIs reported in Results/Subgroup Analysis.", 80, 862, size=15, fill="#64748b"),
            "</svg>",
        ]
    )
    return "\n".join(parts)


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate the PaceLetics Bohm tendon adaptation diagram.")
    parser.add_argument(
        "--output",
        type=Path,
        default=Path("PaceLetics.Web/wwwroot/images/diagrams/load-adaptation-window.svg"),
        help="Output SVG path.",
    )
    args = parser.parse_args()
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(generate_svg(), encoding="utf-8")
    print(f"Wrote {args.output}")


if __name__ == "__main__":
    main()
