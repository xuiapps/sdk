using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBlt_un_s(int offset) =>
        EmitBranchComparison(OpCode.Blt_un_s, offset, ComparisonKind.LessThan, true);
}
