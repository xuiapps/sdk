using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>Reference assignability for named types and closed generic interface arguments.</summary>
internal static class TypeRelations
{
    public static bool IsAssignable(TypeIdentity source, TypeIdentity target) => Named(source, target, new());

    private static bool Named(TypeIdentity source, TypeIdentity target, HashSet<(TypeIdentity, TypeIdentity)> active)
    {
        if (source == target) return true;
        if (Exceptions.ExceptionTypes.Kind(source) is { } exception && Exceptions.ExceptionTypes.Kind(target) is { } targetException)
            return Exceptions.ExceptionTypes.IsAssignable(exception, targetException);
        if (source.IsValueType || target.IsValueType) return false;
        if (FrameworkTypes.Is(target, "System", "Object")) return true;
        if (!active.Add((source, target))) return false;
        try
        {
            if (target.IsInterface)
            {
                if (Variant(source, target, active)) return true;
                foreach (var contract in source.Interfaces())
                    if (contract == target || Variant(contract, target, active)) return true;
                return false;
            }
            var visited = new HashSet<TypeIdentity>();
            for (TypeIdentity? current = source; current is { } type && visited.Add(type); current = type.BaseType)
                if (type == target) return true;
            return false;
        }
        finally { active.Remove((source, target)); }
    }

    private static bool Variant(TypeIdentity source, TypeIdentity target, HashSet<(TypeIdentity, TypeIdentity)> active)
    {
        if (!source.IsInterface || source.GenericDefinition != target.GenericDefinition ||
            source.Arguments is not { } from || target.Arguments is not { } to) return false;
        var parameters = source.Module.GetType((TypeDefinitionHandle)source.Handle).GenericParameters;
        if (from.Values.Length != parameters.Count || to.Values.Length != parameters.Count) return false;
        foreach (var handle in parameters)
        {
            var parameter = source.Module.GetGenericParameter(handle);
            var a = from.Values[parameter.Index];
            var b = to.Values[parameter.Index];
            if (a == b) continue;
            bool compatible = (parameter.Attributes & GenericParameterAttributes.VarianceMask) switch
            {
                GenericParameterAttributes.Covariant => Reference(a, b, active),
                GenericParameterAttributes.Contravariant => Reference(b, a, active),
                _ => false
            };
            if (!compatible) return false;
        }
        return true;
    }

    private static bool Reference(SignatureType source, SignatureType target, HashSet<(TypeIdentity, TypeIdentity)> active)
    {
        static bool IsReference(SignatureType type) => !type.IsByReference &&
            (type.IsReferenceType || type.Code is SignatureTypeCode.String or SignatureTypeCode.Object or SignatureTypeCode.SZArray);
        if (!IsReference(source) || !IsReference(target)) return false;
        if (source == target || target.Code == SignatureTypeCode.Object) return true;
        if (target.Identity is { } root && FrameworkTypes.Is(root, "System", "Object")) return true;
        if (source.Identity is { } from && target.Identity is { } to) return Named(from, to, active);
        if (source.Array is { } a && target.Array is { } b) return Reference(a.Element, b.Element, active);
        if (source.Array is not null && target.Identity is { } arrayTarget)
        {
            if (FrameworkTypes.Is(arrayTarget, "System", "Array")) return true;
            if (arrayTarget.IsInterface)
            {
                var definition = arrayTarget.Module.GetType((TypeDefinitionHandle)arrayTarget.Handle);
                if (FrameworkTypes.Is(arrayTarget, definition.Namespace, definition.Name))
                    throw new NotSupportedException($"Array interface variance is not implemented: {arrayTarget}.");
            }
        }
        // Primitive strings have no named identity in a signature. Resolve their framework
        // declaration only when those implementation assemblies were explicitly supplied.
        if (source.Code == SignatureTypeCode.String && target.Identity is { } contract)
        {
            var assemblies = contract.Module.Compilation?.Assemblies;
            if (assemblies is not null)
                foreach (var assembly in assemblies)
                    foreach (var handle in assembly.ManifestModule.Types)
                    {
                        var identity = new TypeIdentity(assembly.ManifestModule, handle);
                        if (FrameworkTypes.Is(identity, "System", "String")) return Named(identity, contract, active);
                    }
            if (contract.Handle.Kind != HandleKind.TypeDefinition)
                throw new NotSupportedException($"String interface variance requires a resolved target: {contract}.");
            var definition = contract.Module.GetType((TypeDefinitionHandle)contract.Handle);
            if (FrameworkTypes.Is(contract, definition.Namespace, definition.Name))
                throw new NotSupportedException($"String interface variance requires its framework implementation assembly: {contract}.");
        }
        return false;
    }
}
