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
    private readonly IReadOnlyDictionary<int, Model.Intrinsics.SpanAccess> spanCalls = SpanCalls.Read(module, body, graph, context, methodArguments);
    private readonly IReadOnlyDictionary<int, Model.Intrinsics.InterpolatedStringHandler> interpolationCalls = InterpolationCalls.Read(module, body, graph, context, methodArguments);
    private readonly IReadOnlyDictionary<int, StaticSpanCall> staticSpans = StaticSpanCalls.Read(module, body, graph, context, methodArguments);
    private readonly IReadOnlyDictionary<int, PrimitiveCall> primitiveCalls = PrimitiveCalls.Read(module, body, graph, context, methodArguments);
    private readonly IReadOnlyDictionary<int, InvariantFormattingCall> invariantFormatting = InvariantFormattingCalls.Read(module, body, graph);

    private readonly IReadOnlyDictionary<int, ConstrainedCall> constrainedCalls = ConstrainedCalls.Read(module, body, graph, context, methodArguments);

    public void Apply(InstructionNode node)
    {
        var reader = body.Instructions;
        reader.Bytes.Offset = node.Offset;
        var instruction = reader.Read();
        var stack = new StackCursor(node);
        if (graph.Specializations.TryGetValue(node.Offset, out var specialized))
        {
            if (specialized.Kind == SpecializationKind.Int32) stack.Push(StackType.Int32);
            else if (specialized.Kind == SpecializationKind.Branch) stack.Pop();
            else if (specialized.Kind == SpecializationKind.NullTypeTest) { stack.Pop(); stack.Push(StackType.Null); }
            else if (specialized.Kind is SpecializationKind.DiscardAndInt32 or SpecializationKind.NullCheckAndInt32) { stack.Pop(); stack.Push(StackType.Int32); }
            node.After = stack.State; node.Lowest = stack.Lowest;
            return;
        }
        switch (instruction.Code)
        {
            case OpCode.Ldsflda:
            case OpCode.Ldtoken:
                if (staticSpans.TryGetValue(node.Offset, out var spanToken))
                {
                    _ = instruction.ReadToken(ref reader);
                    stack.Push(new(StackType.RuntimeFieldHandle, FieldToken: spanToken.Field)); break;
                }
                if (node.Code == OpCode.Ldsflda) goto case OpCode.Stsfld;
                var tokenType = Model.Intrinsics.ManagedType.Token(module, MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value), context.Arguments, methodArguments);
                stack.Push(new(StackType.RuntimeTypeHandle, TypeToken: tokenType));
                break;
            case OpCode.Ldsfld:
                var staticField = MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value);
                if (Model.Intrinsics.StringEmpty.Is(module, staticField)) { stack.Push(StackType.String); break; }
                if (Model.Intrinsics.StopwatchConstant.Read(module, staticField) is { } clockConstant)
                {
                    stack.Push(StackEntry.From(new SignatureType(clockConstant.Code))); break;
                }
                var zeroType = Model.Intrinsics.TimeSpanZero.Read(module, staticField, context);
                if (zeroType is not null)
                { stack.Push(StackEntry.From(new SignatureType(SignatureTypeCode.TypeHandle, zeroType))); break; }
                reader.Seek(node.Offset); _ = reader.Read();
                goto case OpCode.Stsfld;
            case OpCode.Stsfld:
                var staticIdentity = Model.MemberResolver.Field(module, MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value), context, methodArguments);
                var staticDeclaration = staticIdentity.DeclaringType.Module.GetField(staticIdentity.Handle);
                if ((staticDeclaration.Attributes & System.Reflection.FieldAttributes.Static) == 0)
                    throw stack.Error("Static field instruction targets an instance field");
                var staticType = SignatureReader.Field(staticIdentity.DeclaringType.Module, staticDeclaration.Signature, staticIdentity.DeclaringType.Arguments);
                if (node.Code == OpCode.Stsfld) stack.Store(StackEntry.From(staticType));
                else stack.Push(StackEntry.From(node.Code == OpCode.Ldsflda ? staticType.ByReference() : staticType));
                break;
            case OpCode.Newarr:
                ArrayIndex(ref stack);
                var element = SignatureReader.StorageType(module, MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value), context.Arguments, methodArguments);
                stack.Push(StackEntry.From(SignatureType.Vector(element)));
                break;
            case OpCode.Sizeof:
                _ = instruction.ReadToken(ref reader);
                stack.Push(StackType.Int32); break;
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
                if (!spanCalls.ContainsKey(reader.Offset) && !interpolationCalls.ContainsKey(reader.Offset) && !primitiveCalls.ContainsKey(reader.Offset) && !constrainedCalls.ContainsKey(reader.Offset)) throw new NotSupportedException("Constrained dispatch requiring boxing or a native primitive receiver is not implemented.");
                break;
            case OpCode.Nop:
            case OpCode.Volatile: break;
            case OpCode.Localloc:
                ArrayIndex(ref stack);
                if (stack.State.Depth != 0) throw stack.Error("localloc requires only its size on the evaluation stack");
                stack.Push(StackType.NativeInt); break;
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
                var testToken = MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value);
                var testBox = Model.BoxingType.Read(module, testToken, context.Arguments, methodArguments);
                var testStorage = SignatureReader.StorageType(module, testToken, context.Arguments, methodArguments);
                if (testStorage.Code == SignatureTypeCode.String)
                {
                    if (stack.Pop().Type is not (StackType.ObjectReference or StackType.String or StackType.Null))
                        throw stack.Error("String casts require an object or native string reference");
                    stack.Push(StackType.String); break;
                }
                var targetType = testBox?.Identity ?? testStorage.Identity;
                if (testStorage.Code != SignatureTypeCode.Object && (targetType is not { } testIdentity ||
                    testIdentity.Handle.Kind != HandleKind.TypeDefinition && !Model.DelegateInfo.IsRuntimeBase(testIdentity) && Model.Exceptions.ExceptionTypes.Kind(testIdentity) is null))
                    throw new NotSupportedException("Type tests require linked classes or supported boxed values.");
                var tested = stack.Pop();
                if (tested.Type is not (StackType.ObjectReference or StackType.ArrayReference or StackType.Null))
                    throw stack.Error("Class type tests require a class reference");
                stack.Push(StackEntry.From(testBox is not null || testStorage.Code == SignatureTypeCode.Object ? new SignatureType(SignatureTypeCode.Object) : new SignatureType(SignatureTypeCode.TypeHandle, targetType, IsReferenceType: true)));
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
                if (Model.Intrinsics.RectangularArray.Read(module, MetadataTokens.EntityHandle(constructorToken), context.Arguments, methodArguments) is { Operation: ".ctor" } rectangleConstructor)
                {
                    foreach (var parameter in rectangleConstructor.Signature.Parameters.Reverse()) stack.Store(StackEntry.From(parameter));
                    stack.Push(StackEntry.From(rectangleConstructor.ArrayType!.Value)); break;
                }
                if (Model.Intrinsics.StringCharacters.Read(module, MetadataTokens.EntityHandle(constructorToken)) is { } characters)
                {
                    foreach (var parameter in characters.Parameters.Reverse()) stack.Store(StackEntry.From(parameter));
                    stack.Push(StackType.String); break;
                }
                if (Model.Intrinsics.StringRepeat.Is(module, MetadataTokens.EntityHandle(constructorToken)))
                {
                    stack.Store(StackType.Int32); stack.Store(StackType.Int32);
                    stack.Push(StackType.String); break;
                }
                if (Model.Intrinsics.StringSpan.Read(module,MetadataTokens.EntityHandle(constructorToken)) is { Constructor:true } stringConstructor)
                {
                    stack.Store(StackEntry.From(stringConstructor.Signature.Parameters[0]));
                    stack.Push(StackType.String); break;
                }
                if (staticSpans.TryGetValue(node.Offset, out var staticConstructor))
                {
                    stack.Store(StackType.Int32);
                    stack.Pop(new(StackType.RuntimeFieldHandle, FieldToken: staticConstructor.Field));
                    stack.Push(StackEntry.From(staticConstructor.Result)); break;
                }
                if (Model.Intrinsics.SpanAccess.Read(module, MetadataTokens.EntityHandle(constructorToken), context.Arguments, methodArguments) is { Operation: "FromArray" or "FromReference" or "FromPointer" } arraySpan)
                {
                    for (int i = arraySpan.Signature.Parameters.Length - 1; i >= 0; i--)
                        stack.Store(arraySpan.Operation == "FromPointer" && i == 0 ? new(StackType.NativeInt) : StackEntry.From(arraySpan.Signature.Parameters[i]));
                    stack.Push(StackEntry.From(new SignatureType(SignatureTypeCode.TypeHandle, arraySpan.Owner))); break;
                }
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
            case OpCode.Unbox:
            case OpCode.Unbox_any:
                var boxToken = MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value);
                var box = Model.BoxingType.Read(module, boxToken, context.Arguments, methodArguments);
                var boxValue = SignatureReader.StorageType(module, boxToken, context.Arguments, methodArguments);
                if (node.Code == OpCode.Box)
                {
                    stack.Store(StackEntry.From(boxValue));
                    stack.Push(StackEntry.From(box is null ? boxValue : new SignatureType(SignatureTypeCode.Object)));
                }
                else
                {
                    var unboxed = stack.Pop();
                    if (unboxed.Type is not (StackType.ObjectReference or StackType.Null) && !(boxValue.Code == SignatureTypeCode.String && unboxed.Type == StackType.String))
                        throw stack.Error("Unboxing requires an object reference");
                    if (node.Code == OpCode.Unbox && box is null) throw new NotSupportedException("unbox requires a value type.");
                    stack.Push(StackEntry.From(node.Code == OpCode.Unbox ? boxValue.ByReference() : boxValue));
                }
                break;
            case OpCode.Initobj:
            case OpCode.Ldobj:
            case OpCode.Stobj:
            case OpCode.Cpobj:
                var valueHandle = MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value);
                var resolvedValue = SignatureReader.StorageType(module, valueHandle, context.Arguments, methodArguments);
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
                if (Model.Intrinsics.MonitorMethod.Read(module, MetadataTokens.EntityHandle(callToken)) is { } monitorCall &&
                    (monitorCall.HasLockTaken ? stack.State.Parent : stack.State)?.Entry?.Type == StackType.String)
                    throw new NotSupportedException("Monitors on native strings require managed string identity.");
                if (Model.Intrinsics.BitConversion.Read(module, MetadataTokens.EntityHandle(callToken)) is { } bitConversion)
                { stack.Store(StackEntry.From(bitConversion.Input)); stack.Push(StackEntry.From(bitConversion.Result)); break; }
                if (Model.Intrinsics.SuppressFinalization.Is(module, MetadataTokens.EntityHandle(callToken)))
                { stack.Store(StackEntry.From(new Model.Signatures.SignatureType(SignatureTypeCode.Object, IsReferenceType: true))); break; }
                if (Model.Intrinsics.RectangularArray.Read(module, MetadataTokens.EntityHandle(callToken), context.Arguments, methodArguments) is { } rectangle)
                {
                    foreach (var parameter in rectangle.Signature.Parameters.Reverse()) stack.Store(StackEntry.From(parameter));
                    if (rectangle.ArrayType is { } arrayType) stack.Store(StackEntry.From(arrayType));
                    else if (stack.Pop().DeclaredType?.Array is null) throw stack.Error("Array dimension queries require an exact array type");
                    if (rectangle.Signature.Result.Code != SignatureTypeCode.Void) stack.Push(StackEntry.From(rectangle.Signature.Result));
                    break;
                }
                if (Model.Intrinsics.ManagedType.Read(module, MetadataTokens.EntityHandle(callToken)) is { Operation: Model.Intrinsics.ManagedTypeOperation.FromHandle } typeHandle)
                {
                    if (stack.Pop().Type != StackType.RuntimeTypeHandle) throw stack.Error("GetTypeFromHandle requires a type token");
                    stack.Push(StackEntry.From(typeHandle.Signature.Result));
                    break;
                }
                if (Model.Intrinsics.CompositeFormatting.Read(module, MetadataTokens.EntityHandle(callToken)) is { } composite)
                {
                    for (int i = composite.Signature.Parameters.Length - 1; i >= 0; i--)
                    {
                        // This intrinsic can consume native JS strings directly, without a general string-to-object wrapper.
                        if (composite.Signature.Parameters[i].Code == SignatureTypeCode.Object && stack.State.Entry?.Type == StackType.String)
                            stack.Pop();
                        else stack.Store(StackEntry.From(composite.Signature.Parameters[i]));
                    }
                    stack.Push(StackType.String);
                    break;
                }
                if (invariantFormatting.TryGetValue(node.Offset, out var formatting))
                {
                    if (formatting.IsProvider) stack.Push(StackEntry.From(formatting.Provider));
                    else
                    {
                        stack.Pop(StackEntry.From(formatting.Provider));
                        if (formatting.Parsing is { } parsing)
                        {
                            if (parsing.HasStyle) stack.Pop(StackType.Int32);
                            stack.Store(StackEntry.From(parsing.Input));
                            stack.Push(StackEntry.From(parsing.Result));
                            break;
                        }
                        if (formatting.HasNullFormat || formatting.ValueType is SignatureTypeCode.Double or SignatureTypeCode.Single) stack.Store(StackType.String);
                        stack.Pop(StackEntry.From(formatting.Value.ByReference()));
                        stack.Push(StackType.String);
                    }
                    break;
                }
                if (Model.Intrinsics.ArrayClear.Read(module, MetadataTokens.EntityHandle(callToken)) is { } clear)
                {
                    if (clear.HasIndices) { stack.Pop(StackType.Int32); stack.Pop(StackType.Int32); }
                    if (stack.Pop().Type is not (StackType.ArrayReference or StackType.Null))
                        throw new NotSupportedException("Array.Clear requires a statically typed vector.");
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
                    if (!copy.ByteOffsets && sourceArray.DeclaredType?.Array is { } from && destination.DeclaredType?.Array is { } to && from != to)
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
                if (staticSpans.TryGetValue(node.Offset, out var staticSpan))
                {
                    stack.Pop(new(StackType.RuntimeFieldHandle, FieldToken: staticSpan.Field));
                    if (staticSpan.ArrayInitializer)
                    {
                        var initialized = stack.Pop();
                        if (initialized.DeclaredType?.Array is not { } initializedArray || StaticSpanCalls.ArrayElementSize(initializedArray.Element) == 0)
                            throw new NotSupportedException("InitializeArray requires an exact primitive or enum vector.");
                    }
                    else stack.Push(StackEntry.From(staticSpan.Result));
                    break;
                }
                if (node.Code == OpCode.Callvirt && stack.State.Type is StackType.String or StackType.Null &&
                    PrimitiveCalls.IsObjectHash(module, MetadataTokens.EntityHandle(callToken)))
                {
                    stack.Store(StackType.String); stack.Push(StackType.Int32); break;
                }
                if (primitiveCalls.TryGetValue(node.Offset, out var primitive))
                {
                    if (primitive.Operation is "Equals" or "CompareTo") stack.Store(StackEntry.From(primitive.Element));
                    stack.Store(StackEntry.From(primitive.AddressReceiver ? primitive.Element.ByReference() : primitive.Element));
                    stack.Push(primitive.Operation == "ToString" ? StackType.String : StackType.Int32); break;
                }
                if (spanCalls.TryGetValue(node.Offset,out var spanCall))
                {
                    stack.Store(StackEntry.From(new SignatureType(SignatureTypeCode.TypeHandle,spanCall.Owner,IsByReference:true)));
                    stack.Push(StackType.String); break;
                }
                var called = interpolationCalls.TryGetValue(node.Offset, out var handlerCall)
                    ? new Signature([new SignatureType(SignatureTypeCode.TypeHandle, handlerCall.Owner, IsByReference: true)], StackEntry.From(handlerCall.Signature.Result))
                    : constrainedCalls.TryGetValue(node.Offset, out var concrete)
                        ? ConstrainedSignature(concrete)
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
                if (stack.State.Depth != 0) throw stack.Error("Return requires an empty evaluation stack after consuming its result");
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
            case OpCode.Mul_ovf_un:
            case OpCode.Add_ovf:
            case OpCode.Add_ovf_un:
            case OpCode.Sub_ovf:
            case OpCode.Sub_ovf_un:
                var factor = Numeric(ref stack);
                var multiplicand = Numeric(ref stack);
                if (factor.Type == StackType.Float || multiplicand.Type == StackType.Float ||
                    (factor.Type == StackType.Int64) != (multiplicand.Type == StackType.Int64))
                    throw stack.Error("Checked arithmetic requires compatible integer operands");
                stack.Push(factor.Type == StackType.NativeInt || multiplicand.Type == StackType.NativeInt ? new(StackType.NativeInt) : factor);
                break;
            case OpCode.Div:
            case OpCode.Div_un:
            case OpCode.Rem:
            case OpCode.Rem_un:
                var quotient = NumericPair(ref stack);
                if (quotient == StackType.Float && node.Code is OpCode.Div_un or OpCode.Rem_un)
                    throw new NotSupportedException("Unsigned floating division and remainder are not valid MSIL.");
                stack.Push(quotient); break;
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
        index < slots.Length ? StackEntry.From(slots[index]) : throw new InvalidProgramException($"Invalid {SlotKind(instruction.Code)} index {index} at IL_{instruction.Offset:x4}.");

    private static StackEntry Address(SignatureType[] slots, int index, InstructionNode node) =>
        index < slots.Length ? StackEntry.From(slots[index].ByReference()) :
            throw new InvalidProgramException($"Invalid {SlotKind(node.Code)} index {index} at IL_{node.Offset:x4}.");

    private static string SlotKind(OpCode code) => code is >= OpCode.Ldarg_0 and <= OpCode.Ldarg_3 or
        OpCode.Ldarg or OpCode.Ldarg_s or OpCode.Ldarga or OpCode.Ldarga_s or OpCode.Starg or OpCode.Starg_s ? "argument" : "local";

    private static Signature ConstrainedSignature(ConstrainedCall call)
    {
        var target = call.Target;
        var signature = Signature.Read(target.DeclaringType.Module, target.DeclaringType.Module.GetMethod(target.Handle).Signature, target.DeclaringType, target.Arguments);
        if (call.ReferenceReceiver is { } receiver) signature.Parameters[0] = receiver.ByReference();
        return signature;
    }

    private Signature CallSignature(int token)
    {
        if (calls.TryGetValue(token, out var signature)) return signature;
        var handle = MetadataTokens.EntityHandle(token);
        if (Model.Intrinsics.ManagedType.Read(module, handle) is { } managedType)
            signature = new(managedType.Signature.HasThis ? [new(SignatureTypeCode.Object)] : managedType.Signature.Parameters.ToArray(),
                StackEntry.From(managedType.Signature.Result));
        else if (Model.Intrinsics.CompositeFormatting.Read(module, handle) is { } format)
            signature = new(format.Signature.Parameters.ToArray(), StackType.String);
        else if (Model.Intrinsics.NativeStringComparer.Read(module, handle) is { } nativeComparer)
            signature = new(nativeComparer.Parameters.ToArray(), StackEntry.From(nativeComparer.Result));
        else if (Model.Intrinsics.CharFormatting.Read(module, handle) is { } character)
            signature = new([new(SignatureTypeCode.Char, IsByReference: character.HasThis)], StackType.String);
        else if (Model.Intrinsics.StringAccess.Read(module, handle) is { } stringAccess)
            signature = new([new(SignatureTypeCode.String), .. stringAccess.Signature.Parameters], StackEntry.From(stringAccess.Signature.Result));
        else if (Model.Intrinsics.StringSplit.Read(module, handle) is { } split)
            signature = new([new(SignatureTypeCode.String), .. split.Signature.Parameters], StackEntry.From(split.Signature.Result));
        else if (Model.Intrinsics.TextElement.Read(module, handle) is { } textElement)
            signature = new(textElement.Signature.Parameters.ToArray(), StackType.Int32);
        else if (Model.Intrinsics.StringConcatenation.Read(module, handle) is { } concat)
            signature = new(concat.Signature.Parameters.ToArray(), StackType.String);
        else if (Model.Intrinsics.ResourceString.Read(module, handle) is not null)
            signature = new([], StackType.String);
        else if (Model.Intrinsics.MonotonicClock.Read(module, handle) is not null)
            signature = new([], StackType.Int64);
        else if (Model.Intrinsics.ExecutionStack.Read(module, handle) is { } stackQuery)
            signature = new([], stackQuery == "TryEnsureSufficientExecutionStack" ? StackType.Int32 : null);
        else if (Model.Intrinsics.MemoryBarrier.Is(module, handle))
            signature = new([], null);
        else if (Model.Intrinsics.CurrentThread.Is(module, handle))
            signature = new([], StackType.Int32);
        else if (Model.Intrinsics.ObjectText.Is(module, handle))
            signature = new([new(SignatureTypeCode.Object)], StackType.String);
        else if (Model.Intrinsics.ObjectHash.Is(module, handle))
            signature = new([new(SignatureTypeCode.Object)], StackType.Int32);
        else if (Model.Intrinsics.GcArrayAllocation.Read(module, handle, context.Arguments, methodArguments) is { } allocation)
            signature = new(allocation.Signature.Parameters.ToArray(), StackEntry.From(allocation.Signature.Result));
        else if (Model.Intrinsics.Int32Formatting.InvariantProvider(module, handle) is { } provider)
            signature = new([], StackEntry.From(provider));
        else if (Model.Intrinsics.Int32Formatting.Is(module, handle))
            signature = new([new(SignatureTypeCode.Int32, IsByReference: true)], StackType.String);
        else if (Model.Intrinsics.HostScheduler.Read(module, handle) is { } scheduler)
            signature = new(scheduler.Signature.Parameters.ToArray(), null);
        else if (Model.Intrinsics.MonitorMethod.Read(module, handle) is { } monitor)
            signature = new(monitor.Signature.Parameters.ToArray(), monitor.Signature.Result.Code == SignatureTypeCode.Void ? null : StackEntry.From(monitor.Signature.Result));
        else if (Model.Intrinsics.ConstructorFactory.Read(module, handle, context.Arguments, methodArguments) is { } constructor)
            signature = new([], StackEntry.From(new SignatureType(SignatureTypeCode.TypeHandle, constructor.Type, IsReferenceType: true)));
        else if (Model.Intrinsics.InlineArrayAccess.Read(module, handle, context.Arguments, methodArguments) is { } inline)
            signature = new(inline.Signature.Parameters.ToArray(), StackEntry.From(inline.Signature.Result));
        else if (Model.Intrinsics.StringSpan.Read(module,handle) is { } stringSpan)
            signature = new(stringSpan.Signature.Parameters.ToArray(),StackEntry.From(stringSpan.Signature.Result));
        else if (Model.Intrinsics.SpanAccess.Read(module, handle, context.Arguments, methodArguments) is { } span)
            signature = new(span.Signature.HasThis
                ? [new SignatureType(SignatureTypeCode.TypeHandle, span.Owner, IsByReference: true), .. span.Signature.Parameters.Select(p => span.Operation == "FromPointer" && p.Code == SignatureTypeCode.Pointer ? new SignatureType(SignatureTypeCode.IntPtr) : p)]
                : span.Signature.Parameters.ToArray(), span.Signature.Result.Code == SignatureTypeCode.Void ? null : StackEntry.From(span.Signature.Result));
        else if (Model.Intrinsics.InterpolatedStringHandler.Read(module, handle, context.Arguments, methodArguments) is { } interpolation)
            signature = new(interpolation.Operation == "Create" ? interpolation.Signature.Parameters.ToArray() : [new SignatureType(SignatureTypeCode.TypeHandle, interpolation.Owner, IsByReference: true), .. interpolation.Signature.Parameters],
                interpolation.Signature.Result.Code == SignatureTypeCode.Void ? null : StackEntry.From(interpolation.Signature.Result));
        else if (Model.Intrinsics.StringEquality.Read(module, handle) is { } textEquality)
            signature = new([new(SignatureTypeCode.String), new(SignatureTypeCode.String),
                .. textEquality.HasComparison ? new[] { textEquality.Signature.Parameters[^1] } : []], StackType.Int32);
        else if (Model.Intrinsics.SharedArrayPool.Read(module, handle, context.Arguments, methodArguments) is { } pool)
            signature = new(pool.Signature.HasThis
                ? [new SignatureType(SignatureTypeCode.TypeHandle, pool.Owner, IsReferenceType: true), .. pool.Signature.Parameters]
                : pool.Signature.Parameters.ToArray(), pool.Signature.Result.Code == SignatureTypeCode.Void ? null : StackEntry.From(pool.Signature.Result));
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
        else if (Model.Intrinsics.ManagedBuffer.Read(module, handle, context.Arguments, methodArguments) is { } buffer)
            signature = new(buffer.Signature.Parameters.ToArray(), buffer.Signature.Result.Code == SignatureTypeCode.Void ? null : StackEntry.From(buffer.Signature.Result));
        else if (Model.Intrinsics.ManagedReference.Read(module, handle, context.Arguments, methodArguments) is { } address)
            signature = new(address.Signature.Parameters.ToArray(), StackEntry.From(address.Signature.Result));
        else if (Model.Intrinsics.ReferenceInterlocked.Read(module, handle, context.Arguments, methodArguments) is { } exchange)
            signature = new(exchange.Parameters.ToArray(), StackEntry.From(exchange.Result));
        else if (Model.Exceptions.ExceptionTypes.TryMember(module, handle, out _, out _))
        {
            if (handle.Kind == HandleKind.MemberReference)
            {
                var reference = module.GetMemberReference((MemberReferenceHandle)handle);
                signature = Signature.Read(module, reference.Signature, new Model.TypeIdentity(module, reference.Parent));
            }
            else
            {
                var definition = module.GetMethod((MethodDefinitionHandle)handle);
                signature = Signature.Read(module, definition.Signature, new Model.TypeIdentity(module, definition.DeclaringType));
            }
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
