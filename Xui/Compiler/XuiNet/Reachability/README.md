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

Assembly and type roots default to `MemberPolicy.All`. `PublicApi` selects externally visible types and public/protected members, then follows their private dependencies. Type roots preserve declared methods, constructors, accessors, and fields. Open generic type roots preserve the template and apply their policy to each discovered closed specialization; they do not invent every possible argument list. Generic methods and cross-assembly member resolution remain unsupported.

Requirements distinguish type identity, complete instance layout, instantiated classes, method bodies, fields, virtual calls, possible type initialization, string literals, and host rewrites. Field edges distinguish reads, writes, and addresses. Retained layouts keep every instance field, including inherited and nested value fields, so pruning methods does not change offsets or struct copies. Reference fields retain their declared layouts without implying allocations or retaining all their methods. Initializer dependencies are conservative: they preserve possible side effects without scheduling eager execution. Static storage and initializer emission are still unsupported by WASM.

An interned work queue visits each requirement once. Each specialized method is scanned at reachable IL offsets, using control-flow successors without requiring WASM emission or stack analysis. Exception regions and unresolved executable dependencies fail explicitly. An invoked virtual slot and a compatible instantiated type jointly retain an implementation; the edge's `Condition` records that allocation requirement. Indexing allocations by their class ancestors handles either discovery order and avoids repeatedly comparing every method with every class. Slot declarations currently retain their bodies as well as signatures, matching the conservative behavior of the existing emitter.

This is a closed-world analysis. Instance method roots account for a concrete receiver supplied by a host. Additional possible receiver types must be rooted explicitly; dynamic loading, reflection-based discovery, and unknown external subclasses are not inferred. Existing generic argument depth limits reject endlessly expanding specialization. Public API preservation is not a promise of arbitrary future generic specializations or dynamically loaded extensions.

An optional `InstructionRewriter` runs once per reachable instruction. Returning true records a `Rewrite` boundary and suppresses the original instruction's managed dependencies; the host owns the replacement's requirements. The WASM adapter uses this for the test/playground console imports. `System.Object..ctor` is a separate known intrinsic boundary. Neither mechanism pulls the original BCL implementation into the graph.

`WasmEmitter.Emit` always roots the managed entry point and accepts `additionalRoots`. It consumes the graph's methods and dispatch slots, and exposes the graph as `WasmEmitter.Dependencies` after successful emission. Preserving a method does not export it. Library-only graph analysis is supported; library-only WASM exports are not implemented yet.

The [reachability tests](../../Tests/XuiNet/Reachability/ReachabilityTests.cs) snapshot entry-point, type, public-API, assembly, and generic dispatch closures. Execution checks cover unused classes leaving the WASM bytes unchanged, explicit preservation, recursive dependencies, host rewrites, and virtual targets discovered through other virtual implementations.
