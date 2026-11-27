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
    private bool used, measured, images, imageSizes, groups, gpu;
    private TypeIndex type, measureType, imageType, imageSizeType, groupType, endGroupType, gpuType;
    private FunctionIndex function, measureFunction, imageFunction, imageSizeFunction, groupFunction, endGroupFunction, gpuFunction;
    public override void BeginModule(Model.Module module) { root = source = module; used = measured = images = imageSizes = groups = gpu = false; }
    public override void UseSource(Model.Module module) => source = module;
    private string? Match(int token)
    {
        var handle = MetadataTokens.EntityHandle(token);
        if (source != root || handle.Kind != HandleKind.MethodDefinition) return null;
        var method = source.GetMethod((MethodDefinitionHandle)handle);
        var owner = source.GetType(method.DeclaringType);
        if (owner.Namespace != "TestAppCanvas") return null;
        var signature = SignatureReader.Method(source, method.Signature);
        if (owner.Name == "NativeGpu" && method.Name == "Invoke" && !signature.HasThis && signature.Result.Code == SignatureTypeCode.Int32 &&
            signature.Parameters.SequenceEqual(new SignatureType[] { new(SignatureTypeCode.String), new(SignatureTypeCode.Int32),
                new(SignatureTypeCode.Int32), new(SignatureTypeCode.Int32), new(SignatureTypeCode.Int32), new(SignatureTypeCode.Int32), new(SignatureTypeCode.String) })) return "GpuInvoke";
        if (owner.Name != "NativeCanvas") return null;
        if (method.Name == "Group" && !signature.HasThis && signature.Result.Code == SignatureTypeCode.Int32 &&
            signature.Parameters.Length == 1 && signature.Parameters[0].Code == SignatureTypeCode.Object && !signature.Parameters[0].IsByReference) return "Group";
        if (method.Name == "EndGroup" && !signature.HasThis && signature.Result.Code == SignatureTypeCode.Int32 && signature.Parameters.Length == 0) return "EndGroup";
        if (method.Name == "LoadImage" && !signature.HasThis && signature.Result.Code == SignatureTypeCode.Int32 &&
            signature.Parameters.SequenceEqual(new SignatureType[] { new(SignatureTypeCode.String) })) return "LoadImage";
        if (method.Name == "ImageSize" && !signature.HasThis && signature.Result.Code == SignatureTypeCode.Double &&
            signature.Parameters.SequenceEqual(new SignatureType[] { new(SignatureTypeCode.Int32), new(SignatureTypeCode.Int32) })) return "ImageSize";
        if (method.Name == "Measure" && !signature.HasThis && signature.Result.Code == SignatureTypeCode.Double &&
            signature.Parameters.SequenceEqual(new SignatureType[] { new(SignatureTypeCode.String), new(SignatureTypeCode.Int32) })) return "Measure";
        return method.Name == "Command" && !signature.HasThis && signature.Result.Code == SignatureTypeCode.Void && signature.Parameters.SequenceEqual(
            new SignatureType[] { new(SignatureTypeCode.String), new(SignatureTypeCode.Double), new(SignatureTypeCode.Double),
                new(SignatureTypeCode.Double), new(SignatureTypeCode.Double), new(SignatureTypeCode.String) }) ? "Command" : null;
    }
    public override bool Collect(Instruction instruction, ref ILReader reader)
    {
        if (instruction.Code != OpCode.Call || Match(instruction.ReadToken(ref reader).Value) is not { } name) return false;
        if (name == "GpuInvoke") gpu = true; else if (name is "Group" or "EndGroup") groups = true; else if (name == "Measure") measured = true; else if (name == "LoadImage") images = true; else if (name == "ImageSize") imageSizes = true; else used = true;
        return true;
    }
    public override void DefineTypes(ref TypeWriter writer)
    {
        if (gpu) gpuType = writer.FunctionType([ExternRef, I32, I32, I32, I32, I32, ExternRef], I32);
        if (groups) { groupType = writer.FunctionType([EqRef], I32); endGroupType = writer.FunctionType([], I32); }
        if (images) imageType = writer.FunctionType([ExternRef], I32);
        if (imageSizes) imageSizeType = writer.FunctionType([I32, I32], F64);
        if (used) type = writer.FunctionType([ExternRef, F64, F64, F64, F64, ExternRef]);
        if (measured) measureType = writer.FunctionType([ExternRef, I32], [F64]);
    }
    public override void ImportFunctions(ref ImportWriter writer)
    {
        if (gpu) gpuFunction = writer.Function("gpu", "Invoke", gpuType);
        if (groups) { groupFunction = writer.Function("canvas", "Group", groupType); endGroupFunction = writer.Function("canvas", "EndGroup", endGroupType); }
        if (images) imageFunction = writer.Function("canvas", "LoadImage", imageType);
        if (imageSizes) imageSizeFunction = writer.Function("canvas", "ImageSize", imageSizeType);
        if (used) function = writer.Function("canvas", "Command", type);
        if (measured) measureFunction = writer.Function("canvas", "Measure", measureType);
    }
    public override bool TryRewrite(Instruction instruction, ref ILReader reader, CodeEmitter emitter, int offset)
    {
        if (instruction.Code != OpCode.Call || Match(instruction.ReadToken(ref reader).Value) is not { } name) return false;
        if (name == "GpuInvoke")
        {
            emitter.Pop(SignatureTypeCode.String, instruction.Code, offset);
            for (int i = 0; i < 5; i++) emitter.Pop(SignatureTypeCode.Int32, instruction.Code, offset);
            emitter.Pop(SignatureTypeCode.String, instruction.Code, offset);
            emitter.Writer.Call(gpuFunction); emitter.Push(new(SignatureTypeCode.Int32)); return true;
        }
        if (name is "Group" or "EndGroup")
        {
            if (name == "Group") emitter.Pop(SignatureTypeCode.Object, instruction.Code, offset);
            emitter.Writer.Call(name == "Group" ? groupFunction : endGroupFunction);
            emitter.Push(new(SignatureTypeCode.Int32)); return true;
        }
        if (name == "LoadImage")
        {
            emitter.Pop(SignatureTypeCode.String, instruction.Code, offset);
            emitter.Writer.Call(imageFunction); emitter.Push(new(SignatureTypeCode.Int32)); return true;
        }
        if (name == "ImageSize")
        {
            emitter.Pop(SignatureTypeCode.Int32, instruction.Code, offset); emitter.Pop(SignatureTypeCode.Int32, instruction.Code, offset);
            emitter.Writer.Call(imageSizeFunction); emitter.Push(new(SignatureTypeCode.Double)); return true;
        }
        if (name == "Measure")
        {
            emitter.Pop(SignatureTypeCode.Int32, instruction.Code, offset);
            emitter.Pop(SignatureTypeCode.String, instruction.Code, offset);
            emitter.Writer.Call(measureFunction);
            emitter.Push(new(SignatureTypeCode.Double));
            return true;
        }
        emitter.Pop(SignatureTypeCode.String, instruction.Code, offset);
        for (int i = 0; i < 4; i++) emitter.Pop(SignatureTypeCode.Double, instruction.Code, offset);
        emitter.Pop(SignatureTypeCode.String, instruction.Code, offset);
        emitter.Writer.Call(function); return true;
    }
}
