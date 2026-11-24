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
Catch type is not a supported built-in exception: System.Resources.MissingManifestResourceException.
```

## Dependency path

```text
Root { Value = Method { Identity = Program.Main } }
 → Method { Identity = Program.Main }
 → Method { Identity = Xui.Apps.TestApp.MobilePerformanceMetrics..ctor }
 → Method { Identity = Xui.Apps.TestApp.MobileFrameRate..ctor }
 → Method { Identity = System.TimeSpan.FromSeconds }
 → Method { Identity = System.TimeSpan.Interval }
 → Method { Identity = System.ThrowHelper.ThrowArgumentException_Arg_CannotBeNaN }
 → Method { Identity = System.SR.get_Arg_CannotBeNaN }
 → Method { Identity = System.SR.GetResourceString }
```

Generated event subscriptions now execute using reference CompareExchange in the single-agent runtime. The probe also supplies IServiceProvider's implementation assembly. Dependency discovery next reaches MobileFrameRate's TimeSpan.FromSeconds call: its NaN guard pulls in System.SR resource lookup and an unsupported MissingManifestResourceException catch. This is dependency discovery, not a runtime NaN failure. Static initialization, timing/services, collections, text measurement, and full event routing still require further probes.

## WAT

Not emitted.
