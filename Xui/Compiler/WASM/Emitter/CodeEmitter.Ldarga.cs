using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdarga(Instruction.Variable instruction, int offset)
    {
        var argument = GetArgument(instruction.Index, instruction.Code, offset);
        if (TryVariableScalarAddress(instruction.Index, true, argument)) return;
        if (TryVariableReferenceAddress(instruction.Index, true, argument)) return;
        if (argument.Identity is not { IsValueType: true } identity || argument.IsByReference)
            throw new NotSupportedException("Only by-value struct argument addresses are implemented.");
        writer.LocalGet(new LocalIndex(instruction.Index));
        ValueAddress(argument);
        stack.Push(new MsilStackEntry(argument.TypeCode, identity, true));
    }
}
