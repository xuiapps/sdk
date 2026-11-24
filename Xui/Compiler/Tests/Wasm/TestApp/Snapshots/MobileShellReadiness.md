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
Delegate base APIs and multicast operations are not implemented.
```

## Dependency path

```text
Root { Value = Method { Identity = Program.Main } }
 → Method { Identity = Program.Main }
 → Method { Identity = Xui.Apps.TestApp.MobileTestAppShell..ctor }
 → Method { Identity = Xui.Apps.TestApp.MobileHomePage..ctor }
 → Method { Identity = Xui.Core.UI.ScrollView.add_ScrollChanged }
 → Method { Identity = System.Delegate.Combine }
```

Single-target delegates, captured closures, and the real Action<Vector> callback now execute. Dependency discovery reaches ScrollView event subscription: Delegate.Combine needs multicast invocation lists, followed by Remove and the event accessor synchronization path. Static initialization, collections, services, text measurement, and full event routing still require further probes.

## WAT

Not emitted.
