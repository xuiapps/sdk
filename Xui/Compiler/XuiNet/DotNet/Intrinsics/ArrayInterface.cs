using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Vector collection contracts whose calls can use array storage directly.</summary>
public static class ArrayInterface
{
    /// <summary>Whether a vector implements this supported, closed collection contract.</summary>
    public static bool Matches(ArraySignature array, TypeIdentity contract) =>
        array.IsVector && contract.Arguments?.Values.Length == 1 && ElementMatches(array.Element, contract.Arguments.Values[0]) &&
        (FrameworkTypes.Is(contract, "System.Collections.Generic", "IReadOnlyList`1") ||
         FrameworkTypes.Is(contract, "System.Collections.Generic", "IReadOnlyCollection`1") ||
         (FrameworkTypes.Is(contract, "System.Collections.Generic", "ICollection`1") ||
          FrameworkTypes.Is(contract, "System.Collections.Generic", "IEnumerable`1")) && array.Element == contract.Arguments.Values[0]);

    /// <summary>Whether a collection conversion needs an adapter beyond the exact-element implementation.</summary>
    public static bool RequiresUnsupportedConversion(ArraySignature array, TypeIdentity contract) =>
        array.IsVector && contract.Arguments?.Values.Length == 1 && array.Element != contract.Arguments.Values[0] &&
        ElementMatches(array.Element, contract.Arguments.Values[0]) &&
        (FrameworkTypes.Is(contract, "System.Collections.Generic", "ICollection`1") ||
         FrameworkTypes.Is(contract, "System.Collections.Generic", "IEnumerable`1"));

    private static bool ElementMatches(SignatureType source, SignatureType target) => Reduced(source) == Reduced(target) ||
        source.Identity is { IsValueType: false } from && target.Identity is { IsValueType: false } to && from.IsAssignableTo(to);

    /// <summary>Whether a framework interface has array implementations, including operations not yet lowered.</summary>
    public static bool HasArrayImplementation(TypeIdentity type) =>
        FrameworkTypes.Is(type, "System.Collections.Generic", "IReadOnlyList`1") ||
        FrameworkTypes.Is(type, "System.Collections.Generic", "IReadOnlyCollection`1") ||
        FrameworkTypes.Is(type, "System.Collections.Generic", "IEnumerable`1") ||
        FrameworkTypes.Is(type, "System.Collections.Generic", "ICollection`1") ||
        FrameworkTypes.Is(type, "System.Collections.Generic", "IList`1") ||
        FrameworkTypes.Is(type, "System.Collections", "IEnumerable") ||
        FrameworkTypes.Is(type, "System.Collections", "ICollection") ||
        FrameworkTypes.Is(type, "System.Collections", "IList") || FrameworkTypes.Is(type, "System", "Array");

    /// <summary>The supplied CoreLib's ordinary managed vector enumerator constructor.</summary>
    public static MethodIdentity? EnumeratorConstructor(MethodIdentity method)
    {
        if (Operation(method) != "GetEnumerator") return null;
        var module = method.DeclaringType.Module;
        foreach (var handle in module.Types)
        {
            var type = module.GetType(handle);
            if (type.Namespace != "System" || type.Name != "SZGenericArrayEnumerator`1") continue;
            var identity = new TypeIdentity(module, handle, method.DeclaringType.Arguments);
            foreach (var candidate in type.Methods)
            {
                var definition = module.GetMethod(candidate);
                if (definition.Name != ".ctor") continue;
                var signature = SignatureReader.Method(module, definition.Signature, identity.Arguments);
                if (signature.Parameters.Length == 2 && signature.Parameters[0].Array is { IsVector: true } && signature.Parameters[1].Code == SignatureTypeCode.Int32) return new(identity, candidate);
            }
        }
        throw new NotSupportedException("The supplied CoreLib has no supported vector enumerator constructor.");
    }

    // CLI array compatibility uses reduced integer types, including enum underlying types.
    private static SignatureType Reduced(SignatureType type)
    {
        type = EnumInfo.Storage(type);
        return type.Identity is not null ? type : type.Code switch {
            SignatureTypeCode.Boolean or SignatureTypeCode.Byte => new(SignatureTypeCode.SByte),
            SignatureTypeCode.Char or SignatureTypeCode.UInt16 => new(SignatureTypeCode.Int16),
            SignatureTypeCode.UInt32 => new(SignatureTypeCode.Int32),
            SignatureTypeCode.UInt64 => new(SignatureTypeCode.Int64),
            SignatureTypeCode.UIntPtr => new(SignatureTypeCode.IntPtr),
            _ => type
        };
    }

    /// <summary>Returns the supported operation for a trusted framework collection declaration.</summary>
    public static string? Operation(MethodIdentity method)
    {
        var type = method.DeclaringType;
        if (type.Arguments?.Values.Length != 1 || !Matches(new(type.Arguments.Values[0]), type)) return null;
        var definition = type.Module.GetMethod(method.Handle);
        var signature = SignatureReader.Method(type.Module, definition.Signature, type.Arguments);
        if (!signature.HasThis || signature.GenericParameterCount != 0) return null;
        if (FrameworkTypes.Is(type, "System.Collections.Generic", "IEnumerable`1") && definition.Name == "GetEnumerator" && signature.Parameters.Length == 0) return "GetEnumerator";
        if (definition.Name == "get_Count" && signature.Parameters.Length == 0 && signature.Result.Code == SignatureTypeCode.Int32) return "Count";
        if (definition.Name == "get_Item" && signature.Parameters.Length == 1 && signature.Parameters[0].Code == SignatureTypeCode.Int32 && signature.Result == type.Arguments.Values[0]) return "Item";
        if (FrameworkTypes.Is(type, "System.Collections.Generic", "ICollection`1"))
        {
            if (definition.Name == "get_IsReadOnly" && signature.Parameters.Length == 0 && signature.Result.Code == SignatureTypeCode.Boolean) return "IsReadOnly";
            if (definition.Name == "CopyTo" && signature.Result.Code == SignatureTypeCode.Void && signature.Parameters.Length == 2 &&
                signature.Parameters[0].Array is { IsVector: true } array && array.Element == type.Arguments.Values[0] &&
                signature.Parameters[1].Code == SignatureTypeCode.Int32) return "CopyTo";
            if (definition.Name == "Clear" && signature.Parameters.Length == 0 && signature.Result.Code == SignatureTypeCode.Void) return "FixedSize";
            if (definition.Name is "Add" or "Remove" && signature.Parameters.Length == 1 && signature.Parameters[0] == type.Arguments.Values[0]) return "FixedSize";
        }
        return null;
    }
}
