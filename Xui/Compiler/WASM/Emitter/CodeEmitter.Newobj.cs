using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitNewobj(Instruction.Token instruction, int offset)
    {
        if (TryStaticSpan(instruction, offset)) return;
        if (TryInterpolation(instruction, offset)) return;
        if (TryDelegateConstructor(instruction, offset)) return;
        if (TryExceptionMember(instruction, offset)) return;
        var callee = methods[ResolveCall(source, method.Identity, instruction.Value, offset)];
        if (!callee.Signature.IsConstructor || !callee.Signature.HasThis) throw new InvalidProgramException("newobj requires an instance constructor.");
        var parameters = callee.Signature.Parameters;
        var temps = method.CallTemporaries[callee.Identity];
        for (int i = parameters.Length - 1; i >= 1; i--)
        {
            PopAssignable(parameters[i], instruction.Code, offset);
            if (parameters[i].IsByReference) writer.LocalSet(method.CallReferenceTemporaries[callee.Identity][i]);
            Narrow(parameters[i]); writer.LocalSet(temps[i]);
        }
        var identity = parameters[0].Identity!.Value;
        var shape = method.Storage.Shape(identity);
        if (method.Storage.IsGc(identity)) writer.StructNewDefault(shape.Type).LocalSet(temps[0]);
        else
        {
            FrameAddress(Frame.Values[offset]);
            ClearStruct(identity);
            writer.RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.EqRef).LocalSet(temps[0]);
        }
        if (method.Storage.IsClass(identity))
            writer.LocalGet(temps[0]).RefCast(shape.Type).GlobalGet(method.Storage.Classes.Resolve(identity).Descriptor)
                .StructSet(shape.Type, new(0));
        if (parameters[0].IsByReference)
        {
            if (method.Storage.IsGc(identity)) writer.I32Const(method.Storage.ReferenceField(new(shape, 0), parameters[0]));
            else FrameAddress(Frame.Values[offset]);
            writer.LocalSet(method.CallReferenceTemporaries[callee.Identity][0]);
        }
        writer.LocalGet(temps[0]);
        for (int i = 1; i < parameters.Length; i++) writer.LocalGet(temps[i]);
        for (int i = 0; i < parameters.Length; i++)
            if (parameters[i].IsByReference) writer.LocalGet(method.CallReferenceTemporaries[callee.Identity][i]);
        writer.Call(callee.Function);
        if (method.Storage.IsGc(identity)) writer.LocalGet(temps[0]); else FrameAddress(Frame.Values[offset]);
        stack.Push(new(System.Reflection.Metadata.SignatureTypeCode.TypeHandle, identity));
    }
}
