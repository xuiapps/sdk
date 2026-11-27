# Browser managed runtime

This project contains small managed implementations of APIs whose desktop runtime
depends on CLR services. XuiNet compiles this PE alongside the application. The
application keeps its original assembly references and C# source; the explicit
`BrowserTasks.Replacements(coreLibrary, browserRuntime)` binding profile redirects
the selected type definitions before reachability, specialization and layout.
There is no runtime type-name lookup or fallback to an unsupported CLR body.

The first profile covers `Task`, `Task<T>`, their awaiters and configured awaiters,
the async task method builders, and factory-based `Lazy<T>`. Roslyn still generates
the application's async state machine. The builder retains its completion task
and copies the state machine into a GC holder at each suspension; resuming calls
that holder's `MoveNext`. This preserves value state across calls and permits
multiple waiters. It currently allocates a holder and delegate per suspension.

`NativeScheduler.Post` is the host boundary. The compiler passes a declared WASM
function reference and a managed Action to `xui-task.post`; JavaScript retains
both until a timer invokes the callback. Work is deferred on the same browser
agent. This is not a thread pool: a long computation still occupies that agent
until it returns. Completed-task awaits may proceed synchronously, as ordinary
C# awaits do. Pending `.Result` throws instead of blocking the browser agent.
`ConfigureAwait(false)` uses this same agent; synchronization contexts and
execution-context flow are not implemented.

`Environment.CurrentManagedThreadId` is the stable positive value `1` for this
single agent, including deferred task continuations. Generated iterator reuse
checks therefore behave consistently; separately requested enumerators still
have independent cursors. This does not expose worker or OS thread identifiers.

The supported factories are `Run(Action)`, `Run(Func<T>)`, `FromResult`,
`FromException` and `CompletedTask`. `await` propagates the original managed
exception; `Result` and `Exception` expose a single-inner `AggregateException`.
Cancellation, blocking waits, delays, Task.WhenAll/WhenAny, task-unwrapping Run
overloads, ValueTask and async-void builders are outside this profile. Missing
members fail compilation. Lazy caches its factory value or exception and rejects
recursive factory access; threading modes and other constructors are unsupported.

The integration tests in `Tests/Wasm/Threading/BrowserTaskTests.cs` compile ordinary
System.Threading.Tasks source, select the profile, emit WASM and execute it in
Node with the shared browser host imports. Tests cover asynchronous ordering,
state-machine values, finally, shared waiters, faults and lazy factories.

## Packaged resources

`BrowserResources.Replacements(coreLibrary, compression, browserRuntime)` binds
`Assembly`, `Stream`, `GZipStream` and the primitive-reading part of `BinaryReader`.
The original application still calls `typeof(T).Assembly.GetManifestResourceStream`.
The compiler identifies an owning assembly by its name and module MVID; a resource
catalogue carries the original embedded PE bytes beside the module. Linked files
are rejected. Hosts prepare this catalogue before calling the WASM entry point.

The optional `ResourceJavaScript.Source` defines `createResourceHost`. It decodes
base64 assets, decompresses `.gz` assets with `DecompressionStream`, and exposes
`xui-resource` imports. This startup step is asynchronous; later managed reads are
synchronous. Independent stream cursors live in a JavaScript WeakMap keyed by
their WASM objects. Disposal releases those entries. Gzip errors are recorded
during preparation and become managed `InvalidDataException` on reading.

Each read borrows at most 4 KB of the current WASM linear-memory frame, then copies
into the destination span, including spans backed by GC arrays. No pointer escapes
the import. The host reacquires the memory view on each read. The real Floorplan
GPU atlas test checks every mip byte against .NET, including reads larger than a
linear frame. This is a correct staging path, not a zero-copy asset loader.

This profile supports read-only packaged streams, seeking in uncompressed
resources, primitive little-endian reads, EOF/disposal failures and `leaveOpen`.
BinaryReader's encoding constructors accept the original encoding argument, but
character/string reading is not implemented. Gzip requires a prepared compressed
resource at its beginning; arbitrary compressed streams, compression, asynchronous
I/O and file/network access are outside this profile. Missing members fail at
compilation; unsupported gzip sources fail explicitly at execution.

`RuntimeImportAttribute` declares the primitive/reference host boundary inside
the selected browser runtime assembly. Reachability treats those methods as
imports, validates their signatures and emits imported WASM calls instead of
compiling their placeholder bodies. This provides a reusable boundary for future
browser-backed runtime services without putting app-specific calls in CodeEmitter.

## Weak references

`BrowserWeakReferences.Replacements` supplies `WeakReference` and `WeakReference<T>`
for managed WASM objects. Add `WeakReferenceJavaScript.Source` and bind
`xui-weak` to `createWeakReferenceImports()` when selecting that profile. Its
WeakMap associates each managed handle with a JavaScript WeakRef to the target;
it does not keep a strong target registry. Retrieval returns the same managed
reference, and normal GC references keep a retrieved target alive while used.
Collection timing remains controlled by the browser, including JavaScript's
same-job retention after WeakRef retrieval.

