# Real TestApp rendering gate

[Open FillRect](Snapshots/FillRectRender.html) directly in Chromium. The file embeds
the JavaScript host and the Xui-generated WASM; it needs no server, network, or .NET
runtime. [The snapshot](Snapshots/FillRectRender.md) contains the driver, unchanged
TestApp drawing source, Canvas commands, exit code, retained methods, and WAT.

The test compiles TestApp's actual `FillRectTest.cs` and `ICanvasDrawingExample.cs`
into one PE, compiles a driver/Canvas adapter into another, and links both with Core
and selected framework implementation PEs. Core is a separate assembly, compiled
from its complete source tree. The application executes the real
`View.Measure → Arrange → Render` path, including virtual dispatch, caching checks,
culling, default drawing state, and render cleanup. Only the explicit
`TestAppCanvas.NativeCanvas.Command` host boundary is rewritten into a JS import.

## Explicit first-frame profile

`Frontend.CoreCanvasProfile` defines `XUI_COMPILER_CANVAS` when compiling Core:

- Void instrumentation calls and their arguments are omitted through Conditional
  attributes on InstrumentsAccessor. Interpolated diagnostic handlers and reflection
  therefore do not enter the reachable graph. This does not disable all possible
  diagnostics; `Trace` is outside this first-frame path.
- Font's default weight and stretch construct the same immutable 400/100 values
  directly, preserving the original first-frame profile around static fields `FontWeight.Normal` and
  `FontStretch.Normal`. General static storage/type initialization is now supported separately.

Normal Core builds do not define this symbol. No View method or TestApp drawing
source is substituted. The existing construction gate still tests the normal Core
implementation without this profile.

The minimal adapter supports the state operations and solid rectangles used here.
Unsupported drawing/measurement methods throw; font handling is limited to upright,
normal-stretch fonts. This is one real TestApp view, not the full application's
navigation, Window/RootView lifecycle, input, or text-rendering coverage.

## Verification

The driver arranges a 400×240 view at (20,30), then renders five rectangles. Debug
and Release application/view PEs must match CLR execution loaded into an isolated
assembly context with the same Core profile. Numeric comparisons account for the
compiler's f32 NFloat target versus the host CLR's f64 NFloat. The test checks an
exit code of zero, five rectangles, and Save/Restore surrounding the render.

```sh
dotnet test Xui/Compiler/Tests/Xui.Compiler.Tests.csproj -c Release --filter FullyQualifiedName~TestApp
node Xui/Compiler/Tests/Wasm/Canvas/browser.mjs
```

The shared browser runner checks offline execution, exact recorded commands,
rectangle colours/overlap/position, reruns, and a narrow viewport. It uses the
playground's installed Playwright dependency; see the Canvas test README for setup.


## Persistent phone preview

[MobilePhone.html](Snapshots/MobilePhone.html) opens a white browser surface with
the existing emulator middleware's `PhoneFrameGeometry` and `PhoneChromeRenderer`.
It runs the actual FillRectTest view in a persistent managed session. A fixture
subclass adds a draggable contact marker and a one-second release animation. The
normal `View.Animate`, animation request flags, Measure/Arrange/Render, and
`PointerEventRef` handler execute in WASM.

JavaScript holds the session as an opaque WASM GC reference. Named exports handle
frames, pointer packets, and inspection. `requestAnimationFrame` runs only when a
resize/input event invalidates the surface or the View requests another animation
frame. Frame timestamps cross the boundary as i64 100ns ticks (`BigInt`), preserving
TimeSpan precision. Canvas backing pixels follow devicePixelRatio; pointer packets
stay in CSS pixels and the middleware geometry maps them to phone coordinates.
The host handles capture, release, cancel/lost capture, blur, visibility, resize,
and disposal. It clears per-frame command recordings and releases the held session
when disposed.

This milestone forwards one contact directly to one View. It does **not** yet run
`EmulatorAppWindow`, `EmulatedWindow`, `PhoneFrameView`, RootView/EventRouter,
keyboard/text input, multitouch gesture routing, or the complete mobile navigation
shell. The explicit emulator compiler profile constructs its immutable device,
chrome specification, and colour defaults directly until static initialization is
supported. Normal builds retain the existing cached values.

