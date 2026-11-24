using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Canvas;

internal static class CanvasHost
{
    // Shared by Node's recording host and the standalone page. All geometry arrives from WASM.
    public const string JavaScript = """
        function canvasImports(context, commands) {
            const color = (r,g,b,a) => `rgba(${Math.round(r*255)},${Math.round(g*255)},${Math.round(b*255)},${a})`;
            return {
                Clear(r,g,b,a) {
                    commands.push(['Clear',r,g,b,a]);
                    if (context) { context.clearRect(0,0,640,360); context.fillStyle = color(r,g,b,a); context.fillRect(0,0,640,360); }
                },
                FillRect(x,y,width,height,r,g,b,a) {
                    commands.push(['FillRect',x,y,width,height,r,g,b,a]);
                    if (context) { context.fillStyle = color(r,g,b,a); context.fillRect(x,y,width,height); }
                },
                FillText(text,x,y,size,r,g,b,a) {
                    commands.push(['FillText',text,x,y,size,r,g,b,a]);
                    if (context) { context.fillStyle = color(r,g,b,a); context.font = `${size}px sans-serif`; context.fillText(text,x,y); }
                }
            };
        }
        """;

    public static async Task<(string Commands, int ExitCode)> Record(byte[] wasm, string? javaScript = null)
    {
        var start = new ProcessStartInfo("node") { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true };
        start.ArgumentList.Add("--input-type=module"); start.ArgumentList.Add("--eval");
        start.ArgumentList.Add(Xui.Compiler.WASM.Runtime.JavaScript.Source + "\n" + (javaScript ?? JavaScript) + """
            const chunks = []; for await (const chunk of process.stdin) chunks.push(chunk);
            const module = await WebAssembly.compile(Buffer.concat(chunks), { builtins: ['js-string'] });
            const commands = [];
            const instance = await WebAssembly.instantiate(module, { ...globalThis.xuiRuntimeImports, canvas: canvasImports(null, commands) });
            const exitCode = instance.exports.main();
            process.stdout.write(JSON.stringify({ commands, exitCode }));
            """);
        using var node = Process.Start(start)!;
        var output = node.StandardOutput.ReadToEndAsync(); var error = node.StandardError.ReadToEndAsync();
        await node.StandardInput.BaseStream.WriteAsync(wasm); node.StandardInput.Close();
        await node.WaitForExitAsync(); Assert.True(node.ExitCode == 0, await error);
        using var json = JsonDocument.Parse(await output);
        return (JsonSerializer.Serialize(json.RootElement.GetProperty("commands"), new JsonSerializerOptions { WriteIndented = true }), json.RootElement.GetProperty("exitCode").GetInt32());
    }

    public static string Html(string title, string source, byte[] wasm, string? javaScript = null) => $$"""
        <!doctype html>
        <html lang="en">
        <meta charset="utf-8">
        <meta name="viewport" content="width=device-width,initial-scale=1">
        <title>Xui WASM · {{WebUtility.HtmlEncode(title)}}</title>
        <style>
          :root { color-scheme: dark; font-family: system-ui,sans-serif; background:#081122; color:#edf4ff; }
          body { max-width:880px; margin:48px auto; padding:0 20px; }
          header { display:flex; align-items:center; justify-content:space-between; gap:20px; }
          h1 { font-size:26px; font-weight:600; margin:0; } p { color:#aabbcf; }
          button { background:#204b78; color:white; border:1px solid #547da4; border-radius:8px; padding:10px 20px; cursor:pointer; }
          canvas { display:block; width:100%; height:auto; border:1px solid #2b405c; border-radius:12px; }
          #status { min-height:24px; } pre { white-space:pre-wrap; overflow-wrap:anywhere; font-size:13px; line-height:1.5; }
          details { margin-top:24px; } summary { cursor:pointer; color:#aabbcf; }
        </style>
        <header><h1>Xui WASM / {{WebUtility.HtmlEncode(title)}}</h1><button id="run">Run again</button></header>
        <p>A small canvas scene compiled from C# with Xui geometry.</p>
        <canvas id="canvas" width="640" height="360" aria-label="{{WebUtility.HtmlEncode(title)}} canvas demo"></canvas>
        <p id="status" role="status">Starting…</p>
        <details><summary>C# source</summary><pre>{{WebUtility.HtmlEncode(source)}}</pre></details>
        <script>
        {{Xui.Compiler.WASM.Runtime.JavaScript.Source}}
        {{javaScript ?? JavaScript}}
        const bytes = Uint8Array.from(atob('{{Convert.ToBase64String(wasm)}}'), c => c.charCodeAt(0));
        async function run() {
            const button = document.getElementById('run');
            const status = document.getElementById('status');
            button.disabled = true; window.demoResult = null;
            try {
                const commands = [];
                const module = await WebAssembly.compile(bytes, { builtins: ['js-string'] });
                const context = document.getElementById('canvas').getContext('2d');
                const instance = await WebAssembly.instantiate(module, { ...globalThis.xuiRuntimeImports, canvas: canvasImports(context, commands) });
                const exitCode = instance.exports.main();
                window.demoResult = { commands, exitCode };
                status.textContent = `Rendered ${commands.length} drawing commands · Exit code ${exitCode}`;
            } catch (error) {
                window.demoResult = { error: String(error) };
                status.textContent = `Unable to run this demo: ${error.message ?? error}. Requires a browser with WASM GC and JS string builtins.`;
            } finally { button.disabled = false; }
        }
        document.getElementById('run').addEventListener('click', run);
        run();
        </script>
        </html>
        """ + "\n";
}
