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
        if (operation.HasRoundingMode)
        {
            Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.LocalSet(method.GcI32);
            Pop(SignatureTypeCode.Double, instruction.Code, offset);
            if (operation.IsSingle) writer.F32DemoteF64().F64PromoteF32();
            writer.LocalSet(method.GcF64);
            writer.LocalGet(method.GcI32).I32Const(4).I32GtU().If();
            ThrowArgumentException(XuiNet.DotNet.Exceptions.ExceptionKind.ArgumentException, "mode"); writer.End();
            for (int mode = 0; mode <= 4; mode++)
            {
                writer.LocalGet(method.GcI32).I32Const(mode).I32Eq().If();
                writer.LocalGet(method.GcF64);
                switch (mode)
                {
                    case 0: writer.F64Nearest(); break;
                    case 1:
                        // Round the fractional magnitude, then restore the sign (including negative zero).
                        // Avoid value + 0.5, which can spuriously round the predecessor of a midpoint.
                        writer.F64Abs().F64Trunc()
                            .LocalGet(method.GcF64).F64Abs().LocalGet(method.GcF64).F64Abs().F64Trunc().F64Sub()
                            .F64Const(.5).F64Ge().F64ConvertI32S().F64Add()
                            .LocalGet(method.GcF64).F64CopySign();
                        break;
                    case 2: writer.F64Trunc(); break;
                    case 3: writer.F64Floor(); break;
                    case 4: writer.F64Ceil(); break;
                }
                writer.LocalSet(method.GcF64).End();
            }
            writer.LocalGet(method.GcF64);
            stack.Push(new(SignatureTypeCode.Double));
            return true;
        }
        Pop(SignatureTypeCode.Double, instruction.Code, offset);
        if (operation.Operation is "Sin" or "Cos" or "Log" or "Exp")
        {
            if (operation.IsSingle) writer.F32DemoteF64().F64PromoteF32();
            writer.Call(operation.Operation switch { "Sin" => method.Storage.MathImports.Sin, "Cos" => method.Storage.MathImports.Cos, "Log" => method.Storage.MathImports.Log, _ => method.Storage.MathImports.Exp });
            if (operation.IsSingle) writer.F32DemoteF64().F64PromoteF32();
            stack.Push(new(SignatureTypeCode.Double));
            return true;
        }
        if (operation.Operation is "Floor" or "Ceiling" or "Truncate" or "Round" or "Abs")
        {
            if (operation.IsSingle) writer.F32DemoteF64();
            switch (operation.Operation, operation.IsSingle)
            {
                case ("Abs", true): writer.F32Abs(); break;
                case ("Abs", false): writer.F64Abs(); break;
                case ("Floor", true): writer.F32Floor(); break;
                case ("Floor", false): writer.F64Floor(); break;
                case ("Ceiling", true): writer.F32Ceil(); break;
                case ("Ceiling", false): writer.F64Ceil(); break;
                case ("Truncate", true): writer.F32Trunc(); break;
                case ("Truncate", false): writer.F64Trunc(); break;
                case ("Round", true): writer.F32Nearest(); break;
                case ("Round", false): writer.F64Nearest(); break;
                default: throw new NotSupportedException($"Math operation {operation.Operation} is not implemented.");
            }
            if (operation.IsSingle) writer.F64PromoteF32();
            stack.Push(new(SignatureTypeCode.Double));
            return true;
        }
        if (operation.Operation == "Sqrt")
        {
            if (operation.IsSingle) writer.F32DemoteF64().F32Sqrt().F64PromoteF32();
            else writer.F64Sqrt();
            stack.Push(new(SignatureTypeCode.Double));
            return true;
        }
        Pop(SignatureTypeCode.Double, instruction.Code, offset);
        if (operation.IsSingle && operation.Operation is "Min" or "Max" or "CopySign")
        {
            writer.F32DemoteF64().LocalSet(method.NativeFloatTemporary).F32DemoteF64().LocalGet(method.NativeFloatTemporary);
            if (operation.Operation == "Min") writer.F32Min();
            else if (operation.Operation == "Max") writer.F32Max();
            else writer.F32CopySign();
            writer.F64PromoteF32();
            stack.Push(new(SignatureTypeCode.Double));
            return true;
        }
        if (operation.IsSingle)
            writer.F32DemoteF64().LocalSet(method.NativeFloatTemporary).F32DemoteF64().F64PromoteF32()
                .LocalGet(method.NativeFloatTemporary).F64PromoteF32();
        if (operation.Operation == "CopySign") writer.F64CopySign();
        else if (operation.Operation == "Min") writer.F64Min();
        else if (operation.Operation == "Max") writer.F64Max();
        else if (operation.Operation == "Pow") writer.Call(method.Storage.MathImports.Pow);
        else throw new NotSupportedException($"Math operation {operation.Operation} is not implemented.");
        if (operation.IsSingle) writer.F32DemoteF64().F64PromoteF32();
        stack.Push(new(SignatureTypeCode.Double));
        return true;
    }
}
