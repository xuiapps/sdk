using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitRet(int offset)
    {
        if (returnType != SignatureTypeCode.Void)
        {
            var result = method.Signature.Result;
            if (result.IsByReference && stack.Peek().Location is not null)
                throw new NotSupportedException("Returning inline GC value field addresses is not implemented.");
            PopAssignable(result, OpCode.Ret, offset);
            Narrow(result);
            if (result.IsLinearValue)
            {
                writer.LocalSet(Frame.AddressScratch).LocalGet(method.Signature.ResultAddress).LocalGet(Frame.AddressScratch);
                CopyStruct(result.Identity!.Value);
                writer.LocalGet(method.Signature.ResultAddress);
            }
        }
        if (stack.Count != 0)
            throw new InvalidProgramException($"{returnType} Ret at IL_{offset:x4} requires an empty evaluation stack after its result; found {stack.Count} value(s).");
        LeaveFrame();
        writer.Return();
    }
}
