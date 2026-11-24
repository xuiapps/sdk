# Full TestApp mobile shell: compiler readiness

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
var metrics = new MobilePerformanceMetrics(new LayoutFrameInstruments());
var shell = new MobileTestAppShell(metrics);
return shell.Count;
```

## Compiler failure

```text
Generic virtual method dispatch is not implemented: Xui.Apps.TestApp.Pages.IExampleNavigation.PushExamplePage<Xui.Apps.TestApp.Pages.Canvas.CanvasTestsExample>.
```

## Dependency path

```text
Root { Value = Method { Identity = Program.Main } }
 → Method { Identity = Program.Main }
 → Method { Identity = Xui.Apps.TestApp.MobileTestAppShell..ctor }
 → Method { Identity = Xui.Apps.TestApp.MobileHomePage..ctor }
 → Method { Identity = Xui.Apps.TestApp.MobileDemoMenu..ctor }
 → Method { Identity = Xui.Apps.TestApp.MobileDemoMenu.AddCanvas }
 → Method { Identity = Xui.Apps.TestApp.MobileDemoCategoryPanel.AddExample<Xui.Apps.TestApp.Pages.Canvas.CanvasTestsExample> }
 → Method { Identity = Xui.Apps.TestApp.MobileDemoButton<Xui.Apps.TestApp.Pages.Canvas.CanvasTestsExample>..ctor }
 → Method { Identity = Xui.Apps.TestApp.MobileDemoButton<Xui.Apps.TestApp.Pages.Canvas.CanvasTestsExample>.NavigateToExample }
```

Generated event subscriptions and TimeSpan factories now pass dependency discovery. Duration conversion emits WASM tick arithmetic with managed NaN/range failures and English messages, without retaining System.SR or ResourceManager for those guards. The rating panel's string interpolation now uses mutable UTF-16 GC buffers and integer formatting without retaining the BCL handler body. The probe next reaches IExampleNavigation.PushExamplePage<CanvasTestsExample> from MobileDemoButton.NavigateToExample, which requires generic interface method dispatch. Static initialization, clock/services, collections, text measurement, and full event routing still require further probes.

## WAT

Not emitted.
