using System.Net;

namespace Xui.Compiler.Tests.Wasm.TestApp;

internal static class BrowserInteractivePage
{
    public static string Html(byte[] wasm, string host) => $$"""
        <!doctype html>
        <html lang="en">
        <meta charset="utf-8">
        <meta name="viewport" content="width=device-width,initial-scale=1">
        <title>Xui compiler · interactive TestApp</title>
        <style>
          {{BrowserShellPage.Font("Regular", 400)}}
          {{BrowserShellPage.Font("Medium", 500)}}
          {{BrowserShellPage.Font("SemiBold", 600)}}
          {{BrowserShellPage.Font("Bold", 700)}}
          html, body { margin:0; width:100%; height:100%; overflow:hidden; background:white; }
          canvas { display:block; width:100%; height:100%; touch-action:none; outline:none; }
          #status { position:fixed; left:12px; bottom:6px; margin:0; color:#666; font:11px system-ui,sans-serif; pointer-events:none; }
          details { position:fixed; right:12px; bottom:6px; color:#666; font:11px system-ui,sans-serif; }
          details[open] { background:white; max-width:80vw; max-height:70vh; overflow:auto; }
          pre { white-space:pre-wrap; }
        </style>
        <canvas id="canvas" tabindex="0" aria-label="Interactive Xui TestApp inside the middleware phone frame"></canvas>
        <p id="status" role="status">TestApp · tap Canvas Drawing, then Back · browser catalogue preview</p>
        <details><summary>Inter license</summary><pre>{{WebUtility.HtmlEncode(File.ReadAllText(Path.Combine(Frontend.SourceProject.XuiRoot, "Core/Fonts/Inter/LICENSE.txt")))}}</pre></details>
        <script>
        {{Xui.Compiler.WASM.Runtime.JavaScript.Source}}
        {{BrowserCanvasHost.JavaScript}}
        {{host}}
        const bytes = Uint8Array.from(atob('{{Convert.ToBase64String(wasm)}}'), c => c.charCodeAt(0));
        Promise.all([400,500,600,700].map(weight => document.fonts.load(`${weight} 20px Inter`)))
            .then(() => startMobile(bytes)).catch(error => {
                window.mobileDemo = { error: String(error) };
                document.getElementById('status').textContent = `Unable to start: ${error.message ?? error}`;
            });
        </script>
        </html>
        """ + "\n";
}
