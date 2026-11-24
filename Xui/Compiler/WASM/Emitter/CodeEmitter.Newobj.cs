using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitNewobj(Instruction.Token instruction, int offset)
    {
        if (TryExceptionMember(instruction, offset)) return;
        var callee = methods[ResolveCall(source, method.Identity.DeclaringType, instruction.Value, offset)];
        if (!callee.Signature.IsConstructor || !callee.Signature.HasThis) throw new InvalidProgramException("newobj requires an instance constructor.");
        var parameters = callee.Signature.Parameters;
        var temps = method.CallTemporaries[callee.Identity];
        for (int i = parameters.Length - 1; i >= 1; i--)
        {
            PopAssignable(parameters[i], instruction.Code, offset);
            Narrow(parameters[i]); writer.LocalSet(temps[i]);
        }
        var identity = parameters[0].Identity!.Value;
        var shape = method.Storage.Shape(identity);
        if (method.Storage.IsGc(identity)) writer.StructNewDefault(shape.Type).LocalSet(temps[0]);
        else
        {
            FrameAddress(Frame.Values[offset]);
            writer.LocalTee(temps[0]);
            ClearStruct(identity);
        }
        if (method.Storage.IsClass(identity))
            writer.LocalGet(temps[0]).RefCast(shape.Type).GlobalGet(method.Storage.Classes.Resolve(identity).Descriptor)
                .StructSet(shape.Type, new(0));
        writer.LocalGet(temps[0]);
        for (int i = 1; i < parameters.Length; i++) writer.LocalGet(temps[i]);
        writer.Call(callee.Function).LocalGet(temps[0]);
        stack.Push(new(System.Reflection.Metadata.SignatureTypeCode.TypeHandle, identity));
    }
}
