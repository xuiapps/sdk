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
Required signature modifier is not implemented: System.Runtime.CompilerServices.IsVolatile.
```

## Dependency path

```text
Root { Value = Method { Identity = Program.Main } }
 → Method { Identity = Program.Main }
 → Method { Identity = Xui.Apps.TestApp.MobileTestAppShell..ctor }
 → Method { Identity = Xui.Apps.TestApp.MobileDemoMenu..ctor }
 → Method { Identity = Xui.Apps.TestApp.MobileDemoMenu.AddPerformance }
 → Method { Identity = Xui.Apps.TestApp.Pages.Floorplan.FloorplanDrawing.Preload }
 → Method { Identity = Xui.Apps.TestApp.Pages.Floorplan.FloorplanDrawing.LoadSharedAsync }
 → Method { Identity = System.Lazy<System.Threading.Tasks.Task<Xui.Apps.TestApp.Pages.Floorplan.FloorplanDrawing>>.get_Value }
```

Generated event subscriptions and TimeSpan factories now pass dependency discovery. Duration conversion emits WASM tick arithmetic with managed NaN/range failures and English messages, without retaining System.SR or ResourceManager for those guards. The rating panel's string interpolation now uses mutable UTF-16 GC buffers and integer formatting without retaining the BCL handler body. Closed generic interface methods now specialize their implementations and use distinct dispatch slots, including the demo navigation call. Default string/scalar equality now specializes without BCL factories. Class default equality and array search now preserve identity and virtual Equals overrides. The current full-catalogue blocker is optional floorplan preloading through Lazy/async and its IsVolatile signature modifier. Static initialization, clock/services, collections, text measurement, and full event routing still require further probes.

## WAT

Not emitted.
