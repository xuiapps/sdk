# WASM playground

The playground is a static site published at **https://xuiapps.com/wasm/playground.html** by the existing [Pages workflow](../../../.github/workflows/Web.yml). It has a plain C# editor, Run/Stop controls, console output, compiler/runtime errors, the entry point's exit code, and a WASM download. Examples cover Hello World, a loop, and generic virtual dispatch. Ctrl/Command+Enter runs the editor; Escape stops the current job.

The page itself is HTML/CSS/JavaScript. A persistent module worker hosts Roslyn, XuiNet, and the Xui WASM emitter using the framework's .NET WebAssembly runtime. Roslyn emits a PE image; `AssemblyCompiler` reads it; `WasmEmitter` produces the program's WASM. **The edited program executes as Xui-generated WASM, not as managed code in the framework runtime.** The framework runtime only hosts the compiler. This follows the [.NET Web Worker hosting pattern](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-on-webworkers?view=aspnetcore-10.0).

Every execution uses a fresh JavaScript worker and WASM instance. The host imports collect console output, preserving .NET Boolean spelling and integer output without a BigInt suffix. A run stops after 15 seconds, and output is capped to keep the editor responsive. Stop terminates execution without unloading the compiler; stopping compilation recreates the compiler worker. No source or execution requests go to a server. Unsupported compiler features report their diagnostic in the output pane.

Roslyn receives embedded `System.Runtime` and `System.Console` reference assemblies from the build's .NET targeting pack. It does not try to read runtime assembly paths or WebCIL assets. Concurrent compilation is disabled because a single-threaded browser worker cannot block waiting for other managed threads. Trimming and native relinking are disabled for this initial host to preserve Roslyn's dependencies; the first load downloads the .NET runtime and compiler assemblies, while subsequent runs reuse them. This is a larger download than the tiny WASM modules it emits.

## Build and preview

Use the .NET 10 SDK. Native AOT and the `wasm-tools` workload are not required for this interpreter-hosted compiler.

```sh
dotnet publish Xui/Compiler/Playground/Xui.Compiler.Playground.csproj -c Release
python3 -m http.server 8088 --directory Xui/Compiler/Playground/bin/Release/net10.0/publish/wwwroot
```

Open `http://localhost:8088/` for the emulator or `http://localhost:8088/playground.html` for the editor. A browser with WASM GC, native exception handling (`try_table`), and the `js-string` builtins is required; the published app is smoke-tested in Chromium. Files must be served over HTTP(S), with JavaScript and `.wasm` MIME types. All asset and worker paths are relative, so the same bundle works at `/` or `/wasm/`. There is no shared-memory/thread requirement or server-side compilation service.

## Verify

```sh
dotnet test Xui/Compiler/Tests/Xui.Compiler.Tests.csproj -c Release
npm ci --prefix Xui/Compiler/Playground/browser-tests
cd Xui/Compiler/Playground/browser-tests
npx playwright install chromium --only-shell
npm test
```

The smoke test serves the published files at `/wasm/`, exercises the real .NET compiler and generated WASM in Chromium, and checks examples, errors, output escaping, downloads, stopping an infinite loop, compiler reuse, and a narrow viewport. It writes desktop/mobile screenshots to ignored `browser-tests/test-output/`. Pass a different published `wwwroot` as `npm test -- /absolute/path/to/wwwroot`. `PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH` can select an existing Chromium binary.

## Deployment

`Web.yml` publishes and browser-tests this project, copies its `wwwroot` into `_site/wasm`, then uploads the combined documentation and playground artifact to GitHub Pages. The existing `xuiapps.com` CNAME is preserved. Deployment occurs when the changes reach `main` (or when that workflow is manually dispatched with the intended ref). The compiler's source and dependencies are built from that same checkout.


## TestApp preview

The default [WASM page](https://xuiapps.com/wasm/) opens the self-contained TestApp
inside the emulator phone. The editor remains at `/wasm/playground.html`, and the
previous `/wasm/testapp.html` address redirects to `/wasm/`.
It runs the real mobile shell, event routing, scrolling, and animated navigation
with a measured FPS badge. The browser catalogue contains Text Metrics and 20
Canvas drawings; diagnostics and async/GPU demos remain excluded. The expandable Page size
panel accounts for HTML, embedded fonts, JavaScript and WASM sections.

Publishing copies `Tests/Wasm/TestApp/Snapshots/BrowserInteractive.html` as
`wwwroot/index.html`. Regenerate and review that snapshot with
`BrowserInteractionTests` as compiler support grows. It embeds its own WASM and JS
runtime and does not load the playground's Roslyn/.NET runtime. The published-site
smoke test checks the default emulator route, absence of framework downloads on that
page, legacy redirect, FPS warmup, Canvas Drawing navigation and Back.
