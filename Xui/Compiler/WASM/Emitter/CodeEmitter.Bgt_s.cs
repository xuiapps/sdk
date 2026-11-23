using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBgt_s(int offset) =>
        EmitBranchComparison(OpCode.Bgt_s, offset, ComparisonKind.GreaterThan, false);
}
