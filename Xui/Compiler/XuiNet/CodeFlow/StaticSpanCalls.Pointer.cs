using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Module = Xui.Compiler.XuiNet.DotNet.Module;

namespace Xui.Compiler.XuiNet.CodeFlow;

public static partial class StaticSpanCalls
{
    // Roslyn emits ldsflda/constant/newobj ReadOnlySpan(void*, int) for one-byte elements.
    // Recognize only this bounded sequence; general unmanaged pointers remain unsupported.
    private static void ReadPointerConstructors(Module module, MethodBody body, InstructionGraph graph, TypeIdentity context,
        GenericArguments? methodArguments, Dictionary<int, StaticSpanCall> result)
    {
        var incoming = graph.Instructions.SelectMany(n => n.Successors).ToLookup(n => n.Offset);
        var reader = body.Instructions;
        for (int i = 0; i + 2 < graph.Instructions.Count; i++)
        {
            var address = graph.Instructions[i]; var length = graph.Instructions[i + 1]; var call = graph.Instructions[i + 2];
            if (address.Code != OpCode.Ldsflda || call.Code != OpCode.Newobj || incoming[length.Offset].Count() != 1 || incoming[call.Offset].Count() != 1 ||
                address.Successors.Length != 1 || address.Successors[0] != length || length.Successors.Length != 1 || length.Successors[0] != call) continue;
            bool Boundary(int offset) => body.ExceptionRegions.Any(r => offset == r.TryOffset || offset == r.TryOffset + r.TryLength ||
                offset == r.HandlerOffset || offset == r.HandlerOffset + r.HandlerLength);
            if (Boundary(length.Offset) || Boundary(call.Offset)) continue;
            reader.Seek(length.Offset);
            var constant = reader.Read();
            int? count = constant.Code switch {
                OpCode.Ldc_i4 => constant.ReadInt(ref reader).Value,
                OpCode.Ldc_i4_s => constant.ReadShortInt(ref reader).Value,
                OpCode.Ldc_i4_m1 => -1,
                >= OpCode.Ldc_i4_0 and <= OpCode.Ldc_i4_8 => constant.Code - OpCode.Ldc_i4_0,
                _ => null
            };
            if (count is null) continue;
            reader.Seek(call.Offset);
            var token = MetadataTokens.EntityHandle(reader.Read().ReadToken(ref reader).Value);
            if (token.Kind != HandleKind.MemberReference) continue;
            var member = module.GetMemberReference((MemberReferenceHandle)token);
            if (member.Kind != MemberReferenceKind.Method || member.Name != ".ctor" || member.Parent.Kind != HandleKind.TypeSpecification) continue;
            var spanType = SignatureReader.Resolve(module, member.Parent, context.Arguments, methodArguments);
            if (spanType.Identity is not { } span || !FrameworkTypes.Is(span, "System", "ReadOnlySpan`1") || span.Arguments?.Values.Length != 1) continue;
            var signature = member.Signature;
            if (signature.ReadSignatureHeader().RawValue != (byte)SignatureAttributes.Instance || signature.ReadCompressedInteger() != 2 ||
                signature.ReadSignatureTypeCode() != SignatureTypeCode.Void || signature.ReadSignatureTypeCode() != SignatureTypeCode.Pointer ||
                signature.ReadSignatureTypeCode() != SignatureTypeCode.Void || signature.ReadSignatureTypeCode() != SignatureTypeCode.Int32 || signature.RemainingBytes != 0) continue;
            int size = Size(EnumInfo.Storage(span.Arguments.Values[0]));
            if (size == 0) throw new NotSupportedException("Static spans require fixed-width primitive or enum elements.");
            reader.Seek(address.Offset);
            var field = MetadataTokens.EntityHandle(reader.Read().ReadToken(ref reader).Value);
            var access = Describe(module, field, context, methodArguments, spanType, size, address.Offset, count);
            result.Add(address.Offset, access); result.Add(call.Offset, access);
        }
    }
}
