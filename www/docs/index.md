---
title: Introduction
description: What Xui is, what it is not, and why it exists.
---

# Xui

Xui is a native .NET UI application framework built around a **Canvas-based 2D drawing API**.

It targets Windows, macOS, iOS, Android, and Browser from a single C# codebase — compiled ahead-of-time with the .NET AoT compiler, with no reflection at runtime.

## What Xui is

- A **thin, high-performance rendering layer** over each platform's native 2D API (Direct2D, CoreGraphics, Canvas).
- A **layout engine** with a `View` tree that drives measure, arrange, and render passes.
- A **cross-platform input model** unified over pointer, keyboard, and scroll events.
- A **testable architecture** — the abstract layer has no platform dependencies and can be driven by an SVG renderer in unit tests.

## What Xui is not

- Not XAML. There are no XML templates or data-binding pipelines.
- Not Blazor. There is no DOM, no JavaScript interop, no HTML.
- Not WinUI, MAUI, or Avalonia. No shared control library built on platform controls.
- Not a game engine. Though the Canvas API is similar to HTML5 Canvas, the layout system is designed for app UIs.

## Key design choices

**Abstract / Actual split.** The core framework (`Xui.Core`) has zero platform dependencies. Platform runtimes (`Xui.Runtime.Windows`, `Xui.Runtime.macOS`, …) implement the actual interfaces. This makes the entire UI layer unit-testable without a GPU.

**No boxing on the hot path.** Fill styles (`LinearGradient`, `ImagePattern`) are `ref struct` values passed directly to the context methods — never boxed through an interface.

**Service chain instead of static globals.** Views request platform services (`IImage`, `ITextMeasureContext`) through `IServiceProvider` walking up the view tree to the window, then to the platform runtime. No statics, no singletons in the core.

## Next steps

- [Getting Started](getting-started.md) — create and run your first Xui app.
- [Architecture](architecture.md) — understand the Abstract/Actual split in depth.
- [Canvas API](canvas.md) — learn the drawing primitives.
