using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Exceptions;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    // Arrays retain their GC identity. Only the selected call arm knows their concrete shape;
    // ordinary class implementations continue through the interface dispatch table.
    private void TagArray(ArrayShape shape)
    {
        if (!method.Storage.ArrayIdentity.Enabled) return;
        writer.LocalTee(method.ArrayReference).ExternConvertAny().I32Const((int)shape.Type.Value)
            .Call(method.Storage.ArrayIdentity.Register).LocalGet(method.ArrayReference).RefCast(shape.Type);
    }

    private void EmitArrayCollectionCopy(ArrayShape array, LocalIndex[] arguments)
    {
        writer.LocalGet(arguments[0]).LocalSet(method.GcSource);
        writer.LocalGet(arguments[1]).LocalSet(method.GcOwner);
        writer.LocalGet(arguments[2]).LocalSet(method.CopyDestinationIndex);
        writer.I32Const(0).LocalSet(method.CopySourceIndex);
        writer.LocalGet(method.GcSource).RefCast(array.Type).ArrayLen().LocalSet(method.CopyLength);
        CheckCopyNull(method.GcOwner, "destinationArray");
        CheckCopyNegative(method.CopyDestinationIndex, "destinationIndex");
        CheckCopyRange(method.GcOwner, method.CopyDestinationIndex, array);
        if (array.StructElements) CopyStructElements(array);
        else writer.LocalGet(method.GcOwner).RefCast(array.Type).LocalGet(method.CopyDestinationIndex)
            .LocalGet(method.GcSource).RefCast(array.Type).I32Const(0).LocalGet(method.CopyLength).ArrayCopy(array.Type, array.Type);
    }

    private int EmitArrayInterfaceArms(MethodIdentity target, LocalIndex[] arguments, int offset)
    {
        var operation = ArrayInterface.Operation(target);
        if (operation is null) return 0;
        int arms = 0;
        foreach (var array in method.Storage.ArrayImplementations(target.DeclaringType))
        {
            writer.LocalGet(arguments[0]).ExternConvertAny().I32Const((int)array.Type.Value).Call(method.Storage.ArrayIdentity.Test).If(method.Storage.ArrayInterfaceBlock(target));
            if (operation == "Count") writer.LocalGet(arguments[0]).RefCast(array.Type).ArrayLen();
            else if (operation == "IsReadOnly") writer.I32Const(1);
            else if (operation == "FixedSize") ThrowException(ExceptionKind.NotSupportedException);
            else if (operation == "CopyTo") EmitArrayCollectionCopy(array, arguments);
            else if (operation == "GetEnumerator")
            {
                var constructor = ArrayInterface.EnumeratorConstructor(target)!.Value;
                var shape = method.Storage.Shape(constructor.DeclaringType);
                var temps = method.CallTemporaries[constructor];
                writer.StructNewDefault(shape.Type).LocalSet(temps[0]);
                writer.LocalGet(temps[0]).RefCast(shape.Type).GlobalGet(method.Storage.Classes.Resolve(constructor.DeclaringType).Descriptor).StructSet(shape.Type, new(0));
                writer.LocalGet(temps[0]).LocalGet(arguments[0]).LocalGet(arguments[0]).RefCast(array.Type).ArrayLen()
                    .Call(methods[constructor].Function).LocalGet(temps[0]);
            }
            else
            {
                writer.LocalGet(arguments[1]).LocalGet(arguments[0]).RefCast(array.Type).ArrayLen().I32LtU().I32Eqz().If();
                ThrowArgumentException(ExceptionKind.ArgumentOutOfRangeException, "index"); writer.End();
                writer.LocalGet(arguments[0]).RefCast(array.Type).LocalGet(arguments[1]).ArrayGet(array.Type);
                if (array.StructElements)
                {
                    var identity = array.Element.Identity!.Value;
                    ReadInlineValue(identity, new(method.Storage.Shape(identity), 0), offset);
                }
            }
            writer.Else(); arms++;
        }
        return arms;
    }
}
