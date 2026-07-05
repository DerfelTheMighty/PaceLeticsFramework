---
id: pace-controlled-training
title: Pace-guided training
summary: Why we train pace-guided
category: training
sourceModule: Markdown
contentKind: PaceModelInfo
sortOrder: 30
tags:
 - Pace
 - VDOT
 - Critical speed
 - D'
 - Training load
references:
 - Anderson et al. (2026): The Measurement and Application of Critical Speed and D' in Running. | https://link.springer.com/article/10.1007/s40279-026-02410-x
 - Lipkova et al. (2025): Field-based tests for determining critical speed among runners and its practical application. | https://pmc.ncbi.nlm.nih.gov/articles/PMC11933073/
 - Hawley et al. (2018): Adaptations to Endurance and Strength Training. | https://pubmed.ncbi.nlm.nih.gov/28490537/
 - MacInnis & Gibala (2017): Physiological adaptations to interval training and the role of exercise intensity. | https://pubmed.ncbi.nlm.nih.gov/27748956/
 - Kubo et al. (2010): Time course of changes in muscle and tendon properties during strength training and detraining. | https://paulogentil.com/pdf/Time%20Course%20of%20Changes%20in%20Muscle%20and%20Tendon%20Properties%20During%20Strength%20Training%20and%20Detraining.pdf
 - Bohm et al. (2019): Functional Adaptation of Connective Tissue by Training. | https://www.germanjournalsportsmedicine.com/archive/archive-2019/issue-4/functional-adaptation-of-connective-tissue-by-training/
 - Bohm et al. (2015): Human tendon adaptation in response to mechanical loading. | https://link.springer.com/article/10.1186/s40798-015-0009-9
 - Papagiannaki et al. (2020): Running-Related Injury From an Engineering, Medical and Sport Science Perspective. | https://www.frontiersin.org/journals/bioengineering-and-biotechnology/articles/10.3389/fbioe.2020.533391/full
 - Jiang et al. (2024): Comparison of ground reaction forces as running speed increases between male and female runners. | https://www.frontiersin.org/journals/bioengineering-and-biotechnology/articles/10.3389/fbioe.2024.1378284/full
 - Billat et al. (2020): Pacing Strategy Affects the Sub-Elite Marathoner's Cardiac Drift and Performance. | https://www.frontiersin.org/journals/psychology/articles/10.3389/fpsyg.2019.03026/full
 - Warden et al. (2021): Preventing Bone Stress Injuries in Runners with Optimal Workload. | https://pubmed.ncbi.nlm.nih.gov/33635519/
---

## Why pace-guided training?

