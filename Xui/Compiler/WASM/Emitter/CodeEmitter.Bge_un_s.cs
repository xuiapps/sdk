using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBge_un_s(int offset) =>
        EmitBranchComparison(OpCode.Bge_un_s, offset, ComparisonKind.GreaterThanOrEqual, true);
}
