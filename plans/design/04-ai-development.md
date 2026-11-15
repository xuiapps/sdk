> WIP

# AI Development Readiness — Xui Framework

## Overview

This document captures Xui's current readiness for AI-assisted app development — how an AI
agent (Copilot, Claude, GPT-4o, Gemini, etc.) can contribute to building a production-quality
Xui application today, and what investments will close the remaining gaps.

---

## 1. Why Xui Is Well-Suited for AI

### 1.1 Web Standards Familiarity

Xui's `IContext` API mirrors the HTML5 Canvas 2D API closely:

```csharp
// Xui                          // HTML5 Canvas
ctx.BeginPath();                // ctx.beginPath()
ctx.Arc(cx, cy, r, 0, Math.PI * 2); // ctx.arc(cx, cy, r, 0, Math.PI * 2)
ctx.SetFill(Colors.Red);        // ctx.fillStyle = "red"
ctx.Fill();                     // ctx.fill()
```

Every major AI model has been trained on millions of lines of HTML5 Canvas code.  The
conceptual transfer is near-perfect.  An AI that knows Canvas can write Xui rendering code
with very little additional context.

### 1.2 Typed, Discoverable API

Xui is strongly typed with no reflection at runtime.  Method signatures, enums, and
generic constraints are all statically visible.  AI tools that use MCP or LSP can traverse
the type graph and produce correct code on the first attempt far more reliably than
reflection-heavy or stringly-typed frameworks.

### 1.3 Zero-Magic Architecture

There is no XAML compiler, no markup extension pipeline, no hidden code generation.  The
full execution path from a user gesture to a rendered pixel is traceable by reading C# code.
AI can reason about the full stack — event → view → canvas → platform — without needing to
understand a separate templating language.

### 1.4 SVG as a First-Class Interchange Format

`Xui.Runtime.Software` ships an `SvgDrawingContext` that implements `IContext` and produces
SVG output.  AI can:

- Generate design-intent SVGs from a natural-language description
- Feed those SVGs into test assertions (snapshot tests)
- Iteratively refine the rendering by reading back the SVG output

This makes the design → code → verify loop entirely text-based, which is the native medium
for AI agents.

### 1.5 `plans/` Folder Convention

The `plans/` folder (this folder) is a structured way to communicate design intent to both
humans and AI agents.  A `plans/design/` subfolder with:

- SVG artboards (from the design studio)
- Markdown acceptance criteria (this document family)

…gives an AI agent everything it needs to implement a feature end-to-end with only
occasional human clarification.

The target is **80% of a feature implemented correctly on the first AI pass**.

---

## 2. Current AI-Friendly Signals

| Signal | Status | Notes |
|---|---|---|
| Canvas API mirrors web standards | ✅ | Direct transfer from MDN Canvas docs |
| Strongly typed C# throughout | ✅ | No reflection, no stringly-typed config |
| XML doc comments on all public APIs | 🟡 Partial | Core and Runtime well-covered; Middleware improving |
| MCP server for Xui in `Xui/Utils/MCP` | ✅ | AI can query the live API surface |
| SVG snapshot testing | ✅ | `Xui.Tests.Component` uses SVG diffs |
| `plans/` folder convention established | ✅ | DI, GPU, design-system, and now design/ plans |
| Hot-reload support | 🟡 Partial | `IHotReload` interface exists; platform bindings in progress |
| Natural-language test specs | 🟡 Partial | Component tests exist; prose acceptance criteria being added |
| Example apps covering key patterns | 🟡 Partial | ClockApp, LoadTestApp, TestApp; BlankApp for playground |

---

## 3. The AI Development Loop

The intended workflow for an AI-assisted Xui feature:

```
1. Human writes a plan (plans/design/<feature>.md + optional SVG artboard)
        ↓
2. AI reads the plan + scans the IContext / View / DeviceProfile APIs via MCP
        ↓
3. AI generates a View subclass (OnMeasure, OnArrange, OnRender)
        ↓
4. AI generates a component test (SvgDrawingContext + snapshot)
        ↓
5. Human runs the test → SVG diff appears in CI
        ↓
6. Human or AI adjusts until snapshot matches the design artboard
        ↓
7. Feature is merged; hot-reload allows the designer to tweak live
```

