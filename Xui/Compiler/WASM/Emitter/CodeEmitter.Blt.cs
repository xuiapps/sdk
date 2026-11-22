using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBlt(int offset) =>
        EmitBranchComparison(OpCode.Blt, offset, ComparisonKind.LessThan, false);
}
