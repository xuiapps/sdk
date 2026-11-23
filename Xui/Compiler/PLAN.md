# Plan: display a Xui app using the Xui WASM compiler

## Objective and shortest path

Produce a visible canvas frame from C# compiled through **Roslyn → PE/MSIL → XuiNet → our WASM emitter**. Start with a small rendering slice using real Xui geometry and color types, then grow it into a persistent, interactive Xui window/view application.

The route is a dedicated compiler sample that supplies Xui Core and the sample as separate implementation assemblies, uses a small explicit host adapter, and draws rectangles and text. Primitive/string calls across three assemblies now work in one WASM module. The next linking step is narrowly scoped to linear-memory structs defined in a dependency assembly; cross-assembly GC objects are not a prerequisite for the first frame. Source linking remains a fallback, not the default route. It does not start through `HostBuilder`, Microsoft DI, or the existing browser runtime. The framework .NET WASM runtime may continue hosting Roslyn in the playground; it must not execute the sample application.

The first visible frame is deliberately a **Xui canvas slice**, not a claim that arbitrary `Application`, `Window`, or `View` code works. A later acceptance gate requires the real framework lifecycle, rendering, and input paths.

## Reassessment after multi-assembly linking

Reassessed on 2026-11-23 by inspecting the current compiler and playground. The latest compiler suite passed **542 tests with one existing skip**. The [three-assembly demo](Tests/Wasm/Repl/MultiAssembly/Snapshots/ThreeAssemblies.md) runs in Node and returns 42. The rendering fixture below has **not yet been implemented or executed**.

The goal remains achievable as a small rendering slice, but assembling three PEs does not yet make real Xui geometry executable. The shortest sequence is now:

1. Prove an application can use an ordinary numeric struct from a separate DLL.
2. Use actual `Xui.Core.dll` `Rect`/`Color` locals and implement their needed `NFloat` operations.
3. Record drawing imports in Node, then replay the same commands on the browser canvas.

| Area | Current evidence | Work needed for the first frame |
| --- | --- | --- |
| Assembly linking | Explicit implementation assemblies resolve types and direct/transitive static calls into one WASM module. Only reachable methods are retained. | Supply the real Core implementation PE alongside the application; do not load the BCL implementation closure. |
| Foreign struct storage | `StorageModel.IsGc` rejects foreign definitions; `MemoryFrame.Layout` requires its own module. `LayoutResolver` is module-bound and rejects unresolved external value fields. | Resolve/classify/layout numeric structs using their defining modules, including nested fields. Key shared caches by module-qualified identities. |
| `NFloat` | WASM32 layout already assigns four bytes, but this does not implement numeric calls or a scalar evaluation-stack representation. | Add a shared target-aware policy for discovery, stack typing, storage, and emission. Implement only conversions and operations reached by the fixture. |
| Struct method ABI | Linear struct parameters, returns, constructors/instance receivers remain rejected. | Avoid them initially: initialize locals with `default`, assign/read public fields, and pass scalar values to the host. |
| Core declarations | `Rect` and `Color` each have four public `nfloat` fields. Their interfaces and nested type dependencies can still appear in the graph. | Inspect the actual closure; preserve required layouts without unnecessarily rooting interface methods or unrelated Core methods. |
| Browser execution | The worker already executes Xui-generated WASM, but supplies only console imports. | Add drawing imports, a bounded command batch, a canvas, and a rendering example. |
| Browser compilation | Only System.Runtime/System.Console reference metadata is embedded; XuiNet currently reads only the editor PE. | Embed Core implementation PE bytes, use their metadata in Roslyn, and register those same bytes in the XuiNet compilation context. Keep PE lifetimes valid through emission. |

[Storage selection](WASM/Emitter/StorageModel.cs), [memory frames](WASM/Emitter/MemoryFrame.cs), [layout resolver](XuiNet/Layout/LayoutResolver.cs), [method ABI](WASM/Emitter/MethodSignature.cs), [playground compiler](Playground/PlaygroundCompiler.cs), and [execution worker](Playground/wwwroot/execute-worker.js) are the concrete change points.

Earlier probes of the full framework found unsupported object signatures in Application/Window/View constructors and explicit MethodImpl mappings in RootView. Those findings predate linking and interface support and are not a fresh full-app execution result. Object signatures for class-backed references and explicit interface MethodImpl now have support; rerun the full-framework probe before treating these old first failures as current blockers. The existing Browser runtime's generated .NET JS interop is also not a binding layer for our emitted modules.

## Interface prerequisite — implemented

Class-backed interface calls now support implicit/explicit implementations, inherited and reimplemented mappings, virtual overrides, and closed invariant generic interfaces. `is`/`as`/explicit casts work with nominal interface membership, including class references stored as `object`. Interface contracts may come from a separate supplied assembly. See the [interface execution snapshot](Tests/Wasm/Interfaces/Snapshots/Units.md).

