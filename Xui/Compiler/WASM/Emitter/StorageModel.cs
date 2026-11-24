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
internal sealed partial class StorageModel(XuiNet.Reachability.DependencyGraph dependencies)
{
    public Runtime.TypeRegistry Classes { get; } = new(dependencies);
    public HashSet<XuiNet.DotNet.Exceptions.ExceptionKind> ExtraExceptionKinds { get; } = new();
    public bool ExceptionsEnabled { get; set; }
    public TypeIndex ExceptionType { get; private set; }
    public TypeIndex ExceptionTagType { get; private set; }
    public TagIndex ExceptionTag { get; set; }
    public TypeIndex ExceptionPayloadBlock { get; private set; }
    public TypeIndex ExceptionCatchBlock { get; private set; }
    public TypeIndex ExceptionReferenceBlock { get; private set; }
    public TypeIndex EmptyBlock { get; private set; }
    public TypeIndex ClassRoot { get; private set; }
    public TypeIndex DescriptorType { get; private set; }
    public TypeIndex InstanceTestType { get; private set; }
    public FunctionIndex InstanceTest { get; set; }
    public uint TableSize { get; private set; }
    private readonly Dictionary<Model.Module, LayoutResolver> layoutResolvers = new();
    private LayoutResolver Layouts(TypeIdentity identity)
    {
        if (!layoutResolvers.TryGetValue(identity.Module, out var resolver))
            layoutResolvers.Add(identity.Module, resolver = new(identity.Module, TargetLayout.Wasm32));
        return resolver;
    }
    private readonly Dictionary<TypeIdentity, GcShape> shapes = new();
    private readonly HashSet<TypeIdentity> flattening = new();

    public bool IsClass(TypeIdentity identity) => XuiNet.DotNet.Exceptions.ExceptionTypes.Kind(identity) is not null || identity.Handle.Kind == HandleKind.TypeDefinition && !identity.IsValueType;
    public bool IsGc(TypeIdentity identity)
    {
        if (XuiNet.DotNet.Intrinsics.SpanAccess.Is(identity)) return false;
        return IsClass(identity) || (!StackTypes.Contains(identity) && identity.Handle.Kind == HandleKind.TypeDefinition &&
            HasStackReferences(identity));
    }

    public LocalVariable Bind(SignatureType type)
    {
        if (type.Identity is { } native && XuiNet.DotNet.Intrinsics.NativeFloat.Is(native))
        {
            if (type.IsByReference) throw new NotSupportedException("NFloat managed-address ABI is not implemented.");
            return new(SignatureTypeCode.Single);
        }
        if (type.Code is SignatureTypeCode.GenericTypeParameter or SignatureTypeCode.GenericMethodParameter)
            throw new NotSupportedException("Open generic parameters cannot be emitted.");
        if (type.IsReferenceType && type.Identity is { Handle.Kind: not HandleKind.TypeDefinition } external && XuiNet.DotNet.Exceptions.ExceptionTypes.Kind(external) is null)
            throw new NotSupportedException($"External class storage is not implemented: {type}.");
        if (type.Array is { } array) _ = ArrayShape(array);
        return new(type.Code, type.Identity, type.IsByReference, type.Code is SignatureTypeCode.Object or SignatureTypeCode.SZArray || type.Identity is { } identity && IsGc(identity), type.Array);
    }

