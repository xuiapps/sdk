using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryScalarObject(Instruction.Token instruction, int offset)
    {
        var declared = SignatureReader.StorageType(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        var type = method.Storage.Bind(declared);
        if (!type.IsScalar) return false;
        switch (instruction.Code)
        {
            case OpCode.Ldobj:
                ReadScalarAddress(Pop(instruction.Code, offset), type);
                stack.Push(new(type.StackType));
                break;
            case OpCode.Stobj:
                PopAssignable(type, instruction.Code, offset);
                writer.LocalSet(GcScratch(type));
                WriteScalarAddress(Pop(instruction.Code, offset), type);
                break;
            case OpCode.Cpobj:
                ReadScalarAddress(Pop(instruction.Code, offset), type);
                writer.LocalSet(GcScratch(type));
                WriteScalarAddress(Pop(instruction.Code, offset), type);
                break;
            case OpCode.Initobj:
                if (type.StackType == SignatureTypeCode.Double) writer.F64Const(0);
                else if (type.StackType == SignatureTypeCode.Int64) writer.I64Const(0);
                else writer.I32Const(0);
                writer.LocalSet(GcScratch(type));
                WriteScalarAddress(Pop(instruction.Code, offset), type);
                break;
            default: throw new InvalidProgramException($"Unexpected scalar object operation {instruction.Code}.");
        }
        return true;
    }

    private static void CheckScalarAddress(MsilStackEntry address, LocalVariable type)
    {
        if (!address.IsByReference || address.Identity is not null || address.TypeCode != type.TypeCode)
            throw new InvalidProgramException($"Expected managed address of {type.TypeCode}.");
    }

    private void ReadScalarAddress(MsilStackEntry address, LocalVariable type)
    {
        CheckScalarAddress(address, type);
        writer.Call(method.Storage.ReferenceAccessor(type, false));
        if (type.TypeCode == SignatureTypeCode.Single) writer.F64PromoteF32();
    }

    private void WriteScalarAddress(MsilStackEntry address, LocalVariable type)
    {
        CheckScalarAddress(address, type);
        writer.LocalGet(GcScratch(type)); Narrow(type);
        writer.Call(method.Storage.ReferenceAccessor(type, true));
    }
}
