using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBlt_s(int offset) =>
        EmitBranchComparison(OpCode.Blt_s, offset, ComparisonKind.LessThan, false);
}
