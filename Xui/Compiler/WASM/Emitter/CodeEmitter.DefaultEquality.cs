using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryDefaultEquality(Instruction.Token instruction, int offset)
    {
        var access = DefaultEquality.Read(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        if (access is null) return false;
        if (access.Operation == "get_Default")
        {
            writer.GlobalGet(method.Storage.DefaultComparers[access.Owner]);
            stack.Push(new(SignatureTypeCode.TypeHandle, access.Owner));
            return true;
        }
        if (access.Operation == "IndexOf") return TryComparerIndexOf(access, instruction, offset);
        if (access.Operation == "GetHashCode")
        {
            PopAssignable(method.Storage.Bind(access.Element), instruction.Code, offset);
            var hashLocal = method.EqualityTemporaries[method.Storage.Bind(access.Element)].Left;
            StorePrimitive(method.Storage.Bind(access.Element), hashLocal);
            var comparer = Pop(instruction.Code, offset);
            if (comparer.IsByReference || comparer.Identity is not null && comparer.Identity != access.Owner)
                throw new InvalidProgramException("Default hashing requires its declared comparer receiver.");
            writer.RefIsNull().If(); ThrowException(ExceptionKind.NullReferenceException); writer.End();
            EmitDefaultHash(access, hashLocal);
            stack.Push(new(SignatureTypeCode.Int32));
            return true;
        }
        var storage = method.Storage.Bind(access.Element);
        var (left, right) = method.EqualityTemporaries[storage];
        PopAssignable(storage, instruction.Code, offset); StorePrimitive(storage, right);
        PopAssignable(storage, instruction.Code, offset); StorePrimitive(storage, left);
        var receiver = Pop(instruction.Code, offset);
        if (receiver.IsByReference || receiver.Identity is not null && receiver.Identity != access.Owner)
            throw new InvalidProgramException("Default equality requires its declared comparer receiver.");
        writer.RefIsNull().If(); ThrowException(ExceptionKind.NullReferenceException); writer.End();
        CompareEquality(access, left, right);
        stack.Push(new(SignatureTypeCode.Int32));
        return true;
    }

    private void CompareEquality(DefaultEquality access, LocalIndex left, LocalIndex right)
    {
        var storage = method.Storage.Bind(access.Element);
        if (access.UsesArrayIdentity)
        {
            writer.LocalGet(left).LocalGet(right).RefEq();
            return;
        }
        if (access.Implementation is { } equals)
        {
            if (equals.DeclaringType.IsInterface)
            {
                writer.LocalGet(left).RefIsNull().If();
                writer.LocalGet(right).RefIsNull().LocalSet(method.GcI32).Else();
                writer.LocalGet(right).RefIsNull().If().I32Const(0).LocalSet(method.GcI32).Else();
                writer.LocalGet(left).LocalGet(right).LocalGet(left).RefCast(method.Storage.ClassRoot)
                    .StructGet(method.Storage.ClassRoot, new(0)).RefCast(method.Storage.DescriptorType)
                    .StructGet(method.Storage.DescriptorType, new((uint)Xui.Compiler.WASM.Runtime.DescriptorField.TableStart))
                    .I32Const((int)method.Storage.Classes.Slot(equals)).I32Add().CallIndirect(methods[equals].Type)
                    .LocalSet(method.GcI32).End().End().LocalGet(method.GcI32);
                return;
            }
            if (equals.DeclaringType.IsValueType)
            {
                if (storage.UsesGc) writer.LocalGet(left); else writer.RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.EqRef);
                writer.LocalGet(right);
                if (storage.UsesGc) writer.I32Const(method.Storage.ReferenceField(new(method.Storage.Shape(equals.DeclaringType), 0), storage));
                else writer.LocalGet(left);
            }
            else writer.LocalGet(left).LocalGet(right);
            writer.Call(methods[equals].Function);
        }
        else if (access.UsesObjectEquality) CompareObjects(access.Element.Identity!.Value, left, right);
        else ComparePrimitive(storage, left, right);
    }

    private void ComparePrimitive(LocalVariable storage, LocalIndex left, LocalIndex right)
    {
        var category = storage.StackType == SignatureTypeCode.IntPtr ? SignatureTypeCode.Int32 : storage.StackType;
        if (category == SignatureTypeCode.String) CompareStrings(left, right);
        else
        {
            writer.LocalGet(left).LocalGet(right);
            switch (category)
            {
                case SignatureTypeCode.Int32: writer.I32Eq(); break;
                case SignatureTypeCode.Int64: writer.I64Eq(); break;
                case SignatureTypeCode.Double:
                    // .NET Equals regards every NaN as equal; signed zeros compare equal too.
                    writer.F64Eq().LocalGet(left).LocalGet(left).F64Eq().I32Eqz()
                        .LocalGet(right).LocalGet(right).F64Eq().I32Eqz().I32And().I32Or();
                    break;
                default: throw new NotSupportedException($"Default equality category {category} is unsupported.");
            }
        }
    }

    private void CompareStrings(LocalIndex left, LocalIndex right)
    {
        writer.LocalGet(left).RefIsNull().If();
        writer.LocalGet(right).RefIsNull().LocalSet(method.GcI32).Else();
        writer.LocalGet(right).RefIsNull().If().I32Const(0).LocalSet(method.GcI32).Else();
        writer.LocalGet(left).LocalGet(right).Call(Runtime.StringEquals!.Value).LocalSet(method.GcI32);
        writer.End().End().LocalGet(method.GcI32);
    }
}
