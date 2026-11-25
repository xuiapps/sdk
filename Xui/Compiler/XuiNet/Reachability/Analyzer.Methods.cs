using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.Reachability;

public sealed partial class Analyzer
{
    private void ReadMethod(Dependency.Method node)
    {
        var identity = node.Identity;
        var module = identity.DeclaringType.Module;
        var method = module.GetMethod(identity.Handle);
        ReadDeclaration(node, identity, true);
        if ((method.Attributes & MethodAttributes.Abstract) != 0) return;
        var body = method.Body ?? throw new NotSupportedException($"The method has no managed body: {identity}.");
        foreach (var region in body.ExceptionRegions)
        {
            if (region.Kind == ExceptionRegionKind.Filter) throw new NotSupportedException("Exception filters are not implemented.");
            if (region.Kind == ExceptionRegionKind.Catch)
            {
                var caught = SignatureReader.Resolve(module, region.CatchType);
                if (caught.Identity is not { } catchType || DotNet.Exceptions.ExceptionTypes.Kind(catchType) is null && !DotNet.Exceptions.ExceptionTypes.IsCatchAll(catchType))
                    throw new NotSupportedException($"Catch type is not a supported built-in exception: {caught}.");
                Signature(node, caught, DependencyReason.TypeOperand, region.HandlerOffset);
            }
        }
        System.Collections.Immutable.ImmutableArray<SignatureType> locals;
        try { locals = SignatureReader.Locals(module, body.LocalSignature, identity.DeclaringType.Arguments, identity.Arguments); }
        catch (NotSupportedException error)
        {
            throw new NotSupportedException($"Unsupported local type in {identity}: {error.Message}", error);
        }
        foreach (var local in locals)
            Signature(node, local, DependencyReason.Local);

        var graph = InstructionGraph.Read(body.Instructions);
        TypeSpecialization.Apply(module, body, identity, graph);
        var interpolationCalls = InterpolationCalls.Read(module, body, graph, identity.DeclaringType, identity.Arguments);
        var staticSpans = StaticSpanCalls.Read(module, body, graph, identity.DeclaringType, identity.Arguments);
        var primitiveCalls = PrimitiveCalls.Discover(module, body, graph, identity);
        var invariantFormatting = InvariantFormattingCalls.Read(module, body, graph);
        var reachable = new HashSet<InstructionNode>();
        var pending = new Stack<InstructionNode>();
        pending.Push(graph.Entry);
        foreach (var region in body.ExceptionRegions) pending.Push(graph.Instructions.Single(n => n.Offset == region.HandlerOffset));
        while (pending.TryPop(out var instruction))
            if (reachable.Add(instruction))
                foreach (var successor in instruction.Successors) pending.Push(successor);
        var reader = body.Instructions;
        foreach (var instructionNode in graph.Instructions)
        {
            if (!reachable.Contains(instructionNode)) continue;
            int offset = instructionNode.Offset;
            if (graph.Specializations.TryGetValue(offset, out var specialized))
            {
                if (specialized.Kind == SpecializationKind.Int32)
                    Add(new Dependency.Intrinsic("Type.IsValueType"), node, DependencyReason.Call, offset);
                continue;
            }
            reader.Seek(offset);
            var instruction = reader.Read();
            var candidate = reader;
            if (rewrite?.Invoke(identity, offset, instruction, ref candidate) == true)
            {
                Add(new Dependency.Rewrite(identity, offset), node, DependencyReason.Rewrite, offset);
                continue;
            }
            switch (instruction.Code)
            {
                case OpCode.Call:
                case OpCode.Callvirt:
                case OpCode.Newobj:
                case OpCode.Ldftn:
                case OpCode.Ldvirtftn:
                    var token = MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value);
                    if (staticSpans.TryGetValue(offset, out var staticSpan))
                    {
                        Add(new Dependency.Intrinsic(staticSpan.PointerConstructor ? "ReadOnlySpan.StaticData" : "RuntimeHelpers.CreateSpan"), node, DependencyReason.Call, offset);
                        Add(new Dependency.Field(staticSpan.Field), node, DependencyReason.FieldRead, offset);
                        Signature(node, staticSpan.Result, DependencyReason.Signature, offset);
                        break;
                    }
                    if (primitiveCalls.TryGetValue(offset, out var primitive))
                    {
                        Add(new Dependency.Intrinsic(EnumInfo.Storage(primitive.Element).Code + "." + primitive.Operation), node, DependencyReason.Call, offset);
                        Signature(node, primitive.Element, DependencyReason.Signature, offset);
                        break;
                    }
                    if (instruction.Code is OpCode.Call or OpCode.Callvirt && DotNet.Intrinsics.ManagedType.Read(module, token) is { } managedType)
                    {
                        Add(new Dependency.Intrinsic("Type." + managedType.Operation), node, DependencyReason.Call, offset);
                        Signature(node, managedType.Signature.Result, DependencyReason.Signature, offset);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && DotNet.Intrinsics.CompositeFormatting.Read(module, token) is not null)
                    {
                        Add(new Dependency.Intrinsic("String.Format.Neutral"), node, DependencyReason.Call, offset);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && DotNet.Intrinsics.StringConcatenation.Read(module, token) is not null)
                    {
                        Add(new Dependency.Intrinsic("String.Concat"), node, DependencyReason.Call, offset);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && DotNet.Intrinsics.ResourceString.Read(module, token) is { } resourceText)
                    {
                        Add(new Dependency.Intrinsic("BCL.ResourceString"), node, DependencyReason.Call, offset);
                        Add(new Dependency.Literal(resourceText), node, DependencyReason.Literal, offset);
                        break;
                    }
                    if (invariantFormatting.ContainsKey(offset))
                    {
                        Add(new Dependency.Intrinsic("Int32.ToString.InvariantCulture"), node, DependencyReason.Call, offset);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && DotNet.Intrinsics.Int32Formatting.Is(module, token))
                    {
                        Add(new Dependency.Intrinsic("Int32.ToString"), node, DependencyReason.Call, offset);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && DotNet.Intrinsics.ConstructorFactory.Read(module, token, identity.DeclaringType.Arguments, identity.Arguments) is { } constructor)
                    {
                        Add(new Dependency.Intrinsic("Activator.CreateInstance"), node, DependencyReason.Call, offset);
                        Add(new Dependency.Method(constructor), node, DependencyReason.Call, offset);
                        Add(new Dependency.Instantiation(constructor.DeclaringType), node, DependencyReason.Allocation, offset);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && DotNet.Intrinsics.ArrayCopy.Read(module, token) is not null)
                    {
                        Add(new Dependency.Intrinsic("Array.Copy"), node, DependencyReason.Call, offset);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && DotNet.Intrinsics.MonotonicClock.Read(module, token) is { } clock)
                    {
                        Add(new Dependency.Intrinsic("Clock." + clock.Operation), node, DependencyReason.Call, offset);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && DotNet.Intrinsics.MathMethod.Read(module, token) is { } math)
                    {
                        Add(new Dependency.Intrinsic("Math." + math.Operation), node, DependencyReason.Call, offset);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && DotNet.Intrinsics.InlineArrayAccess.Read(module, token, identity.DeclaringType.Arguments, identity.Arguments) is { } inline)
                    {
                        Add(new Dependency.Intrinsic(inline.Operation), node, DependencyReason.Call, offset);
                        Signature(node, inline.Signature.Result, DependencyReason.Signature, offset);
                        Signature(node, inline.Signature.Parameters[0], DependencyReason.Signature, offset);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && DotNet.Intrinsics.SpanAccess.Read(module, token, identity.DeclaringType.Arguments, identity.Arguments) is { } span)
                    {
                        Add(new Dependency.Intrinsic($"Span.{span.Operation}"), node, DependencyReason.Call, offset);
                        Signature(node, span.Signature.Result, DependencyReason.Signature, offset);
                        break;
                    }
                    if ((interpolationCalls.GetValueOrDefault(offset) ?? (instruction.Code is OpCode.Call or OpCode.Newobj
                        ? DotNet.Intrinsics.InterpolatedStringHandler.Read(module, token, identity.DeclaringType.Arguments, identity.Arguments) : null)) is { } interpolation)
                    {
                        Add(new Dependency.Intrinsic("Interpolation." + interpolation.Operation), node, DependencyReason.Call, offset);
                        Add(new Dependency.Layout(interpolation.Owner), node, DependencyReason.Layout, offset);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && DotNet.Intrinsics.NullGuard.Is(module, token))
                    {
                        Add(new Dependency.Intrinsic("ArgumentNullException.ThrowIfNull"), node, DependencyReason.Call, offset);
                        break;
                    }
                    if (instruction.Code is OpCode.Call or OpCode.Callvirt && DotNet.Intrinsics.StringEquality.Read(module, token) is { } textEquality)
                    {
                        Add(new Dependency.Intrinsic("String." + textEquality.Operation), node, DependencyReason.Call, offset);
                        break;
                    }
                    if (instruction.Code is OpCode.Call or OpCode.Callvirt && DotNet.Intrinsics.DefaultEquality.Read(module, token, identity.DeclaringType.Arguments, identity.Arguments) is { } equality)
                    {
                        Add(new Dependency.Intrinsic("DefaultEquality." + equality.Operation), node, DependencyReason.Call, offset);
                        if (DotNet.Intrinsics.DefaultEquality.Is(equality.Owner))
                            Add(new Dependency.Layout(equality.Owner), node, DependencyReason.Layout, offset);
                        Signature(node, equality.Element, DependencyReason.Signature, offset);
                        if (equality.Operation is "Equals" or "IndexOf" && equality.Implementation is { } equals)
                            Add(new Dependency.Method(equals), node, DependencyReason.Call, offset);
                        if (equality.Operation is "Equals" or "IndexOf" && equality.UsesObjectEquality)
                            Add(new Dependency.Equality(equality.Element.Identity!.Value), node, DependencyReason.VirtualCall, offset);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && DotNet.Intrinsics.TimeSpanFactory.Read(module, token) is { } duration)
                    {
                        Add(new Dependency.Intrinsic("TimeSpan." + duration.Name), node, DependencyReason.Call, offset);
                        Signature(node, duration.Signature.Result, DependencyReason.Signature, offset);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && DotNet.Intrinsics.ManagedReference.Read(module, token, identity.DeclaringType.Arguments, identity.Arguments) is { } address)
                    {
                        Add(new Dependency.Intrinsic("Unsafe." + address.Operation), node, DependencyReason.Call, offset);
                        Signature(node, address.Element, DependencyReason.Signature, offset);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && DotNet.Intrinsics.ReferenceCompareExchange.Read(module, token, identity.DeclaringType.Arguments, identity.Arguments) is { } exchange)
                    {
                        Add(new Dependency.Intrinsic("Interlocked.CompareExchange.Reference"), node, DependencyReason.Call, offset);
                        Signature(node, exchange.Result, DependencyReason.Signature, offset);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && DotNet.Intrinsics.NativeFloat.TryMethod(module, token, out var nativeOperation, out _))
                    {
                        Add(new Dependency.Intrinsic($"NFloat.{nativeOperation}"), node, DependencyReason.Call, offset);
                        break;
                    }
                    if (DotNet.Exceptions.ExceptionTypes.TryMember(module, token, out var exceptionKind, out _))
                    {
                        Add(new Dependency.Intrinsic($"System.{exceptionKind}"), node, DependencyReason.Call, offset);
                        break;
                    }
                    if (instruction.Code == OpCode.Call && WellKnownMethods.IsObjectConstructor(module, token))
                    {
                        Add(new Dependency.Intrinsic("System.Object..ctor"), node, DependencyReason.Call, offset);
                        break;
                    }
                    if (DelegateInfo.Operation(module, token, identity) is { } delegateOperation)
                    {
                        Add(new Dependency.Intrinsic("Delegate." + delegateOperation), node, DependencyReason.Call, offset);
                        break;
                    }
                    if (DelegateInfo.Member(module, token, identity) is { } delegateMember)
                    {
                        string name = delegateMember.DeclaringType.Module.GetMethod(delegateMember.Handle).Name;
                        if (instruction.Code == OpCode.Newobj && name == ".ctor" || instruction.Code is OpCode.Call or OpCode.Callvirt && name == "Invoke")
                        {
                            Add(new Dependency.Signature(DelegateInfo.Invoke(delegateMember.DeclaringType)), node, DependencyReason.Signature, offset);
                            Add(new Dependency.Layout(delegateMember.DeclaringType), node, DependencyReason.Layout, offset);
                            Add(new Dependency.Intrinsic("Delegate." + name), node, DependencyReason.Call, offset);
                            if (instruction.Code == OpCode.Newobj) Add(new Dependency.Instantiation(delegateMember.DeclaringType), node, DependencyReason.Allocation, offset);
                            break;
                        }
                        throw new NotSupportedException($"Delegate member is not implemented: {delegateMember}.");
                    }
                    if (module.Compilation is null && token.Kind == HandleKind.MemberReference &&
                        module.GetMemberReference((MemberReferenceHandle)token).Parent.Kind is not
                            (HandleKind.TypeDefinition or HandleKind.TypeSpecification))
                        throw new NotSupportedException($"Unsupported {instruction.Code} at IL_{offset:x4}; only same-module method definitions or explicit rewrites are supported.");
                    MethodIdentity target;
                    try { target = MemberResolver.Method(module, token, identity.DeclaringType, identity.Arguments); }
                    catch (NotSupportedException error)
                    {
                        throw new NotSupportedException($"Unsupported {instruction.Code} at IL_{offset:x4} in {identity}: {error.Message}", error);
                    }
                    bool isVirtual = instruction.Code is OpCode.Callvirt or OpCode.Ldvirtftn &&
                        (target.DeclaringType.Module.GetMethod(target.Handle).Attributes & MethodAttributes.Virtual) != 0;
                    if (isVirtual && target.Arguments is not null && !target.DeclaringType.IsInterface) throw new NotSupportedException($"Generic virtual method dispatch is not implemented: {target}.");
                    Add(isVirtual ? new Dependency.VirtualCall(target) : new Dependency.Method(target), node,
                        isVirtual ? DependencyReason.VirtualCall : DependencyReason.Call, offset);
                    if (instruction.Code == OpCode.Newobj)
                        Add(new Dependency.Instantiation(target.DeclaringType), node, DependencyReason.Allocation, offset);
                    break;
                case OpCode.Ldfld:
                case OpCode.Stfld:
                case OpCode.Ldflda:
                case OpCode.Ldsfld:
                case OpCode.Stsfld:
                case OpCode.Ldsflda:
                    var fieldToken = MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value);
                    if (staticSpans.TryGetValue(offset, out var staticAddress))
                    {
                        Add(new Dependency.Field(staticAddress.Field), node, DependencyReason.FieldRead, offset); break;
                    }
                    if (instruction.Code == OpCode.Ldsfld && DotNet.Intrinsics.StopwatchConstant.Read(module, fieldToken) is { } clockConstant)
                    {
                        Add(new Dependency.Intrinsic("Stopwatch." + clockConstant.Name), node, DependencyReason.FieldRead, offset);
                        break;
                    }
                    if (instruction.Code == OpCode.Ldsfld && DotNet.Intrinsics.TimeSpanZero.Read(module, fieldToken, identity.DeclaringType) is { } zeroType)
                    {
                        Add(new Dependency.Intrinsic("TimeSpan.Zero"), node, DependencyReason.FieldRead, offset);
                        Signature(node, new(SignatureTypeCode.TypeHandle, zeroType), DependencyReason.FieldType, offset);
                        break;
                    }
                    var field = MemberResolver.Field(module, fieldToken, identity.DeclaringType, identity.Arguments);
                    var reason = instruction.Code switch
                    {
                        OpCode.Ldfld or OpCode.Ldsfld => DependencyReason.FieldRead,
                        OpCode.Stfld or OpCode.Stsfld => DependencyReason.FieldWrite,
                        _ => DependencyReason.FieldAddress
                    };
                    Add(new Dependency.Field(field), node, reason, offset);
                    if (instruction.Code is OpCode.Ldsfld or OpCode.Stsfld or OpCode.Ldsflda)
                        Add(new Dependency.Initialization(field.DeclaringType), node, DependencyReason.Initialization, offset);
                    break;
                case OpCode.Ldstr:
                    Add(new Dependency.Literal(module.GetString(MetadataTokens.UserStringHandle(instruction.ReadToken(ref reader).Value))),
                        node, DependencyReason.Literal, offset);
                    break;
                case OpCode.Box:
                case OpCode.Unbox:
                case OpCode.Unbox_any:
                case OpCode.Isinst:
                case OpCode.Castclass:
                case OpCode.Cpobj:
                case OpCode.Initobj:
                case OpCode.Ldobj:
                case OpCode.Stobj:
                case OpCode.Sizeof:
                case OpCode.Newarr:
                case OpCode.Ldelema:
                case OpCode.Ldelem:
                case OpCode.Stelem:
                case OpCode.Mkrefany:
                case OpCode.Refanyval:
                case OpCode.Constrained:
                    Signature(node, SignatureReader.Resolve(module, MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value),
                        identity.DeclaringType.Arguments, identity.Arguments), DependencyReason.TypeOperand, offset);
                    break;
                case OpCode.Ldtoken:
                    ReadHandle(node, MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value), offset);
                    break;
                case OpCode.Calli:
                case OpCode.Jmp:
                    throw new NotSupportedException($"Indirect call reachability is not implemented: {identity} at IL_{offset:x4}.");
                // Remaining opcodes carry no metadata dependencies; operands stay in the PE.
            }
        }
    }

    private void ReadDeclaration(Dependency node, MethodIdentity identity, bool retainBody)
    {
        var module = identity.DeclaringType.Module;
        var method = module.GetMethod(identity.Handle);
        if (DelegateInfo.IsRuntimeBase(identity.DeclaringType)) throw new NotSupportedException("This delegate base API is not implemented.");
        if (IsOpen(identity.DeclaringType) || (method.GenericParameters.Count != (identity.Arguments?.Values.Length ?? 0)) || identity.Arguments?.ContainsGenericParameters == true)
            throw new NotSupportedException($"Method reachability requires a closed declaring type and closed method arguments: {identity}.");
        if (retainBody) UseModule(module, node);
        Add(new Dependency.Type(identity.DeclaringType), node, DependencyReason.DeclaringType);
        // BeforeFieldInit permits deferring initialization until the first static field access.
        // Strict initializers remain required by calls and constructors.
        if (retainBody && TypeInitialization.AtMethodEntry(identity))
            Add(new Dependency.Initialization(identity.DeclaringType), node, DependencyReason.Initialization);
        var signature = SignatureReader.Method(module, method.Signature, identity.DeclaringType.Arguments, identity.Arguments);
        if (signature.HasThis)
        {
            Add(new Dependency.Layout(identity.DeclaringType), node, DependencyReason.Layout);
            if (!identity.DeclaringType.IsValueType) _ = dispatch.Resolve(identity.DeclaringType);
        }
        foreach (var parameter in signature.Parameters) Signature(node, parameter, DependencyReason.Signature);
        Signature(node, signature.Result, DependencyReason.Signature);
    }

    private void ReadHandle(Dependency.Method node, EntityHandle handle, int offset)
    {
        var module = node.Identity.DeclaringType.Module;
        switch (handle.Kind)
        {
            case HandleKind.TypeDefinition:
            case HandleKind.TypeReference:
            case HandleKind.TypeSpecification:
                Signature(node, SignatureReader.Resolve(module, handle, node.Identity.DeclaringType.Arguments, node.Identity.Arguments), DependencyReason.TypeOperand, offset);
                break;
            case HandleKind.FieldDefinition:
                Add(new Dependency.Field(MemberResolver.Field(module, handle, node.Identity.DeclaringType, node.Identity.Arguments)), node, DependencyReason.TypeOperand, offset);
                break;
            case HandleKind.MethodDefinition:
                Add(new Dependency.Method(MemberResolver.Method(module, handle, node.Identity.DeclaringType, node.Identity.Arguments)), node, DependencyReason.TypeOperand, offset);
                break;
            case HandleKind.MemberReference:
                if (module.GetMemberReference((MemberReferenceHandle)handle).Kind == MemberReferenceKind.Field)
                    Add(new Dependency.Field(MemberResolver.Field(module, handle, node.Identity.DeclaringType, node.Identity.Arguments)), node, DependencyReason.TypeOperand, offset);
                else Add(new Dependency.Method(MemberResolver.Method(module, handle, node.Identity.DeclaringType, node.Identity.Arguments)), node, DependencyReason.TypeOperand, offset);
                break;
            default: throw new NotSupportedException($"Metadata token reachability is not implemented: {handle.Kind}.");
        }
    }
}
