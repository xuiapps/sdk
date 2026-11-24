using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.XuiNet.Layout;
using Xui.Compiler.XuiNet.Reachability;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class StorageModel
{
    // Fixed capacity for now, like the existing byte stack. No per-call table allocation.
    public const uint ReferenceCapacity = 65536;
    public bool ReferenceSlotsEnabled { get; set; }
    private HashSet<TypeIdentity> StackTypes { get; } = new();
    private readonly Dictionary<TypeIdentity, StackStructLayout> stackLayouts = new();

    public static bool IsReference(LocalVariable type) => type.TypeCode is SignatureTypeCode.String or SignatureTypeCode.Object ||
        type.Identity is { IsValueType: false };

    public bool HasStackReferences(TypeIdentity type) => type.Handle.Kind == HandleKind.TypeDefinition && type.IsValueType &&
        Layouts(type).Classify(type) == StorageKind.ContainsManagedReferences;

    public void SelectStackTypes(DependencyGraph dependencies)
    {
        foreach (var identity in dependencies.Methods.Concat(dependencies.Nodes.OfType<Dependency.Signature>().Select(n => n.Identity)).Distinct())
        {
            var owner = identity.DeclaringType;
            var method = owner.Module.GetMethod(identity.Handle);
            var signature = SignatureReader.Method(owner.Module, method.Signature, owner.Arguments);
            foreach (var parameter in signature.Parameters)
                if (parameter.IsByReference && parameter.Identity is { IsValueType: true } type) Select(type);
            if (dependencies.Contains(new Dependency.Method(identity)) && method.Body is { } body)
                foreach (var local in SignatureReader.Locals(owner.Module, body.LocalSignature, owner.Arguments))
                    if (local.IsByReference && local.Identity is { IsValueType: true } type) Select(type);
        }
        void Select(TypeIdentity type)
        {
            if (!StackTypes.Add(type)) return;
            if (type.Handle.Kind != HandleKind.TypeDefinition) return;
            foreach (var fieldHandle in type.Module.GetType((TypeDefinitionHandle)type.Handle).Fields)
            {
                var field = type.Module.GetField(fieldHandle);
                if ((field.Attributes & FieldAttributes.Static) != 0) continue;
                var signature = SignatureReader.Field(type.Module, field.Signature, type.Arguments);
                if (signature.Identity is { IsValueType: true } nested) Select(nested);
            }
        }
    }

    public StackStructLayout StackLayout(TypeIdentity identity)
    {
        if (stackLayouts.TryGetValue(identity, out var found)) return found;
        if (identity.Handle.Kind != HandleKind.TypeDefinition || !identity.IsValueType)
            throw new NotSupportedException($"External struct storage is not implemented: {identity}.");
        if (Layouts(identity).Classify(identity) == StorageKind.LinearMemory)
        {
            var numeric = Layouts(identity).Resolve(identity);
            return stackLayouts[identity] = new(numeric.Size, numeric.Alignment, numeric.Fields);
        }
        ReferenceSlotsEnabled = true;
        ExceptionsEnabled = true;
        var ownerModule = identity.Module;
        var type = ownerModule.GetType((TypeDefinitionHandle)identity.Handle);
        if ((type.Attributes & TypeAttributes.LayoutMask) != TypeAttributes.SequentialLayout && !FrameworkTypes.IsValueTuple(identity))
            throw new NotSupportedException($"Reference-slot stack storage requires a sequential struct or framework ValueTuple: {identity}.");
        int pack = type.DeclaredLayout.PackingSize == 0 ? TargetLayout.Wasm32.DefaultPackingSize : type.DeclaredLayout.PackingSize;
        int end = 0, alignment = 1;
        var fields = new Dictionary<FieldDefinitionHandle, FieldLayout>();
        foreach (var handle in type.Fields)
        {
            var field = ownerModule.GetField(handle);
            if ((field.Attributes & FieldAttributes.Static) != 0) continue;
            var signature = SignatureReader.Field(ownerModule, field.Signature, identity.Arguments);
            if (signature.IsByReference || signature.Code is SignatureTypeCode.SZArray or SignatureTypeCode.Array)
                throw new NotSupportedException("Byref and array fields in stack structs are not implemented.");
            var storage = Bind(signature);
            (int size, int align) = IsReference(storage) ? (4, 4) : signature.Identity is { } nested
                ? Nested(nested) : signature.Code switch
                {
                    SignatureTypeCode.Boolean or SignatureTypeCode.Byte or SignatureTypeCode.SByte => (1, 1),
                    SignatureTypeCode.Char or SignatureTypeCode.Int16 or SignatureTypeCode.UInt16 => (2, 2),
                    SignatureTypeCode.Int64 or SignatureTypeCode.UInt64 or SignatureTypeCode.Double => (8, 8),
                    SignatureTypeCode.Int32 or SignatureTypeCode.UInt32 or SignatureTypeCode.IntPtr or SignatureTypeCode.UIntPtr or SignatureTypeCode.Single => (4, 4),
                    _ => throw new NotSupportedException($"Unsupported stack field: {signature}.")
                };
            align = Math.Min(pack, align); alignment = Math.Max(alignment, align);
            end = Align(end, align); fields.Add(handle, new(end, size)); end = checked(end + size);
        }
        return stackLayouts[identity] = new(Math.Max(1, Math.Max(type.DeclaredLayout.Size, Align(end, alignment))), alignment, fields);

        (int, int) Nested(TypeIdentity nested) => MemoryFrame.IsNativeFloat(nested) ? (4, 4) : (StackLayout(nested).Size, StackLayout(nested).Alignment);
        static int Align(int value, int align) => checked((value + align - 1) / align * align);
    }
}
