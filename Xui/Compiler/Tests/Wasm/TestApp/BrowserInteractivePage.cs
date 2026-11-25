namespace Xui.Compiler.Tests.Wasm.TestApp;

internal static class BrowserInteractivePage
{
    public static string Html(byte[] wasm, string host) => PageSizeBreakdown.AddTo($$"""
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
          #status { max-width:calc(100% - 230px); }
          #page-size { position:fixed; right:12px; bottom:6px; color:#444; font:12px system-ui,sans-serif; }
          #page-size summary { cursor:pointer; text-align:right; }
          #page-size[open] { box-sizing:border-box; background:white; width:680px; max-width:calc(100vw - 24px); max-height:85vh; overflow:auto; padding:16px; border:1px solid #ddd; border-radius:8px; box-shadow:0 4px 24px #0002; }
          #page-size h2 { font-size:15px; margin:18px 0 8px; }
          #page-size p { line-height:1.5; }
          #page-size table { width:100%; border-collapse:collapse; font-variant-numeric:tabular-nums; }
          #page-size th, #page-size td { padding:6px 5px; text-align:right; border-bottom:1px solid #eee; }
          #page-size th:first-child, #page-size td:first-child, #wasm-size th:last-child, #wasm-size td:last-child { text-align:left; }
          #page-size tfoot { font-weight:600; }
        </style>
        <canvas id="canvas" tabindex="0" aria-label="Interactive Xui TestApp inside the middleware phone frame"></canvas>
        <p id="status" role="status">TestApp · drag to browse</p>
        {{PageSizeBreakdown.Placeholder}}
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
        """ + "\n", wasm);
}
