using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Instructions;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody;

public ref partial struct Writer
{
    /// <summary>Allocates a GC struct initialized from its field values.</summary>
    public readonly Writer StructNew(TypeIndex type) { body.Instruction(GcOpCode.StructNew).U32(type.Value); return this; }
    /// <summary>Allocates a GC struct initialized to zero/null.</summary>
    public readonly Writer StructNewDefault(TypeIndex type) { body.Instruction(GcOpCode.StructNewDefault).U32(type.Value); return this; }
    /// <summary>Reads an unpacked field of a typed GC struct reference.</summary>
    public readonly Writer StructGet(TypeIndex type, FieldIndex field) { body.Instruction(GcOpCode.StructGet).U32(type.Value).U32(field.Value); return this; }
    /// <summary>Writes a mutable field of a typed GC struct reference.</summary>
    public readonly Writer StructSet(TypeIndex type, FieldIndex field) { body.Instruction(GcOpCode.StructSet).U32(type.Value).U32(field.Value); return this; }
    /// <summary>Casts a GC reference to a non-null concrete struct type.</summary>
    public readonly Writer RefCast(TypeIndex type) { body.Instruction(GcOpCode.RefCast).Signed(type.Value); return this; }
    /// <summary>Compares two GC references by identity.</summary>
    public readonly Writer RefEq() { body.Instruction(OpCode.RefEq); return this; }
    /// <summary>Pushes null for a shorthand reference type.</summary>
    public readonly Writer RefNull(ValueTypeCode type) { body.Instruction(OpCode.RefNull).Signed((byte)type - 0x80); return this; }
}
