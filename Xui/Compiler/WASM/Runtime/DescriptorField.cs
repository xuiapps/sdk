namespace Xui.Compiler.WASM.Runtime;

/// <summary>Immutable fields of the shared runtime type descriptor GC struct.</summary>
internal enum DescriptorField : uint
{
    Id,
    BaseType,
    TableStart,
    NameUtf16,
    VirtualSlotCount,
    FieldCount
}
