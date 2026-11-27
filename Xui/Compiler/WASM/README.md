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

Calls through `object.ToString()` preserve class overrides, inherited overrides,
hidden-slot distinctions and explicit `base.ToString()` behavior. Default names
include namespace, nested declarations, generic arguments and array element/rank
identity. Native strings and supported primitive/enum boxes format through the
JS-string runtime; numeric formatting uses the invariant browser profile.
Reachability retains overrides on instantiated classes. Compiler-owned exception
and Type carriers do not yet implement their ToString surface and throw managed
NotSupportedException through an object call. Arbitrary struct boxing remains unsupported.

Default equality for classes implementing `IEquatable<T>` uses their exact closed
interface contract, including explicit implementations and virtual overrides.
Both null operands compare equal; a single null operand compares unequal without
calling user equality. Array search and dictionary keys use the same path.
`System.Type` descriptors have identity equality and stable per-instance hashes,
supporting generated record equality contracts. Sealed record keys are tested;
record inheritance still reaches the unsupported covariant clone override.

`Buffer.BlockCopy` supports statically known primitive vectors with byte offsets,
unaligned ranges and memmove overlap semantics. Same-type byte-sized arrays use
`array.copy`; wider/cross-type copies read and modify little-endian bytes within
GC elements without pinning or temporary arrays. Floating-point payload bits are
preserved. Native-sized elements follow the WASM32 width. Nonprimitive vectors
throw ArgumentException; rectangular and type-erased Array inputs remain unsupported.

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
UTF-16 name array, slot count, and field count. A narrow `System.Type` facade reuses
these descriptors directly: `GetType()` on a named class/interface returns its
actual runtime descriptor, and `.Name` returns the metadata simple name (including
generic arity, excluding namespace/nesting prefixes). Type equality compares
immutable descriptor identity; locals, arguments, fields, and casts preserve it.
GetType/Name on null throw managed NullReferenceException. Descriptor simple-name
storage is added only when the facade is used. Object-typed references to descriptor-backed
classes and supported boxes also expose GetType; arrays, exceptions, and general reflection remain unsupported.
`typeof` supports linked classes/interfaces and supported boxed value types, including closed generic specializations.
`ldtoken` pushes the existing descriptor as a transient opaque runtime type handle;
`Type.GetTypeFromHandle` exposes that same reference as `Type`, without allocation.
This permits exact type comparisons in service lookup and agrees with instance
GetType identity. Outside folded closed IsValueType queries, array/open-generic type tokens, raw
RuntimeTypeHandle locals/fields/call ABIs, and method/field handles remain unsupported.

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
event add/remove accessors execute using reference CompareExchange. The compiler
recognizes the framework generic/object overloads and CompareExchangeObject by
identity and signature. It reads through the managed owner/selector address,
compares with `ref.eq`, writes only on a match, and returns the original reference.
It never uses delegate equality or calls user Equals methods. Existing accessors
cover object fields and frame-owned slots, including reference fields in stack
structs. The read/compare/write sequence has no suspension and allocates no
objects. This implementation supports single-agent execution only; it provides no
cross-thread atomicity or memory-ordering guarantees. Numeric Interlocked overloads
remain unsupported. Reference `Exchange` and `ExchangeObject` also read and replace
the slot, returning its previous value. Exchange supports native JS strings because
it needs no string-identity comparison. String reference CompareExchange is explicitly rejected because
native JavaScript strings do not retain managed string object identity.
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
three-level hierarchy. Reference and primitive `default(T)` via `initobj` are supported; scalar `ldobj`/`stobj`/`cpobj` use typed managed-address accessors. Expanding instantiations stop at a nesting depth of 32.

Open generic structs, arbitrary external calls, arbitrary
jump graphs, multidimensional arrays, unmanaged pointers,
arbitrary struct boxing, and arbitrary BCL objects are not implemented. Class signatures and type tests require definitions in supplied implementation assemblies.
`object` storage holds emitted class/interface references, supported scalar boxes, and null.
Native JS strings and arbitrary BCL objects do not yet have a general object representation. Unresolved external class hierarchies,
boxed struct dispatch,
and explicit class MethodImpl/covariant return overrides remain unsupported. Explicit-layout structs cannot
be flattened into GC fields yet; their supported linear-memory layouts are unchanged.
Closed generic **methods** now specialize through MethodSpec tokens, including calls
across supplied assemblies, nested type/method arguments, recursion, and overloads.
Each cache key includes both the declaring type and method argument list. Open
method templates are expanded only by closed calls. Closed generic class and interface
virtual calls retain specialized dispatch entries and resolve overrides in template space; `box T` is a no-op only when T specializes to a supported reference type.

Managed `ref`/`in`/`out` arguments and local aliases carry an `eqref` owner plus an
`i32` accessor selector. Generated getters/setters use constant WASM field indices.
No descriptor object is allocated per byref. Class fields (including reference leaves
inside flattened structs) keep their owner alive; saved refs keep pointing to the
original object after the source variable changes. Address-taken reference locals and
arguments use caller-owned GC-visible table slots, cleared on return and unwinding.
Strings use the same convention with externref getters/setters. Calls, constructors,
virtual/interface dispatch, ref locals, and conditional/loop stacks preserve the pair.
Managed reference returns preserve this pair, including virtual/interface and multicast
delegate calls. Scalar and struct locations use the same pair: a null owner identifies
linear storage, while a GC owner identifies a flattened field range. Generated typed
accessors preserve storage widths and reference visibility. Null managed references
use `(null, -1)`, including initialized byref locals; Unsafe.NullRef/IsNullRef use that
representation and dereferencing it throws NullReferenceException.

Struct `ref`/`in`/`out` parameters and local aliases work for linear stack locations,
inline GC fields, and struct-array elements. Nested field locations, constructors,
returns, conditional joins, and loops retain their owner and selector; taking a ref
allocates no wrapper. Pure numeric struct values keep their linear representation. Class-reference
joins widen to a common linked base class, including null/class joins.
Class references can also join an existing assignable interface or `object` stack type.
Other stack types still require exact matches; synthesizing a common interface for unrelated
class stack types remains unsupported.
Checked integer addition/subtraction and unsigned multiplication throw managed
`OverflowException` for signed/unsigned i32/i64 bounds; native integers use WASM32
width. General native-integer arithmetic, signed checked multiplication and checked
numeric conversions remain incomplete; the opcode table records individual support.

All emitted methods use [XuiNet.CodeFlow](../XuiNet/CodeFlow) to check the stack on
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
(such as `decimal`), general raw-pointer dereferences, and pinned locals remain unsupported. Supported boxes
use object-reference locals; primitive local addresses use managed owner/selector pairs.

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
General spans, byref fields, linear struct values carried across
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
linear struct. Other static fields and addresses use the persistent static-storage path below. `IsExternalInit` required modifiers on void init
setter returns preserve the ordinary setter ABI; other unknown required modifiers reject.

Single-value `TimeSpan.FromDays/Hours/Minutes/Seconds/Milliseconds/Microseconds`
factories lower to WASM arithmetic over signed 64-bit ticks (100ns units), stored
in independent linear struct frame slots. Floating inputs truncate fractional
ticks, preserve the rounded maximum boundary, and throw managed ArgumentException
for NaN or OverflowException for out-of-range values. Integral overloads use exact
multiplication with ArgumentOutOfRangeException on overflow. Framework implementation
metadata supplies the layout; factory bodies and their System.SR/ResourceManager
message dependencies are omitted. Guard messages are English, not localized.
[CLR/Node comparisons](../Tests/Wasm/Time/TimeSpanTests.cs) cover both build modes,
rounding boundaries, infinities, and exception messages. Composite overloads,
formatting, parsing, and clock reads are not supplied by these intrinsics. Duration
conversion has no JavaScript host dependency.

Primitive `GetHashCode` and typed `Equals` calls specialize without virtual dispatch,
including constrained generic receivers. Default comparers share the same operations
for bool, char, signed/unsigned integer widths, native integers, float/double,
NFloat, enums, and strings. Scalar hashes follow CoreLib rules: char repeats its
16 bits, 64-bit values fold both halves, and floating hashes normalize signed zero
and every NaN payload. Native integers/NFloat follow WASM32 storage. String hashing
uses a host UTF-16 FNV-1a implementation, consistent with ordinal equality; hash
numbers intentionally differ from CoreLib's randomized string hashes. Default
string comparers hash null as zero, while null instance/comparer receivers throw
managed NullReferenceException. Custom comparer implementations retain normal dispatch.

