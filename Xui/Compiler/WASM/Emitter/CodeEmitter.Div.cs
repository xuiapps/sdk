using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitDiv(int offset)
    {
        var right = Pop(OpCode.Div, offset);
        if (right.TypeCode != SignatureTypeCode.Double)
            throw new NotSupportedException($"Integer division is not implemented at IL_{offset:x4}.");
        Pop(SignatureTypeCode.Double, OpCode.Div, offset);
        writer.F64Div(); stack.Push(right);
    }
}
