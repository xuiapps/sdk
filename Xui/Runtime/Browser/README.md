# .NET browser runtime

The browser host runs shared Xui app code on .NET 10 WebAssembly through the
Blazor WebAssembly SDK. TestApp includes the mobile emulator middleware and its
full catalogue. The published demo is at https://xuiapps.com/wasm/; the independent
Xui compiler experiment remains at https://xuiapps.com/wasm/compiler/.

## Run and publish

```sh
dotnet workload install wasm-tools
dotnet run --project Xui/Apps/TestApp/TestApp.Browser.csproj -c Release
# Compile managed methods to WebAssembly and strip their compiled IL:
dotnet publish Xui/Apps/TestApp/TestApp.Browser.csproj -c Release \
  -p:RunAOTCompilation=true -p:WasmStripILAfterAOT=true -o artifacts/testapp-wasm
python3 -m http.server 5083 --directory artifacts/testapp-wasm/wwwroot
```

Normal development uses the .NET interpreter/Jiterpreter. AOT is a publish-time
option, still backed by .NET runtime services such as GC, metadata and interop.
It is different from Xui's custom MSIL compiler and from desktop NativeAOT.
The header identifies the build mode. `BlankApp.Browser` and `LoadTestApp.Browser`
use the same host assets and build imports.

The host uses relative URLs and works under `/wasm/` without a server backend.
Serve `.wasm` as `application/wasm` and JavaScript as `text/javascript`. The
trailing slash matters when hosting below a directory. `tools/build-web.ps1`
builds DocFX and both WASM apps and assembles the deployable `_site` directory.

## Rendering and interop

Canvas operations are recorded into a reusable numeric command buffer and replayed
in JavaScript once per frame. Strings are deduplicated per frame; font and text
measurements are cached. Span-backed upload views are consumed synchronously and
never retained beyond the interop call. Input crosses as primitive values and
enters Xui's normal window dispatch and mobile emulator routing.

GPU surfaces use WebGL 2 with generated GLSL shaders, retained framebuffer targets,
vertex/uniform buffers and texture uploads. Presentation blits to an offscreen
canvas and composites in Canvas draw order without CPU readback. Explicit GPU
readback remains available. The host tries to create a WebGL context and accepts
success; it does not inspect GPU vendor strings or reject browser-selected drivers.
When creation fails, `ICompositionContext.GpuSurface(..., out failure)` reports
`GpuSurfaceFailure.NotSupported` and the cube displays “Not supported on this device”.
Portable SVG/clip fallbacks are distinguished by `PortableFallbackRequired`.

Current limits: a single top-level browser window; no sampled render-target
texture or clamp-to-border sampler support. WebGL context loss reports an error
and requires a reload. Browser composition uses immediate Canvas replay rather
than native DOM layers for every retained group.

## Verification and measurement

```sh
npm ci --prefix Xui/Runtime/Browser/browser-tests
cd Xui/Runtime/Browser/browser-tests
npx playwright install chromium --only-shell
npm test -- /absolute/path/to/published/wwwroot
npm run perf -- http://127.0.0.1:5083/
```

The smoke test mounts published output at `/wasm/` and checks startup, canvas
navigation, text input, WebGL rendering/resource disposal and unavailable-WebGL
behavior. `XUI_EXPECT_AOT=1` additionally requires the AOT build label. Screenshots
are written to ignored `browser-tests/artifacts/`. Set
`PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH` to use an installed Chrome executable.

The performance script enables continuous rendering, discards five reporting
windows, then samples five windows without canvas instrumentation. The header
reports browser rAF frequency, actual rendered FPS and CPU time. Results depend
on device, browser, display refresh and page; compare identical workloads after
warmup, and distinguish CPU frame cost from display-limited FPS.
