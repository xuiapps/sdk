# Plan: display a Xui app using the Xui WASM compiler

## Active objective: compile and run the complete TestApp

The acceptance target is the production TestApp catalogue and window lifecycle,
compiled from its implementation assemblies by XuiNet. The current curated
browser menu is a preview and does not satisfy this target.

Work proceeds through production-page execution probes, followed by the complete
application entry point. Each passing compiler/runtime increment is committed.
Remaining work is measured by actual reachable dependencies and browser behavior:

1. **Production page dependencies.** Exercise the unchanged carousel pages and
   their sections. Native string operations, constrained struct interface calls,
   static array initialization, array collection interfaces, attached-property
   dictionary keys, and formatting are the current integration path.
   The production Basics (3), Flexbox (3), and Grid (12) sections now execute and
   pass offline Chromium rendering/input checks. This includes attached-property
   dictionaries, named grid areas, native string object storage, and array-backed
   spans. Canvas Drawing (22) and Canvas Animations (3) also execute, including
   packaged image drawing and bitmap fills through the injected asset catalogue.
   Gestures (6) now renders with invariant Double.ToString custom decimal formats.
   Text Layout, Text Metrics, View Collection Alignment, Device Info, and Haptics
   also execute, together with Text Input, Design System, both Vibration pages and Market: sixteen production
   examples, 59 sections (14 of the 17 mobile-menu entries). Haptics renders its actual
   controls; absent platform services leave haptic calls inactive. The host does
   not claim hardware haptics support.
   Full multi-touch input is still a separate host/lifecycle requirement.
   Borrowed string spans, character search, scalar Cbrt/Atan2, enum names, and
   floating-point interpolation now pass CLR/Node comparisons. Shared array pools,
   span fill/clear, primitive ordering, span concatenation, String.Empty and host
   grapheme boundaries now support the real Text Input page. Its browser gate tests
   typing, deletion, Tab navigation, password masking, input filters and committed
   IME text. Provisional composition and native selection handles remain future work.
   Design System's palette, live-preview tabs and action button also pass; its
   color-wheel PNGs are embedded in the injected asset catalogue. Bounded linear
   stackalloc and pointer-backed spans for reference-free elements now execute,
   including dynamic lengths and return/exception cleanup.
   Vibration pages now support invariant-provider interpolation and sizeof-driven
   stackalloc. Chromium checks live dragging, recorded curve playback, and Stop/Start;
   hardware vibration remains absent. Market now runs its real 5,000-stock list and
   price-history detail pages, with signed numeric sections, NFloat formatting,
   invariant lowercasing, Span.ToArray and static interface specialization.
   The remaining mobile-menu pages are 3D and the two Floorplan renderers.
   Generic virtual LINQ dispatch now specializes; Nested Stacks (off-menu) still needs a fresh browser gate.
   The legacy standalone AnimatedHeart example executes but has a zero-height view
   in the carousel and is not counted as visually accepted. The production Canvas
   Animations heart is already covered. Browser task state machines now execute in focused tests; complete startup integration remains.
2. **Actual window and services.** Complete application startup, the emulator
   middleware lifecycle, async asset loading, resize, animation, pointer capture,
   keyboard/IME, and navigation. Preserve real service contracts.
3. **DOM composition.** Honor stable group keys, child order, inherited drawing
   state, transforms, clipping, border/radius/shadow, and invalidation. Give retained
   groups dedicated DOM surfaces. This is now implemented in the production-page
   host, including browser blur for LiquidGlass. Keyed elements are reused, omitted
   children released, and visible content redrawn every frame. Arbitrary inherited
   Canvas clips choose the immediate fallback. Hidden caching and dirty-content
   reuse remain optimizations; full-shell integration follows the page gates.
4. **Assets and fonts.** Publish an injected asset catalogue alongside WASM; resolve
   `asset://` identifiers, decode/cache images, preload fonts, and report missing
   assets through the normal application paths. The standalone page gate embeds
   its required PNGs, predecodes them before startup, and supplies synchronous
   IImage.Load/Size and Canvas drawing/patterns. Async loading and raw pixel upload
   remain explicit unsupported operations.
