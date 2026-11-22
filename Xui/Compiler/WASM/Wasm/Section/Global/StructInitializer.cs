using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Instructions;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Wasm.Section.Global;

/// <summary>Writes a constant GC struct initializer; disposal allocates the struct and ends the expression.</summary>
public ref struct StructInitializer
{
    private readonly Section.Writer section;
    private readonly TypeIndex type;
    internal StructInitializer(Section.Writer section, TypeIndex type, GlobalIndex index)
    { this.section = section; this.type = type; Index = index; }
    /// <summary>The global being defined.</summary>
    public GlobalIndex Index { get; }
    /// <summary>Pushes an integer field value.</summary>
    public StructInitializer I32(int value) { section.Instruction(OpCode.I32Const).Signed(value); return this; }
    /// <summary>Pushes the value of a previously defined immutable global.</summary>
    public StructInitializer GlobalGet(GlobalIndex global) { section.Instruction(OpCode.GlobalGet).U32(global.Value); return this; }
    /// <summary>Pushes a nullable reference field value.</summary>
    public StructInitializer RefNull(ValueTypeCode type) { section.Instruction(OpCode.RefNull).Signed((byte)type - 0x80); return this; }
    /// <summary>Pushes a packed UTF-16 array created in the constant expression.</summary>
    public StructInitializer Utf16Array(TypeIndex arrayType, scoped ReadOnlySpan<char> value)
    {
        foreach (char codeUnit in value) section.Instruction(OpCode.I32Const).Signed(codeUnit);
        section.Instruction(GcOpCode.ArrayNewFixed).U32(arrayType.Value).U32((uint)value.Length);
        return this;
    }
    /// <summary>Completes this initializer without disposing the containing global section.</summary>
    public void Dispose() { section.Instruction(GcOpCode.StructNew).U32(type.Value).Instruction(OpCode.End); }
}
