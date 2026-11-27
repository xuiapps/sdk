using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.WASM.Wasm.Section;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using WasmValue = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueType;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class StorageModel
{
    public Dictionary<BoxingType, GcShape> Boxes { get; } = new();

    public GcShape Box(BoxingType value)
    {
        if (Boxes.TryGetValue(value, out var shape)) return shape;
        ExceptionsEnabled = true;
        _ = Classes.Resolve(value.Identity);
        var payload = Bind(value.Storage);
        shape = new(value.Identity);
        shape.Leaves.Add(new(new(System.Reflection.Metadata.SignatureTypeCode.Object, UsesGc: true), 0));
        shape.Leaves.Add(new(payload, 0));
        Boxes.Add(value, shape);
        return shape;
    }

    public void RequireBoxAddress(BoxingType value)
    {
        var shape = Box(value);
        var payload = Bind(value.Storage);
        RequireAddress(payload);
        _ = ReferenceField(new(shape, 1), payload);
    }

    public void RequireBoxAllocation(BoxingType value)
    {
        foreach (var call in dependencies.Nodes.OfType<XuiNet.Reachability.Dependency.VirtualCall>())
            if (FrameworkTypes.IsRuntimeBase(call.Identity.DeclaringType) ||
                value.Identity.Interfaces().Any(contract => contract.IsAssignableTo(call.Identity.DeclaringType)))
                throw new NotSupportedException($"Virtual dispatch on boxed values requires an unboxing adapter: {value.Identity} -> {call.Identity}.");
        _ = Box(value);
    }

    private void DefineBoxTypes(ref TypeWriter writer)
    {
        foreach (var (value, shape) in Boxes)
        {
            shape.Type = writer.StructType([ValueTypeCode.EqRef, (WasmValue)Bind(value.Storage).WasmType], baseType: ClassRoot);
            var descriptor = Classes.Resolve(value.Identity);
            descriptor.WasmType = shape.Type;
            descriptor.FieldCount = 2;
            descriptor.Interfaces = Array.AsReadOnly(Classes.Types.Where(type => type.IsInterface &&
                value.Identity.Interfaces().Any(contract => contract.IsAssignableTo(type.Identity))).ToArray());
            // Reserve empty dispatch slots: boxed virtual/interface calls need adapters,
            // and must never accidentally address another class's methods.
            descriptor.TableStart = TableSize;
            TableSize += (uint)Classes.DispatchMethods.Count;
        }
    }
}
