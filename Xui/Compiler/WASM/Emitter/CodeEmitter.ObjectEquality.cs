using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void CompareObjects(TypeIdentity type, LocalIndex left, LocalIndex right)
    {
        var storage = method.Storage;
        var targets = storage.EqualityTargets(type);
        writer.LocalGet(left).RefIsNull().If();
        writer.LocalGet(right).RefIsNull().LocalSet(method.GcI32).Else();
        writer.LocalGet(right).RefIsNull().If().I32Const(0).LocalSet(method.GcI32).Else();
        foreach (var (receiver, implementation) in targets)
        {
            writer.LocalGet(left).RefCast(storage.ClassRoot).StructGet(storage.ClassRoot, new(0))
                .GlobalGet(storage.Classes.Resolve(receiver).Descriptor).RefEq().If();
            writer.LocalGet(left).LocalGet(right).Call(methods[implementation].Function).LocalSet(method.GcI32).Else();
        }
        writer.LocalGet(left).LocalGet(right).RefEq().LocalSet(method.GcI32);
        foreach (var _ in targets) writer.End();
        writer.End().End().LocalGet(method.GcI32);
    }
}
