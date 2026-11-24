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
Catch type is not a supported built-in exception: System.Resources.MissingManifestResourceException.
```

## Dependency path

```text
Root { Value = Method { Identity = Program.Main } }
 → Method { Identity = Program.Main }
 → Method { Identity = Xui.Apps.TestApp.MobileTestAppShell..ctor }
 → Method { Identity = Xui.Core.UI.View.AddProtectedChild }
 → Method { Identity = System.ArgumentNullException..ctor }
 → Method { Identity = System.SR.get_ArgumentNull_Generic }
 → Method { Identity = System.SR.GetResourceString }
```

Single-target and multicast delegates, captured closures, and the real Action<Vector> callback now execute. Dependency discovery passes Delegate.Combine and reaches the ArgumentNullException constructor used by View.AddProtectedChild, which pulls in BCL resource lookup and an unsupported MissingManifestResourceException catch. This is a compile-time dependency failure, not evidence that the app passed a null child at runtime. Event accessor synchronization, static initialization, collections, services, text measurement, and full event routing still require further probes.

## WAT

Not emitted.
