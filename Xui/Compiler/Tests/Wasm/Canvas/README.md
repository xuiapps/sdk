# Standalone Xui canvas demos

Open any of these files directly in a browser with WASM GC, native exception
handling, and JS string builtins (tested in Chromium). No HTTP server, network
access, .NET runtime, or separate assets are needed:

- [Shapes](Snapshots/Shapes.html): two rectangles and a label, including `Rect.Right` executed from `Xui.Core.dll`.
- [Grid](Snapshots/Grid.html): nested C# loops calculate 18 rectangle positions using NFloat arithmetic.
- [Values](Snapshots/Values.html): real Point/Vector/Rect tuple conversions feed Core’s `Rect.Inset`; interface returns and Rect/Color parameters preserve value copies.
- [Events](Snapshots/Events.html): a stack event holds actual Xui Rect/Color values, a string, and an interface reference; interface dispatch draws the scene.

Each HTML file embeds the generated WASM as base64, the JavaScript Canvas 2D host,
and its C# source. The application runs as **Xui-generated WASM**. Roslyn is used
only when the tests generate the files. Core is supplied as a separate implementation
assembly, not source-linked into the fixture. These are geometry/host-adapter demos;
they do not yet instantiate Xui's full Window/View lifecycle or implement all of IContext.

The companion [Shapes](Snapshots/Shapes.md), [Grid](Snapshots/Grid.md), and
[Events](Snapshots/Events.md), and [Values](Snapshots/Values.md) Markdown snapshots show source, exact recorded drawing
commands, main's exit code, retained methods, and WAT. The C# adapter extracts scalar
components from Rect and Color; only three exact NativeCanvas method signatures are
replaced with imports. Geometry and event dispatch execute inside WASM, while JavaScript
performs the final Canvas API calls. Unsupported operations are not silently ignored.

Run from the repository root:

```sh
dotnet test Xui/Compiler/Tests/Xui.Compiler.Tests.csproj -c Release --filter FullyQualifiedName~CanvasTests
npm ci --prefix Xui/Compiler/Playground/browser-tests
cd Xui/Compiler/Playground/browser-tests
npx playwright install chromium --only-shell
cd ../../../..
node Xui/Compiler/Tests/Wasm/Canvas/browser.mjs
```

The browser runner reuses the playground's pinned Playwright dependency. It opens
`file://` URLs with networking disabled, compares the recorded commands, checks
rectangle/text pixels, reruns each module, and checks a narrow viewport. Screenshots
are written to the ignored `Playground/browser-tests/test-output/canvas` directory.
`PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH` can select an installed Chromium executable.

Tests compile each scene in Debug and Release. Additional cases check three-assembly
nested layouts with overlapping field row numbers, NFloat rounding at WASM32 boundaries,
cross-assembly inherited fields and virtual/interface struct results, IEEE comparisons/division,
and explicit rejection of unsupported NFloat operations. Numeric semantics are based
on the WASM32 target (f32 NFloat), not the host machine's 64-bit NFloat.

To regenerate the checked-in HTML and Markdown after an intentional change:

```sh
XUI_UPDATE_SNAPSHOTS=1 dotnet test Xui/Compiler/Tests/Xui.Compiler.Tests.csproj -c Release --filter FullyQualifiedName~CanvasTests
```

Review command/WAT diffs and browser output before accepting updated artifacts.
