# Real TestApp view: construction gate

Repository sources: `Xui/Apps/TestApp/Pages/Canvas/Tests/FillRectTest.cs` and `Xui/Apps/TestApp/Pages/Canvas/ICanvasDrawingExample.cs`.

These unchanged sources form a separate TestApp.Canvas assembly linked with the actual Xui.Core implementation and this driver. This first gate deliberately tests construction before adding the rendering host.

## Driver

```csharp
using Xui.Apps.TestApp.Pages.Canvas.Tests;
public static class Program {
    public static int Main() {
        var view = new FillRectTest();
        return view == null ? 1 : 0;
    }
}
```

## Status

Blocked during compilation. WASM was not emitted; no browser rendering is claimed.

## WASM / blocker

```text
System.NotSupportedException: Only string[] entry-point array signatures are supported.
```
