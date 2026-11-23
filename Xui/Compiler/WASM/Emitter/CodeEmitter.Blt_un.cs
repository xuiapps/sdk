using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBlt_un(int offset) =>
        EmitBranchComparison(OpCode.Blt_un, offset, ComparisonKind.LessThan, true);
}
