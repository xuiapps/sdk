using Xui.Compiler.WASM.Wasm.Section;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.XuiNet.Layout;
using Model = Xui.Compiler.XuiNet.DotNet;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using WasmValue = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueType;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Selects linear or GC storage and recursively flattens value-type instance fields.</summary>
internal sealed class StorageModel(Model.Module module, XuiNet.Reachability.DependencyGraph dependencies)
{
    public Runtime.TypeRegistry Classes { get; } = new(module, dependencies);
    public TypeIndex ClassRoot { get; private set; }
    public TypeIndex DescriptorType { get; private set; }
    public TypeIndex InstanceTestType { get; private set; }
    public FunctionIndex InstanceTest { get; set; }
    public uint TableSize { get; private set; }
    private readonly LayoutResolver layouts = new(module, TargetLayout.Wasm32);
    private readonly Dictionary<TypeIdentity, GcShape> shapes = new();
    private readonly HashSet<TypeIdentity> flattening = new();

    public bool IsClass(TypeIdentity identity) => identity.Handle.Kind == HandleKind.TypeDefinition && !identity.IsValueType;
    public bool IsGc(TypeIdentity identity) => IsClass(identity) ||
        (identity.Handle.Kind == HandleKind.TypeDefinition && layouts.Classify((TypeDefinitionHandle)identity.Handle) == StorageKind.ContainsManagedReferences);

    public LocalVariable Bind(SignatureType type)
    {
        if (type.Code is SignatureTypeCode.GenericTypeParameter or SignatureTypeCode.GenericMethodParameter)
            throw new NotSupportedException("Open generic parameters cannot be emitted.");
        if (type.Identity is { IsValueType: true, Arguments: not null })
            throw new NotSupportedException("Generic struct storage is not implemented yet.");
        if (type.IsReferenceType && type.Identity is { Handle.Kind: not HandleKind.TypeDefinition })
            throw new NotSupportedException($"External class storage is not implemented: {type}.");
        return new(type.Code, type.Identity, type.IsByReference, type.Identity is { } identity && IsGc(identity));
    }

    public GcShape Shape(TypeIdentity identity)
    {
        if (flattening.Contains(identity)) throw new NotSupportedException($"Recursive inline struct: {identity}.");
        if (shapes.TryGetValue(identity, out var found)) return found;
        if (MemoryFrame.IsNativeFloat(identity))
        {
            var scalar = new GcShape(identity);
            scalar.Leaves.Add(new(new(SignatureTypeCode.Single), 0));
            shapes.Add(identity, scalar);
            return scalar;
        }
        if (identity.Module != module || identity.Handle.Kind != HandleKind.TypeDefinition)
            throw new NotSupportedException($"External GC layout is not implemented: {identity}.");
        var type = module.GetType((TypeDefinitionHandle)identity.Handle);
        if (type.IsInterface || type.GenericParameterCount != (identity.Arguments?.Values.Length ?? 0))
            throw new NotSupportedException($"Interface and open generic GC layouts are not implemented: {identity}.");
        GcShape? parentShape = null;
        if (IsClass(identity))
        {
            var runtimeType = Classes.Resolve(identity);
            if (runtimeType.BaseType is { } parent) parentShape = Shape(parent.Identity);
        }
        else if ((type.Attributes & TypeAttributes.LayoutMask) == TypeAttributes.ExplicitLayout)
            throw new NotSupportedException($"Explicit-layout structs cannot be flattened into GC fields yet: {identity}.");
        var shape = new GcShape(identity);
        if (IsClass(identity))
        {
            if (parentShape is not null)
            {
                shape.Leaves.AddRange(parentShape.Leaves);
                foreach (var field in parentShape.Fields) shape.Fields.Add(field.Key, field.Value);
            }
            else shape.Leaves.Add(new(new(SignatureTypeCode.Object, UsesGc: true), 0));
        }
        shapes.Add(identity, shape);
        flattening.Add(identity);
        try
        {
            var byteLayout = identity.IsValueType && !IsGc(identity) ? layouts.Resolve((TypeDefinitionHandle)identity.Handle) : null;
            foreach (var handle in type.Fields)
            {
                var field = module.GetField(handle);
                if ((field.Attributes & FieldAttributes.Static) != 0) continue;
                var declared = SignatureReader.Field(module, field.Signature, identity.Arguments);
                if (declared.IsByReference || declared.Code == SignatureTypeCode.SZArray)
                    throw new NotSupportedException($"Managed byref and array GC fields are not implemented: {field.Name}.");
                var storage = Bind(declared);
                int first = shape.Leaves.Count;
                int byteOffset = byteLayout?.Fields[handle].Offset ?? 0;
                if (declared.Identity is { } nested && !declared.IsReferenceType)
                {
                    foreach (var leaf in Shape(nested).Leaves)
                        shape.Leaves.Add(new(leaf.Storage, byteOffset + leaf.MemoryOffset));
                }
                else shape.Leaves.Add(new(storage, byteOffset));
                shape.Fields.Add(handle, new(storage, first, shape.Leaves.Count - first));
            }
        }
        catch { shapes.Remove(identity); throw; }
        finally { flattening.Remove(identity); }
        // Reference edges do not flatten; register their shapes after finishing the current one.
        foreach (var leaf in shape.Leaves)
            if (leaf.Storage.Identity is { } child && IsClass(child) && !shapes.ContainsKey(child)) _ = Shape(child);
        return shape;
    }

    public void DefineTypes(ref TypeWriter writer, TypeIndex utf16ArrayType)
    {
        if (Classes.Types.Count != 0)
        {
            DescriptorType = writer.StructType([ValueTypeCode.I32, ValueTypeCode.EqRef, ValueTypeCode.I32,
                WasmValue.RefNull(utf16ArrayType), ValueTypeCode.I32, ValueTypeCode.I32], Mutability.Immutable);
            ClassRoot = writer.StructType([ValueTypeCode.EqRef], extensible: true);
            InstanceTestType = writer.FunctionType([ValueTypeCode.EqRef, ValueTypeCode.EqRef], ValueTypeCode.EqRef);
        }
        foreach (var shape in shapes.Values)
        {
            var fields = shape.Leaves.Select(leaf => (WasmValue)leaf.Storage.WasmType).ToArray();
            if (IsClass(shape.Identity))
            {
                var type = Classes.Resolve(shape.Identity);
                shape.Type = writer.StructType(fields, baseType: type.BaseType?.WasmType ?? ClassRoot, extensible: true);
                type.WasmType = shape.Type;
                type.FieldCount = (uint)shape.Leaves.Count;
                type.Fields = Array.AsReadOnly(shape.Fields.Select(pair =>
                {
                    var field = module.GetField(pair.Key);
                    var declaring = shape.Identity;
                    while (declaring.Handle != field.DeclaringType) declaring = declaring.BaseType!.Value;
                    return new Runtime.Field(pair.Key, field.Name, declaring,
                        SignatureReader.Field(module, field.Signature, declaring.Arguments), new((uint)pair.Value.First), (uint)pair.Value.Count);
                }).ToArray());
                type.TableStart = TableSize;
                TableSize += (uint)type.VirtualSlots.Count;
            }
            else shape.Type = writer.StructType(fields);
        }
    }
}
