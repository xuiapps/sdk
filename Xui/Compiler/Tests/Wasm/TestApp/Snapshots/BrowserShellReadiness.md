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
Catch type is not a supported built-in exception: System.Resources.MissingManifestResourceException.
```

## Dependency path

```text
Root { Value = Method { Identity = Program.Main } }
 → Method { Identity = Program.Main }
 → Method { Identity = Xui.Apps.TestApp.BrowserMenu..ctor }
 → Method { Identity = Xui.Core.UI.View.AddProtectedChild }
 → Method { Identity = Xui.Core.UI.View.AttachSubtree }
 → VirtualCall { Identity = Xui.Core.UI.View.get_Item }
 → Method { Identity = Xui.Core.UI.ViewCollection.get_Item }
 → VirtualCall { Identity = System.Collections.Generic.List<Xui.Core.UI.View>.get_Item }
 → Method { Identity = System.Collections.Generic.List<Xui.Core.UI.View>.get_Item }
 → Method { Identity = System.ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessException }
 → Method { Identity = System.ThrowHelper.GetArgumentOutOfRangeException }
 → Method { Identity = System.ThrowHelper.GetResourceString }
 → Method { Identity = System.SR.get_ArgumentOutOfRange_IndexMustBeLessOrEqual }
 → Method { Identity = System.SR.GetResourceString }
```

This probe injects a Canvas-only catalogue into the real MobileTestAppShell. It retains the real home/header, rating controls, navigation, and mobile page wrapper. The full-menu probe remains separate and includes optional async/GPU/resource showcases. This is not an execution claim.

## WAT

Not emitted.
