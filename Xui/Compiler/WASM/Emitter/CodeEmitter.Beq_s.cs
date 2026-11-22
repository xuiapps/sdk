using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBeq_s(int offset) =>
        EmitBranchComparison(OpCode.Beq_s, offset, ComparisonKind.Equal, false);
}
