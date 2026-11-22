using Microsoft.CodeAnalysis;
using Xui.GPU.IR;

namespace Xui.CompileTime.TypeMapping;

/// <summary>
/// Maps C# type symbols to IR types.
/// </summary>
internal class ShaderTypeMapper
{
    private static readonly IrScalarType F32 = new(ScalarKind.F32);
    private static readonly IrScalarType I32 = new(ScalarKind.I32);
    private static readonly IrScalarType U32 = new(ScalarKind.U32);
    private static readonly IrScalarType Bool = new(ScalarKind.Bool);

    private readonly Dictionary<string, IrType> _cache = new();

    public IrType MapType(ITypeSymbol type)
    {
        var key = type.ToDisplayString();
        if (_cache.TryGetValue(key, out var cached))
            return cached;

        var result = MapTypeCore(type);
        _cache[key] = result;
        return result;
    }

    private IrType MapTypeCore(ITypeSymbol type)
    {
        var fullName = type.ToDisplayString();

        return fullName switch
        {
            "Xui.GPU.Shaders.Types.F32" => F32,
            "Xui.GPU.Shaders.Types.I32" => I32,
            "Xui.GPU.Shaders.Types.U32" => U32,
            "Xui.GPU.Shaders.Types.Bool" => Bool,

            "Xui.GPU.Shaders.Types.Float2" => new IrVectorType(F32, 2),
            "Xui.GPU.Shaders.Types.Float3" => new IrVectorType(F32, 3),
            "Xui.GPU.Shaders.Types.Float4" => new IrVectorType(F32, 4),
            "Xui.GPU.Shaders.Types.Color4" => new IrVectorType(F32, 4), // Color4 is float4 in shaders

            "Xui.GPU.Shaders.Types.Texture2D<Xui.GPU.Shaders.Types.Color4>" => new IrTextureType(new IrVectorType(F32, 4)),
            "Xui.GPU.Shaders.Types.Sampler" => IrSamplerType.Instance,

            "Xui.GPU.Shaders.Types.Float4x4" => new IrMatrixType(F32, 4, 4),

            _ => MapStructType(type)
        };
    }

    public IrStructType MapStructType(ITypeSymbol type)
    {
        var key = type.ToDisplayString();
        if (_cache.TryGetValue(key, out var cached) && cached is IrStructType cachedStruct)
            return cachedStruct;

        var structType = new IrStructType(type.Name);

        // Cache early to handle potential recursive references
        _cache[key] = structType;

        foreach (var member in type.GetMembers())
        {
            if (member is IFieldSymbol field && !field.IsStatic && !field.IsConst)
            {
                var irField = new IrStructField
                {
                    Name = field.Name,
                    Type = MapType(field.Type)
                };

                // Read [Location(n)] attribute
                foreach (var attr in field.GetAttributes())
                {
                    var attrName = attr.AttributeClass?.ToDisplayString();
                    if (attrName == "Xui.GPU.Shaders.Attributes.LocationAttribute"
                        && attr.ConstructorArguments.Length > 0)
                    {
                        var index = (int)attr.ConstructorArguments[0].Value!;
                        irField.Decorations.Add(new IrLocationDecoration(index));
                    }
                    else if (attrName == "Xui.GPU.Shaders.Attributes.BuiltInAttribute"
                        && attr.ConstructorArguments.Length > 0)
                    {
                        var semantic = (int)attr.ConstructorArguments[0].Value!;
                        irField.Decorations.Add(new IrBuiltInDecoration((BuiltInSemantic)semantic));
                    }
                }

                structType.Fields.Add(irField);
            }
        }

        return structType;
    }

    /// <summary>
    /// Maps a struct type used as vertex input, auto-assigning Location(N) to fields that lack decorations.
    /// </summary>
    public IrStructType MapVertexInputType(ITypeSymbol type)
    {
        var structType = MapStructType(type);
        AutoAssignLocations(structType);
        return structType;
    }

    /// <summary>
    /// Maps a struct type used as fragment output, auto-assigning Location(N) to fields that lack decorations.
    /// </summary>
    public IrStructType MapFragmentOutputType(ITypeSymbol type)
    {
        var structType = MapStructType(type);
        AutoAssignLocations(structType);
        return structType;
    }

    private static void AutoAssignLocations(IrStructType structType)
    {
        int nextLocation = 0;
        foreach (var field in structType.Fields)
        {
            if (!field.Decorations.Any(d => d is IrLocationDecoration || d is IrBuiltInDecoration))
            {
                field.Decorations.Add(new IrLocationDecoration(nextLocation));
            }
            if (field.Decorations.Any(d => d is IrLocationDecoration))
            {
                nextLocation++;
            }
        }
    }

    /// <summary>
    /// Maps a struct type used as bindings, adding IrBindingDecoration to each field.
    /// </summary>
    public IrStructType MapBindingsType(ITypeSymbol type)
    {
        var structType = MapStructType(type);

        // Add binding decorations to fields that don't already have them
        for (int i = 0; i < structType.Fields.Count; i++)
        {
            var field = structType.Fields[i];
            if (!field.Decorations.Any(d => d is IrBindingDecoration))
            {
                field.Decorations.Add(new IrBindingDecoration(0, i));
            }
        }

        return structType;
    }
}
