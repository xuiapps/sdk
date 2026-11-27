using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Module = Xui.Compiler.XuiNet.DotNet.Module;
using MethodBody = Xui.Compiler.XuiNet.DotNet.MethodBody;

namespace Xui.Compiler.XuiNet.CodeFlow;

public static partial class TypeSpecialization
{
    private static SignatureType? ProducedType(Module module, MethodBody body, MethodIdentity method, InstructionNode node)
    {
        var reader = body.Instructions; reader.Seek(node.Offset); var instruction = reader.Read();
        switch (node.Code)
        {
            case >= OpCode.Ldarg_0 and <= OpCode.Ldarg_3:
            case OpCode.Ldarg: case OpCode.Ldarg_s:
                int argument = node.Code switch {
                    >= OpCode.Ldarg_0 and <= OpCode.Ldarg_3 => (int)node.Code - (int)OpCode.Ldarg_0,
                    OpCode.Ldarg_s => instruction.ReadShortVariable(ref reader).Index,
                    _ => instruction.ReadVariable(ref reader).Index
                };
                return Signature.Read(module, module.GetMethod(method.Handle).Signature, method.DeclaringType, method.Arguments).Parameters[argument];
            case >= OpCode.Ldloc_0 and <= OpCode.Ldloc_3:
            case OpCode.Ldloc: case OpCode.Ldloc_s:
                int local = node.Code switch {
                    >= OpCode.Ldloc_0 and <= OpCode.Ldloc_3 => (int)node.Code - (int)OpCode.Ldloc_0,
                    OpCode.Ldloc_s => instruction.ReadShortVariable(ref reader).Index,
                    _ => instruction.ReadVariable(ref reader).Index
                };
                return SignatureReader.Locals(module, body.LocalSignature, method.DeclaringType.Arguments, method.Arguments)[local];
            case OpCode.Ldfld: case OpCode.Ldsfld:
                var field = MemberResolver.Field(module, MetadataTokens.EntityHandle(instruction.ReadToken(ref reader).Value), method.DeclaringType, method.Arguments);
                return SignatureReader.Field(field.DeclaringType.Module, field.DeclaringType.Module.GetField(field.Handle).Signature, field.DeclaringType.Arguments);
            default: return null;
        }
    }
}
