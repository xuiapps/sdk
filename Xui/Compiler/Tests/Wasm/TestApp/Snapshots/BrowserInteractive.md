# Interactive TestApp in the middleware phone frame

[Open standalone page](BrowserInteractive.html). JavaScript, WASM and Inter fonts are embedded; no server or .NET browser runtime is required.

The real Window, RootView, EventRouter, design root, mobile shell, buttons and NavigationView execute in WASM. Browser pointer events become TouchEventRef values; requestAnimationFrame supplies the platform clock and Window.OnAnimationFrame. The host explicitly calls RootView.Measure/Arrange/Render. The middleware supplies phone geometry/chrome and the fixture maps screen coordinates into the logical safe area.

The browser catalogue exposes Canvas Drawing (the unchanged FillRectTest), with press feedback, animated navigation and Back. Optional diagnostics and the full async/GPU catalogue remain outside this preview. This is not the complete EmulatedWindow/runtime composition.

Sources: [Interactive.cs](Fixtures/TestApp/Browser/Interactive.cs), [Platform.cs](Fixtures/TestApp/Browser/Platform.cs), [CanvasContext.cs](Fixtures/TestApp/Browser/CanvasContext.cs), [BrowserMenu.cs](Fixtures/TestApp/Mobile/BrowserMenu.cs), [PhoneHost.cs](Fixtures/TestApp/Mobile/PhoneHost.cs).

## Execution

```text
Node: 30/60/120 FPS warmup and label updates, idle exclusion, persistent window, touch down/up, Canvas Drawing navigation, Back navigation and animation completion passed.
WASM bytes: 1075454
Retained managed methods: 1275
```

## Exit Code

```text
0
```
