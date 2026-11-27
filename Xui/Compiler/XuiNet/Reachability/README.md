# Source reachability

`Analyzer.Analyze` builds a dependency graph before WASM planning. A reference to an assembly does not root its declarations: an application that never references a class does not read its bodies or emit its layout. The graph keeps source identities and explanatory edges; it does not assign WASM indices. Keep the owning PE assemblies alive while inspecting it.

```csharp
var application = Analyzer.Analyze([Root.EntryPoint(assembly)]);
var library = Analyzer.Analyze([new Root.Assembly(assembly, MemberPolicy.PublicApi)]);
var preserved = Analyzer.Analyze([
    Root.EntryPoint(assembly),
    new Root.Type(typeIdentity),
    new Root.Method(methodIdentity)
]);
```

Assembly and type roots default to `MemberPolicy.All`. `PublicApi` selects externally visible types and public/protected members, then follows their private dependencies. Type roots preserve declared methods, constructors, accessors, and fields. Open generic type roots preserve the template and apply their policy to each discovered closed specialization; they do not invent every possible argument list. Closed generic methods are specialized when reached. Bind separately read implementation assemblies with `new XuiNet.Compilation(app, libraryA, libraryB)` before analysis to resolve calls across their metadata tables. The context borrows these assemblies; keep them alive until analysis and emission finish. Assembly availability does not add roots. Names, versions, cultures, and public-key tokens must match; duplicate assembly names are rejected. Assembly type forwarders follow explicitly supplied facades and implementation PEs, with identity checks and cycle rejection. Multi-module assemblies remain unsupported.

An explicit `Compilation(assemblies, replacements)` can select target-runtime
implementations with `TypeReplacement(sourceDefinition, implementationDefinition)`.
Both definitions must be supplied and preserve class/struct/interface category and
generic arity. Named signatures, generic instantiations, inherited types and member
references resolve to the selected implementation, including MethodDef/FieldDef
calls from other code in the original assembly. The replacement owns its layout and
method bodies; unavailable members fail rather than falling back to CLR internals.
This is an intentional runtime implementation boundary, not a binary-layout alias
or automatic BCL compatibility. Resolve externally constructed preservation roots
through the compilation before supplying them. Bindings are fixed before resolution;
cycles and invalid mappings fail without partially binding the input assemblies.

Requirements distinguish type identity, complete instance layout, instantiated classes, method bodies, callable signatures, fields, virtual calls, possible type initialization, string literals, and host rewrites. Field edges distinguish reads, writes, and addresses. Retained layouts keep every instance field, including inherited and nested value fields, so pruning methods does not change offsets or struct copies. Reference fields retain their declared layouts without implying allocations or retaining all their methods. Strict initializers remain dependencies of calls and construction. `BeforeFieldInit` types defer initialization until static field access; static reads/writes/addresses and explicit preservation still retain their initializer. Unboxed value types do not acquire layout or dispatch requirements for unused interfaces or the Object/ValueType/Enum runtime marker bases. Static storage and initializer emission are still unsupported by WASM.

An interned work queue visits each requirement once. Each specialized method is scanned at reachable IL offsets, using control-flow successors without requiring WASM emission or stack analysis. Catch/finally/fault handlers are conservative reachability roots, so handler-only dependencies are retained. Supported framework exception constructors and Message getters are intrinsic dependencies. Filters, unsupported catch types, and unresolved executable dependencies fail explicitly. An invoked class or interface slot and a compatible instantiated type jointly retain an implementation; the edge's `Condition` records that allocation requirement. Indexing allocations by their class ancestors handles either discovery order and avoids repeatedly comparing every method with every class. Default interface declarations retain a `Signature` requirement; their bodies are retained only when selected for an instantiated receiver or otherwise directly rooted/called. Class slot declarations keep the existing conservative body retention.

This is a closed-world analysis. Instance method roots account for a concrete receiver supplied by a host. Additional possible receiver types must be rooted explicitly; dynamic loading, reflection-based discovery, and unknown external subclasses are not inferred. Existing generic argument depth limits reject endlessly expanding specialization. Public API preservation is not a promise of arbitrary future generic specializations or dynamically loaded extensions.

An optional `InstructionRewriter` runs once per reachable instruction. Returning true records a `Rewrite` boundary and suppresses the original instruction's managed dependencies; the host owns the replacement's requirements. The WASM adapter uses this for the test/playground console imports. `System.Object..ctor` is a separate known intrinsic boundary. Neither mechanism pulls the original BCL implementation into the graph.

`WasmEmitter.Emit` always roots the managed entry point and accepts `additionalRoots`. It consumes the graph's methods and dispatch slots, and exposes the graph as `WasmEmitter.Dependencies` after successful emission. Preserving a method does not export it. Library-only graph analysis is supported; library-only WASM exports are not implemented yet.

The [reachability tests](../../Tests/XuiNet/Reachability/ReachabilityTests.cs) snapshot entry-point, type, public-API, assembly, and generic dispatch closures. Execution checks cover unused classes leaving the WASM bytes unchanged, explicit preservation, recursive dependencies, host rewrites, and virtual targets discovered through other virtual implementations.

Interface mappings include explicit MethodImpl declarations and inherited public implementations. Base mappings survive method hiding unless the derived class reimplements the interface; virtual overrides still update the selected target. Generic mapping compares open signatures before substitution so `F(T)` does not collapse into `F(int)`. Uncalled interface implementations are not rooted. The frozen graph exposes reachable receiver/interface-method mappings through `InterfaceImplementation`.

Default interface selection first searches class mappings, then chooses the most-specific interface MethodImpl. Abstract overrides suppress less-specific bodies; incomparable defaults or a remaining abstract target fail explicitly. `new` interface members keep separate slots. Unselected defaults do not retain their body dependencies or trigger strict type initialization.

Closed generic interface methods now allocate a distinct dispatch slot per reachable
method argument list. Implementation selection preserves arguments through implicit,
explicit, inherited, variant, and default interface mappings. Class overrides are
selected before specialization without confusing hidden `new virtual` slots.
Only reachable implementations on instantiated receiver types are emitted. Direct
generic class-virtual calls remain unsupported; interface calls to generic class
implementations are supported. Tests cover separate int/string/i64 specializations,
struct results and byrefs, default overrides, and linked assemblies.
