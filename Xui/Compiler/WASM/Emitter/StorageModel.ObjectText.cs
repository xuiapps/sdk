using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.XuiNet.Reachability;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class StorageModel
{
    public bool ObjectTextEnabled { get; set; }
    public IEnumerable<ArrayShape> ObjectTextArrays => arrays.Values;
    private (TypeIdentity Type, MethodIdentity? Override)[]? objectTextTargets;
    public (TypeIdentity Type, MethodIdentity? Override)[] ObjectTextTargets => objectTextTargets ??=
        dependencies.Nodes.OfType<Dependency.Instantiation>().Where(n => !n.Identity.IsValueType && XuiNet.DotNet.Exceptions.ExceptionTypes.Kind(n.Identity) is null)
            .Select(n => (n.Identity, ObjectText.Implementation(dependencies.VirtualSlots(n.Identity)))).ToArray();

    public IEnumerable<string> ObjectTextLiterals()
    {
        if (!ObjectTextEnabled) yield break;
        yield return "{0}";
        foreach (var target in ObjectTextTargets) yield return ObjectText.TypeName(target.Type);
        foreach (var box in Boxes.Keys)
        {
            yield return ObjectText.TypeName(box.Identity);
            if (EnumInfo.UnderlyingType(box.Identity) is not null) yield return Formatting.RequireEnum(box.Identity);
        }
        foreach (var array in arrays.Keys) yield return ObjectText.TypeName(new SignatureType(SignatureTypeCodeFor(array), Array: array));
        yield return "System.String";
    }

    private static System.Reflection.Metadata.SignatureTypeCode SignatureTypeCodeFor(ArraySignature array) =>
        array.IsVector ? System.Reflection.Metadata.SignatureTypeCode.SZArray : System.Reflection.Metadata.SignatureTypeCode.Array;
}
