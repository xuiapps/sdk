# WASM backend support

The backend emits a `void` or `int` entry point and reachable, non-generic static and instance
methods. Primitive/string static calls can cross assemblies explicitly bound with
`XuiNet.Compilation`; all retained bodies share one output module and function index space.
Concrete classes, inheritance, constructors, virtual/interface dispatch, and casts can cross supplied assemblies. Inherited fields are keyed by their declaring type and metadata handle, preventing row-number collisions. Struct layouts and field access resolve declarations in supplied dependency assemblies. Method bodies support straight-line code, nested conditionals, and single-entry loops.
It supports integer and floating-point constants, arithmetic, bitwise operations,
shifts, numeric comparisons, selected conversions, primitive local and argument loads/stores, stack duplication/discard, and string literals.
MSIL `Int32` and `Int64` use WASM `i32` and `i64`; floating-point evaluation values
use `f64`. Strings use `externref`, with passive UTF-16LE data materialized through
`wasm:js-string.fromCharCodeArray` and cached in globals.

`WasmEmitter.Emit(..., namedExports: ...)` also roots and exports selected static
methods. Host calls accept full-width integer/float scalars, strings, and opaque
managed references; a returned GC class instance can be retained by JavaScript and
passed back across frame/input calls. Struct values, managed byrefs, narrow scalar
parameters, and instance methods require wrappers and currently reject. The `main`
export remains reserved. Export roots do not retain unrelated methods.

Classes from supplied assemblies use WASM GC objects with reference identity.
Structs classified as `ContainsManagedReferences` use GC objects with value-copy
semantics unless selected for addressable stack storage, described below. Their string fields remain native JavaScript `externref` values.
Nested struct fields are recursively flattened into their owner's GC fields,
including numeric structs: an embedded struct adds fields, not a child allocation.
Class-reference fields stay references, including cycles. Struct loads, duplication,
arguments and returns preserve independent values while copying embedded references
shallowly. Stores update existing struct storage so outstanding field addresses stay
valid. Numeric struct values transfer between linear memory and flattened GC fields.

Linked class and GC-struct constructors and direct instance calls are supported.
Class inheritance preserves a GC field prefix, with a runtime type descriptor at
field zero. Virtual and abstract class methods dispatch through a function table
using `call_indirect`; overrides replace inherited slots, while `new virtual`
methods append slots. Nonvirtual calls and explicit `base.Method()` calls remain
direct. `callvirt` checks its receiver for null. The parameterless core `System.Object`
constructor has no emitted body. Class equality compares identity; null assignment,
null tests, and default zero/null fields are supported. Null field access and null
instance calls throw catchable `NullReferenceException` objects. `is` and `as` lower to `isinst`, which follows nominal descriptor
parent links; identical field layouts do not make unrelated classes interchangeable.
`castclass` uses the same check and throws `InvalidCastException` on a non-null mismatch. See the
[GC objects demo](../Tests/Wasm/Gc/Snapshots/Objects.md) and
[polymorphism demo](../Tests/Wasm/Polymorphism/Snapshots/Units.md).

Interfaces implemented by classes support implicit public implementations, explicit
MethodImpl mappings, inherited implementations, reimplementation, and virtual overrides.
Inherited interfaces and closed generic interfaces retain nominal identity; reference-type
covariance and contravariance participate in dispatch and casts.
Each concrete class's function-table range starts with a shared interface-slot prefix;
only reachable interface calls retain their implementations. Interface `callvirt`
uses `call_indirect` with the same scalar/reference/struct-value ABI as class methods.
`is`, `as`, and explicit casts work between classes, interfaces, and class references
stored as `object`. Null casts preserve null; failed non-null casts throw `InvalidCastException`.
Interface membership is emitted as descriptor comparisons over the retained type set,
while class tests continue walking parent descriptors. Identical layouts do not imply
interface membership. Interface contracts and concrete classes may come from separate supplied assemblies.
See the [interface demo](../Tests/Wasm/Interfaces/Snapshots/Units.md).

[Runtime.Type](Runtime/Type.cs), exposed by `WasmEmitter.Types`, records source
identity, base type, implemented interfaces, flattened field ranges, class/interface dispatch slots, and WASM indices.
Immutable runtime GC descriptors contain identity, base descriptor, table start,
UTF-16 name array, slot count, and field count. This is groundwork for a managed
`Type` facade; `System.Type`, `GetType`, `typeof`, and reflection calls are not emitted yet.

Delegates use a compiler-owned GC object containing a nominal type descriptor,
retained target/closure, callable table slot, canonical method identity, and an
optional invocation list. `ldftn` binds a closed method through an adapter;
`ldvirtftn` captures the selected virtual/interface implementation. Canonical method
identity lets equality and removal match callbacks bound through different slots.
`Invoke` null-checks and calls a generated typed loop, forwarding reference byrefs
and caller-owned struct results. Handlers run in order; the last result is returned,
and an exception stops the remaining calls. Each handler gets independent by-value
struct arguments. Invocation creates no delegate wrapper or invocation list.

Two-operand `Delegate.Combine` and `Remove`, and the delegate equality operators,
are supported. Combine flattens lists; Remove removes the last matching contiguous
sequence. Published lists stay unchanged, including during reentrant subscription
changes. Null combinations and no-match removal reuse existing objects; a new
multi-handler result allocates a GC array and wrapper. Different nominal delegate
types cannot combine, even with identical signatures.
See the [multicast execution snapshot](../Tests/Wasm/Delegates/Snapshots/Multicast.md)
and [captured closures](../Tests/Wasm/Delegates/Snapshots/Closures.md).
Framework Action/Func definitions come from supplied implementation assemblies;
a dedicated test executes Action<Xui.Core.Math2D.Vector>.

Other delegate base APIs (including Equals/GetInvocationList/RemoveAll and the
params-array Combine overload), delegate-type variance, open-instance/closed-static
binding, and boxed value-type receivers remain unsupported. Compiler-generated
event accessors still need their Interlocked synchronization path supported.
Compiler-generated static caches for noncapturing lambdas/method groups still require
static-field and initializer support; explicit delegate construction and capturing
lambdas work. General `calli` and unmanaged function pointers remain unsupported.