5. **GPU and 3D.** Generate browser shader code from the shared shader model and
   supply a browser GPU surface. Render the production 3D views in their canvas;
   do not replace them with screenshots or prerecorded frames.
   The production GpuHardwareCubeTestView now compiles with its real mesh, shaders,
   matrix calculation and input handling. A browser WebGL 2 device consumes borrowed
   typed vertex/constant uploads and presents into retained composition surfaces.
   Native backends use the same typed API through synchronous pointer defaults;
   WASM copies GC-array values into a bounded linear scratch frame instead of pinning.
   The standalone HardwareCubeExample gate isolates this actual production view.
   The complete ThreeDExample also constructs the optional software renderer, whose
   pinned arrays/native framebuffer are still unsupported. Do not count the full
   3D page as ready yet. Texture/sampler services and async Floorplan loading remain
   separate dependencies. Both real GLSL shaders already pass matrix/depth/mip/fade
   pixel tests (see [GPU gates](Tests/Wasm/Gpu/README.md)).
   Full-shell discovery now passes missing-constructor branches, single-agent
   monitors, browser Task/Lazy replacements and closed generic virtual dispatch.
   TestApp PEs now contain their declared embedded resources. The browser resource
   profile serves original manifest streams and BinaryReader calls; gzip assets
   are decompressed before startup and read synchronously through bounded 4 KB
   linear scratch buffers into GC arrays. All bytes of the real 2048×2048 atlas
   and its five mip levels match the CLR checksum. This adds loading infrastructure,
   not a rendered Floorplan page. Neutral XML message getters now resolve against
   their own supplied PE resource tables. Single-agent thread identity now supports XML's generated iterators.
   Virtual Object.ToString now passes XML attribute conversion. Default IEquatable class equality now supports the floorplan Region record,
   including Type descriptor keys. Primitive Buffer.BlockCopy now passes JSON resource loading; XML namespace caching
   now uses managed wrappers around JavaScript WeakRef. Struct/tuple hashing now
   runs declared overrides with value-copy semantics and a browser HashCode seed.
   Null-check-only boxing of non-nullable structs is folded; escaping struct boxes
   (including dictionary error formatting) remain unsupported. The next discovery
   rejection is string reference CompareExchange in BCL local-timezone discovery reached by XML value conversion; integer interpolation now preserves signed widths and hexadecimal formats; UriFormatException catches now preserve FormatException ancestry; Rect now exposes its typed equality through IEquatable; StringBuilder now uses mutable GC character buffers and native string snapshots; execution-stack queries now use a bounded per-instance call-depth budget; single-agent memory barriers now bypass CLR thread implementation; ordinal String.Equals comparison modes now execute using native strings; Unicode encoding-name lookup now executes; streaming Unicode decoding and character-buffer string construction now execute; UTF-16 conversion now supports both byte orders; explicit NativeMemory allocation/free now has a reusable linear heap; ordinal string replacement now supports SVG dimension parsing; constrained reference hashing now preserves virtual overrides; invariant Double/UInt32 parsing now supports SVG coordinates and colors; the explicit browser JSON document profile now bypasses the BCL parser; built-in exception text now formats managed payloads and inner chains; all signed/unsigned integer widths now support invariant formatting; invariant Decimal formatting now preserves coefficient and scale; proven null-format Int32 and invariant NumberFormatInfo pairs now execute; primitive-field struct hashing now covers AffineTransform; comparer acquisition now defers operation validation and arrays support identity hashing/equality; the Threading facade now uses existing single-agent Monitor operations; XML exception payloads, Stream.ReadAtLeast and MissingManifestResourceException catches now pass. Explicit managed-span UTF-8 now passes JSON transcoding without pinned locals; reference Interlocked.Exchange passes disposal. The actual encoding facades are supplied. Exact-element array slicing and typed managed-buffer copies now pass the FloorplanDrawing.BuildSurfaces path. Repeated-character String construction now supports SVG hex-color expansion with UTF-16 semantics. The shared SVG transform scanner preserves the prior regex matching behavior without the regex engine. Real LINQ iterator-to-array materialization now passes for tuples with string fields, using checked add/subtract, AppContext switches and array collection/enumeration adapters. Unpinned GC array factories now allocate WASM GC vectors; Object.GetHashCode preserves class overrides and scalar-box semantics. SVG syntax searches use explicit ordinal operations, and encoding fallback exception catches pass JSON discovery.
6. **Acceptance.** Navigate every production page in the browser, exercise controls
   and animations, inspect screenshots, and make `/wasm/` host that build. Keep the
   curated preview until this gate passes; an unsupported feature remains visible
   in development probes rather than becoming a silent no-op.

The sections below record earlier milestones and their narrower acceptance gates.

## Current milestone: interactive mobile shell

[BrowserInteractive.html](Tests/Wasm/TestApp/Snapshots/BrowserInteractive.html)
runs the real themed TestApp home, EventRouter, press feedback, Canvas Drawing
example and animated Back navigation inside the middleware phone renderer. Node
and offline Chromium verify this path, including cancellation, resize and idle.
The fixture retains a real Window/RootView, supplies a browser clock, and calls
Measure/Arrange/Render explicitly. Full EmulatedWindow composition, keyboard/IME,
multitouch, diagnostics and the complete async/GPU catalogue remain future work.
The earlier gates below document the incremental route to this milestone.

