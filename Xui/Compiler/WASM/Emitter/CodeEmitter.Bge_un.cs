using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBge_un(int offset) =>
        EmitBranchComparison(OpCode.Bge_un, offset, ComparisonKind.GreaterThanOrEqual, true);
}