```sh
dotnet test Xui/Compiler/Tests/Xui.Compiler.Tests.csproj -c Release --filter FullyQualifiedName~Mobile
node Xui/Compiler/Tests/Wasm/TestApp/mobile.browser.mjs
```

The browser test runs offline, checks the real chrome/rectangle pixels at 2× DPR,
drags a contact, verifies its release fade and idle scheduling, cancels through blur,
uses touch input, resizes to a narrow viewport, and verifies disposal. Screenshots
are written to the ignored playground browser-test output directory.

## Full mobile shell: measured blocker

[MobileShellReadiness.md](Snapshots/MobileShellReadiness.md) is an explicit
expected-unsupported regression. Complete Core, Core.DI, Fonts, DevKit.UI,
Software, Emulator, and TestApp sources compile into separate PEs. TestApp's real
shader generator runs; desktop Program/Platform startup is excluded. This metadata
probe does not package embedded font or GPU assets; the complete app will also need
those assets supplied by the browser host. The .NET SDK's
ASP.NET shared framework supplies the Microsoft.Extensions metadata for Core.DI.

Constructing `MobileTestAppShell` now passes ArgumentNullException guards and
reference CompareExchange in generated event accessors during dependency discovery.
The exchange supports single-agent execution, preserving reference identity and
the previous value through the existing managed-byref representation. Separate
CLR/Node tests execute generated event add/remove accessors and invocation snapshots.
The probe now includes the implementation assembly defining IServiceProvider.

TimeSpan's single-value factories now emit duration arithmetic in WASM, including
managed NaN/range failures. Their English guard messages do not require BCL
resource lookup. Separate CLR/Node tests compare integer and floating-point
boundaries, fractional ticks, and exception behavior.

The rating panel's string interpolation now uses a compiler-owned mutable UTF-16
GC buffer with string/character/boolean/integer append operations and immutable JS
string materialization. The BCL handler's pointer-based Span<char> implementation
is not retained. Separate CLR/Node tests cover growth, copies, nested calls, integer
limits, Unicode, and exceptional cleanup.

Generic interface navigation methods now specialize into separate dispatch slots.
The full-menu probe currently reaches HashSet<View> default equality in diagnostics;
optional preload handlers also retain known Lazy/async dependencies. Separate optional async/resource-heavy showcases for
the first browser profile, then continue through clocks,
static initialization, collections, service/type lookup, text measurement/drawing,
and the complete window/input lifecycle. This test does not claim the full
application executes.

## Browser catalogue probe

[BrowserShellReadiness](Snapshots/BrowserShellReadiness.md) passes an explicit
[BrowserMenu](Fixtures/TestApp/Mobile/BrowserMenu.cs) into MobileTestAppShell.
The default shell constructor still creates the full TestApp catalogue. The browser
fixture uses the real mobile category card, demo button, navigation, and page wrapper
for a FillRectTest page; optional async/GPU showcases are not included in its menu.
LayoutFrameInstruments.CreateCountersOnly omits unique-view tracking while the
normal constructors retain their preallocated HashSet. Core's existing compiler
profile still omits diagnostic instrumentation. Default equality, primitive boxing,
composite String.Format, and the Dictionary duplicate-key message path now compile.
Separate CLR/Node regressions execute integer-key Dictionary operations, resizing,
and struct values with string references. The browser catalogue completes reachability
and now executes the real mobile-shell constructor in WASM. Node validates the
435 KB module and observes `shell.Count == 1`. This includes the home/header,
rating controls, Canvas catalogue and navigation construction, static initializers,
and actual BCL List code. The separate full-menu probe still includes optional
async/GPU/resource showcases.

Shared short-circuit branches now support nested tails; larger acyclic regions use
typed forward blocks with one copy of each instruction run. RuntimeHelpers reference
content queries specialize from actual field classification, avoiding recursive CLR
intrinsic stub bodies. DateTime uses compiler-owned automatic storage, and byte-channel
NFloat conversions in Color use direct target-width conversion.