Each step is tractable with current tooling.  Steps 2–4 can be fully automated.
Steps 5–6 require human eyes but AI can accelerate them by auto-suggesting fixes
from the diff.

---

## 4. Gap Analysis & Roadmap

### 4.1 XML Documentation Coverage

**Gap:** Several middleware and runtime types lack XML doc comments, which reduces the
accuracy of AI-generated code.

**Action:** Audit and fill XML doc comments in:
- `Xui.Middleware.EmulatorLegacy.*`
- `Xui.Runtime.Software.*`
- `Xui.Core.UI.*` (View subclasses, layout primitives)

### 4.2 Prose Acceptance Criteria

**Gap:** Tests assert the SVG output matches a snapshot, but do not describe *what* the
snapshot should look like in natural language.

**Action:** Add a `/// <remarks>` block to each component test method describing the expected
visual in one sentence.  AI tools can use this to regenerate a test if the snapshot is
invalidated.

### 4.3 Design → Code Token Bridge

**Gap:** Design tokens from the studio (colours, radii, spacing) exist in JSON but are not
yet consumed by a typed `IDesignSystem` in `Xui.Core`.

**Action:** Complete the `IDesignSystem` interface described in `plans/design-system.md` and
wire it to the token JSON files in `plans/design-system/`.  AI can then reference the token
names directly when writing rendering code.

### 4.4 Scaffold Templates

**Gap:** `dotnet new xui-app` template does not yet exist.

**Action:** Create a minimal `Xui.Templates` NuGet package with:
- `xui-app` — minimal app (Application + Window + HelloView)
- `xui-view` — bare View subclass with `OnMeasure` / `OnArrange` / `OnRender` stubs
- `xui-demo` — demo module shell matching the Technology Preview app structure (§03)

AI tools can call `dotnet new xui-view -n GaugeView` and get a correctly structured
starting point with no hallucination risk.

### 4.5 Inline Canvas Previews in IDE

**Gap:** Developers (and AI agents using IDE tools) cannot see a preview of a View's
rendered output without running the full app.

**Action:** Expose an MSBuild target `xui:preview` that renders a named View to SVG using
`SvgDrawingContext` and writes it to `obj/preview/<ViewName>.svg`.  IDEs and AI tools can
call this target to get a live preview without a GPU.

---

## 5. Reachable Milestone: AI Builds a Full Demo Module

**Target:** An AI agent, given only the plan documents in `plans/design/`, should be able to
implement a complete demo module from §03 (e.g. the Health Monitor) including:

- The View hierarchy (root + child views)
- Simulated data model
- Canvas rendering of charts and gauges
- Touch input handling
- A snapshot test suite

…with zero human-written code, subject only to a final human review pass.

This milestone validates that the `plans/` → code pipeline is working end-to-end.

**Estimated readiness:** Q3 2025 (pending doc coverage, token bridge, and templates — §4.1–4.4)

---

## 6. Design Deliverable Impact on AI Readiness

The design deliverables from the studio (SVG artboards + token JSON) directly accelerate
AI development by:

| Deliverable | How it helps AI |
|---|---|
| SVG artboards | AI can read SVG geometry and derive canvas draw calls |
| Annotated Figma layers with code names | AI maps layer names to View/IContext method names |
| Token JSON (colours, radii, spacing) | AI uses exact values instead of hallucinating colours |
| Motion spec (timing, spring params) | AI generates correct `Easing` / `AnimationCurve` calls |
| Acceptance criteria in markdown | AI generates matching test assertions |

---

## 7. Summary

Xui's design philosophy — web-standard Canvas API, strongly typed C#, SVG-testable abstract
layer, and the `plans/` folder convention — positions it as a **natively AI-friendly framework**.

The main remaining work is documentation completeness, design tokens, and scaffolding
templates.  Once those are in place, AI agents should be able to contribute 80% of a
feature end-to-end from a plan document, with the human role shifting to design authorship,
final review, and product direction.