## Objective and shortest path

Produce a visible canvas frame from C# compiled through **Roslyn → PE/MSIL → XuiNet → our WASM emitter**. Start with a small rendering slice using real Xui geometry and color types, then grow it into a persistent, interactive Xui window/view application.

The route is a dedicated compiler sample that supplies Xui Core and the sample as separate implementation assemblies, uses a small explicit host adapter, and draws rectangles and text. Primitive/string calls across three assemblies now work in one WASM module. Dependency-defined numeric structs and GC classes now work, including cross-assembly inheritance and dispatch. Source linking remains a fallback, not the default route. It does not start through `HostBuilder`, Microsoft DI, or the existing browser runtime. The framework .NET WASM runtime may continue hosting Roslyn in the playground; it must not execute the sample application.

The first visible frame is deliberately a **Xui canvas slice**, not a claim that arbitrary `Application`, `Window`, or `View` code works. A later acceptance gate requires the real framework lifecycle, rendering, and input paths.

## Persistent browser host and mobile-shell readiness

The [phone preview](Tests/Wasm/TestApp/Snapshots/MobilePhone.html) now supplies a
white browser surface, real emulator phone geometry/chrome, persistent managed
state, frame exports, resize, and pointer capture/cancel. It executes the real
FillRectTest with a fixture interaction/animation overlay. Browser tests verify
pixels, 2× DPR, drag/touch input, fade completion, idle scheduling, and disposal.

This is a direct single-view bridge, not the full emulator/window/event-router
composition. The [full-shell probe](Tests/Wasm/TestApp/Snapshots/MobileShellReadiness.md)
compiles all application dependencies as separate PEs with the real shader
generator. Closed generic methods and reference byrefs now execute with aliasing,
linked-call, and unwind tests, covering the SetProtectedChild pattern. The full-shell
probe now passes delegate combination, ArgumentNullException guards, and reference
CompareExchange in generated event accessors. The exchange compares object identity,
conditionally updates the existing owner/selector address, and returns the previous
reference without host callbacks. This is a single-agent implementation; shared
multi-threaded state and string reference identity remain unsupported.

The probe also supplies the implementation assembly defining IServiceProvider.
TimeSpan's single-value factories now emit tick arithmetic and managed NaN/range
checks with English messages, avoiding resource-manager dependencies for these guards.
CLR/Node comparisons cover fractional ticks, integer limits, NaN, and infinities.
The rating panel's interpolated strings now use compiler-owned mutable UTF-16 GC
buffers. Strings, characters, booleans, and default integer formatting execute in
WASM; handler bodies, ArrayPool, and pointer-based Span<char> are not retained.
Closed generic interface navigation calls now have specialized dispatch slots.
The full-menu probe now passes single-agent volatile field access and supplies
the XML implementation assemblies. Rectangular array signatures, row-major storage,
dimension queries, and element byrefs now execute in CLR/Node comparison tests.
The full shell next reaches `System.MissingMethodException` in the default-constructor
branch of `Lazy<Task<FloorplanDrawing>>`. Async floorplan preloading and complete
application execution remain unverified.

The next sequence is:

1. The browser catalogue is now injected into the real shell, separately from the
   unchanged default catalogue. Default string/scalar and typed IEquatable struct equality now execute, including
   Color and its NFloat fields. Ordinary string equality in Button also executes.
   Header easing now uses the shared Pow runtime; same-type scalar/reference Array.Copy
   executes directly with overlap and managed range checks. Class default equality and Array.IndexOf now execute, including virtual Equals
   overrides. Generic page factories now select constructors at compile time and preserve
   TargetInvocationException wrapping. The header’s Int32.ToString now produces native
   JS strings, including direct InvariantCulture calls. GetType().Name now uses
   existing runtime descriptors, and example IDs now concatenate native JS strings.
   Typeof-based service lookup now executes with interface dispatch. The browser
   resource boundary now reads neutral message strings from supplied BCL resources
   and emits only requested literals. Primitive and enum hashing/typed equality now specialize, including default comparers;
   scalar/string IEqualityComparer dispatch now executes for default and application comparers.
   Managed byrefs now cover GC scalar/struct fields, array-entry field ranges, and null locations. Struct-array copies preserve aliases. Dependency discovery passes dictionary hashing and gesture-routing span slices/copies; navigation/feedback clocks now use the monotonic host runtime. HashHelpers’ PE-backed prime table now becomes shared static span data; its square-root operation now emits WASM directly. The next dependency is duplicate-key exception formatting through String.Format; structured switches now emit `br_table`, including the actual CoreLib argument-name and resource-message selectors.
   Continue through the window and service paths.
