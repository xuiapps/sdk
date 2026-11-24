using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryReferenceObject(Instruction.Token instruction, int offset)
    {
        var type = method.Storage.Bind(SignatureReader.Resolve(source, MetadataTokens.EntityHandle(instruction.Value),
            method.Identity.DeclaringType.Arguments, method.Identity.Arguments));
        if (!StorageModel.IsReference(type)) return false;
        var expected = type with { IsByReference = true };
        if (instruction.Code == OpCode.Cpobj)
        {
            PopAssignable(expected, instruction.Code, offset);
            writer.Call(method.Storage.ReferenceAccessor(type, false));
            PopAssignable(expected, instruction.Code, offset);
            writer.Call(method.Storage.ReferenceAccessor(type, true));
        }
        else if (instruction.Code == OpCode.Initobj)
        {
            PopAssignable(expected, instruction.Code, offset);
            writer.RefNull(type.WasmType).Call(method.Storage.ReferenceAccessor(type, true));
        }
        else
        {
            var address = instruction.Code == OpCode.Stobj ? stack.ElementAt(1) : stack.Peek();
            if (address.Identity != type.Identity || address.TypeCode != type.TypeCode || address.Array != type.Array)
                throw new InvalidProgramException("Object operation requires a matching managed address.");
            EmitIndirect(instruction.Code == OpCode.Stobj ? OpCode.Stind_ref : OpCode.Ldind_ref, offset, instruction.Code == OpCode.Stobj);
        }
        return true;
    }
}
