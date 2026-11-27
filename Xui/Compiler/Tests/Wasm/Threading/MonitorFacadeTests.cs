using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Threading;

[Collection(nameof(ManagedConsoleCollection))]
public class MonitorFacadeTests
{
    [Fact]
    public async Task ThreadingFacadePreservesReentrancyAndRelease()
    {
        MemberReferenceHandle create = default, enter = default, exit = default, entered = default;
        byte[] image = Frontend.Msil.Compile(il =>
        {
            il.OpCode(ILOpCode.Newobj); il.Token(create); il.StoreLocal(0);
            il.LoadLocal(0); il.Call(enter); il.LoadLocal(0); il.Call(enter);
            il.LoadLocal(0); il.Call(exit); il.LoadLocal(0); il.Call(entered); il.StoreLocal(1);
            il.LoadLocal(0); il.Call(exit); il.LoadLocal(0); il.Call(entered);
            il.LoadConstantI4(0); il.OpCode(ILOpCode.Ceq); il.LoadLocal(1); il.OpCode(ILOpCode.And); il.OpCode(ILOpCode.Ret);
        }, [SignatureTypeCode.Object, SignatureTypeCode.Int32], configureMetadata: metadata =>
        {
            string file = Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Threading.dll");
            var name = AssemblyName.GetAssemblyName(file);
            var assembly = metadata.AddAssemblyReference(metadata.GetOrAddString(name.Name!), name.Version!, default,
                metadata.GetOrAddBlob(name.GetPublicKeyToken()!), 0, default);
            var monitor = metadata.AddTypeReference(assembly, metadata.GetOrAddString("System.Threading"), metadata.GetOrAddString("Monitor"));
            var blob = new BlobBuilder();
            new BlobEncoder(blob).MethodSignature(isInstanceMethod: true).Parameters(0, r => r.Void(), _ => { });
            create = metadata.AddMemberReference(MetadataTokens.TypeReferenceHandle(1), metadata.GetOrAddString(".ctor"), metadata.GetOrAddBlob(blob));
            MemberReferenceHandle Method(string method, bool result = false)
            {
                var signature = new BlobBuilder();
                new BlobEncoder(signature).MethodSignature().Parameters(1, r => { if (result) r.Type().Boolean(); else r.Void(); }, p => p.AddParameter().Type().Object());
                return metadata.AddMemberReference(monitor, metadata.GetOrAddString(method), metadata.GetOrAddBlob(signature));
            }
            enter = Method("Enter"); exit = Method("Exit"); entered = Method("IsEntered", true);
        });
        var expected = GcTests.RunManaged(image);
        Assert.Equal(1, expected.ExitCode);
        using var app = CanvasTests.Read(image);
        using var framework = new Frontend.Framework("System.Threading.dll");
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        Assert.Equal(expected, await Node.ExecuteAsync(new WasmEmitter().Emit(app)));
    }
}
