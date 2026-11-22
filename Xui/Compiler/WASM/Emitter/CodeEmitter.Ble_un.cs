using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBle_un(int offset) =>
        EmitBranchComparison(OpCode.Ble_un, offset, ComparisonKind.LessThanOrEqual, true);
}
