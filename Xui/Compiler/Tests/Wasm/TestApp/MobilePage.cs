namespace Xui.Compiler.Tests.Wasm.TestApp;

internal static class MobilePage
{
    public static string Html(byte[] wasm, string host) => $$"""
        <!doctype html>
        <html lang="en">
        <meta charset="utf-8">
        <meta name="viewport" content="width=device-width,initial-scale=1">
        <title>Xui compiler · mobile preview</title>
        <style>
          html, body { margin:0; width:100%; height:100%; overflow:hidden; background:white; }
          canvas { display:block; width:100%; height:100%; touch-action:none; outline:none; }
          #status { position:fixed; left:12px; bottom:6px; margin:0; color:#666; font:11px system-ui,sans-serif; pointer-events:none; }
        </style>
        <canvas id="canvas" tabindex="0" aria-label="Xui TestApp view inside the middleware phone frame"></canvas>
        <p id="status" role="status">Compiler preview: FillRect view · drag to test input · full mobile navigation pending</p>
        <script>
        {{Xui.Compiler.WASM.Runtime.JavaScript.Source}}
        {{ViewCanvasHost.JavaScript}}
        {{host}}
        const bytes = Uint8Array.from(atob('{{Convert.ToBase64String(wasm)}}'), c => c.charCodeAt(0));
        startMobile(bytes).catch(error => {
            window.mobileDemo = { error: String(error) };
            document.getElementById('status').textContent = `Unable to start: ${error.message ?? error}`;
        });
        </script>
        </html>
        """ + "\n";
}