2. Re-run the full-shell probe; implement reachable static initialization and
   collections as their actual dependencies appear.
3. Supply service/type lookup and a persistent Actual window, then connect the real
   RootView/EventRouter and EmulatedWindow rather than the single-view bridge.
4. Expand the Canvas host to text measurement, text drawing, paths and clipping
   required by the mobile home/navigation, then test scrolling and navigation in-browser.

The Canvas source profile still explicitly omits instrumentation and constructs
immutable defaults directly. Keep these cuts documented until their general
compiler support exists. See [host details and limits](Tests/Wasm/TestApp/README.md).

## Real View first frame now executes

[FillRectRender](Tests/Wasm/TestApp/Snapshots/FillRectRender.html) runs the unchanged
TestApp FillRectTest through real `View.Measure`, `Arrange`, and `Render` and draws
five rectangles in Canvas. Debug/Release execution is compared with .NET; the
standalone file is also checked offline in Chromium. Core, TestApp view, and driver
remain separate input assemblies. See the [profile and limits](Tests/Wasm/TestApp/README.md).

This gate explicitly uses `XUI_COMPILER_CANVAS`: instrumentation calls are omitted,
and Font constructs equivalent default weight/stretch values instead of reading
static fields. Normal Core behavior is unchanged. Compiler additions include
layout NFloat helpers, init-only setter signatures, TimeSpan.Zero value loads,
shared short-circuit tails/early-return joins, and linear struct block results.

Persistent state across exported resize/frame calls is now implemented in the phone
preview above. Input through the real RootView/EventRouter remains outstanding.
Before full TestApp navigation, address general static initialization, collections,
text measurement/drawing, remaining Canvas operations, and Window/service setup.
The first-frame host is intentionally incomplete and rejects unsupported operations.

## Reassessment after standalone canvas execution

Reassessed on 2026-11-24. The first canvas slice now runs: [Shapes](Tests/Wasm/Canvas/Snapshots/Shapes.html),
[Grid](Tests/Wasm/Canvas/Snapshots/Grid.html), and [Events](Tests/Wasm/Canvas/Snapshots/Events.html)
are standalone HTML artifacts with embedded JavaScript and Xui-generated WASM.
Tests compile separate application/Core PEs in Debug and Release, verify exact drawing
commands in Node, and open the files offline in Chromium to check pixels and text.
Shapes executes the real `Rect.Right` method from Xui.Core.dll. Events carries Rect,
Color, string, and interface values through a stack event and interface call.

| Area | Current evidence | Remaining work |
| --- | --- | --- |
| Assembly linking and layouts | Separate implementation assemblies share one module; numeric struct layouts and nested fields follow their defining modules. Three-library tests cover overlapping field row numbers. | Collections and remaining unresolved external dependencies remain. |
| NFloat | Target f32 storage and signatures, Int32/Single/Double conversions, and add/subtract/multiply/divide, comparisons, and infinity/NaN constants execute without BCL bodies. | Add further members only when required, with explicit target rounding tests. |
| Struct ABI | By-value arguments use callee-owned copies; results use caller-owned storage. Direct/virtual/interface calls, stack reference slots, recursion, and readonly `in` signatures are tested. | GC-field byrefs, primitive/reference byrefs, and further BCL operations remain. |
| Drawing host | Explicit clear, rectangle, and text imports draw through Canvas 2D. Geometry runs in WASM. | Full IContext coverage, measurement, images, paths, clipping, and persistent rendering. |
| Standalone output | Tests generate single-file HTML, with no runtime download or server needed. | Integrate the drawing host and Core compilation inputs into the editor playground separately. |
| Actual app lifecycle | Ref-event/interface demo executes using a small application-defined adapter. | Instantiate the real Window/RootView/View path; support its remaining spans, static state, ABI, and BCL dependencies. |

Closed generic struct layouts and framework ValueTuple constructors now execute,
including Xui's `Point point = (0, 0)` conversions. Default interface selection, arrays, and generic interface variance now execute.
Enum stack/storage normalization now executes too. Real View layout/render now executes under the explicit first-frame profile described above.
The next visible host milestone is adding these scenes to the playground editor, followed by persistent
state and frame/input exports. The current artifacts establish the canvas slice,
not arbitrary Xui application compatibility.

## Real TestApp gate after struct calls and linked classes

The [construction snapshot](Tests/Wasm/TestApp/Snapshots/FillRectConstruction.md) now
compiles the unchanged `FillRectTest.cs` and `ICanvasDrawingExample.cs` from TestApp
into a separate PE, linked with the actual Core implementation and a small driver.
It now passes as a required regression test: the module executes in Node and checks
the initialized `WritingMode` through the real property getter. The ValueTuple constructor
in `View..ctor` resolves into supplied framework implementation PEs. The probe supplies
`System.ComponentModel` for `IServiceProvider` and `System.Collections` for collection types.
Array signatures, variant-interface metadata, and enum stack/storage normalization pass.
Construction success does not yet establish layout, rendering, input, or full TestApp support.
No Core/View method is replaced or silently omitted by this gate.

