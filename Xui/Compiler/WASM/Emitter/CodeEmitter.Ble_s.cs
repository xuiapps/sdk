using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBle_s(int offset) =>
        EmitBranchComparison(OpCode.Ble_s, offset, ComparisonKind.LessThanOrEqual, false);
}
