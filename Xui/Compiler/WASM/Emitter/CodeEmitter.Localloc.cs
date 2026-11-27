using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLocalloc(int offset)
    {
        var size = Pop(OpCode.Localloc, offset);
        if (size.IsByReference || size.Identity is not null || size.TypeCode is not (SignatureTypeCode.Int32 or SignatureTypeCode.IntPtr) || stack.Count != 0)
            throw new InvalidProgramException("localloc requires only an integer size on the evaluation stack.");
        // Dynamic blocks follow the fixed frame. Reclaim all of them together on
        // return/unwind; blocks allocated by repeated loop iterations stay live.
        writer.LocalSet(Frame.I32Scratch).LocalGet(Frame.I32Scratch).I32Eqz().If()
            .I32Const(0).LocalSet(Frame.AddressScratch).Else();
        writer.LocalGet(Frame.I32Scratch).I32Const(MemoryFrame.Capacity).I32GtU().If().Unreachable().End();
        writer.LocalGet(Frame.I32Scratch).I32Const(15).I32Add().I32Const(-16).I32And().LocalSet(Frame.I32Scratch);
        writer.GlobalGet(Runtime.StackPointer!.Value).LocalSet(Frame.AddressScratch)
            .LocalGet(Frame.AddressScratch).I32Const(MemoryFrame.Capacity).LocalGet(Frame.I32Scratch).I32Sub()
            .I32GtU().If().Unreachable().End();
        writer.LocalGet(Frame.AddressScratch).LocalGet(Frame.I32Scratch).I32Add().GlobalSet(Runtime.StackPointer.Value);
        if (initLocals)
            writer.LocalGet(Frame.AddressScratch).I32Const(0).LocalGet(Frame.I32Scratch).MemoryFill();
        writer.End().LocalGet(Frame.AddressScratch);
        stack.Push(new(SignatureTypeCode.IntPtr));
    }
}
