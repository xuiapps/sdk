# Browser TestApp catalogue: construction

Status: **Real mobile shell constructs in WASM and returns successfully in Node.**

## Inputs

Complete repository sources are compiled into separate PEs. TestApp uses its production shader generator; desktop startup and Platform.cs are excluded. Core/emulator use the documented Canvas compiler profile.

- `Xui.Core`
- `Xui.Core.DI`
- `Xui.Core.Fonts`
- `Xui.DevKit.UI`
- `Xui.Runtime.Software`
- `Xui.Middleware.Emulator`
- `TestApp.Mobile`

Framework implementation PEs: `System.Private.CoreLib`, `System.Runtime`, `System.Runtime.InteropServices`, `System.ComponentModel`.

## Entry

```csharp
var metrics = new MobilePerformanceMetrics(LayoutFrameInstruments.CreateCountersOnly());
var shell = new MobileTestAppShell(metrics, new BrowserMenu());
return shell.Count;
```

## Output

No console output.

## Exit Code

```text
1
```

The shell reports one navigation child. This executes the real home/header, rating controls, Canvas-only catalogue, and navigation constructors, including static initializers and BCL collection code. Optional async/GPU/resource showcases remain in the separate full-menu probe.

This is a construction gate, not yet a rendered or interactive full-shell browser test. The next gate must supply Canvas measurement/drawing, arrange and render the shell, then attach the real window/event-routing lifecycle.

## WASM

Emitted, validated and executed by Node with the shared JavaScript runtime. Full application-sized WAT is omitted here; focused instruction/feature snapshots retain their WAT.