Completed in this step:

- Struct argument/return ownership, including recursive calls, alias-preserving
  `starg`, results through interface/virtual calls, and reference slots surviving cleanup.
- Classes and inheritance across supplied assemblies, with module-qualified inherited
  field identities and assertions on runtime field descriptors.
- Framework `InAttribute` required modifiers on readonly virtual/interface parameters.
- NFloat division, comparisons, and infinity/NaN constants.
- A fourth offline Canvas artifact, [Values](Tests/Wasm/Canvas/Snapshots/Values.html),
  executing Core's `Rect.Inset` and passing Rect/Color values through ordinary methods.

Tuple milestone: closed generic layouts/classification, actual ValueTuple constructors,
assembly type forwarding, target-width NFloat definitions, and lawful deferral of unused
BeforeFieldInit constants are implemented. Tests cover arities 2–7, named/nested tuples,
ref copies, and actual Xui Point/Vector/Size/Rect/Frame conversions. See
[tuple geometry](Tests/Wasm/Tuples/Snapshots/XuiGeometry.md).

Default interface dispatch and reachability are implemented, including class precedence,
most-specific defaults, linked assemblies, and pruning unselected bodies. See the
[default layout methods](Tests/Wasm/Interfaces/Snapshots/Defaults.md).

Array milestone: managed vectors, struct element copies, inline buffers, and linear-backed
Span/ReadOnlySpan length/indexers execute. The [gradient fixture](Tests/Wasm/InlineArrays/Snapshots/Gradients.md)
uses actual Xui GradientStop/LinearGradient and tuple collection syntax.

Variance milestone: class/interface reference conversions, dispatch, casts, inherited contracts,
and specialized default bodies execute. See [variance](Tests/Wasm/Interfaces/Snapshots/Variance.md).
Representation-changing calls and multiple variant default candidates still fail explicitly.

Enum milestone: all eight underlying types retain nominal signatures and exact linear
storage widths while using integer evaluation-stack values. Signedness, flags, generic
specializations, and arrays/inline buffers execute; the TestApp constructor gate passes.
See [enum widths](Tests/Wasm/Enums/Snapshots/Widths.md).

Next sequence: grow the successful construction gate to Measure/Arrange/Render with a Canvas host. Closed
Nullable storage is enabled by generic layout; its callable BCL surface still needs testing.
Static initialization, GC-field byrefs, collection methods and remaining span operations, numeric helpers, and
persistent frame/input exports remain later gates. The full TestApp is not running yet.

## Interface prerequisite — implemented

Class-backed interface calls now support implicit/explicit implementations, inherited and reimplemented mappings, virtual overrides, and closed generic interfaces with reference-type covariance and contravariance. `is`/`as`/explicit casts work with nominal interface membership, including class references stored as `object`. Interface contracts may come from a separate supplied assembly. See the [interface execution snapshot](Tests/Wasm/Interfaces/Snapshots/Units.md).

This removes the basic interface dispatch/MethodImpl blocker. It does not yet implement boxed/constrained struct dispatch, representation-changing variance adapters, static-abstract interface dispatch, generic virtual methods, or every signature used by `IContext`. Interface methods still use the existing supported argument/result ABI. Dependency-defined numeric structs and the initial NFloat intrinsic set now execute; ordinary Canvas signatures can now use supported struct values; remaining framework types and operations still need implementation.

## Exception prerequisite — initial support implemented

Failed casts and null instance access now throw managed exceptions through native
WASM tags. Typed catches, throw/rethrow, finally/fault, cross-method propagation,
and frame-stack restoration are executable. The [exception demo](Tests/Wasm/Exceptions/Snapshots/Casts.md)
shows failed interface casts caught before execution continues. Six built-in exception
types have intrinsic constructors and messages; filters, custom exception subclasses,
managed stack traces, and multiple normal exits from one protected region remain unsupported.
This enables basic validation and cleanup paths; it does not establish arbitrary BCL
or disposal coverage. Native traps still require discarding the instance.

## Milestone 1 — one visible frame

### 1. Support dependency-defined linear structs, then add the real-Core fixture

