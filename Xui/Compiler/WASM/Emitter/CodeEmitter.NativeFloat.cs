using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryNativeFloat(Instruction.Token instruction, int offset)
    {
        if (instruction.Code != OpCode.Call || !NativeFloat.TryMethod(source, MetadataTokens.EntityHandle(instruction.Value), out var operation, out var signature)) return false;
        for (int i = signature.Parameters.Length - 1; i >= 0; i--)
            Pop(method.Storage.Bind(signature.Parameters[i]).StackType, instruction.Code, offset);
        if (signature.Parameters.Length == 2)
        {
            writer.F32DemoteF64().LocalSet(method.NativeFloatTemporary).F32DemoteF64().LocalGet(method.NativeFloatTemporary);
            switch (operation)
            {
                case "op_Addition": writer.F32Add(); break;
                case "op_Subtraction": writer.F32Sub(); break;
                case "op_Multiply": writer.F32Mul(); break;
                case "op_Division": writer.F32Div(); break;
                case "op_Equality": writer.F32Eq(); break;
                case "op_Inequality": writer.F32Ne(); break;
                case "op_LessThan": writer.F32Lt(); break;
                case "op_LessThanOrEqual": writer.F32Le(); break;
                case "op_GreaterThan": writer.F32Gt(); break;
                case "op_GreaterThanOrEqual": writer.F32Ge(); break;
                default: throw new NotSupportedException($"NFloat operation {operation} is not implemented.");
            }
            if (signature.Result.Code != SignatureTypeCode.Boolean) writer.F64PromoteF32();
        }
        else if (signature.Parameters.Length == 0)
        {
            writer.F64Const(operation switch {
                "get_PositiveInfinity" => double.PositiveInfinity,
                "get_NegativeInfinity" => double.NegativeInfinity,
                "get_NaN" => double.NaN,
                _ => throw new NotSupportedException($"NFloat constant {operation} is not implemented.")
            });
        }
        else if (signature.Result.Identity is { } result && NativeFloat.Is(result))
        {
            if (signature.Parameters[0].Code == SignatureTypeCode.Int32) writer.F32ConvertI32S();
            else writer.F32DemoteF64();
            writer.F64PromoteF32();
        }
        // NFloat values already carry f32 rounding; conversion to double is an exact promotion.
        stack.Push(new(method.Storage.Bind(signature.Result).StackType));
        return true;
    }
}
