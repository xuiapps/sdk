using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryMath(Instruction.Token instruction, int offset)
    {
        var operation = MathMethod.Read(source, MetadataTokens.EntityHandle(instruction.Value));
        if (operation is null) return false;
        Pop(SignatureTypeCode.Double, instruction.Code, offset);
        if (operation.Operation == "Sqrt")
        {
            if (operation.IsSingle) writer.F32DemoteF64().F32Sqrt().F64PromoteF32();
            else writer.F64Sqrt();
            stack.Push(new(SignatureTypeCode.Double));
            return true;
        }
        Pop(SignatureTypeCode.Double, instruction.Code, offset);
        if (operation.IsSingle)
            writer.F32DemoteF64().LocalSet(method.NativeFloatTemporary).F32DemoteF64().F64PromoteF32()
                .LocalGet(method.NativeFloatTemporary).F64PromoteF32();
        writer.Call(method.Storage.MathImports.Pow);
        if (operation.IsSingle) writer.F32DemoteF64().F64PromoteF32();
        stack.Push(new(SignatureTypeCode.Double));
        return true;
    }
}
