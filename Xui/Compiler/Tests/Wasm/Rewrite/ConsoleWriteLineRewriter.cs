using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.WASM.Emitter.Rewrite;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet.Op;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using IR = Xui.Compiler.XuiNet.DotNet;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;

namespace Xui.Compiler.Tests.Wasm.Rewrite;

// Test host policy: replace supported Console.WriteLine overloads with JavaScript logging imports.
internal class ConsoleWriteLineRewriter : CodeRewriter
{
    private IR.Module source = null!;
    private readonly List<SignatureTypeCode> parameters = new();
    private readonly Dictionary<SignatureTypeCode, TypeIndex> types = new();
    private readonly Dictionary<SignatureTypeCode, FunctionIndex> functions = new();

    public override void BeginModule(IR.Module source)
    {
        this.source = source;
        parameters.Clear();
        types.Clear();
        functions.Clear();
    }

    public override bool Collect(Instruction instruction, ref ILReader reader)
    {
        if (instruction.Code != OpCode.Call) return false;
        var parameter = GetConsoleWriteLineParameter(source, instruction.ReadToken(ref reader).Value);
        if (parameter is not { } type) return false;
        if (!parameters.Contains(type)) parameters.Add(type);
        return true;
    }

    public override void DefineTypes(ref TypeWriter writer)
    {
        foreach (var parameter in parameters)
        {
            var wasmType = parameter switch
            {
                SignatureTypeCode.String => ExternRef,
                SignatureTypeCode.Int32 or SignatureTypeCode.Boolean => I32,
                SignatureTypeCode.Int64 => I64,
                SignatureTypeCode.Single => F32,
                SignatureTypeCode.Double => F64,
                _ => throw new NotSupportedException($"Unsupported WriteLine parameter: {parameter}.")
            };
            types.Add(parameter, writer.FunctionType([wasmType]));
        }
    }

    public override void ImportFunctions(ref ImportWriter imports)
    {
        foreach (var parameter in parameters)
            functions.Add(parameter, imports.Function("xui",
                parameter == SignatureTypeCode.Boolean ? "console-log-boolean" : "console-log",
                types[parameter]));
    }

    public override bool TryRewrite(Instruction instruction, ref ILReader reader, CodeEmitter emitter, int offset)
    {
        if (instruction.Code != OpCode.Call) return false;
        var parameter = GetConsoleWriteLineParameter(source, instruction.ReadToken(ref reader).Value);
        if (parameter is not { } type) return false;
        var stackType = type switch
        {
            SignatureTypeCode.Boolean => SignatureTypeCode.Int32,
            SignatureTypeCode.Single => SignatureTypeCode.Double,
            _ => type
        };
        emitter.Pop(stackType, instruction.Code, offset);
        if (type == SignatureTypeCode.Single) emitter.Writer.F32DemoteF64();
        emitter.Writer.Call(functions[type]);
        return true;
    }

    private static SignatureTypeCode? GetConsoleWriteLineParameter(IR.Module module, int token)
    {
        var handle = MetadataTokens.EntityHandle(token);
        if (handle.Kind != HandleKind.MemberReference) return null;
        var member = module.GetMemberReference((MemberReferenceHandle)handle);
        if (member.Kind != MemberReferenceKind.Method || member.Name != "WriteLine" ||
            member.Parent.Kind != HandleKind.TypeReference) return null;
        var type = module.GetTypeReference((TypeReferenceHandle)member.Parent);
        if (type.Namespace != "System" || type.Name != "Console" ||
            type.ResolutionScope.Kind != HandleKind.AssemblyReference ||
            module.GetAssemblyReference((AssemblyReferenceHandle)type.ResolutionScope).Name != "System.Console") return null;
        // Read the overload's declared type: bool and int both use i32 on the stack.
        var signature = member.Signature;
        if (signature.Length != 4 ||
            signature.ReadSignatureHeader().RawValue != (byte)SignatureKind.Method ||
            signature.ReadCompressedInteger() != 1 ||
            signature.ReadSignatureTypeCode() != SignatureTypeCode.Void) return null;
        var parameter = signature.ReadSignatureTypeCode();
        return parameter is SignatureTypeCode.String or SignatureTypeCode.Int32 or
            SignatureTypeCode.Int64 or SignatureTypeCode.Single or SignatureTypeCode.Double or SignatureTypeCode.Boolean
            ? parameter : null;
    }
}
