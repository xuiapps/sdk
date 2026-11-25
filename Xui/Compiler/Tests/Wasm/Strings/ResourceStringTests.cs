using System.Globalization;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Resources;
using System.Text;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Strings;

public class ResourceStringTests
{
    [Theory]
    [InlineData("ArgumentOutOfRange_IndexMustBeLess")]
    [InlineData("ArgumentOutOfRange_IndexMustBeLessOrEqual")]
    [InlineData("InvalidOperation_EnumFailedVersion")]
    public async Task BclGettersEmitOnlyTheirNeutralResourceText(string key)
    {
        using var app = CanvasTests.Read(Caller(key));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        byte[] wasm = new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter());
        string expected = NeutralString(key);
        Assert.Equal(expected + "\n", (await Node.ExecuteAsync(wasm)).Output);
        Assert.True(wasm.AsSpan().IndexOf(Encoding.Unicode.GetBytes(expected)) >= 0);
        Assert.True(wasm.AsSpan().IndexOf(Encoding.Unicode.GetBytes(NeutralString("ArgumentNull_Generic"))) < 0);
    }

    [Fact]
    public async Task CompilationReadsTheSuppliedPeRatherThanTheCompilersLoadedBcl()
    {
        const string key = "ArgumentOutOfRange_IndexMustBeLess";
        string original = NeutralString(key);
        byte[] image = File.ReadAllBytes(typeof(object).Assembly.Location);
        int offset = image.AsSpan().IndexOf(Encoding.UTF8.GetBytes(original));
        Assert.True(offset >= 0);
        // Resource values are UTF-8; equal byte length preserves the resource table's offsets.
        Encoding.UTF8.GetBytes("IMAGE").CopyTo(image, offset);
        string expected = "IMAGE" + original[5..];
        using var suppliedCore = CanvasTests.Read(image);
        using var app = CanvasTests.Read(Caller(key));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, suppliedCore,
            .. framework.Assemblies.Where(a => a.Identity.Name != "System.Private.CoreLib")]);
        var result = await Node.ExecuteAsync(new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter()));
        Assert.Equal(expected + "\n", result.Output);
    }

    private static string NeutralString(string key)
    {
        var resources = new ResourceManager("System.Private.CoreLib.Strings", typeof(object).Assembly);
        try { return resources.GetString(key, CultureInfo.InvariantCulture)!; }
        finally { resources.ReleaseAllResources(); }
    }

    private static byte[] Caller(string key)
    {
        MemberReferenceHandle getter = default, write = default;
        return Frontend.Msil.Compile(il =>
        {
            il.Call(getter); il.Call(write); il.LoadConstantI4(0); il.OpCode(ILOpCode.Ret);
        }, configureMetadata: metadata =>
        {
            var sr = metadata.AddTypeReference(MetadataTokens.AssemblyReferenceHandle(1), metadata.GetOrAddString("System"), metadata.GetOrAddString("SR"));
            var signature = new BlobBuilder();
            new BlobEncoder(signature).MethodSignature().Parameters(0, result => result.Type().String(), _ => { });
            getter = metadata.AddMemberReference(sr, metadata.GetOrAddString("get_" + key), metadata.GetOrAddBlob(signature));
            var console = typeof(Console).Assembly.GetName();
            var consoleAssembly = metadata.AddAssemblyReference(metadata.GetOrAddString(console.Name!), console.Version!, default,
                metadata.GetOrAddBlob(console.GetPublicKeyToken()!), 0, default);
            var consoleType = metadata.AddTypeReference(consoleAssembly, metadata.GetOrAddString("System"), metadata.GetOrAddString("Console"));
            signature = new BlobBuilder();
            new BlobEncoder(signature).MethodSignature().Parameters(1, result => result.Void(), parameters => parameters.AddParameter().Type().String());
            write = metadata.AddMemberReference(consoleType, metadata.GetOrAddString("WriteLine"), metadata.GetOrAddBlob(signature));
        });
    }
}
