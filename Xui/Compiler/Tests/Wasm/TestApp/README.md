# Real TestApp rendering gate

[Open FillRect](Snapshots/FillRectRender.html) directly in Chromium. The file embeds
the JavaScript host and the Xui-generated WASM; it needs no server, network, or .NET
runtime. [The snapshot](Snapshots/FillRectRender.md) contains the driver, unchanged
TestApp drawing source, Canvas commands, exit code, retained methods, and WAT.

The test compiles TestApp's actual `FillRectTest.cs` and `ICanvasDrawingExample.cs`
into one PE, compiles a driver/Canvas adapter into another, and links both with Core
and selected framework implementation PEs. Core is a separate assembly, compiled
from its complete source tree. The application executes the real
`View.Measure → Arrange → Render` path, including virtual dispatch, caching checks,
culling, default drawing state, and render cleanup. Only the explicit
`TestAppCanvas.NativeCanvas.Command` host boundary is rewritten into a JS import.

## Explicit first-frame profile

`Frontend.CoreCanvasProfile` defines `XUI_COMPILER_CANVAS` when compiling Core:

- Void instrumentation calls and their arguments are omitted through Conditional
  attributes on InstrumentsAccessor. Interpolated diagnostic handlers and reflection
  therefore do not enter the reachable graph. This does not disable all possible
  diagnostics; `Trace` is outside this first-frame path.
- Font's default weight and stretch construct the same immutable 400/100 values
  directly, avoiding the unsupported static fields `FontWeight.Normal` and
  `FontStretch.Normal`. General static storage/type initialization is still unsupported.

Normal Core builds do not define this symbol. No View method or TestApp drawing
source is substituted. The existing construction gate still tests the normal Core
implementation without this profile.

The minimal adapter supports the state operations and solid rectangles used here.
Unsupported drawing/measurement methods throw; font handling is limited to upright,
normal-stretch fonts. This is one real TestApp view, not the full application's
navigation, Window/RootView lifecycle, input, or text-rendering coverage.

## Verification

The driver arranges a 400×240 view at (20,30), then renders five rectangles. Debug
and Release application/view PEs must match CLR execution loaded into an isolated
assembly context with the same Core profile. Numeric comparisons account for the
compiler's f32 NFloat target versus the host CLR's f64 NFloat. The test checks an
exit code of zero, five rectangles, and Save/Restore surrounding the render.

```sh
dotnet test Xui/Compiler/Tests/Xui.Compiler.Tests.csproj -c Release --filter FullyQualifiedName~TestApp
node Xui/Compiler/Tests/Wasm/Canvas/browser.mjs
```

The shared browser runner checks offline execution, exact recorded commands,
rectangle colours/overlap/position, reruns, and a narrow viewport. It uses the
playground's installed Playwright dependency; see the Canvas test README for setup.
