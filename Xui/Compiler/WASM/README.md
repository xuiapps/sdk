# WASM backend support

The backend emits a `void` or `int` entry point and reachable, non-generic static and instance
methods in the same module. Method bodies support straight-line code, nested conditionals, and single-entry loops.
It supports integer and floating-point constants, arithmetic, bitwise operations,
shifts, numeric comparisons, selected conversions, primitive local and argument loads/stores, stack duplication/discard, and string literals.
MSIL `Int32` and `Int64` use WASM `i32` and `i64`; floating-point evaluation values
use `f64`. Strings use `externref`, with passive UTF-16LE data materialized through
`wasm:js-string.fromCharCodeArray` and cached in globals.

Classes declared in the same module use WASM GC objects with reference identity.
Structs classified as `ContainsManagedReferences` use GC objects with value-copy
semantics. Their string fields remain native JavaScript `externref` values.
Nested struct fields are recursively flattened into their owner's GC fields,
including numeric structs: an embedded struct adds fields, not a child allocation.
Class-reference fields stay references, including cycles. Struct loads, duplication,
arguments and returns preserve independent values while copying embedded references
shallowly. Stores update existing struct storage so outstanding field addresses stay
valid. Numeric struct values transfer between linear memory and flattened GC fields.

Same-module class and GC-struct constructors and direct instance calls are supported.
Class inheritance preserves a GC field prefix, with a runtime type descriptor at
field zero. Virtual and abstract class methods dispatch through a function table
using `call_indirect`; overrides replace inherited slots, while `new virtual`
methods append slots. Nonvirtual calls and explicit `base.Method()` calls remain
direct. `callvirt` checks its receiver for null. The parameterless core `System.Object`
constructor has no emitted body. Class equality compares identity; null assignment,
null tests, and default zero/null fields are supported. Null field access and null
instance calls trap; translating these traps into catchable .NET exceptions remains
unsupported. `is` and `as` lower to `isinst`, which follows nominal descriptor
parent links; identical field layouts do not make unrelated classes interchangeable.
`castclass` uses the same check and traps on a non-null mismatch. See the
[GC objects demo](../Tests/Wasm/Gc/Snapshots/Objects.md) and
[polymorphism demo](../Tests/Wasm/Polymorphism/Snapshots/Units.md).

[Runtime.Type](Runtime/Type.cs), exposed by `WasmEmitter.Types`, records source
identity, base type, flattened field ranges, virtual slots, and WASM indices.
Immutable runtime GC descriptors contain identity, base descriptor, table start,
UTF-16 name array, slot count, and field count. This is groundwork for a managed
`Type` facade; `System.Type`, `GetType`, `typeof`, and reflection calls are not emitted yet.

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
three-level hierarchy. Primitive/reference `default(T)` via `initobj` is not
implemented. Expanding instantiations stop at a nesting depth of 32.

Interfaces, generic methods/structs, arbitrary external calls, switches, arbitrary
jump graphs, lambdas, closures, delegates, managed arrays, unmanaged pointers,
boxing, and exception handling are not implemented. Class signatures and type tests
currently require same-module declarations; general `object` storage and external
class hierarchies remain unsupported. Explicit MethodImpl mappings and covariant
return overrides are rejected. Explicit-layout structs cannot
be flattened into GC fields yet; their supported linear-memory layouts are unchanged.
Managed-address locals and explicit byref parameters remain unsupported. Addresses
of inline struct fields can be used within expressions and conditional arms, but
cannot be passed as instance receivers or carried across loops. Class-reference
joins widen to a common same-module base class, including null/class joins.
Other stack types still require exact matches; joins requiring System.Object or
interface resolution remain unsupported.
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
overlapping regions throw `Code flow jumps unsupported`. Dead instructions in these methods are omitted
without modifying the PE or its IL offsets.

Locals support `bool`, `char`, `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`,
`long`, `ulong`, `float`, `double`, `nint`, `nuint`, and `string`. Native integers
use a 32-bit WASM target representation. Stores narrow small integers and round
`float` to f32; loads restore the MSIL stack representation. `InitLocals` supplies
zero/null defaults; without it, each slot must be stored before it is read.
Unmanaged structs with a supported XuiNet layout use linear-memory local storage;
classes and reference-containing structs use GC locals. Unresolved external types
(such as `decimal`), `object`/boxed values, pointers, byref locals, and pinned locals
remain unsupported. Primitive local addresses are not implemented.