Default comparers for named classes preserve virtual `Equals` and `GetHashCode`
overrides. Classes without a hash override use stable host identity hashes held in
a WeakMap; this does not retain collected objects. Null keys hash as zero at the
comparer boundary (Dictionary still enforces its own null-key checks).
Ordinary virtual `Object.GetHashCode` calls also retain class overrides, including
inherited overrides while ignoring hidden new slots. Nonvirtual base calls use
stable identity hashing. Object-slot strings and scalar/enum boxes use their
typed hash operations; arrays use stable identity hashes. Null receivers throw.
Every emitted method now receives stack analysis, including straight-line
string-to-object arguments and returns that need a managed string carrier.
Value-type default comparers call declared `GetHashCode` overrides on independent
copies, including linear structs, structs containing references, records and
tuples. Constrained hash calls use the same managed overrides. CoreLib's
`HashCode.Combine` arithmetic runs as compiled WASM; browser crypto supplies its
per-module seed. Hash numbers are not portable across module instances.
Non-nullable struct boxing used only for a null check is folded without dropping
producer side effects; nullable values retain their normal checks. Escaping
arbitrary struct boxes remain unsupported, including tuple-key dictionary error
formatting paths. This is not yet full support for dictionaries with struct keys.
Closed type equality, proven constant boolean call arguments, and impossible
interface/class tests can prune unreachable collection branches. Type-test pruning
checks supplied subclasses, preserves receiver evaluation, and declines proof for
open generic descendants or unresolved base types. No dynamic assembly loading is assumed.

String-key dictionaries retain CoreLib's storage, resize, remove, and lookup code.
The internal non-randomized string-comparer wrapper is replaced by the supplied
native comparer; custom `IEqualityComparer<string>` instances remain unchanged.
CoreLib's wrapper-specific rehash branch is unused with this representation.
Native string hashes remain the runtime's UTF-16 hash described above.

Strings stored in `object` locals, fields, arrays, or generic object slots use small
GC carriers. Casts recover the native JS string; null remains null and invalid
casts throw managed exceptions. A per-module weak cache reuses carriers while
they are alive, without retaining their string contents indefinitely. Because JS
primitive strings have value identity, equal text shares a carrier: this does not
preserve .NET reference-identity differences between separately allocated equal
strings. Arbitrary virtual/interface dispatch on these carriers is not implemented.

`Math.Sqrt`, `MathF.Sqrt`, and the corresponding Single/Double/NFloat static methods
emit `f32.sqrt` or `f64.sqrt` directly, without a host import. Native floating-point
uses WASM32 single precision; signed zero, infinities, and NaNs preserve IEEE behavior.
[Square-root tests](../Tests/Wasm/Mathematics/SquareRootTests.cs) compare CLR results,
including subnormals and float rounding boundaries.

The corresponding `Sin`, `Cos`, `Log`, `Exp`, `Cbrt`, and `Atan2` methods import
`xui-math.sin/cos/log/exp/cbrt/atan2`, supplied by
JavaScript Math. Single/NFloat inputs and outputs round to f32; special-value,
signed-zero, and finite-result comparisons are covered by
[trigonometry tests](../Tests/Wasm/Mathematics/TrigonometryTests.cs).
Finite transcendental results need not be bit-identical across host platforms.
`Abs`, `CopySign`, `Min`, `Max`, `Floor`, `Ceiling`, `Truncate`, and unary `Round`
emit scalar WASM instructions. Round defaults to ties-to-even; the MidpointRounding
overload implements all five modes and rejects invalid modes with ArgumentException.
Away-from-zero uses fractional magnitude to avoid rounding midpoint predecessors
incorrectly. Decimal/digits overloads are not intrinsic. NFloat limits and epsilon
use the target's f32 constants, not the compilation host's f64 constants.
Floating `rem` and NFloat modulus use the host's truncated remainder (`%`), including
NaN, infinities and signed zero. NFloat casts to Int32/Int64 use the existing signed
saturating conversion policy, after target-width rounding.

`Environment.TickCount64` and `Stopwatch.GetTimestamp` import the host's monotonic
`performance.now()` clock. Environment uses whole milliseconds; Stopwatch uses
nanosecond units with `Frequency = 1_000_000_000`. Precision still depends on the
browser, and the origin is the host time origin, not machine boot or wall time.
Only used clock imports are retained. The supplied CoreLib Stopwatch/TimeProvider
elapsed-time methods execute with matching frequency constants; custom TimeProvider
overrides retain ordinary virtual dispatch. See [clock tests](../Tests/Wasm/Time/ClockTests.cs).

Floating `conv.i8` uses WASM's non-trapping saturating conversion: finite in-range
values truncate toward zero, out-of-range values clamp, and NaN becomes zero.
This is the target's defined behavior for unchecked non-finite/overflow inputs;
other CLR targets can differ. Other floating-to-integer conversions remain unsupported.
Supported exception constructors/properties now also accept MethodDef references
inside the supplied CoreLib, preserving its validation branches and resource messages.

`String.Concat` with two, three, or four string arguments calls `wasm:js-string.concat`.
The emitter preserves evaluation order and normalizes null arguments to empty strings;
UTF-16 contents remain native JS strings. Object, array, and enumerable Concat
overloads are not intrinsic. This also covers Roslyn's short string interpolation
optimization, such as `$"Example.{GetType().Name}"`.

`String.Length`, character indexing, and both `Substring` overloads use
`wasm:js-string` builtins after managed null/range checks. Indices and lengths count
UTF-16 code units, including individual surrogate halves.

`String.Split` supports char, string, char[] and string[] separators, optional count,
and `RemoveEmptyEntries`/`TrimEntries`. The JS runtime uses .NET's UTF-16 whitespace
set and preserves the unsplit remainder at the count limit. Results are copied into
ordinary mutable WASM GC string arrays; null receivers and invalid arguments throw
managed exceptions. Span separator overloads remain unsupported. [Parity tests](../Tests/Wasm/Strings/SplitTests.cs)
compare Debug and Release code with CoreLib.

String.IndexOf(char[, startIndex[, count]]) and Contains(char) search exact UTF-16
code units, including individual surrogate halves. Bounds and null failures remain
managed exceptions. Search uses the native string's length and charCodeAt builtins;
no character array or culture search implementation is retained.

`StartsWith`/`EndsWith` with a char, `Contains(string)`, and string searches with
explicit `StringComparison.Ordinal` use native JavaScript UTF-16 searches.
Empty patterns and surrogate halves follow CLR ordinal behavior; null inputs and
invalid comparison values throw managed argument exceptions. Other comparison
modes throw `NotSupportedException` instead of silently becoming ordinal. Default
culture-sensitive prefix/suffix overloads are not substituted. The floorplan SVG
parser explicitly selects ordinal matching for its syntax tokens.

`Int32.ToString()` reads the current managed integer address and imports
`xui-string.int32-to-string`, producing invariant decimal native JS strings. An
adjacent `CultureInfo.InvariantCulture` getter consumed only by
`Int32.ToString(IFormatProvider)` uses the same path. The compiler proves that the
provider cannot escape or be entered through another control-flow edge; arbitrary
providers and format strings are unsupported.

`Double/Single/NFloat.ToString(format, CultureInfo.InvariantCulture)` uses the same proven
provider pairing and produces native JS strings. It supports null/empty/general
formats and custom `0`, `0.0`, `0.##`, … up to 99 fractional placeholders. Custom
formats use the CLR's 15/7-significant-digit decimal buffer and midpoint rounding
away from zero, including negative zero and extreme exponents; JavaScript's
binary `toFixed` rounding is not substituted. Other formats throw a managed
NotSupportedException. Arbitrary culture providers remain unsupported.
The same fixed custom formats work in composite String.Format. One to three
decimal sections with optional literal signs support positive/negative/zero formats
such as `+0.00;-0.00;0.00`, including rounding-to-zero section selection. Other
custom syntax (grouping, percent scaling, quoted literals) remains unsupported.
NFloat uses this target’s 32-bit native floating-point storage.

