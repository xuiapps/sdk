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
Unsupported local type in System.Array.Copy: Unsupported signature type: Pointer.
```

## Dependency path

```text
Root { Value = Method { Identity = Program.Main } }
 → Method { Identity = Program.Main }
 → Method { Identity = Xui.Apps.TestApp.MobileTestAppShell..ctor }
 → Method { Identity = Xui.Apps.TestApp.MobileNavigation..ctor }
 → Method { Identity = Xui.DevKit.UI.Mobile.Navigation.NavigationView..ctor }
 → VirtualCall { Identity = System.Collections.Generic.List<Xui.Core.UI.View>.Add }
 → Method { Identity = System.Collections.Generic.List<Xui.Core.UI.View>.Add }
 → Method { Identity = System.Collections.Generic.List<Xui.Core.UI.View>.AddWithResize }
 → Method { Identity = System.Collections.Generic.List<Xui.Core.UI.View>.Grow }
 → Method { Identity = System.Collections.Generic.List<Xui.Core.UI.View>.set_Capacity }
 → Method { Identity = System.Array.Copy }
```

This probe injects a Canvas-only catalogue into the real MobileTestAppShell. It retains the real home/header, rating controls, navigation, and mobile page wrapper. The full-menu probe remains separate and includes optional async/GPU/resource showcases. This is not an execution claim.

## WAT

Not emitted.
