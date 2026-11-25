using System.Text;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Delegates;

public class LargeTableTests
{
    [Fact]
    public async Task VirtualDelegatesResolveAcrossLargeMethodTableChunks()
    {
        var source = new StringBuilder("using System; public class Base {");
        for (int i = 0; i < 103; i++) source.Append($"public virtual int M{i}() => {i};");
        source.Append('}');
        for (int i = 0; i < 105; i++) source.Append($"public class C{i} : Base {{ public override int M0() => {i}; }}");
        source.Append("public static class Program { public static int Main() { Base[] values = [");
        source.AppendJoin(',', Enumerable.Range(0, 105).Select(i => $"new C{i}()"));
        source.Append("]; Func<int> first=values[0].M0, last=values[104].M0, same=values[104].M0; return first()==0 && last()==104 && last==same && first!=last ? 42 : 1; } }");
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile(source.ToString()));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        var emitter = new WasmEmitter();
        byte[] wasm = emitter.Emit(app);
        Assert.True(emitter.Types.Max(type => type.TableStart) > 10000);
        Assert.Equal(42, (await Node.ExecuteAsync(wasm)).ExitCode);
    }
}
