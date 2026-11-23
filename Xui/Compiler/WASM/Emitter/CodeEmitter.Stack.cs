using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    /// <summary>Pushes the source MSIL type of a value emitted by a replacement instruction.</summary>
    public void Push(MsilStackEntry entry) => stack.Push(entry);

    private MsilStackEntry Pop(OpCode code, int offset)
    {
        if (!stack.TryPop(out var actual))
            throw new InvalidProgramException($"Evaluation stack underflow for {code} at IL_{offset:x4}.");
        return actual;
    }

    /// <summary>Consumes a source MSIL value and checks its type, including for replacement instructions.</summary>
    public void Pop(SignatureTypeCode expected, OpCode code, int offset)
    {
        var actual = Pop(code, offset);
        if (actual.IsNull && expected == SignatureTypeCode.String)
        {
            writer.Drop().RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.ExternRef); return;
        }
        if (actual.TypeCode != expected || actual.IsByReference || actual.Identity is not null)
            throw new InvalidProgramException($"Evaluation stack type mismatch for {code} at IL_{offset:x4}; expected {expected}, found {actual.TypeCode}.");
    }
}
