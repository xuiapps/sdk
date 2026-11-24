using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.WASM.Emitter;

public sealed partial class WasmEmitter
{
    private static void ValidateExport(string name, MethodIdentity identity)
    {
        if (string.IsNullOrEmpty(name) || name == "main")
            throw new ArgumentException("Named exports need a nonempty name other than the reserved main export.");
        var module = identity.DeclaringType.Module;
        var method = module.GetMethod(identity.Handle);
        var signature = SignatureReader.Method(module, method.Signature, identity.DeclaringType.Arguments);
        if (signature.HasThis || method.Name is ".ctor" or ".cctor")
            throw new NotSupportedException($"Export {name} must be an ordinary static method.");
        foreach (var parameter in signature.Parameters) Check(parameter, result: false);
        Check(signature.Result, result: true);

        void Check(SignatureType type, bool result)
        {
            // Managed references stay opaque host-held WASM GC values. Struct values
            // and byrefs need ownership/marshalling wrappers before they can escape.
            bool supported = !type.IsByReference && !type.ContainsGenericParameters &&
                (type.IsReferenceType || type.Code is SignatureTypeCode.String or SignatureTypeCode.Object or
                    SignatureTypeCode.Int32 or SignatureTypeCode.UInt32 or SignatureTypeCode.Int64 or SignatureTypeCode.UInt64 or
                    SignatureTypeCode.IntPtr or SignatureTypeCode.UIntPtr or SignatureTypeCode.Single or SignatureTypeCode.Double ||
                    result && type.Code == SignatureTypeCode.Void);
            if (!supported) throw new NotSupportedException($"Export {name} has an unsupported host ABI type: {type}.");
        }
    }
}
