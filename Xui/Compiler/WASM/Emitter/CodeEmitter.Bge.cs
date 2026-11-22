using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBge(int offset) =>
        EmitBranchComparison(OpCode.Bge, offset, ComparisonKind.GreaterThanOrEqual, false);
}
