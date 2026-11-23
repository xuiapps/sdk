using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBgt_un_s(int offset) =>
        EmitBranchComparison(OpCode.Bgt_un_s, offset, ComparisonKind.GreaterThan, true);
}