Null targets, replacement, typed retrieval, Target and IsAlive are supported.
Resurrection tracking requires finalizers and explicitly throws. Native JS strings
also throw: weak lifetime must not depend on the temporary object carrier used
when passing a primitive string as object. Serialization members are absent and
fail compilation if referenced. Tests cover live target identity, replacement,
clearing and these unsupported cases without relying on nondeterministic GC timing.

## Browser JSON documents

`BrowserJson.Replacements` binds JsonDocument, JsonElement and its array cursor.
The production GPU Floorplan loader keeps its existing System.Text.Json calls.
The optional `JsonJavaScript.Source` supplies `createJsonHost().imports` under
`xui-json`. JSON syntax/string decoding uses the JavaScript engine; a separate
token table retains original number lexemes, so Int64/UInt64 reads never round
through Number. Element structs retain their document, and disposal releases the
host token table and makes further element reads throw ObjectDisposedException.

The implemented surface is standard JSON parsing from strings or UTF-8 streams,
property lookup (last duplicate wins), array indexing/enumeration, primitive
getters and checked integer TryGet operations. Depth and duplicate-property
options are honored; comments and trailing commas are explicitly unsupported.
The stream stays open after parsing. Invalid UTF-8 streams are rejected eagerly.
Serializer/reflection support, object enumeration, mutation, cloning and arbitrary
encodings are not supplied. JsonException currently carries message and inner
exception; its path/line/byte-position API is not implemented. Browser parser
error wording is engine-specific. Tests compare the real atlas's every label and
numeric field against .NET, in addition to document lifetime and numeric edges.

## Explicit unmanaged buffers

`BrowserNativeMemory` binds only `NativeMemory.Alloc`, `AllocZeroed` (both overloads)
and `Free` to the browser runtime. Supply `createNativeMemoryHost(() => api.memory)`
as the `xui-memory` import object. Each instance owns a 16-byte-aligned free-list
heap after the initial stack/static-data region. Allocation may grow the module's
memory, up to its 2 GiB limit; failure returns through a managed OutOfMemoryException.
Free coalesces reusable ranges; committed WASM pages do not shrink. Null free is
accepted, zero-size allocation returns a releasable address, and stale/double free
is diagnosed by the host. All borrowed JS buffer views must be reacquired after
memory growth. Ordinary classes and arrays remain WASM GC objects. Realloc, custom
alignment APIs, shared/threaded allocation and pinning GC arrays remain unsupported.

## UTF-16 conversion

The encoding profile also binds UnicodeEncoding and the Unicode/BigEndianUnicode
singletons. Managed-span conversion preserves little/big-endian code units, BOMs,
valid surrogate pairs and .NET replacement/strict fallback behavior. Tests compare
every UTF-16 code unit and randomized malformed byte sequences against CoreLib,
then exercise the public APIs in Debug/Release WASM. Locale/code-page services remain separate work.

All emitted linear memories now permit demand growth up to 2 GiB, including
modules that do not import the unmanaged allocator. Initial pages cover only the
existing stack/static region. The frame stack retains its own fixed bounds check.

## Stateful Unicode decoding

`Encoding.GetDecoder` and the explicit Decoder type binding retain unfinished
UTF-8/UTF-16 characters across array reads. GetCharCount does not consume pending
state; flush applies strict/replacement fallback, and Reset drops pending input.
Convert supports bounded output buffers with partial progress. A partial character
uses at most three pending bytes; joining it with the next input chunk currently
copies that chunk. This is a correctness-first loading path, not a zero-allocation
streaming codec. Custom fallback objects and pointer/span Decoder overloads remain
unsupported. CLR comparisons exercise every split of valid/malformed fixtures,
and Debug/Release WASM tests cover chunking, conversion, flush and reset.

`Encoding.GetEncoding(string)` accepts the supported canonical Unicode names
(`utf-8`, `utf-16`, `utf-16BE`) and the `unicode`/`unicodeFFFE` aliases, ignoring
case. Other names explicitly throw NotSupportedException; this is not a general
legacy code-page registry. Null preserves ArgumentNullException.

## Mutable string construction

`BrowserStringBuilder` explicitly replaces `System.Text.StringBuilder` with a
managed UTF-16 buffer. Append, clear, length/index edits and string slices operate
on GC character arrays; ToString copies into an independent native JavaScript
string. No CLR string allocation, pinning or mutable JS string is required.
The initial profile covers constructors with capacity/initial text, capacity
reservation, append of strings/objects/characters/arrays/spans, repeated characters,
and append-line using the browser's LF newline. Capacity growth is a target policy;
the BCL's internal chunk layout is not reproduced. Other StringBuilder members
remain unresolved instead of silently falling back to CLR memory operations.
