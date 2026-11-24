using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Instructions;

namespace Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody;

public ref partial struct Writer
{
    /// <summary>Starts a void try_table that sends a matching tag payload to an enclosing label.</summary>
    /// <remarks>The catch label is relative to the enclosing labels, excluding try_table itself.</remarks>
    public readonly Writer TryCatch(TagIndex tag, LabelIndex label)
    {
        body.Instruction(OpCode.TryTable).Byte((byte)BlockTypeCode.Empty).U32(1).Byte((byte)CatchKind.Catch).U32(tag.Value).U32(label.Value);
        return this;
    }
    /// <summary>Captures a matching tag's payload and original exception reference at an enclosing label.</summary>
    public readonly Writer TryCatchRef(TagIndex tag, LabelIndex label)
    {
        body.Instruction(OpCode.TryTable).Byte((byte)BlockTypeCode.Empty).U32(1).Byte((byte)CatchKind.CatchRef).U32(tag.Value).U32(label.Value);
        return this;
    }
    /// <summary>Starts a void try_table that forwards any exception reference to a cleanup label.</summary>
    public readonly Writer TryCatchAllRef(LabelIndex label)
    {
        body.Instruction(OpCode.TryTable).Byte((byte)BlockTypeCode.Empty).U32(1).Byte((byte)CatchKind.CatchAllRef).U32(label.Value);
        return this;
    }
    /// <summary>Throws the payload on the stack with the given tag.</summary>
    public readonly Writer Throw(TagIndex tag) { body.Instruction(OpCode.Throw).U32(tag.Value); return this; }
    /// <summary>Rethrows the exception reference on the stack.</summary>
    public readonly Writer ThrowRef() { body.Instruction(OpCode.ThrowRef); return this; }
}
