# Xui

Xui is a cross-platform .NET UI framework. Application code is written against a
platform-independent core; runtime projects connect that core to Windows, macOS,
iOS, Android, the browser, or deterministic software/test implementations.

Most projects target .NET 10. `Xui.CompileTime` targets `netstandard2.0` because it
is a Roslyn component. The repository solution is `Xui.sln`.

## Scoped guidance

Before changing one of these areas, read its local instructions in addition to
this file:

- `Xui/Runtime/AGENTS.md` — platform backends, native interop, and runtime parity.
- `Xui/Tests/AGENTS.md` — test-layer selection, deterministic harnesses, and
  snapshot handling.
- `Xui/Apps/AGENTS.md` — shared app sources and platform project composition.
- `www/AGENTS.md` — authored documentation, generated assets, and DocFX.

## Architecture

### Abstract, Actual, and Middleware

The central boundary is in `Xui/Core/Core`:

- `Xui.Core.Abstract` contains the application-facing `Application`, `Window`,
  window contracts, lifecycle, and input event contracts.
- `Xui.Core.Actual` contains the interfaces a platform must implement:
  `IRuntime`, `IRunLoop`, `IDispatcher`, `IWindow`, and image services.
- A concrete `IRuntime` creates an Actual window for an Abstract window. Native
  events travel from the Actual window into the Abstract window; invalidation,
  title changes, keyboard requests, and other operations travel back to Actual.
- `Xui.Core.Middleware` defines components that implement both sides of the
  boundary. Middleware can sit between an app window and a real runtime to
  transform events or rendering. The emulator and DevTools use this pattern.

The usual object flow is:

```text
Application
  -> IRuntime / IRunLoop
  -> Abstract.Window <-> optional Middleware <-> Actual.IWindow
  -> RootView -> View tree
```

### UI, input, and rendering

- `Xui.Core.UI.RootView` connects an Abstract window to its `View` hierarchy.
- Native mouse, touch, keyboard, and frame callbacks enter
  `Xui.Core.Abstract.Window`, then pass to `RootView`.
- `Xui.Core.UI.Input.EventRouter` performs view-tree hit testing and event
  routing. Input and window event carriers are short-lived `ref struct` values.
- Rendering is driven by invalidation. A platform supplies
  `Xui.Core.Canvas.IContext`; the Abstract window updates and renders its
  `RootView` into that context.
- `Xui.Core.Canvas` is the Web-like 2D drawing and text-measure contract used by
  native, browser, SVG, and software contexts.
- `Xui.Core.UI.Layout` contains FlexBox and Grid. Basic views, layers, focus,
  overlays, text input, and view lifecycle code also live under `Xui.Core.UI`.
- `Xui.Core.GPU` contains the experimental GPU API, shader types, intermediate
  representation, and HLSL/MSL backends. `Xui.CompileTime` supplies the Roslyn
  compile-time side of this shader pipeline.

## Project map

### Core

- `Xui/Core/Core/Xui.Core.csproj` — foundational abstractions and shared
  implementation. Important areas include `Abstract`, `Actual`, `Animation`,
  `Canvas`, `Curves2D`, `Curves3D`, `GPU`, `Math1D`, `Math2D`, `Math3D`,
  `Memory`, `Middleware`, `Set`, and `UI`.
- `Xui/Core/Core.DI/Xui.Core.DI.csproj` — integration with
  `Microsoft.Extensions.Hosting` and application/window dependency injection.
- `Xui/Core/Fonts/Xui.Core.Fonts.csproj` — bundled Inter font assets used for
  consistent rendering and software text measurement.
- `Xui/Core/Core.Tests/Xui.Core.Tests.csproj` — xUnit tests for Core math, curves,
  UI layout, GPU types, shader generation, and software GPU behavior.
- `Xui/CompileTime/Xui.CompileTime.csproj` — Roslyn source generator/analyzer for
  GPU shader discovery and emission. It links the shared GPU IR and backend
  sources from Core.

### Platform runtimes

Each primary runtime implements the Actual contracts and references only Core:

- `Xui/Runtime/Windows/Windows.csproj` — Win32 windowing/input plus COM wrappers
  for Direct2D, Direct3D 11, DirectComposition, DirectWrite, DXGI, and WIC.
- `Xui/Runtime/MacOS/MacOS.csproj` — AppKit and Objective-C interop with
  CoreFoundation, CoreGraphics, CoreText, CoreAnimation, Metal, and dispatch
  wrappers.
- `Xui/Runtime/IOS/IOS.csproj` — UIKit and Objective-C interop with
  CoreFoundation, CoreGraphics, CoreText, CoreAnimation, and Metal wrappers.
- `Xui/Runtime/Android/Android.csproj` — Android Activity/View integration and
  Android Canvas bindings. This runtime targets `net10.0-android`.
- `Xui/Runtime/Browser/Browser.csproj` — browser/WASM runtime backed by DOM and
  Canvas APIs.
- `Xui/Runtime/Software/Xui.Runtime.Software.csproj` — deterministic managed
  implementations used by tests and tooling: SVG drawing, font parsing and text
  measurement, tessellation, rasterization, bitmap rendering, and software GPU.

Supporting runtimes:

- `Xui/Runtime/Test/Xui.Runtime.Test.csproj` — deterministic application/window
  harness built on Core, Software, Fonts, DI, and Emulator.
- `Xui/Runtime/E2E/Xui.Runtime.E2E.csproj` — client-side harness for driving a
  real application through the DevTools protocol.

### Middleware and developer tooling

- `Xui/Middleware/EmulatorLegacy/Xui.Middleware.EmulatorLegacy.csproj` — wraps a desktop
  runtime, draws mobile device chrome/safe areas, and translates desktop input
  into emulated mobile input.
