using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdarg_2(int offset) => LoadArgument(2, OpCode.Ldarg_2, offset);
}
