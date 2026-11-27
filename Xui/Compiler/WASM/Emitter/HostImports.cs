using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.WASM.Wasm.Indices;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;

namespace Xui.Compiler.WASM.Emitter;

internal sealed class HostImports
{
    private readonly Dictionary<MethodIdentity, RuntimeImport> imports = new();
    private readonly Dictionary<MethodIdentity, TypeIndex> types = new();
    public Dictionary<MethodIdentity, FunctionIndex> Functions { get; } = new();
    public bool UsesNativeHeap => imports.Values.Any(import => import.ModuleName == "xui-memory");
    public void Require(RuntimeImport import) => imports.TryAdd(import.Method, import);
    public void DefineTypes(ref TypeWriter writer, StorageModel storage)
    {
        foreach (var (method, import) in imports)
            types.Add(method, writer.FunctionType(import.Signature.Parameters.Select(p => (Wasm.Section.TypeDefinitions.ValueType)storage.Bind(p).WasmType).ToArray(),
                import.Signature.Result.Code == System.Reflection.Metadata.SignatureTypeCode.Void ? null : (Wasm.Section.TypeDefinitions.ValueType?)storage.Bind(import.Signature.Result).WasmType));
    }
    public void Import(ref ImportWriter writer)
    {
        foreach (var (method, import) in imports) Functions.Add(method, writer.Function(import.ModuleName, import.Name, types[method]));
    }
}
