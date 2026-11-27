using System.Reflection.Metadata;
using System.Text.Json;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.WASM.Wasm.Indices;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class FormattingRuntime
{
    private readonly Dictionary<TypeIdentity, string> enums = new();
    private TypeIndex enumType;
    public FunctionIndex Enum { get; private set; }

    public string RequireEnum(TypeIdentity identity)
    {
        if (enums.TryGetValue(identity, out var descriptor)) return descriptor;
        var code = EnumInfo.UnderlyingType(identity) ?? throw new ArgumentException("Expected an enum.");
        int width = code switch {
            SignatureTypeCode.Byte or SignatureTypeCode.SByte => 8,
            SignatureTypeCode.Int16 or SignatureTypeCode.UInt16 => 16,
            SignatureTypeCode.Int64 or SignatureTypeCode.UInt64 => 64,
            _ => 32
        };
        bool signed = code is SignatureTypeCode.SByte or SignatureTypeCode.Int16 or SignatureTypeCode.Int32 or SignatureTypeCode.Int64;
        return enums[identity] = JsonSerializer.Serialize(new {
            width, signed, flags = EnumInfo.IsFlags(identity),
            values = EnumInfo.Constants(identity).OrderBy(c => c.Bits).Select(c => new[] { c.Bits.ToString(System.Globalization.CultureInfo.InvariantCulture), c.Name })
        });
    }
}
