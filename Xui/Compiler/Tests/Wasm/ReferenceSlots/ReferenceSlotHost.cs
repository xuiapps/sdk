using System.Diagnostics;
using System.Text.RegularExpressions;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.ReferenceSlots;

internal static class ReferenceSlotHost
{
    public static async Task AssertCleared(byte[] wasm, bool throws)
    {
        string wat = await WasmTools.ToWatAsync(wasm);
        var tables = Regex.Matches(wat, @"\(table \(;(\d+);\) \d+ \d+ (eqref|externref)\)");
        Assert.Equal(2, tables.Count);
        string exports = string.Join("\n", tables.Select(m => $"(export \"{m.Groups[2].Value}\" (table {m.Groups[1].Value}))"));
        // Export only in the inspected test module; production modules keep these tables private.
        wasm = await WasmTools.FromWatAsync(wat.Insert(wat.LastIndexOf(')'), exports));
        var start = new ProcessStartInfo("node") { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true };
        start.ArgumentList.Add("--input-type=module"); start.ArgumentList.Add("--eval");
        start.ArgumentList.Add($$"""
            import assert from 'node:assert/strict';
            const chunks = []; for await (const chunk of process.stdin) chunks.push(chunk);
            const module = await WebAssembly.compile(Buffer.concat(chunks), { builtins: ['js-string'] });
            const instance = await WebAssembly.instantiate(module);
            for (let invocation = 0; invocation < 3; invocation++) {
                {{(throws ? "assert.throws(() => instance.exports.main(), WebAssembly.Exception);" : "assert.equal(instance.exports.main(), 42);")}}
                for (const name of ['eqref', 'externref']) {
                    const table = instance.exports[name];
                    for (let i = 0; i < table.length; i++) assert.equal(table.get(i), null, `${name}[${i}] retained after return`);
                }
            }
            """);
        using var node = Process.Start(start)!;
        var stdout = node.StandardOutput.ReadToEndAsync(); var stderr = node.StandardError.ReadToEndAsync();
        await node.StandardInput.BaseStream.WriteAsync(wasm); node.StandardInput.Close();
        await node.WaitForExitAsync();
        Assert.True(node.ExitCode == 0, await stderr);
        Assert.Equal("", await stdout);
    }
}