Closed generic classes are specialized at compile time, including inheritance and
virtual/abstract dispatch.
`Box<int>` and `Box<string>` have distinct layouts, descriptors, and method plans;
member references resolve against their generic declarations before substituting
arguments. Constructors, ordinary instance/static methods, signatures, fields, and
locals receive the constructed context. [The Boxes demo](../Tests/Wasm/Generics/Snapshots/Boxes.md)
executes both specializations in one module. `Runtime.Type` exposes their generic
definition and arguments on the compiler side. Base arguments are substituted at
each inheritance level; derived layouts preserve the specialized base's field
prefix. Overrides reuse inherited slots, while `new virtual` declarations append
slots. Override matching uses the open declaring context so `F(T)` and `F(int)`
stay distinct when `T` is `int`. [The DerivedBoxes demo](../Tests/Wasm/Generics/Snapshots/DerivedBoxes.md)
shows specialized fields, tables, base calls, and nominal `is`/`as` tests across a
three-level hierarchy. Reference `default(T)` via `initobj` is supported; primitive generic object operations remain unsupported. Expanding instantiations stop at a nesting depth of 32.

Generic virtual methods, open generic structs, arbitrary external calls, switches, arbitrary
jump graphs, multidimensional arrays, unmanaged pointers,
value-type boxing, and arbitrary BCL objects are not implemented. Class signatures and type tests require definitions in supplied implementation assemblies.
`object` storage currently holds emitted class/interface references and null, not strings,
boxed values, arrays, or arbitrary BCL objects. Unresolved external class hierarchies,
static-abstract interface dispatch, generic interface methods, boxed/constrained struct dispatch,
and explicit class MethodImpl/covariant return overrides remain unsupported. Explicit-layout structs cannot
be flattened into GC fields yet; their supported linear-memory layouts are unchanged.
Closed generic **methods** now specialize through MethodSpec tokens, including calls
across supplied assemblies, nested type/method arguments, recursion, and overloads.
Each cache key includes both the declaring type and method argument list. Open
method templates are expanded only by closed calls. Generic virtual dispatch remains
unsupported; `box T` is a no-op only when T specializes to a supported reference type.

Reference `ref`/`in`/`out` arguments and local aliases carry an `eqref` owner plus an
`i32` accessor selector. Generated getters/setters use constant WASM field indices.
No descriptor object is allocated per byref. Class fields (including reference leaves
inside flattened structs) keep their owner alive; saved refs keep pointing to the
original object after the source variable changes. Address-taken reference locals and
arguments use caller-owned GC-visible table slots, cleared on return and unwinding.
Strings use the same convention with externref getters/setters. Calls, constructors,
virtual/interface dispatch, ref locals, and conditional/loop stacks preserve the pair.
Primitive byref parameters, escaping byref returns, and arbitrary array-element
byrefs remain unsupported.

Struct `ref`/`in`/`out` parameters and local aliases work for linear stack locations. Addresses
of inline struct fields can be used within expressions and conditional arms, but
cannot be passed as instance receivers or carried across loops. Class-reference
joins widen to a common linked base class, including null/class joins.
Class references can also join an existing assignable interface or `object` stack type.
Other stack types still require exact matches; synthesizing a common interface for unrelated
class stack types remains unsupported.
Native-integer arithmetic, checked arithmetic, division/remainder, and floating-point
to integer conversions also remain unsupported.

Methods with control flow or named object/value types use [XuiNet.CodeFlow](../XuiNet/CodeFlow) to check the stack on
all reachable edges before emission. Each `if` declares the input suffix its arms
consume and the output suffix they produce; untouched values remain below the
block. Non-empty inputs, multiple results, nested `if`/`else`, ternaries, and early
returns are supported. The emitter restores the analyzed input state for the
second arm, checks emitted stack effects, and intersects definite local
initialization at joins. `while`, `do while`, `for`, nested loops, and structured
`break`/`continue` emit typed `block`/`loop` labels. Back edges pass loop parameters;
exits pass loop results. Loop-local stores contribute to definite initialization
only on paths that actually exit. Loops currently require one entry and one
shared continuation; multiple-entry cycles, loops without a continuation, and
arbitrary overlapping regions throw `Code flow jumps unsupported`. Short-circuit branches may share straight-line result/return tails; guard clauses may return early and rejoin the outer fallthrough. Linear struct values cross block boundaries as frame addresses. Dead instructions in these methods are omitted
without modifying the PE or its IL offsets.

Locals support `bool`, `char`, `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`,
`long`, `ulong`, `float`, `double`, `nint`, `nuint`, and `string`. Native integers
use a 32-bit WASM target representation. Stores narrow small integers and round
`float` to f32; loads restore the MSIL stack representation. `InitLocals` supplies
zero/null defaults; without it, each slot must be stored before it is read.
Unmanaged structs with a supported XuiNet layout use linear-memory local storage;
classes and reference-containing structs without stack-address requirements use GC locals. Unresolved external types
(such as `decimal`), boxed values, pointers, and pinned locals
remain unsupported. Primitive local addresses are not implemented.

Struct locals classified as `LinearMemory` occupy aligned slots in a per-invocation
frame in an internal, fixed 1 MiB memory. The mutable stack pointer starts at 16;
entry checks available capacity and reserves the frame, and every normal return
restores it. Modules with exception support also catch and rethrow exceptions around
framed methods to restore the pointer during unwinding, including throwing host imports.
Frame overflow and other WASM traps are not exceptions and do not run cleanup;
discard the instance after a trap. `InitLocals` clears local
storage on each invocation. Addressable struct locals currently require this flag.
`ldloca`, field addresses, primitive/nested field loads and stores, `initobj`,
`ldobj`, `stobj`, and `cpobj` use the calculated byte layout. Loads and `dup` copy
struct values into distinct frame temporaries; assignments copy bytes rather than
aliasing the source. Temporary slots are reused across iterations, not allocated
per iteration. GC struct/class block parameters and results use references; linear struct block parameters/results remain unsupported.

Linear struct arguments are copied into callee-owned frame slots. Argument loads snapshot
values, while `starg` copies into the existing slot so aliases remain valid. Struct
results use a trailing hidden destination address supplied by the caller, and are
copied before the callee releases its frame. Direct, virtual, and interface calls
share this ABI. Reference-containing stack values copy the references, never their
slot indices, so results survive callee cleanup and unwinding. Constructors and direct
instance receivers work with these value arguments too. Managed-address returns and
primitive/reference argument addresses remain unsupported. GC-backed structs retain
their GC copy representation. The [Point + Vector demo](../Tests/Wasm/Structs/Snapshots/PointVector.md)
now executes rather than reporting an unsupported ABI.