- [x] Prove separate-assembly compilation with three fixtures, direct/transitive calls, a shared output module, and unused-class pruning. See [ThreeAssemblies](Tests/Wasm/Repl/MultiAssembly/Snapshots/ThreeAssemblies.md).
- [x] Add a small separate-library fixture with a numeric struct, a nested numeric struct, and overlapping metadata row numbers in the application and library. Exercise `default`, field stores/loads, and value copies in Node. This isolates module ownership from NFloat and browser concerns.
- [x] Extend linear storage classification/layout/frame allocation to resolve each type in its defining module. Module-bound field maps can remain local to their owning layout; any shared field map must use a qualified identity. Include nested value fields across dependencies. The subsequent linked-class step also covers inherited GC field maps and cross-assembly virtual calls.
- [x] Supply the compiled Xui Core implementation assembly alongside the sample entry point. Keep the normal Xui numeric aliases. Supplying Core must not root every declaration. Source linking is an optional temporary fallback only if this specific path reveals a hard stop.
- [x] Start with `Rect` and `Color` values, scalar field access, literal text, and calls to a small host adapter. Avoid `Window`, `View`, DI, tuples, collections, events, and optional services in this first fixture.
- [x] Use default initialization plus field assignments where this avoids an unsupported constructor. That is an initial test shape, not a permanent requirement for application code.
- [x] Add a REPL-style Markdown snapshot with source, discovery status/dependency graph, output, and WAT or the precise unsupported operation. Keep the fixture visibly inconclusive until execution succeeds; never replace rendering with a silent no-op.

**Gate:** the independent numeric-struct fixture executes across assemblies. The real-Core fixture compiles to PE using actual Xui types and records its dependency closure. Every remaining failure has a concrete method and, where applicable, IL offset.

### 2. Implement only the numeric support that fixture needs

- [x] Start with numeric-to-NFloat and NFloat-to-scalar conversions used by assignments and host arguments. Avoid emitting the general BCL NFloat implementation; recognize a narrow, identity/signature-checked intrinsic set.
- [x] Treat `NFloat` consistently as the target's scalar representation. Under the existing WASM32 layout policy this is `f32` storage; preserve the chosen MSIL floating evaluation behavior and round at storage/call boundaries as needed.
- [x] Add narrowly identified intrinsics for the required `NFloat` conversions/operators. Apply the same rules in dependency discovery, stack typing, storage binding, and code emission. An emission-only rewrite is insufficient if earlier analysis rejects the type or call.
- [x] Make the selected geometry/color fields work with that scalar representation, including initialization, loads, and stores. Use normalized color component assignments initially, avoiding the byte/packed Color constructors and their division calls. Add only arithmetic actually used by the fixture; general division is not a first-frame prerequisite.
- [x] Test numeric arguments delivered to host imports, including fractional values and f32 rounding. A native 64-bit managed run uses a different NFloat width; use explicitly rounded WASM32 reference calculations rather than treating host-native NFloat results as the target oracle.

Use the existing linear-memory struct frame where it works. **Do not make the full flattened-struct redesign, general byref support, or reference-slot runtime prerequisites for the first pixels.** Introduce flattened values as a subsequent representation optimization and ABI improvement, with the existing storage path available when an address is required.

**Gate:** real Rect/Color locals can be initialized, copied, read, and converted to scalar host arguments in generated WASM. The recording-host gate below verifies the complete drawing sequence. Referencing unused Core code must not pull its methods into the module.

### 3. Add a small drawing import contract

The initial scene is a background, two colored rectangles, and a literal label. Use actual `Rect` and `Color` locals, with at least one coordinate derived in C#, then extract scalar fields at the host boundary. This establishes geometry execution and rendering without claiming `IContext`, `Window`, or `View` support.

- [x] Define explicit host operations such as `clear(r,g,b,a)`, `fill-rect(x,y,width,height,r,g,b,a)`, and `fill-text(text,x,y,size)` using scalar arguments and native JavaScript strings.
- [x] Supply these operations through the rewriter/import infrastructure. Keep browser policy outside `CodeEmitter`; match specific adapter methods by identity/signature.
- [x] Have the C# adapter read Xui values and pass their scalar components. Until struct arguments work, the initial sample can perform this extraction at its call sites.
- [x] Implement the same contract with a recording host in Node and a Canvas 2D host in standalone HTML. Editor-playground integration remains pending. Fonts, gradients, images, clipping, composition, and text measurement are outside the first contract; unsupported calls must report that fact.

**Gate:** the Node command snapshot proves geometry/color calculations executed in our WASM, rather than being recreated in JavaScript.

### 4. Standalone output implemented; playground integration pending

- [x] Generate self-contained HTML artifacts for three scenes, with embedded JS and WASM.
- [x] Verify offline file opening, command output, pixels, labels, reruns, and narrow viewports.

The following editor integration remains pending:

