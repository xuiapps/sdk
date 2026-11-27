using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.XuiNet.Reachability;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using BodyWriter = Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody.Writer;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using GlobalWriter = Xui.Compiler.WASM.Wasm.Section.Global.Writer;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class StorageModel
{
    public Dictionary<TypeIdentity, StaticType> Statics { get; } = new();
    public Dictionary<XuiNet.DotNet.Module, StaticType> ModuleInitializers { get; } = new();
    public TypeIndex StaticInitializerType { get; private set; }

    public void CollectStatics(DependencyGraph graph)
    {
        StaticType Type(TypeIdentity identity)
        {
            if (!Statics.TryGetValue(identity, out var type)) Statics.Add(identity, type = new(identity));
            return type;
        }
        foreach (var node in graph.Nodes.OfType<Dependency.Initialization>())
        {
            if (TypeInitialization.Constructor(node.Identity) is not { } constructor) continue;
            var type = Type(node.Identity);
            type.Constructor = constructor;
            if (node.Identity.Module.GetType((TypeDefinitionHandle)node.Identity.Handle).Name == "<Module>")
                ModuleInitializers.Add(node.Identity.Module, type);
            ExceptionsEnabled = InnerExceptionsEnabled = ArgumentExceptionsEnabled = true;
            ExtraExceptionKinds.Add(ExceptionKind.TypeInitializationException);
        }
        foreach (var node in graph.Nodes.OfType<Dependency.Field>())
        {
            var identity = node.Identity;
            var field = identity.DeclaringType.Module.GetField(identity.Handle);
            // RVA spans use StaticData's immutable linear bytes; literals are metadata,
            // not mutable fields. Retaining their tokens does not allocate a GC holder.
            if ((field.Attributes & FieldAttributes.Static) == 0 ||
                (field.Attributes & (FieldAttributes.HasFieldRVA | FieldAttributes.Literal)) != 0) continue;
            try
            {
                var type = Type(identity.DeclaringType);
                var declared = SignatureReader.Field(identity.DeclaringType.Module, field.Signature, identity.DeclaringType.Arguments);
                if (declared.IsByReference) throw new NotSupportedException("Static byref fields are not supported.");
                var storage = Bind(declared);
                int first = type.Fields.Leaves.Count;
                if (storage.Identity is { IsValueType: true } value)
                    type.Fields.Leaves.AddRange(Shape(value).Leaves);
                else
                {
                    type.Fields.Leaves.Add(new(storage, 0));
                    if (storage.Identity is { } reference) _ = Shape(reference);
                }
                type.Fields.Fields.Add(identity, new(storage, first, type.Fields.Leaves.Count - first));
            }
            catch (NotSupportedException error)
            {
                error.Data["Xui.StaticField"] = $"{identity.DeclaringType}.{field.Name}";
                error.Data["Xui.DependencyPath"] = string.Join("\n → ", graph.PathTo(node));
                throw;
            }
        }
    }

    private void DefineStaticTypes(ref TypeWriter writer)
    {
        if (Statics.Values.Any(type => type.Constructor is not null)) StaticInitializerType = writer.FunctionType();
        foreach (var type in Statics.Values.Where(type => type.Fields.Fields.Count != 0))
            type.Fields.Type = writer.StructType(type.Fields.Leaves.Select(leaf => (Wasm.Section.TypeDefinitions.ValueType)leaf.Storage.WasmType).ToArray());
    }

    public void DefineStaticGlobals(ref GlobalWriter globals)
    {
        foreach (var type in Statics.Values)
        {
            if (type.Fields.Fields.Count != 0) type.Holder = globals.DefaultStruct(type.Fields.Type);
            if (type.Constructor is null) continue;
            type.State = globals.I32((int)TypeInitializationState.Uninitialized, Mutability.Mutable);
            type.Failure = globals.RefNull(ValueTypeCode.ExnRef, Mutability.Mutable);
        }
    }

    public void EmitStaticInitializer(BodyWriter writer, StaticType type, MethodPlan constructor, EmissionRuntime runtime,
        IReadOnlyDictionary<string, (DataIndex Data, GlobalIndex Global)> literals)
    {
        // Reentrant same-agent access sees zero/partially initialized fields. Completed
        // initializers never rerun, and failures keep their original managed cause.
        writer.GlobalGet(type.State).I32Const((int)TypeInitializationState.Initialized).I32Eq().If().Return().End();
        writer.GlobalGet(type.State).I32Const((int)TypeInitializationState.Initializing).I32Eq().If().Return().End();
        writer.GlobalGet(type.State).I32Const((int)TypeInitializationState.Failed).I32Eq().If().GlobalGet(type.Failure).ThrowRef().End();
        writer.I32Const((int)TypeInitializationState.Initializing).GlobalSet(type.State);
        // Cache foreign host exceptions too, without pretending they are managed objects.
        writer.Block(ExceptionReferenceBlock).TryCatchAllRef(new(0));
        writer.Block(ExceptionPayloadBlock).TryCatch(ExceptionTag, new(0));
        writer.Call(constructor.Function).I32Const((int)TypeInitializationState.Initialized).GlobalSet(type.State).Return().End();
        writer.Unreachable().End().LocalSet(new(0));
        writer.I32Const((int)ExceptionKind.TypeInitializationException);
        EmitStaticString(writer, type.FailureMessage, runtime, literals);
        // The auxiliary string payload is a parameter name for argument exceptions,
        // and a type name for initialization failures; the kinds cannot overlap.
        EmitStaticString(writer, type.Name, runtime, literals);
        writer.LocalGet(new(0));
        if (ActualValueExceptionsEnabled) writer.RefNull(ValueTypeCode.EqRef);
        WriteXmlExceptionDefaults(writer);
        writer.StructNew(ExceptionType).Throw(ExceptionTag);
        writer.End().Unreachable().End().GlobalSet(type.Failure);
        writer.I32Const((int)TypeInitializationState.Failed).GlobalSet(type.State).GlobalGet(type.Failure).ThrowRef();
    }

    private static void EmitStaticString(BodyWriter writer, string text, EmissionRuntime runtime,
        IReadOnlyDictionary<string, (DataIndex Data, GlobalIndex Global)> literals)
    {
        var literal = literals[text];
        writer.GlobalGet(literal.Global).RefIsNull().If().I32Const(0).I32Const(text.Length)
            .ArrayNewData(runtime.Utf16ArrayType, literal.Data).I32Const(0).I32Const(text.Length)
            .Call(runtime.FromCharCodeArrayFunction).GlobalSet(literal.Global).End().GlobalGet(literal.Global);
    }
}
