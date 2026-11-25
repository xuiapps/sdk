using System.Net;

namespace Xui.Compiler.Tests.Wasm.TestApp;

internal static class BrowserShellPage
{
    public static string Html(byte[] wasm) => $$"""
        <!doctype html>
        <html lang="en">
        <meta charset="utf-8">
        <meta name="viewport" content="width=device-width,initial-scale=1">
        <title>Xui compiler · TestApp home</title>
        <style>
          {{Font("Regular", 400)}}
          {{Font("Medium", 500)}}
          {{Font("SemiBold", 600)}}
          {{Font("Bold", 700)}}
          html, body { margin:0; min-height:100%; background:white; font:13px system-ui,sans-serif; }
          main { width:min(393px,100%); margin:24px auto; }
          canvas { display:block; width:100%; height:auto; }
          #status, details { padding:0 12px; color:#555; }
          button { margin:8px 12px; padding:6px 12px; }
          pre { white-space:pre-wrap; font-size:11px; }
        </style>
        <main>
          <canvas id="canvas" width="393" height="759" aria-label="Real Xui TestApp mobile home screen"></canvas>
          <p id="status" role="status">Starting…</p>
          <button id="run">Render again</button>
          <p>First-frame preview: browser Canvas catalogue; input routing and diagnostics overlay are not enabled.</p>
          <details><summary>Inter font license</summary><pre>{{WebUtility.HtmlEncode(File.ReadAllText(Path.Combine(Frontend.SourceProject.XuiRoot, "Core/Fonts/Inter/LICENSE.txt")))}}</pre></details>
        </main>
        <script>
        {{Xui.Compiler.WASM.Runtime.JavaScript.Source}}
        {{BrowserCanvasHost.JavaScript}}
        const bytes = Uint8Array.from(atob('{{Convert.ToBase64String(wasm)}}'), c => c.charCodeAt(0));
        async function run() {
            window.demoResult = null;
            const button = document.getElementById('run'); button.disabled = true;
            try {
                await Promise.all([400,500,600,700].map(weight => document.fonts.load(`${weight} 20px Inter`)));
                const canvas = document.getElementById('canvas'), ratio = devicePixelRatio || 1;
                canvas.width = Math.round(393*ratio); canvas.height = Math.round(759*ratio);
                const context = canvas.getContext('2d'); context.scale(ratio,ratio);
                context.fillStyle = 'white'; context.fillRect(0,0,393,759);
                const commands = [];
                const module = await WebAssembly.compile(bytes, { builtins: ['js-string'] });
                const instance = await WebAssembly.instantiate(module, { ...globalThis.xuiRuntimeImports, canvas: canvasImports(context,commands) });
                const exitCode = instance.exports.main();
                window.demoResult = { commands, exitCode };
                document.getElementById('status').textContent = `TestApp rendered · Exit code ${exitCode}`;
            } catch (error) {
                window.demoResult = { error: String(error) };
                document.getElementById('status').textContent = `Unable to render: ${error.message ?? error}`;
            } finally { button.disabled = false; }
        }
        document.getElementById('run').addEventListener('click',run);
        run();
        </script>
        </html>
        """ + "\n";

    private static string Font(string name, int weight) =>
        $"@font-face {{ font-family:Inter; font-weight:{weight}; src:url(data:font/ttf;base64,{Convert.ToBase64String(File.ReadAllBytes(Path.Combine(Frontend.SourceProject.XuiRoot, $"Core/Fonts/Inter/Inter-{name}.ttf")))}) format('truetype'); }}";
}
