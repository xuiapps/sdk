using System.Diagnostics;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Exports;

internal static class ExportHost
{
    public static async Task<string> RunAsync(byte[] wasm, string script)
    {
        var start = new ProcessStartInfo("node") { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true };
        string path = Path.Combine(Path.GetTempPath(), "xui-export-" + Guid.NewGuid().ToString("N") + ".mjs");
        await File.WriteAllTextAsync(path, """
            import assert from 'node:assert/strict';
            const chunks = []; for await (const chunk of process.stdin) chunks.push(chunk);
            const module = await WebAssembly.compile(Buffer.concat(chunks), { builtins: ['js-string'] });
            """ + "\n" + Xui.Compiler.WASM.Runtime.JavaScript.Source + "\n" + script);
        start.ArgumentList.Add(path);
        try
        {
            using var process = Process.Start(start)!;
            var output = process.StandardOutput.ReadToEndAsync(); var error = process.StandardError.ReadToEndAsync();
            await process.StandardInput.BaseStream.WriteAsync(wasm); process.StandardInput.Close();
            await process.WaitForExitAsync();
            Assert.True(process.ExitCode == 0, await error);
            return await output;
        }
        finally { File.Delete(path); }
    }
}
