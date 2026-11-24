# Value-type layout

`LayoutResolver.Classify(typeHandle)` separately reports `StorageKind.LinearMemory` for instance storage without managed references, or `ContainsManagedReferences` for direct or nested object references and managed byrefs. It follows value-type fields across supplied assemblies, recognizes NFloat, and ignores static fields. Strings, objects, arrays, class and interface fields require reference tracking. Unresolved external value types and open generics throw instead of being assumed reference-free. Results are cached independently from layout and do not depend on pointer size.

Classification describes reference-tracking requirements, not a final WASM allocation strategy. `LinearMemory` does not promise that `Resolve` supports the type's layout (for example, auto layout remains unsupported). `ContainsManagedReferences` does not mean boxing or force a GC allocation: lowering may use separate locals, GC fields, or handles. A managed byref can point to stack storage rather than a heap object. No pointer-sized byte layout is assigned to an externref.

`new LayoutResolver(module, TargetLayout.Wasm32).Resolve(typeHandle)` calculates an unboxed struct's size, alignment, and instance-field offsets and sizes. The overloads accepting `TypeIdentity` specialize closed generic fields; caches include their arguments and defining module. Results are cached per resolver; the target is explicit and never inferred from the host process. `TargetLayout.X64` supplies the corresponding 64-bit scalar rules for comparison. These profiles are data-layout rules, not complete calling conventions or managed object models.

Sequential fields retain metadata order, with each offset aligned to the smaller of its natural alignment and the packing limit. Nested structs contribute their own size and alignment. Explicit layouts use the declared field offsets and permit overlap for supported storage types. With no declared size, the final extent is rounded to the struct alignment. A nonzero metadata size sets a minimum extent without implicit tail rounding. Empty structs occupy at least one byte. Static fields do not contribute to instance layout.

Supported storage includes primitive numeric fields, one-byte managed Boolean, two-byte managed Char, native integers, enums, linked and closed generic nested structs, and the well-known `System.Runtime.InteropServices.NFloat` type. WASM32 uses four-byte native integers and NFloat; X64 uses eight bytes. These are managed storage sizes, not marshalled representations: `MarshalAs` and character-set transformations are outside this resolver.

Reference fields, classes, arbitrary auto-layout structs, open generics, and other unresolved external value types throw `NotSupportedException` from numeric layout. Framework `ValueTuple` auto layouts are deliberately laid out in declaration order for our target; these compiler-owned bytes are not a CLR interop guarantee. Reference-containing generic specializations classify correctly and are lowered separately by the backend. Resolved framework NFloat definitions still use the target width rather than the host PE's scalar field width.

The PE views expose `TypeDefinition.DeclaredLayout` and `FieldDefinition.DeclaredOffset` separately from calculated layout. A sequential field usually has no declared offset. Assembly snapshots report the selected layout target, struct alignment and size, and field offsets and sizes; unsupported struct layouts retain an explicit diagnostic.

`InlineArrayInfo.Read` decodes InlineArrayAttribute from the PE and retains the repeated
field, length, and substituted element type. Numeric inline arrays, including Roslyn's
generated auto-layout buffers, use the element's stride, packed alignment, and `length * elementSize`
without extra inter-element padding. The single metadata field describes element zero.
Reference inline arrays classify as containing managed references and receive their
frame/GC storage from the backend. See the [layout snapshot](../../Tests/XuiNet/DotNet/Snapshots/Buffers.md).
