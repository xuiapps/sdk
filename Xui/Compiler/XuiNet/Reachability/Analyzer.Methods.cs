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
        if (IsOpen(identity.DeclaringType) || method.GenericParameters.Count != 0)
            throw new NotSupportedException($"Method reachability requires a closed declaring type and a non-generic method: {identity}.");
        UseModule(module, node);
        Add(new Dependency.Type(identity.DeclaringType), node, DependencyReason.DeclaringType);
        if (method.Name != ".cctor")
            Add(new Dependency.Initialization(identity.DeclaringType), node, DependencyReason.Initialization);
        var signature = SignatureReader.Method(module, method.Signature, identity.DeclaringType.Arguments);
        if (signature.HasThis)
        {
            Add(new Dependency.Layout(identity.DeclaringType), node, DependencyReason.Layout);
            if (!identity.DeclaringType.IsValueType) _ = dispatch.Resolve(identity.DeclaringType);
        }
        foreach (var parameter in signature.Parameters) Signature(node, parameter, DependencyReason.Signature);
        Signature(node, signature.Result, DependencyReason.Signature);
        if ((method.Attributes & MethodAttributes.Abstract) != 0) return;
        var body = method.Body ?? throw new NotSupportedException($"The method has no managed body: {identity}.");
        if (body.ExceptionRegions.Length != 0)
            throw new NotSupportedException($"Exception-handler reachability is not implemented: {identity}.");
        System.Collections.Immutable.ImmutableArray<SignatureType> locals;
        try { locals = SignatureReader.Locals(module, body.LocalSignature, identity.DeclaringType.Arguments); }
        catch (NotSupportedException error)
        {
            throw new NotSupportedException($"Unsupported local type in {identity}: {error.Message}", error);
        }
        foreach (var local in locals)
            Signature(node, local, DependencyReason.Local);

        var graph = InstructionGraph.Read(body.Instructions);
        var reachable = new HashSet<InstructionNode>();
        var pending = new Stack<InstructionNode>();
        pending.Push(graph.Entry);
        while (pending.TryPop(out var instruction))
            if (reachable.Add(instruction))
                foreach (var successor in instruction.Successors) pending.Push(successor);
        var reader = body.Instructions;
        foreach (var instructionNode in graph.Instructions)
        {
            if (!reachable.Contains(instructionNode)) continue;
            int offset = instructionNode.Offset;
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
                    if (instruction.Code == OpCode.Call && WellKnownMethods.IsObjectConstructor(module, token))
                    {
                        Add(new Dependency.Intrinsic("System.Object..ctor"), node, DependencyReason.Call, offset);
                        break;
                    }
                    if (token.Kind == HandleKind.MemberReference &&
                        module.GetMemberReference((MemberReferenceHandle)token).Parent.Kind is not
                            (HandleKind.TypeDefinition or HandleKind.TypeSpecification))
                        throw new NotSupportedException($"Unsupported {instruction.Code} at IL_{offset:x4}; only same-module method definitions or explicit rewrites are supported.");
                    MethodIdentity target;
                    try { target = MemberResolver.Method(module, token, identity.DeclaringType); }
                    catch (NotSupportedException error)
                    {
                        throw new NotSupportedException($"Unsupported {instruction.Code} at IL_{offset:x4} in {identity}: {error.Message}", error);
                    }
                    bool isVirtual = instruction.Code is OpCode.Callvirt or OpCode.Ldvirtftn &&
                        (module.GetMethod(target.Handle).Attributes & MethodAttributes.Virtual) != 0;
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
                    var field = MemberResolver.Field(module, MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value), identity.DeclaringType);
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
                        identity.DeclaringType.Arguments), DependencyReason.TypeOperand, offset);
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

    private void ReadHandle(Dependency.Method node, EntityHandle handle, int offset)
    {
        var module = node.Identity.DeclaringType.Module;
        switch (handle.Kind)
        {
            case HandleKind.TypeDefinition:
            case HandleKind.TypeReference:
            case HandleKind.TypeSpecification:
                Signature(node, SignatureReader.Resolve(module, handle, node.Identity.DeclaringType.Arguments), DependencyReason.TypeOperand, offset);
                break;
            case HandleKind.FieldDefinition:
                Add(new Dependency.Field(MemberResolver.Field(module, handle, node.Identity.DeclaringType)), node, DependencyReason.TypeOperand, offset);
                break;
            case HandleKind.MethodDefinition:
                Add(new Dependency.Method(MemberResolver.Method(module, handle, node.Identity.DeclaringType)), node, DependencyReason.TypeOperand, offset);
                break;
            case HandleKind.MemberReference:
                if (module.GetMemberReference((MemberReferenceHandle)handle).Kind == MemberReferenceKind.Field)
                    Add(new Dependency.Field(MemberResolver.Field(module, handle, node.Identity.DeclaringType)), node, DependencyReason.TypeOperand, offset);
                else Add(new Dependency.Method(MemberResolver.Method(module, handle, node.Identity.DeclaringType)), node, DependencyReason.TypeOperand, offset);
                break;
            default: throw new NotSupportedException($"Metadata token reachability is not implemented: {handle.Kind}.");
        }
    }
}
