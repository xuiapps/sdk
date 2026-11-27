using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryStaticSpan(Instruction.Token instruction, int offset)
    {
        if (!method.StaticSpans.TryGetValue(offset, out var span)) return false;
        if (span.ArrayInitializer) { EmitArrayInitializer(instruction, offset, span); return true; }
        if (span.PointerConstructor) { Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.Drop(); }
        if (Pop(instruction.Code, offset).FieldToken != span.Field)
            throw new InvalidProgramException("CreateSpan requires its paired static data token.");
        writer.LocalSet(Frame.AddressScratch);
        FrameAddress(Frame.Values[offset]); writer.I32Load().RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.EqRef).TableSet(Runtime.ObjectReferences!.Value);
        FrameAddress(Frame.Values[offset] + 4); writer.LocalGet(Frame.AddressScratch).I32Store();
        FrameAddress(Frame.Values[offset] + 8); writer.I32Const(span.ElementCount).I32Store();
        FrameAddress(Frame.Values[offset]);
        stack.Push(new(SignatureTypeCode.TypeHandle, span.Result.Identity));
        return true;
    }
}
