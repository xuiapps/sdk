using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBeq(int offset) =>
        EmitBranchComparison(OpCode.Beq, offset, ComparisonKind.Equal, false);
}
