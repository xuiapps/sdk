using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBle_un_s(int offset) =>
        EmitBranchComparison(OpCode.Ble_un_s, offset, ComparisonKind.LessThanOrEqual, true);
}
