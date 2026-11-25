using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.CodeFlow;

public class BclSelectorTests
{
    [Theory]
    [InlineData("GetArgumentName", "System.ExceptionArgument", "index")]
    [InlineData("GetResourceString", "System.ExceptionResource", "ArgumentOutOfRange_IndexMustBeLess")]
    public async Task ActualCoreLibSwitchBodiesExecute(string methodName, string enumName, string selectedName)
    {
        var core = typeof(object).Assembly;
        var enumType = core.GetType(enumName)!;
        var method = core.GetType("System.ThrowHelper")!.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic,
            [enumType])!;
        int[] values = [0, Convert.ToInt32(Enum.Parse(enumType, selectedName)), Enum.GetValues(enumType).Cast<object>().Max(Convert.ToInt32), -1, int.MaxValue];
        string expected = string.Concat(values.Select(value => method.Invoke(null, [Enum.ToObject(enumType, value)]) + "\n"));
        MemberReferenceHandle target = default, write = default;
        byte[] image = Frontend.Msil.Compile(il =>
        {
            foreach (int value in values) { il.LoadConstantI4(value); il.Call(target); il.Call(write); }
            il.LoadConstantI4(0); il.OpCode(ILOpCode.Ret);
        }, configureMetadata: metadata =>
        {
            var coreReference = MetadataTokens.AssemblyReferenceHandle(1);
            var helper = metadata.AddTypeReference(coreReference, metadata.GetOrAddString("System"), metadata.GetOrAddString("ThrowHelper"));
            var argument = metadata.AddTypeReference(coreReference, metadata.GetOrAddString("System"), metadata.GetOrAddString(enumName[7..]));
            var signature = new BlobBuilder();
            new BlobEncoder(signature).MethodSignature().Parameters(1, result => result.Type().String(),
                parameters => parameters.AddParameter().Type().Type(argument, isValueType: true));
            target = metadata.AddMemberReference(helper, metadata.GetOrAddString(methodName), metadata.GetOrAddBlob(signature));
            var console = typeof(Console).Assembly.GetName();
            var consoleReference = metadata.AddAssemblyReference(metadata.GetOrAddString(console.Name!), console.Version!, default,
                metadata.GetOrAddBlob(console.GetPublicKeyToken()!), 0, default);
            var consoleType = metadata.AddTypeReference(consoleReference, metadata.GetOrAddString("System"), metadata.GetOrAddString("Console"));
            signature = new BlobBuilder();
            new BlobEncoder(signature).MethodSignature().Parameters(1, result => result.Void(), parameters => parameters.AddParameter().Type().String());
            write = metadata.AddMemberReference(consoleType, metadata.GetOrAddString("WriteLine"), metadata.GetOrAddBlob(signature));
        });
        using var app = CanvasTests.Read(image);
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        byte[] wasm = new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter());
        Assert.Equal(expected, (await Node.ExecuteAsync(wasm)).Output);
        Assert.Contains("br_table", await WasmTools.ToWatAsync(wasm));
    }
}
