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
Unsupported local type in System.Number.NegativeInt32ToDecStr: Unsupported signature type: Pinned.
```

## Dependency path

```text
Root { Value = Method { Identity = Program.Main } }
 → Method { Identity = Program.Main }
 → Method { Identity = Xui.Apps.TestApp.BrowserMenu..ctor }
 → Method { Identity = Xui.Core.UI.View.AddProtectedChild }
 → Method { Identity = Xui.Core.UI.View.ActivateSubtree }
 → VirtualCall { Identity = Xui.Core.UI.View.OnActivate }
 → Method { Identity = Xui.Apps.TestApp.MobileHomeHeader.OnActivate }
 → Method { Identity = Xui.Apps.TestApp.MobileHomeHeader.UpdateFrameRate }
 → Method { Identity = Xui.Apps.TestApp.MobileHomeActionButton.SetFrameRate }
 → Method { Identity = System.Int32.ToString }
 → Method { Identity = System.Number.FormatInt32 }
 → Method { Identity = System.Number.NegativeInt32ToDecStr }
```

This probe injects a Canvas-only catalogue into the real MobileTestAppShell. It retains the real home/header, rating controls, navigation, and mobile page wrapper. The full-menu probe remains separate and includes optional async/GPU/resource showcases. This is not an execution claim.

## WAT

Not emitted.