Struct locals classified as `LinearMemory` occupy aligned slots in a per-invocation
frame in an internal, fixed 1 MiB memory. The mutable stack pointer starts at 16;
entry checks available capacity and reserves the frame, and every normal return
restores it. Frame overflow traps. A trap or a throwing host import does not unwind
the pointer; discard the instance after such a failure. `InitLocals` clears local
storage on each invocation. Addressable struct locals currently require this flag.
`ldloca`, field addresses, primitive/nested field loads and stores, `initobj`,
`ldobj`, `stobj`, and `cpobj` use the calculated byte layout. Loads and `dup` copy
struct values into distinct frame temporaries; assignments copy bytes rather than
aliasing the source. Temporary slots are reused across iterations, not allocated
per iteration. GC struct/class block parameters and results use references; linear struct block parameters/results remain unsupported.

Linear struct parameters/returns, linear struct instance constructors, and NFloat
arithmetic/conversion calls remain unsupported. Numeric locals continue to use
linear storage; GC-backed structs do not use this frame allocator.
See the [linear-memory copy demo](../Tests/Wasm/Structs/Snapshots/LinearMemory.md).

Static method parameters and results support the same primitive/string types as
locals, plus `void` results. Calls check argument types, preserve order, and narrow
small integers and `float` at storage boundaries. Function indices are assigned
before bodies are emitted, supporting forward and recursive references; unused
method bodies remain unread. Calls currently require a `MethodDefinition` token.
Type/module initializers, synchronized methods, and entry-point `string[] args`
access are rejected. `dup` uses reusable typed temporary locals.

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
| `ldarg.0` | `local.get`; optional `f64.promote_f32` | Loads primitive/string arguments; float storage promotes to the f64 stack. |
| `ldarg.1` | `local.get`; optional `f64.promote_f32` | Loads primitive/string arguments; float storage promotes to the f64 stack. |
| `ldarg.2` | `local.get`; optional `f64.promote_f32` | Loads primitive/string arguments; float storage promotes to the f64 stack. |
| `ldarg.3` | `local.get`; optional `f64.promote_f32` | Loads primitive/string arguments; float storage promotes to the f64 stack. |
| `ldloc.0` | `local.get`; optional `f64.promote_f32`, `memory.copy`, or `struct.new` | Loads class references; struct loads copy values into independent storage. |
| `ldloc.1` | `local.get`; optional `f64.promote_f32`, `memory.copy`, or `struct.new` | Loads class references; struct loads copy values into independent storage. |
| `ldloc.2` | `local.get`; optional `f64.promote_f32`, `memory.copy`, or `struct.new` | Loads class references; struct loads copy values into independent storage. |
| `ldloc.3` | `local.get`; optional `f64.promote_f32`, `memory.copy`, or `struct.new` | Loads class references; struct loads copy values into independent storage. |
| `stloc.0` | Optional `i32.extend8_s` / `i32.extend16_s` / `i32.const; i32.and` / `f32.demote_f64`; `local.set`, `memory.copy`, or `struct.set` | Copies structs; otherwise narrows scalars and marks the slot initialized. |
| `stloc.1` | Optional `i32.extend8_s` / `i32.extend16_s` / `i32.const; i32.and` / `f32.demote_f64`; `local.set`, `memory.copy`, or `struct.set` | Copies structs; otherwise narrows scalars and marks the slot initialized. |
| `stloc.2` | Optional `i32.extend8_s` / `i32.extend16_s` / `i32.const; i32.and` / `f32.demote_f64`; `local.set`, `memory.copy`, or `struct.set` | Copies structs; otherwise narrows scalars and marks the slot initialized. |
| `stloc.3` | Optional `i32.extend8_s` / `i32.extend16_s` / `i32.const; i32.and` / `f32.demote_f64`; `local.set`, `memory.copy`, or `struct.set` | Copies structs; otherwise narrows scalars and marks the slot initialized. |
| `ldarg.s` | `local.get`; optional `f64.promote_f32` | Loads primitive/string arguments; float storage promotes to the f64 stack. |
| `ldarga.s` | `local.get` | Addresses a by-value GC struct parameter; other arguments remain unsupported. |
| `starg.s` | Optional narrowing; `local.set` | Narrows small integers and floats before replacing the argument slot. |
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
| `call` | Optional argument narrowing/spills; `call`; optional `f64.promote_f32` | Calls same-module static/direct instance methods; preserves argument value semantics. |
| `calli` |  |  |
| `ret` | Optional result narrowing; `return` | Returns scalars, class references or copied GC struct values. |
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
| `ldind.i1` |  |  |
| `ldind.u1` |  |  |
| `ldind.i2` |  |  |
| `ldind.u2` |  |  |
| `ldind.i4` |  |  |
| `ldind.u4` |  |  |
| `ldind.i8` |  |  |
| `ldind.i` |  |  |
| `ldind.r4` |  |  |
| `ldind.r8` |  |  |
| `ldind.ref` |  |  |
| `stind.ref` |  |  |
| `stind.i1` |  |  |
| `stind.i2` |  |  |
| `stind.i4` |  |  |
| `stind.i8` |  |  |
| `stind.r4` |  |  |
| `stind.r8` |  |  |
| `add` | `i32.add` / `i64.add` / `f64.add` | Type stack selects integer width or f64; integer overflow wraps. |
| `sub` | `i32.sub` / `i64.sub` / `f64.sub` | Type stack selects integer width or f64; integer overflow wraps. |
| `mul` | `i32.mul` / `i64.mul` / `f64.mul` | Type stack selects integer width or f64; integer overflow wraps. |
| `div` |  |  |
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
| `callvirt` | `ref.cast`; `struct.get`; `call_indirect` or `call` | Null-checks receiver; virtual slots select the runtime class implementation. |
| `cpobj` | `memory.copy` or leaf loads/stores | Copies matching structs, including transfers between GC and linear storage. |
| `ldobj` | `memory.copy` or `struct.get`; `struct.new` | Loads an independent struct value from its managed address. |
| `ldstr` | `global.get; ref.is_null; if; i32.const; i32.const; array.new_data; i32.const; i32.const; call; global.set; end; global.get` | Materializes passive UTF-16LE data through js-string; caches the JavaScript string. |
| `newobj` | `struct.new_default`; `call` | Allocates storage, initializes class type header, then invokes constructor. |
| `castclass` | Descriptor helper `call`; `unreachable` on mismatch | Preserves null; checks nominal ancestry; failed casts currently trap. |
| `isinst` | Descriptor helper `call`; `ref.eq`; parent traversal | Returns original reference or null; supports class is/as tests. |
| `conv.r.un` | `f64.convert_i32_u` / `f64.convert_i64_u` | Interprets integer bits as unsigned before conversion to floating point. |
| `unbox` |  |  |
| `throw` |  |  |
| `ldfld` | Numeric load / `memory.copy` / `struct.get` / `struct.new` | Reads linear or flattened GC fields; struct values are copied. |
| `ldflda` | `i32.add` or GC owner reference | Retains the owner and static field range for inline addresses. |
| `stfld` | Numeric store / `memory.copy` / `struct.set` | Writes linear or flattened fields; struct assignment copies leaf values. |
| `ldsfld` |  |  |
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
| `box` |  |  |
| `newarr` |  |  |
| `ldlen` |  |  |
| `ldelema` |  |  |
| `ldelem.i1` |  |  |
| `ldelem.u1` |  |  |
| `ldelem.i2` |  |  |
| `ldelem.u2` |  |  |
| `ldelem.i4` |  |  |
| `ldelem.u4` |  |  |
| `ldelem.i8` |  |  |
| `ldelem.i` |  |  |
| `ldelem.r4` |  |  |
| `ldelem.r8` |  |  |
| `ldelem.ref` |  |  |
| `stelem.i` |  |  |
| `stelem.i1` |  |  |
| `stelem.i2` |  |  |
| `stelem.i4` |  |  |
| `stelem.i8` |  |  |
| `stelem.r4` |  |  |
| `stelem.r8` |  |  |
| `stelem.ref` |  |  |
| `ldelem` |  |  |
| `stelem` |  |  |
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
| `endfinally` |  |  |
| `leave` |  |  |
| `leave.s` |  |  |
| `stind.i` |  |  |
| `conv.u` | No instruction (i32/native) / `i32.wrap_i64` (i64) | Native integers use 32 bits; floating-point inputs remain unsupported. |
| `arglist` |  |  |
| `ceq` | Numeric equality / `ref.eq` / `ref.is_null` | Compares numbers, class identity, or null; produces int32 Boolean. |
| `cgt` | `i32.gt_s` / `i64.gt_s` / `f64.gt` | Numeric operands only; pushes int32 0/1; NaN comparisons yield false. |
| `cgt.un` | `i32.gt_u` / `i64.gt_u` / `f64.le; i32.eqz` | Unsigned integers; inverted ordered floating comparison makes NaN yield true. |
| `clt` | `i32.lt_s` / `i64.lt_s` / `f64.lt` | Numeric operands only; pushes int32 0/1; NaN comparisons yield false. |
| `clt.un` | `i32.lt_u` / `i64.lt_u` / `f64.ge; i32.eqz` | Unsigned integers; inverted ordered floating comparison makes NaN yield true. |
| `ldftn` |  |  |
| `ldvirtftn` |  |  |
| `ldarg` | `local.get`; optional `f64.promote_f32` | Loads primitive/string arguments; float storage promotes to the f64 stack. |
| `ldarga` | `local.get` | Addresses a by-value GC struct parameter; other arguments remain unsupported. |
| `starg` | Optional narrowing; `local.set` | Narrows small integers and floats before replacing the argument slot. |
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
| `rethrow` |  |  |
| `sizeof` |  |  |
| `refanytype` |  |  |
| `readonly.` |  |  |
