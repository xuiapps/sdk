# Xui runtimes

These projects implement or simulate the platform-facing contracts defined in
`Xui/Core/Core/Actual`. Follow the repository-root `AGENTS.md` first.

## Runtime map

- `Windows` — Win32 lifecycle/input, Direct2D/DirectWrite 2D rendering, Direct3D
  GPU rendering, DirectComposition, DXGI, WIC, and COM bindings.
- `MacOS` — AppKit windowing with Objective-C, CoreFoundation, CoreGraphics,
  CoreText, CoreAnimation, Dispatch, ImageIO, and Metal bindings.
- `IOS` — UIKit windowing/input with Objective-C, CoreFoundation, CoreGraphics,
  CoreText, CoreAnimation, and Metal bindings.
- `Android` — binding-free managed composition, C/JNI transport, and Java View/Canvas playback.
  GPU nodes present through a C Vulkan backend and TextureView-owned surfaces.
  `Android/Host` contains the optional .NET Android startup and legacy haptic adapters.
- `Browser` — browser lifecycle, DOM input, and Canvas rendering through
  JavaScript interop.
- `Software` — deterministic SVG, font, rasterization, tessellation, bitmap, and
  software-GPU implementations used by tests and tools.
- `Test` — synchronous `IRuntime`/`IWindow` harness built on Software, Fonts, DI,
  and optional Emulator middleware.
- `E2E` — launches and controls a real app through the DevTools client.

Platform entry points normally live in `Runtime/<Platform>/Actual`: start with
the platform class implementing `IRuntime`, its window implementing
`Xui.Core.Actual.IWindow`, and its drawing context implementing
`Xui.Core.Canvas.IContext`.

## Contract changes and parity

When changing an interface under `Xui.Core.Actual`, `Xui.Core.Canvas`, or
`Xui.Core.GPU`:

1. Read the Core contract and its call sites before changing an implementation.
2. Search for every implementer across `Xui/Runtime` and `Xui/Middleware`.
3. Update middleware forwarding or interception where the contract crosses it.
4. Update Software and Test implementations; they are part of the contract, not
   disposable mocks.
5. Check Browser, Android, Windows, macOS, and iOS independently. Existing
   `NotImplementedException` members mean interface presence is not proof of
   support.
6. Add the narrowest meaningful Core, component, or integration test.

Do not hide missing platform work behind an empty implementation or a plausible
default. Preserve an explicit unsupported path when full behavior is unavailable.

## Native interop and ownership

Ownership is part of the type design. Match the surrounding binding family:

- A handle borrowed or retained only within one call uses a `ref struct`,
  commonly named `Ref` or ending in `Ref`, and is disposed within that call.
- A handle owned across several calls commonly uses a disposable value type
  ending in `Ptr`. Its containing object must dispose it.
- A native object kept across frames commonly uses a class wrapper with explicit
  native reference ownership.
- Windows COM wrappers must balance every acquired reference with `Release`.
  Copying a raw pointer does not transfer or duplicate ownership; call `AddRef`
  when another owner must outlive the source.
- Apple CoreFoundation/CoreGraphics/CoreText and Objective-C wrappers must
  balance create/copy/retain operations with the appropriate release. Do not
  assume a returned pointer is retained.
- Keep disposal idempotent where the surrounding wrapper pattern supports it,
  and clear mutable native pointers after releasing them.
- Never let a `Span`, callback, or native pointer outlive the managed/native
  storage that backs it.

Use `nfloat`/`NFloat` at framework and drawing boundaries. Convert to native
integer or floating-point types only at the interop call that requires it. Keep
logical coordinates distinct from physical pixels and preserve each platform's
scale and coordinate-direction conversions.

## Window, input, and drawing behavior

- An Actual window holds or receives the paired `Xui.Core.Abstract.IWindow`.
  Native lifecycle, input, animation, and paint callbacks delegate to that
  Abstract side.
- The active drawing context is exposed through the Actual window's
  `IServiceProvider.GetService(typeof(IContext))` during rendering. Respect its
  validity window; do not cache a frame-bound context for later frames.
- Populate `DisplayArea`, `SafeArea`, and `ScreenCornerRadius` in logical
  coordinates and update them when native geometry changes.
- Translate native input into the allocation-sensitive event `ref struct`s
  defined in Core. Preserve button/touch identity, phase, modifiers, and text
  measurement context.
- Invalidation schedules platform painting; it must not directly invent a second
  framework render lifecycle.
- Runtime callbacks normally run on the UI thread. Marshal through the platform
  dispatcher instead of making view or window state concurrently mutable.

## Backend-specific boundaries

- Windows native declarations belong in the relevant `COM`, `Win32`, `D2D1`,
  `D3D11`, `DComp`, `DWrite`, `DXGI`, or `WIC` area; orchestration belongs in
  `Actual`.
- macOS/iOS framework bindings belong in their named native-framework areas;
  application-facing orchestration belongs in `Actual`. Keep equivalent Apple
  bindings structurally aligned when practical, but do not copy code without
  checking platform API differences.
- Android records through `RetainedCanvasContext` and sends versioned float32 wire
  packets through a C ABI to Java-owned views. Keep the renderer independent of
  managed Android bindings; those belong only in the optional host adapter.
  Check `Android/README.md` for current gaps and native/emulator test commands.
- Browser hot paths should minimize managed/JavaScript crossings and temporary
  string/object creation.
- Software output must be deterministic across machines. Use bundled Inter fonts
  when text metrics affect tests and invariant formatting for serialized SVG.

## Validation

Build the smallest affected project first:

```sh
dotnet build Xui/Runtime/Software/Xui.Runtime.Software.csproj
dotnet build Xui/Runtime/Test/Xui.Runtime.Test.csproj
dotnet build Xui/Runtime/MacOS/MacOS.csproj
dotnet build Xui/Runtime/Windows/Windows.csproj
dotnet build Xui/Runtime/Browser/Browser.csproj
dotnet build Xui/Runtime/Android/Android.csproj
```

Android and app-hosted iOS/browser validation may require installed workloads.
After shared Canvas, window, input, font, or GPU changes, run the relevant Core,
component, and integration tests described in `Xui/Tests/AGENTS.md`.
