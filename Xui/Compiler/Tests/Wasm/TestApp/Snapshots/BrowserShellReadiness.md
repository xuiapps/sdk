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
The method has no managed body: System.Math.Pow.
```

## Dependency path

```text
Root { Value = Method { Identity = Program.Main } }
 → Method { Identity = Program.Main }
 → Method { Identity = Xui.Apps.TestApp.MobileTestAppShell..ctor }
 → Method { Identity = Xui.Apps.TestApp.MobileHomePage..ctor }
 → Method { Identity = Xui.Apps.TestApp.MobileHomePage.<.ctor>b__1_0 }
 → Method { Identity = Xui.Apps.TestApp.MobileHomeHeader.SetScrollOffset }
 → Method { Identity = Xui.Apps.TestApp.MobileHomeHeader.UpdateTransforms }
 → Method { Identity = Xui.Core.Animation.Easing.EaseInOutCubic }
 → Method { Identity = System.Runtime.InteropServices.NFloat.Pow }
 → Method { Identity = System.Double.Pow }
 → Method { Identity = System.Math.Pow }
```

This probe injects a Canvas-only catalogue into the real MobileTestAppShell. It retains the real home/header, rating controls, navigation, and mobile page wrapper. The full-menu probe remains separate and includes optional async/GPU/resource showcases. This is not an execution claim.

## WAT

Not emitted.
