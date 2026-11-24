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

## Entry

```csharp
var metrics = new MobilePerformanceMetrics(new LayoutFrameInstruments());
var shell = new MobileTestAppShell(metrics);
return shell.Count;
```

## Compiler failure

```text
The method has no managed body: System.Threading.Interlocked.CompareExchangeObject.
```

## Dependency path

```text
Root { Value = Method { Identity = Program.Main } }
 → Method { Identity = Program.Main }
 → Method { Identity = Xui.Apps.TestApp.MobileTestAppShell..ctor }
 → Method { Identity = Xui.Apps.TestApp.MobileHomePage..ctor }
 → Method { Identity = Xui.Core.UI.ScrollView.add_ScrollChanged }
 → Method { Identity = System.Threading.Interlocked.CompareExchange<System.Action<Xui.Core.Math2D.Vector>> }
 → Method { Identity = System.Threading.Interlocked.CompareExchange }
 → Method { Identity = System.Threading.Interlocked.CompareExchangeObject }
```

Single-target and multicast delegates, captured closures, and the real Action<Vector> callback now execute. ArgumentNullException now preserves ParamName, Message, and the ArgumentException hierarchy without pulling in BCL resource lookup. Dependency discovery reaches ScrollView event subscription and stops at Interlocked.CompareExchangeObject, the runtime-provided reference update used by the generated add accessor. This needs a compiler/runtime intrinsic for the supported execution model. Static initialization, collections, services, text measurement, and full event routing still require further probes.

## WAT

Not emitted.
