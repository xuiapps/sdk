using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A field definition in its declaring type's specialization context.</summary>
public readonly record struct FieldIdentity(
    TypeIdentity DeclaringType,
    FieldDefinitionHandle Handle);
