using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitConv_i(int offset)
    {
        // Native integers have 32-bit storage on this WASM target.
        ConvertToInt32(OpCode.Conv_i, offset);
        stack.Pop();
        stack.Push(new MsilStackEntry(SignatureTypeCode.IntPtr));
    }
}