Volatile field signatures retain their ordinary storage shape. The `volatile.`
prefix emits ordinary accesses on this single-agent target, whose GC objects and
linear memory are not shared. Instance/static fields, generic reference fields,
null failures and call ordering are tested against .NET. This is not shared-memory
thread support; a shared target will require acquire/release access lowering.

`DefaultInterpolatedStringHandler` has a compiler-owned eight-byte stack header:
a frame-owned GC reference slot for a mutable packed UTF-16 array, plus a character
count. Capacity grows geometrically; appends write code units directly. Supported
members are the two-integer constructor, AppendLiteral, single-value AppendFormatted
for strings/chars/bools/all fixed-width integers, ToString, and ToStringAndClear.
Integer formatting uses invariant decimal digits with exact signed/unsigned i64
arithmetic, including MinValue/MaxValue. ToString materializes an immutable native
JS string; ToStringAndClear also releases the handler's buffer reference. Copies
share buffer storage but retain independent counts and reference-slot ownership.
Normal and exceptional frame exit clears references. Allocation limits still use
WASM allocation traps; managed OutOfMemoryException is not implemented.

The handler's constrained Object.ToString call is specialized explicitly; this does
not implement general constrained dispatch. Single, Double and NFloat append through
native JS formatting, including invariant G/R and custom decimal zero/optional-#
placeholders. Enum values retain their closed type's names and flags metadata.
Providers, alignment, decimal and arbitrary object formatting remain unsupported
and fail explicitly. The BCL handler, ArrayPool, and pointer-based Span<char> code
are not retained. [CLR/Node comparisons](../Tests/Wasm/Strings/InterpolationTests.cs)
cover both optimization modes; [the REPL snapshot](../Tests/Wasm/Strings/Snapshots/RatingText.md)
contains source, output, exit code, and WAT.

Numeric layouts follow linked TypeRefs into their defining modules; caches and
field maps retain module ownership. The [standalone canvas demos](../Tests/Wasm/Canvas/README.md)
use real Rect and Color fields, execute Rect.Right from Xui.Core.dll, and expose scalar
drawing imports through a test host rewriter. Generated HTML includes all JS/WASM.

Static method parameters and results support the same primitive/string types as
locals, plus `void` results. Calls check argument types, preserve order, and narrow
small integers and `float` at storage boundaries. Function indices are assigned
before bodies are emitted, supporting forward and recursive references; unused
method bodies remain unread. Calls resolve MethodDef and MemberRef tokens across supplied assemblies.
Synchronized methods and entry-point `string[] args` access are rejected. `BeforeFieldInit` types defer initialization until a static field is
accessed, so unused static constants do not block ordinary geometry operations. `dup` uses reusable typed temporary locals.

Exception regions lower to native WASM `try_table`, a module-local exception tag,
`throw`, and `throw_ref`. Ordered typed catches, nested handlers, cross-method
propagation, bare `catch`, `throw;`, `finally`, and MSIL `fault` are supported.
`leave` clears the evaluation stack and routes through cleanup; `finally` runs on
normal and exceptional exits, while `fault` runs only during unwinding. Cleanup
handlers forward foreign host exceptions too; managed catches match our tag only.
Rethrow preserves the original exception reference. See the
[failed-cast demo](../Tests/Wasm/Exceptions/Snapshots/Casts.md).

BCL resource strings come from the supplied implementation PE. XuiNet exposes borrowed
manifest resource declarations and embedded-data readers, plus string-only `.resources`
lookup. For trusted `System.SR` getters whose IL is exactly a constant key followed by
`GetResourceString` and `ret`, compilation reads the neutral resource table and emits
only the requested text as an ordinary cached UTF-16/JS string literal. Messages are
not copied into a compiler-maintained list. This avoids retaining ResourceManager,
culture probing, and the complete resource table in WASM. Linked resources, dynamic
keys, satellite cultures, and runtime culture/resource-key switching remain unsupported.
The surrounding BCL exception helpers still compile as ordinary IL and require the
same opcode coverage as other methods, including their switches.

