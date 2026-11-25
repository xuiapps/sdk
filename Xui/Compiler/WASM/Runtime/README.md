# Runtime class metadata

`TypeRegistry` resolves a linked class's base chain before laying out virtual slots. `Type` is the compiler-side view exposed by `WasmEmitter.Types`: it includes nominal identity, name, base type, flattened field ranges, slot declarations and implementations, and WASM locations. Source identities include their defining module and generic context; emitted indices belong to the single output WASM module. Inherited fields retain their declaring identities, so identical metadata row numbers across PEs do not collide. Keep the source PE assembly alive when inspecting source handles. GC field ranges are indices, not CLR byte offsets.

Each class object begins with an `eqref` descriptor header. Derived GC struct types retain their base's field prefix and append their own flattened fields. The immutable descriptor contains an ID, base descriptor, function-table start, a packed UTF-16 name array, slot count, and field count. Descriptor globals are initialized parent first. The name array can later be converted by the existing JS string builtin without a dynamic data-segment lookup. Unboxed GC struct values retain their existing copy representation; they do not receive class headers.

A virtual call adds its inherited slot index to the runtime descriptor's table start and uses `call_indirect`. Reachability follows instantiated classes and invoked slots to include overrides; unused or abstract entries remain null. Explicit base calls keep their method target. `isinst` walks descriptor parent links using reference identity, so WASM structural type equivalence never substitutes for .NET nominal identity. `castclass` preserves null and throws a managed InvalidCastException for failed casts.

This is the foundation for a managed `Type` facade. Reflection, `GetType`, `typeof`, unresolved external hierarchies, and boxing are still unsupported. The internal object header and dispatch table are not exported for host mutation. Binary subtype and indirect-call encodings follow the [WebAssembly core specification](https://webassembly.github.io/spec/core/bikeshed/).

Closed generic classes carry a structurally comparable argument vector in `TypeIdentity`. `MethodIdentity` combines the declaring specialization with the PE method handle, so constructors, ordinary members, and virtual implementations receive separate bodies for each reachable argument list. Base arguments are substituted at every level; inherited fields and virtual slots retain their constructed declaring type. Override matching compares declarations in the overriding type's open context, before specialization can collapse overloads such as `F(T)` and `F(int)`. Each specialization has its own descriptor and table range, including when two specializations have structurally identical WASM layouts. The compiler-side `Type` exposes the generic definition and argument list; runtime descriptors remain nominal class descriptors. Generic reflection metadata is not yet serialized for a managed `Type.GetGenericArguments` implementation.

Interface dispatch uses a shared prefix of each class table. Class mappings take precedence over default bodies; otherwise the most-specific interface implementation supplies the function. A signature-only default declaration has no emitted function until selected or called directly. Nominal descriptors also record interface membership for `is`/`as` and casts. Variance, static-abstract dispatch, and boxed/constrained receivers remain unsupported.

Closed generic interface methods now allocate a distinct dispatch slot per reachable
method argument list. Implementation selection preserves arguments through implicit,
explicit, inherited, variant, and default interface mappings. Class overrides are
selected before specialization without confusing hidden `new virtual` slots.
Only reachable implementations on instantiated receiver types are emitted. Direct
generic class-virtual calls remain unsupported; interface calls to generic class
implementations are supported. Tests cover separate int/string/i64 specializations,
struct results and byrefs, default overrides, and linked assemblies.

The shared [JavaScript host source](xui-runtime.js) defines `xuiRuntimeImports`.
Spread it into the import object when instantiating generated modules. The compiler
imports `xui-math.pow` only when reachable code uses Math/MathF/Double/Single/NFloat
Pow, and `xui-math.sin/cos` for reachable trigonometry. Pow's IEEE identities account for the differences in JavaScript Math.pow;
MathF and WASM32 NFloat operands/results round to f32 in generated WASM.
Floating remainder imports `xui-math.remainder`, implemented with JavaScript `%`.
Finite transcendental results depend on the host math implementation and need not
be bit-identical across platforms. The same source is embedded for Node/standalone
HTML hosts and linked into the playground worker's published assets.