- [ ] Add a canvas output area and a rendering example to the existing playground.
- [ ] For the first static frame, record drawing imports in the execution worker and send one command batch to the page after `main` completes. The page replays that batch on Canvas 2D. This avoids making worker-owned canvas support or synchronous cross-thread measurement a first-frame dependency.
- [ ] Preserve compile/runtime errors and console output alongside the canvas. Stop/rerun must clear the previous run and ignore stale worker messages.
- [ ] Embed the Core implementation PE and the host-adapter material needed by the browser compiler. Use the same Core bytes as a Roslyn metadata reference and as a supplied XuiNet assembly. Do not substitute the build reference assembly or browser WebCIL asset when executable method bodies are needed. Keep the host adapter in the application PE initially to avoid unnecessary extra packaging.
- [ ] Add a browser smoke test that checks a known rendered region and text command, and retain a Node snapshot of the identical drawing contract.

**Gate:** clicking Run shows a rectangle and literal label produced by Xui-generated WASM. The edited program is not running on the framework CLR/Mono runtime. The compiler may still be hosted by .NET.

## Milestone 2 — persistent rendering and input

- [ ] Allow explicit exported roots beyond `main`, initially scalar entry points such as `initialize`, `frame(width,height,time)`, and `pointer(x,y,buttons)`. Adding preservation roots currently does not create exports.
- [ ] Keep the generated module instance alive for an execution session. Adapt the playground's one-shot completion and timeout policy so an interactive app can run until stopped while each callback remains bounded.
- [ ] Add minimal application-state storage. Prefer ordinary static field support and defined initialization behavior; a narrowly defined host-owned state handle is an alternative if it gets the sample working sooner.
- [ ] Deliver animation ticks and pointer input from the page to the worker in order. Initially render only when requested or invalidated. Keep the drawing batches simple; move rendering to a worker-owned canvas only if needed for responsiveness or synchronous measurement.
- [ ] Extend the sample with a rectangle that changes position/color on pointer input and retains state across frames.

**Gate:** the same WASM instance receives repeated callbacks, updates C# state, and redraws. Stop and rerun release the previous instance and its host resources.

## Milestone 3 — the real Window/View path

Start from a minimal custom Xui window and one drawing view. Existing BlankApp is not a small compiler smoke test: it brings hosting/DI, scrolling, animation, text formatting, icons, and many framework dependencies.

Implement features in the order demonstrated by this fixture's dependency closure:

| Requirement | Why Xui needs it | Initial scope |
| --- | --- | --- |
| Struct value ABI | `Point`, `Rect`, `Size`, and `Color` cross method boundaries and are returned by operators. | Flatten small values into parameters/results where practical; preserve value copies. Support constructors and instance receivers. |
| Managed byrefs | Layout, input, and render callbacks use `ref`, `in`, and addressable fields. | Preserve shared identity and aliasing across calls; handle local and embedded struct locations. Do not use general copy-in/copy-out. |
| Interface calls and MethodImpl | `IContext`, Actual contracts, and explicit `RootView.IContent` implementations. | Class-backed dispatch and explicit mappings implemented. Add the struct/byref signatures and any further interface features demonstrated by the actual closure. |
| General object and minimal type identity | `IServiceProvider.GetService(Type)`, casts, instrumentation names. | Nominal type tokens/equality and required reference conversions. A dictionary-backed or reflective DI container is unnecessary for the first app. |
| Static fields and initialization | Geometry constants, singleton/null services, host state, cached values. | Defined once-only initialization and per-specialization static state. Preserve initializer side effects. |
| Generic value types and methods | `Nullable<Rect>`, tuples, generic service helpers, collections. | Closed compile-time specialization; no runtime generic construction. |
| Arrays and a small BCL surface | Child storage, `List<T>`, spans and input buffers. | Implement or explicitly substitute only the exercised operations. Vector storage now exists; collection constructors, methods, and static state still need their own coverage. |
| Math and time operations | `NFloat`, `TimeSpan`, layout calculations. | Small, tested intrinsic set; preserve WASM32 numeric rules. |
| Delegates and exceptions | Event wiring, hot reload, disposal, validation paths. | Remove optional wiring in the compiler profile first; add real semantics when reachable application behavior requires them. |

**Gate:** instantiate a real Xui window/view under a minimal host, perform measure/arrange/render, and route one pointer event through framework code. Compare the draw/input results with a managed run of the same profile.

## Explicit escape hatches

Cuts must be visible in a named compiler sample/profile. Keep normal platform builds unchanged, test the profile on managed .NET as well, and document each omitted capability.