The managed payload is a GC struct containing a nominal exception kind and a
native JavaScript message string, with a parameter-name slot when needed.
Recognized framework references are `Exception`, `SystemException`,
`InvalidCastException`, `NullReferenceException`, `InvalidOperationException`,
`ArgumentException`, `ArgumentNullException`, `ArgumentOutOfRangeException`, `IndexOutOfRangeException`, and
`OverflowException`. `EncoderFallbackException` and `DecoderFallbackException`
also support default/message/message-with-inner constructors, their ArgumentException
hierarchy, and inner identity. Encoding-specific index/character/byte payloads and
constructors are not implemented. Parameterless/string constructors, `Message`, hierarchy tests,
and casts work without compiling BCL bodies. ArgumentNullException and
ArgumentOutOfRangeException take a parameter name in their single-string constructor;
their two-string overload takes (paramName, message). ArgumentOutOfRangeException also
accepts `(paramName, actualValue, message)` and preserves `ActualValue`. Its message
formats supported scalar/string values lazily when read; arbitrary object formatting
remains unsupported. ArgumentException's two-string overload reverses those roles. `ParamName`
preserves null and empty names, and `Message` appends a parameter suffix only for
nonempty names. Dynamic names use the standard
[JS string builtins](https://github.com/WebAssembly/js-string-builtins/blob/main/proposals/js-string-builtins/Overview.md)
for length and concatenation; these imports are emitted only for modules needing
parameter-name support. ArgumentNullException and ArgumentOutOfRangeException are catchable as ArgumentException,
SystemException, and Exception. CLR/Node tests cover Debug and Release, with and
without linked framework implementation PEs.
`throw null` throws `NullReferenceException`. Default messages are English;
cast failures use the default message rather than CLR-generated runtime type names.
Filters, custom exception subclasses and managed stack traces remain unsupported.
Protected regions with multiple normal continuations select a destination after cleanup. This is
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
| `ldarga.s` | `local.get` | Addresses frame-owned scalar/struct storage or a managed reference slot. |
| `starg.s` | Optional narrowing; `local.set` | Replaces scalar arguments; copies structs into alias-preserving parameter storage. |
| `ldloc.s` | `local.get`; optional `f64.promote_f32`, `memory.copy`, or `struct.new` | Loads class references; struct loads copy values into independent storage. |
| `ldloca.s` | `local.get` | Addresses scalar/struct locals or managed reference slots, preserving aliases. |
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
| `switch` | `block`, `br_table`, `br` | Typed case arms share a continuation; overlapping regions remain unsupported. |
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
| `div` | `i32.div_s`, `i64.div_s`, `f64.div` | Integer zero/overflow throw managed exceptions; floating division follows IEEE. |
| `div.un` | `i32.div_u`, `i64.div_u` | Zero divisor throws a managed DivideByZeroException. |
| `rem` | `i32.rem_s`, `i64.rem_s`, host `%` | Signed integer overflow throws; floating remainder uses JavaScript semantics. |
| `rem.un` | `i32.rem_u`, `i64.rem_u` | Zero divisor throws a managed DivideByZeroException. |
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
| `conv.i4` | No instruction / `i32.wrap_i64` / `i32.trunc_sat_f64_s` | Floating inputs truncate and saturate; NaN becomes zero. |
| `conv.i8` | `i64.extend_i32_s` / `i64.trunc_sat_f64_s` / identity | Floating values truncate; overflow saturates, NaN maps to zero. |
| `conv.r4` | `f32.convert_i32_s; f64.promote_f32` / `f32.convert_i64_s; f64.promote_f32` / `f32.demote_f64; f64.promote_f32` | Rounds directly to binary32, then restores the f64 stack representation. |
| `conv.r8` | `f64.convert_i32_s` / `f64.convert_i64_s` / no instruction (f64) | Signed integer conversion; floating-point stack values already use f64. |
| `conv.u4` | No instruction (i32) / `i32.wrap_i64` (i64) | Keeps low 32 bits; floating-point inputs are currently rejected. |
| `conv.u8` | `i64.extend_i32_u` (i32) / no instruction (i64) | Zero-extends int32 inputs; already-wide integers retain their original bits. |
| `callvirt` | `ref.cast`; `struct.get`; `call_indirect` or `call` | Null-checks receiver; class/interface slots or delegate entries select callable methods. |
| `cpobj` | `memory.copy` or leaf loads/stores | Copies matching structs, including transfers between GC and linear storage. |
| `ldobj` | `memory.copy` or `struct.get`; `struct.new` | Loads an independent struct value from its managed address. |
| `ldstr` | `global.get; ref.is_null; if; i32.const; i32.const; array.new_data; i32.const; i32.const; call; global.set; end; global.get` | Materializes passive UTF-16LE data through js-string; caches the JavaScript string. |
| `newobj` | Frame address / `struct.new_default`; `struct.new`; `call` | Constructs values/classes or binds a delegate target and callable. |
| `castclass` | Descriptor checks; managed `throw` on mismatch | Preserves null; checks ancestry, interfaces, and nominal boxed types. |
| `isinst` | Descriptor checks; `ref.eq` | Class/interface/boxed type test; returns original reference or null. |
| `conv.r.un` | `f64.convert_i32_u` / `f64.convert_i64_u` | Interprets integer bits as unsigned before conversion to floating point. |
| `unbox` | Checked descriptor test; owner and selector | Borrows mutable scalar payload; null and mismatches throw managed exceptions. |
| `throw` | `throw` | Throws managed payload; null becomes a NullReferenceException object. |
| `ldfld` | Numeric load / `memory.copy` / `struct.get` / `struct.new` | Reads linear or flattened GC fields; struct values are copied. |
| `ldflda` | `i32.add`; owner + accessor selector | Reference fields preserve owner identity through calls and local aliases. |
| `stfld` | Numeric store / `memory.copy` / `struct.set` | Writes linear or flattened fields; struct assignment copies leaf values. |
| `ldsfld` | Initialization guard, `global.get`, `struct.get` | Loads copy struct values; persistent storage belongs to module instance. |
| `ldsflda` | Initialization guard, holder/field selector | Stable managed static address; RVA span intrinsics remain specialized. |
| `stsfld` | Initialization guard, `struct.set` | Copies flattened values into persistent storage; existing aliases remain valid. |
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
| `box` | `struct.new` with descriptor and scalar payload | Copies primitive/enum/NFloat values; reference specializations preserve identity. |
| `newarr` | `array.new_default` | Struct elements get independent mutable GC storage; negative lengths throw. |
| `ldlen` | `array.len` | Returns native-width length; null throws a managed exception. |
| `ldelema` | Array owner/index or `array.get` for struct holder | Checked managed addresses preserve aliases across calls and element stores. |
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
| `unbox.any` | Checked `struct.get`; reference cast | Copies scalar payload; enum underlying compatibility preserves CLR behavior. |
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
| `ldtoken` | `global.get` / `i32.const` | Type descriptors or static RVA fields consumed by CreateSpan. |
| `conv.u2` | Optional `i32.wrap_i64`; `i32.const 65535; i32.and` | Integer inputs only; narrow results occupy an int32 stack slot. |
| `conv.u1` | Optional `i32.wrap_i64`; `i32.const 255; i32.and` | Integer inputs only; narrow results occupy an int32 stack slot. |
| `conv.i` | No instruction (i32/native) / `i32.wrap_i64` (i64) | Native integers use 32 bits; floating-point inputs remain unsupported. |
| `conv.ovf.i` |  |  |
| `conv.ovf.u` |  |  |
| `add.ovf` | `i32.add` / `i64.add`, sign-bit overflow test | Throws managed overflow when operands and wrapped result change sign. |
| `add.ovf.un` | `i32.add` / `i64.add`, unsigned result comparison | Throws managed overflow when the result wraps below an operand. |
| `mul.ovf` |  |  |
| `mul.ovf.un` | widened `i64.mul` / unsigned division guard | Throws managed overflow; native integers use WASM32 width. |
| `sub.ovf` | `i32.sub` / `i64.sub`, sign-bit overflow test | Throws managed overflow when opposite-sign subtraction exceeds signed bounds. |
| `sub.ovf.un` | `i32.sub` / `i64.sub`, unsigned operand comparison | Throws managed overflow when subtracting a larger unsigned operand. |
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
| `ldarga` | `local.get` | Addresses frame-owned scalar/struct storage or a managed reference slot. |
| `starg` | Optional narrowing; `local.set` | Replaces scalar arguments; copies structs into alias-preserving parameter storage. |
| `ldloc` | `local.get`; optional `f64.promote_f32`, `memory.copy`, or `struct.new` | Loads class references; struct loads copy values into independent storage. |
| `ldloca` | `local.get` | Addresses scalar/struct locals or managed reference slots, preserving aliases. |
| `stloc` | Optional `i32.extend8_s` / `i32.extend16_s` / `i32.const; i32.and` / `f32.demote_f64`; `local.set`, `memory.copy`, or `struct.set` | Copies structs; otherwise narrows scalars and marks the slot initialized. |
| `localloc` | stack global, aligned pointer arithmetic, `memory.fill` | Bounded frame lifetime; zeroes storage when InitLocals is set. |
| `endfilter` |  |  |
| `unaligned.` |  |  |
| `volatile.` | ordinary memory access | Single-agent target only; shared memory requires acquire/release lowering. |
| `tail.` |  |  |
| `initobj` | `memory.fill` or `struct.set` | Clears struct storage, including null references in GC fields. |
| `constrained.` | direct call | Struct implementations preserve byrefs; class receivers load then dispatch virtually. |
| `cpblk` |  |  |
| `initblk` |  |  |
| `no.` |  |  |
| `rethrow` | `throw_ref` | Preserves the original exception captured by the enclosing catch. |
| `sizeof` | `i32.const` | Reference-free closed types use the target layout size. |
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
are not implemented. Boxed or constrained calls requiring boxing remain unsupported.

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

Zero-based rectangular arrays (`T[,]`, `T[,,]`, up to rank 32) use a GC struct
containing immutable dimensions and a flat row-major backing array. Generic element
types, primitive/reference elements, independent struct copies, and element/field
byrefs share vector storage semantics. Every coordinate is checked separately;
`Length`, `Rank`, `GetLength`, `GetLowerBound`, and `GetUpperBound` work on exact
array receivers, including vectors. Empty dimensions produce empty storage.
Negative dimensions and a nonempty dimension product exceeding Int32.MaxValue
throw managed OverflowException (a compiler target limit; large CLR allocations
can instead report OutOfMemoryException). Engine allocation limits still apply.
Nonzero lower bounds, rectangular Array.Copy/Clear, covariance, and dimension
queries through an erased System.Array receiver remain unsupported.
[Rectangular-array tests](../Tests/Wasm/Arrays/RectangularArrayTests.cs) compare
2D/3D indexing, bounds, copies, and outstanding aliases with CLR execution.

Inline arrays repeat the element's storage with size `length * elementSize`. Numeric
buffers use the linear frame; string/object buffers repeat frame-owned reference slots.
Copies retain independent values/slots, and class fields flatten every repeated leaf.
Roslyn's inline-array helpers and Span/ReadOnlySpan length, empty checks, indexing,
slicing, CopyTo/TryCopyTo, and Span-to-ReadOnlySpan conversion work over both linear
backing and inline fields in GC owners. A span holds a rooted owner, selector, and
length in a 12-byte frame value. A null owner selects a linear byte address; a GC
owner selects flattened fields. Copies root the same owner in independent frame
slots; they do not copy the backing elements. NFloat element addresses use f32
storage consistently, including inline-array and span helpers. This compiler ABI does
not change XuiNet's classification of CLR spans as containing managed references.
Slice validates start/count without overflowing their sum. CopyTo supports overlap;
reference-bearing copies transfer values into destination-owned handles, preserving
live aliases and independent slots. TryCopyTo leaves a short destination unchanged.
[Span tests](../Tests/Wasm/InlineArrays/SpanTests.cs) compare these paths with CLR execution.

Single-element Span(ref T) and ReadOnlySpan(in T) borrow the original managed
address. Null-owner reference selectors use a tagged table index, distinct from a
linear address containing a reference-slot index; returning or slicing a span keeps
that distinction. No element copy is introduced.

String.AsSpan, implicit string-to-ReadOnlySpan<char>, and their integer ranges borrow
an immutable native string through a rooted string carrier and UTF-16 index. Reads,
slices, copies and returned spans retain the owner. Character-span ToString and
new string(ReadOnlySpan<char>) materialize native strings. Pointer-based span
construction accepts reference-free linear elements; reference-bearing elements fail
explicitly. General pointer locals and pointer arithmetic are still unsupported.
[String-span tests](../Tests/Wasm/Strings/SpanTests.cs)
compare UTF-16 code units, bounds, copying, and return lifetimes with CLR execution.

Array-to-Span/ReadOnlySpan conversions and array constructors, including start/count
ranges, borrow the GC array through that same owner/selector ABI. Scalars, class and
string references, and flattened struct elements alias their original storage.
Struct arrays use nominal array tags to distinguish otherwise identical WASM array
shapes; overlapping span copies preserve value semantics and outstanding refs.
Null arrays produce empty spans, and invalid ranges/indexing throw managed exceptions.
[Array-span tests](../Tests/Wasm/Arrays/ArraySpanTests.cs) exercise these behaviors in Debug and Release.

`localloc` reserves 16-byte-aligned blocks after the method's fixed linear frame.
Allocations remain live until method return or exception unwind, including multiple
allocations in loops; nested calls cannot reuse a live caller block. InitLocals zeroes
new storage. The shared linear stack is bounded to 1 MiB, with checked capacity before
advancing its pointer; exhaustion traps rather than throwing a managed exception.
The Span/ReadOnlySpan(void*, int) constructor borrows reference-free linear backing.
Roslyn's dynamic-size `mul.ovf.un` supports i32/native-int and i64 unsigned operands,
throwing managed OverflowException instead of wrapping. Stackalloc character buffers
can receive native-string spans and materialize new JS strings. Raw pointer
loads/stores remain future work. `sizeof` uses the WASM32 layout for closed,
reference-free types, including packed and explicit-layout structs; managed/reference
storage is rejected. Struct stackalloc uses these same field offsets and sizes.
[Stack-allocation tests](../Tests/Wasm/InlineArrays/StackAllocationTests.cs) cover
CLR parity, recursive calls, loop lifetimes, unwind cleanup, overflow and stack limits.

Constant `ReadOnlySpan<T>` data can be read from RVA-backed fields in supplied PEs.
`RuntimeHelpers.CreateSpan<T>` and Roslyn's bounded field-address/length/span-constructor
pattern retain fixed-width primitive or enum data once, in active WASM data segments
after the bounded frame stack. Returned spans borrow that stable backing. Native-width
values, reference-containing elements, arbitrary pointer constructors, and field handles
escaping these recognized sequences remain unsupported. Unused blobs are omitted;
field identities include their source module. See [static span tests](../Tests/Wasm/InlineArrays/StaticSpanTests.cs).

[The gradient demo](../Tests/Wasm/InlineArrays/Snapshots/Gradients.md) compiles the real
Xui `ReadOnlySpan<GradientStop> stops = [(0, 0xff0000ffu), (1, 0x0000ffffu)]`,
constructs `LinearGradient`, and reads its stops in a separate method. The supplied
Core/framework PEs provide Point/Color/GradientStop and tuple constructors; the
unsigned NFloat conversion used by Color is a scalar intrinsic.

Still unsupported: array covariance and arbitrary Array/BCL
collection methods and general span enumeration helpers remain unsupported. The span descriptor itself
lives in a linear frame; its borrowed backing can be linear or managed. Linear,
reference-free copies retain the memory.copy fast path. Managed copies use typed
field accessors and preserve overlap direction and destination reference ownership.

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

Enum byrefs preserve their underlying storage widths in linear locals and GC fields.
Direct constrained enum ToString and interpolation use retained names and flags
metadata, with signed/unsigned numeric fallback for unnamed values. Primitive enum
boxing is supported; boxed enum formatting in String.Format, general reflection and
arbitrary System.Enum helpers remain unsupported.


The [persistent mobile preview](../Tests/Wasm/TestApp/Snapshots/MobilePhone.html)
uses named exports to retain a session across frame/input calls and the real
emulator middleware's phone geometry/chrome. The full mobile-shell readiness probe
currently stops at `ScrollView.Content → SetProtectedChild<ScrollContent>`.
Unused generic virtual declarations no longer prevent ordinary vtable construction;
reachable generic method calls still reject explicitly. See the
[readiness snapshot](../Tests/Wasm/TestApp/Snapshots/MobileShellReadiness.md).

Closed generic interface methods now allocate a distinct dispatch slot per reachable
method argument list. Implementation selection preserves arguments through implicit,
explicit, inherited, variant, and default interface mappings. Class overrides are
selected before specialization without confusing hidden `new virtual` slots.
Only reachable implementations on instantiated receiver types are emitted. Direct
generic class-virtual calls remain unsupported; interface calls to generic class
implementations are supported. Tests cover separate int/string/i64 specializations,
struct results and byrefs, default overrides, and linked assemblies.

`ArgumentNullException.ThrowIfNull(object, string)` lowers to a null test and the
managed exception tag, including CallerArgumentExpression parameter names and
null/empty names. Strings, arrays, classes, and delegates retain their existing
reference representation. The helper's BCL body and localization resources are
not retained; pointer overloads and arbitrary boxing remain unsupported.

Default EqualityComparer<T> instances are rooted singleton GC values per closed T.
Default/Equals specialize for strings, numeric primitives, bool, char, enums, and
NFloat without loading ComparerHelpers or reflection. String equality is ordinal
and null-safe; floating equality preserves .NET NaN and signed-zero semantics.
Structs implementing IEquatable<T> call their concrete typed equality method,
including explicit implementations and closed generic structs. Arguments retain
value-copy semantics; NFloat field Equals handles NaN and signed zero.
Scalar/string IEqualityComparer calls compare the receiver with the exact default
singleton, then use the existing specialized operation or ordinary interface dispatch
for application implementations. Nominal is/as/casts distinguish closed default
singletons even when their GC shapes are structurally identical. Single-precision
arguments are promoted after ABI narrowing before equality comparisons. Null
receivers throw managed NullReferenceException. Named class/struct adapters and
class IEquatable contracts use the corresponding typed implementations. Custom
comparer subclasses retain interface dispatch. Default comparer GetType through
this interface, fallback ValueType.Equals/GetHashCode, and arbitrary boxed-value
equality remain explicitly unsupported.

String equality operators, static Equals(string, string), and instance Equals(string)
use ordinal UTF-16 equality over JavaScript strings. Static comparisons accept null;
instance equality throws managed NullReferenceException for a null receiver. Comparison-mode
static and instance overloads support Ordinal and OrdinalIgnoreCase through native
JavaScript strings. Ignore-case uses scalar simple uppercase mappings (including
supplementary letters), preserves lone surrogates, and avoids full-casing expansions
and .NET's excluded dotless-I/long-S mappings. Unicode data follows the host engine.
Invalid modes throw ArgumentException; culture-sensitive modes explicitly throw
NotSupportedException until a culture comparison contract is implemented.
Object overloads remain unsupported. Calls stage string arguments
before converting MSIL null into externref, including multiple null arguments.

Array.Copy's int-indexed three- and five-argument overloads emit GC array.copy
for identical, statically known vector types containing scalars or references.

`GC.AllocateArray<T>` and `GC.AllocateUninitializedArray<T>` with `pinned: false`
allocate ordinary, zero-initialized WASM GC vectors, including independent struct
element holders. Negative lengths throw managed `OverflowException`; pinned
requests explicitly throw `NotSupportedException`. Explicit BCL guard throws of
`OutOfMemoryException` have a managed identity and default message; this does not
translate an engine allocation failure into a catchable managed exception.

Array.Copy preserves object identity and overlap order; null, negative-index/length,
and capacity failures throw managed argument exceptions before mutation. Struct
elements use an overlap-aware field-copy loop into existing holders, preserving
outstanding element/field references and independent value storage.
System.Array-typed operands, covariance, element conversions, multidimensional
arrays, and 64-bit index overloads are not supported.

Array.Clear's whole-vector and int-indexed range overloads zero scalar elements
and null reference elements. Struct elements reset flattened fields in place so
existing element/field byrefs remain valid. Null arrays throw ArgumentNullException;
invalid ranges throw IndexOutOfRangeException before mutation. Empty ranges at the
end of an array are accepted. [Clear tests](../Tests/Wasm/Arrays/ClearTests.cs)
compare Debug/Release CLR and Node execution.

Default equality for named classes preserves null and reference-identity behavior,
retaining virtual Object.Equals overrides on reachable instantiated receivers.
Hidden new virtual Equals slots do not replace Object.Equals. Generic Array.IndexOf
and the comparer search helper execute scalar/string/class searches with managed
argument checks, including NaN equality, nulls, and range slices. Struct-element
search remains unsupported. The dependency graph explicitly records equality edges
so override bodies survive tree shaking even without an ordinary callvirt instruction.

Closed generic new T()/Activator.CreateInstance<T>() specialize public parameterless
constructors of concrete named classes. Allocation initializes the normal class
descriptor and invokes the selected constructor; no reflection cache is retained.
Managed constructor failures are wrapped in TargetInvocationException, preserving
InnerException and the ApplicationException hierarchy. Direct newobj still exposes
the original exception. Value-type factories, missing/private constructors, and
reflection-based overload selection remain unsupported.

Scalar locals/arguments whose addresses are taken use aligned frame slots, with
loads and stores routed through that one location. Primitive, enum, and NFloat
ref/in/out calls retain storage widths, float rounding, and aliasing. Frames unwind
on managed exceptions. Generic scalar ldobj/stobj/cpobj/initobj use typed loads and
stores. Scalar and struct GC-field addresses now cross calls and returns using
owner/selector pairs. These returns do not copy the value or allocate a byref wrapper.
Raw pointers into WASM GC storage remain unsupported.


Primitive boxing uses a WASM GC object containing the nominal runtime descriptor
and one mutable scalar payload. Supported values are Boolean, Char, signed/unsigned
integers, native integers (WASM32), Single, Double, enums, and NFloat. `box` copies
the value; aliases share the box. `unbox.any` returns a copy, whereas `unbox` returns
the existing owner/selector managed address without allocating another holder.
Loads, stores, and initialization through that address update the original box.
Null unboxing throws NullReferenceException; incompatible types throw
InvalidCastException. Enum unboxing permits the exact underlying integer type
and other enums with that same underlying type, while `is`, GetType, and typeof
retain nominal identity. ValueType/Enum and retained interface membership tests
also work. See the [boxing demo](../Tests/Wasm/Boxing/Snapshots/Primitives.md).

This increment requires linked framework definitions for primitive descriptors.
Arbitrary struct and Nullable<T> boxing, boxed virtual/interface dispatch adapters,
and general string/array-to-object conversions remain unsupported. A reachable
virtual dispatch that could target an allocated box is rejected during compilation.
General boxed ToString remains unsupported. Boxing alone adds no culture/resource
loading or host formatting dependency.

Composite `String.Format` accepts one to three object arguments or an object array,
with optional null provider. Supported boxes are converted inside WASM to tagged
primitive values before crossing into the shared JavaScript string runtime; GC
objects stay inside WASM. Fixed arguments also accept native JS strings directly.
Formatting supports repeated indexes, escaped braces, alignment, null, booleans,
UTF-16 characters, decimal and hexadecimal integers, and default/general/round-trip
floating-point text. Int64/UInt64 use BigInt without loss of precision. Integer
hexadecimal preserves the source width, including signed byte/short values.
Non-null providers, enum names, arbitrary object formatting, and unsupported numeric
formats throw managed NotSupportedException. Malformed formats throw FormatException;
null format/argument-array checks preserve ArgumentNullException parameter names.
The profile uses invariant text and neutral BCL resource messages; it does not load
cultures or emulate arbitrary ICustomFormatter implementations. Fixed-arity SR.Format
uses this same path, including Dictionary duplicate-key messages.

Closed `typeof(T).IsValueType` queries fold before reachability and stack analysis.
Proven boolean branches keep only their selected edge, preserving original IL
locations and any executed side effects. Unknown values remain runtime branches.
This removes the string-only randomized comparer path from value-key Dictionary
specializations. Natural loops may contain finite return/throw arms alongside their
normal continuation; arbitrary irreducible graphs still reject.

Scalar/reference array element addresses carry the array owner and checked element
index through the existing managed-byref ABI. They preserve aliasing across calls
without a per-address allocation. Struct array elements retain their existing holder
representation. Address-taken scalar locals and struct storage are zeroed even with SkipLocalsInit
so constructors, initobj, and out calls can initialize them through borrowed addresses. Floating `conv.i4` truncates toward zero and saturates
at Int32 limits, with NaN mapped to zero.


Static fields have persistent, zero-initialized GC holders, separate from instance
fields and from temporary frame storage. Nested value types flatten into the holder,
including numeric structs; loads copy values and stores update the existing fields.
Class/string/array fields keep their references. Managed static-field addresses use
the same owner/selector ABI as instance fields, without per-address allocation.
Only retained fields receive storage. Each closed generic type and each WASM instance
has independent storage and initialization state.

A generated guard executes each retained `.cctor` once. BeforeFieldInit types use
field-access guards. Strict types also guard static methods, instance constructors,
and value-type instance methods. Same-agent recursive initialization sees existing
zero/partially initialized fields. Managed failures become TypeInitializationException
with the type name, English message, and original InnerException; later accesses
rethrow the saved failure. Foreign host exceptions are saved and rethrown unchanged.
WASM traps remain fatal traps rather than managed initialization failures. This is
a single-agent protocol, with no cross-thread initialization locks.

Module initializers run before methods or static-field access in their owning
module, including linked assemblies accessed only through a field. Default storage
uses WASM global constant expressions; no explicit JavaScript init call is required.
The [initialization REPL](../Tests/Wasm/Statics/Snapshots/Initialization.md) shows
source, execution order, copied struct values, exit code, and WAT. Static storage
currently shares the existing supported field layouts: explicit-layout flattening,
arbitrary direct RVA/literal storage
access remain unsupported. Existing static-span and framework-constant intrinsics
keep their specialized paths.

Automatic value-type layouts use compiler-owned declaration-order storage. Numeric
values stay in linear storage and managed-reference fields use the existing GC or
frame reference slots. This supports the real DateTime tick constructors/accessors
and static MinValue/MaxValue; it does not imply CLR/native interop layout compatibility
or support for wall-clock/time-zone APIs.

NFloat conversions accept all integral scalar inputs, including byte/short color
components, native integers, and signed/unsigned 64-bit integers. They convert
directly to f32 before promoting to the MSIL floating stack representation, avoiding
double rounding through f64. WASM32 native integers retain their 32-bit width.

Shared acyclic control flow can lower to nested typed forward blocks, with each
instruction run emitted once and direct branches to its continuation. This fallback
supports nonempty stack results, managed byrefs and switches; cyclic graphs still
need supported natural-loop structure. RuntimeHelpers.IsReferenceOrContainsReferences<T>
is a compile-time query over closed type/field classification, not a call into its
recursive CLR intrinsic stub. Address-taken reference locals have initialized null
frame slots even for SkipLocalsInit methods, matching the existing scalar-frame policy.

`WasmEmitter.Methods` maps emitted function indices to managed method identities,
allowing hosts/tests to annotate engine `wasm-function[N]` stack traces without
adding debug metadata to the module. Shared exception subtrees define their block
types once, while repeated emissions reset loop-exit initialization state.

Delegate method-ID metadata larger than 10,000 entries uses immutable 4,096-entry
chunks, staying below engine limits for array.new_fixed. Lookup splits the table
index into a chunk and element index; virtual delegate identity stays unchanged.

Constrained calls to interface methods implemented by a closed struct resolve directly
to that implementation. The receiver remains a managed address, including structs
containing references, so mutations affect the original value. Explicit interface
implementations and specialized `List<T>` enumerator disposal use this path.

Primitive and enum array initializers copy bounded PE RVA data into independent,
mutable WASM arrays. Packed integers, UTF-16 code units, floating-point values,
and 64-bit values retain their declared widths and little-endian representation.

Managed arrays can be passed as `IReadOnlyList<T>` and `IReadOnlyCollection<T>`
for indexed reads and `Count`. Array and class implementations share call sites;
struct items are copied on reads. Interface casts use weak host metadata to retain
managed element identity even when WASM array storage is structurally identical.
Signed/unsigned CLI element compatibility and named class covariance are supported.
Exact-element `ICollection<T>` adds Count, IsReadOnly and CopyTo; Add, Remove and
Clear throw managed NotSupportedException because vectors have fixed size. CopyTo
copies struct fields into existing element holders, preserving outstanding byrefs.
Exact-element `IEnumerable<T>` uses the supplied CoreLib's managed vector enumerator;
its state checks, Current copies, Reset and Dispose compile through ordinary methods.
The dependency graph retains it only when a compatible vector and enumeration call
are reachable. Other collection operations, non-generic array contracts and collection
covariance remain explicitly unsupported; readonly class covariance remains supported.

Closed constrained class/interface receivers are loaded through their managed
address before dispatch. Calls preserve virtual overrides and managed null checks,
including receivers stored in locals, class fields, or array elements.

### Shared array pools

`ArrayPool<T>.Shared`, `Rent` and `Return` use a single-agent pool per closed
element type. One returned array of at most 65,536 elements is cached; rents
remove a suitable array before exposing it, so simultaneous leases stay distinct.
Larger returned arrays are left to WASM GC. `clearArray` clears reference and
value elements, preserving struct element holders for existing byrefs. This is
not the CoreLib bucket/thread-local implementation. Custom pools and `Create`
remain explicitly unsupported.

`Span<T>.Fill` supports primitive and reference elements across linear memory,
GC arrays and borrowed references. `Clear` also resets struct leaves in place.
Struct-valued `Fill` remains unsupported until its input can be snapshotted
independently of an overlapping destination.

`StringInfo.GetNextTextElementLength(ReadOnlySpan<char>)` uses the host's
`Intl.Segmenter` extended grapheme implementation, returning UTF-16 lengths.
Combining marks, surrogate pairs, emoji ZWJ sequences, flags, CRLF, malformed
surrogates and empty spans have CLR/Node comparisons. Unicode data follows the
browser version; exact parity across different Unicode releases is not promised.
The two-, three- and four-character-span `String.Concat` overloads also execute.

Primitive typed `CompareTo`, including constrained `IComparable<T>` calls,
specializes directly. Unsigned integers retain unsigned ordering; floating NaNs
sort below numbers and compare equal to each other, matching .NET. Culture-sensitive
string comparison remains outside this specialization.

Explicit `CultureInfo.InvariantCulture` can escape into locals/arguments as an opaque
singleton for `DefaultInterpolatedStringHandler` construction and
`String.Create(IFormatProvider, ref DefaultInterpolatedStringHandler)`. Formatting
stays in the existing invariant JS-string runtime; null providers use that same
neutral default. Other non-null providers throw managed NotSupportedException.
Culture objects, provider callbacks, reflection and casts involving this opaque
provider remain unsupported. Scalar ToString's existing nonescaping provider pair
still avoids allocating a token. [Provider tests](../Tests/Wasm/Strings/ProviderInterpolationTests.cs)
compare CLR/Node output and verify that custom providers fail explicitly.

Closed `constrained.` + `call` instructions for static interface members resolve
directly to the selected implementation, without an instance or runtime table slot.
Public, explicit, inherited class and default static interface implementations are
supported, including specialized generic methods. Ambiguous/reabstracted defaults
still fail explicitly. [Static interface tests](../Tests/Wasm/Interfaces/StaticInterfaceTests.cs)
compare those selection rules with CLR execution.

`Span<T>.ToArray()` and `ReadOnlySpan<T>.ToArray()` copy slices into owned GC arrays.
Linear values and managed references use the same typed address accessors as span
copy; struct fields are copied into independent element holders. This preserves
value semantics after the source frame returns. Empty spans produce empty arrays.
[Span materialization tests](../Tests/Wasm/Arrays/SpanToArrayTests.cs) cover linear,
managed, reference-containing and borrowed string backing.

`String.ToLowerInvariant()` calls a host import that applies simple scalar casing.
It preserves dotted capital I, avoids contextual final sigma, and preserves lone
surrogates. Unicode versions follow the browser engine, as with grapheme boundaries;
locale-sensitive casing and uppercasing remain unsupported.


## Borrowed GPU uploads and scalar runtime helpers

Raw pointer parameters/locals use WASM32 native integers. This allows a bounded
`stackalloc` buffer to be passed to a pointer-backed span or a synchronous host
import. Managed byrefs retain their owner/selector representation; they are not
converted into raw pointers. Pinning GC arrays and general pointer dereference
remain unsupported. `WasmEmitter.Emit(exportMemory: true)` exports existing linear
storage as `memory`; it rejects a conflicting export name or a module with no
linear storage. The default does not expose memory.

Trusted `BitConverter` float/int and double/long bit conversions, including unsigned
overloads, use WASM reinterpret instructions. Finite patterns, signed zero,
infinities and subnormals are covered by CLR/Node tests. Single values use the
compiler's promoted F64 evaluation stack, so exact Single NaN payload preservation
is not promised. `Math.Tan`, `MathF.Tan` and the reached NFloat tangent path use the
JavaScript numeric host alongside the existing trigonometric operations.

`GC.SuppressFinalize` checks for null and throws `ArgumentNullException("obj")`.
For objects without managed finalizers it has no further action. A compilation
retaining this intrinsic rejects reachable finalizable types; executing managed
finalizers is not implemented. This enables normal non-finalizable IDisposable
cleanup without importing the CLR's native finalization machinery.


`MemberAccessException`, `MissingMemberException`, `MissingMethodException` and
`MissingFieldException` support default/message construction, their nominal base
hierarchy, catches, casts and rethrow. A closed generic `Activator.CreateInstance<T>()`
for a concrete class with no public parameterless constructor now emits a managed
`MissingMethodException` at the call site. It does not fail compilation or invoke
private constructors. This uses the default English message, without CLR's dynamic
type-name suffix. Valid constructors retain their existing direct specialization
and TargetInvocationException wrapping. Abstract and value-type factories, the
class/member-name exception constructors and general reflection remain unsupported.


## Single-agent monitors

`Monitor.Enter(object)`, `Enter(object, ref bool)`, `Exit` and `IsEntered` retain
recursive entry counts in weak host metadata keyed by managed object identity.
They never invoke user callbacks or suspend. Nested locks and compiler-generated
try/finally release paths execute; an extra Exit throws SynchronizationLockException.
The ref-bool overload checks that the flag starts false and writes true only after
entry. Local, field and array-element flag aliases are tested against the CLR,
as are null inputs and release during exception unwinding.

These operations are for one JavaScript agent and provide no shared-memory
synchronization. TryEnter/timeouts, Wait/Pulse, blocking waits and thread ownership
across workers are not implemented. Direct native-string locks fail compilation;
object-typed string carriers throw NotSupportedException before entry because
content-based string canonicalization cannot preserve CLR monitor identity.
The opt-in [browser task runtime](../Runtime/README.md) supplies factory-based Lazy
and single-agent Task/async implementations without retaining the CLR scheduler.

### Browser application switches

The explicit `BrowserConfiguration` profile maps AppContext.SetSwitch and TryGetSwitch
to a managed per-module switch table. Unknown switches return false with an unset
result; callers can set and replace values. No host process environment or desktop
configuration file is consulted. This allows ordinary BCL LINQ materialization to
choose its default implementation. Real iterator-to-array tests cover tuples with
managed string fields, including CoreLib buffer allocation and interface dispatch.
Other AppContext APIs remain unsupported.

`new string(char, int)` fills a packed UTF-16 GC array with `array.new` and
materializes a native JS string through the existing js-string builtin. Empty
strings, null characters and unpaired surrogates retain their code units; negative
counts throw managed ArgumentOutOfRangeException with parameter name `count`.
Engine allocation failure is still a host failure, not a managed OOM guarantee.

### Typed managed buffers and array ranges

MemoryMarshal.GetArrayDataReference<T> and element-relative Unsafe.Add<T> keep an
owner plus a selector. Even ldelema of a struct keeps its array owner; advancing
the reference reaches the next element rather than leaving an isolated holder.
Buffer.Memmove<T> reuses span copying: reference-free linear backing uses
memory.copy, while GC backing and structs containing references copy typed fields
in overlap-safe order. These operations do not expose raw GC pointers.

CoreLib range slicing executes for arrays of sealed reference classes, strings
and non-enum named structs. The type-specialization pass proves their exact array
type, retains GetType's null failure and removes the reflection-only covariance
branch. Covariant classes, primitive/reduced-integer aliases and enums are not
assumed exact. General reflective Array.CreateInstance remains unsupported.


### Browser UTF-8 profile

`BrowserEncoding.Replacements` explicitly substitutes `Encoding` and `UTF8Encoding`
with a managed-span implementation. Array, string and span conversions support
UTF-8 scalar decoding, surrogate pairs, replacement fallback, strict fallback,
byte/character counts and the optional UTF-8 preamble. Invalid sequences consume
the Unicode maximal subpart; strict conversion throws managed encoding fallback
exceptions. Results become native JavaScript strings through the existing UTF-16
GC-array conversion. No GC storage is pinned or exposed as a raw pointer.

The profile currently covers whole-buffer UTF-8 conversion, not other encodings,
streaming Encoder/Decoder state, configurable fallback objects or pointer overloads.
Fallback exception byte/index payloads remain unsupported. CLR parity tests cover
malformed and truncated bytes, all isolated surrogate units and a Unicode scalar
sweep; Debug and Release modules execute conversion and span writes in Node.
The real mobile-shell probe now passes JSON UTF-8 transcoding with this profile.

`MissingManifestResourceException` supports typed/SystemException catches, default
and supplied messages, and inner-exception identity without ResourceManager.

The browser resource stream implements `ReadAtLeast`: short reads accumulate until
the requested minimum, zero requests perform no read, and EOF either returns the
partial count or throws EndOfStreamException. Invalid minima throw a managed
ArgumentOutOfRangeException. Debug/Release tests compare custom short-reading
streams with the CLR, including span writes and ReadExactly.


XML exception support preserves line number, line position, source URI, resource
text, messages and inner-exception identity. Public constructors and internal
resource constructors accepting a string or string-array argument emit through
the same WASM exception tag. Position suffixes and resource placeholders use
native-string formatting. The optional payload is retained only in modules using
XML exception members; other exceptions receive empty position fields there.
Constructor overloads taking IXmlLineInfo, serialization and HResult access are
not yet implemented. Tests compare public and internal constructors with the CLR,
including rethrow, finally, type-initializer and constructor-factory wrappers.
See the [.NET XML exception contract](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.Xml/src/System/Xml/XmlException.cs).


Default-comparer acquisition does not require unused equality or hash operations.
A struct declaring only GetHashCode can use that override without implementing
IEquatable. Calling unsupported struct equality still fails explicitly.
Array default comparers use reference identity, not element-wise equality;
identity hashes stay stable across element mutations and null hashes to zero.
This applies to vector, jagged and rectangular references, including comparer
interface calls, array searches and dictionary keys. Hash values are runtime-local,
not portable CLR hash codes.


Default hashing for structs containing only primitive value fields combines the
normalized scalar hashes in declaration order. Nested primitive-only structs,
enums, packed fields and target-width NFloat are supported; padding is ignored.
The resulting numeric hashes are compiler-runtime values, not replicas of CLR's
runtime-specific hash algorithm. Declared GetHashCode overrides still take
precedence. Reference fields, inline arrays and nested custom hash overrides
remain explicitly unsupported by this fallback.


Invariant formatting also recognizes NumberFormatInfo.InvariantInfo. Int32's
`ToString(null, invariantProvider)` uses native decimal formatting only when the
control-flow graph proves the format argument is null. Non-null or unknown
integer formats are not silently dropped. Floating-point round-trip formatting
uses the existing string host; actual XmlConvert integer/single/double calls pass
CLR/WASM comparisons, including XML infinity spellings and negative zero.
Custom decimal formatting, other culture objects and arbitrary provider callbacks remain
unsupported.

### Exact invariant decimal text

A proven `Decimal.ToString(invariantProvider)` call (or the null-format overload)
reads the supplied CoreLib representation through the existing managed-address
accessors. Linear locals, array elements and inline class fields share this path.
The host joins the 96-bit coefficient with `BigInt`, inserts the stored decimal
scale, and suppresses the sign for zero. No floating-point conversion is involved.
The real `XmlConvert.ToString(decimal)` body uses this specialization. Tests compare
CLR and emitted Debug/Release WASM for extrema, scaled zero, preserved trailing
zeros, and randomized coefficients. Custom formats and arbitrary providers remain
explicitly unsupported; this does not implement general decimal arithmetic.

Invariant provider/null-format pairs also support all signed and unsigned 8-, 16-,
32- and 64-bit integers. The host formats i64 values with BigInt and reinterprets
unsigned values at their declared width, including UInt64.MaxValue. These paths
read managed addresses and preserve class-field/array aliases. Custom integer
formats still require separate support.

### Managed exception diagnostic text

Built-in exception `ToString()` works both directly and through `object`. It uses
the existing managed payload instead of retaining CoreLib's native string
allocation path. Type names, default/custom/empty messages, argument actual
values, nested inner exceptions, and single-inner aggregate messages are preserved.
Message/inner constructors now apply to the supported built-in exception types.
`PlatformNotSupportedException` is supported, including its NotSupportedException
catch hierarchy. Unsupported derived exception representations fail explicitly.

Diagnostic formatting currently has no captured managed source stack frames.
Tests compare unthrown exception text with .NET; a thrown exception retains its
payload and inner chain but does not acquire a CLR-style stack trace.

### Browser JSON profile

The opt-in `BrowserJson` profile supplies native JSON DOM parsing for the GPU
floorplan metadata. It retains numeric lexemes for exact integer reads and uses
GC-owned document tokens with disposal checks. It supports string/UTF-8-stream
input, property lookup, array cursors and primitive getters, with explicit depth
and duplicate-key policy. See [runtime limitations](../Runtime/README.md#browser-json-documents).
The original 148 KB atlas metadata is checked field-by-field against the CLR.

### Invariant SVG numeric parsing

Adjacent invariant-provider calls to `Double.Parse` and `UInt32.Parse` accept
strings and borrowed `ReadOnlySpan<char>` slices. The native string runtime
preserves floating-point bits, unsigned ranges, signs, exponent/group syntax,
hexadecimal/binary digits and managed null/format/overflow/style failures.
Double supports Float/AllowThousands subsets; UInt32 supports Integer subsets,
HexNumber and BinaryNumber. Other valid styles explicitly throw NotSupportedException;
arbitrary providers and locale-aware parsing remain unsupported. CLR/Node tests
compare both Debug and Release output, including malformed and boundary inputs.

### Ordinal string replacement

`String.Replace(string, string)` and `String.Replace(char, char)` now use native
JavaScript strings. Matching and replacement preserve UTF-16 code units, are
non-overlapping, and treat replacement dollar signs literally. Null new strings
remove matches; null/empty old strings throw managed argument exceptions.
Culture/comparison overloads remain outside this intrinsic.

### Character-buffer strings

The `String(char[])` and `String(char[], int, int)` constructors copy managed
UTF-16 arrays into native JavaScript strings, including NUL and unpaired surrogate
code units. Subsequent array writes cannot mutate the string. Null whole arrays
produce empty strings; sliced construction preserves managed null/range failures
and parameter names. No pinned array or CLR string allocation is retained.

### Single-agent memory fences

`Thread.MemoryBarrier()` and `Interlocked.MemoryBarrier()` are recognized as
zero-stack-effect operations and emit no instructions. The module has no shared
memory or concurrent agents; ordinary writes are already sequenced before later
reads. Reachability records the intrinsic without retaining CLR Thread types.
This does not provide cross-worker synchronization or shared-memory support.

### Execution-stack queries

Modules reaching `RuntimeHelpers.TryEnsureSufficientExecutionStack` or
`EnsureSufficientExecutionStack` track emitted method-body depth in an instance-local
global. The query reports insufficient space at 192 active bodies; the throwing
form raises managed `InsufficientExecutionStackException`. Entry at depth 256 also
throws, leaving a reserve between the advisory and hard limits for cleanup. Normal
returns and exception unwinding release the depth count, including methods without
linear frames. Modules without either query receive no depth instrumentation.

This is a conservative target policy, not measurement of the browser's native stack
bytes or the CLR stack limit. Large native frames, host recursion and WASM traps
still have engine-specific limits; linear stack-frame bounds remain independent.

### Formatted integer interpolation

Interpolated signed/unsigned integers of every 8/16/32/64-bit width support general
formatting and decimal/hexadecimal `D`/`X` formats with precision through the native
JavaScript numeric formatter. Int64/UInt64 remain BigInt throughout; negative hex
values retain their declared width. Invalid formats throw FormatException, and
unsupported formats throw NotSupportedException. Alignment and culture-sensitive
providers remain unsupported. This does not expand the direct integer ToString
overloads, whose invariant-provider recognition remains separate.
