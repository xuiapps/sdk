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
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;

namespace Xui.Compiler.Tests.Wasm.TestApp;

internal sealed class CanvasCommandRewriter : CodeRewriter
{
    private Model.Module root = null!, source = null!;
    private bool used;
    private TypeIndex type;
    private FunctionIndex function;
    public override void BeginModule(Model.Module module) { root = source = module; used = false; }
    public override void UseSource(Model.Module module) => source = module;
    private bool Match(int token)
    {
        var handle = MetadataTokens.EntityHandle(token);
        if (source != root || handle.Kind != HandleKind.MethodDefinition) return false;
        var method = source.GetMethod((MethodDefinitionHandle)handle);
        var owner = source.GetType(method.DeclaringType);
        if (owner.Namespace != "TestAppCanvas" || owner.Name != "NativeCanvas" || method.Name != "Command") return false;
        var signature = SignatureReader.Method(source, method.Signature);
        return !signature.HasThis && signature.Result.Code == SignatureTypeCode.Void && signature.Parameters.SequenceEqual(
            new SignatureType[] { new(SignatureTypeCode.String), new(SignatureTypeCode.Double), new(SignatureTypeCode.Double),
                new(SignatureTypeCode.Double), new(SignatureTypeCode.Double), new(SignatureTypeCode.String) });
    }
    public override bool Collect(Instruction instruction, ref ILReader reader)
    {
        if (instruction.Code != OpCode.Call || !Match(instruction.ReadToken(ref reader).Value)) return false;
        used = true; return true;
    }
    public override void DefineTypes(ref TypeWriter writer) { if (used) type = writer.FunctionType([ExternRef, F64, F64, F64, F64, ExternRef]); }
    public override void ImportFunctions(ref ImportWriter writer) { if (used) function = writer.Function("canvas", "Command", type); }
    public override bool TryRewrite(Instruction instruction, ref ILReader reader, CodeEmitter emitter, int offset)
    {
        if (instruction.Code != OpCode.Call || !Match(instruction.ReadToken(ref reader).Value)) return false;
        emitter.Pop(SignatureTypeCode.String, instruction.Code, offset);
        for (int i = 0; i < 4; i++) emitter.Pop(SignatureTypeCode.Double, instruction.Code, offset);
        emitter.Pop(SignatureTypeCode.String, instruction.Code, offset);
        emitter.Writer.Call(function); return true;
    }
}