The construction gate remains separate from the themed first-frame gate below.
The existing phone HTML still displays the interactive FillRect fixture; the real
shell's persistent window callbacks and EventRouter connection now execute in BrowserInteractive below.

## Themed mobile home: first frame

[BrowserShell.html](Snapshots/BrowserShell.html) now displays the real mobile home
screen offline. Its embedded WASM executes Window/RootView construction and view
activation, TestAppDesignRoot's device-aware palette/spacing/typography, and the
actual Measure/Arrange/Render traversal. The explicit browser platform provides
mobile device identity and a Canvas context; it does not substitute view code.
The catalogue remains Canvas-only and the diagnostics overlay is omitted through
constructor injection. Normal TestApp constructors retain their original overlay.

The expanded fixture adapter supports Canvas text metrics, text, paths, ellipses,
linear gradients and Core's SurfaceEffects renderer. Shadows and backdrops use
Core's documented portable approximation. Inter regular/medium/semibold/bold font
bytes and their license are embedded in the HTML. Node uses deterministic metrics
for the command snapshot; Chromium uses actual font shaping. Unsupported images,
radial gradients and remaining Canvas operations fail explicitly.

[The snapshot](Snapshots/BrowserShellRender.md) records the first frame and exit
code. This first-frame page remains separate from the interactive shell below.
The MobilePhone preview retains its smaller interaction fixture.

```sh
node Xui/Compiler/Tests/Wasm/TestApp/shell.browser.mjs
```

The runner verifies offline loading, real labels, nonempty pixels, embedded fonts,
balanced drawing state, repeat rendering and a narrow viewport. Screenshots go to
the ignored playground browser-test output directory.


## Interactive mobile shell

Open [BrowserInteractive.html](Snapshots/BrowserInteractive.html) and tap **Canvas
Drawing**, then **Back**. The page embeds the compiled module, Canvas host and
licensed Inter fonts. No HTTP server, network resources, or framework WASM runtime
is needed. The compiler emits 1.08 MB of WASM for 1,275 retained managed methods.
The **Page size** footer reports exact HTML bytes, base64 overhead, individual
font payloads, JavaScript and markup, plus a section-by-section WASM breakdown.
There are no image files: Xui draws the logo, icons and phone chrome through Canvas.

A persistent fixture session owns the real Abstract.Window, RootView and themed
MobileTestAppShell. Pointer events become stack-backed TouchEventRef values and
pass through Window.OnTouch and EventRouter. The browser clock drives
Window.OnAnimationFrame, press feedback and NavigationView transitions. The FPS
badge samples requested frames using MobileFrameRate, warms up for two seconds,
and retains its last measured value while idle; idle time is not a missed frame. Rendering
explicitly calls RootView.Measure/Arrange/Render. The host reuses middleware phone
geometry/chrome; it does not instantiate the complete EmulatedWindow composition.

The browser catalogue contains the unchanged FillRectTest in a real example page.
Optional diagnostics and async/GPU/resource-heavy examples remain excluded. The
adapter currently accepts one touch contact; keyboard, IME and full multi-touch
support remain future work. Native canvas text metrics and the documented portable
surface-effects approximation apply as in the first-frame gate.

```sh
dotnet test Xui/Compiler/Tests/Xui.Compiler.Tests.csproj -c Release --filter FullyQualifiedName~BrowserInteractionTests
node Xui/Compiler/Tests/Wasm/TestApp/interactive.browser.mjs
```

Node checks 30/60/120 FPS warmup, rendered badge updates, idle exclusion, both
navigation directions and animation completion. Chromium checks
offline loading, the phone frame, cancelled presses, mouse and touch navigation,
RGB pixels from the actual example, DPR resizing, idle scheduling and disposal.
Screenshots are saved under the ignored playground browser-test output directory.
Array.Clear preserves element byrefs; logarithm/exponential, scalar rounding and
NFloat limits support the reached animation/event-routing paths. Math and array
regressions compare CLR and Node execution, with explicit WASM32 limit checks.
