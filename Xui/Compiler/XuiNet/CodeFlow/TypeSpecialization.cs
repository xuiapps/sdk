using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Module = Xui.Compiler.XuiNet.DotNet.Module;
using MethodBody = Xui.Compiler.XuiNet.DotNet.MethodBody;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Folds adjacent typeof(T).IsValueType queries and proven boolean branches for closed methods.</summary>
/// <remarks>Unknown values remain unknown. No calls or stores are removed based on guessed runtime values.</remarks>
public static class TypeSpecialization
{
    public static void Apply(Module module, MethodBody body, MethodIdentity method, InstructionGraph graph)
    {
        var nodes = graph.Instructions;
        var incoming = nodes.SelectMany(n => n.Successors).ToLookup(n => n);
        var boundaries = body.ExceptionRegions.SelectMany(r => new[] { r.TryOffset, r.TryOffset + r.TryLength, r.HandlerOffset, r.HandlerOffset + r.HandlerLength }).ToHashSet();
        bool Adjacent(int i) => i > 0 && incoming[nodes[i]].Count() == 1 && nodes[i - 1].Successors.Length == 1 &&
            nodes[i - 1].Successors[0] == nodes[i] && !boundaries.Contains(nodes[i].Offset);
        EntityHandle Token(int i)
        {
            var reader = body.Instructions; reader.Seek(nodes[i].Offset);
            return MetadataTokens.EntityHandle(reader.Read().ReadToken(ref reader).Value);
        }
        for (int i = 0; i + 2 < nodes.Count; i++)
        {
            if (nodes[i].Code != OpCode.Ldtoken || nodes[i + 1].Code != OpCode.Call ||
                nodes[i + 2].Code is not (OpCode.Call or OpCode.Callvirt) || !Adjacent(i + 1) || !Adjacent(i + 2) ||
                ManagedType.Read(module, Token(i + 1))?.Operation != ManagedTypeOperation.FromHandle || !IsValueTypeGetter(module, Token(i + 2))) continue;
            var type = SignatureReader.StorageType(module, Token(i), method.DeclaringType.Arguments, method.Arguments);
            bool? value = type.ContainsGenericParameters ? null : type.Identity is { } identity
                ? identity.Handle.Kind == HandleKind.TypeDefinition ? identity.IsValueType : null
                : type.Code is SignatureTypeCode.Object or SignatureTypeCode.String or SignatureTypeCode.SZArray ? false
                : type.Code is >= SignatureTypeCode.Boolean and <= SignatureTypeCode.Double or SignatureTypeCode.IntPtr or SignatureTypeCode.UIntPtr ? true : null;
            if (value is null) continue;
            Replace(i, SpecializationKind.Omit); Replace(i + 1, SpecializationKind.Omit);
            Replace(i + 2, SpecializationKind.Int32, value.Value ? 1 : 0);
            FoldBranch(i + 2, value.Value ? 1 : 0);
        }

        void Replace(int i, SpecializationKind kind, int value = 0) => graph.Specialized[nodes[i].Offset] = new(kind, nodes[i].End, value);
        void FoldBranch(int start, int value)
        {
            var values = new Stack<int?>(); values.Push(value);
            for (int i = start + 1; i < nodes.Count && Adjacent(i); i++)
            {
                var node = nodes[i];
                var reader = body.Instructions; reader.Seek(node.Offset); var instruction = reader.Read();
                switch (node.Code)
                {
                    case OpCode.Nop: break;
                    case >= OpCode.Ldc_i4_m1 and <= OpCode.Ldc_i4_8:
                        values.Push((int)node.Code - (int)OpCode.Ldc_i4_0); break;
                    case OpCode.Ldc_i4: values.Push(instruction.ReadInt(ref reader).Value); break;
                    case OpCode.Ldc_i4_s: values.Push(instruction.ReadShortInt(ref reader).Value); break;
                    case >= OpCode.Ldarg_0 and <= OpCode.Ldarg_3:
                    case OpCode.Ldarg: case OpCode.Ldarg_s:
                    case >= OpCode.Ldloc_0 and <= OpCode.Ldloc_3:
                    case OpCode.Ldloc: case OpCode.Ldloc_s: values.Push(null); break;
                    case OpCode.Ceq: case OpCode.And: case OpCode.Or: case OpCode.Xor:
                        if (values.Count < 2) return;
                        var right = values.Pop(); var left = values.Pop();
                        values.Push(node.Code switch {
                            OpCode.And when left == 0 || right == 0 => 0,
                            OpCode.Or when left == -1 || right == -1 => -1,
                            _ when left is null || right is null => null,
                            OpCode.Ceq => left == right ? 1 : 0,
                            OpCode.And => left & right, OpCode.Or => left | right, _ => left ^ right });
                        break;
                    case OpCode.Brtrue: case OpCode.Brtrue_s: case OpCode.Brfalse: case OpCode.Brfalse_s:
                        if (values.Count == 0 || values.Pop() is not { } condition || node.Successors.Length != 2) return;
                        bool taken = node.Code is OpCode.Brtrue or OpCode.Brtrue_s ? condition != 0 : condition == 0;
                        node.Successors = [node.Successors[taken ? 1 : 0]];
                        Replace(i, SpecializationKind.Branch);
                        return;
                    default: return;
                }
            }
        }
    }

    private static bool IsValueTypeGetter(Module module, EntityHandle token)
    {
        TypeIdentity owner; string name; BlobReader blob;
        if (token.Kind == HandleKind.MemberReference)
        {
            var member = module.GetMemberReference((MemberReferenceHandle)token);
            if (member.Kind != MemberReferenceKind.Method || member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition)) return false;
            owner = new(module, member.Parent); name = member.Name; blob = member.Signature;
        }
        else if (token.Kind == HandleKind.MethodDefinition)
        {
            var definition = module.GetMethod((MethodDefinitionHandle)token);
            owner = new(module, definition.DeclaringType); name = definition.Name; blob = definition.Signature;
        }
        else return false;
        if (name != "get_IsValueType" || !ManagedType.Is(owner)) return false;
        var signature = SignatureReader.Method(module, blob);
        return signature.HasThis && signature.GenericParameterCount == 0 && signature.Parameters.Length == 0 && signature.Result.Code == SignatureTypeCode.Boolean;
    }
}
