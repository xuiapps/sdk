using System.Diagnostics;
using System.Text.Json;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Repl;

internal static class Node
{
    public static Task<NodeResult> ExecuteAsync(byte[] wasm) => ExecuteCoreAsync(wasm, expectedError: null);

    public static async Task AssertThrowsAsync(byte[] wasm) => _ = await ExecuteCoreAsync(wasm, expectedError: "WebAssembly.Exception");

    public static async Task AssertTrapsAsync(byte[] wasm) => _ = await ExecuteCoreAsync(wasm, expectedError: "RuntimeError");

    private static async Task<NodeResult> ExecuteCoreAsync(byte[] wasm, string? expectedError)
    {
        var start = new ProcessStartInfo("node")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        start.ArgumentList.Add("--input-type=module");
        start.ArgumentList.Add("--eval");
        start.ArgumentList.Add(Xui.Compiler.WASM.Runtime.JavaScript.Source + "\n" + """
            import { Console } from 'node:console';
            import { Writable } from 'node:stream';
            const output = [];
            const hostConsole = new Console(new Writable({
                write(chunk, encoding, callback) { output.push(chunk.toString()); callback(); }
            }));
            const chunks = [];
            for await (const chunk of process.stdin) chunks.push(chunk);
            const module = await WebAssembly.compile(Buffer.concat(chunks), { builtins: ['js-string'] });
            const instance = await WebAssembly.instantiate(module, {
                ...globalThis.xuiRuntimeImports,
                xui: {
                    'console-log': value => hostConsole.log(value === null ? '' : typeof value === 'bigint' ? value.toString() : value),
                    'console-log-boolean': value => hostConsole.log(value ? 'True' : 'False')
                }
            });
            const result = instance.exports.main();
            // A void entry point completes successfully with exit code zero.
            process.stdout.write(JSON.stringify({ Output: output.join(''), ExitCode: result ?? 0 }));
            """);
        using var node = Process.Start(start)!;
        var stdout = node.StandardOutput.ReadToEndAsync();
        var stderr = node.StandardError.ReadToEndAsync();
        await node.StandardInput.BaseStream.WriteAsync(wasm);
        node.StandardInput.Close();
        await node.WaitForExitAsync();

        string error = await stderr;
        if (expectedError is not null)
        {
            Assert.NotEqual(0, node.ExitCode);
            Assert.Contains(expectedError, error);
            return default;
        }
        Assert.True(node.ExitCode == 0, error);
        Assert.Equal("", error);
        return JsonSerializer.Deserialize<NodeResult>(await stdout);
    }
}
