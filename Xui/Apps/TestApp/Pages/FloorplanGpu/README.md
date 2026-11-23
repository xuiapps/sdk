# Floorplan GPU

A GPU floorplan performance example. `Office.svg` remains the source for furniture,
walls, windows, columns, stairs, and doors. Labels remain
`IContext.FillText` operations above the GPU surface. The vector example and this
page share `FloorplanCameraView` for gestures, inertia, zoom thresholds, and
navigation-aware invalidation.

## Generate the assets

From the repository root on macOS with .NET 10:

```sh
tools/FloorplanCompiler/generate.sh
```

The compiler uses the existing SVG importer and CoreGraphics to rasterize each
furniture symbol. It expands composite workstations into chair, desk, monitor,
and keyboard placements before creating quads. Simplified L0/L1 workstation
rectangles become independent desk/chair sprites too. Bitmap rows and cropped
local bounds use the SVG Y-down convention before placement transforms are applied. It resolves all four detail states
offline and preserves their painter order within structure and furniture batches. Runtime loading performs no XML or
path parsing; it decompresses validated bytes on the background preload task.
The loading placeholder stays visible through an active navigation transition
and until GPU preparation completes. A worker uploads every mip and layer, creates
the pipeline, draws the whole atlas at each mip, and exercises all four layers
offscreen. Completed GPU work and resource ownership transfer to the UI thread;
abandoned preparations dispose their resources when the worker finishes. Metal
entry points establish autorelease pools for worker-thread preparation.

Generated files in `Assets/FloorplanGpu`:

- `atlas.json`: source SHA-256, atlas dimensions, sprite pixel rectangles and local
  bounds, quad counts, and text/font/transform metadata for each detail level.
- `atlas.bin.gz`: versioned, losslessly compressed premultiplied RGBA8 mip chain.
  Five levels use linear downsampling of premultiplied colors. Sixteen-pixel
  transparent gutters and 16-pixel-aligned allocations isolate sprites through
  mip level 4. Sampling clamps to that final level.
- `atlas-mip0.png` through `atlas-mip4.png`: straight-alpha review images.
- `quads-l0.bin` through `quads-l3.bin`: one versioned little-endian blob per detail layer.
  Each quad stores four world-space XY corners, UV bounds, and a mip-density bias.
  Loading expands each quad into two triangles once, off the UI thread.
- `structure-l0.bin` through `structure-l3.bin`: versioned `XFPG` quad meshes. Each
  record stores four XY/edge-normal/coverage vertices and one atlas color coordinate.
  Structure and furniture are combined into a single immutable vertex buffer per layer.

The current atlas is 2048 × 2048 (about 21.3 MiB across its mip chain), with 104
furniture sprite/detail variants and 24 solid-color cells. Furniture uses 683, 683,
877, and 877 quads; structure uses 2,937, 11,442, 15,482, and 18,162 quads including
edge coverage. Generation is deterministic
for the same source and CoreGraphics environment. Regenerate and inspect previews
when the authored floorplan changes; the source hash test catches stale exports.
The SVG authoring/export site does not need a separate geometry source.

## Rendering

`FloorplanShader` is C# compiled by `Xui.CompileTime` into embedded MSL/HLSL source.
Native shader compilation happens once when creating device resources, as in the
existing cube example. Textures and immutable vertex buffers upload once per
active page/device, while the loading placeholder is visible. A frame updates two float4 uniforms (camera and fade/mip
selection), then issues one draw per visible layer. GPU clipping rejects offscreen
quads. There are no per-chair Xui or native composition views.

The incoming layer is drawn beneath the old layer. It fades in over 300 ms; the
old top layer then fades out over another 300 ms. Fast zoom changes queue only the
latest requested level while the current handoff completes. Animations stop when
settled and pause during page navigation.

Native texture resources are currently implemented by Metal on iOS and macOS.
iOS presents directly into its retained GPU composition surface. macOS uses the
existing offscreen GPU/readback path. Other backends use the same C# shader in the
software renderer until they implement `IGpuResourceDevice`; this fallback is for
correctness and tests, not hardware-performance parity. Windows and Android
explicitly reject the new pipeline features if requested through their older
command-list APIs.

Software support includes bilinear/trilinear sampling, premultiplied source-over,
a top-left triangle fill rule (avoids doubled alpha on quad diagonals), and native
GPU NDC orientation. Tests exercise sampling, layer handoffs, model validation,
shared-shader software rendering, and real Metal/software image comparison.

## Wall and door geometry

The compiler selects authored structure and underlay groups, including door symbols,
while leaving cabinets/lockers in the furniture atlas. CoreGraphics expands strokes
with the authored width, cap, and join. Xui's scanline tessellator converts the filled
outlines to trapezoid quads, preserving nonzero/even-odd holes. Curves are flattened
offline to a 0.025 world-unit tolerance; no path parsing or tessellation runs in-app.

Boundary quads provide a 0.75-physical-pixel antialias fringe. The shader applies
normal offsets in screen space and interpolates premultiplied coverage, so the fringe
width stays fixed while zooming. Constant color cells share the existing mipmapped
atlas; wall shapes themselves are geometry, with no SDF or wall bitmap. They use the
same shader, warmup, and 300 ms + 300 ms layer handoff as furniture, still with one
draw per settled layer. Door underlays precede wall outlines, with furniture above.
