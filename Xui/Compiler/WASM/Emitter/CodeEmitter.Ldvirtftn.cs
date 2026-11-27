using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdvirtftn(Instruction.Token instruction, int offset)
    {
        var target = ResolveCall(source, method, instruction.Value, offset);
        var callee = methods[target];
        if (!callee.Signature.HasThis || target.DeclaringType.IsValueType)
            throw new NotSupportedException("ldvirtftn requires a class or interface instance method.");
        PopAssignable(callee.Signature.Parameters[0], instruction.Code, offset);
        CheckExceptionNull();
        var storage = method.Storage;
        writer.RefCast(storage.ClassRoot).StructGet(storage.ClassRoot, new(0))
            .RefCast(storage.DescriptorType).StructGet(storage.DescriptorType, new((uint)Xui.Compiler.WASM.Runtime.DescriptorField.TableStart))
            .I32Const((int)storage.Classes.Slot(target)).I32Add();
        stack.Push(new(SignatureTypeCode.IntPtr) { Method = target });
    }
}
