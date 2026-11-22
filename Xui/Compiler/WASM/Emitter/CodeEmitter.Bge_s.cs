using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBge_s(int offset) =>
        EmitBranchComparison(OpCode.Bge_s, offset, ComparisonKind.GreaterThanOrEqual, false);
}