This removes the basic interface dispatch/MethodImpl blocker. It does not yet implement boxed/constrained struct dispatch, variance, default/static interface methods, generic methods, or every signature used by `IContext`. Interface methods still use the existing supported argument/result ABI. The next rendering prerequisites remain dependency-defined numeric structs and NFloat; ordinary Canvas calls also need their struct/byref ABI where exercised.

## Milestone 1 — one visible frame

### 1. Support dependency-defined linear structs, then add the real-Core fixture

- [x] Prove separate-assembly compilation with three fixtures, direct/transitive calls, a shared output module, and unused-class pruning. See [ThreeAssemblies](Tests/Wasm/Repl/MultiAssembly/Snapshots/ThreeAssemblies.md).
- [ ] Add a small separate-library fixture with a numeric struct, a nested numeric struct, and overlapping metadata row numbers in the application and library. Exercise `default`, field stores/loads, and value copies in Node. This isolates module ownership from NFloat and browser concerns.
- [ ] Extend linear storage classification/layout/frame allocation to resolve each type in its defining module. Module-bound field maps can remain local to their owning layout; any shared field map must use a qualified identity. Include nested value fields across dependencies. Leave GC classes, inherited GC field maps, and cross-assembly virtual calls for a separate step.
- [ ] Supply the compiled Xui Core implementation assembly alongside the sample entry point. Keep the normal Xui numeric aliases. Supplying Core must not root every declaration. Source linking is an optional temporary fallback only if this specific path reveals a hard stop.
- [ ] Start with `Rect` and `Color` values, scalar field access, literal text, and calls to a small host adapter. Avoid `Window`, `View`, DI, tuples, collections, events, and optional services in this first fixture.
- [ ] Use default initialization plus field assignments where this avoids an unsupported constructor. That is an initial test shape, not a permanent requirement for application code.
- [ ] Add a REPL-style Markdown snapshot with source, discovery status/dependency graph, output, and WAT or the precise unsupported operation. Keep the fixture visibly inconclusive until execution succeeds; never replace rendering with a silent no-op.

**Gate:** the independent numeric-struct fixture executes across assemblies. The real-Core fixture compiles to PE using actual Xui types and records its dependency closure. Every remaining failure has a concrete method and, where applicable, IL offset.

### 2. Implement only the numeric support that fixture needs

- [ ] Start with numeric-to-NFloat and NFloat-to-scalar conversions used by assignments and host arguments. Avoid emitting the general BCL NFloat implementation; recognize a narrow, identity/signature-checked intrinsic set.
- [ ] Treat `NFloat` consistently as the target's scalar representation. Under the existing WASM32 layout policy this is `f32` storage; preserve the chosen MSIL floating evaluation behavior and round at storage/call boundaries as needed.
- [ ] Add narrowly identified intrinsics for the required `NFloat` conversions/operators. Apply the same rules in dependency discovery, stack typing, storage binding, and code emission. An emission-only rewrite is insufficient if earlier analysis rejects the type or call.
- [ ] Make the selected geometry/color fields work with that scalar representation, including initialization, loads, and stores. Use normalized color component assignments initially, avoiding the byte/packed Color constructors and their division calls. Add only arithmetic actually used by the fixture; general division is not a first-frame prerequisite.
- [ ] Test numeric arguments delivered to host imports, including fractional values and f32 rounding. A native 64-bit managed run uses a different NFloat width; use explicitly rounded WASM32 reference calculations rather than treating host-native NFloat results as the target oracle.

Use the existing linear-memory struct frame where it works. **Do not make the full flattened-struct redesign, general byref support, or reference-slot runtime prerequisites for the first pixels.** Introduce flattened values as a subsequent representation optimization and ABI improvement, with the existing storage path available when an address is required.

**Gate:** real Rect/Color locals can be initialized, copied, read, and converted to scalar host arguments in generated WASM. The recording-host gate below verifies the complete drawing sequence. Referencing unused Core code must not pull its methods into the module.

### 3. Add a small drawing import contract

The initial scene is a background, two colored rectangles, and a literal label. Use actual `Rect` and `Color` locals, with at least one coordinate derived in C#, then extract scalar fields at the host boundary. This establishes geometry execution and rendering without claiming `IContext`, `Window`, or `View` support.

- [ ] Define explicit host operations such as `clear(r,g,b,a)`, `fill-rect(x,y,width,height,r,g,b,a)`, and `fill-text(text,x,y,size)` using scalar arguments and native JavaScript strings.
- [ ] Supply these operations through the rewriter/import infrastructure. Keep browser policy outside `CodeEmitter`; match specific adapter methods by identity/signature.
- [ ] Have the C# adapter read Xui values and pass their scalar components. Until struct arguments work, the initial sample can perform this extraction at its call sites.
- [ ] Implement the same contract with a recording host in Node and a Canvas 2D host in the playground. Fonts, gradients, images, clipping, composition, and text measurement are outside the first contract; unsupported calls must report that fact.

