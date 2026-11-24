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
Unsupported signature type: Pointer.
```

## Dependency path

```text
Root { Value = Method { Identity = Program.Main } }
 → Method { Identity = Program.Main }
 → Method { Identity = Xui.Apps.TestApp.BrowserMenu..ctor }
 → Method { Identity = Xui.Apps.TestApp.MobileDemoCategoryPanel.AddExample<Xui.Apps.TestApp.BrowserCanvasExample> }
 → Method { Identity = Xui.Apps.TestApp.MobileDemoButton<Xui.Apps.TestApp.BrowserCanvasExample>..ctor }
 → Method { Identity = Xui.Apps.TestApp.MobileDemoButton<Xui.Apps.TestApp.BrowserCanvasExample>.NavigateToExample }
 → VirtualCall { Identity = Xui.Apps.TestApp.Pages.IExampleNavigation.PushExamplePage<Xui.Apps.TestApp.BrowserCanvasExample> }
 → Method { Identity = Xui.Apps.TestApp.MobileNavigation.PushExamplePage<Xui.Apps.TestApp.BrowserCanvasExample> }
 → Method { Identity = Xui.Apps.TestApp.Pages.MobileExamplePage<Xui.Apps.TestApp.BrowserCanvasExample>..ctor }
 → Method { Identity = Xui.Apps.TestApp.BrowserCanvasExample..ctor }
 → Method { Identity = Example..ctor }
 → Method { Identity = System.Object.GetType }
 → Method { Identity = System.Runtime.CompilerServices.RuntimeHelpers.GetMethodTable }
```

This probe injects a Canvas-only catalogue into the real MobileTestAppShell. It retains the real home/header, rating controls, navigation, and mobile page wrapper. The full-menu probe remains separate and includes optional async/GPU/resource showcases. This is not an execution claim.

## WAT

Not emitted.
