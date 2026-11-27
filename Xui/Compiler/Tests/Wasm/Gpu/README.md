# Production GPU rendering in WebGL 2

`ShaderGenerationTests` runs the actual TestApp source generator and extracts the
GLSL constants for `CubeShader` and `FloorplanShader` from the resulting PE. The
reviewed `.glsl` snapshots contain both stages. This gate isolates the shader
compiler. The separate hardware-cube gate below runs the real production view through XuiNet.

```sh
dotnet test Xui/Compiler/Tests/Xui.Compiler.Tests.csproj -c Release --filter FullyQualifiedName~ShaderGenerationTests
node Xui/Compiler/Tests/Wasm/Gpu/shaders.browser.mjs
```

The browser test uses the existing Playwright installation in
`Compiler/Playground/browser-tests`. It works offline and requires WebGL 2; lack
of a GPU context is a failure. It compiles and links both real shader programs,
then checks pixels for translated matrix geometry, clipping on both sides of the
Xui depth range, and Floorplan's explicit mip selection and fade multiplication.
It checks the matrix's reported row-major layout and stride. Browser screenshots
are diagnostics under the ignored browser `test-output/shaders` directory.

The shared `GlslCodeGenerator` emits GLSL ES 3.00. Define exactly one of
`XUI_VERTEX` and `XUI_FRAGMENT` immediately after `#version 300 es`; each stage's
entry point is `main`. Vertex attributes use their declared locations. Varying
names derive from locations so independently named fields can link. The
`XuiBindings` uniform block uses std140 and row-major matrix storage. A host must
pack other uniform fields according to std140; arbitrary CPU struct layouts are
not automatically interchangeable. This follows the [WebGL 2 uniform-block
requirements](https://registry.khronos.org/webgl/specs/2.0/#UNIFORM_BLOCK_LAYOUT).
Xui matrix constructors use rows and are transposed into GLSL constructor order.
Vertex clip depth is converted from `[0,w]` to `[-w,w]`.

Direct texture/sampler binding pairs become combined sampler uniforms such as
`x_texture_Atlas_Sampler`. Current sampling supports float4 textures with `Sample`
and `SampleLevel`. Scalar/vector arithmetic, supported math intrinsics, branches,
struct fields, and explicit float matrix-row constructors are emitted. Linear
(non-perspective) interpolation, floating remainder, direct matrix-row access,
user helper functions, and unsupported expressions fail explicitly. A shader
unsupported by GLSL still gets its existing HLSL/MSL sources; requesting its GLSL
source throws with the reason. Interpolation attributes now survive the Roslyn
IR mapping for all three backends.

## Typed uploads and production hardware cube

`IGpuCommandList` accepts borrowed `ReadOnlySpan<T>` vertices and `in T` constants
for unmanaged T. Existing native implementations inherit defaults that pin only
for their synchronous pointer call. `BrowserGpuCommandList` specializes these
methods to copy values from managed GC arrays into a bounded stackalloc frame,
then passes the address and byte count through the GPU import. The host reads the
exported WASM memory immediately; no pointer or typed-array view survives a call.
This supports nested scalar structs without pretending a GC array has a linear
address. Large uploads beyond the compiler's stack budget fail; streaming upload
buffers are future work.

`BrowserGpuHost.js` owns WebGL 2 devices, compiled shaders, linked programs, vertex
and uniform buffers, command lists and color/depth targets. It checks resource
ownership and upload bounds. `GpuContext.cs` retains targets by composition key,
replaces them on size changes and releases omitted targets. GPU output participates
in DOM composition, clipping and transforms. The hardware cube uses a matrix-only
uniform block; arbitrary constant structs are not automatically repacked to std140.
Texture/sampler binding and GPU readback remain explicitly unsupported.

```sh
dotnet test Xui/Compiler/Tests/Xui.Compiler.Tests.csproj -c Release --filter 'FullyQualifiedName~UploadTests|FullyQualifiedName~RealCatalogueTests&DisplayName~HardwareCubeExample'
node Xui/Compiler/Tests/Wasm/Gpu/host.browser.mjs
node Xui/Compiler/Tests/Wasm/TestApp/real-catalogue.browser.mjs Xui/Compiler/Tests/bin/Release/net10.0/TestApp/HardwareCubeExample.html 1
```

`UploadTests` executes generic interface calls in Node and verifies nested vertex
bytes, constants and empty buffers over repeated frames. Core `GpuUploadTests`
checks the default native overload with packed structs. `host.browser.mjs` checks
pixels, bounds errors, disposed resources and stable resource counts. The actual
production view gate checks visible cube faces, routed dragging, target resizing,
removal and disposal. Node's explicit recording mode checks command flow only;
Chromium supplies the rendering acceptance gate.

This does not complete the ThreeD menu entry: it also constructs an optional
software renderer with pinned arrays/native framebuffer storage. Floorplan still
needs async asset loading and GPU texture services. Neither full page is marked ready.