**Gate:** the Node command snapshot proves geometry/color calculations executed in our WASM, rather than being recreated in JavaScript.

### 4. Display that frame in the playground

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
| Arrays and a small BCL surface | Child storage, `List<T>`, spans and input buffers. | Implement or explicitly substitute only the exercised operations. Lists still require an array/storage strategy; specialization alone does not provide it. |
| Math and time operations | `NFloat`, `TimeSpan`, layout calculations. | Small, tested intrinsic set; preserve WASM32 numeric rules. |
| Delegates and exceptions | Event wiring, hot reload, disposal, validation paths. | Remove optional wiring in the compiler profile first; add real semantics when reachable application behavior requires them. |

**Gate:** instantiate a real Xui window/view under a minimal host, perform measure/arrange/render, and route one pointer event through framework code. Compare the draw/input results with a managed run of the same profile.

## Explicit escape hatches

Cuts must be visible in a named compiler sample/profile. Keep normal platform builds unchanged, test the profile on managed .NET as well, and document each omitted capability.

- **Hosting/DI:** replace `HostBuilder` with direct construction and a small service provider. This still needs the object/interface/type-token semantics used by that provider; bypassing Microsoft DI does not remove those automatically.
- **Cross-assembly compilation:** identity resolution, semantic signature matching, and module-qualified primitive/string static-call planning and emission now work with explicitly supplied PEs. Next extend numeric struct layout and linear storage across modules. Cross-assembly GC field identities, runtime descriptors, and object emission follow when a fixture needs classes. Type forwarding remains deferred with BCL support. Load implementation assemblies when method bodies are required; reference assemblies are insufficient.
- **Optional framework features:** omit hot reload, DevTools, instrumentation, overlays, class-name overflow, image loading, or advanced composition in the dedicated profile if they obstruct the minimal sample. Replace optional services with explicitly selected minimal implementations where useful.
- **Retained field dependencies:** `View` contains `ClassNameCollection`, whose `HashSet<string>` overflow field contributes to its retained layout; `RootView` contains an overlay list. With our current conservative layout rules, unused methods alone do not remove these dependencies. A profile cut must remove/replace the relevant storage explicitly, or the compiler must learn that representation. Do not assume tree shaking will solve it.
- **Browser binding:** inject the narrow drawing import contract instead of compiling generated .NET JS interop machinery. Reuse the existing Browser runtime's mapping ideas, not its runtime dependency graph.
- **Text:** draw literal strings at explicit positions first. Real text layout later requires synchronous measurement in the execution context or a deliberately designed measurement protocol/cache; an asynchronous drawing batch cannot return a synchronous measurement result.
- **Exceptions:** an uncaught failure may terminate the initial sample and surface a host error. Required catch/finally/disposal behavior cannot be silently dropped.

No cut may fabricate successful rendering, silently ignore required operations, or erase observable initialization/mutation behavior.

## Allocation-sensitive event structs

This is a follow-on runtime task, not a first-frame blocker.

- Flatten values into locals when no shared runtime location is needed.
- Use linear-memory frames for reference-free values requiring an address.
- For temporary structs containing object/string references, evaluate frame-owned slots in GC-visible reference tables, with indices stored in our linear representation.
- Passing byref shares locations; copying by value copies reference values into independent destination slots. Returned/stored values must transfer references before the source frame is reclaimed.
- Clear reclaimed slots, restore stack marks, and handle reentrant calls and exceptional exits. Traps need host-boundary cleanup if the module instance will be reused.
- Preallocated capacity can avoid per-event allocations; initial allocation and growth remain explicit costs. This representation does not promise native CLR object-pointer layout.

For the first real event demo, a GC holder is an acceptable correctness milestone if its allocation cost is reported. Allocation-free Xui event dispatch is a separate acceptance gate, not an implied property of `ref struct`.

## Immediate next work

1. **Dependency-defined numeric structs:** a focused fixture, module-qualified layout/storage, and Node execution. No browser or NFloat work in this first change.
2. **Actual Xui geometry:** a separate Core implementation PE, `default(Rect)`/`default(Color)` field operations, and the minimum shared NFloat intrinsic policy.
3. **Drawing contract:** scalar rectangle/text imports and a deterministic Node command snapshot proving the geometry came from generated WASM.
4. **Visible demo:** embed Core in the compiler host, add the canvas example and command replay, then verify pixels and text commands in the browser smoke test.

General BCL compilation, full struct ABI, cross-assembly GC classes, and the flattened-struct/reference-slot redesign are deferred from this first frame. The first follow-on for ordinary `new Rect(...)` and geometry method calls is linear struct constructors/receivers and value parameters/results. The real Window/View milestone additionally needs its remaining ABI, object-runtime, and static-state dependencies; it is a larger milestone than adding a canvas host.

Only after the static scene works should the demo grow persistent state, input, and the real framework lifecycle. Keep the reachability graph as the explanation of what each added feature brings into the emitted module.
