using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBgt_un(int offset) =>
        EmitBranchComparison(OpCode.Bgt_un, offset, ComparisonKind.GreaterThan, true);
}
