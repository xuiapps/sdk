using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.WASM.Emitter.Rewrite;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Model = Xui.Compiler.XuiNet.DotNet;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using ValueType = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueType;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;

namespace Xui.Compiler.Tests.Wasm.Canvas;

internal sealed class CanvasRewriter : CodeRewriter
{
    private Model.Module root = null!, source = null!;
    private readonly Dictionary<string, SignatureTypeCode[]> operations = new();
    private readonly Dictionary<string, TypeIndex> types = new();
    private readonly Dictionary<string, FunctionIndex> functions = new();
    public override void BeginModule(Model.Module module) { root = source = module; operations.Clear(); types.Clear(); functions.Clear(); }
    public override void UseSource(Model.Module module) => source = module;
    private string? Match(int token, out SignatureTypeCode[] parameters)
    {
        parameters = [];
        var handle = MetadataTokens.EntityHandle(token);
        if (source != root || handle.Kind != HandleKind.MethodDefinition) return null;
        var method = source.GetMethod((MethodDefinitionHandle)handle);
        var owner = source.GetType(method.DeclaringType);
        if (owner.Namespace != "CanvasDemo" || owner.Name != "NativeCanvas" || (method.Attributes & MethodAttributes.Static) == 0) return null;
        var signature = SignatureReader.Method(source, method.Signature);
        int count = method.Name switch { "Clear" => 4, "FillRect" or "FillText" => 8, _ => 0 };
        if (count == 0 || signature.HasThis || signature.Result.Code != SignatureTypeCode.Void || signature.Parameters.Length != count) return null;
        parameters = signature.Parameters.Select(p => p.Code).ToArray();
        for (int i = 0; i < count; i++)
            if (signature.Parameters[i].IsByReference || signature.Parameters[i].Identity is not null ||
                parameters[i] != (i == 0 && method.Name == "FillText" ? SignatureTypeCode.String : SignatureTypeCode.Double)) return null;
        return method.Name;
    }
    public override bool Collect(Instruction instruction, ref ILReader reader)
    {
        if (instruction.Code != OpCode.Call) return false;
        string? name = Match(instruction.ReadToken(ref reader).Value, out var parameters);
        if (name is null) return false;
        operations.TryAdd(name, parameters); return true;
    }
    public override void DefineTypes(ref TypeWriter writer)
    {
        foreach (var (name, parameters) in operations)
            types.Add(name, writer.FunctionType(parameters.Select(p => (ValueType)(p == SignatureTypeCode.String ? ExternRef : F64)).ToArray()));
    }
    public override void ImportFunctions(ref ImportWriter writer)
    {
        foreach (var (name, type) in types) functions.Add(name, writer.Function("canvas", name, type));
    }
    public override bool TryRewrite(Instruction instruction, ref ILReader reader, CodeEmitter emitter, int offset)
    {
        if (instruction.Code != OpCode.Call) return false;
        string? name = Match(instruction.ReadToken(ref reader).Value, out var parameters);
        if (name is null) return false;
        for (int i = parameters.Length - 1; i >= 0; i--) emitter.Pop(parameters[i], instruction.Code, offset);
        emitter.Writer.Call(functions[name]); return true;
    }
}