    public GcShape Shape(TypeIdentity identity)
    {
        if (flattening.Contains(identity)) throw new NotSupportedException($"Recursive inline struct: {identity}.");
        if (shapes.TryGetValue(identity, out var found)) return found;
        if (XuiNet.DotNet.Exceptions.ExceptionTypes.Kind(identity) is not null)
        {
            ExceptionsEnabled = true;
            var exception = new GcShape(identity);
            shapes.Add(identity, exception);
            return exception;
        }
        if (XuiNet.DotNet.Intrinsics.SpanAccess.Is(identity))
        {
            var span = new GcShape(identity);
            span.Leaves.Add(new(new(SignatureTypeCode.Int32), 0));
            span.Leaves.Add(new(new(SignatureTypeCode.Int32), 4));
            shapes.Add(identity, span);
            return span;
        }
        if (MemoryFrame.IsNativeFloat(identity))
        {
            var scalar = new GcShape(identity);
            scalar.Leaves.Add(new(new(SignatureTypeCode.Single), 0));
            shapes.Add(identity, scalar);
            return scalar;
        }
        if (identity.IsInterface)
        {
            _ = Classes.Resolve(identity);
            var contract = new GcShape(identity);
            shapes.Add(identity, contract);
            return contract;
        }
        if (identity.Handle.Kind != HandleKind.TypeDefinition)
            throw new NotSupportedException($"External GC layout is not implemented: {identity}.");
        var ownerModule = identity.Module;
        var type = ownerModule.GetType((TypeDefinitionHandle)identity.Handle);
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
            var byteLayout = identity.IsValueType && !IsGc(identity) ? StackLayout(identity) : null;
            foreach (var handle in type.Fields)
            {
                var field = ownerModule.GetField(handle);
                if ((field.Attributes & FieldAttributes.Static) != 0) continue;
                var declared = SignatureReader.Field(ownerModule, field.Signature, identity.Arguments);
                if (declared.IsByReference)
                    throw new NotSupportedException($"Managed byref GC fields are not implemented: {field.Name}.");
                var storage = Bind(declared);
                int first = shape.Leaves.Count;
                int byteOffset = byteLayout?.Fields[handle].Offset ?? 0;
                if (declared.Identity is { } nested && !declared.IsReferenceType && !MemoryFrame.IsNativeFloat(nested))
                {
                    foreach (var leaf in Shape(nested).Leaves)
                        shape.Leaves.Add(new(leaf.Storage, byteOffset + leaf.MemoryOffset));
                }
                else shape.Leaves.Add(new(storage, byteOffset));
                shape.Fields.Add(new(identity, handle), new(storage, first, shape.Leaves.Count - first));
            }
            if (InlineArrayInfo.Read(identity) is { } inline)
            {
                var elementLeaves = shape.Leaves.ToArray();
                int stride = byteLayout?.Fields[inline.Field.Handle].Size ?? 0;
                for (int index = 1; index < inline.Length; index++)
                    foreach (var leaf in elementLeaves) shape.Leaves.Add(new(leaf.Storage, checked(leaf.MemoryOffset + index * stride)));
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
        if (ExceptionsEnabled)
        {
            ExceptionType = writer.StructType([ValueTypeCode.I32, ValueTypeCode.ExternRef], Mutability.Immutable);
            ExceptionTagType = writer.FunctionType([ValueTypeCode.EqRef]);
            ExceptionPayloadBlock = writer.FunctionType([], ValueTypeCode.EqRef);
            ExceptionCatchBlock = writer.FunctionType([], [ValueTypeCode.EqRef, ValueTypeCode.ExnRef]);
            ExceptionReferenceBlock = writer.FunctionType([], ValueTypeCode.ExnRef);
            EmptyBlock = writer.FunctionType();
        }
        if (Classes.Types.Count != 0)
        {
            DescriptorType = writer.StructType([ValueTypeCode.I32, ValueTypeCode.EqRef, ValueTypeCode.I32,
                WasmValue.RefNull(utf16ArrayType), ValueTypeCode.I32, ValueTypeCode.I32], Mutability.Immutable);
            ClassRoot = writer.StructType([ValueTypeCode.EqRef], extensible: true);
            InstanceTestType = writer.FunctionType([ValueTypeCode.EqRef, ValueTypeCode.EqRef], ValueTypeCode.EqRef);
            foreach (var contract in Classes.Types.Where(type => type.IsInterface)) contract.WasmType = ClassRoot;
        }
        foreach (var shape in shapes.Values)
        {
            if (XuiNet.DotNet.Exceptions.ExceptionTypes.Kind(shape.Identity) is not null)
            {
                shape.Type = ExceptionType;
                continue;
            }
            if (shape.Identity.IsInterface)
            {
                shape.Type = ClassRoot;
                Classes.Resolve(shape.Identity).WasmType = ClassRoot;
                continue;
            }
            var fields = shape.Leaves.Select(leaf => (WasmValue)leaf.Storage.WasmType).ToArray();
            if (IsClass(shape.Identity))
            {
                var type = Classes.Resolve(shape.Identity);
                shape.Type = writer.StructType(fields, baseType: type.BaseType?.WasmType ?? ClassRoot, extensible: true);
                type.WasmType = shape.Type;
                type.FieldCount = (uint)shape.Leaves.Count;
                type.Fields = Array.AsReadOnly(shape.Fields.Select(pair =>
                {
                    var declaring = pair.Key.DeclaringType;
                    var field = declaring.Module.GetField(pair.Key.Handle);
                    return new Runtime.Field(pair.Key.Handle, field.Name, declaring,
                        SignatureReader.Field(declaring.Module, field.Signature, declaring.Arguments), new((uint)pair.Value.First), (uint)pair.Value.Count);
                }).ToArray());
                type.TableStart = TableSize;
                TableSize += (uint)(Classes.InterfaceMethods.Count + type.VirtualSlots.Count);
            }
            else shape.Type = writer.StructType(fields);
        }
        DefineArrayTypes(ref writer);
        if (Classes.InterfaceMethods.Count != 0 && TableSize == 0) TableSize = (uint)Classes.InterfaceMethods.Count;
    }
}
