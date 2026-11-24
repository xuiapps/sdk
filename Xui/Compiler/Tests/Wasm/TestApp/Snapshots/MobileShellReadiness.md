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
Unsupported Newobj at IL_0023 in System.Span<Char>.Slice: Unsupported signature type: Pointer.
```

## Dependency path

```text
Root { Value = Method { Identity = Program.Main } }
 → Method { Identity = Program.Main }
 → Method { Identity = Xui.Apps.TestApp.MobileTestAppShell..ctor }
 → Method { Identity = Xui.Apps.TestApp.MobileHomePage..ctor }
 → Method { Identity = Xui.Apps.TestApp.MobileHomeHeader..ctor }
 → Method { Identity = Xui.Apps.TestApp.MobileHomeHeader.ShowRating }
 → Method { Identity = Xui.Apps.TestApp.MobileRatingPanel..ctor }
 → Method { Identity = System.Runtime.CompilerServices.DefaultInterpolatedStringHandler.AppendLiteral }
 → Method { Identity = System.Span<Char>.Slice }
```

Generated event subscriptions and TimeSpan factories now pass dependency discovery. Duration conversion emits WASM tick arithmetic with managed NaN/range failures and English messages, without retaining System.SR or ResourceManager for those guards. The probe next reaches MobileRatingPanel's interpolated strings: DefaultInterpolatedStringHandler.AppendLiteral calls Span<Char>.Slice, whose pointer signature is unsupported. Static initialization, clock/services, collections, text measurement, and full event routing still require further probes.

## WAT

Not emitted.
