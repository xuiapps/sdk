using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using BodyWriter = Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody.Writer;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class StorageModel
{
    // GC locations address a contiguous range of flattened leaves. Linear locations
    // hold byte addresses, except reference-valued locations, which hold table indices.
    public bool ReferenceAddressesEnabled { get; set; }
    public TypeIndex AddressIndexType { get; private set; }
    public List<TypeIndex> ReferenceAccessorTypes { get; } = new();
    public List<FunctionIndex> ReferenceAccessorFunctions { get; } = new();
    private readonly List<(GcLocation Location, LocalVariable Type)> referenceFields = new();
    private readonly Dictionary<GcShape, int> addressShapes = new();
    private readonly List<LocalVariable> accessorValues = [new(SignatureTypeCode.Object, UsesGc: true), new(SignatureTypeCode.String)];
    public bool HasAddressProjection { get; private set; }
    public bool HasScalarAddressAccessors => accessorValues.Count > 2;

    public void RequireAddress(LocalVariable type)
    {
        ReferenceAddressesEnabled = ExceptionsEnabled = true;
        type = type with { IsByReference = false };
        if (type.Identity is { IsValueType: true } identity)
        {
            HasAddressProjection = true;
            _ = StackLayout(identity);
            if (IsExplicitValue(identity))
            {
                foreach (var handle in identity.Module.GetType((TypeDefinitionHandle)identity.Handle).Fields)
                {
                    var field = identity.Module.GetField(handle);
                    if ((field.Attributes & System.Reflection.FieldAttributes.Static) == 0)
                        RequireAddress(Bind(XuiNet.DotNet.Signatures.SignatureReader.Field(identity.Module, field.Signature, identity.Arguments)));
                }
            }
            else foreach (var leaf in Shape(identity).Leaves) RequireAddress(leaf.Storage);
        }
        else if (type.IsScalar && !accessorValues.Any(v => v.TypeCode == type.TypeCode)) accessorValues.Add(type);
    }

    public static bool IsExplicitValue(XuiNet.DotNet.TypeIdentity identity) => identity.IsValueType &&
        identity.Handle.Kind == HandleKind.TypeDefinition &&
        (identity.Module.GetType((TypeDefinitionHandle)identity.Handle).Attributes & System.Reflection.TypeAttributes.LayoutMask) == System.Reflection.TypeAttributes.ExplicitLayout;

    public int ReferenceField(GcLocation location, LocalVariable type)
    {
        if (!addressShapes.TryGetValue(location.Owner, out int first))
        {
            first = referenceFields.Count;
            addressShapes.Add(location.Owner, first);
            for (int i = 0; i < location.Owner.Leaves.Count; i++)
                referenceFields.Add((new(location.Owner, i), location.Owner.Leaves[i].Storage));
            if (location.Owner.Leaves.Count == 0) referenceFields.Add((location, new(SignatureTypeCode.Void)));
        }
        return first + location.First;
    }

    public FunctionIndex ReferenceAccessor(LocalVariable type, bool store)
    {
        int index = IsReference(type) ? (type.TypeCode == SignatureTypeCode.String ? 1 : 0)
            : accessorValues.FindIndex(v => v.TypeCode == type.TypeCode);
        if (index < 0) throw new InvalidOperationException($"Address accessor was not collected for {type}.");
        return ReferenceAccessorFunctions[index * 2 + (store ? 1 : 0)];
    }

    public void DefineReferenceAddressTypes(ref TypeWriter writer)
    {
        if (!ReferenceAddressesEnabled) return;
        AddressIndexType = writer.FunctionType([], ValueTypeCode.I32);
        foreach (var value in accessorValues)
        {
            ReferenceAccessorTypes.Add(writer.FunctionType([ValueTypeCode.EqRef, ValueTypeCode.I32], value.WasmType));
            ReferenceAccessorTypes.Add(writer.FunctionType([ValueTypeCode.EqRef, ValueTypeCode.I32, value.WasmType]));
        }
    }

    public void EmitReferenceAccessor(BodyWriter writer, int accessor, EmissionRuntime runtime, IReadOnlyDictionary<string, (DataIndex Data, GlobalIndex Global)> literals)
    {
        writer.LocalGet(new(0)).RefIsNull().LocalGet(new(1)).I32Const(-1).I32Eq().I32And().If();
        var message = XuiNet.DotNet.Exceptions.ExceptionTypes.Message(XuiNet.DotNet.Exceptions.ExceptionKind.NullReferenceException);
        var literal = literals[message];
        writer.GlobalGet(literal.Global).RefIsNull().If().I32Const(0).I32Const(message.Length)
            .ArrayNewData(runtime.Utf16ArrayType, literal.Data).I32Const(0).I32Const(message.Length)
            .Call(runtime.FromCharCodeArrayFunction).GlobalSet(literal.Global).End();
        writer.I32Const((int)XuiNet.DotNet.Exceptions.ExceptionKind.NullReferenceException).GlobalGet(literal.Global);
        if (ArgumentExceptionsEnabled) writer.RefNull(ValueTypeCode.ExternRef);
        if (InnerExceptionsEnabled) writer.RefNull(ValueTypeCode.EqRef);
        writer.StructNew(ExceptionType).Throw(ExceptionTag).End();
        bool store = (accessor & 1) != 0;
        var value = accessorValues[accessor / 2];
        bool reference = IsReference(value);
        if (!reference || ReferenceSlotsEnabled)
        {
            writer.LocalGet(new(0)).RefIsNull().If();
            writer.LocalGet(new(1));
            if (store) writer.LocalGet(new(2));
            if (reference)
            {
                var table = value.TypeCode == SignatureTypeCode.String ? runtime.StringReferences!.Value : runtime.ObjectReferences!.Value;
                if (store) writer.TableSet(table); else writer.TableGet(table);
            }
            else if (store) StoreAddressScalar(writer, value); else LoadAddressScalar(writer, value);
            writer.Return().End();
        }
        for (int i = 0; i < referenceFields.Count; i++)
        {
            var (location, type) = referenceFields[i];
            if (reference ? !IsReference(type) || (type.TypeCode == SignatureTypeCode.String) != (value.TypeCode == SignatureTypeCode.String)
                : type.TypeCode != value.TypeCode) continue;
            writer.LocalGet(new(1)).I32Const(i).I32Eq().If();
            writer.LocalGet(new(0)).RefCast(location.Owner.Type);
            if (store) writer.LocalGet(new(2)).StructSet(location.Owner.Type, new((uint)location.First));
            else writer.StructGet(location.Owner.Type, new((uint)location.First));
            writer.Return().End();
        }
        writer.Unreachable();
    }

    private static void LoadAddressScalar(BodyWriter writer, LocalVariable type)
    {
        switch (type.TypeCode)
        {
            case SignatureTypeCode.SByte: writer.I32Load8S(); break;
            case SignatureTypeCode.Boolean: case SignatureTypeCode.Byte: writer.I32Load8U(); break;
            case SignatureTypeCode.Int16: writer.I32Load16S(); break;
            case SignatureTypeCode.Char: case SignatureTypeCode.UInt16: writer.I32Load16U(); break;
            case SignatureTypeCode.Int64: case SignatureTypeCode.UInt64: writer.I64Load(); break;
            case SignatureTypeCode.Single: writer.F32Load(); break;
            case SignatureTypeCode.Double: writer.F64Load(); break;
            default: writer.I32Load(); break;
        }
    }

    private static void StoreAddressScalar(BodyWriter writer, LocalVariable type)
    {
        switch (type.TypeCode)
        {
            case SignatureTypeCode.SByte: case SignatureTypeCode.Boolean: case SignatureTypeCode.Byte: writer.I32Store8(); break;
            case SignatureTypeCode.Int16: case SignatureTypeCode.Char: case SignatureTypeCode.UInt16: writer.I32Store16(); break;
            case SignatureTypeCode.Int64: case SignatureTypeCode.UInt64: writer.I64Store(); break;
            case SignatureTypeCode.Single: writer.F32Store(); break;
            case SignatureTypeCode.Double: writer.F64Store(); break;
            default: writer.I32Store(); break;
        }
    }
}
