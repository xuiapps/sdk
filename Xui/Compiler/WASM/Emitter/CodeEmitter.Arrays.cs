using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private SignatureType ArrayElement(int token) => SignatureReader.StorageType(source, MetadataTokens.EntityHandle(token), method.Identity.DeclaringType.Arguments, method.Identity.Arguments);

    private void PopArrayIndex(OpCode code, int offset)
    {
        var index = Pop(code, offset);
        if (index.IsByReference || index.TypeCode is not (SignatureTypeCode.Int32 or SignatureTypeCode.IntPtr))
            throw new InvalidProgramException("Array index/length must be an integer.");
    }

    // Spills only the array/index, preserving operands lower on the WASM stack.
    private ArrayShape CheckedArray(OpCode code, int offset, SignatureType? explicitElement = null)
    {
        PopArrayIndex(code, offset);
        var value = Pop(code, offset);
        var shape = method.Storage.ArrayShape(value.Array ?? throw new InvalidProgramException("Expected a typed vector."));
        ArrayOperations.Validate(code, shape.Signature.Element, explicitElement);
        writer.LocalSet(method.ArrayIndex).LocalSet(method.ArrayReference);
        writer.LocalGet(method.ArrayReference);
        CheckExceptionNull();
        writer.Drop();
        writer.LocalGet(method.ArrayIndex).LocalGet(method.ArrayReference).RefCast(shape.Type).ArrayLen().I32LtU().I32Eqz().If();
        ThrowException(ExceptionKind.IndexOutOfRangeException);
        writer.End();
        return shape;
    }

    private void EmitArrayLoad(OpCode code, int offset, SignatureType? explicitElement = null, bool address = false)
    {
        var shape = CheckedArray(code, offset, explicitElement);
        if (address && !shape.StructElements)
        {
            // The array owns the storage; the selector is its already-checked index.
            writer.LocalGet(method.ArrayReference).LocalGet(method.ArrayIndex);
            stack.Push(new(shape.Element.TypeCode, shape.Element.Identity, true, shape.Element.Array));
            return;
        }
        writer.LocalGet(method.ArrayReference).RefCast(shape.Type).LocalGet(method.ArrayIndex).ArrayGet(shape.Type);
        if (shape.StructElements)
        {
            var identity = shape.Element.Identity!.Value;
            var elementShape = method.Storage.Shape(identity);
            if (address)
            {
                writer.I32Const(method.Storage.ReferenceField(new(elementShape, 0), shape.Element));
                stack.Push(new(SignatureTypeCode.TypeHandle, identity, true));
                return;
            }
            ReadInlineValue(identity, new(elementShape, 0), offset);
        }
        else
        {
            if (shape.Element.TypeCode == SignatureTypeCode.Single) writer.F64PromoteF32();
            if (code == OpCode.Ldelem_i1) writer.I32Extend8S();
            if (code == OpCode.Ldelem_u1) writer.I32Const(255).I32And();
            if (code == OpCode.Ldelem_i2) writer.I32Extend16S();
            if (code == OpCode.Ldelem_u2) writer.I32Const(65535).I32And();
        }
        stack.Push(new(shape.Element.StackType, shape.Element.Identity, array: shape.Element.Array));
    }

    private void EmitArrayStore(OpCode code, int offset, SignatureType? explicitElement = null)
    {
        var signature = stack.ElementAt(2).Array ?? throw new InvalidProgramException("Expected a typed vector.");
        var element = method.Storage.ArrayShape(signature).Element;
        PopAssignable(element, code, offset);
        writer.LocalSet(GcScratch(element));
        var shape = CheckedArray(code, offset, explicitElement);
        writer.LocalGet(method.ArrayReference).RefCast(shape.Type).LocalGet(method.ArrayIndex);
        if (shape.StructElements)
        {
            // Keep the element object: outstanding ldelema references must continue to alias it.
            writer.ArrayGet(shape.Type).LocalSet(method.GcOwner);
            var valueShape = method.Storage.Shape(element.Identity!.Value);
            StoreInlineValue(element.Identity.Value, element.UsesGc, new(valueShape, 0));
        }
        else
        {
            writer.LocalGet(GcScratch(element)); Narrow(element);
            writer.ArraySet(shape.Type);
        }
    }
}
