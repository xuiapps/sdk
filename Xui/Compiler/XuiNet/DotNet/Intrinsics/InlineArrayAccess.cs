using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Recognizes Roslyn's closed inline-array access helpers, without enabling arbitrary Unsafe calls.</summary>
public sealed record InlineArrayAccess(
    string Operation,
    TypeIdentity Buffer,
    InlineArrayInfo Layout,
    MethodSignature Signature)
{
    public static InlineArrayAccess? Read(Module module, EntityHandle token, GenericArguments? context)
    {
        if (token.Kind != HandleKind.MethodSpecification) return null;
        var spec = module.Metadata.GetMethodSpecification((MethodSpecificationHandle)token);
        if (spec.Method.Kind != HandleKind.MethodDefinition) return null;
        var method = module.GetMethod((MethodDefinitionHandle)spec.Method);
        if (method.Name is not ("InlineArrayFirstElementRef" or "InlineArrayElementRef" or "InlineArrayAsSpan" or "InlineArrayAsReadOnlySpan")) return null;
        var owner = module.Metadata.GetTypeDefinition(method.DeclaringType);
        if (module.Metadata.GetString(owner.Name) != "<PrivateImplementationDetails>" || !owner.GetCustomAttributes().Any(h =>
        {
            var attr = module.Metadata.GetCustomAttribute(h);
            return attr.Constructor.Kind == HandleKind.MemberReference && FrameworkTypes.Is(new(module,
                module.GetMemberReference((MemberReferenceHandle)attr.Constructor).Parent), "System.Runtime.CompilerServices", "CompilerGeneratedAttribute");
        })) return null;
        var args = module.Metadata.GetBlobReader(spec.Signature);
        if (args.ReadSignatureHeader().Kind != SignatureKind.MethodSpecification || args.ReadCompressedInteger() != 2) return null;
        var buffer = SignatureReader.Type(module, ref args, context);
        var element = SignatureReader.Type(module, ref args, context);
        if (args.RemainingBytes != 0 || buffer.Identity is not { } identity || buffer.IsByReference ||
            InlineArrayInfo.Read(identity) is not { } layout || layout.Element != element) return null;
        var signature = method.Signature;
        var header = signature.ReadSignatureHeader();
        if (header.IsInstance || !header.IsGeneric || signature.ReadCompressedInteger() != 2) return null;
        int count = signature.ReadCompressedInteger();
        var result = Substitute(SignatureReader.Type(module, ref signature));
        var parameters = new SignatureType[count];
        for (int i = 0; i < count; i++) parameters[i] = Substitute(SignatureReader.Type(module, ref signature));
        bool first = method.Name == "InlineArrayFirstElementRef";
        if (signature.RemainingBytes != 0 || count != (first ? 1 : 2) || parameters[0] != buffer.ByReference() ||
            !first && parameters[1].Code != SignatureTypeCode.Int32) return null;
        if (method.Name is "InlineArrayFirstElementRef" or "InlineArrayElementRef")
        {
            if (result != element.ByReference()) return null;
        }
        else if (result.Identity is not { } span || !SpanAccess.Is(span) || span.Arguments?.Values[0] != element) return null;
        return new(method.Name, identity, layout, new([.. parameters], result, false));

        SignatureType Substitute(SignatureType value)
        {
            if (value.Code == SignatureTypeCode.GenericMethodParameter)
            {
                var replaced = value.GenericParameterIndex switch { 0 => buffer, 1 => element, _ => throw new BadImageFormatException("Inline-array helper type argument is out of range.") };
                return value.IsByReference ? replaced.ByReference() : replaced;
            }
            if (value.Identity is { Arguments: { } nested } id)
                return value with { Identity = id with { Arguments = new(nested.Values.Select(Substitute)) } };
            return value;
        }
    }
}
