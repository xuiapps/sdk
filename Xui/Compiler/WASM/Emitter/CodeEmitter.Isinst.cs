using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private static void RequireTypeTest(MethodPlan method, XuiNet.DotNet.Module source, EntityHandle token)
    {
        method.Storage.ExceptionsEnabled = true;
        method.RequireGcTemporaries();
        var signature = XuiNet.DotNet.Signatures.SignatureReader.StorageType(source, token, method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        if (signature.Identity is { Arguments.Values.Length: 1 } contract &&
            XuiNet.DotNet.Intrinsics.ArrayInterface.Matches(new(contract.Arguments.Values[0]), contract)) method.Storage.ArrayIdentity.Enabled = true;
        if (signature.Code == SignatureTypeCode.Object) return;
        if (signature.Code == SignatureTypeCode.String) { method.Storage.StringObjects.Enable(method.Storage, source); return; }
        if (BoxingType.Read(source, token, method.Identity.DeclaringType.Arguments, method.Identity.Arguments) is { } box)
        {
            _ = method.Storage.Box(box);
            return;
        }
        if (signature.Identity is not { } target || !method.Storage.IsClass(target))
            throw new NotSupportedException("Reference type tests currently require a named class or interface.");
        if (XuiNet.DotNet.Intrinsics.InvariantCulture.Is(target) || FrameworkTypes.Is(target, "System", "IFormatProvider"))
            throw new NotSupportedException("Runtime casts of formatting providers are not implemented.");
        method.Storage.ArrayTypeTests.Add(target);
        method.RequireGc(target);
        if (XuiNet.DotNet.Intrinsics.DefaultEquality.IsInterface(target))
            method.RequireGc(XuiNet.DotNet.Intrinsics.DefaultEquality.InterfaceOwner(target));
    }

    private void EmitIsinst(Instruction.Token instruction, int offset) => EmitTypeTest(instruction, offset, false);

    private void EmitTypeTest(Instruction.Token instruction, int offset, bool throwing)
    {
        var testedStorage = XuiNet.DotNet.Signatures.SignatureReader.StorageType(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        if (testedStorage.Code == SignatureTypeCode.String) { EmitStringCast(instruction.Code, offset, throwing); return; }
        if (testedStorage.Code == SignatureTypeCode.Object)
        {
            if (!IsObjectReference(Pop(instruction.Code, offset))) throw new NotSupportedException("Object type tests require a managed object reference.");
            stack.Push(new(SignatureTypeCode.Object));
            return;
        }
        var testedBox = BoxingType.Read(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        if (testedBox is { } box)
        {
            if (!IsObjectReference(Pop(instruction.Code, offset))) throw new InvalidProgramException("Box type tests require an object reference.");
            // Preserve null separately; EmitBoxTest uses the GC scratch references.
            writer.LocalTee(method.GcOwner).RefIsNull().LocalSet(method.GcI32).LocalGet(method.GcOwner);
            EmitBoxTest(box, compatible: false);
            if (throwing)
            {
                writer.LocalTee(method.GcOwner).RefIsNull().LocalGet(method.GcI32).I32Eqz().I32And().If();
                ThrowException(XuiNet.DotNet.Exceptions.ExceptionKind.InvalidCastException);
                writer.End().LocalGet(method.GcOwner);
            }
            stack.Push(new(SignatureTypeCode.Object));
            return;
        }
        var target = Xui.Compiler.XuiNet.DotNet.Signatures.SignatureReader.Resolve(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments).Identity
            ?? throw new NotSupportedException("Type tests currently require same-module classes.");
        var value = Pop(instruction.Code, offset);
        if (!IsObjectReference(value))
            throw new NotSupportedException("Class type tests currently accept only same-module class references.");
        if (throwing) writer.LocalTee(method.GcSource);
        if (XuiNet.DotNet.Intrinsics.DefaultEquality.Is(target) || XuiNet.DotNet.Intrinsics.DefaultEquality.IsInterface(target))
        {
            bool contract = XuiNet.DotNet.Intrinsics.DefaultEquality.IsInterface(target);
            var comparer = contract ? XuiNet.DotNet.Intrinsics.DefaultEquality.InterfaceOwner(target) : target;
            writer.LocalSet(method.GcOwner).LocalGet(method.GcOwner).GlobalGet(method.Storage.DefaultComparers[comparer]).RefEq()
                .If(method.Storage.ExceptionPayloadBlock).LocalGet(method.GcOwner).Else();
            if (contract)
                writer.LocalGet(method.GcOwner).GlobalGet(method.Storage.Classes.Resolve(target).Descriptor).Call(method.Storage.InstanceTest);
            else writer.RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.EqRef);
            writer.End();
        }
        else if (XuiNet.DotNet.Intrinsics.ManagedType.Is(target))
        {
            writer.LocalSet(method.GcOwner).LocalGet(method.GcOwner).RefTest(method.Storage.DescriptorType)
                .If(method.Storage.ExceptionPayloadBlock).LocalGet(method.GcOwner).Else()
                .RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.EqRef).End();
        }
        else if (DelegateInfo.IsRuntimeBase(target))
        {
            writer.LocalSet(method.GcOwner).LocalGet(method.GcOwner).RefTest(method.Storage.DelegateType)
                .If(method.Storage.ExceptionPayloadBlock).LocalGet(method.GcOwner).Else()
                .RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.EqRef).End();
        }
        else if (XuiNet.DotNet.Exceptions.ExceptionTypes.Kind(target) is { } exceptionKind)
        {
            writer.LocalSet(method.GcOwner).LocalGet(method.GcOwner).RefTest(method.Storage.ExceptionType)
                .If(method.Storage.ExceptionPayloadBlock);
            EmitExceptionMatch(method.GcOwner, exceptionKind);
            writer.If(method.Storage.ExceptionPayloadBlock).LocalGet(method.GcOwner).Else()
                .RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.EqRef).End().Else()
                .RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.EqRef).End();
        }
        else
        {
            var arrays = method.Storage.ArrayImplementations(target).ToArray();
            if (arrays.Length == 0) writer.GlobalGet(method.Storage.Classes.Resolve(target).Descriptor).Call(method.Storage.InstanceTest);
            else
            {
                writer.LocalSet(method.GcOwner);
                foreach (var array in arrays)
                    writer.LocalGet(method.GcOwner).ExternConvertAny().I32Const((int)array.Type.Value).Call(method.Storage.ArrayIdentity.Test).If(method.Storage.ExceptionPayloadBlock).LocalGet(method.GcOwner).Else();
                writer.LocalGet(method.GcOwner).GlobalGet(method.Storage.Classes.Resolve(target).Descriptor).Call(method.Storage.InstanceTest);
                foreach (var _ in arrays) writer.End();
            }
        }
        if (throwing)
        {
            writer.LocalSet(method.GcOwner).LocalGet(method.GcSource).RefIsNull().I32Eqz()
                .LocalGet(method.GcOwner).RefIsNull().I32And().If();
            ThrowException(XuiNet.DotNet.Exceptions.ExceptionKind.InvalidCastException);
            writer.End().LocalGet(method.GcOwner);
        }
        stack.Push(new(SignatureTypeCode.TypeHandle, target));
    }
}