Virtual/interface `in` parameters preserve the framework `InAttribute` required
modifier in signature metadata. Stack analysis normalizes it to the existing managed
address representation; mutable addresses can be supplied to readonly parameters.
Unknown required modifiers still fail explicitly.
See the [linear-memory copy demo](../Tests/Wasm/Structs/Snapshots/LinearMemory.md).

Addressable structs containing strings, objects, or interfaces use compiler-owned
linear layouts with **32-bit reference-slot indices** in their reference fields.
Before method planning, explicit struct byref parameters and byref locals select
this representation for that type and its nested value fields. This selection is
module-wide, so every method agrees on its address representation. Other existing
GC struct values retain their previous representation. This is not native CLR
pointer layout, and explicit-layout reference-containing structs are rejected.

Two private WASM tables hold the actual references: `externref` for strings and
`eqref` for objects/interfaces. Each frame reserves a contiguous range, assigns
stable indices to its locals and value temporaries, and releases it by clearing
both tables on normal return or exception unwinding. Struct copies transfer the
references into the destination's own slots; they never copy indices or share
mutable slots accidentally. `initobj` clears values while preserving slot identity.
Copies to/from flattened class fields transfer the references as well. A callee
borrows a caller's address; it does not release the caller's slots. Nested calls
and reentrancy use independent frame ranges.

The tables are allocated once per module instance, currently with 65,536 entries
each. Calls and copies allocate no GC holders for these stack structs. Existing
object creation and first-use string materialization still allocate. Capacity
exhaustion traps, as for the byte stack; traps require discarding the instance.
Spans, byref fields, escaping byref returns, linear struct values carried across
control-flow blocks, and arbitrary GC interior references remain unsupported.
See the [stack event demo](../Tests/Wasm/ReferenceSlots/Snapshots/Events.md).

`System.Runtime.InteropServices.NFloat` has an intrinsic WASM32 scalar representation:
f32 in storage and method signatures, promoted to the floating evaluation category.
Conversions from Int32/Single/Double and back to Single/Double, plus addition,
subtraction, multiplication, division, comparisons, unary negation, Abs, Min, Max, Clamp,
IsFinite, IsNaN, IsInfinity, Pi, and NaN/infinity constants are recognized by framework identity and signature.
Min/Max preserve IEEE signed-zero and NaN behavior; Clamp follows Single.Clamp
(including NaN bounds) and throws ArgumentException for reversed bounds.
Operations use f32 rounding. The known System.Runtime.InteropServices/System.Runtime/
System.Private.CoreLib facade identities match across assembly field/method references.
Other NFloat members remain unsupported; BCL implementation bodies are not compiled
for these intrinsics. Supplied framework facades resolve type forwarders into the
supplied implementation PE while retaining the target's f32 NFloat representation.

The framework's immutable `TimeSpan.Zero` value load creates an independent zeroed
linear struct. Taking its static address (`ldsflda`) and general static storage/type
initialization remain unsupported. `IsExternalInit` required modifiers on void init
setter returns preserve the ordinary setter ABI; other unknown required modifiers reject.

Numeric layouts follow linked TypeRefs into their defining modules; caches and
field maps retain module ownership. The [standalone canvas demos](../Tests/Wasm/Canvas/README.md)
use real Rect and Color fields, execute Rect.Right from Xui.Core.dll, and expose scalar
drawing imports through a test host rewriter. Generated HTML includes all JS/WASM.

Static method parameters and results support the same primitive/string types as
locals, plus `void` results. Calls check argument types, preserve order, and narrow
small integers and `float` at storage boundaries. Function indices are assigned
before bodies are emitted, supporting forward and recursive references; unused
method bodies remain unread. Calls resolve MethodDef and MemberRef tokens across supplied assemblies.
Required type/module initializers, synchronized methods, and entry-point `string[] args`
access are rejected. `BeforeFieldInit` types defer initialization until a static field is
accessed, so unused static constants do not block ordinary geometry operations. `dup` uses reusable typed temporary locals.

Exception regions lower to native WASM `try_table`, a module-local exception tag,
`throw`, and `throw_ref`. Ordered typed catches, nested handlers, cross-method
propagation, bare `catch`, `throw;`, `finally`, and MSIL `fault` are supported.
`leave` clears the evaluation stack and routes through cleanup; `finally` runs on
normal and exceptional exits, while `fault` runs only during unwinding. Cleanup
handlers forward foreign host exceptions too; managed catches match our tag only.
Rethrow preserves the original exception reference. See the
[failed-cast demo](../Tests/Wasm/Exceptions/Snapshots/Casts.md).

The initial managed payload is a GC struct containing a nominal exception kind and
a native JavaScript message string. Recognized framework references are
`Exception`, `SystemException`, `InvalidCastException`, `NullReferenceException`,
`InvalidOperationException`, and `ArgumentException`. Their parameterless/string
constructors, `Message`, hierarchy tests, and casts work without compiling BCL bodies.
`throw null` throws `NullReferenceException`. Default messages are English;
cast failures use the default message rather than CLR-generated runtime type names.
Filters, custom exception subclasses, inner exceptions, managed stack traces, and
protected regions with multiple normal continuations remain unsupported. This is
an explicit intrinsic exception surface, not the full BCL exception implementation.
WASM traps are not converted to managed exceptions; currently emitted null and
cast checks throw before a trap can occur.

[CodeRewriter](Emitter/Rewrite/CodeRewriter.cs) can supply replacements before normal
emission. The default rewriter does nothing. The tests and browser playground plug in
[ConsoleWriteLineRewriter](../Playground/ConsoleWriteLineRewriter.cs) to map
selected `Console.WriteLine` overloads to JavaScript logging imports; this is not
a built-in Console special case. The built-in string conversion import remains part of
[EmissionRuntime](Emitter/EmissionRuntime.cs).

