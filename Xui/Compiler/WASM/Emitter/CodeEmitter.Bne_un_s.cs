using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBne_un_s(int offset) =>
        EmitBranchComparison(OpCode.Bne_un_s, offset, ComparisonKind.NotEqual, true);
}
