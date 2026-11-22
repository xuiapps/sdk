using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBle(int offset) =>
        EmitBranchComparison(OpCode.Ble, offset, ComparisonKind.LessThanOrEqual, false);
}
