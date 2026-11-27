using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Module = Xui.Compiler.XuiNet.DotNet.Module;
using MethodBody = Xui.Compiler.XuiNet.DotNet.MethodBody;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Folds closed type/reference-content queries and proven boolean branches.</summary>
/// <remarks>Unknown values remain unknown. No calls or stores are removed based on guessed runtime values.</remarks>
public static partial class TypeSpecialization
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
        if (method.ConstantBoolean is { } argument)
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node.Code is not (>= OpCode.Ldarg_0 and <= OpCode.Ldarg_3 or OpCode.Ldarg or OpCode.Ldarg_s)) continue;
                var reader = body.Instructions; reader.Seek(node.Offset); var instruction = reader.Read();
                int index = node.Code switch {
                    >= OpCode.Ldarg_0 and <= OpCode.Ldarg_3 => (int)node.Code - (int)OpCode.Ldarg_0,
                    OpCode.Ldarg_s => instruction.ReadShortVariable(ref reader).Index,
                    _ => instruction.ReadVariable(ref reader).Index
                };
                if (index == argument.Index) Replace(i, SpecializationKind.Int32, argument.Value ? 1 : 0);
            }
        var layouts = new Layout.LayoutResolver(module, Layout.TargetLayout.Wasm32);
        for (int i = 0; i + 1 < nodes.Count; i++)
        {
            // Boxing a non-nullable value solely to test for null needs no object.
            // Preserve the producer: it may call user code or mutate storage.
            if (nodes[i].Code != OpCode.Box || !Adjacent(i + 1)) continue;
            bool branch = nodes[i + 1].Code is OpCode.Brtrue or OpCode.Brtrue_s or OpCode.Brfalse or OpCode.Brfalse_s;
            bool comparison = i + 2 < nodes.Count && nodes[i + 1].Code == OpCode.Ldnull && Adjacent(i + 2) &&
                nodes[i + 2].Code is OpCode.Ceq or OpCode.Cgt_un;
            if (!branch && !comparison) continue;
            var type = SignatureReader.StorageType(module, Token(i), method.DeclaringType.Arguments, method.Arguments);
            if (type.ContainsGenericParameters || type.IsByReference || type.Identity is not { IsValueType: true } identity ||
                FrameworkTypes.Is(identity, "System", "Nullable`1")) continue;
            int value = comparison && nodes[i + 2].Code == OpCode.Ceq ? 0 : 1;
            Replace(i, SpecializationKind.DiscardAndInt32, value);
            if (comparison) { Replace(i + 1, SpecializationKind.Omit); Replace(i + 2, SpecializationKind.Omit); }
            FoldBranch(comparison ? i + 2 : i, value);
        }
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].Code != OpCode.Call || ReferenceContent.Read(module, Token(i), method.DeclaringType.Arguments, method.Arguments) is not { } type) continue;
            if (type.ContainsGenericParameters) throw new NotSupportedException("Reference-content queries require a closed type.");
            bool value = type.IsReferenceType || type.Code is SignatureTypeCode.String or SignatureTypeCode.Object or SignatureTypeCode.SZArray or SignatureTypeCode.Array;
            if (!value && type.Identity is { } identity && !NativeFloat.Is(identity))
            {
                if (identity.Handle.Kind != HandleKind.TypeDefinition) throw new NotSupportedException($"Reference-content classification requires a supplied value type: {identity}.");
                value = layouts.Classify(identity) == Layout.StorageKind.ContainsManagedReferences;
            }
            graph.Specialized[nodes[i].Offset] = new(SpecializationKind.Int32, nodes[i].End, value ? 1 : 0, "RuntimeHelpers.IsReferenceOrContainsReferences");
            FoldBranch(i, value ? 1 : 0);
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

        // A sealed reference element or a non-enum named value element admits no array
        // covariance/reduced-integer alias. Preserve GetType's null failure while folding
        // typeof(T[]) == values.GetType(), as used by CoreLib's range-slice helper.
        for (int i = 0; i + 4 < nodes.Count; i++)
        {
            if (nodes[i].Code != OpCode.Ldtoken || nodes[i + 1].Code != OpCode.Call ||
                nodes[i + 3].Code != OpCode.Callvirt || nodes[i + 4].Code != OpCode.Call ||
                !Adjacent(i + 1) || !Adjacent(i + 2) || !Adjacent(i + 3) || !Adjacent(i + 4) ||
                ManagedType.Read(module, Token(i + 1))?.Operation != ManagedTypeOperation.FromHandle ||
                ManagedType.Read(module, Token(i + 3))?.Operation != ManagedTypeOperation.GetType) continue;
            var operation = ManagedType.Read(module, Token(i + 4))?.Operation;
            if (operation is not (ManagedTypeOperation.Equal or ManagedTypeOperation.NotEqual)) continue;
            var token = SignatureReader.StorageType(module, Token(i), method.DeclaringType.Arguments, method.Arguments);
            var producer = ProducedType(module, body, method, nodes[i + 2]);
            if (token.Array is not { IsVector: true } array || producer?.Array != array || !ExactArrayElement(array.Element)) continue;
            int result = operation == ManagedTypeOperation.Equal ? 1 : 0;
            Replace(i, SpecializationKind.Omit); Replace(i + 1, SpecializationKind.Omit);
            Replace(i + 3, SpecializationKind.NullCheckAndInt32, result);
            Replace(i + 4, SpecializationKind.Omit);
            FoldBranch(i + 4, result);
        }

        for (int i = 0; i + 4 < nodes.Count; i++)
        {
            if (nodes[i].Code != OpCode.Ldtoken || nodes[i + 1].Code != OpCode.Call || nodes[i + 2].Code != OpCode.Ldtoken ||
                nodes[i + 3].Code != OpCode.Call || nodes[i + 4].Code != OpCode.Call ||
                !Adjacent(i + 1) || !Adjacent(i + 2) || !Adjacent(i + 3) || !Adjacent(i + 4) ||
                ManagedType.Read(module, Token(i + 1))?.Operation != ManagedTypeOperation.FromHandle ||
                ManagedType.Read(module, Token(i + 3))?.Operation != ManagedTypeOperation.FromHandle) continue;
            var operation = ManagedType.Read(module, Token(i + 4))?.Operation;
            if (operation is not (ManagedTypeOperation.Equal or ManagedTypeOperation.NotEqual)) continue;
            var left = SignatureReader.StorageType(module, Token(i), method.DeclaringType.Arguments, method.Arguments);
            var right = SignatureReader.StorageType(module, Token(i + 2), method.DeclaringType.Arguments, method.Arguments);
            if (left.ContainsGenericParameters || right.ContainsGenericParameters) continue;
            int result = (operation == ManagedTypeOperation.Equal ? left == right : left != right) ? 1 : 0;
            for (int j = 0; j < 4; j++) Replace(i + j, SpecializationKind.Omit);
            Replace(i + 4, SpecializationKind.Int32, result);
            FoldBranch(i + 4, result);
        }

        for (int i = 1; i < nodes.Count; i++)
        {
            if (nodes[i].Code != OpCode.Isinst || !Adjacent(i)) continue;
            var target = SignatureReader.Resolve(module, Token(i), method.DeclaringType.Arguments, method.Arguments);
            if (target.Identity is { } native && NativeStringComparer.IsWrapper(native))
            {
                Replace(i, SpecializationKind.NullTypeTest);
                FoldBranch(i, 0);
                continue;
            }
            var source = ProducedType(module, body, method, nodes[i - 1]);
            if (source?.Identity is not { IsInterface: true } contract || target.Identity is not { } tested ||
                !ClosedTypeTests.Disjoint(contract, tested)) continue;
            Replace(i, SpecializationKind.NullTypeTest);
            FoldBranch(i, 0);
        }

        if (method.ConstantBoolean is not null)
            for (int i = 0; i < nodes.Count; i++)
                if (graph.Specializations.TryGetValue(nodes[i].Offset, out var known) && known.Kind == SpecializationKind.Int32)
                    FoldBranch(i, known.Value);

        void Replace(int i, SpecializationKind kind, int value = 0) => graph.Specialized[nodes[i].Offset] = new(kind, nodes[i].End, value);
        void FoldBranch(int start, int value)
        {
            var values = new Stack<int?>(); values.Push(value);
            var locals = new Dictionary<int, int?>();
            for (int i = start + 1; i < nodes.Count && Adjacent(i); i++)
            {
                var node = nodes[i];
                var reader = body.Instructions; reader.Seek(node.Offset); var instruction = reader.Read();
                if (graph.Specializations.TryGetValue(node.Offset, out var known) && known.Kind == SpecializationKind.Int32)
                { values.Push(known.Value); continue; }
                switch (node.Code)
                {
                    case OpCode.Nop: break;
                    case OpCode.Ldnull: values.Push(0); break;
                    case >= OpCode.Ldc_i4_m1 and <= OpCode.Ldc_i4_8:
                        values.Push((int)node.Code - (int)OpCode.Ldc_i4_0); break;
                    case OpCode.Ldc_i4: values.Push(instruction.ReadInt(ref reader).Value); break;
                    case OpCode.Ldc_i4_s: values.Push(instruction.ReadShortInt(ref reader).Value); break;
                    case >= OpCode.Ldarg_0 and <= OpCode.Ldarg_3:
                    case OpCode.Ldarg: case OpCode.Ldarg_s: values.Push(null); break;
                    case >= OpCode.Ldloc_0 and <= OpCode.Ldloc_3:
                    case OpCode.Ldloc: case OpCode.Ldloc_s:
                    case >= OpCode.Stloc_0 and <= OpCode.Stloc_3:
                    case OpCode.Stloc: case OpCode.Stloc_s:
                        int index = node.Code switch {
                            >= OpCode.Ldloc_0 and <= OpCode.Ldloc_3 => (int)node.Code - (int)OpCode.Ldloc_0,
                            >= OpCode.Stloc_0 and <= OpCode.Stloc_3 => (int)node.Code - (int)OpCode.Stloc_0,
                            OpCode.Ldloc_s or OpCode.Stloc_s => instruction.ReadShortVariable(ref reader).Index,
                            _ => instruction.ReadVariable(ref reader).Index
                        };
                        if (node.Code is >= OpCode.Stloc_0 and <= OpCode.Stloc_3 or OpCode.Stloc or OpCode.Stloc_s)
                        { if (values.Count == 0) return; locals[index] = values.Pop(); }
                        else values.Push(locals.GetValueOrDefault(index));
                        break;
                    case OpCode.Ceq: case OpCode.Cgt_un: case OpCode.And: case OpCode.Or: case OpCode.Xor:
                        if (values.Count < 2) return;
                        var right = values.Pop(); var left = values.Pop();
                        values.Push(node.Code switch {
                            OpCode.And when left == 0 || right == 0 => 0,
                            OpCode.Or when left == -1 || right == -1 => -1,
                            _ when left is null || right is null => null,
                            OpCode.Ceq => left == right ? 1 : 0,
                            OpCode.Cgt_un => (uint)left.Value > (uint)right.Value ? 1 : 0,
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

    private static bool ExactArrayElement(SignatureType element)
    {
        if (element.ContainsGenericParameters || element.IsByReference) return false;
        if (element.Code == SignatureTypeCode.String) return true;
        if (element.Identity is not { Handle.Kind: HandleKind.TypeDefinition } identity || EnumInfo.UnderlyingType(identity) is not null) return false;
        return identity.IsValueType || (identity.Module.GetType((TypeDefinitionHandle)identity.Handle).Attributes & System.Reflection.TypeAttributes.Sealed) != 0;
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
