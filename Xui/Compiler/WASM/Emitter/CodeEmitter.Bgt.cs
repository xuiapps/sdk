using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBgt(int offset) =>
        EmitBranchComparison(OpCode.Bgt, offset, ComparisonKind.GreaterThan, false);
}