The table lists every canonical MSIL instruction and prefix in opcode order: all
218 entries in [XuiNet's OpCode](../XuiNet/DotNet/Op/OpCode.cs), plus the standard
`no.` prefix, which that enum omits. `no.` is defined in
[ECMA-335, III.2.2](https://www.ecma-international.org/wp-content/uploads/ECMA-335_6th_edition_june_2012.pdf#page=344).
Aliases share their canonical instruction's row: `brnull`/`brzero` mean `brfalse`,
`brinst` means `brtrue` (including short forms), `ldind.u8` means `ldind.i8`,
`ldelem.u8` means `ldelem.i8`, and `endfault` means `endfinally`.
Reserved encodings are not instructions.

Blank WASM and note cells mean unsupported by the core emitter. Populated cells
may describe only the supported operand types; unsupported types still throw.
`/` separates alternatives selected by operand type; `;` separates emitted
instructions. Optional `i32.wrap_i64` applies only to i64 inputs. Operands are
omitted except where needed to explain a translation. “No instruction” denotes
supported operations that require no emitted bytes.

| MSIL instruction | WASM translation | Implementation note |
| --- | --- | --- |
| `nop` | No instruction | Elided entirely; leaves the MSIL evaluation stack unchanged. |
| `break` |  |  |
| `ldarg.0` | `local.get`; optional `f64.promote_f32` | Loads arguments; struct values copy and floats promote to f64. |
| `ldarg.1` | `local.get`; optional `f64.promote_f32` | Loads arguments; struct values copy and floats promote to f64. |
| `ldarg.2` | `local.get`; optional `f64.promote_f32` | Loads arguments; struct values copy and floats promote to f64. |
| `ldarg.3` | `local.get`; optional `f64.promote_f32` | Loads arguments; struct values copy and floats promote to f64. |
| `ldloc.0` | `local.get`; optional `f64.promote_f32`, `memory.copy`, or `struct.new` | Loads class references; struct loads copy values into independent storage. |
| `ldloc.1` | `local.get`; optional `f64.promote_f32`, `memory.copy`, or `struct.new` | Loads class references; struct loads copy values into independent storage. |
| `ldloc.2` | `local.get`; optional `f64.promote_f32`, `memory.copy`, or `struct.new` | Loads class references; struct loads copy values into independent storage. |
| `ldloc.3` | `local.get`; optional `f64.promote_f32`, `memory.copy`, or `struct.new` | Loads class references; struct loads copy values into independent storage. |
| `stloc.0` | Optional `i32.extend8_s` / `i32.extend16_s` / `i32.const; i32.and` / `f32.demote_f64`; `local.set`, `memory.copy`, or `struct.set` | Copies structs; otherwise narrows scalars and marks the slot initialized. |
| `stloc.1` | Optional `i32.extend8_s` / `i32.extend16_s` / `i32.const; i32.and` / `f32.demote_f64`; `local.set`, `memory.copy`, or `struct.set` | Copies structs; otherwise narrows scalars and marks the slot initialized. |
| `stloc.2` | Optional `i32.extend8_s` / `i32.extend16_s` / `i32.const; i32.and` / `f32.demote_f64`; `local.set`, `memory.copy`, or `struct.set` | Copies structs; otherwise narrows scalars and marks the slot initialized. |
| `stloc.3` | Optional `i32.extend8_s` / `i32.extend16_s` / `i32.const; i32.and` / `f32.demote_f64`; `local.set`, `memory.copy`, or `struct.set` | Copies structs; otherwise narrows scalars and marks the slot initialized. |
| `ldarg.s` | `local.get`; optional `f64.promote_f32` | Loads arguments; struct values copy and floats promote to f64. |
| `ldarga.s` | `local.get` | Addresses owned struct parameter storage; primitive argument addresses remain unsupported. |
| `starg.s` | Optional narrowing; `local.set` | Replaces scalar arguments; copies structs into alias-preserving parameter storage. |
| `ldloc.s` | `local.get`; optional `f64.promote_f32`, `memory.copy`, or `struct.new` | Loads class references; struct loads copy values into independent storage. |
| `ldloca.s` | `local.get` | Returns a linear address or GC owner for struct locals. |
| `stloc.s` | Optional `i32.extend8_s` / `i32.extend16_s` / `i32.const; i32.and` / `f32.demote_f64`; `local.set`, `memory.copy`, or `struct.set` | Copies structs; otherwise narrows scalars and marks the slot initialized. |
| `ldnull` | `ref.null` | Untyped null adapts to class or host-string assignment storage. |
| `ldc.i4.m1` | `i32.const -1` |  |
| `ldc.i4.0` | `i32.const 0` |  |
| `ldc.i4.1` | `i32.const 1` |  |
| `ldc.i4.2` | `i32.const 2` |  |
| `ldc.i4.3` | `i32.const 3` |  |
| `ldc.i4.4` | `i32.const 4` |  |
| `ldc.i4.5` | `i32.const 5` |  |
| `ldc.i4.6` | `i32.const 6` |  |
| `ldc.i4.7` | `i32.const 7` |  |
| `ldc.i4.8` | `i32.const 8` |  |
| `ldc.i4.s` | `i32.const` | Sign-extends the signed byte operand before emitting the integer constant. |
| `ldc.i4` | `i32.const` |  |
| `ldc.i8` | `i64.const` |  |
| `ldc.r4` | `f32.const; f64.promote_f32` | Loads binary32 precision, then promotes to the f64 evaluation stack. |
| `ldc.r8` | `f64.const` |  |
| `dup` | `local.tee`; `local.get`; `memory.copy` or `struct.new` | Classes retain identity; struct duplicates receive independent value storage. |
| `pop` | `drop` | Discards runtime value and its tracked MSIL stack type. |
| `jmp` |  |  |
| `call` | Optional argument narrowing/spills; `call`; optional `f64.promote_f32` | Calls linked methods; struct results use caller-owned destination storage. |
| `calli` |  |  |
| `ret` | Optional result narrowing; `return` | Copies struct results to caller storage before releasing the frame. |
| `br.s` | `Structured sequencing / br` | Sequences regions or branches to typed loop exit/repeat labels. |
| `brfalse.s` | `zero/null test; if` | Consumes condition; typed arms may consume inputs and produce results. |
| `brtrue.s` | `truth test; if` | Tests integers or references; supports typed inputs and results. |
| `beq.s` | `i32.eq / i64.eq / f64.eq; if` | Numeric comparison consumes operands before entering the typed conditional block. |
| `bge.s` | `integer lt_s; i32.eqz / f64.ge; if` | Integer comparison inverted; floating-point comparison remains ordered for NaN. |
| `bgt.s` | `i32.gt_s / i64.gt_s / f64.gt; if` | Signed integers or ordered floats select the taken conditional arm. |
| `ble.s` | `integer gt_s; i32.eqz / f64.le; if` | Integer comparison inverted; floating-point comparison remains ordered for NaN. |
| `blt.s` | `i32.lt_s / i64.lt_s / f64.lt; if` | Signed integers or ordered floats select the taken conditional arm. |
| `bne.un.s` | `eq; i32.eqz; if` | Inverts equality; floating-point NaN makes the branch condition true. |
| `bge.un.s` | `integer lt_u / f64.lt; i32.eqz; if` | Unsigned integers; inverted ordered floating-point comparison includes NaN as true. |
| `bgt.un.s` | `integer gt_u / f64.le; i32.eqz; if` | Unsigned integers; inverted ordered floating-point comparison includes NaN as true. |
| `ble.un.s` | `integer gt_u / f64.gt; i32.eqz; if` | Unsigned integers; inverted ordered floating-point comparison includes NaN as true. |
| `blt.un.s` | `integer lt_u / f64.ge; i32.eqz; if` | Unsigned integers; inverted ordered floating-point comparison includes NaN as true. |
| `br` | `Structured sequencing / br` | Sequences regions or branches to typed loop exit/repeat labels. |
| `brfalse` | `zero/null test; if` | Consumes condition; typed arms may consume inputs and produce results. |
| `brtrue` | `truth test; if` | Tests integers or references; supports typed inputs and results. |
| `beq` | `i32.eq / i64.eq / f64.eq; if` | Numeric comparison consumes operands before entering the typed conditional block. |
| `bge` | `integer lt_s; i32.eqz / f64.ge; if` | Integer comparison inverted; floating-point comparison remains ordered for NaN. |
| `bgt` | `i32.gt_s / i64.gt_s / f64.gt; if` | Signed integers or ordered floats select the taken conditional arm. |
| `ble` | `integer gt_s; i32.eqz / f64.le; if` | Integer comparison inverted; floating-point comparison remains ordered for NaN. |
| `blt` | `i32.lt_s / i64.lt_s / f64.lt; if` | Signed integers or ordered floats select the taken conditional arm. |
| `bne.un` | `eq; i32.eqz; if` | Inverts equality; floating-point NaN makes the branch condition true. |
| `bge.un` | `integer lt_u / f64.lt; i32.eqz; if` | Unsigned integers; inverted ordered floating-point comparison includes NaN as true. |
| `bgt.un` | `integer gt_u / f64.le; i32.eqz; if` | Unsigned integers; inverted ordered floating-point comparison includes NaN as true. |
| `ble.un` | `integer gt_u / f64.gt; i32.eqz; if` | Unsigned integers; inverted ordered floating-point comparison includes NaN as true. |
| `blt.un` | `integer lt_u / f64.ge; i32.eqz; if` | Unsigned integers; inverted ordered floating-point comparison includes NaN as true. |
| `switch` |  |  |
| `ldind.i1` | `i32.load8_s` | Reads typed addresses; reference slots remain GC-visible. |
| `ldind.u1` | `i32.load8_u` | Reads typed addresses; reference slots remain GC-visible. |
| `ldind.i2` | `i32.load16_s` | Reads typed addresses; reference slots remain GC-visible. |
| `ldind.u2` | `i32.load16_u` | Reads typed addresses; reference slots remain GC-visible. |
| `ldind.i4` | `i32.load` / `struct.get` | Reads typed Int32/UInt32 addresses, including existing GC field locations. |
| `ldind.u4` | `i32.load` | Reads typed addresses; reference slots remain GC-visible. |
| `ldind.i8` | `i64.load` | Reads typed addresses; reference slots remain GC-visible. |
| `ldind.i` | `i32.load` | Reads typed addresses; reference slots remain GC-visible. |
| `ldind.r4` | `f32.load; f64.promote_f32` | Reads typed addresses; reference slots remain GC-visible. |
| `ldind.r8` | `f64.load` | Reads typed addresses; reference slots remain GC-visible. |
| `ldind.ref` | `table.get / struct.get` | Reads typed addresses; reference slots remain GC-visible. |
| `stind.ref` | `table.set / struct.set` | Updates typed addresses, including inline-array elements. |
| `stind.i1` | `i32.store8` | Updates typed addresses, including inline-array elements. |
| `stind.i2` | `i32.store16` | Updates typed addresses, including inline-array elements. |
| `stind.i4` | `i32.store` / `struct.set` | Updates typed Int32/UInt32 addresses without copying their owning struct. |
| `stind.i8` | `i64.store` | Updates typed addresses, including inline-array elements. |
| `stind.r4` | `f32.demote_f64; f32.store` | Updates typed addresses, including inline-array elements. |
| `stind.r8` | `f64.store` | Updates typed addresses, including inline-array elements. |
| `add` | `i32.add` / `i64.add` / `f64.add` | Type stack selects integer width or f64; integer overflow wraps. |
| `sub` | `i32.sub` / `i64.sub` / `f64.sub` | Type stack selects integer width or f64; integer overflow wraps. |
| `mul` | `i32.mul` / `i64.mul` / `f64.mul` | Type stack selects integer width or f64; integer overflow wraps. |
| `div` | `f64.div` | Floating operands only; integer division and its exceptions remain unsupported. |
| `div.un` |  |  |
| `rem` |  |  |
| `rem.un` |  |  |
| `and` | `i32.and` / `i64.and` | Requires two integers of matching width; floating-point operands are rejected. |
| `or` | `i32.or` / `i64.or` | Requires two integers of matching width; floating-point operands are rejected. |
| `xor` | `i32.xor` / `i64.xor` | Requires two integers of matching width; floating-point operands are rejected. |
| `shl` | `i32.shl` / `i64.extend_i32_u; i64.shl` | Widens int32 counts for i64; WASM masks the shift count. |
| `shr` | `i32.shr_s` / `i64.extend_i32_u; i64.shr_s` | Widens int32 counts for i64; WASM masks the shift count. |
| `shr.un` | `i32.shr_u` / `i64.extend_i32_u; i64.shr_u` | Widens int32 counts for i64; WASM masks the shift count. |
| `neg` | `i32.const -1; i32.mul` / `i64.const -1; i64.mul` / `f64.neg` | Integer negation wraps; floating-point negation preserves signed zero behavior. |
| `not` | `i32.const -1; i32.xor` / `i64.const -1; i64.xor` | XOR with all-one bits implements integer complement at either width. |
| `conv.i1` | Optional `i32.wrap_i64`; `i32.extend8_s` | Integer inputs only; narrow results occupy an int32 stack slot. |
| `conv.i2` | Optional `i32.wrap_i64`; `i32.extend16_s` | Integer inputs only; narrow results occupy an int32 stack slot. |
| `conv.i4` | No instruction (i32) / `i32.wrap_i64` (i64) | Keeps low 32 bits; floating-point inputs are currently rejected. |
| `conv.i8` | `i64.extend_i32_s` (i32) / no instruction (i64) | Sign-extends int32 inputs; already-wide integers retain their original bits. |
| `conv.r4` | `f32.convert_i32_s; f64.promote_f32` / `f32.convert_i64_s; f64.promote_f32` / `f32.demote_f64; f64.promote_f32` | Rounds directly to binary32, then restores the f64 stack representation. |
| `conv.r8` | `f64.convert_i32_s` / `f64.convert_i64_s` / no instruction (f64) | Signed integer conversion; floating-point stack values already use f64. |
| `conv.u4` | No instruction (i32) / `i32.wrap_i64` (i64) | Keeps low 32 bits; floating-point inputs are currently rejected. |
| `conv.u8` | `i64.extend_i32_u` (i32) / no instruction (i64) | Zero-extends int32 inputs; already-wide integers retain their original bits. |
| `callvirt` | `ref.cast`; `struct.get`; `call_indirect` or `call` | Null-checks receiver; class/interface slots or delegate entries select callable methods. |
| `cpobj` | `memory.copy` or leaf loads/stores | Copies matching structs, including transfers between GC and linear storage. |
| `ldobj` | `memory.copy` or `struct.get`; `struct.new` | Loads an independent struct value from its managed address. |
| `ldstr` | `global.get; ref.is_null; if; i32.const; i32.const; array.new_data; i32.const; i32.const; call; global.set; end; global.get` | Materializes passive UTF-16LE data through js-string; caches the JavaScript string. |
| `newobj` | Frame address / `struct.new_default`; `struct.new`; `call` | Constructs values/classes or binds a delegate target and callable. |
| `castclass` | Descriptor helper `call`; `unreachable` on mismatch | Preserves null; checks class ancestry/interface membership; failed casts trap. |
| `isinst` | Descriptor helper `call`; `ref.eq`; ancestry/membership checks | Returns original reference or null; supports class/interface is/as tests. |
| `conv.r.un` | `f64.convert_i32_u` / `f64.convert_i64_u` | Interprets integer bits as unsigned before conversion to floating point. |
| `unbox` |  |  |
| `throw` | `throw` | Throws managed payload; null becomes a NullReferenceException object. |
| `ldfld` | Numeric load / `memory.copy` / `struct.get` / `struct.new` | Reads linear or flattened GC fields; struct values are copied. |
| `ldflda` | `i32.add`; owner + accessor selector | Reference fields preserve owner identity through calls and local aliases. |
| `stfld` | Numeric store / `memory.copy` / `struct.set` | Writes linear or flattened fields; struct assignment copies leaf values. |
| `ldsfld` | `memory.fill`, frame address | Only framework TimeSpan.Zero value loads; other static fields unsupported. |
| `ldsflda` |  |  |
| `stsfld` |  |  |
| `stobj` | `memory.copy` or leaf loads/stores | Copies struct values into linear or flattened GC storage. |
| `conv.ovf.i1.un` |  |  |
| `conv.ovf.i2.un` |  |  |
| `conv.ovf.i4.un` |  |  |
| `conv.ovf.i8.un` |  |  |
| `conv.ovf.u1.un` |  |  |
| `conv.ovf.u2.un` |  |  |
| `conv.ovf.u4.un` |  |  |
| `conv.ovf.u8.un` |  |  |
| `conv.ovf.i.un` |  |  |
| `conv.ovf.u.un` |  |  |
| `box` | no instruction for reference specializations | Generic reference identity is preserved; value-type boxing remains unsupported. |
| `newarr` | `array.new_default` | Struct elements get independent mutable GC storage; negative lengths throw. |
| `ldlen` | `array.len` | Returns native-width length; null throws a managed exception. |
| `ldelema` | `array.get` | Struct element locations only; general array byref ABI remains unsupported. |
| `ldelem.i1` | `array.get` | Checks null/bounds; preserves declared element type and load promotion. |
| `ldelem.u1` | `array.get` | Checks null/bounds; preserves declared element type and load promotion. |
| `ldelem.i2` | `array.get` | Checks null/bounds; preserves declared element type and load promotion. |
| `ldelem.u2` | `array.get` | Checks null/bounds; preserves declared element type and load promotion. |
| `ldelem.i4` | `array.get` | Checks null/bounds; preserves declared element type and load promotion. |
| `ldelem.u4` | `array.get` | Checks null/bounds; preserves declared element type and load promotion. |
| `ldelem.i8` | `array.get` | Checks null/bounds; preserves declared element type and load promotion. |
| `ldelem.i` | `array.get` | Checks null/bounds; preserves declared element type and load promotion. |
| `ldelem.r4` | `array.get` | Checks null/bounds; preserves declared element type and load promotion. |
| `ldelem.r8` | `array.get` | Checks null/bounds; preserves declared element type and load promotion. |
| `ldelem.ref` | `array.get` | Checks null/bounds; preserves declared element type and load promotion. |
| `stelem.i` | `array.set` | Checks null/bounds; narrows numeric values and preserves reference identity. |
| `stelem.i1` | `array.set` | Checks null/bounds; narrows numeric values and preserves reference identity. |
| `stelem.i2` | `array.set` | Checks null/bounds; narrows numeric values and preserves reference identity. |
| `stelem.i4` | `array.set` | Checks null/bounds; narrows numeric values and preserves reference identity. |
| `stelem.i8` | `array.set` | Checks null/bounds; narrows numeric values and preserves reference identity. |
| `stelem.r4` | `array.set` | Checks null/bounds; narrows numeric values and preserves reference identity. |
| `stelem.r8` | `array.set` | Checks null/bounds; narrows numeric values and preserves reference identity. |
| `stelem.ref` | `array.set` | Checks null/bounds; narrows numeric values and preserves reference identity. |
| `ldelem` | `array.get; struct.get` | Struct loads copy values into independent evaluation-stack storage. |
| `stelem` | `array.get; struct.set` | Copies struct fields while preserving the destination element identity. |
| `unbox.any` |  |  |
| `conv.ovf.i1` |  |  |
| `conv.ovf.u1` |  |  |
| `conv.ovf.i2` |  |  |
| `conv.ovf.u2` |  |  |
| `conv.ovf.i4` |  |  |
| `conv.ovf.u4` |  |  |
| `conv.ovf.i8` |  |  |
| `conv.ovf.u8` |  |  |
| `refanyval` |  |  |
| `ckfinite` |  |  |
| `mkrefany` |  |  |
| `ldtoken` |  |  |
| `conv.u2` | Optional `i32.wrap_i64`; `i32.const 65535; i32.and` | Integer inputs only; narrow results occupy an int32 stack slot. |
| `conv.u1` | Optional `i32.wrap_i64`; `i32.const 255; i32.and` | Integer inputs only; narrow results occupy an int32 stack slot. |
| `conv.i` | No instruction (i32/native) / `i32.wrap_i64` (i64) | Native integers use 32 bits; floating-point inputs remain unsupported. |
| `conv.ovf.i` |  |  |
| `conv.ovf.u` |  |  |
| `add.ovf` |  |  |
| `add.ovf.un` |  |  |
| `mul.ovf` |  |  |
| `mul.ovf.un` |  |  |
| `sub.ovf` |  |  |
| `sub.ovf.un` |  |  |
| `endfinally` | `br`; `throw_ref` | Completes cleanup, then resumes normal leave or exception unwinding. |
| `leave` | `drop`; `br` | Clears evaluation stack and follows structured cleanup before continuation. |
| `leave.s` | `drop`; `br` | Short relative target; same cleanup behavior as long leave. |
| `stind.i` | `i32.store` | Updates typed addresses, including inline-array elements. |
| `conv.u` | No instruction (i32/native) / `i32.wrap_i64` (i64) | Native integers use 32 bits; floating-point inputs remain unsupported. |
| `arglist` |  |  |
| `ceq` | Numeric equality / `ref.eq` / `ref.is_null` | Compares numbers, class identity, or null; produces int32 Boolean. |
| `cgt` | `i32.gt_s` / `i64.gt_s` / `f64.gt` | Numeric operands only; pushes int32 0/1; NaN comparisons yield false. |
| `cgt.un` | `i32.gt_u` / `i64.gt_u` / `f64.le; i32.eqz` | Unsigned integers; inverted ordered floating comparison makes NaN yield true. |
| `clt` | `i32.lt_s` / `i64.lt_s` / `f64.lt` | Numeric operands only; pushes int32 0/1; NaN comparisons yield false. |
| `clt.un` | `i32.lt_u` / `i64.lt_u` / `f64.ge; i32.eqz` | Unsigned integers; inverted ordered floating comparison makes NaN yield true. |
| `ldftn` | `i32.const` | Captures a closed method adapter in the function table. |
| `ldvirtftn` | `struct.get`; `i32.add` | Null-checks receiver and captures its selected virtual/interface table entry. |
| `ldarg` | `local.get`; optional `f64.promote_f32` | Loads arguments; struct values copy and floats promote to f64. |
| `ldarga` | `local.get` | Addresses owned struct parameter storage; primitive argument addresses remain unsupported. |
| `starg` | Optional narrowing; `local.set` | Replaces scalar arguments; copies structs into alias-preserving parameter storage. |
| `ldloc` | `local.get`; optional `f64.promote_f32`, `memory.copy`, or `struct.new` | Loads class references; struct loads copy values into independent storage. |
| `ldloca` | `local.get` | Returns a linear address or GC owner for struct locals. |
| `stloc` | Optional `i32.extend8_s` / `i32.extend16_s` / `i32.const; i32.and` / `f32.demote_f64`; `local.set`, `memory.copy`, or `struct.set` | Copies structs; otherwise narrows scalars and marks the slot initialized. |
| `localloc` |  |  |
| `endfilter` |  |  |
| `unaligned.` |  |  |
| `volatile.` |  |  |
| `tail.` |  |  |
| `initobj` | `memory.fill` or `struct.set` | Clears struct storage, including null references in GC fields. |
| `constrained.` |  |  |
| `cpblk` |  |  |
| `initblk` |  |  |
| `no.` |  |  |
| `rethrow` | `throw_ref` | Preserves the original exception captured by the enclosing catch. |
| `sizeof` |  |  |
| `refanytype` |  |  |
| `readonly.` |  |  |

## Tuples and TestApp progress

Closed generic structs specialize field types, layouts, and method signatures. Cache
keys include the complete declaring identity and arguments, so `Pair<int>` and
`Pair<long>` have distinct sizes and offsets. Numeric specializations use linear
memory; reference-containing specializations use GC values or reference slots when
selected for stack addresses.

[Framework tuples](../Tests/Wasm/Tuples/Snapshots/Values.md) execute actual constructors
from the supplied `System.Private.CoreLib` PE. Tests cover tuple arities 2–7, named
fields, nested tuples, deconstruction, argument/result copies, and reference tuples
passed by ref. This does not promise arbitrary tuple BCL methods, boxing, generic
factory methods, or eight-plus-element constructor validation. Framework ValueTuple
uses compiler-owned declaration-order layout, including its auto-layout declarations;
this is not a CLR interop layout guarantee.

[Xui geometry](../Tests/Wasm/Tuples/Snapshots/XuiGeometry.md) now executes
`Point point = (0, 0)`, Vector/Size/Rect/Frame conversions, and `Point + Vector` against
the actual Core and framework implementation PEs. The [Values scene](../Tests/Wasm/Canvas/Snapshots/Values.html)
uses tuple syntax in an offline browser artifact.

The [real TestApp construction gate](../Tests/Wasm/TestApp/Snapshots/FillRectConstruction.md)
now compiles and executes unchanged `FillRectTest`/Core construction in Node, including
checking the initialized `WritingMode`. It supplies framework implementation PEs for
`IServiceProvider` and collection dependencies. This is a required passing test; layout,
rendering, input, and the full TestApp are not yet exercised by it.

## Default interface implementations

Class implementations take precedence; otherwise dispatch selects the most-specific
interface implementation, including explicit interface overrides, inherited defaults,
and closed generic contracts from linked assemblies. Default bodies can call ordinary
private/static helpers and dispatch back into class implementations. Struct arguments
and results use the existing value/byref ABI.

The [layout demo](../Tests/Wasm/Interfaces/Snapshots/Defaults.md) shows source, output,
selected dispatch targets, and WAT. Reachability retains a callable signature without
retaining an overridden or uncalled default body. Conflicting defaults and reabstraction
without a concrete implementation fail during compilation; runtime ambiguity exceptions
are not implemented. Generic virtual methods, static-abstract dispatch, and boxed or
constrained struct receivers remain unsupported.

## Managed arrays and inline gradient buffers

Vector signatures (`T[]`) preserve their exact element types through fields, locals,
arguments, results, generic substitution, and jagged arrays. [The array demo](../Tests/Wasm/Arrays/Snapshots/Vectors.md)
executes primitive/reference arrays and independent struct element copies. Storage is
a WASM GC array: small integers currently use i32 elements with narrowing, strings
use externref, and classes/jagged arrays use eqref. Struct elements currently each
use a separate GC struct, allocated eagerly and updated in place; these arrays do
not promise contiguous linear-memory element layout. Null access, bounds failures,
and negative allocation lengths throw managed exceptions. Engine resource exhaustion
remains outside the managed exception surface.

Inline arrays repeat the element's storage with size `length * elementSize`. Numeric
buffers use the linear frame; string/object buffers repeat frame-owned reference slots.
Copies retain independent values/slots, and class fields flatten every repeated leaf.
Roslyn's inline-array helpers and the length/indexer operations of Span/ReadOnlySpan
over this linear backing are recognized intrinsically. A span borrows an address
and length; it does not own or copy its backing elements. This compiler ABI does
not change XuiNet's classification of CLR spans as containing managed references.

[The gradient demo](../Tests/Wasm/InlineArrays/Snapshots/Gradients.md) compiles the real
Xui `ReadOnlySpan<GradientStop> stops = [(0, 0xff0000ffu), (1, 0x0000ffffu)]`,
constructs `LinearGradient`, and reads its stops in a separate method. The supplied
Core/framework PEs provide Point/Color/GradientStop and tuple constructors; the
unsigned NFloat conversion used by Color is a scalar intrinsic.

Still unsupported: multidimensional arrays, array covariance/type tests/object
conversions, primitive/reference array-element byrefs, general escaping GC-element
byrefs, bulk static-data initializers (`RuntimeHelpers.InitializeArray/CreateSpan`),
and arbitrary Array/BCL collection methods. Span construction from managed arrays,
slicing, enumeration helpers, stackalloc, and inline-array access through GC-field
addresses are not implemented. Unsupported paths throw during compilation.

## Generic interface variance

Closed `out` and `in` interface arguments support reference conversions through class
inheritance, inherited interfaces, and nested variant contracts. Calls, `is`, `as`,
and explicit casts use the same nominal relation. Value-type arguments remain
invariant; there is no implicit boxing. The [variance demo](../Tests/Wasm/Interfaces/Snapshots/Variance.md)
shows covariant sources, contravariant sinks, a specialized default body, and casts.

Reachability indexes constructed interface calls and allocations by generic definition,
then retains only compatible implementations. At each class level exact mappings
precede compatible variant mappings in declared interface order, before consulting the
base class. Class virtual overrides remain effective. Default bodies execute in their
original constructed interface, with interface inheritance determining specificity.

Class/interface/object references share the existing eqref ABI. String/object and
array/object calls, and covariant array parameter/result conversions, need adapters or
additional runtime support and fail explicitly before a function-table entry is emitted.
Unused variant contracts and nominal membership tests do not require those adapters.
Synthesized array interface conversions remain explicitly unsupported; string interface
relations require the framework implementation PE to be supplied.
Multiple compatible variant default candidates without an exact contract are explicitly
unsupported. Delegate variance, generic methods, and boxed struct dispatch remain outside
this implementation.

## Enum values

All eight C# enum underlying types execute through locals, arguments/results, class
and struct fields, vectors, closed generic types, and supported inline-buffer accesses.
`EnumInfo` reads `value__` from the PE; declared signatures, overload resolution,
generic specialization, and array identities keep the enum's nominal identity.
Only stack analysis and backend storage selection reduce its values to integers.

| Underlying type | Linear storage | MSIL stack / WASM numeric slot |
| --- | --- | --- |
| `sbyte`, `byte` | 1 byte | `Int32` / `i32` |
| `short`, `ushort` | 2 bytes | `Int32` / `i32` |
| `int`, `uint` | 4 bytes | `Int32` / `i32` |
| `long`, `ulong` | 8 bytes | `Int64` / `i64` |

Stores truncate to the declared width; loads extend with the correct signedness.
GC fields and vector elements currently use widened i32 slots for small integers,
matching primitive storage in this backend. Linear struct/inline-buffer fields retain
their exact byte sizes, offsets, and packing. Enums do not allocate struct wrappers.
Flags bit operations, unsigned comparisons, explicit casts, and unnamed values work.
See [enum widths](../Tests/Wasm/Enums/Snapshots/Widths.md).

Enum byref parameters/locals and address-taking of scalar enum locals remain unsupported,
as do boxing, enum-name formatting, reflection, and arbitrary System.Enum helpers.
This milestone does not add general primitive-address or generic initobj support.


The [persistent mobile preview](../Tests/Wasm/TestApp/Snapshots/MobilePhone.html)
uses named exports to retain a session across frame/input calls and the real
emulator middleware's phone geometry/chrome. The full mobile-shell readiness probe
currently stops at `ScrollView.Content → SetProtectedChild<ScrollContent>`.
Unused generic virtual declarations no longer prevent ordinary vtable construction;
reachable generic method calls still reject explicitly. See the
[readiness snapshot](../Tests/Wasm/TestApp/Snapshots/MobileShellReadiness.md).
