using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdarga(Instruction.Variable instruction, int offset)
    {
        var argument = GetArgument(instruction.Index, instruction.Code, offset);
        if (!argument.UsesGc || argument.Identity is not { IsValueType: true } identity || argument.IsByReference)
            throw new NotSupportedException("Only by-value GC struct argument addresses are implemented.");
        writer.LocalGet(new LocalIndex(instruction.Index));
        stack.Push(new MsilStackEntry(argument.TypeCode, identity, true) { Location = new(method.Storage.Shape(identity), 0) });
    }
}
