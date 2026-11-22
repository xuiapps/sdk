using System.Diagnostics;

namespace Xui.Compiler.Tests.Wasm;

/// <summary>Prints WASM as WAT using wasm-tools on PATH, piping bytes through stdin and text through stdout.</summary>
internal static class WasmTools
{
    public static async Task<string> ToWatAsync(ReadOnlyMemory<byte> wasm)
    {
        var start = new ProcessStartInfo("wasm-tools")
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        start.ArgumentList.Add("print");
        start.ArgumentList.Add("-");
        start.ArgumentList.Add("--color");
        start.ArgumentList.Add("never");

        using var process = Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.StandardInput.BaseStream.WriteAsync(wasm);
        process.StandardInput.Close();
        await process.WaitForExitAsync();
        string wat = await stdout;
        string error = await stderr;
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"wasm-tools print failed ({process.ExitCode}): {error}");
        return wat;
    }
}
