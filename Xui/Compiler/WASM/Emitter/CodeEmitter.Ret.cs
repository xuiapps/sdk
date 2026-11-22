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
            PopAssignable(result, OpCode.Ret, offset);
            Narrow(result);
        }
        if (stack.Count != 0)
            throw new InvalidProgramException($"{returnType} Ret at IL_{offset:x4} requires an empty evaluation stack after its result; found {stack.Count} value(s).");
        LeaveFrame();
        writer.Return();
    }
}