- **Hosting/DI:** replace `HostBuilder` with direct construction and a small service provider. This still needs the object/interface/type-token semantics used by that provider; bypassing Microsoft DI does not remove those automatically.
- **Cross-assembly compilation:** identity resolution, semantic signature matching, and module-qualified primitive/string static-call planning and emission now work with explicitly supplied PEs. Numeric struct layout and linear storage now resolve across modules. Cross-assembly GC field identities, runtime descriptors, and object emission follow when a fixture needs classes. Type forwarding remains deferred with BCL support. Load implementation assemblies when method bodies are required; reference assemblies are insufficient.
- **Optional framework features:** omit hot reload, DevTools, instrumentation, overlays, class-name overflow, image loading, or advanced composition in the dedicated profile if they obstruct the minimal sample. Replace optional services with explicitly selected minimal implementations where useful.
- **Retained field dependencies:** `View` contains `ClassNameCollection`, whose `HashSet<string>` overflow field contributes to its retained layout; `RootView` contains an overlay list. With our current conservative layout rules, unused methods alone do not remove these dependencies. A profile cut must remove/replace the relevant storage explicitly, or the compiler must learn that representation. Do not assume tree shaking will solve it.
- **Browser binding:** inject the narrow drawing import contract instead of compiling generated .NET JS interop machinery. Reuse the existing Browser runtime's mapping ideas, not its runtime dependency graph.
- **Text:** draw literal strings at explicit positions first. Real text layout later requires synchronous measurement in the execution context or a deliberately designed measurement protocol/cache; an asynchronous drawing batch cannot return a synchronous measurement result.
- **Exceptions:** basic managed catch/finally now works. Keep required cleanup and reject unsupported filters/custom exception types explicitly; an uncaught failure surfaces a host error.

No cut may fabricate successful rendering, silently ignore required operations, or erase observable initialization/mutation behavior.

## Allocation-sensitive event structs — reference-slot foundation implemented

The [stack event demo](Tests/Wasm/ReferenceSlots/Snapshots/Events.md) now runs a
`ref struct` containing numeric fields, a string, and an interface reference through
nested `ref` calls. Interface dispatch with ref event parameters, in/out parameters,
local aliases, and value copies are tested. Linear fields hold 32-bit indices into
two GC-visible tables, reserved per frame and cleared on return or unwinding.
Repeated exceptions and direct host inspection verify slot restoration and clearing.
The tables have a fixed initial capacity; there is no per-event holder allocation.

This establishes the storage mechanism, not complete Xui event dispatch. Spans,
byref fields, dependency-defined class storage, and the remaining geometry ABI still need work.


- Flatten values into locals when no shared runtime location is needed.
- Use linear-memory frames for reference-free values requiring an address.
- Implemented for selected addressable structs: frame-owned GC-visible reference slots, with indices in the linear representation.
- Passing byref shares locations; copying by value copies reference values into independent destination slots. Returned/stored values must transfer references before the source frame is reclaimed.
- Clear reclaimed slots, restore stack marks, and handle reentrant calls and exceptional exits. Traps need host-boundary cleanup if the module instance will be reused.
- Preallocated capacity can avoid per-event allocations; initial allocation and growth remain explicit costs. This representation does not promise native CLR object-pointer layout.

Allocation-free full Xui event dispatch remains a separate acceptance gate: the reference-slot mechanism is implemented, but actual events also need their backing-storage adapters and additional framework dependencies; spans over inline linear storage now execute.

## Immediate next work

1. Expand the injected browser catalogue with ordinary layout, widget and gesture
   examples; verify real scrolling/inertia and navigation transitions visually.
2. Route the complete Window update/render lifecycle, then the EmulatedWindow
   composition, keeping the current persistent-session tests as regression gates.
3. Add browser keyboard/IME and multitouch adapters with explicit cleanup tests.
4. Bring the same Core assemblies and host adapter into the playground worker.
5. Expand assets, async services and optional demos only as their concrete
   dependency closures become supported. Do not claim full catalogue coverage.

General BCL coverage is still incremental. Keep the reachability graph as the
explanation of what each added feature brings into the emitted module.

## Themed TestApp home now renders

The browser-catalogue fixture now constructs a real Window/RootView, activates
TestAppDesignRoot, and renders the mobile home with actual theme tokens. Its
self-contained [HTML](Tests/Wasm/TestApp/Snapshots/BrowserShell.html) embeds the
WASM, Canvas host and Inter fonts. Node records the first frame; Chromium checks
pixels and offline execution. Optional diagnostics use constructor injection,
leaving normal TestApp builds unchanged.

This step required managed-owner spans/inline arrays, sin/cos and floating
remainder, signed NFloat conversions, shared exception-block type reuse and
chunked delegate metadata above the engine's fixed-array initializer limit.
The subsequent interactive gate now passes in BrowserInteractive: persistent shell
state, actual routed touches and animation updates inside the middleware phone
renderer. BrowserShell remains the first-frame snapshot and MobilePhone the smaller
FillRect fixture. Full catalogue assets/async/GPU demos remain outside the profile.
