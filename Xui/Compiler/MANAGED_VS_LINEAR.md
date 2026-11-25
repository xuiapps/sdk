# Managed objects and linear memory

Xui needs two useful properties of .NET at once. View hierarchies, services, and
closures need managed object identity and garbage collection. Points, matrices,
vertices, and other numerical data benefit from compact value storage, predictable
layout, and contiguous memory. A struct is a value with copy semantics; it does not
by itself promise stack allocation or a particular physical representation. Our
compiler should preserve those semantics while choosing storage appropriate to the
use: linear memory where layout and address arithmetic matter, and WASM GC storage
where object references must remain visible to the collector.

This boundary is more consequential than translating arithmetic instructions. WASM
GC references are opaque values, not addresses we can write into linear memory.
Native JavaScript strings have the same constraint. A byte buffer can represent a
matrix perfectly well, but it cannot directly hold a live reference to a View or a
JavaScript string. Conversely, a WASM GC field is not memory to which we can expose
an arbitrary byte pointer. Supporting both storage systems means that a C# managed
reference cannot always be represented by one linear address.

The BCL is where these requirements meet. A dictionary brings specialized generic
types, equality and hashing, arrays of entries, references to entry fields,
resizing, and exception paths. Even `TryGetValue`, whose public result is a copy,
can use a byref-returning helper internally. Exceptions bring another dependency
chain: constructors, inheritance, unwinding, argument names, and localized resource
strings. Much of our recent effort has therefore gone into preserving runtime
semantics, rather than adding individual MSIL opcodes. That work is reusable: the
same managed-address machinery serves collections, spans, struct methods, and
Xui's short-lived event arguments.

## Values belong to their destination

Today, addressable structs can live in linear frames. Reference-containing frame
values use indices into frame-owned GC reference tables; frame cleanup clears the
references. Classes instead hold GC-visible references, with embedded struct fields
flattened into their owning object. Managed struct arrays currently use a GC holder
per element, including elements whose standalone values use linear storage. This
keeps element locations stable during assignment, but has an allocation cost that
we can optimize later. It is not yet the contiguous numerical-array representation
we want for large math workloads.

Copying between these representations copies the value's fields, not the physical
encoding of its references. Inserting a stack struct into a dictionary must transfer
its string or object references into dictionary-owned GC fields. It must never
leave the dictionary holding indices into a temporary caller frame. Value copies
are independent, while the objects referenced by those copies retain their shared
identity. A list or array field is itself one reference; its contents are not
deep-copied with the enclosing struct.

## A managed address is a location

The owner-and-selector representation now covers scalar and struct byrefs as well
as reference-valued fields. Conceptually, `ref T` carries a GC-visible
owner plus a location. A linear location identifies an address; a GC location
selects fields in its owner through generated, typed accessors. A defined null
representation supports `Unsafe.NullRef<T>` and `Unsafe.IsNullRef<T>` without
converting GC references into raw pointers. The exact type remains compiler
metadata, supplied by generic specialization, rather than a fresh runtime wrapper
allocated for every reference.

This representation must survive parameters, returns, locals, and control-flow
joins. Field projection must derive another location, and reads and writes must
reach the original storage. Copy-in/copy-out would be incorrect: two references
to the same field must observe each other's writes immediately. Keeping the owner
in a WASM reference slot also keeps the referenced storage alive. Array replacement
must not silently retarget an existing reference to a different array, and this
does not extend the BCL's documented guarantees for references across collection
mutations. Raw pointers remain a separate feature restricted to suitable linear
storage; a managed address is not a promise of native layout.

## Where effort goes next

Managed locations now survive calls and returns, including primitive and embedded
struct fields. Null-reference checks and struct-array copies execute. The next
step is continuing through the actual dictionary implementation and its constrained
hashing calls. Tests need to
check aliasing, nested fields, value-copy independence, GC ownership, and cleanup
through exceptions, as well as the final values. Linear math storage remains a
first-class path; GC-backed storage is not a reason to turn every point or matrix
into an allocated object.

For BCL dependencies, the policy is similarly selective. We compile ordinary
method bodies and specialize the required closed generic types. Neutral resource
strings are read from the supplied assembly images and only referenced strings
are embedded; full runtime localization is not implemented. Small runtime bridges
cover representation boundaries such as native JavaScript strings and the current
managed exception payload. Structured switches now execute the actual CoreLib
argument-name and resource-message selectors. These foundations reduce the need
for one-off replacements without requiring us to ship all of CoreLib before a
Xui window can run.
