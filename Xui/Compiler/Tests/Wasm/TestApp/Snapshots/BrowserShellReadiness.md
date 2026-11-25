# Browser TestApp catalogue: compiler readiness

Status: **WASM not emitted.** This is an expected-unsupported regression, not an execution test.

## Inputs

The complete repository sources are compiled into separate PEs. TestApp uses its production shader generator; desktop startup and Platform.cs are excluded so the fixture can select the mobile shell directly. Core/emulator use the documented Canvas compiler profile.

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

## Compiler failure

```text
Static field stack analysis is not implemented except TimeSpan.Zero and Stopwatch constants. Field: SemiBold.
Method: Xui.Apps.TestApp.MobileDemoCategoryPanel..ctor
Instruction: Ldsfld at IL_0037
```

## Dependency path

```text
Reachability completed; failure occurred during method planning/emission.
```

This probe injects a Canvas-only catalogue into the real MobileTestAppShell. It retains the real home/header, rating controls, navigation, and mobile page wrapper. The full-menu probe remains separate and includes optional async/GPU/resource showcases. This is not an execution claim.

## WAT

Not emitted.
