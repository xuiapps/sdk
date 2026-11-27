using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryBitConversion(Instruction.Token instruction, int offset)
    {
        var conversion = BitConversion.Read(source, MetadataTokens.EntityHandle(instruction.Value));
        if (conversion is null) return false;
        Pop(method.Storage.Bind(conversion.Input).StackType, instruction.Code, offset);
        switch (conversion.Input.Code)
        {
            case SignatureTypeCode.Single: writer.F32DemoteF64().I32ReinterpretF32(); break;
            case SignatureTypeCode.Double: writer.I64ReinterpretF64(); break;
            case SignatureTypeCode.Int32:
            case SignatureTypeCode.UInt32: writer.F32ReinterpretI32().F64PromoteF32(); break;
            case SignatureTypeCode.Int64:
            case SignatureTypeCode.UInt64: writer.F64ReinterpretI64(); break;
            default: throw new NotSupportedException("Unsupported bit conversion.");
        }
        stack.Push(new(method.Storage.Bind(conversion.Result).StackType));
        return true;
    }
}
