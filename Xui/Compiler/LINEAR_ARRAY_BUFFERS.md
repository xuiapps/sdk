# Linear arrays, borrowed buffers and GPU uploads

The current hardware-cube path starts with a normal managed `Vertex[]`. The browser
GPU adapter copies its values into a temporary linear buffer, then gives JavaScript
an address and byte count for a synchronous GPU upload. That copy is an intentional
bridge between two storage systems. It gets the real Xui rendering code running
without pretending that a WASM GC reference is a native pointer. It is not the final
storage strategy for large meshes, particle simulations or continuously updated
geometry. This essay continues [Managed objects and linear memory](MANAGED_VS_LINEAR.md)
with the bulk-data and rendering side of that boundary.

## Three kinds of storage

WASM linear memory is an addressable sequence of bytes. GC arrays and structs are
accessed through typed operations and opaque references; their physical addresses
are not part of the program's interface. A browser engine can choose an efficient
internal representation, but our generated code cannot turn its GC-array reference
into an offset in `WebAssembly.Memory`. The distinction is an address-space rule,
not a claim that all GC arrays are physically scattered or slow.
See the [WASM memory overview](https://webassembly.github.io/gc/core/intro/overview.html)
and [reference types](https://webassembly.github.io/gc/core/syntax/types.html).

Our current representation adds a more specific cost: a managed array of structs
uses a GC holder for each element. This preserves value-copy semantics and stable
element byrefs, including references to nested fields, but adds allocations and
indirection. That is our compiler's implementation choice, not a requirement that
WASM impose on every numerical array. Primitive GC arrays already have a different
representation from arrays of struct holders.

`GC.AllocateArray<T>` and `GC.AllocateUninitializedArray<T>` now use this same GC
storage when `pinned` is false. We initialize both; the latter permits skipping
initialization but does not require uninitialized bytes. Neither factory creates
a linear allocation. A request for pinned storage throws `NotSupportedException`
instead of producing an address that cannot be honored. LINQ and other BCL code
can use these factories without depending on the CLR's native array allocator.

Byte-oriented operations do not always require a linear allocation. Our current
`Buffer.BlockCopy` lowers same-type byte-vector copies to `array.copy`. For wider
primitive elements, it extracts and replaces the requested little-endian bytes
inside GC values, preserving overlap and floating-point bit patterns. This gives
JSON/resource loaders their ordinary byte-copy behavior without pinning or a
temporary buffer. It is a logical byte view, not an address that can be handed to
WebGL; borrowed GPU uploads still need the linear staging path below.

Typed buffer operations can also remain managed. `GetArrayDataReference<T>` and
`Unsafe.Add<T>` carry the GC array owner plus an element/field selector, even when
the reference originally came from `array[i]`. They are not physical addresses.
`Buffer.Memmove<T>` uses the same field-aware copy path as spans for GC storage and
reference-containing values, preserving overlap and reference ownership. Only
reference-free linear backing takes the raw `memory.copy` path. Ordinary array
range slices can therefore run without a CLR write barrier or a staging buffer;
this does not make those arrays directly uploadable to WebGL.

GPU buffers are a third storage system. A JavaScript typed-array view over WASM
memory does not itself copy the bytes, but WebGL `bufferData` copies supplied data
into a buffer object's storage. Keeping a mesh in linear memory therefore removes
the GC-to-linear staging copy; it does not make CPU memory become GPU storage or
promise a zero-copy driver implementation. The caller's input can be reused after
the upload call returns even though GPU execution continues later.
See [WebGL buffer operations](https://registry.khronos.org/webgl/specs/latest/2.0/#3.7.3).

## How the current path works

```text
Managed Vertex[] / ReadOnlySpan<Vertex>
    -> copy fields into stackalloc Vertex[count]
    -> pass WASM32 offset + byte length + vertex stride
    -> JavaScript creates a temporary view over exported memory
    -> WebGL uploads into its vertex buffer
    -> draw into a color/depth target
    -> compose that target into the page's Canvas surface
```

`IGpuCommandList.SetVertexBuffer<T>(ReadOnlySpan<T>)` and
`SetConstantBuffer<T>(in T)` express borrowed uploads of unmanaged values. Native
backends inherit defaults that pin the input only around their synchronous pointer
overload. The browser implementation specializes the methods, copies into a linear
scratch frame, and calls the host before that frame ends. Constants follow the
same rule: the cube's matrix is copied into a small temporary buffer.

The pointer is an integer offset into this module's linear memory. It is not a
JavaScript object handle or a managed `ref T`. The host checks byte ranges and
consumes the view immediately; it keeps neither a pointer nor a view after the
call. Frame cleanup reclaims scratch storage. This currently has a bounded stack
budget, so an arbitrarily large mesh cannot use this path unchanged. Supporting
pointer parameters does not imply support for pinning GC arrays or every unsafe
pointer operation.

The retained target has a longer lifetime than an upload frame. The browser owns
its WebGL resources; composition reuses the target until its size changes or its
view disappears. The target is presented into a Canvas surface with the normal
transforms and clipping. This presentation step can also consume bandwidth. A
faster mesh upload alone does not eliminate composition or render-target costs.

## A better home for large numerical arrays

For large reference-free datasets, the intended path is an explicitly owned linear
allocation with length, element stride and alignment. Its managed owner can be a
small GC object, while its payload lives contiguously in linear memory. A span can
borrow that payload for math, and the GPU uploader can use its address directly.
This allocation is independent of the call stack; temporary scratch buffers still
serve short-lived transformations. Neither persistent linear allocation nor an
automatic conversion of ordinary `T[]` into this representation is implemented by
the current upload work.

An ordinary managed array must retain its identity, length, bounds behavior,
zero-initialized contents and aliasing semantics. An escaped element byref must
keep referring to the same storage. We cannot copy an array into linear memory and
quietly let existing references keep modifying the original GC holders. A future
linear-backed managed array must make all its consumers agree on that backing;
an explicitly owned buffer is a smaller first step. Disposal must not leave live
spans or retained host callbacks pointing into reused storage.

Start with points, vectors, matrices, vertices and indices whose elements contain
no managed references. A string, array or View field cannot become a raw pointer
inside the bytes. Persistent buffers containing reference handles would require
their own GC-root ownership and cleanup; borrowing indices from a temporary
managed-reference stack would be incorrect. We should keep that complexity out
of the numerical-buffer path.

Contiguous layout also needs an agreed ABI. Element size, padding, field offsets
and vertex stride must match what the GPU consumes. Uniform buffers have an
additional shader-layout contract: our GLSL uses std140 and row-major matrices.
The cube's matrix fits that contract; an arbitrary C# struct may need explicit
packing or a generated conversion. Array-of-structs is convenient for interleaved
vertices, while separate component arrays may suit some CPU math loops better.
That choice should follow measured workloads, not a universal layout rule.

## Where performance work should go

Compared with our current holder-per-element representation, contiguous linear
payloads should reduce allocation pressure, pointer chasing and staging bandwidth.
That is an engineering expectation, not a measured speedup. GC scalar arrays can
also be efficient, and bounds checks, accessors, conversion work, upload frequency
and the browser's implementation all influence the result. Explicit SIMD emission
would be additional compiler work; choosing linear storage does not add it by itself.

For the cube, avoiding redundant uploads may matter more than changing array
storage. Its mesh is static while its matrix changes. A persistent vertex buffer
uploaded once, followed by small constant updates, avoids both the staging copy
and the vertex upload on subsequent frames. Dynamic geometry needs reusable CPU
storage, persistent GPU capacity and updates of changed ranges. The current host
reuses buffer objects but still calls `bufferData` on each upload; it does not yet
track mesh dirtiness or preserve uploaded content through a dedicated mesh API.

The useful comparison is therefore: GC data staged every frame; persistent linear
data uploaded every frame; and persistent GPU data updated only when necessary.
Measure allocations, CPU math time, staging bytes, uploaded bytes, frame time and
resource counts separately. Include repeated resize/disposal and large inputs.
The existing upload and browser tests establish correctness and ownership, not a
performance benchmark. No threads are required for any of these designs: begin
with single-threaded preparation and browser scheduling, then consider workers
only when measurements justify a separate concurrency model.

## Explicit unmanaged allocation

The browser NativeMemory profile now allocates persistent buffers directly in
linear memory, after the frame stack and static PE data. Its instance-local host
allocator grows WASM memory and recycles freed blocks. These buffers survive
method returns; their owner must call Free. Memory growth can replace JavaScript's
ArrayBuffer view, so upload hosts must reacquire views from the current memory.
This does not move GC arrays into linear memory or make them pinnable. Typed GC
array uploads still copy; explicitly unmanaged buffers can avoid that copy when
the consuming API accepts their pointer. The software renderer still requires
raw pointer operations and integration checks beyond allocation itself.