- `Xui/Middleware/DevTools/Xui.Middleware.DevTools.csproj` — runtime middleware
  for UI-tree inspection, SVG screenshot capture, synthetic input, and named-pipe
  control of debug desktop apps.
- `Xui/Middleware/DevTools.Client/Xui.Middleware.DevTools.Client.csproj` —
  protocol client shared by E2E infrastructure and the MCP server. This project
  exists in the repository but is not currently listed in `Xui.sln`.
- `Xui/DevKit/UI/Xui.DevKit.UI.csproj` — higher-level design-system primitives
  and widgets built on Core UI.
- `Xui/DevKit/ThreeD/Xui.DevKit.ThreeD.csproj` — higher-level 3D development
  helpers built on Core.
- `Xui/DevKit/UI.Tests/Xui.DevKit.UI.Tests.csproj` and
  `Xui/DevKit/ThreeD.Tests/Xui.DevKit.ThreeD.Tests.csproj` — their xUnit tests.

### Test projects

- `Xui/Tests/Component/Xui.Tests.Component.csproj` — component and rendering
  tests using Core, TestApp, DevKit UI, and the Software runtime; includes SVG
  and image snapshots.
- `Xui/Tests/Integration/Xui.Tests.Integration.csproj` — deterministic TestApp
  scenarios driven through `Xui.Runtime.Test`, including desktop and emulated
  device variants. Expected SVGs live beside scenario README files.
- `Xui/Tests/E2E/Xui.Tests.E2E.csproj` — drives a real TestApp process through
  `Xui.Runtime.E2E` and DevTools.
- `Xui/Tests/Docs/Xui.Tests.Docs.csproj` — generates SVG figures used by the
  documentation.

### Apps

Apps share source within each app directory and select runtime references through
their local `.targets` files.

- `Xui/Apps/BlankApp` — minimal playground, with Desktop, Emulator, Browser, and
  Mobile project variants.
- `Xui/Apps/TestApp` — framework feature and interaction test bed, with Desktop,
  Emulator, and Mobile variants. Component and integration tests reuse it.
- `Xui/Apps/LoadTestApp` — scrolling/rendering load and performance test app,
  with Desktop, Emulator, Browser, and Mobile variants.
- `Xui/Apps/ClockApp` — low-level window/render-loop example for desktop.
- `Xui/Apps/XuiSDK` — SDK/design exploration app with Desktop, Emulator, and
  Mobile variants.

Desktop projects select Windows or macOS from the host OS. Emulator projects add
the Emulator middleware over that desktop runtime. Mobile projects target Android
and iOS through MAUI host projects. Browser projects use Blazor WebAssembly.
Debug desktop variants can insert DevTools middleware.

### Utilities and ancillary tools

- `Xui/Utils/Templates/Xui.Templates.csproj` — package containing Xui project
  templates. Projects below `content/` are template payloads, not solution
  projects.
- `Xui/Utils/MCP/Xui.MCP.csproj` — stdio MCP server that starts or connects to
  DevTools-enabled Xui apps and exposes inspection, screenshots, input, and logs.
- `tools/DocsGenerator` — documentation asset generator.
- `tools/XuiApiExport` — Roslyn-based public API export utility.
- `tools/GlyphLoader`, `tools/PngEncoder`, and `tools/TesselatorPlayground` —
  experimental software-rendering/font utilities. These tools are outside
  `Xui.sln`; verify their project references before relying on them.

## Repository-wide implementation rules

- Use `NFloat` or the `nfloat` alias for UI coordinates, dimensions, angles, and
  other values that would otherwise be `float` or `double`.
- Preserve allocation-sensitive paths. Input events are `ref struct` values and
  should travel through window/view dispatch without heap event objects.
- Keep the Abstract/Actual dependency direction intact. Core defines contracts;
  runtimes implement them. Do not introduce platform dependencies into Core.
- A change to an Actual contract or Canvas operation may require updates across
  every runtime, middleware wrapper, the Software runtime, and test doubles.
  Search for all implementers before editing the contract.
- Platform support is not uniformly complete. Do not infer parity from an
  interface alone; inspect the relevant implementation for unsupported members.
- Native interop code is intentionally unsafe and resource ownership is part of
  the API design:
  - Single-call borrowed or temporarily retained handles use `ref struct`,
    commonly with a `Ref` suffix or nested `Ref`, and dispose within the call.
  - Owned handles that span several calls use a disposable `struct`, commonly
    with a `Ptr` suffix, and are disposed by their owning object.
  - Long-lived native objects use classes wrapping Objective-C or COM pointers
    and must respect native retain/release or AddRef/Release semantics.
- Prefer a focused project build/test while iterating. Use the full solution only
  when the change crosses project or platform boundaries; some app targets
  require optional platform workloads.
- Release builds enable stricter warning behavior in important projects. Do not
  assume a successful Debug build is sufficient for package-facing changes.

## Common commands

```sh
# Core library and unit tests
dotnet build Xui/Core/Core/Xui.Core.csproj
dotnet test Xui/Core/Core.Tests/Xui.Core.Tests.csproj

# Higher-level suites
dotnet test Xui/Tests/Component/Xui.Tests.Component.csproj
dotnet test Xui/Tests/Integration/Xui.Tests.Integration.csproj
dotnet test Xui/Tests/E2E/Xui.Tests.E2E.csproj

# Full solution; may require installed mobile/WASM workloads
dotnet build Xui.sln
```

When changing snapshot-producing tests, review the generated SVG/image diff
rather than accepting regenerated snapshots solely because the test runner
produced them.
