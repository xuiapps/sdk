using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Op;
using Model = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Applies supported MSIL stack effects using signatures, independently of any backend.</summary>
internal sealed class MethodAnalysis(Model.Module module, Model.MethodBody body, Signature signature, Model.TypeIdentity context, InstructionGraph graph, GenericArguments? methodArguments = null)
{
    private readonly SignatureType[] locals = SignatureReader.Locals(module, body.LocalSignature, context.Arguments, methodArguments).ToArray();
    private readonly Dictionary<int, Signature> calls = new();
    private readonly IReadOnlyDictionary<int, Model.Intrinsics.InterpolatedStringHandler> interpolationCalls = InterpolationCalls.Read(module, body, graph, context, methodArguments);
    private readonly IReadOnlyDictionary<int, InvariantFormattingCall> invariantFormatting = InvariantFormattingCalls.Read(module, body, graph);

    public void Apply(InstructionNode node)
    {
        var reader = body.Instructions;
        reader.Bytes.Offset = node.Offset;
        var instruction = reader.Read();
        var stack = new StackCursor(node);
        switch (instruction.Code)
        {
            case OpCode.Ldtoken:
                var tokenType = Model.Intrinsics.ManagedType.Token(module, MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value), context.Arguments, methodArguments);
                stack.Push(new(StackType.RuntimeTypeHandle, TypeToken: tokenType));
                break;
            case OpCode.Ldsfld:
                var zeroType = Model.Intrinsics.TimeSpanZero.Read(module, MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value), context)
                    ?? throw new NotSupportedException("Static field stack analysis is not implemented except TimeSpan.Zero.");
                stack.Push(StackEntry.From(new SignatureType(SignatureTypeCode.TypeHandle, zeroType)));
                break;
            case OpCode.Newarr:
                ArrayIndex(ref stack);
                var element = SignatureReader.StorageType(module, MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value), context.Arguments, methodArguments);
                stack.Push(StackEntry.From(SignatureType.Vector(element)));
                break;
            case OpCode.Ldlen:
                var vector = stack.Pop();
                if (vector.Type is not (StackType.ArrayReference or StackType.Null)) throw stack.Error("Expected an array");
                stack.Push(StackType.NativeInt);
                break;
            case >= OpCode.Ldelema and <= OpCode.Stelem:
                bool storeElement = node.Code is >= OpCode.Stelem_i and <= OpCode.Stelem_ref or OpCode.Stelem;
                StackEntry? stored = storeElement ? stack.Pop() : null;
                ArrayIndex(ref stack);
                var arrayEntry = stack.Pop();
                var arrayElement = arrayEntry.DeclaredType?.Array?.Element
                    ?? throw stack.Error("Array element operations require an exact vector type");
                var explicitElement = node.Code is OpCode.Ldelema or OpCode.Ldelem or OpCode.Stelem
                    ? SignatureReader.StorageType(module, MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value), context.Arguments, methodArguments) : (SignatureType?)null;
                ArrayOperations.Validate(node.Code, arrayElement, explicitElement);
                if (stored is { } item) { stack.Push(item); stack.Store(StackEntry.From(arrayElement)); }
                else stack.Push(StackEntry.From(node.Code == OpCode.Ldelema ? arrayElement.ByReference() : arrayElement));
                break;
            case OpCode.Constrained:
                _ = instruction.ReadToken(ref reader);
                if (!interpolationCalls.ContainsKey(reader.Offset)) throw new NotSupportedException("Constrained dispatch is only implemented for interpolation ToString.");
                break;
            case OpCode.Nop: break;
            case OpCode.Ldind_i1:
            case OpCode.Ldind_u1:
            case OpCode.Ldind_i2:
            case OpCode.Ldind_u2:
            case OpCode.Ldind_i4:
            case OpCode.Stind_i4:
            case OpCode.Ldind_u4:
            case OpCode.Ldind_i8:
            case OpCode.Ldind_i:
            case OpCode.Ldind_r4:
            case OpCode.Ldind_r8:
            case OpCode.Ldind_ref:
            case OpCode.Stind_i1:
            case OpCode.Stind_i2:
            case OpCode.Stind_i8:
            case OpCode.Stind_i:
            case OpCode.Stind_r4:
            case OpCode.Stind_r8:
            case OpCode.Stind_ref:
                bool storeIndirect = instruction.Code is >= OpCode.Stind_ref and <= OpCode.Stind_r8 or OpCode.Stind_i;
                var indirectValue = storeIndirect ? stack.Pop() : (StackEntry?)null;
                var indirectAddress = stack.Pop();
                if (indirectAddress.Type != StackType.ManagedPointer || indirectAddress.DeclaredType is not { } indirectType)
                    throw stack.Error("Indirect operation requires a managed address");
                indirectType = indirectType with { IsByReference = false, IsReadOnlyByReference = false };
                IndirectOperations.Validate(node.Code, indirectType);
                if (indirectValue is { } storedIndirect) { stack.Push(storedIndirect); stack.Store(StackEntry.From(indirectType)); }
                else stack.Push(StackEntry.From(indirectType));
                break;
            case >= OpCode.Ldc_i4_m1 and <= OpCode.Ldc_i4_8:
            case OpCode.Ldc_i4:
            case OpCode.Ldc_i4_s: stack.Push(StackType.Int32); break;
            case OpCode.Ldc_i8: stack.Push(StackType.Int64); break;
            case OpCode.Ldc_r4:
            case OpCode.Ldc_r8: stack.Push(StackType.Float); break;
            case OpCode.Ldnull: stack.Push(StackType.Null); break;
            case OpCode.Ldstr: stack.Push(StackType.String); break;
            case OpCode.Pop: stack.Pop(); break;
            case OpCode.Dup:
                var duplicated = stack.Pop(); // The original top is read, so it belongs in a future block's parameters.
                stack.Push(duplicated); stack.Push(duplicated);
                break;
            case >= OpCode.Ldarg_0 and <= OpCode.Ldarg_3:
                stack.Push(Slot(signature.Parameters, (int)instruction.Code - (int)OpCode.Ldarg_0, node)); break;
            case OpCode.Ldarg:
                stack.Push(Slot(signature.Parameters, instruction.ReadVariable(ref reader).Index, node)); break;
            case OpCode.Ldarg_s:
                stack.Push(Slot(signature.Parameters, instruction.ReadShortVariable(ref reader).Index, node)); break;
            case OpCode.Starg:
                stack.Store(Slot(signature.Parameters, instruction.ReadVariable(ref reader).Index, node)); break;
            case OpCode.Starg_s:
                stack.Store(Slot(signature.Parameters, instruction.ReadShortVariable(ref reader).Index, node)); break;
            case >= OpCode.Ldloc_0 and <= OpCode.Ldloc_3:
                stack.Push(Slot(locals, (int)instruction.Code - (int)OpCode.Ldloc_0, node)); break;
            case OpCode.Ldloc:
                stack.Push(Slot(locals, instruction.ReadVariable(ref reader).Index, node)); break;
            case OpCode.Ldloc_s:
                stack.Push(Slot(locals, instruction.ReadShortVariable(ref reader).Index, node)); break;
            case >= OpCode.Stloc_0 and <= OpCode.Stloc_3:
                stack.Store(Slot(locals, (int)instruction.Code - (int)OpCode.Stloc_0, node)); break;
            case OpCode.Stloc:
                stack.Store(Slot(locals, instruction.ReadVariable(ref reader).Index, node)); break;
            case OpCode.Stloc_s:
                stack.Store(Slot(locals, instruction.ReadShortVariable(ref reader).Index, node)); break;
            case OpCode.Ldloca:
                stack.Push(Address(locals, instruction.ReadVariable(ref reader).Index, node)); break;
            case OpCode.Ldloca_s:
                stack.Push(Address(locals, instruction.ReadShortVariable(ref reader).Index, node)); break;
            case OpCode.Ldarga:
                stack.Push(Address(signature.Parameters, instruction.ReadVariable(ref reader).Index, node)); break;
            case OpCode.Ldarga_s:
                stack.Push(Address(signature.Parameters, instruction.ReadShortVariable(ref reader).Index, node)); break;
            case OpCode.Ldfld:
            case OpCode.Ldflda:
            case OpCode.Stfld:
                var fieldToken = MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value);
                var resolvedField = Model.MemberResolver.Field(module, fieldToken, context, methodArguments);
                var field = resolvedField.DeclaringType.Module.GetField(resolvedField.Handle);
                if ((field.Attributes & System.Reflection.FieldAttributes.Static) != 0)
                    throw stack.Error("Instance field instruction targets a static field");
                var fieldType = SignatureReader.Field(resolvedField.DeclaringType.Module, field.Signature, resolvedField.DeclaringType.Arguments);
                if (node.Code == OpCode.Stfld) stack.Store(StackEntry.From(fieldType));
                var ownerIdentity = resolvedField.DeclaringType;
                var owner = new SignatureType(SignatureTypeCode.TypeHandle, ownerIdentity, IsReferenceType: !ownerIdentity.IsValueType);
                var instance = stack.Pop();
                bool address = instance == StackEntry.From(owner.ByReference());
                if (!address && !(instance.Type == StackType.Null && !ownerIdentity.IsValueType) &&
                    ((ownerIdentity.IsValueType && node.Code != OpCode.Ldfld) || !(instance == StackEntry.From(owner) ||
                        (instance.Type == StackType.ObjectReference && instance.DeclaredType?.Identity is { } actualOwner && actualOwner.IsAssignableTo(ownerIdentity)))))
                    throw stack.Error($"Expected {owner}&" + (node.Code == OpCode.Ldfld ? $" or {owner}" : ""));
                if (node.Code == OpCode.Ldfld) stack.Push(StackEntry.From(fieldType));
                if (node.Code == OpCode.Ldflda) stack.Push(StackEntry.From(fieldType.ByReference()));
                break;
            case OpCode.Isinst:
            case OpCode.Castclass:
                var targetType = SignatureReader.Resolve(module, MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value), context.Arguments, methodArguments).Identity!.Value;
                if ((targetType.Handle.Kind != HandleKind.TypeDefinition && !Model.DelegateInfo.IsRuntimeBase(targetType) && Model.Exceptions.ExceptionTypes.Kind(targetType) is null) || targetType.IsValueType)
                    throw new NotSupportedException("Type tests currently require same-module classes.");
                var tested = stack.Pop();
                if (tested.Type is not (StackType.ObjectReference or StackType.Null))
                    throw stack.Error("Class type tests require a class reference");
                stack.Push(StackEntry.From(new SignatureType(SignatureTypeCode.TypeHandle, targetType, IsReferenceType: true)));
                break;
            case OpCode.Ldftn:
            case OpCode.Ldvirtftn:
                var pointed = Model.MemberResolver.Method(module, MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value), context, methodArguments);
                if (node.Code == OpCode.Ldvirtftn)
                {
                    var receiver = stack.Pop();
                    if (receiver.Type is not (StackType.ObjectReference or StackType.Null)) throw stack.Error("ldvirtftn requires an object receiver");
                }
                stack.Push(new(StackType.MethodPointer, Method: pointed));
                break;
            case OpCode.Newobj:
                var constructorToken = instruction.ReadToken(ref reader).Value;
                var constructor = CallSignature(constructorToken);
                if (Model.DelegateInfo.Member(module, MetadataTokens.EntityHandle(constructorToken), new(context, default, methodArguments)) is { } delegateConstructor)
                {
                    if (delegateConstructor.DeclaringType.Module.GetMethod(delegateConstructor.Handle).Name != ".ctor") throw stack.Error("Expected a delegate constructor");
                    if (stack.Pop().Type != StackType.MethodPointer) throw stack.Error("Delegate construction requires a method pointer");
                    if (stack.Pop().Type is not (StackType.ObjectReference or StackType.Null)) throw stack.Error("Delegate construction requires an object target");
                    stack.Push(StackEntry.From(new SignatureType(SignatureTypeCode.TypeHandle, delegateConstructor.DeclaringType, IsReferenceType: true)));
                    break;
                }
                if (Model.Exceptions.ExceptionTypes.TryMember(module, MetadataTokens.EntityHandle(constructorToken), out _, out var exceptionMember) && Model.Exceptions.ExceptionTypes.IsConstructor(exceptionMember))
                {
                    for (int i = constructor.Parameters.Length - 1; i >= 1; i--) stack.Store(StackEntry.From(constructor.Parameters[i]));
                    stack.Push(StackEntry.From(constructor.Parameters[0]));
                    break;
                }
                var constructorHandle = Model.MemberResolver.Method(module, MetadataTokens.EntityHandle(constructorToken), context, methodArguments);
                if (constructorHandle.DeclaringType.Module.GetMethod(constructorHandle.Handle).Name != ".ctor" ||
                    constructor.Parameters.Length == 0 || constructor.Result is not null)
                    throw new NotSupportedException("Only same-module constructors are currently analyzed.");
                for (int i = constructor.Parameters.Length - 1; i >= 1; i--) stack.Store(StackEntry.From(constructor.Parameters[i]));
                stack.Push(StackEntry.From(constructor.Parameters[0] with { IsByReference = false }));
                break;
            case OpCode.Box:
                var boxed = SignatureReader.Resolve(module, MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value), context.Arguments, methodArguments);
                if (!boxed.IsReferenceType && boxed.Code is not (SignatureTypeCode.String or SignatureTypeCode.Object or SignatureTypeCode.SZArray))
                    throw new NotSupportedException("Boxing value types is not implemented.");
                stack.Store(StackEntry.From(boxed));
                stack.Push(StackEntry.From(boxed));
                break;
            case OpCode.Initobj:
            case OpCode.Ldobj:
            case OpCode.Stobj:
            case OpCode.Cpobj:
                var valueHandle = MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value);
                var resolvedValue = SignatureReader.Resolve(module, valueHandle, context.Arguments, methodArguments);
                if (resolvedValue.Identity is not { } && resolvedValue.Code is not (SignatureTypeCode.String or SignatureTypeCode.Object or SignatureTypeCode.SZArray))
                    throw new NotSupportedException("Primitive generic initobj/object operations are not implemented.");
                var value = resolvedValue;
                var pointer = StackEntry.From(value.ByReference());
                if (node.Code == OpCode.Stobj) stack.Store(StackEntry.From(value));
                if (node.Code == OpCode.Cpobj) stack.Pop(pointer);
                stack.Pop(pointer);
                if (node.Code == OpCode.Ldobj) stack.Push(StackEntry.From(value));
                break;
            case OpCode.Callvirt:
            case OpCode.Call:
                int callToken = instruction.ReadToken(ref reader).Value;
                if (Model.Intrinsics.ManagedType.Read(module, MetadataTokens.EntityHandle(callToken)) is { Operation: Model.Intrinsics.ManagedTypeOperation.FromHandle } typeHandle)
                {
                    if (stack.Pop().Type != StackType.RuntimeTypeHandle) throw stack.Error("GetTypeFromHandle requires a type token");
                    stack.Push(StackEntry.From(typeHandle.Signature.Result));
                    break;
                }
                if (invariantFormatting.TryGetValue(node.Offset, out var formatting))
                {
                    if (formatting.IsProvider) stack.Push(StackEntry.From(formatting.Provider));
                    else
                    {
                        stack.Pop(StackEntry.From(formatting.Provider));
                        stack.Pop(StackEntry.From(new SignatureType(SignatureTypeCode.Int32, IsByReference: true)));
                        stack.Push(StackType.String);
                    }
                    break;
                }
                if (Model.Intrinsics.ArrayCopy.Read(module, MetadataTokens.EntityHandle(callToken)) is { } copy)
                {
                    stack.Pop(StackType.Int32);
                    if (copy.HasIndices) stack.Pop(StackType.Int32);
                    var destination = stack.Pop();
                    if (copy.HasIndices) stack.Pop(StackType.Int32);
                    var sourceArray = stack.Pop();
                    if (sourceArray.Type is not (StackType.ArrayReference or StackType.Null) || destination.Type is not (StackType.ArrayReference or StackType.Null))
                        throw new NotSupportedException("Array.Copy requires statically typed vectors.");
                    if (sourceArray.DeclaredType?.Array is { } from && destination.DeclaredType?.Array is { } to && from != to)
                        throw new NotSupportedException("Array.Copy requires the same exact vector element type.");
                    break;
                }
                if (Model.WellKnownMethods.IsObjectConstructor(module, MetadataTokens.EntityHandle(callToken)))
                {
                    if (stack.Pop().Type != StackType.ObjectReference) throw stack.Error("Object constructor requires a class receiver");
                    break;
                }
                if (Model.Intrinsics.NullGuard.Is(module, MetadataTokens.EntityHandle(callToken)))
                {
                    stack.Store(StackType.String);
                    if (stack.Pop().Type is not (StackType.ObjectReference or StackType.String or StackType.ArrayReference or StackType.Null))
                        throw stack.Error("ThrowIfNull requires a reference value");
                    break;
                }
                var called = interpolationCalls.TryGetValue(node.Offset, out var handlerCall)
                    ? new Signature([new SignatureType(SignatureTypeCode.TypeHandle, handlerCall.Owner, IsByReference: true)], StackEntry.From(handlerCall.Signature.Result))
                    : CallSignature(callToken);
                for (int i = called.Parameters.Length - 1; i >= 0; i--) stack.Store(StackEntry.From(called.Parameters[i]));
                if (called.Result is StackEntry result) stack.Push(result);
                break;
            case OpCode.Throw:
                if (stack.Pop().Type is not (StackType.ObjectReference or StackType.Null)) throw stack.Error("throw requires an exception reference");
                while (stack.State.Depth != 0) stack.Pop();
                break;
            case OpCode.Leave:
            case OpCode.Leave_s:
                while (stack.State.Depth != 0) stack.Pop();
                break;
            case OpCode.Rethrow:
            case OpCode.Endfinally:
                if (stack.State.Depth != 0) throw stack.Error("Exception transfer requires an empty evaluation stack");
                break;
            case OpCode.Ret:
                if (signature.Result is StackEntry returned) stack.Store(returned);
                if (stack.State.Depth != 0) throw stack.Error("Return leaves unconsumed values");
                break;
            case OpCode.Br:
            case OpCode.Br_s: break;
            case OpCode.Brtrue:
            case OpCode.Brtrue_s:
            case OpCode.Brfalse:
            case OpCode.Brfalse_s:
                if (stack.Pop().Type is not (StackType.Int32 or StackType.Int64 or StackType.NativeInt or StackType.String or StackType.ArrayReference or StackType.ObjectReference or StackType.Null))
                    throw stack.Error("Invalid truth-test type");
                break;
            case OpCode.Switch: stack.Pop(StackType.Int32); break;
            case OpCode.Beq:
            case OpCode.Beq_s:
            case OpCode.Bne_un:
            case OpCode.Bne_un_s:
                ComparePair(ref stack, node.Code); break;
            case >= OpCode.Beq_s and <= OpCode.Blt_un_s:
            case >= OpCode.Beq and <= OpCode.Blt_un:
                NumericPair(ref stack); break;
            case OpCode.Ceq:
            case OpCode.Cgt_un:
                ComparePair(ref stack, node.Code); stack.Push(StackType.Int32); break;
            case OpCode.Cgt:
            case OpCode.Clt:
            case OpCode.Clt_un:
                NumericPair(ref stack); stack.Push(StackType.Int32); break;
            case OpCode.Add:
            case OpCode.Sub:
            case OpCode.Mul:
                stack.Push(NumericPair(ref stack)); break;
            case OpCode.Div:
                if (NumericPair(ref stack) != StackType.Float)
                    throw new NotSupportedException("Integer division is not implemented.");
                stack.Push(StackType.Float); break;
            case OpCode.And:
            case OpCode.Or:
            case OpCode.Xor:
                var integer = NumericPair(ref stack);
                if (integer == StackType.Float) throw stack.Error("Bitwise operands must be integers");
                stack.Push(integer); break;
            case OpCode.Neg:
                var negated = Numeric(ref stack); stack.Push(negated); break;
            case OpCode.Not:
                var inverted = Numeric(ref stack);
                if (inverted == StackType.Float) throw stack.Error("Not requires an integer");
                stack.Push(inverted); break;
            case OpCode.Shl:
            case OpCode.Shr:
            case OpCode.Shr_un:
                if (stack.Pop().Type is not (StackType.Int32 or StackType.NativeInt)) throw stack.Error("Invalid shift count type");
                var shifted = Numeric(ref stack);
                if (shifted == StackType.Float) throw stack.Error("Shift requires an integer");
                stack.Push(shifted); break;
            case OpCode.Conv_i1:
            case OpCode.Conv_u1:
            case OpCode.Conv_i2:
            case OpCode.Conv_u2:
            case OpCode.Conv_i4:
            case OpCode.Conv_u4:
                Numeric(ref stack); stack.Push(StackType.Int32); break;
            case OpCode.Conv_i8:
            case OpCode.Conv_u8:
                Numeric(ref stack); stack.Push(StackType.Int64); break;
            case OpCode.Conv_r4:
            case OpCode.Conv_r8:
                Numeric(ref stack); stack.Push(StackType.Float); break;
            case OpCode.Conv_r_un:
                if (Numeric(ref stack) == StackType.Float) throw stack.Error("Conv_r_un requires an integer");
                stack.Push(StackType.Float); break;
            case OpCode.Conv_i:
            case OpCode.Conv_u:
                Numeric(ref stack); stack.Push(StackType.NativeInt); break;
            default:
                throw new NotSupportedException($"Stack analysis for {instruction.Code} at IL_{node.Offset:x4} is not implemented.");
        }
        node.After = stack.State;
        node.Lowest = stack.Lowest;
    }

    private static void ArrayIndex(ref StackCursor stack)
    {
        if (stack.Pop().Type is not (StackType.Int32 or StackType.NativeInt)) throw stack.Error("Array index/length requires an integer");
    }

    private static StackEntry Slot(SignatureType[] slots, int index, InstructionNode instruction) =>
        index < slots.Length ? StackEntry.From(slots[index]) : throw new InvalidProgramException($"Invalid slot {index} at IL_{instruction.Offset:x4}.");

    private static StackEntry Address(SignatureType[] slots, int index, InstructionNode node) =>
        index < slots.Length ? StackEntry.From(slots[index].ByReference()) :
            throw new InvalidProgramException($"Invalid slot {index} at IL_{node.Offset:x4}.");

    private Signature CallSignature(int token)
    {
        if (calls.TryGetValue(token, out var signature)) return signature;
        var handle = MetadataTokens.EntityHandle(token);
        if (Model.Intrinsics.ManagedType.Read(module, handle) is { } managedType)
            signature = new(managedType.Signature.HasThis ? [new(SignatureTypeCode.Object)] : managedType.Signature.Parameters.ToArray(),
                StackEntry.From(managedType.Signature.Result));
        else if (Model.Intrinsics.StringConcatenation.Read(module, handle) is { } concat)
            signature = new(concat.Signature.Parameters.ToArray(), StackType.String);
        else if (Model.Intrinsics.ResourceString.Read(module, handle) is not null)
            signature = new([], StackType.String);
        else if (Model.Intrinsics.Int32Formatting.Is(module, handle))
            signature = new([new(SignatureTypeCode.Int32, IsByReference: true)], StackType.String);
        else if (Model.Intrinsics.ConstructorFactory.Read(module, handle, context.Arguments, methodArguments) is { } constructor)
            signature = new([], StackEntry.From(new SignatureType(SignatureTypeCode.TypeHandle, constructor.DeclaringType, IsReferenceType: true)));
        else if (Model.Intrinsics.InlineArrayAccess.Read(module, handle, context.Arguments, methodArguments) is { } inline)
            signature = new(inline.Signature.Parameters.ToArray(), StackEntry.From(inline.Signature.Result));
        else if (Model.Intrinsics.SpanAccess.Read(module, handle, context.Arguments, methodArguments) is { } span)
            signature = new([new SignatureType(SignatureTypeCode.TypeHandle, span.Owner, IsByReference: true), .. span.Signature.Parameters], StackEntry.From(span.Signature.Result));
        else if (Model.Intrinsics.InterpolatedStringHandler.Read(module, handle, context.Arguments, methodArguments) is { } interpolation)
            signature = new([new SignatureType(SignatureTypeCode.TypeHandle, interpolation.Owner, IsByReference: true), .. interpolation.Signature.Parameters],
                interpolation.Signature.Result.Code == SignatureTypeCode.Void ? null : StackEntry.From(interpolation.Signature.Result));
        else if (Model.Intrinsics.StringEquality.Read(module, handle) is { } textEquality)
            signature = new([new(SignatureTypeCode.String), new(SignatureTypeCode.String)], StackType.Int32);
        else if (Model.Intrinsics.DefaultEquality.Read(module, handle, context.Arguments, methodArguments) is { } equality)
            signature = new(equality.Signature.HasThis
                ? [new SignatureType(SignatureTypeCode.TypeHandle, equality.Owner, IsReferenceType: true), .. equality.Signature.Parameters]
                : equality.Signature.Parameters.ToArray(), StackEntry.From(equality.Signature.Result));
        else if (Model.Intrinsics.NativeFloat.TryMethod(module, handle, out _, out var native) && native.HasThis)
        {
            var member = module.GetMemberReference((MemberReferenceHandle)handle);
            signature = new([new SignatureType(SignatureTypeCode.TypeHandle, module.ResolveType(member.Parent), IsByReference: true), .. native.Parameters], StackEntry.From(native.Result));
        }
        else if (Model.Intrinsics.TimeSpanFactory.Read(module, handle) is { } duration)
            signature = new(duration.Signature.Parameters.ToArray(), StackEntry.From(duration.Signature.Result));
        else if (Model.Intrinsics.ManagedReference.Read(module, handle, context.Arguments, methodArguments) is { } address)
            signature = new(address.Signature.Parameters.ToArray(), StackEntry.From(address.Signature.Result));
        else if (Model.Intrinsics.ReferenceCompareExchange.Read(module, handle, context.Arguments, methodArguments) is { } exchange)
            signature = new(exchange.Parameters.ToArray(), StackEntry.From(exchange.Result));
        else if (Model.Exceptions.ExceptionTypes.TryMember(module, handle, out _, out _))
        {
            var reference = module.GetMemberReference((MemberReferenceHandle)handle);
            signature = Signature.Read(module, reference.Signature, new Model.TypeIdentity(module, reference.Parent));
        }
        else if (handle.Kind == HandleKind.MemberReference &&
            module.GetMemberReference((MemberReferenceHandle)handle).Parent.Kind is not (HandleKind.TypeDefinition or HandleKind.TypeSpecification) &&
            module.ResolveType(module.GetMemberReference((MemberReferenceHandle)handle).Parent).Handle.Kind != HandleKind.TypeDefinition)
        {
            // External static calls still provide signatures for rewrite-aware analysis.
            signature = Signature.Read(module, module.GetMemberReference((MemberReferenceHandle)handle).Signature);
        }
        else
        {
            var resolved = Model.MemberResolver.Method(module, handle, context, methodArguments);
            signature = Signature.Read(resolved.DeclaringType.Module, resolved.DeclaringType.Module.GetMethod(resolved.Handle).Signature, resolved.DeclaringType, resolved.Arguments);
        }
        calls.Add(token, signature);
        return signature;
    }

    private static void ComparePair(ref StackCursor stack, OpCode code)
    {
        var right = stack.Pop(); var left = stack.Pop();
        bool Reference(StackEntry value) => value.Type is StackType.ObjectReference or StackType.ArrayReference or StackType.String or StackType.Null;
        if (Reference(left) && Reference(right))
        {
            if (code == OpCode.Cgt_un && right.Type != StackType.Null) throw stack.Error("Reference cgt.un requires null on the right");
            if (left.Type == StackType.Null || right.Type == StackType.Null || left == right ||
                (left.Type == StackType.ObjectReference && right.Type == StackType.ObjectReference)) return;
            throw stack.Error("Reference comparison type mismatch");
        }
        if (left != right || left.Type is not (StackType.Int32 or StackType.Int64 or StackType.NativeInt or StackType.Float))
            throw stack.Error("Comparison type mismatch");
    }

    private static StackEntry Numeric(ref StackCursor stack)
    {
        var type = stack.Pop();
        if (type.Type is not (StackType.Int32 or StackType.Int64 or StackType.NativeInt or StackType.Float)) throw stack.Error("Expected a numeric operand");
        return type;
    }

    private static StackEntry NumericPair(ref StackCursor stack)
    {
        var right = Numeric(ref stack);
        stack.Pop(right);
        return right;
    }
}
