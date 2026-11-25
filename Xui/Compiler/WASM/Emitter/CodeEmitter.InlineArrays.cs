using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private int ElementSize(SignatureType element)
    {
        var storage = method.Storage.Bind(element);
        if (StorageModel.IsReference(storage)) return 4; // frame-owned reference-table handle
        if (storage.Identity is { } identity) return method.Storage.StackLayout(identity).Size;
        return storage.TypeCode switch
        {
            SignatureTypeCode.Boolean or SignatureTypeCode.Byte or SignatureTypeCode.SByte => 1,
            SignatureTypeCode.Char or SignatureTypeCode.Int16 or SignatureTypeCode.UInt16 => 2,
            SignatureTypeCode.Int64 or SignatureTypeCode.UInt64 or SignatureTypeCode.Double => 8,
            _ => 4
        };
    }

    private bool TryInlineArrayAccess(Instruction.Token instruction, int offset)
    {
        var access = InlineArrayAccess.Read(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        if (access is null) return false;
        bool first = access.Operation == "InlineArrayFirstElementRef";
        bool span = access.Operation is "InlineArrayAsSpan" or "InlineArrayAsReadOnlySpan";
        if (!first)
        {
            Pop(SignatureTypeCode.Int32, instruction.Code, offset);
            writer.LocalSet(Frame.I32Scratch);
            writer.LocalGet(Frame.I32Scratch).I32Const(access.Layout.Length + (span ? 1 : 0)).I32LtU().I32Eqz().If();
            ThrowException(ExceptionKind.IndexOutOfRangeException);
            writer.End();
        }
        var address = Pop(instruction.Code, offset);
        if (address.Identity != access.Buffer || !address.IsByReference)
            throw new InvalidProgramException("Inline-array helper requires the declared buffer address.");
        if (span)
        {
            SaveAddress();
            CheckSavedAddress(method.AddressOwner, method.AddressSelector);
            FrameAddress(Frame.Values[offset]); writer.I32Load().LocalGet(method.AddressOwner).TableSet(Runtime.ObjectReferences!.Value);
            FrameAddress(Frame.Values[offset] + 4); writer.LocalGet(method.AddressSelector).I32Store();
            FrameAddress(Frame.Values[offset] + 8); writer.LocalGet(Frame.I32Scratch).I32Store();
            FrameAddress(Frame.Values[offset]);
            stack.Push(new(SignatureTypeCode.TypeHandle, access.Signature.Result.Identity));
        }
        else
        {
            var element = method.Storage.Bind(access.Layout.Element);
            SaveAddress();
            CheckSavedAddress(method.AddressOwner, method.AddressSelector);
            // A GC owner's selector counts flattened fields; a linear selector counts bytes.
            writer.LocalGet(method.AddressOwner).LocalGet(method.AddressOwner).RefIsNull()
                .If(method.Storage.AddressIndexType).LocalGet(method.AddressSelector);
            if (!first) writer.LocalGet(Frame.I32Scratch).I32Const(ElementSize(access.Layout.Element)).I32Mul().I32Add();
            if (StorageModel.IsReference(element)) writer.I32Load();
            writer.Else().LocalGet(method.AddressSelector);
            if (!first)
            {
                int stride = method.Storage.Shape(access.Buffer).Leaves.Count / access.Layout.Length;
                writer.LocalGet(Frame.I32Scratch).I32Const(stride).I32Mul().I32Add();
            }
            writer.End();
            stack.Push(new(element.TypeCode, element.Identity, true, element.Array));
        }
        return true;
    }

}
