using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

public sealed partial record DefaultEquality
{
    /// <summary>The trusted comparer interface whose default singleton needs a runtime adapter.</summary>
    public static bool IsInterface(TypeIdentity type) => FrameworkTypes.Is(type, "System.Collections.Generic", "IEqualityComparer`1");

    /// <summary>Finds the default comparer corresponding to a closed scalar/string interface.</summary>
    public static TypeIdentity InterfaceOwner(TypeIdentity contract)
    {
        if (!IsInterface(contract) || contract.Arguments?.Values.Length != 1 || contract.Module.Compilation is not { } compilation)
            throw new NotSupportedException($"Default comparer interface requires a linked closed definition: {contract}.");
        var element = contract.Arguments.Values[0];
        var storage = EnumInfo.Storage(element);
        if (storage.Identity is not null && !NativeFloat.Is(storage.Identity.Value) ||
            storage.Code is SignatureTypeCode.Object or SignatureTypeCode.SZArray)
            throw new NotSupportedException($"Default comparer interface adapters currently support scalar and string elements: {element}.");
        return compilation.ResolveType(contract.Module, "System.Collections.Generic", "EqualityComparer`1") with { Arguments = contract.Arguments };
    }

    /// <summary>Describes the singleton arm of an interface call; application implementations use ordinary dispatch.</summary>
    public static DefaultEquality? ForInterface(MethodIdentity method)
    {
        if (!IsInterface(method.DeclaringType)) return null;
        var owner = InterfaceOwner(method.DeclaringType);
        var definition = method.DeclaringType.Module.GetMethod(method.Handle);
        var signature = SignatureReader.Method(method.DeclaringType.Module, definition.Signature, method.DeclaringType.Arguments);
        var element = owner.Arguments!.Values[0];
        if (definition.Name == "Equals" && signature.HasThis && signature.Result.Code == SignatureTypeCode.Boolean &&
            signature.Parameters.Length == 2 && signature.Parameters.All(p => p == element))
            return Create("Equals", owner, element, signature);
        if (definition.Name == "GetHashCode" && signature.HasThis && signature.Result.Code == SignatureTypeCode.Int32 &&
            signature.Parameters.Length == 1 && signature.Parameters[0] == element)
            return Create("GetHashCode", owner, element, signature);
        throw new NotSupportedException($"Comparer interface member is not implemented: {method}.");
    }
}
