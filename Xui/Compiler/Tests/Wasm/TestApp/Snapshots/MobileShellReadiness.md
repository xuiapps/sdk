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
Default equality is not implemented for Xui.Core.UI.View.
```

## Dependency path

```text
Root { Value = Method { Identity = Program.Main } }
 → Method { Identity = Program.Main }
 → Method { Identity = Xui.Apps.TestApp.LayoutFrameInstruments..ctor }
 → Method { Identity = Xui.Apps.TestApp.LayoutFrameInstruments..ctor }
 → Method { Identity = System.Collections.Generic.HashSet<Xui.Core.UI.View>..ctor }
 → Method { Identity = System.Collections.Generic.HashSet<Xui.Core.UI.View>..ctor }
 → Method { Identity = System.Collections.Generic.HashSet<Xui.Core.UI.View>..ctor }
```

Generated event subscriptions and TimeSpan factories now pass dependency discovery. Duration conversion emits WASM tick arithmetic with managed NaN/range failures and English messages, without retaining System.SR or ResourceManager for those guards. The rating panel's string interpolation now uses mutable UTF-16 GC buffers and integer formatting without retaining the BCL handler body. Closed generic interface methods now specialize their implementations and use distinct dispatch slots, including the demo navigation call. Default string/scalar equality now specializes without BCL factories. The current full-catalogue blocker is default View equality reached by the diagnostic HashSet<View>. Optional floorplan preloading also retains known unsupported Lazy/async dependencies. Static initialization, clock/services, collections, text measurement, and full event routing still require further probes.

## WAT

Not emitted.
