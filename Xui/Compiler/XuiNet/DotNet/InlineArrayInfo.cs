using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>The PE-declared repeated element of a C# inline array, in its closed generic context.</summary>
public readonly record struct InlineArrayInfo(
    int Length,
    FieldIdentity Field,
    SignatureType Element)
{
    /// <summary>Reads InlineArrayAttribute without creating a runtime type or instantiating the attribute.</summary>
    public static InlineArrayInfo? Read(TypeIdentity identity)
    {
        if (identity.Handle.Kind != HandleKind.TypeDefinition) return null;
        var module = identity.Module;
        var definition = module.Metadata.GetTypeDefinition((TypeDefinitionHandle)identity.Handle);
        foreach (var handle in definition.GetCustomAttributes())
        {
            var attribute = module.Metadata.GetCustomAttribute(handle);
            EntityHandle owner = attribute.Constructor.Kind switch
            {
                HandleKind.MemberReference => module.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent,
                HandleKind.MethodDefinition => module.GetMethod((MethodDefinitionHandle)attribute.Constructor).DeclaringType,
                _ => default
            };
            if (owner.IsNil || !FrameworkTypes.Is(new(module, owner), "System.Runtime.CompilerServices", "InlineArrayAttribute")) continue;
            var value = module.Metadata.GetBlobReader(attribute.Value);
            if (value.ReadUInt16() != 1) throw new BadImageFormatException("Invalid InlineArrayAttribute prolog.");
            int count = value.ReadInt32();
            var fields = definition.GetFields().Where(f => (module.GetField(f).Attributes & FieldAttributes.Static) == 0).ToArray();
            if (count <= 0 || fields.Length != 1 || !identity.IsValueType ||
                (definition.Attributes & TypeAttributes.LayoutMask) == TypeAttributes.ExplicitLayout)
                throw new BadImageFormatException($"Invalid inline array declaration: {identity}.");
            return new(count, new(identity, fields[0]), SignatureReader.Field(module, module.GetField(fields[0]).Signature, identity.Arguments));
        }
        return null;
    }
}
