# Stack structs with managed references

[Events](Snapshots/Events.md) compiles a stack event containing a numeric Point,
a string, and an interface reference. Nested `ref` calls mutate the original;
a value copy keeps its own fields while sharing the referenced object. The snapshot
includes the source, output, exit code, and WAT reference-table operations.

The tests compare Debug and Release MSIL execution with Node for aliasing,
`ref`/`in`/`out`, local aliases, nested fields, constructor results, interface dispatch,
class-field transfers, and 80,000 caught exceptions. A separate host test exports
the otherwise-private reference tables in a test-only module and checks that every
slot is cleared after normal return and an uncaught exception, across repeated calls.

The tables are preallocated once per module instance. No GC holder is allocated for
these stack structs; ordinary classes, exceptions, and first-use strings still allocate.
Primitive/reference array-element addresses and general spans remain separate compiler work.

`ReturnAddressTests` exercises caller-owned scalar and struct addresses, returned
class/string field locations, interface and virtual returns, and multicast delegates
whose last handler supplies the returned address. Debug and Release runs compare
with CLR execution; aliases retain their original owner and caller slots survive
callee cleanup. `InteriorAddressTests` covers mixed linear/GC locations, nested entry fields, array
growth, value-copy independence, null locations, and initialized byref locals.
GC value-field returns preserve their original owner. No per-byref object is allocated.
