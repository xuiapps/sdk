using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

/// <summary>A value type, including references to types defined in this module.</summary>
public readonly struct ValueType
{
    private readonly ValueTypeCode code;
    private readonly long heapType;

    private ValueType(ValueTypeCode code, long heapType = 0)
    {
        this.code = code;
        this.heapType = heapType;
    }

    /// <summary>Creates a nullable reference to a type definition.</summary>
    public static ValueType RefNull(TypeIndex typeIndex) => new(ValueTypeCode.RefNull, typeIndex.Value);

    /// <summary>Creates a non-null reference to a type definition.</summary>
    public static ValueType Ref(TypeIndex typeIndex) => new(ValueTypeCode.Ref, typeIndex.Value);

    /// <summary>A non-null host reference, such as a string returned by a JS builtin.</summary>
    public static ValueType NonNullExternRef => new(ValueTypeCode.Ref, (byte)ValueTypeCode.ExternRef - 0x80);

    /// <summary>Converts a single-byte value type into a signature value type.</summary>
    public static implicit operator ValueType(ValueTypeCode code) => new(code);

    internal void WriteTo(Section.Writer writer)
    {
        writer.Byte((byte)code);
        if (code is ValueTypeCode.Ref or ValueTypeCode.RefNull)
            writer.Signed(heapType);
    }
}