We primarily control training zones through pace because running speed is a fairly good proxy for
external biomechanical load. [[1]](https://link.springer.com/article/10.1007/s40279-026-02410-x)
[[2]](https://pmc.ncbi.nlm.nih.gov/articles/PMC11933073/)

The core issue is the different adaptation speed of the systems involved: beginners often feel
cardiovascularly fitter after only a few weeks, while tendons, bones, joints, and muscle-tendon
structures need to adapt much more gradually to repeated impact loading.
[[3]](https://pubmed.ncbi.nlm.nih.gov/28490537/) [[4]](https://pubmed.ncbi.nlm.nih.gov/27748956/)
[[5]](https://paulogentil.com/pdf/Time%20Course%20of%20Changes%20in%20Muscle%20and%20Tendon%20Properties%20During%20Strength%20Training%20and%20Detraining.pdf)
[[6]](https://www.germanjournalsportsmedicine.com/archive/archive-2019/issue-4/functional-adaptation-of-connective-tissue-by-training/)
[[11]](https://pubmed.ncbi.nlm.nih.gov/33635519/)

If training is guided only by heart rate or generic zone systems, this mechanical exposure is easily
underestimated. Pace provides a stable external target for how much forceful running is repeated;
heart rate remains useful as a complementary signal for internal strain and recovery. For
heterogeneous group training, the model should also be simple, equipment-light, and consistent over
several months. Pace is the simplest common language for this: measurable, comparable, and
immediately usable in training.

![Adaptation timeline](/images/diagrams/adaptation-timeline.svg)

The following model is deliberately qualitative: the vertical axis has no unit and does not show
measured values, but the relationship between training demand and adaptation. The logic is what
matters. If pace is kept stable over longer blocks and increased only step by step, slower
structures such as tendons and bones can catch up without the mechanical demand constantly racing
ahead of cardiovascular adaptation.

![Six-month step adaptation](/images/diagrams/six-month-step-adaptation.svg)

![Mechanical load versus pace](/images/diagrams/mechanical-load-vs-pace.svg)

## Is high pace inherently bad?

No. Mechanical load is not only a risk; it is also required for adaptation. Tendons and bone respond
to sufficiently strong loading stimuli: Bohm et al. (2015) show in a meta-analysis that tendon
stiffness and material properties improve especially with higher loading intensities.
[[7]](https://link.springer.com/article/10.1186/s40798-015-0009-9)

The diagram below uses their reported effect sizes: high intensities above 70 percent MVC/RM
produced a clear tendon-stiffness adaptation, while low intensities were near zero.

Warden et al. (2021) frame bone stress injuries not as a consequence of load itself, but as a
workload dosing error. For training, this means we do not avoid fast paces; we dose them.
[[11]](https://pubmed.ncbi.nlm.nih.gov/33635519/)

![Load adaptation window](/images/diagrams/load-adaptation-window.svg)

## Daniels/VDOT

The Daniels/VDOT model originates in the work of Jack Daniels and Jimmy Gilbert, published in 1979
as *Oxygen Power: Performance Tables for Distance Runners*. VDOT is not a directly measured VO2max,
but an effective or pseudo-VO2max inferred from actual running performance. A race result is
translated into one performance value through running velocity, the oxygen cost of running, and the
fraction of VO2max that can be sustained for the race duration. Its main strength is coaching
consistency: a known VDOT maps to standardized zones such as Easy, Marathon, Threshold, Interval,
and Repetition.
[[10]](https://www.frontiersin.org/journals/psychology/articles/10.3389/fpsyg.2019.03026/full)

## Critical Speed

Critical Speed (CS) is the running-specific form of the critical-power concept. It describes the
asymptotic speed of the distance-time relationship and is physiologically interpreted as the
boundary between heavy and severe intensity: below CS, a metabolic steady state is more likely;
above CS, fatigue-related markers rise progressively until the effort must stop or speed must be
reduced. The finite distance capacity above CS is modelled as D'.
[[8]](https://www.frontiersin.org/journals/bioengineering-and-biotechnology/articles/10.3389/fbioe.2020.533391/full)
[[9]](https://www.frontiersin.org/journals/bioengineering-and-biotechnology/articles/10.3389/fbioe.2024.1378284/full)

## Conversion in PaceLetics

All calculations are performed internally with speed, because percentages on pace would be inverted.
For Daniels, the entered performance is matched against the VDOT table; the nearest table value
returns the classic pace table for Recovery, Easy, Marathon, Threshold, Interval, and Repetition. CS
is no longer interpreted as a second Daniels table. From the running data we estimate Critical Speed
and D': a single 1200 m test estimates CS conservatively as `0.84 * v1200`, and a single 3 km test
as `0.90 * v3k`. If 1200 m and 3600 m are available, we calculate `CS = (3600 - 1200) / (t3600 -
t1200)` and `D' = 1200 - CS * t1200`; with several distances we use regression.
[[2]](https://pmc.ncbi.nlm.nih.gov/articles/PMC11933073/)
[[8]](https://www.frontiersin.org/journals/bioengineering-and-biotechnology/articles/10.3389/fbioe.2020.533391/full)
[[9]](https://www.frontiersin.org/journals/bioengineering-and-biotechnology/articles/10.3389/fbioe.2024.1378284/full)

![Pace model](/images/diagrams/pace-model-belastungskurve.svg)

## Sources

1. [Anderson et al. (2026): The Measurement and Application of Critical Speed and D' in Running.](https://link.springer.com/article/10.1007/s40279-026-02410-x)
2. [Lipkova et al. (2025): Field-based tests for determining critical speed among runners and its practical application.](https://pmc.ncbi.nlm.nih.gov/articles/PMC11933073/)
3. [Hawley et al. (2018): Adaptations to Endurance and Strength Training.](https://pubmed.ncbi.nlm.nih.gov/28490537/)
4. [MacInnis & Gibala (2017): Physiological adaptations to interval training and the role of exercise intensity.](https://pubmed.ncbi.nlm.nih.gov/27748956/)
5. [Kubo et al. (2010): Time course of changes in muscle and tendon properties during strength training and detraining.](https://paulogentil.com/pdf/Time%20Course%20of%20Changes%20in%20Muscle%20and%20Tendon%20Properties%20During%20Strength%20Training%20and%20Detraining.pdf)
6. [Bohm et al. (2019): Functional Adaptation of Connective Tissue by Training.](https://www.germanjournalsportsmedicine.com/archive/archive-2019/issue-4/functional-adaptation-of-connective-tissue-by-training/)
7. [Bohm et al. (2015): Human tendon adaptation in response to mechanical loading.](https://link.springer.com/article/10.1186/s40798-015-0009-9)
8. [Papagiannaki et al. (2020): Running-Related Injury From an Engineering, Medical and Sport Science Perspective.](https://www.frontiersin.org/journals/bioengineering-and-biotechnology/articles/10.3389/fbioe.2020.533391/full)
9. [Jiang et al. (2024): Comparison of ground reaction forces as running speed increases between male and female runners.](https://www.frontiersin.org/journals/bioengineering-and-biotechnology/articles/10.3389/fbioe.2024.1378284/full)
10. [Billat et al. (2020): Pacing Strategy Affects the Sub-Elite Marathoner's Cardiac Drift and Performance.](https://www.frontiersin.org/journals/psychology/articles/10.3389/fpsyg.2019.03026/full)
11. [Warden et al. (2021): Preventing Bone Stress Injuries in Runners with Optimal Workload.](https://pubmed.ncbi.nlm.nih.gov/33635519/)
