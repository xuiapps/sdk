using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.WASM.Wasm.Indices;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Weakly canonicalized object-slot carriers for native JS strings, scoped to a module's String descriptor.</summary>
internal sealed class StringObjectsRuntime
{
    public GcShape? Shape { get; private set; }
    private TypeIndex lookupType, registerType;
    public FunctionIndex Lookup { get; private set; }
    public FunctionIndex Register { get; private set; }

    public void Enable(StorageModel storage, XuiNet.DotNet.Module module)
    {
        if (Shape is not null) return;
        var core = module.Compilation?.Assemblies.FirstOrDefault(a => a.Identity.Name is "System.Private.CoreLib" or "mscorlib")
            ?? throw new NotSupportedException("String object storage requires a linked CoreLib.");
        var handle = core.ManifestModule.Types.Single(h => {
            var type = core.ManifestModule.GetType(h); return type.Namespace == "System" && type.Name == "String";
        });
        Shape = storage.Shape(new(core.ManifestModule, handle));
        storage.ExceptionsEnabled = true;
    }

    public void DefineTypes(ref TypeWriter writer)
    {
        if (Shape is null) return;
        lookupType = writer.FunctionType([EqRef, ExternRef], EqRef);
        registerType = writer.FunctionType([EqRef, ExternRef, EqRef], EqRef);
    }

    public void Import(ref ImportWriter writer)
    {
        if (Shape is null) return;
        Lookup = writer.Function("xui-string", "object-lookup", lookupType);
        Register = writer.Function("xui-string", "object-register", registerType);
    }
}
