using Xui.Compiler.XuiNet.DotNet.Op;
using System.Reflection.Metadata;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryReferenceComparison(OpCode code, int offset, bool invert)
    {
        if (stack.Count < 2) return false;
        var right = stack.Peek(); var left = stack.ElementAt(1);
        var storage = method.Storage;
        bool Reference(MsilStackEntry value) => !value.IsByReference &&
            (value.IsNull || value.TypeCode is SignatureTypeCode.String or SignatureTypeCode.Object ||
             (value.Identity is { } identity && storage.IsClass(identity)));
        if (!Reference(left) || !Reference(right)) return false;
        if (code == OpCode.Cgt_un && !right.IsNull) throw new NotSupportedException("Reference cgt.un requires null on the right.");
        _ = Pop(code, offset); _ = Pop(code, offset);
        if (left.IsNull && right.IsNull) writer.Drop().Drop().I32Const(1);
        else if (right.IsNull) writer.Drop().RefIsNull();
        else if (left.IsNull) writer.RefIsNull().LocalSet(method.GcI32).Drop().LocalGet(method.GcI32);
        else
        {
            if (left.TypeCode == SignatureTypeCode.String || right.TypeCode == SignatureTypeCode.String)
                throw new NotSupportedException("Only class identities and null references can be compared.");
            writer.RefEq();
        }
        if (invert) writer.I32Eqz();
        return true;
    }
}
