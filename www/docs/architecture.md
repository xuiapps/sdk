---
title: Architecture
description: The Abstract/Actual split, service resolution chain, and middleware layers.
---

# Architecture

Xui is organized as three layers stacked vertically:

```
┌─────────────────────────────────────────┐
│              Application Code           │  ← your Views, Windows, Pages
├─────────────────────────────────────────┤
│           Xui.Core  (Abstract)          │  ← Window, View, IContext, IImage …
├───────────────┬──────────────┬──────────┤
│  Middleware   │  Emulator    │  Tests   │  ← SvgDrawingContext, TestApp …
├───────────────┴──────────────┴──────────┤
│         Platform Runtime (Actual)       │  ← Win32Window, Direct2DContext …
│   Windows | macOS | iOS | Browser      │
└─────────────────────────────────────────┘
```

## Abstract layer — `Xui.Core`

`Xui.Core` has **zero platform dependencies** (no NuGet packages, no P/Invoke).

Key types:
- `Window` — abstract window; holds `RootView`, routes input, manages lifecycle.
- `View` — base class for all UI elements; participates in layout and rendering.
- `IContext` — the aggregate 2D drawing interface, modeled after HTML5 Canvas.
- `IImagePipeline` / `IImage` — image loading abstraction.

The abstract layer defines *what* the platform must provide, not *how*.

## Actual layer — platform runtimes

Each platform package (`Xui.Runtime.Windows`, `Xui.Runtime.macOS`, …) implements:
- `IWindow` — wraps the OS window (HWND, NSWindow, UIWindow…).
- `IContext` → `Direct2DContext` / `CoreGraphicsContext` / `CanvasContext` — the drawing implementation.
- `IImagePipeline` → `DirectXImageFactory` / `CGImageFactory` — decodes and GPU-uploads images.

**Direction rule:** abstract calls into actual. Actual never calls back into abstract.

## Service resolution chain

Platform services (images, text measurement) are exposed to views through one
forward-only service chain:

```
view.GetService<IImage>()
  → parent.GetService()
  → ... (walks up the view tree)
  → RootView.GetService()
  → Window.GetService()
      → middleware actual windows (if present)
      → native actual window  ← Win32Window returns IImage via DirectXImageFactory
      → window DI scope / application services
```

Generic helpers in `Xui.Core.DI`:
```csharp
IImage image = this.GetService<IImage>();           // null if not found
IImage image = this.GetRequiredService<IImage>();   // throws if not found
```

## Middleware

Middleware sits between abstract and actual and swaps out the platform implementation:

| Middleware | Purpose |
|---|---|
| `Xui.Middleware.EmulatorLegacy` | Renders the app inside a device frame inside another app (dev tool) |
| `Xui.Runtime.Software` | CPU-side SVG renderer (`SvgDrawingContext`) used for snapshot testing |

## DI integration (`Xui.Core.DI`)

`Xui.Core.DI` adds optional Microsoft.Extensions.Hosting integration:
- `Run<TApplication>()` starts the host and then the Xui application.
- `CreateAndShowOnce<TWindow>()` creates one DI scope per window and disposes it on close.
- Actual-window and middleware services are resolved before the window's DI scope;
  native callbacks never resolve services back through the abstract window.
